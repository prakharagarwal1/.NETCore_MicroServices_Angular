# LearningShop

A demo e-commerce microservices application built with **.NET 10** and **Angular 22**. The project exposes a product catalog and order management API through a YARP-based reverse proxy, and provides a single-page Angular frontend.

## Architecture

```
┌─────────────┐     ┌──────────────┐     ┌────────────────┐
│  Angular UI │────▶│  API Gateway │────▶│  ProductService │
│  (port 80)  │     │  (port 5000) │     │  (port 5001)   │
└─────────────┘     └──────────────┘     └────────────────┘
                              │
                              ▼
                    ┌────────────────┐
                    │  OrderService  │
                    │  (port 5002)   │
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

The gateway expects the services on `http://localhost:5001` and `http://localhost:5002` by default (see `backend/gateway/LearningShop.Gateway/appsettings.Development.json`).

### Frontend

```bash
cd frontend/learning-shop-ui
npm install
ng serve
```

The Angular dev server is proxied to the gateway via `proxy.conf.json`, so API calls go to `/api/*`.

## Docker Compose

The project ships with a `docker-compose.yml` and development override at the repository root. The override bind-mounts backend source into containers for `dotnet watch` and runs the Angular development server. Docker Compose Watch syncs UI source changes and rebuilds/recreates a service when its project, dependency, or development Dockerfile changes. Use Docker Compose v2.22.0 or later.

```bash
docker compose up --build --watch
```

Open the Angular dev server at http://localhost:4200. Backend source edits are picked up by `dotnet watch`; Angular source edits are synced to `ng serve`. Reload the browser to fetch data after a backend restart. Changes to a `.csproj`, `package.json`, `package-lock.json`, or a development Dockerfile trigger a rebuild and recreate the affected container. Press Ctrl+C to stop the stack.

| Service            | URL                                        |
|--------------------|--------------------------------------------|
| Angular UI         | http://localhost:4200                      |
| API Gateway        | http://localhost:5000                      |
| Product Service    | http://localhost:5001                      |
| Order Service      | http://localhost:5002                      |

In the containerized setup the gateway uses `appsettings.json` so it routes to the service containers (`products`, `orders`) instead of localhost.

## API Documentation (Swagger)

Each service ships with Swagger UI. In development mode, open these URLs:

| Service | Swagger UI | OpenAPI JSON |
|---------|-----------|--------------|
| API Gateway | http://localhost:5000/swagger | http://localhost:5000/swagger/v1/swagger.json |
| Product Service | http://localhost:5001/swagger | http://localhost:5001/swagger/v1/swagger.json |
| Order Service | http://localhost:5002/swagger | http://localhost:5002/swagger/v1/swagger.json |

### Authentication

The gateway exposes a `POST /api/auth/login` endpoint that returns a JWT token. Hardcoded users:

| Username | Email | Password | Role |
|----------|-------|----------|------|
| `admin` | `admin@example.com` | `Admin@123` | Admin |
| `customer` | `customer@example.com` | `Customer@123` | Customer |
| `seller` | `seller@example.com` | `Seller@123` | Seller |

Use the returned token as `Authorization: Bearer <token>` on protected routes.

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