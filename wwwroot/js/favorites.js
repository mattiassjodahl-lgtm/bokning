// Tunn wrapper kring localStorage for favoritfragor i rapportagenten.
// Anvands via IJSRuntime fran AdminReports.razor.
window.reportFavorites = {
    key: "rp-favorites",

    get: function () {
        try {
            const raw = localStorage.getItem(this.key);
            if (!raw) return [];
            const parsed = JSON.parse(raw);
            return Array.isArray(parsed) ? parsed : [];
        } catch {
            return [];
        }
    },

    save: function (list) {
        try {
            localStorage.setItem(this.key, JSON.stringify(list || []));
            return true;
        } catch {
            return false;
        }
    }
};
