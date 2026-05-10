import { getCurrentScope, onScopeDispose, readonly, ref, type Ref } from "vue";

export type ReducedMotionPreference = {
  prefersReducedMotion: Readonly<Ref<boolean>>;
  isSupported: Readonly<Ref<boolean>>;
};

const REDUCED_MOTION_QUERY = "(prefers-reduced-motion: reduce)";

function canUseMatchMedia(): boolean {
  return typeof window !== "undefined" && typeof window.matchMedia === "function";
}

function registerCleanup(cleanup: () => void): void {
  if (getCurrentScope()) {
    onScopeDispose(cleanup);
  }
}

export function useReducedMotion(): ReducedMotionPreference {
  const prefersReducedMotion = ref(false);
  const isSupported = ref(false);

  if (!canUseMatchMedia()) {
    return {
      prefersReducedMotion: readonly(prefersReducedMotion),
      isSupported: readonly(isSupported)
    };
  }

  const mediaQuery = window.matchMedia(REDUCED_MOTION_QUERY);
  const updatePreference = () => {
    prefersReducedMotion.value = mediaQuery.matches;
  };

  isSupported.value = true;
  updatePreference();

  if (typeof mediaQuery.addEventListener === "function") {
    mediaQuery.addEventListener("change", updatePreference);
    registerCleanup(() => mediaQuery.removeEventListener("change", updatePreference));
  } else {
    mediaQuery.addListener(updatePreference);
    registerCleanup(() => mediaQuery.removeListener(updatePreference));
  }

  return {
    prefersReducedMotion: readonly(prefersReducedMotion),
    isSupported: readonly(isSupported)
  };
}
