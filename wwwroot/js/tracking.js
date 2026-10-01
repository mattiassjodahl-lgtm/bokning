// Statistik och marknadsföring för den publika hemsidan: Google Tag Manager,
// Google Analytics 4 och Meta Pixel.
//
// Inget laddas här förrän psTracking.load() anropas, och det sker först när besökaren
// godkänt cookies (se Components/Tracking.razor). Valet sparas i cookien ps_consent.
// ID:n kontrolleras mot samma format som på servern (Helpers/TrackingIds.cs) innan de
// används i en skriptadress.
window.psTracking = (function () {
    'use strict';

    var COOKIE = 'ps_consent';
    var GA    = /^G-[A-Z0-9]{6,12}$/;
    var GTM   = /^GTM-[A-Z0-9]{4,10}$/;
    var PIXEL = /^\d{10,20}$/;
    var loaded = false;

    function readCookie(name) {
        var m = document.cookie.match(new RegExp('(?:^|; )' + name + '=([^;]*)'));
        return m ? decodeURIComponent(m[1]) : null;
    }

    function writeCookie(name, value) {
        var secure = location.protocol === 'https:' ? '; Secure' : '';
        document.cookie = name + '=' + encodeURIComponent(value) +
            '; Path=/; Max-Age=15552000; SameSite=Lax' + secure;   // 180 dagar
    }

    function addScript(src) {
        var s = document.createElement('script');
        s.async = true;
        s.src = src;
        document.head.appendChild(s);
    }

    function loadTagManager(id) {
        window.dataLayer = window.dataLayer || [];
        window.dataLayer.push({ 'gtm.start': Date.now(), event: 'gtm.js' });
        addScript('https://www.googletagmanager.com/gtm.js?id=' + encodeURIComponent(id));
    }

    function loadAnalytics(id) {
        window.dataLayer = window.dataLayer || [];
        window.gtag = function () { window.dataLayer.push(arguments); };
        window.gtag('js', new Date());
        window.gtag('config', id);   // skickar första sidvisningen
        addScript('https://www.googletagmanager.com/gtag/js?id=' + encodeURIComponent(id));
    }

    function loadMetaPixel(id) {
        if (window.fbq) return;
        var n = window.fbq = function () {
            n.callMethod ? n.callMethod.apply(n, arguments) : n.queue.push(arguments);
        };
        if (!window._fbq) window._fbq = n;
        n.push = n;
        n.loaded = true;
        n.version = '2.0';
        n.queue = [];
        addScript('https://connect.facebook.net/en_US/fbevents.js');
        window.fbq('init', id);
        window.fbq('track', 'PageView');
    }

    return {
        getConsent: function () {
            var v = readCookie(COOKIE);
            return v === 'granted' || v === 'denied' ? v : null;
        },

        setConsent: function (value) {
            writeCookie(COOKIE, value);
        },

        focus: function (id) {
            var el = document.getElementById(id);
            if (el) el.focus();
        },

        load: function (cfg) {
            if (loaded || !cfg) return;
            loaded = true;
            if (cfg.tagManagerId && GTM.test(cfg.tagManagerId)) loadTagManager(cfg.tagManagerId);
            else if (cfg.analyticsId && GA.test(cfg.analyticsId)) loadAnalytics(cfg.analyticsId);
            if (cfg.metaPixelId && PIXEL.test(cfg.metaPixelId)) loadMetaPixel(cfg.metaPixelId);
        },

        // Sajten byter sida utan att ladda om, så varje navigering rapporteras som en ny sidvisning.
        // Kort väntan så att sidans titel hunnit uppdateras.
        pageView: function () {
            setTimeout(function () {
                var page = { page_location: location.href, page_path: location.pathname, page_title: document.title };
                if (window.gtag) window.gtag('event', 'page_view', page);
                else if (window.dataLayer) window.dataLayer.push({ event: 'virtual_page_view', page_location: page.page_location, page_path: page.page_path, page_title: page.page_title });
                if (window.fbq) window.fbq('track', 'PageView');
            }, 150);
        },

        // Redan inladdade skript går inte att ta bort, så när samtycket dras tillbaka rensas
        // deras cookies och sidan laddas om utan dem.
        reset: function () {
            var host = location.hostname;
            var parent = host.split('.').slice(-2).join('.');
            document.cookie.split('; ').forEach(function (c) {
                var name = c.split('=')[0];
                if (!/^(_ga|_gid|_gat|_gcl|_fbp|_fbc)/.test(name)) return;
                [host, '.' + host, '.' + parent].forEach(function (domain) {
                    document.cookie = name + '=; Max-Age=0; Path=/; Domain=' + domain;
                });
                document.cookie = name + '=; Max-Age=0; Path=/';
            });
            location.reload();
        }
    };
})();
