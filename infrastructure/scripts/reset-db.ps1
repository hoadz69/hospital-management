$ErrorActionPreference = "Stop"

docker compose -f "$PSScriptRoot/../docker/docker-compose.dev.yml" down -v
docker compose -f "$PSScriptRoot/../docker/docker-compose.dev.yml" up -d postgres mongodb redis
