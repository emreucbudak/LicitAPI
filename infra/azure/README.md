# Azure Container Apps Notes

This repo is prepared to run in a single Azure Container Apps environment with these app names:

- `gateway` - external HTTP ingress, target port `8080`
- `auth-service` - internal HTTP ingress, target port `8080`
- `tendering-service` - internal HTTP ingress, target port `8080`
- `wallet-service` - internal HTTP ingress, target port `8080`
- `mail-service` - internal HTTP ingress, target port `8080`
- `rabbitmq` - internal TCP ingress, target port `5672`, min replicas `1`

The gateway also expects these licit-go apps to exist in the same ACA environment:

- `bidding-engine` - internal HTTP ingress, target port `5160`
- `auction-streamer` - internal HTTP ingress, target port `5161`

## Required Key Vault Secret Mapping

### `gateway`

- `RateLimiting__Redis__ConnectionString` -> `azure-redis-connection-string`
- `Cors__AllowedOrigins__0` -> your frontend URL

### `auth-service`

- `ConnectionStrings__DefaultConnection` -> `auth-db-connection-string`
- `JwtSettings__Secret` -> `jwt-secret`
- `Redis__ConnectionString` -> `azure-redis-connection-string`
- `RabbitMq__Host` = `rabbitmq`
- `RabbitMq__Port` = `5672`
- `RabbitMq__Username` -> `rabbitmq-username`
- `RabbitMq__Password` -> `rabbitmq-password`

### `tendering-service`

- `ConnectionStrings__DefaultConnection` -> `tendering-db-connection-string`
- `JwtSettings__Secret` -> `jwt-secret`
- `Redis__ConnectionString` -> `azure-redis-connection-string`
- `RabbitMq__Host` = `rabbitmq`
- `RabbitMq__Port` = `5672`
- `RabbitMq__Username` -> `rabbitmq-username`
- `RabbitMq__Password` -> `rabbitmq-password`

### `wallet-service`

- `ConnectionStrings__DefaultConnection` -> `wallet-db-connection-string`
- `JwtSettings__Secret` -> `jwt-secret`
- `Redis__ConnectionString` -> `azure-redis-connection-string`

### `mail-service`

- `ConnectionStrings__DefaultConnection` -> `mail-db-connection-string`
- `JwtSettings__Secret` -> `jwt-secret`
- `AzureCommunicationEmail__ConnectionString` -> `azure-communication-connection-string`
- `AzureCommunicationEmail__FromEmail` -> `azure-communication-from-email`
- `RabbitMq__Host` = `rabbitmq`
- `RabbitMq__Port` = `5672`
- `RabbitMq__Username` -> `rabbitmq-username`
- `RabbitMq__Password` -> `rabbitmq-password`

## Notes

- The gateway production config no longer includes the local frontend catch-all route.
- Database migrations are applied automatically by the .NET services at startup.
- Keep only one public gateway. For the combined system, the recommended public endpoint is this repo's `gateway` app.
