const { chromium } = require('playwright');
const fs = require('node:fs');
const path = require('node:path');

(async () => {
  const browser = await chromium.launch({
    executablePath: process.env.CHROME_PATH || '/usr/bin/google-chrome', headless: true,
    args: ['--no-sandbox', '--use-gl=angle', '--use-angle=swiftshader', '--enable-unsafe-swiftshader', '--autoplay-policy=no-user-gesture-required'],
  });
  try {
    const page = await browser.newPage({ viewport: { width: 1024, height: 768 } });
    page.on('pageerror', error => console.error('[browser]', error.message));
    page.on('console', message => console.log('[browser]', message.text()));
    const base = process.env.EXOLON_WEB_URL || 'http://127.0.0.1:8765/';
    const html = fs.readFileSync(path.join('Exolon.web', 'AppBundle', 'index.html'), 'utf8')
      .replace("'args': [],", "'args': ['--script', 'res://tests/web_autosave_fixture.gd'],");
    if (!html.includes('web_autosave_fixture.gd')) throw new Error('Unable to prepare web checkpoint fixture');
    await page.route('**/autosave-fixture.html', route => route.fulfill({ status: 200, contentType: 'text/html', body: html }));
    await page.goto(new URL('autosave-fixture.html', base).href);
    await page.waitForFunction(() => window.exolonCheckpointReady === true, null, { timeout: 120000 });
    console.log('Checkpoint created; waiting for browser storage');
    // Godot's IDBFS synchronization is asynchronous.
    await page.waitForFunction(async () => {
      const databases = await indexedDB.databases();
      if (!databases.some(db => db.name === '/userfs')) return false;
      return new Promise(resolve => {
        const request = indexedDB.open('/userfs');
        request.onerror = () => resolve(false);
        request.onsuccess = () => {
          const db = request.result;
          const transaction = db.transaction('FILE_DATA', 'readonly');
          const keys = transaction.objectStore('FILE_DATA').getAllKeys();
          keys.onsuccess = () => { resolve(keys.result.some(key => String(key).endsWith('/autosave.cfg'))); db.close(); };
          keys.onerror = () => { resolve(false); db.close(); };
        };
      });
    }, null, { timeout: 30000, polling: 250 });
    console.log('Checkpoint stored; reopening the game');
    await page.goto(base);
    await page.waitForSelector('#touch-controls.ready', { state: 'attached', timeout: 120000 });
    await page.keyboard.press('ArrowDown', { delay: 150 });
    await page.keyboard.press('Space', { delay: 150 });
    await page.waitForTimeout(1200);
    const continued = await page.locator('#canvas').screenshot();
    await page.goto(base);
    await page.waitForSelector('#touch-controls.ready', { state: 'attached', timeout: 120000 });
    await page.keyboard.press('Space', { delay: 150 });
    await page.waitForTimeout(1200);
    const newGame = await page.locator('#canvas').screenshot();
    const readHud = async screenshot => page.evaluate(async data => {
      const image = new Image();
      image.src = 'data:image/png;base64,' + data;
      await image.decode();
      const canvas = document.createElement('canvas');
      canvas.width = image.width;
      canvas.height = image.height;
      const ctx = canvas.getContext('2d', { willReadFrequently: true });
      ctx.drawImage(image, 0, 0);
      const hud = ctx.getImageData(0, Math.floor(image.height * 0.9), image.width, Math.floor(image.height * 0.1)).data;
      let cyan = 0;
      for (let i = 0; i < hud.length; i += 4)
        if (hud[i] < 50 && hud[i + 1] > 100 && hud[i + 2] > 100) cyan++;
      const zone = ctx.getImageData(Math.floor(image.width * 0.84), Math.floor(image.height * 0.95),
        Math.floor(image.width * 0.15), Math.floor(image.height * 0.045)).data;
      return { cyan, zone: Array.from(zone) };
    }, screenshot.toString('base64'));
    const savedHud = await readHud(continued);
    const freshHud = await readHud(newGame);
    if (savedHud.cyan < 10 || freshHud.cyan < 10) throw new Error('Continue or new game did not open gameplay');
    if (JSON.stringify(savedHud.zone) === JSON.stringify(freshHud.zone)) throw new Error('Continue did not restore a different zone from a new game');
    console.log('PASS: web checkpoint persists across reload and Continue opens the saved zone');
  } finally {
    await browser.close();
  }
})().catch(error => { console.error(error); process.exitCode = 1; });
