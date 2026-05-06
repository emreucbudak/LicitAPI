FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY ["src/Services/BiddingService/Licit.BiddingService.API/Licit.BiddingService.API.csproj", "Services/BiddingService/Licit.BiddingService.API/"]
COPY ["src/Services/BiddingService/Licit.BiddingService.Application/Licit.BiddingService.Application.csproj", "Services/BiddingService/Licit.BiddingService.Application/"]
COPY ["src/Services/BiddingService/Licit.BiddingService.Domain/Licit.BiddingService.Domain.csproj", "Services/BiddingService/Licit.BiddingService.Domain/"]
COPY ["src/Services/BiddingService/Licit.BiddingService.Infrastructure/Licit.BiddingService.Infrastructure.csproj", "Services/BiddingService/Licit.BiddingService.Infrastructure/"]
COPY ["src/Services/BiddingService/Licit.BiddingService.Persistence/Licit.BiddingService.Persistence.csproj", "Services/BiddingService/Licit.BiddingService.Persistence/"]
RUN dotnet restore "Services/BiddingService/Licit.BiddingService.API/Licit.BiddingService.API.csproj"

COPY src/Services/BiddingService/ Services/BiddingService/
WORKDIR /src/Services/BiddingService/Licit.BiddingService.API
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Licit.BiddingService.API.dll"]
