import { createApp } from "vue";
import "@clinic-saas/design-tokens/src/v3/v3.css";
import App from "./App.vue";
import { router } from "./router";

createApp(App).use(router).mount("#app");
