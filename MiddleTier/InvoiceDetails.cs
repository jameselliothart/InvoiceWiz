using System.Text;
using System.Text.Json;
using Confluent.Kafka;

namespace MiddleTier;

public record InvoiceRequested(Guid Id, string To, decimal Amount);

public class InvoiceRequestedSerializer : ISerializer<InvoiceRequested>
{
    public byte[]? Serialize(InvoiceRequested data, SerializationContext context)
    {
        if (data == null) return null;
        var jsonString = JsonSerializer.Serialize(data); // Convert to JSON string
        return Encoding.UTF8.GetBytes(jsonString); // Convert JSON string to byte[]
    }
}

public class GuidSerializer : ISerializer<Guid>
{
    public byte[] Serialize(Guid data, SerializationContext context)
    {
        return data.ToByteArray(); // Converts Guid to 16-byte array
    }
}