const { chromium } = require('playwright');
const fs = require('node:fs');

(async () => {
  const browser = await chromium.launch({ executablePath: process.env.CHROME_PATH || '/usr/bin/google-chrome', headless: true, args: ['--no-sandbox', '--use-gl=angle', '--use-angle=swiftshader', '--enable-unsafe-swiftshader'] });
  try {
    const page = await browser.newPage({ viewport: { width: 844, height: 390 }, isMobile: true, hasTouch: true, deviceScaleFactor: 1 });
    const errors = [];
    page.on('pageerror', error => errors.push(error.message));
    await page.goto(process.env.EXOLON_WEB_URL || 'http://127.0.0.1:8765/');
    await page.waitForSelector('#touch-controls.ready', { timeout: 120000 });
    const canvas = page.locator('#canvas');
    const title = await canvas.screenshot();
    await page.locator('[data-key="Space"]').tap();
    await page.waitForTimeout(1500);
    const game = await canvas.screenshot();
    if (game.equals(title)) throw new Error('Touching FIRE did not start the game');
    fs.mkdirSync('artifacts', { recursive: true });
    fs.writeFileSync('artifacts/web-touch-before.png', game);
    const landscapeCanvas = await canvas.boundingBox();
    const leftButton = await page.locator('[data-key="ArrowLeft"]').boundingBox();
    const rightButton = await page.locator('[data-key="ArrowRight"]').boundingBox();
    const fireButton = await page.locator('[data-key="Space"]').boundingBox();
    if (leftButton.x < 0 || rightButton.x + rightButton.width > landscapeCanvas.x || fireButton.x < landscapeCanvas.x + landscapeCanvas.width) throw new Error('Landscape controls overlap the game');
    const ammoClip = { x: landscapeCanvas.x, y: landscapeCanvas.y + landscapeCanvas.height * .94, width: landscapeCanvas.width * .1, height: landscapeCanvas.height * .06 };
    const ammoBefore = await page.screenshot({ clip: ammoClip });
    const cdp = await page.context().newCDPSession(page);
    const center = async key => {
      const box = await page.locator(`[data-key="${key}"]`).boundingBox();
      return { x: Math.round(box.x + box.width / 2), y: Math.round(box.y + box.height / 2) };
    };
    const right = await center('ArrowRight');
    const fire = await center('Space');
    await cdp.send('Input.dispatchTouchEvent', { type: 'touchStart', touchPoints: [{ ...right, id: 1 }] });
    await cdp.send('Input.dispatchTouchEvent', { type: 'touchStart', touchPoints: [{ ...right, id: 1 }, { ...fire, id: 2 }] });
    await page.waitForTimeout(500);
    fs.writeFileSync('artifacts/web-touch-after.png', await canvas.screenshot());
    if ((await page.screenshot({ clip: ammoClip })).equals(ammoBefore)) throw new Error('Touching FIRE did not consume ammunition');
    if (await page.locator('.touch-button.pressed').count() !== 2) throw new Error('Simultaneous movement and fire were not held');
    await page.screenshot({ path: 'artifacts/web-touch-landscape.png' });
    await cdp.send('Input.dispatchTouchEvent', { type: 'touchEnd', touchPoints: [{ ...right, id: 1 }] });
    await cdp.send('Input.dispatchTouchEvent', { type: 'touchEnd', touchPoints: [] });
    if (await page.locator('.touch-button.pressed').count() !== 0) throw new Error('Touch buttons remained pressed');
    await page.setViewportSize({ width: 390, height: 844 });
    const portraitCanvas = await canvas.boundingBox();
    for (const button of await page.locator('.touch-button').all()) {
      const box = await button.boundingBox();
      if (!box || box.width < 44 || box.height < 44 || box.x < 0 || box.x + box.width > 390 || box.y < portraitCanvas.y + portraitCanvas.height) throw new Error('Portrait touch button is too small, off screen or overlaps the game');
    }
    await page.screenshot({ path: 'artifacts/web-touch-portrait.png' });
    if (errors.length) throw new Error(errors.join('\n'));
    console.log('PASS: touch starts game, simultaneous controls release, landscape and portrait layouts');
  } finally { await browser.close(); }
})().catch(error => { console.error(error); process.exitCode = 1; });
