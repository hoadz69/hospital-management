export const v3Motion = {
  duration: {
    xs: "120ms",
    sm: "180ms",
    md: "240ms",
    lg: "320ms",
    xl: "480ms"
  },
  ease: {
    standard: "cubic-bezier(0.2, 0, 0, 1)"
  }
} as const;

export type V3Motion = typeof v3Motion;
