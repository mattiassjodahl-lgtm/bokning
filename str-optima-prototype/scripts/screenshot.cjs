const { chromium } = require('playwright');

const pages = [
	['/', 'dashboard'],
	['/kalender', 'kalender'],
	['/elever', 'elever'],
	['/elever/johanna-johansson', 'student-detalj'],
];

(async () => {
	const browser = await chromium.launch({ executablePath: '/opt/pw-browsers/chromium' });
	const page = await browser.newPage({ viewport: { width: 1440, height: 900 } });
	for (const [path, name] of pages) {
		await page.goto(`http://localhost:4321${path}`, { waitUntil: 'networkidle' });
		await page.screenshot({ path: `/home/claude/str-optima-prototype/scripts/out-${name}.png`, fullPage: true });
		console.log('captured', name);
	}
	await browser.close();
})();
