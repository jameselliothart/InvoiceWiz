# AI Assistant Guidelines for InvoiceWiz

This document provides essential context for AI agents working with the InvoiceWiz codebase.

## Architecture Overview

InvoiceWiz is a microservices application for generating PDF invoices with the following components:

- **APIGateway** (C#): Entry point for web UI communication, handles HTTP requests and SignalR notifications
- **Generator** (C#): Creates PDF invoices and saves to Azure Blob Storage
- **Persister** (C#): Manages invoice data persistence in MongoDB
- **Search** (C#): Retrieves invoice details from MongoDB
- **Web** (React/Redux): User interface for invoice creation and management

### Key Communication Patterns

1. Event-Driven Messaging (MassTransit/RabbitMQ):

   - `InvoiceRequestedEvent`: Triggered when user requests invoice generation
   - `InvoiceGeneratedEvent`: Published when PDF is created and stored
     Example in `Contracts/Events.cs`

2. Real-time Updates:
   - SignalR hub (`APIGateway/Invoices/InvoiceHub.cs`) notifies web UI when invoices are generated

## Development Workflow

### Local Development

1. **Full Stack Development**:

   ```bash
   docker compose up --build
   ```

   Access APIGateway at http://localhost:8080

2. **Web UI Development**:
   ```bash
   cd web
   npm install
   npm run dev
   ```

### Kubernetes Deployment

1. Apply manifests in `k8s/` directory
2. Get service URL: `minikube service apigateway --url -n invoicewiz`

### Testing API Endpoints

Use `APIGateway/APIGateway.http` for example requests:

- GET `/api/invoices/` - List all invoices
- POST `/api/invoices` - Create new invoice
- GET `/api/invoices/{id}` - Get specific invoice

## Project Conventions

1. **MongoDB Storage**:

   - Uses UUIDv6 for ordered identifiers (better indexing performance)
   - Invoice data models defined in `Contracts/Invoice.cs`

2. **Infrastructure Mocking**:

   - Azurite for Azure Blob Storage simulation
   - Local MongoDB for development

3. **Observability**:
   - Jaeger tracing enabled (see `k8s/jaeger.yaml`)
   - Access traces at port 16686

## Integration Points

1. **Message Broker (RabbitMQ)**:

   - Used for inter-service communication
   - Configured in each service's `appsettings.json`

2. **Blob Storage**:

   - PDF invoices stored in Azure Blob Storage (Azurite locally)
   - URI returned in `InvoiceGeneratedEvent`

3. **MongoDB**:
   - Primary data store for invoice metadata
   - Connection details in service configurations
