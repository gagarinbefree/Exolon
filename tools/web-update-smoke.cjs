const { chromium } = require('playwright');

(async () => {
  const browser = await chromium.launch({ executablePath: process.env.CHROME_PATH || '/usr/bin/google-chrome', headless: true,
    args: ['--no-sandbox', '--use-gl=angle', '--use-angle=swiftshader', '--enable-unsafe-swiftshader'] });
  try {
    const page = await browser.newPage({ viewport: { width: 844, height: 390 }, isMobile: true, hasTouch: true });
    let versionRequests = 0;
    await page.route('**/version.json?*', async route => {
      versionRequests++;
      await route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify({ version: '999.0.0' }) });
    });
    await page.goto(process.env.EXOLON_WEB_URL || 'http://127.0.0.1:8765/');
    await page.waitForSelector('#touch-controls.ready', { timeout: 120000 });
    await page.locator('[data-keys="ArrowDown"]').tap();
    await page.waitForTimeout(200);
    await page.locator('#touch-fire').tap();
    await page.waitForTimeout(1000);
    if (versionRequests !== 1) throw new Error('Update check did not request the published version');
    const navigation = page.waitForEvent('framenavigated', { timeout: 10000 });
    await page.locator('#touch-fire').tap();
    await navigation;
    console.log('PASS: an available web update is detected and confirmed with a reload');
  } finally {
    await browser.close();
  }
})().catch(error => { console.error(error); process.exitCode = 1; });
