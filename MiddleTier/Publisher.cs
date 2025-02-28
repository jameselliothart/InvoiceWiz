using Confluent.Kafka;

namespace MiddleTier;

class Publisher(string topic, IProducer<Guid, InvoiceRequested> producer)
{
    public async Task Publish(InvoiceRequested invoiceDetails)
    {
        var msg = new Message<Guid, InvoiceRequested> { Key = invoiceDetails.Id, Value = invoiceDetails };
        try
        {
            var deliveryResult = await producer.ProduceAsync(topic, msg);
            Console.WriteLine($"{topic}: published {deliveryResult.Key} to partition {deliveryResult.TopicPartitionOffset}");
        }
        catch (ProduceException<Guid, InvoiceRequested> ex)
        {
            Console.WriteLine($"Failed to deliver message: {ex.Error.Reason}");
        }
    }
}