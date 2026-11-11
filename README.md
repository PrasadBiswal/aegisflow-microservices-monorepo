# AegisFlow Microservices Monorepo

Ten .NET microservices on a shared platform: Docker, GitHub Actions CI, Terraform on AWS
(VPC, EKS, RDS, ECR), Helm and ArgoCD.

## Services

| Service | Responsibility | Database | Local port |
|---|---|---|---|
| users-api | User accounts | UsersDb | 8081 |
| catalog-api | Product catalog | CatalogDb | 8082 |
| inventory-api | Stock levels | InventoryDb | 8083 |
| cart-api | Shopping carts | CartDb | 8084 |
| orders-api | Orders | OrdersDb | 8085 |
| payments-api | Payments | PaymentsDb | 8086 |
| shipping-api | Shipments | ShippingDb | 8087 |
| notifications-api | Email / SMS notifications | NotificationsDb | 8088 |
| reviews-api | Product reviews | ReviewsDb | 8089 |
| reporting-api | Reports | ReportingDb | 8090 |

> **Status:** all 10 services are implemented (endpoints, validation, business rules, tests).

Each service owns its database. Services never read another service's database.

## Repository layout

```
AegisFlow.sln                  Builds and tests every project with one command
global.json                    Pins the .NET SDK (10.x; services run on .NET 8)
Directory.Build.props          Shared project settings (target framework, warnings as errors)
Directory.Packages.props       One version per NuGet package, for all projects
libs/AegisFlow.ServiceDefaults Shared setup: JSON logging, errors, health checks, SQL Server, "migrate"
services/<name>-api/           One folder per service (Dockerfile builds any of them)
tests/<name>-api.Tests/        Unit and in-memory API tests per service
tests/postman/                 Postman collection + environments
charts/microservice-base/      Shared Helm chart
terraform/                     AWS infrastructure
```

## Service conventions

| Item | Convention |
|---|---|
| Endpoints | `/` (service info), `/health/live`, `/health/ready`, `/api/<resource>` |
| API docs | `/swagger` (Development only) |
| Configuration | Environment variables `DB_SERVER`, `DB_NAME`, `DB_USER`, `DB_PASSWORD`, `DB_TRUST_SERVER_CERTIFICATE` |
| Database changes | EF Core migrations in `services/<name>/Migrations/` |
| Applying migrations | Run the service with the `migrate` argument as a one-off step (Compose: `<name>-migrate`; Kubernetes: a Job) |
| Logs | JSON to stdout |

## Run locally

### Prerequisites
- Docker Desktop, with at least 4 GB of memory for containers
- .NET SDK 10 **and** the .NET 8 runtime (services target `net8.0`): `winget install Microsoft.DotNet.SDK.8`

### First-time setup
```powershell
Copy-Item .env.example .env          # then set DB_PASSWORD in .env
dotnet tool restore                  # installs dotnet-ef from .config/dotnet-tools.json
```

### Create the first migrations (once, then commit the Migrations folders)
```powershell
'users','catalog','inventory','cart','orders','payments','shipping','notifications','reviews','reporting' |
  ForEach-Object { dotnet ef migrations add InitialCreate --project "services/$_-api" }
```

### Build and test
```powershell
dotnet build AegisFlow.sln
dotnet test AegisFlow.sln
```

### Run the stack
```powershell
docker compose up --build
```
SQL Server starts first, then each `<name>-migrate` creates its database, then `<name>` starts on its port (8081–8090, see the table above).

- Service info: http://localhost:8081/
- Swagger UI: http://localhost:8081/swagger
- Readiness: http://localhost:8081/health/ready

### Postman
Import `tests/postman/aegisflow.postman_collection.json` and `tests/postman/local.postman_environment.json`,
select the **AegisFlow - local** environment, and run the collection. Or from the command line:
```powershell
npx newman run tests/postman/aegisflow.postman_collection.json -e tests/postman/local.postman_environment.json
```

## Delivery phases

1. **Local dev and test**: .NET APIs, Docker Compose with SQL Server, unit tests, Postman
2. **Git workflow**: feature branches, pull requests into `dev`
3. **CI**: build, unit tests, Docker images, push to ECR
4. **AWS sandbox deploy**: GitHub OIDC, Terraform (VPC/EKS/RDS), Helm releases via ArgoCD
