// Tunn wrapper för att montera/avmontera Optima Store-embedden (extern e-handel)
// i en container-div. Anvands via IJSRuntime fran Pages/Web/Ehandel.razor.
//
// Scriptet fran Optima ar en egen liten SPA (Preact) som letar upp sin
// container via data-container och bootstrapar sig sjalv nar det laddas.
// Blazor kor sin egen DOM-diffing vid navigering, sa <script>-taggar som
// bara skrivs ut i razor-markup kors aldrig av webblasaren - darfor
// injiceras scriptet imperativt har istallet.
window.optimaStore = {
    scriptId: "optima-store-script",

    mount: function (containerId, opts) {
        this.unmount(containerId);

        var container = document.getElementById(containerId);
        if (!container) return;

        var script = document.createElement("script");
        script.id = this.scriptId;
        script.src = opts.scriptUrl;
        script.async = true;
        script.dataset.key = opts.apiKey;
        script.dataset.api = opts.apiUrl;
        script.dataset.container = "#" + containerId;
        document.body.appendChild(script);
    },

    // Kors nar man navigerar bort fran sidan (Dispose i Razor-komponenten),
    // sa att widgeten inte fortsatter kora mot en container som Blazor
    // redan har tagit bort ur DOM:en.
    unmount: function (containerId) {
        var existing = document.getElementById(this.scriptId);
        if (existing) existing.remove();

        if (containerId) {
            var container = document.getElementById(containerId);
            if (container) container.innerHTML = "";
        }
    }
};
