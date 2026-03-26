FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY ["src/Services/MailService/Licit.MailService.API/Licit.MailService.API.csproj", "Services/MailService/Licit.MailService.API/"]
COPY ["src/Services/MailService/Licit.MailService.Application/Licit.MailService.Application.csproj", "Services/MailService/Licit.MailService.Application/"]
COPY ["src/Services/MailService/Licit.MailService.Domain/Licit.MailService.Domain.csproj", "Services/MailService/Licit.MailService.Domain/"]
COPY ["src/Services/MailService/Licit.MailService.Infrastructure/Licit.MailService.Infrastructure.csproj", "Services/MailService/Licit.MailService.Infrastructure/"]
RUN dotnet restore "Services/MailService/Licit.MailService.API/Licit.MailService.API.csproj"

COPY src/Services/MailService/ Services/MailService/
WORKDIR /src/Services/MailService/Licit.MailService.API
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Licit.MailService.API.dll"]
