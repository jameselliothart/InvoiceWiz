const { kafka } = require("./client");
const { Invoice, messageToUpdateParams } = require("./db");
const group = 'group-invoice-persister';

async function init() {
  const consumer = kafka.consumer({ groupId: group });
  await consumer.connect();

  await consumer.subscribe({ topics: ["invoices", "invoices-generated"], fromBeginning: true });

  await consumer.run({
    eachMessage: async ({ topic, partition, message, heartbeat, pause }) => {
      const messageKey = message.key.toString();
      const messageValue = message.value.toString();
      console.log(`${group}: [${topic}]: PART:${partition}: ${messageKey}-${messageValue}`);
      const topicLogic = {
        'invoices': (msgVal) => messageToUpdateParams(JSON.parse(msgVal)),
        'invoices-generated': (msgVal) => { return {FileLocation: msgVal} },
      };
      const updateParams = topicLogic[topic](messageValue);
      const doc = await Invoice.findOneAndUpdate({ '_id': messageKey }, updateParams, { upsert: true, new: true });
      console.log(`modified ${Invoice.db.name}.${Invoice.collection.name}: ${JSON.stringify(doc.toJSON())}`);
    },
  });
}

init();
