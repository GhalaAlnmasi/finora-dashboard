window.finora = {
    storageGet: function (key) {
        try {
            return window.localStorage.getItem(key);
        } catch {
            return null;
        }
    },
    storageSet: function (key, value) {
        try {
            window.localStorage.setItem(key, value);
            return true;
        } catch {
            return false;
        }
    },
    setTheme: function (theme) {
        var value = theme === "dark" ? "dark" : "light";
        document.documentElement.setAttribute("data-theme", value);
        document.documentElement.style.colorScheme = value;
    },
    downloadText: function (filename, contents) {
        var blob = new Blob([contents], { type: "application/json" });
        var url = URL.createObjectURL(blob);
        var link = document.createElement("a");
        link.href = url;
        link.download = filename;
        link.rel = "noopener";
        document.body.appendChild(link);
        link.click();
        link.remove();
        URL.revokeObjectURL(url);
    },
    focus: function (id) {
        document.getElementById(id)?.focus();
    },
    bindFocusTrap: function (element) {
        if (!element || element.dataset.trapBound) {
            return;
        }

        element.dataset.trapBound = "1";
        element.addEventListener("keydown", function (e) {
            if (e.key !== "Tab") {
                return;
            }

            const nodes = [...element.querySelectorAll("button, [href], input, select, textarea, [tabindex]:not([tabindex='-1'])")]
                .filter(el => !el.disabled && el.getClientRects().length > 0);
            if (!nodes.length) {
                return;
            }

            const first = nodes[0];
            const last = nodes[nodes.length - 1];
            if (e.shiftKey && document.activeElement === first) {
                e.preventDefault();
                last.focus();
            } else if (!e.shiftKey && document.activeElement === last) {
                e.preventDefault();
                first.focus();
            }
        });
    }
};
