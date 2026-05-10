export const v3Colors = {
  brand: {
    clinicalBlue500: "#1E5BD6",
    mint500: "#14B8A6",
    peach300: "#FFB7A0"
  },
  surface: {
    ivory: "#F8FAFC",
    elevated: "#FFFFFF",
    muted: "#F1F5F9",
    borderSubtle: "#E2E8F0"
  },
  text: {
    primary: "#0F172A",
    secondary: "#475569",
    muted: "#94A3B8"
  },
  status: {
    success: "#16A34A",
    warning: "#D97706",
    danger: "#DC2626",
    info: "#0284C7",
    draft: "#64748B",
    specialty: "#7C3AED"
  },
  admin: {
    sidebar: "#0F172A"
  }
} as const;

export type V3Colors = typeof v3Colors;
