# ---------- build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app/publish

# ---------- runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 80

# ✅ Install curl (required for Docker health check)
RUN apt-get update && apt-get install -y curl

COPY --from=build /app/publish .
ENTRYPOINT [ "dotnet", "user-service.dll" ]
