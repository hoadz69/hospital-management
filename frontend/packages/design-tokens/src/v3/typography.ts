export const v3Typography = {
  fontFamily: {
    sans: "Inter, ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, \"Segoe UI\", sans-serif",
    mono: "\"JetBrains Mono\", \"SFMono-Regular\", Consolas, \"Liberation Mono\", monospace"
  },
  text: {
    display: {
      fontSize: "48px",
      lineHeight: "56px",
      fontWeight: "700"
    },
    headline: {
      fontSize: "32px",
      lineHeight: "40px",
      fontWeight: "700"
    },
    title: {
      fontSize: "24px",
      lineHeight: "32px",
      fontWeight: "600"
    },
    body: {
      fontSize: "16px",
      lineHeight: "24px",
      fontWeight: "400"
    },
    caption: {
      fontSize: "12px",
      lineHeight: "16px",
      fontWeight: "400"
    },
    mono: {
      fontSize: "13px",
      lineHeight: "20px",
      fontWeight: "500"
    }
  }
} as const;

export type V3Typography = typeof v3Typography;
