const { Kafka } = require("kafkajs");

exports.kafka = new Kafka({
  clientId: "invoice-wiz-persister",
  brokers: ["broker:29092"],
});
