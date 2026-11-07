const { chromium } = require('playwright');
const fs = require('node:fs');

(async () => {
  const browser = await chromium.launch({
    executablePath: process.env.CHROME_PATH || '/usr/bin/google-chrome',
    headless: true,
    args: ['--no-sandbox', '--use-gl=angle', '--use-angle=swiftshader', '--enable-unsafe-swiftshader'],
  });
  const page = await browser.newPage({ viewport: { width: 1024, height: 768 } });
  const errors = [];
  let started = false;
  let titleScene = false;
  let releaseWasm;
  const wasmGate = new Promise(resolve => { releaseWasm = resolve; });
  await page.route('**/godot.wasm', async route => { await wasmGate; await route.continue(); });
  page.on('pageerror', error => errors.push(error.message));
  page.on('console', message => {
    const text = message.text();
    if (text.includes('2dog is running in the browser!')) started = true;
    if (text.includes('Scene Root: TitleScreen')) titleScene = true;
    if (message.type() === 'error') errors.push(text);
    console.log(`[browser:${message.type()}] ${text}`);
  });
  try {
    await page.goto(process.env.EXOLON_WEB_URL || 'http://127.0.0.1:8765/', { waitUntil: 'domcontentloaded' });
    await page.waitForFunction(() => {
      const image = document.querySelector('#status-splash');
      return image && image.complete && image.naturalWidth === 256 && image.naturalHeight === 64 && image.getAttribute('src') === 'splash.png';
    }, null, { timeout: 30000 });
    if (!await page.locator('#status-splash').isVisible()) throw new Error('Exolon loading image is not visible');
    fs.mkdirSync('artifacts', { recursive: true });
    await page.screenshot({ path: 'artifacts/web-loading.png' });
    releaseWasm();
    const deadline = Date.now() + 120000;
    while ((!started || !titleScene) && Date.now() < deadline && errors.length === 0) await page.waitForTimeout(500);
    if (!started || !titleScene) throw new Error('The C# game did not reach the title screen');
    await page.waitForTimeout(500);
    const canvas = page.locator('#canvas');
    await canvas.click({ position: { x: 10, y: 10 } });
    const title = await canvas.screenshot();
    fs.writeFileSync('artifacts/web-title.png', title);
    await page.evaluate(() => window.dispatchEvent(new Event('beforeinstallprompt', { cancelable: true })));
    if (!await page.locator('#install-app').isVisible()) throw new Error('Install prompt is missing on title screen');
    await page.keyboard.press('Space', { delay: 150 });
    await page.waitForTimeout(1000);
    if (await page.locator('#install-app').isVisible()) throw new Error('Install prompt did not disappear when starting');
    const game = await canvas.screenshot();
    if (game.equals(title)) throw new Error('Pressing Space did not leave the title screen');
    await page.keyboard.press('ArrowRight', { delay: 300 });
    await page.waitForTimeout(500);
    if (errors.length) throw new Error(errors.join('\n'));
    console.log('PASS: Exolon loading image, title screen and start in Chromium');
  } finally {
    releaseWasm();
    fs.mkdirSync('artifacts', { recursive: true });
    await page.screenshot({ path: 'artifacts/web-smoke.png' });
    await browser.close();
  }
})().catch(error => { console.error(error); process.exitCode = 1; });
