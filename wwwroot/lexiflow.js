window.lexiFlow = {
    auth: {
        _bindings: new Map(),
        _nextBindingId: 1,

        bindLoginAutofill: (dotnetRef) => {
            const auth = window.lexiFlow.auth;
            const id = `login-autofill-${auth._nextBindingId++}`;
            const emailField = document.querySelector(".login-email-field input");
            const passwordField = document.querySelector(".login-password-field input");
            const inputs = [emailField, passwordField].filter(Boolean);
            const listeners = [];

            const sync = () => {
                dotnetRef.invokeMethodAsync("SyncAutofillValues", emailField?.value ?? "", passwordField?.value ?? "");
            };

            const addListener = (target, eventName, handler) => {
                if (!target) return;

                target.addEventListener(eventName, handler, true);
                listeners.push([target, eventName, handler]);
            };

            for (const input of inputs) {
                addListener(input, "input", sync);
                addListener(input, "change", sync);
                addListener(input, "animationstart", sync);
                addListener(input, "focus", sync);
                addListener(input, "blur", sync);
            }

            addListener(window, "pageshow", sync);
            addListener(document, "visibilitychange", sync);

            for (const delay of [0, 80, 250, 600, 1200])
                setTimeout(sync, delay);

            auth._bindings.set(id, listeners);
            return id;
        },

        disposeAutofill: (id) => {
            const auth = window.lexiFlow.auth;
            const listeners = auth._bindings.get(id);

            if (!listeners) return;

            for (const [target, eventName, handler] of listeners)
                target.removeEventListener(eventName, handler, true);

            auth._bindings.delete(id);
        }
    },

    storage: {
        get: (key) => localStorage.getItem(key),
        set: (key, value) => localStorage.setItem(key, value),
        remove: (key) => localStorage.removeItem(key)
    },

    audio: {
        _current: null,

        play: (url) => window.lexiFlow.audio.playExclusive(url),

        playExclusive: async (url) => {
            if (!url) return;

            const audioService = window.lexiFlow.audio;
            const previous = audioService._current;

            if (previous) {
                previous.pause();
                previous.currentTime = 0;
            }

            const audio = new Audio(url);
            audioService._current = audio;

            try {
                await audio.play();
            } catch (error) {
                console.error("Pronunciation playback failed", error);
                throw error;
            }
        }
    },

    testGuard: {
        _handler: (event) => {
            event.preventDefault();
            event.returnValue = "";
        },

        enable: () => window.addEventListener("beforeunload", window.lexiFlow.testGuard._handler),
        disable: () => window.removeEventListener("beforeunload", window.lexiFlow.testGuard._handler)
    },

    photo: {
        _cleanup: null,

        bind: (zone, dotnet) => {
            const photo = window.lexiFlow.photo;
            photo.unbind();

            const assignToInput = (file) => {
                const input = zone?.querySelector("input[type=file]");
                if (!input || !file) return false;

                try {
                    const transfer = new DataTransfer();
                    transfer.items.add(file);

                    input.files = transfer.files;
                    input.dispatchEvent(new Event("change", { bubbles: true }));
                    return true;
                } catch (error) {
                    console.error("Could not attach image to upload input", error);
                    return false;
                }
            };

            const send = (file) => {
                if (!file?.type?.startsWith("image/")) return;

                if (!assignToInput(file))
                    dotnet.invokeMethodAsync("FailImageUpload", "The image could not be attached for upload.");
            };

            const paste = (event) => {
                const item = [...(event.clipboardData?.items ?? [])].find(item => item.type.startsWith("image/"));
                if (!item) return;

                event.preventDefault();
                send(item.getAsFile());
            };

            const dragOver = (event) => {
                event.preventDefault();
                dotnet.invokeMethodAsync("SetDragging", true);
            };

            const dragLeave = () => {
                dotnet.invokeMethodAsync("SetDragging", false);
            };

            const drop = (event) => {
                event.preventDefault();
                event.stopPropagation();

                dragLeave();
                send(event.dataTransfer?.files?.[0]);
            };

            document.addEventListener("paste", paste);
            zone?.addEventListener("dragover", dragOver);
            zone?.addEventListener("dragleave", dragLeave);
            zone?.addEventListener("drop", drop);

            photo._cleanup = () => {
                document.removeEventListener("paste", paste);
                zone?.removeEventListener("dragover", dragOver);
                zone?.removeEventListener("dragleave", dragLeave);
                zone?.removeEventListener("drop", drop);
            };
        },

        unbind: () => {
            window.lexiFlow.photo._cleanup?.();
            window.lexiFlow.photo._cleanup = null;
        }
    },

    phrase: {
        tokenIndexAt: (clientX, clientY) => {
            const element = document.elementFromPoint(clientX, clientY);
            const token = element?.closest?.("[data-token-index]");

            if (!token) return null;

            const value = token.getAttribute("data-token-index");
            if (!value) return null;

            const index = Number.parseInt(value, 10);
            return Number.isFinite(index) ? index : null;
        }
    },

    diagnostics: {
        getEnvironment: () => ({
            userAgent: navigator.userAgent || "",
            language: navigator.language || "",
            platform: navigator.platform || "",
            referrer: document.referrer || ""
        })
    }
};
