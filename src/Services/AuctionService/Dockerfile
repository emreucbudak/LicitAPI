FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY ["src/Services/AuctionService/Licit.AuctionService.API/Licit.AuctionService.API.csproj", "Services/AuctionService/Licit.AuctionService.API/"]
COPY ["src/Services/AuctionService/Licit.AuctionService.Application/Licit.AuctionService.Application.csproj", "Services/AuctionService/Licit.AuctionService.Application/"]
COPY ["src/Services/AuctionService/Licit.AuctionService.Domain/Licit.AuctionService.Domain.csproj", "Services/AuctionService/Licit.AuctionService.Domain/"]
COPY ["src/Services/AuctionService/Licit.AuctionService.Infrastructure/Licit.AuctionService.Infrastructure.csproj", "Services/AuctionService/Licit.AuctionService.Infrastructure/"]
COPY ["src/Services/AuctionService/Licit.AuctionService.Persistence/Licit.AuctionService.Persistence.csproj", "Services/AuctionService/Licit.AuctionService.Persistence/"]
RUN dotnet restore "Services/AuctionService/Licit.AuctionService.API/Licit.AuctionService.API.csproj"

COPY src/Services/AuctionService/ Services/AuctionService/
WORKDIR /src/Services/AuctionService/Licit.AuctionService.API
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Licit.AuctionService.API.dll"]
