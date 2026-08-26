# STR – Optima prototyp

En kod-prototyp av koncepten från Figma-filen "STR - Design Mockups" (Optima –
körskole-administration), byggd enligt vår `s5-frontend`-standard: **Astro +
TypeScript + Tailwind**, med web components för enkel interaktivitet.

## Vad är med

- **Instrumentpanel** (`/`) – KPI-kort, bokningsgrad-diagram, dagens lektioner
- **Kalender** (`/kalender`) – veckovy med bokningar, filter, lärarval
- **Elever** (`/elever`) – sökbar elevtabell + "Lägg till elev"-dialog (native `<dialog>`)
- **Elevprofil** (`/elever/[id]`) – flikar (Översikt / Utbildning / Elevkontra), personuppgifter, status per behörighet, kommentarer

All data i `src/utils/mockData.ts` är fejkad/exempel-data för prototypsyfte –
byt ut mot riktiga API-anrop när det är dags att koppla på backend.

## Kom igång

```bash
npm install
npm run dev
```

Öppna http://localhost:4321

## Struktur

Följer standardstrukturen i `s5-frontend`-skillen: `src/layouts`,
`src/components/ui`, `src/web-components`, `src/pages` (fil-baserad routing),
`src/utils`, `src/types`.

## Designkälla

Figma: "STR - Design Mockups" (koncept-ramarna för Dashboard, Kalender,
Elever/Student och "Lägg till elev"). Färger och mått är avlästa visuellt
(ingen inloggning/Dev Mode-åtkomst fanns tillgänglig) – stäm av mot Figma-filen
med Dev Mode om pixel-exakta värden behövs.
