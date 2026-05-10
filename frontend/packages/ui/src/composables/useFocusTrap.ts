import {
  getCurrentScope,
  readonly,
  ref,
  unref,
  watch,
  onScopeDispose,
  type MaybeRef,
  type Ref
} from "vue";

export type FocusTrapOptions = {
  active?: MaybeRef<boolean>;
  initialFocus?: MaybeRef<HTMLElement | null | undefined>;
  restoreFocus?: boolean;
  escapeDeactivates?: boolean;
  onEscape?: () => void;
};

export type FocusTrapControls = {
  isActive: Readonly<Ref<boolean>>;
  activate(): void;
  deactivate(): void;
};

const FOCUSABLE_SELECTOR = [
  "a[href]",
  "button:not([disabled])",
  "textarea:not([disabled])",
  "input:not([disabled])",
  "select:not([disabled])",
  "[tabindex]:not([tabindex='-1'])"
].join(",");

function canUseDocument(): boolean {
  return typeof document !== "undefined";
}

function getFocusableElements(container: HTMLElement): HTMLElement[] {
  return Array.from(container.querySelectorAll<HTMLElement>(FOCUSABLE_SELECTOR)).filter(
    (element) => !element.hasAttribute("disabled") && element.getAttribute("aria-hidden") !== "true"
  );
}

export function useFocusTrap(
  container: Ref<HTMLElement | null>,
  options: FocusTrapOptions = {}
): FocusTrapControls {
  const isActive = ref(false);
  let previousActiveElement: HTMLElement | null = null;

  const restoreFocus = options.restoreFocus ?? true;
  const escapeDeactivates = options.escapeDeactivates ?? true;

  function focusInitialElement(root: HTMLElement): void {
    const configuredInitialFocus = unref(options.initialFocus);
    const target = configuredInitialFocus ?? getFocusableElements(root)[0] ?? root;

    if (typeof target.focus === "function") {
      target.focus({ preventScroll: true });
    }
  }

  function handleKeydown(event: KeyboardEvent): void {
    if (!isActive.value || !container.value) {
      return;
    }

    if (event.key === "Escape" && escapeDeactivates) {
      event.preventDefault();
      options.onEscape?.();
      deactivate();
      return;
    }

    if (event.key !== "Tab") {
      return;
    }

    const focusableElements = getFocusableElements(container.value);
    if (focusableElements.length === 0) {
      event.preventDefault();
      container.value.focus({ preventScroll: true });
      return;
    }

    const firstElement = focusableElements[0];
    const lastElement = focusableElements[focusableElements.length - 1];
    const activeElement = document.activeElement;

    if (event.shiftKey && activeElement === firstElement) {
      event.preventDefault();
      lastElement.focus({ preventScroll: true });
    } else if (!event.shiftKey && activeElement === lastElement) {
      event.preventDefault();
      firstElement.focus({ preventScroll: true });
    }
  }

  function activate(): void {
    if (isActive.value || !canUseDocument() || !container.value) {
      return;
    }

    previousActiveElement =
      document.activeElement instanceof HTMLElement ? document.activeElement : null;
    isActive.value = true;
    document.addEventListener("keydown", handleKeydown);
    focusInitialElement(container.value);
  }

  function deactivate(): void {
    if (!isActive.value || !canUseDocument()) {
      return;
    }

    isActive.value = false;
    document.removeEventListener("keydown", handleKeydown);

    if (restoreFocus && previousActiveElement?.isConnected) {
      previousActiveElement.focus({ preventScroll: true });
    }

    previousActiveElement = null;
  }

  if (options.active !== undefined) {
    watch(
      () => unref(options.active),
      (active) => {
        if (active) {
          activate();
        } else {
          deactivate();
        }
      },
      { immediate: true }
    );
  }

  if (getCurrentScope()) {
    onScopeDispose(() => {
      deactivate();
    });
  }

  return {
    isActive: readonly(isActive),
    activate,
    deactivate
  };
}
