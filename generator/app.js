const {
  S3Client,
  CreateBucketCommand,
  GetObjectCommand,
  HeadBucketCommand,
  waitUntilBucketExists,
  ListObjectsV2Command,
  PutObjectCommand
} = require('@aws-sdk/client-s3');
const { fromEnv } = require('@aws-sdk/credential-providers');
const { getSignedUrl } = require('@aws-sdk/s3-request-presigner');
const fs = require('fs');
const path = require('path');
const kafka = require('./kafka');

async function generateSignedUrl(s3Client, bucket, key) {
  try {
    const getObjectCommand = new GetObjectCommand({ Bucket: bucket, Key: key });
    const objectUrl = await getSignedUrl(
      s3Client, getObjectCommand, { expiresIn: 3600, responseContentDisposition: 'inline' }
    );
    const exposedUrl = objectUrl.replace('localstack', 'localhost');
    return exposedUrl;
  } catch (error) {
    console.error(`Error generating signed URL for ${key}:`, error);
    return null;
  }
}

async function uploadFile(s3Client, filePath, bucket) {
  const fileStream = fs.createReadStream(filePath);
  fileStream.on('error', err => console.error('File error', err));
  const fileName = path.basename(filePath);

  const uploadParams = {
    Bucket: bucket,
    Key: fileName,
    Body: fileStream,
  };

  const putObjectCommand = new PutObjectCommand(uploadParams);
  await s3Client.send(putObjectCommand);
  console.log(`Uploaded ${fileName} to ${bucket}`);
}

async function createBucketIdempotent(s3Client, bucket) {
  try {
    const headBucketCommand = new HeadBucketCommand({ Bucket: bucket });
    await s3Client.send(headBucketCommand);
    console.log(`Bucket ${bucket} already exists`);
  } catch (err) {
    if (err.name === 'NotFound') {
      const createBucketCommand = new CreateBucketCommand({ Bucket: bucket });
      await s3Client.send(createBucketCommand);
      console.log(`Bucket ${bucket} successfully created`);
    } else {
      throw err;
    }
  }
}

async function main() {
  // s3 setup
  process.env.AWS_ACCESS_KEY_ID = 'test';
  process.env.AWS_SECRET_ACCESS_KEY = 'test';

  const s3Client = new S3Client({
    region: 'us-east-1',
    endpoint: 'http://localstack:4566',
    forcePathStyle: true,
    credentials: fromEnv(),
  });
  const bucket = 'invoice-files'

  await createBucketIdempotent(s3Client, bucket);
  await waitUntilBucketExists({ client: s3Client, maxWaitTime: 20 }, { Bucket: bucket });
  console.log(`Bucket ${bucket} is ready`);

  // kafka setup
  const consumer = kafka.client.consumer({ groupId: kafka.group });
  const producer = kafka.client.producer();
  await producer.connect();
  await consumer.connect();
  await consumer.subscribe({ topics: ["invoices"], fromBeginning: true });

  await consumer.run({
    eachMessage: async ({ topic, partition, message, heartbeat, pause }) => {
      const messageValue = message.value.toString();
      console.log(`${kafka.group}: [${topic}]: PART:${partition}: ${messageValue}`);
      const invoice = JSON.parse(messageValue);

      // TODO file created inside container needs to get deleted after upload
      const filePath = `${invoice.To}.txt`;
      fs.writeFile(filePath, `amount: ${invoice.Amount}`, (err) => {
        if (err) console.error('Error writing to file', err);
        console.log('The file has been saved!');
      });
      await uploadFile(s3Client, filePath, bucket);

      const key = path.basename(filePath);
      const signedUrl = await generateSignedUrl(s3Client, bucket, key);
      console.log('Signed URL for the uploaded file:', signedUrl);
      await producer.send({
        topic: 'invoices-generated',
        messages: [
          {
            key: invoice.Id,
            value: signedUrl,
          }
        ]
      });
    },
  });
}

main();
