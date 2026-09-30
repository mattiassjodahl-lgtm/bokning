# ── Build stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY BookingDemo.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish --verbosity minimal 2>&1

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# MudBlazor CSS/JS requires static web assets (same as Development mode)
ENV ASPNETCORE_ENVIRONMENT=Development

# Railway terminerar TLS framför appen. Utan detta ser appen http:// och bygger
# canonical-, sitemap- och JSON-LD-länkar med fel schema.
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

ENTRYPOINT ["dotnet", "BookingDemo.dll"]
