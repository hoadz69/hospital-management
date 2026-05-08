$ErrorActionPreference = "Stop"

docker compose -f "$PSScriptRoot/../docker/docker-compose.dev.yml" down
