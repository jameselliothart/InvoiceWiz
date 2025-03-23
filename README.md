# InvoiceWiz

An app for generating pdf invoices.

## Running

### Docker

In the project root execute:

```sh
docker compose up --build
```

This will build and start the required services.

- MongoDb: persistence for invoice details
- RabbitMQ: facilitates microservice communication via MassTransit
- Azurite: for mocking Azure Blob Storage
- APIGateway: facilitates communication to/from the web ui
- Persister: saves invoice details to mongo
- Generator: creates the invoice file and saves to Azure Blob Storage (Azurite)
- Web: the user-facing webpage *coming soon*

Find more details on each service below.

The APIGateway will be available on http://localhost:8080.
Route requests are configured in APIGateway.http.

### Kubernetes

Coming soon!

## Features

### Web UI - TODO

- Displays a form to gather invoice details
- Once details are entered, click Generate to start invoice generation
- When the invoice is ready a toast message notifies the user
  - Clicking the toast message will start the download
  - The Download button is also enabled for after the toast disappears
- Uses uuid.v6 for ordered uuid for more efficient indexing in mongodb
- TODO: Validation

### APIGateway

- Requesting invoices
  - Receives POST requests of the invoice details
  - Publishes these details to the message broker for consumption by downstream services
- Generated invoices
  - Subscribes to invoice generated messages
  - Notifies the web ui of generated invoices via SignalR connection
- Historical invoices
  - Retrieves all past invoices for grid display
  - Retrieve by id for individual details

### Persister

- Subscribes to Invoice Requested messages -> Saves the requested invoices details to mongodb
- Subscribes to Invoice Generated messages -> Updates the invoice detail record with its saved location

### Generator

- Subscribes to invoice requested messages
- Generates invoices with the requested details
- Saves the invoices to Azure Blob Storage (Azurite mock)
- Publishes an invoice generated message

## Design Decisions

This project relies on a number of microservices to accomplish several disparate tasks.