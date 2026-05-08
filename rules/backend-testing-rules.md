# Backend Testing Rules - Clinic SaaS

## ⚠️ BẮT BUỘC ĐỌC TRƯỚC KHI TEST API

- Xác nhận service nào đã tồn tại và đang chạy port nào.
- Kiểm `/health` trước khi test feature endpoint.
- Không dùng token/user/port giả định nếu chưa có trong task hiện tại.
- Không kết nối database production hoặc server thật nếu owner chưa yêu cầu.
- Không trộn nhiều mode test trong cùng một phiên nếu port/config khác nhau.
- Payload tiếng Việt nên test bằng file JSON UTF-8 để tránh lỗi encoding shell.
- Mọi test result phải ghi rõ port, environment/mode, tenant/user context.

## Trọng Tâm Test Bắt Buộc

- Tenant resolution theo domain/subdomain/header/JWT.
- Clinic Admin không truy cập được tenant khác.
- Owner Super Admin chỉ thực hiện cross-tenant action đã được thiết kế.
- Security context thiếu/invalid phải bị từ chối, không fallback.
- Booking flow tạo appointment đúng state.
- Template apply modes không ghi đè sai field.
- Domain verification xử lý đủ pending, failed và verified.
- Background/event workflow không mất message khi handler lỗi.
- API validation error trả 4xx, không thành 500.
- API response không expose stack trace ngoài môi trường Development.

## Cách Báo Cáo Test

Với mỗi manual/API test, ghi:

- endpoint,
- request summary,
- expected result,
- actual result,
- tenant/user context,
- port và environment/mode,
- pass/fail status.

Nếu service chưa tồn tại, ghi rõ API testing chưa áp dụng.
