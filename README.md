# InvoiceWiz

A small demo microservices app that generates PDF invoices. Designed as a portfolio project to demonstrate event-driven architecture with OpenTelemetry tracing, SignalR realtime updates, and a small React frontend.

## Architecture & Components

- APIGateway (C#): HTTP entry point (`/api/invoices`), SignalR hub (`/invoiceHub`), publishes `InvoiceRequestedEvent` and receives `InvoiceGeneratedEvent`.
- Generator (C#): consumes invoice requests, generates PDF, stores to Azure Blob (Azurite in dev), publishes generated event.
- Persister (C#): stores invoice metadata in MongoDB and updates records with storage location when generation completes.
- Search (C#): exposes read-only access to invoices (used by APIGateway via gRPC).
- Web (React + Vite): UI that posts invoice requests, listens for SignalR notifications and shows download link.
- Infrastructure: RabbitMQ (MassTransit), MongoDB, Azurite (blob storage), Jaeger for tracing.

Contracts are in `Contracts/` (see `Contracts/Events.cs` and `Contracts/Invoice.cs`).

## Features

- Submit invoice requests from the web UI
- Asynchronous PDF generation with event-driven flow (RabbitMQ + MassTransit)
- Real-time notification via SignalR when PDF is ready; toast + download link
- Persistent invoice metadata in MongoDB; historical list in the UI

## Running

### Docker Compose

Quick start (builds local images and runs all services):

```bash
docker compose up --build
```

Services started by compose:
- `apigateway` on `localhost:8080`
- `web` on `localhost:3000`
- `mongodb`, `broker` (RabbitMQ), `generator`, `persister`, `search`, `azurite`, `jaeger`

Useful commands
```bash
# view logs
docker compose logs -f apigateway
docker compose logs -f web

# rebuild just the web image
docker compose build --no-cache web
```

Notes
- The APIGateway exposes endpoints under `/api/` (see `APIGateway/APIGateway.http` for examples).
- SignalR hub is mounted at `/invoiceHub` and the frontend listens for generated invoice events.

### Running the Web frontend (development)

For iterative frontend development with HMR:

```bash
cd web
npm install
npm run dev
```

By default the frontend expects the APIGateway to be reachable at `/api`. In Docker/K8s the nginx proxy is configured to forward `/api` and `/invoiceHub` to the APIGateway service.

### Kubernetes

Manifests live under `k8s/`. To run on a local cluster (minikube):

```bash
# build images and load into minikube if needed
docker build -t apigateway:0.0.2 -f APIGateway/Dockerfile .
docker build -t web:0.0.1 -f web/Dockerfile ./web

kubectl apply -f k8s/

# get external URL (minikube)
minikube service apigateway --url
```

Notes on runtime config
- The `web` manifest sets `VITE_API_URL` to `http://apigateway:8080/api` for convenience. Because the frontend is a static build, environment changes at runtime require a small runtime injection step (or build-time substitution) if you need to change the API host after building the image.
