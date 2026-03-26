FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY ["src/Services/AuthService/Licit.AuthService.API/Licit.AuthService.API.csproj", "Services/AuthService/Licit.AuthService.API/"]
COPY ["src/Services/AuthService/Licit.AuthService.Application/Licit.AuthService.Application.csproj", "Services/AuthService/Licit.AuthService.Application/"]
COPY ["src/Services/AuthService/Licit.AuthService.Domain/Licit.AuthService.Domain.csproj", "Services/AuthService/Licit.AuthService.Domain/"]
COPY ["src/Services/AuthService/Licit.AuthService.Infrastructure/Licit.AuthService.Infrastructure.csproj", "Services/AuthService/Licit.AuthService.Infrastructure/"]
RUN dotnet restore "Services/AuthService/Licit.AuthService.API/Licit.AuthService.API.csproj"

COPY src/Services/AuthService/ Services/AuthService/
WORKDIR /src/Services/AuthService/Licit.AuthService.API
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Licit.AuthService.API.dll"]
