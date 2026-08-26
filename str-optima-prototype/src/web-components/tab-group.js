// @ts-check

/**
 * Accessible tabs following the WAI-ARIA Tabs pattern. Expects children:
 * a `[role="tablist"]` containing `[role="tab"]` buttons (each with a
 * `data-tab` id), and sibling `[role="tabpanel"]` elements with matching
 * `data-tab` ids.
 * @extends HTMLElement
 */
export default class TabGroup extends HTMLElement {
	static tag = 'tab-group';
	static {
		customElements.define(TabGroup.tag, TabGroup);
	}

	/** @type {ElementInternals} */
	#internals;
	/** @type {AbortController} */
	#controller = new AbortController();
	/** @type {HTMLButtonElement[]} */
	#tabs = [];

	constructor() {
		super();
		this.#internals = this.attachInternals();
	}

	connectedCallback() {
		if (document.readyState !== 'loading') {
			this.#init();
			return;
		}
		document.addEventListener('DOMContentLoaded', () => this.#init());
	}

	disconnectedCallback() {
		this.#controller?.abort();
	}

	#init() {
		this.#controller = new AbortController();
		const { signal } = this.#controller;

		this.#tabs = Array.from(this.querySelectorAll('[role="tab"]'));

		this.#tabs.forEach((tab, index) => {
			tab.addEventListener('click', () => this.#activate(tab), { signal });
			tab.addEventListener(
				'keydown',
				(event) => {
					if (event.key === 'ArrowRight') this.#activate(this.#tabs[(index + 1) % this.#tabs.length]);
					if (event.key === 'ArrowLeft') this.#activate(this.#tabs[(index - 1 + this.#tabs.length) % this.#tabs.length]);
				},
				{ signal },
			);
		});

		this.#internals.states.add('--ready');
	}

	/** @param {HTMLButtonElement} tab */
	#activate(tab) {
		const target = tab.dataset.tab;
		this.#tabs.forEach((t) => {
			const selected = t === tab;
			t.setAttribute('aria-selected', String(selected));
			t.tabIndex = selected ? 0 : -1;
		});
		this.querySelectorAll('[role="tabpanel"]').forEach((panel) => {
			if (panel instanceof HTMLElement) panel.hidden = panel.dataset.tab !== target;
		});
		tab.focus();
	}
}
