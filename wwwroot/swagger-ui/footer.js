(() => {
    const addFooter = () => {
        if (!document.body || document.querySelector(".site-footer")) return;

        const footer = document.createElement("footer");
        footer.className = "site-footer";
        footer.textContent = "Coded by Lethabo Mohapi";
        document.body.append(footer);
    };

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", addFooter, { once: true });
    } else {
        addFooter();
    }
})();
