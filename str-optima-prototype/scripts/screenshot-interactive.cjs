const { chromium } = require('playwright');

(async () => {
	const browser = await chromium.launch({ executablePath: '/opt/pw-browsers/chromium' });
	const page = await browser.newPage({ viewport: { width: 1440, height: 900 } });

	await page.goto('http://localhost:4321/elever', { waitUntil: 'networkidle' });
	await page.click('[data-opens="lagg-till-elev-dialog"]');
	await page.waitForTimeout(200);
	await page.screenshot({ path: '/home/claude/str-optima-prototype/scripts/out-dialog-open.png' });

	await page.goto('http://localhost:4321/elever/johanna-johansson', { waitUntil: 'networkidle' });
	await page.click('[data-tab="utbildning"]');
	await page.waitForTimeout(200);
	await page.screenshot({ path: '/home/claude/str-optima-prototype/scripts/out-tab-switched.png' });

	const consoleErrors = [];
	page.on('console', (msg) => {
		if (msg.type() === 'error') consoleErrors.push(msg.text());
	});
	await page.reload({ waitUntil: 'networkidle' });
	console.log('console errors:', JSON.stringify(consoleErrors));

	await browser.close();
})();
