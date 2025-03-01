# InvoiceWiz

An app for generating pdf invoices.

## Running

### Docker

In the project root execute:

```sh
docker-compose up --build
```

This will build and start the required services.

- mongodb: persistence for invoice details
- kafka: facilitates microservice communication
- kafka-init: initializes the required kafka topics
- localstack: for mocking AWS S3
- middle tier: facilitates communication to/from the web ui
- persister: saves invoice details to mongo
- generator: creates the invoice file and saves to S3 (localstack)
- web: the user-facing webpage

Find more details on each service below.

The InvoiceWiz site will be available on http://localhost (**not** http*s*).

### Kubernetes

Coming soon!

## Features

### Web UI

- Displays a form to gather invoice details
- Once details are entered, click Generate to start invoice generation
- When the invoice is ready a toast message notifies the user
  - Clicking the toast message will start the download
  - The Download button is also enabled for after the toast disappears
- TODO: Validation

### Middle Tier

- Requesting invoices
  - Receives POST requests of the invoice details
  - Publishes these details to kafka for consumption by downstream services
- Generated invoices
  - Subscribes to invoice generated messages
  - Notifies the web ui of generated invoices via SignalR connection
- Note the MT is not publicly exposed as nginx serves as a reverse proxy to forward web requests within the backend network

### Persister

- Subscribes to Invoice Requested messages -> Saves the requested invoices details to mongodb
- Subscribes to Invoice Generated messages -> Updates the invoice detail record with its saved location

### Generator

- Subscribes to invoice requested messages
- Generates invoices with the requested details
- Saves the invoices to AWS S3 (localstack mock)
- Publishes an invoice generated message

## Design Decisions

This project relies on a number of microservices to accomplish several disparate tasks.

## Road Map

- Ability to view old invoices
- Ability to generate an invoice pdf via templated [html](https://github.com/janl/mustache.js)
- Convert to using k8