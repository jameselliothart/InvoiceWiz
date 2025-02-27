using Microsoft.AspNetCore.Mvc;

var AllowDevAccessPolicy = "allowDevAccessPolicy";
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy(AllowDevAccessPolicy, policy => policy.AllowAnyOrigin().AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var invoiceRoute = "/api/invoice";

app.MapPost(invoiceRoute, ([FromBody] InvoiceDetails invoiceDetails) =>
{
    Console.WriteLine(invoiceDetails);
});

app.UseCors(AllowDevAccessPolicy);
app.Run();

record InvoiceDetails(string To, decimal Amount);
