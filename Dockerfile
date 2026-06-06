# ── Build stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first (layer-cache friendly)
COPY MotorBikeShop.sln ./
COPY MotorBikeShop/MotorBikeShop.csproj                     MotorBikeShop/
COPY MotorBikeShop.Data/MotorBikeShop.Data.csproj           MotorBikeShop.Data/
COPY MotorBikeShop.Services/MotorBikeShop.Services.csproj   MotorBikeShop.Services/
COPY MotorBikeShop.Tests/MotorBikeShop.Tests.csproj         MotorBikeShop.Tests/

# Restore dependencies
RUN dotnet restore MotorBikeShop.sln

# Copy the rest of the source
COPY . .

# Publish the web app (Release, self-contained=false → uses runtime image)
RUN dotnet publish MotorBikeShop/MotorBikeShop.csproj \
      -c Release \
      -o /app/publish \
      --no-restore

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install sqlcmd (mssql-tools) so the entrypoint can wait for SQL Server
RUN apt-get update \
 && apt-get install -y --no-install-recommends curl gnupg \
 && curl -sSL https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor -o /usr/share/keyrings/microsoft-prod.gpg \
 && curl -sSL https://packages.microsoft.com/config/debian/12/prod.list -o /etc/apt/sources.list.d/mssql-release.list \
 && apt-get update \
 && ACCEPT_EULA=Y apt-get install -y --no-install-recommends mssql-tools18 unixodbc-dev \
 && apt-get clean \
 && rm -rf /var/lib/apt/lists/*

ENV PATH="$PATH:/opt/mssql-tools18/bin"

COPY --from=build /app/publish .

# Write the entrypoint script inline — no external file dependency
RUN printf '%s\n' \
  '#!/bin/bash' \
  'set -e' \
  '' \
  'DB_HOST="${DB_HOST:-db}"' \
  'DB_PORT="${DB_PORT:-1433}"' \
  '' \
  'echo "Waiting for SQL Server at ${DB_HOST}:${DB_PORT} ..."' \
  '' \
  'MAX_RETRIES=30' \
  'RETRY=0' \
  '' \
  'until sqlcmd -S "${DB_HOST},${DB_PORT}" -U sa -P "${DB_SA_PASSWORD}" -Q "SELECT 1" -C -b -o /dev/null 2>&1; do' \
  '  RETRY=$((RETRY + 1))' \
  '  if [ "$RETRY" -ge "$MAX_RETRIES" ]; then' \
  '    echo "SQL Server did not become ready in time. Exiting."' \
  '    exit 1' \
  '  fi' \
  '  echo "  Not ready yet (attempt ${RETRY}/${MAX_RETRIES}). Retrying in 5s ..."' \
  '  sleep 5' \
  'done' \
  '' \
  'echo "SQL Server is ready."' \
  'echo "Starting MotorBikeShop ..."' \
  'exec dotnet MotorBikeShop.dll' \
  > /docker-entrypoint.sh \
 && chmod +x /docker-entrypoint.sh

EXPOSE 8080

ENTRYPOINT ["/docker-entrypoint.sh"]