#!/bin/bash

echo "🚀 EZ Sales Bot - Full API Test Script"
echo "======================================"

# JWT Token (valid until 2126)
JWT_TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwMDAwMDAwMC0wMDAwLTAwMDAtMDAwMC0wMDAwMDAwMDAwMDEiLCJlbWFpbCI6ImFkbWluIiwidGVuYW50X2lkIjoiMDAwMDAwMDAtMDAwMC0wMDAwLTAwMDAtMDAwMDAwMDAwMDAyIiwianRpIjoiYWVlYWRkYmMtYjM5ZS00YjNmLTgxYTYtNzA5MDQ3YjNiOWYyIiwiZXhwIjo0OTI5MjY3MjE3fQ.XuIndFdnwIoFVf3bG6ttG2e5E8zt4KTkdq9GBBL17qM"

TENANT_ID="00000000-0000-0000-0000-000000000002"

echo ""
echo "📝 Tạo file tri thức test..."
cat > test_shop_knowledge.txt << 'EOF'
Dev Shop - Thông tin sản phẩm và khuyến mãi:

🎯 LAPTOP GAMING:
- ASUS ROG Strix: từ 25.000.000đ
- MSI Gaming: từ 22.000.000đ
- Acer Predator: từ 28.000.000đ

🎯 ĐIỆN THOẠI:
- iPhone 15 Pro Max: 35.000.000đ
- Samsung Galaxy S24 Ultra: 32.000.000đ
- Xiaomi 14 Pro: 18.000.000đ

🎯 KHUYẾN MÃI:
- Mã TECH2024: giảm 15% cho tất cả laptop
- Mã PHONE2024: giảm 10% cho điện thoại
- Mã FREESHIP: miễn phí giao hàng cho đơn từ 2 triệu

📞 Hotline: 1900-xxxx
🏪 Địa chỉ: 123 Đường ABC, Quận XYZ, TP.HCM
EOF

echo "✅ Đã tạo file test_shop_knowledge.txt"
echo ""

echo "📤 Step 1: Upload file tri thức..."
UPLOAD_RESPONSE=$(curl -s -X POST http://localhost:5211/api/files/upload \
  -H "Authorization: Bearer $JWT_TOKEN" \
  -F "file=@test_shop_knowledge.txt" \
  -F "tenantId=$TENANT_ID")

echo "📊 Upload Response:"
echo "$UPLOAD_RESPONSE" | jq '.' 2>/dev/null || echo "$UPLOAD_RESPONSE"
echo ""

# Extract file ID
FILE_ID=$(echo "$UPLOAD_RESPONSE" | grep -o '"data":"[^"]*"' | cut -d'"' -f4)
echo "📄 File ID: $FILE_ID"
echo ""

echo "⏳ Step 2: Chờ file được xử lý (15 giây)..."
sleep 15
echo "✅ Đã chờ xong"
echo ""

echo "🤖 Step 3: Test chatbot Q&A - Hỏi về sản phẩm..."
CHAT_RESPONSE_1=$(curl -s -X POST http://localhost:5175/webhook/facebook/test-facebook-sync \
  -H "Content-Type: application/json" \
  -d "{
    \"tenantId\": \"$TENANT_ID\",
    \"message\": \"Shop có bán laptop không? Giá ASUS ROG bao nhiêu?\",
    \"userId\": \"test-user-001\",
    \"userName\": \"Nguyen Van A\"
  }")

echo "💬 Chatbot Response 1:"
echo "$CHAT_RESPONSE_1" | jq -r '.reply' 2>/dev/null || echo "$CHAT_RESPONSE_1"
echo ""

echo "🤖 Step 4: Test chatbot Q&A - Hỏi về khuyến mãi..."
CHAT_RESPONSE_2=$(curl -s -X POST http://localhost:5175/webhook/facebook/test-facebook-sync \
  -H "Content-Type: application/json" \
  -d "{
    \"tenantId\": \"$TENANT_ID\",
    \"message\": \"Mã giảm giá TECH2024 dùng để làm gì?\",
    \"userId\": \"test-user-002\",
    \"userName\": \"Tran Thi B\"
  }")

echo "💬 Chatbot Response 2:"
echo "$CHAT_RESPONSE_2" | jq -r '.reply' 2>/dev/null || echo "$CHAT_RESPONSE_2"
echo ""

echo "🤖 Step 5: Test chatbot Q&A - Hỏi về điện thoại..."
CHAT_RESPONSE_3=$(curl -s -X POST http://localhost:5175/webhook/facebook/test-facebook-sync \
  -H "Content-Type: application/json" \
  -d "{
    \"tenantId\": \"$TENANT_ID\",
    \"message\": \"Giá iPhone 15 Pro Max? Có mã giảm giá không?\",
    \"userId\": \"test-user-003\",
    \"userName\": \"Le Van C\"
  }")

echo "💬 Chatbot Response 3:"
echo "$CHAT_RESPONSE_3" | jq -r '.reply' 2>/dev/null || echo "$CHAT_RESPONSE_3"
echo ""

echo "🎯 Test hoàn thành!"
echo ""
echo "📋 Tóm tắt:"
echo "- File upload: $FILE_ID"
echo "- Chatbot đã sử dụng tri thức từ file để trả lời"
echo "- Kiểm tra xem responses có đề cập đúng thông tin từ file không"
echo ""
echo "🧹 Dọn dẹp..."
rm -f test_shop_knowledge.txt
echo "✅ Đã xóa file test"