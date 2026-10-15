# LearningShop

A demo e-commerce microservices application built with **.NET 10** and **Angular 22**. The project exposes a product catalog and order management API through a YARP-based reverse proxy, and provides a single-page Angular frontend.

## Architecture

```
┌─────────────┐     ┌──────────────┐     ┌────────────────┐
│  Angular UI │────▶│  API Gateway │────▶│  ProductService │
│  (port 80)  │     │  (port 5100) │     │  (port 5101)   │
└─────────────┘     └──────────────┘     └────────────────┘
                              │
                              ▼
                    ┌────────────────┐
                    │  OrderService  │
                    │  (port 5102)   │
                    └────────────────┘
```

- **LearningShop.Gateway** — YARP reverse proxy that routes `/api/products` and `/api/orders` to the backend services. CORS is configured for the Angular dev server.
- **LearningShop.ProductService** — ASP.NET Core Web API exposing product data.
- **LearningShop.OrderService** — ASP.NET Core Web API exposing order data.
- **learning-shop-ui** — Angular 22 single-page application.

## Prerequisites

- **.NET 10 SDK** (or compatible)
- **Node.js 20+** (for Angular)
- **Docker** and **Docker Compose** (for containerized runs)

## Development

### Backend

Build and run each service from the repository root:

```bash
dotnet run --project backend/services/LearningShop.ProductService
dotnet run --project backend/services/LearningShop.OrderService
dotnet run --project backend/gateway/LearningShop.Gateway
```

The gateway expects the services on `http://localhost:5101` and `http://localhost:5102` by default (see `backend/gateway/LearningShop.Gateway/appsettings.json`).

### Frontend

```bash
cd frontend/learning-shop-ui
npm install
ng serve
```

The Angular dev server is proxied to the gateway via `proxy.conf.json`, so API calls go to `/api/*`.

## Docker Compose

The project ships with a `docker-compose.yml` at the repository root that builds and runs all four services. Each backend service has its own multi-stage `Dockerfile`; the frontend is built with Node and served by Nginx.

```bash
docker compose up --build
```

After the build completes:

| Service            | URL                                        |
|--------------------|--------------------------------------------|
| Angular UI         | http://localhost                           |
| API Gateway         | http://localhost:5100                      |
| Product Service    | http://localhost:5101                      |
| Order Service      | http://localhost:5102                      |

In the containerized setup the gateway uses `appsettings.Docker.json` so it routes to the service containers (`products`, `orders`) instead of localhost.

## Project structure

```
LearningShop/
├── backend/
│   ├── gateway/LearningShop.Gateway/
│   ├── services/LearningShop.ProductService/
│   └── services/LearningShop.OrderService/
├── frontend/learning-shop-ui/
├── docker-compose.yml
└── README.md
```

## License

This is a sample/learning project.