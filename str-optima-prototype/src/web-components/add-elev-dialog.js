// @ts-check

/**
 * Wraps a native <dialog> as a controllable modal for the "Lägg till elev"
 * form. Opens when an element with `data-opens="<this element's id>"` is
 * clicked, and closes via its own close button, the backdrop, or Escape
 * (native <dialog> behaviour).
 * @extends HTMLElement
 */
export default class AddElevDialog extends HTMLElement {
	static tag = 'add-elev-dialog';
	static {
		customElements.define(AddElevDialog.tag, AddElevDialog);
	}

	/** @type {ElementInternals} */
	#internals;
	/** @type {AbortController} */
	#controller = new AbortController();
	/** @type {HTMLDialogElement | null} */
	#dialog = null;

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

		this.#dialog = this.querySelector('dialog');
		if (!this.#dialog) return;

		const trigger = this.id ? document.querySelector(`[data-opens="${this.id}"]`) : null;
		trigger?.addEventListener(
			'click',
			(event) => {
				event.preventDefault();
				this.#dialog?.showModal();
			},
			{ signal },
		);

		this.querySelectorAll('[data-close]').forEach((el) =>
			el.addEventListener('click', () => this.#dialog?.close(), { signal }),
		);

		this.#dialog.addEventListener(
			'submit',
			(event) => {
				const form = /** @type {HTMLFormElement} */ (event.target);
				if (form?.tagName === 'FORM') {
					event.preventDefault();
					this.#dialog?.close();
				}
			},
			{ signal },
		);

		this.#internals.states.add('--ready');
	}
}
