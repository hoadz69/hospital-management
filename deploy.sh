#!/bin/bash
set -euo pipefail

# Clinic SaaS deployment helper.
# File này giữ vai trò deploy script như tài liệu cũ, nhưng hiện chỉ là checklist an toàn.
# Không deploy thật cho đến khi owner cung cấp server, registry, domain, SSH và database details.

VALID_SERVICES="api-gateway owner-admin clinic-admin public-web identity-service tenant-service website-cms-service template-service domain-service booking-service catalog-service customer-service billing-service report-service notification-service realtime-gateway"

usage() {
    echo "Usage: bash deploy.sh [service...]"
    echo ""
    echo "  No args          In checklist deploy toàn hệ thống"
    echo "  service...       In checklist deploy service cụ thể"
    echo ""
    echo "Services dự kiến: $VALID_SERVICES"
    echo ""
    echo "Script này chưa deploy thật vì production config chưa được owner cung cấp."
    exit 0
}

[ "${1:-}" = "-h" ] || [ "${1:-}" = "--help" ] && usage

SERVICES="$*"

echo "==> Clinic SaaS deployment checklist"
[ -n "$SERVICES" ] && echo "==> Services: $SERVICES" || echo "==> Services: all"
echo ""
echo "Cần owner xác nhận trước khi deploy thật:"
echo "1. Target environment: dev/staging/production"
echo "2. Git branch/tag"
echo "3. Image registry và tag"
echo "4. Server path và SSH method"
echo "5. Domain/subdomain platform"
echo "6. SSL/domain verification flow"
echo "7. Database/server secrets qua kênh an toàn"
echo "8. Migration strategy và rollback"
echo ""
echo "Không có lệnh deploy nào được chạy trong placeholder này."
