FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY ["src/Gateway/Licit.Gateway.API/Licit.Gateway.API.csproj", "Gateway/Licit.Gateway.API/"]
RUN dotnet restore "Gateway/Licit.Gateway.API/Licit.Gateway.API.csproj"

COPY src/Gateway/ Gateway/
WORKDIR /src/Gateway/Licit.Gateway.API
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Licit.Gateway.API.dll"]
