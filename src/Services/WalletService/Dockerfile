FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY ["src/Services/WalletService/Licit.WalletService.API/Licit.WalletService.API.csproj", "Services/WalletService/Licit.WalletService.API/"]
COPY ["src/Services/WalletService/Licit.WalletService.Application/Licit.WalletService.Application.csproj", "Services/WalletService/Licit.WalletService.Application/"]
COPY ["src/Services/WalletService/Licit.WalletService.Domain/Licit.WalletService.Domain.csproj", "Services/WalletService/Licit.WalletService.Domain/"]
COPY ["src/Services/WalletService/Licit.WalletService.Infrastructure/Licit.WalletService.Infrastructure.csproj", "Services/WalletService/Licit.WalletService.Infrastructure/"]
RUN dotnet restore "Services/WalletService/Licit.WalletService.API/Licit.WalletService.API.csproj"

COPY src/Services/WalletService/ Services/WalletService/
WORKDIR /src/Services/WalletService/Licit.WalletService.API
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Licit.WalletService.API.dll"]
