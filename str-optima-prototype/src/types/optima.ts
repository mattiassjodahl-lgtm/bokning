/** A single driving-school pupil ("elev"). */
export interface Elev {
	id: string;
	namn: string;
	personnummer: string;
	mobil: string;
	epost: string;
	adress: string;
	postnummer: string;
	ort: string;
	behorighet: string[];
	registrerad: string;
	status: 'Aktiv' | 'Inaktiv' | 'Väntar';
	saldo: number;
	lektionssaldo: number;
	skapad: string;
	senastAndrad: string;
	vardnadshavare?: string;
}

/** A comment left on a pupil's profile. */
export interface Kommentar {
	datum: string;
	forfattare: string;
	text: string;
}

/** A booking in the weekly calendar grid. */
export interface Bokning {
	dag: number; // 0 = Monday .. 6 = Sunday
	startTimme: number; // 24h, e.g. 10 for 10:00
	langdTimmar: number;
	titel: string;
	larare: string;
	bokad: boolean;
}

/** One day's booking rate, shown as a bar in the dashboard chart. */
export interface Bokningsgrad {
	dag: string;
	datum: string;
	procent: number;
	idag?: boolean;
}
