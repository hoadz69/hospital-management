import vue from "@vitejs/plugin-vue";
import { defineConfig, loadEnv } from "vite";

// Cấu hình Vite dev cho Owner Admin.
// Khi smoke real API, gateway thật bind 127.0.0.1:5005 (qua SSH tunnel từ máy Lead) và chưa
// cấu hình CORS cho origin localhost:5175. Để FE browser call được mà không sửa backend, dùng
// Vite dev proxy: FE call same-origin /api/* -> Vite forward sang VITE_DEV_PROXY_TARGET (default
// http://127.0.0.1:5005). Khi VITE_API_BASE_URL = "" (empty), client sẽ dùng same-origin nên
// proxy hoạt động minh bạch. KHÔNG sửa backend, KHÔNG mở CORS public.
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), "");
  const proxyTarget = env.VITE_DEV_PROXY_TARGET ?? "http://127.0.0.1:5005";

  return {
    plugins: [vue()],
    server: {
      proxy: {
        "/api": {
          target: proxyTarget,
          changeOrigin: true,
          secure: false
        }
      }
    }
  };
});
