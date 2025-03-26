using Contracts;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Search.DataAccess;
using Search.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
// Register MongoDB
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connStr = config.GetConnectionString("MongoDb");
    var client = new MongoClient(connStr);
    return client.GetDatabase("InvoiceDb").GetCollection<Invoice>("invoices");
});
builder.Services.AddSingleton<IInvoiceRepository, MongoInvoiceRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<InvoiceService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

app.Run();
