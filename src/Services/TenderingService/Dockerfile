FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY ["src/Services/TenderingService/Licit.TenderingService.API/Licit.TenderingService.API.csproj", "Services/TenderingService/Licit.TenderingService.API/"]
COPY ["src/Services/TenderingService/Licit.TenderingService.Application/Licit.TenderingService.Application.csproj", "Services/TenderingService/Licit.TenderingService.Application/"]
COPY ["src/Services/TenderingService/Licit.TenderingService.Domain/Licit.TenderingService.Domain.csproj", "Services/TenderingService/Licit.TenderingService.Domain/"]
COPY ["src/Services/TenderingService/Licit.TenderingService.Infrastructure/Licit.TenderingService.Infrastructure.csproj", "Services/TenderingService/Licit.TenderingService.Infrastructure/"]
RUN dotnet restore "Services/TenderingService/Licit.TenderingService.API/Licit.TenderingService.API.csproj"

COPY src/Services/TenderingService/ Services/TenderingService/
WORKDIR /src/Services/TenderingService/Licit.TenderingService.API
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Licit.TenderingService.API.dll"]
