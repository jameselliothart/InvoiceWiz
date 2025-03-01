using Confluent.Kafka;

namespace MiddleTier;

class Publisher(string topic, IProducer<string, InvoiceRequested> producer)
{
    public async Task Publish(InvoiceRequested invoiceDetails)
    {
        var msg = new Message<string, InvoiceRequested> { Key = invoiceDetails.Id.ToString(), Value = invoiceDetails };
        try
        {
            var deliveryResult = await producer.ProduceAsync(topic, msg);
            Console.WriteLine($"{topic}: published {deliveryResult.Key} to partition {deliveryResult.TopicPartitionOffset}");
        }
        catch (ProduceException<string, InvoiceRequested> ex)
        {
            Console.WriteLine($"Failed to deliver message: {ex.Error.Reason}");
        }
    }
}