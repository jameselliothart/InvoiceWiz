const { kafka } = require("./client");
const { Invoice, messageToUpdateParams } = require("./db");
const group = 'group-invoice-persister';

async function init() {
  const consumer = kafka.consumer({ groupId: group });
  await consumer.connect();

  await consumer.subscribe({ topics: ["invoices"], fromBeginning: true });

  await consumer.run({
    eachMessage: async ({ topic, partition, message, heartbeat, pause }) => {
      const messageValue = message.value.toString();
      console.log(`${group}: [${topic}]: PART:${partition}: ${messageValue}`);
      const invoice = JSON.parse(messageValue);
      const updateParams = messageToUpdateParams(invoice);
      const doc = await Invoice.findOneAndUpdate({ '_id': invoice.Id }, updateParams, { upsert: true, new: true });
      console.log(`modified ${Invoice.db.name}.${Invoice.collection.name}: ${JSON.stringify(doc.toJSON())}`);
    },
  });
}

init();
