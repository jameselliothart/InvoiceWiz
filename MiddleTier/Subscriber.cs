using Confluent.Kafka;
using Microsoft.AspNetCore.SignalR;

namespace MiddleTier;

record InvoiceGenerated(string Id, string Location);

class Subscriber(string topic, IConsumer<string, string> consumer, IHubContext<InvoiceHub> hubContext) : IHostedService
{
    private readonly CancellationTokenSource _cts = new();
    private Task? _consumingTask;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        consumer.Subscribe(topic);
        _consumingTask = Task.Run(() => ConsumeLoop(_cts.Token), cancellationToken);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel();
        if (_consumingTask != null)
        {
            await Task.WhenAny(_consumingTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }
        consumer.Close();
        _cts.Dispose();
    }

    private async Task ConsumeLoop(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var cr = consumer.Consume(cancellationToken);
                    Console.WriteLine($"group-invoice-mt: [{cr.Topic}]: PART:{cr.Partition}: {cr.Message.Key}-{cr.Message.Value}");
                    var invoiceGenerated = new InvoiceGenerated(cr.Message.Key, cr.Message.Value);
                    await hubContext.Clients.All.SendAsync(
                        "ReceiveInvoice", invoiceGenerated.Id, invoiceGenerated.Location,
                        cancellationToken: cancellationToken
                    );
                }
                catch (ConsumeException ex)
                {
                    Console.WriteLine($"group-invoice-mt: Error consuming Kafka message: {ex.Error.Reason}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when stopping
        }
    }
}