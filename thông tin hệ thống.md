# Thông tin sản phẩm
Xây dựng nền tảng chatbot AI cho phép các chủ shop (Merchants) tự động hóa việc chăm sóc khách hàng trên Facebook, TikTok, Shopee bằng cách sử dụng chính dữ liệu sản phẩm/dịch vụ của họ (từ file Excel, Docx) để huấn luyện AI trả lời thông minh, chính xác.

# Thông tin hệ thống
## Công nghệ nền tảng

### Backend
- net 10 - LTS
- Clean Architecdt
- Microservice
- Cache redis

### Frontend
#### Trang quảng trị người bán hàng
- React JS
- Daisy UI + Tailwind Multiple mode theme, đa ngôn ngữ
- Redux toolkit

### Database
- PostgreSQL bản 17

### External service
- Các liên quan đến file sẽ đều lưu trữ lên Cloudinary


### 1. Management API (Cụm Quản trị)
Đây là nơi bạn xử lý mọi thứ liên quan đến người dùng và thiết lập.
* **Chức năng:** Đăng ký, đăng nhập (Auth), quản lý thông tin Merchant.
* **Nhiệm vụ AI:** Lưu trữ `System Prompt` (tính cách bot) và các cài đặt về Model (Temperature, Max tokens).
* **Database:** Tương tác chính với PostgreSQL để lưu cấu hình lâu dài.

### 2. File API (Cụm Xử lý dữ liệu)
Đóng vai trò là "máy nghiền" kiến thức cho AI.
* **Chức năng:** Tiếp nhận file Excel, Docx từ chủ shop.
* **Nhiệm vụ AI:** Đọc nội dung file $\rightarrow$ Chuyển sang Markdown $\rightarrow$ Gọi AI để tạo **Context Cache**.
* **Đầu ra:** Trả về một `CacheID` (ID kiến thức) để lưu vào database.

### 3. Webhook API (Cụm Tiếp nhận)
Đây là "tiền tuyến" tiếp xúc với các nền tảng social.
* **Chức năng:** Nhận tin nhắn từ Facebook, TikTok, Shopee.
* **Nhiệm vụ quan trọng:** Xác thực tin nhắn (Signature verify) để tránh hacker và trả về mã `200 OK` ngay lập tức cho các sàn (để họ không gửi lặp tin nhắn).
* **Điều hướng:** Đẩy tin nhắn vào hàng đợi hoặc chuyển thẳng sang Chatbot API.

### 4. Chatbot API (Cụm Thực thi)
Đây là "trái tim" vận hành luồng chat RAG.
* **Chức năng:** Lấy lịch sử chat cũ (từ Redis) + Lấy `CacheID` (từ Database).
* **Nhiệm vụ AI:** Kết hợp mọi thứ thành một Prompt hoàn chỉnh $\rightarrow$ Gọi AI lấy câu trả lời.
* **Phản hồi:** Gọi ngược lại API của Facebook/TikTok để trả lời khách hàng.

### 5. Frontend trang quản trị người bán