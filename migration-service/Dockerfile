# ---------- build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish migration-service.csproj -c Release -o /app/publish

# ---------- runtime stage ----------
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Copy Migrations folder into runtime image
COPY ./Migrations /app/Migrations

ENTRYPOINT ["dotnet", "migration-service.dll"]
