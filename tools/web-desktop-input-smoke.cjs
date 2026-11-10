const { chromium } = require('playwright');
const fs = require('node:fs');
const path = require('node:path');

(async () => {
  const browser = await chromium.launch({ executablePath: process.env.CHROME_PATH || '/usr/bin/google-chrome', headless: true, args: ['--no-sandbox', '--use-gl=angle', '--use-angle=swiftshader', '--enable-unsafe-swiftshader'] });
  try {
    const page = await browser.newPage({ viewport: { width: 1024, height: 768 } });
    page.on('pageerror', error => console.error('[browser]', error.message));
    page.on('console', message => { if (message.type() === 'error') console.error('[browser]', message.text()); });
    const base = process.env.EXOLON_WEB_URL || 'http://127.0.0.1:8765/';
    const html = fs.readFileSync(path.join('Exolon.web', 'AppBundle', 'index.html'), 'utf8')
      .replace("'args': [],", "'args': ['--script', 'res://tests/web_touch_fixture.gd'],");
    if (!html.includes('web_touch_fixture.gd')) throw new Error('Unable to prepare desktop input fixture');
    await page.route('**/desktop-input-fixture.html', route => route.fulfill({ status: 200, contentType: 'text/html', body: html }));
    await page.goto(new URL('desktop-input-fixture.html', base).href);
    await page.waitForSelector('#touch-controls.ready', { state: 'attached', timeout: 120000 });
    await page.locator('#canvas').focus();
    await page.keyboard.press('Space', { delay: 150 });
    await page.waitForFunction(() => window.exolonTouchReady && window.exolonTestPlayer, null, { timeout: 30000 });
    const pos = () => page.evaluate(() => window.exolonTestPlayer);
    let initial = await pos();
    await page.keyboard.down('d');
    await page.waitForFunction(x => window.exolonTestPlayer.x > x + 5, initial.x, { timeout: 5000 });
    await page.keyboard.up('d');
    initial = await pos();
    await page.keyboard.down('a');
    await page.waitForFunction(x => window.exolonTestPlayer.x < x - 5, initial.x, { timeout: 5000 });
    await page.keyboard.up('a');
    await page.keyboard.down('s');
    await page.waitForFunction(() => window.exolonTestPlayer.ducking, null, { timeout: 5000 });
    await page.keyboard.up('s');
    await page.waitForFunction(() => !window.exolonTestPlayer.ducking, null, { timeout: 5000 });
    initial = await pos();
    await page.keyboard.down('w');
    await page.waitForFunction(y => window.exolonTestPlayer.y < y - 1, initial.y, { timeout: 5000 });
    await page.keyboard.up('w');
    let ammo = await page.evaluate(() => window.exolonTouchAmmo);
    const canvas = await page.locator('#canvas').boundingBox();
    await page.mouse.move(canvas.x + canvas.width / 2, canvas.y + canvas.height / 2);
    await page.mouse.down();
    await page.waitForFunction(before => window.exolonTouchAmmo === before - 1, ammo, { timeout: 5000 });
    await page.mouse.up();
    await page.waitForTimeout(200);
    ammo = await page.evaluate(() => window.exolonTouchAmmo);
    await page.keyboard.down('Space');
    await page.waitForFunction(before => window.exolonTouchAmmo === before - 1, ammo, { timeout: 5000 });
    await page.keyboard.up('Space');
    console.log('PASS: desktop WASD movement and mouse/Space fire in the browser');
  } finally { await browser.close(); }
})().catch(error => { console.error(error); process.exitCode = 1; });
