#!/bin/bash
set -e

DEPLOY_DIR="/opt/services/ez-sales-bot"
REPO_URL="https://github.com/LQT1102/ez-sales-bot.git"
BRANCH="develop"
CONFIG_DIR="$DEPLOY_DIR/backend/config"
REQUIRED_CONFIGS=(
    "appsettings.shared.json"
    "appsettings.Production.shared.json"
)

# Valid services
VALID_SERVICES="management-api fileprocessing-api webhook-api frontend-app"

usage() {
    echo "Usage: bash deploy.sh [service...]"
    echo ""
    echo "  No args          Deploy tất cả services"
    echo "  service...       Deploy 1 hoặc nhiều service cụ thể"
    echo ""
    echo "Services: $VALID_SERVICES"
    echo ""
    echo "Examples:"
    echo "  bash deploy.sh                        # deploy tất cả"
    echo "  bash deploy.sh management-api         # chỉ management"
    echo "  bash deploy.sh management-api frontend-app  # 2 services"
    exit 0
}

[ "$1" = "-h" ] || [ "$1" = "--help" ] && usage

SERVICES="$@"

echo "==> Deploying ez-sales-bot"
[ -n "$SERVICES" ] && echo "==> Services: $SERVICES" || echo "==> Services: all"

# Clone or pull
if [ -d "$DEPLOY_DIR/.git" ]; then
    echo "==> Pulling latest code from $BRANCH..."
    cd "$DEPLOY_DIR"
    git fetch origin
    git checkout "$BRANCH"
    git pull origin "$BRANCH"
else
    echo "==> Cloning repository..."
    git clone -b "$BRANCH" "$REPO_URL" "$DEPLOY_DIR"
    cd "$DEPLOY_DIR"
fi

# Check required config files (gitignored, must be present manually)
echo "==> Checking config files..."
MISSING=0
for cfg in "${REQUIRED_CONFIGS[@]}"; do
    if [ ! -f "$CONFIG_DIR/$cfg" ]; then
        echo "  [MISSING] $CONFIG_DIR/$cfg"
        MISSING=1
    else
        echo "  [OK] $cfg"
    fi
done
if [ "$MISSING" -eq 1 ]; then
    echo ""
    echo "ERROR: Missing config files above. Copy them to $CONFIG_DIR and re-run."
    exit 1
fi

cd "$DEPLOY_DIR"

# Build
echo "==> Building..."
if [ -n "$SERVICES" ]; then
    docker compose -f docker-compose.prod.yml build --no-cache $SERVICES
else
    docker compose -f docker-compose.prod.yml build --no-cache
fi

# Start
echo "==> Starting..."
if [ -n "$SERVICES" ]; then
    docker compose -f docker-compose.prod.yml up -d $SERVICES
else
    docker compose -f docker-compose.prod.yml up -d
fi

# Reload nginx
echo "==> Reloading nginx..."
docker exec nginx nginx -s reload

echo ""
echo "==> Done! Access: http://103.75.182.103"
