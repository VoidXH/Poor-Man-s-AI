    "use strict";

    function syncSwatches(swatches, theme) {
        swatches.forEach(function (s) {
            s.classList.toggle("active", s.dataset.theme === theme);
        });
    }

    function initThemeSwatches() {
        var swatches = document.querySelectorAll(".theme-swatch");
        if (swatches.length === 0) {
            return;
        }

        var currentTheme = document.documentElement.classList.contains("theme-blue")
            ? "blue"
            : document.documentElement.classList.contains("theme-red")
                ? "red"
                : "green";

        syncSwatches(swatches, currentTheme);

        swatches.forEach(function (swatch) {
            swatch.addEventListener("click", function () {
                var theme = swatch.dataset.theme;
                document.documentElement.classList.remove("theme-green", "theme-blue", "theme-red");
                document.documentElement.classList.add("theme-" + theme);
                document.cookie = "theme=" + theme + "; path=/; max-age=31536000";
                syncSwatches(swatches, theme);
            });
        });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initThemeSwatches);
    } else {
        initThemeSwatches();
    }
