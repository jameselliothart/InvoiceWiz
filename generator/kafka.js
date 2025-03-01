const { Kafka } = require("kafkajs");

exports.group = 'group-invoice-generator';
exports.client = new Kafka({
  clientId: "invoice-wiz-generator",
  brokers: ["broker:29092"],
});
