/**
 * Calendar column resize – drag the right edge of a day-column header.
 * Directly updates grid-template-columns on both header and body containers
 * so CSS grid layout is never disrupted. Calls back to Blazor on mouse-up.
 */
(function () {
    const MIN_WIDTH = 240; // 3 × 80 px

    // Per-rootEl stored widths so they survive Blazor re-renders
    const _storedWidths = new WeakMap();

    window.initCalendarResize = function (rootEl, dotnetRef) {
        if (!rootEl || rootEl._resizeInit) return;
        rootEl._resizeInit = true;
        rootEl._dotnetRef  = dotnetRef;
        if (!_storedWidths.has(rootEl)) _storedWidths.set(rootEl, new Map());
        rootEl.addEventListener('mousedown', onMouseDown);

        // Sync horizontal scroll: body → header.
        // The header scrollbar is hidden; we drive it from the body.
        const body   = rootEl.querySelector('.cal-body');
        const header = rootEl.querySelector('.cal-days-header');
        if (body && header) {
            body.addEventListener('scroll', () => {
                header.scrollLeft = body.scrollLeft;
            }, { passive: true });
        }
    };

    let _drag = null;

    function onMouseDown(e) {
        const handle = e.target.closest('.cal-resize-handle');
        if (!handle) return;

        e.preventDefault();
        e.stopPropagation();

        const headerCell = handle.closest('[data-day-index]');
        if (!headerCell) return;

        const root      = headerCell.closest('.cal-root');
        const dayIndex  = parseInt(headerCell.dataset.dayIndex, 10);
        const totalCols = root.querySelectorAll('.cal-days-header [data-day-index]').length;

        _drag = {
            root,
            dayIndex,
            totalCols,
            startX:    e.clientX,
            startW:    headerCell.getBoundingClientRect().width,
            dotnetRef: root._dotnetRef,
            widths:    _storedWidths.get(root),
        };

        document.addEventListener('mousemove', onMouseMove);
        document.addEventListener('mouseup',   onMouseUp);
        document.body.style.cursor     = 'col-resize';
        document.body.style.userSelect = 'none';
    }

    function onMouseMove(e) {
        if (!_drag) return;
        const newW = Math.max(MIN_WIDTH, _drag.startW + e.clientX - _drag.startX);
        _drag.widths.set(_drag.dayIndex, newW);
        applyGridTemplate(_drag.root, _drag.widths, _drag.totalCols);
    }

    function onMouseUp(e) {
        if (!_drag) return;

        const finalW = Math.max(MIN_WIDTH, _drag.startW + e.clientX - _drag.startX);
        _drag.widths.set(_drag.dayIndex, finalW);
        applyGridTemplate(_drag.root, _drag.widths, _drag.totalCols);

        document.removeEventListener('mousemove', onMouseMove);
        document.removeEventListener('mouseup',   onMouseUp);
        document.body.style.cursor     = '';
        document.body.style.userSelect = '';

        if (_drag.dotnetRef) {
            _drag.dotnetRef.invokeMethodAsync('OnColumnResized', _drag.dayIndex, finalW);
        }
        _drag = null;
    }

    /**
     * Build "240px 1fr 360px 1fr …" and apply to both grid containers.
     * Unsized columns use 1fr so they fill remaining space without a fixed minimum,
     * which prevents the grid from overflowing the viewport when days are few.
     */
    function applyGridTemplate(root, widths, totalCols) {
        const parts = [];
        for (let i = 0; i < totalCols; i++) {
            parts.push(widths.has(i) ? `${widths.get(i)}px` : `1fr`);
        }
        const template = parts.join(' ');

        const header = root.querySelector('.cal-days-header');
        const body   = root.querySelector('.cal-days-body');

        if (header) {
            header.style.display              = 'grid';
            header.style.gridTemplateColumns  = template;
        }
        if (body) {
            body.style.display             = 'grid';
            body.style.gridTemplateColumns = template;
        }
    }
})();
