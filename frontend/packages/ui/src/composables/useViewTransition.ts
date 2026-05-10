import { readonly, ref, unref, type MaybeRef, type Ref } from "vue";

type NativeViewTransition = {
  finished: Promise<void>;
};

type ViewTransitionDocument = Document & {
  startViewTransition?: (callback: () => void | Promise<void>) => NativeViewTransition;
};

export type ViewTransitionOptions = {
  disabled?: MaybeRef<boolean>;
};

export type ViewTransitionControls = {
  isRunning: Readonly<Ref<boolean>>;
  run<T>(callback: () => T | Promise<T>): Promise<T>;
};

function supportsViewTransition(): boolean {
  return (
    typeof document !== "undefined" &&
    typeof (document as ViewTransitionDocument).startViewTransition === "function"
  );
}

export function useViewTransition(options: ViewTransitionOptions = {}): ViewTransitionControls {
  const isRunning = ref(false);

  async function run<T>(callback: () => T | Promise<T>): Promise<T> {
    if (unref(options.disabled) || !supportsViewTransition()) {
      return await callback();
    }

    let result: T;
    let callbackError: unknown;
    const transition = (document as ViewTransitionDocument).startViewTransition?.(async () => {
      try {
        result = await callback();
      } catch (error) {
        callbackError = error;
      }
    });

    if (!transition) {
      return await callback();
    }

    isRunning.value = true;
    try {
      await transition.finished;
      if (callbackError) {
        throw callbackError;
      }
      return result!;
    } finally {
      isRunning.value = false;
    }
  }

  return {
    isRunning: readonly(isRunning),
    run
  };
}
