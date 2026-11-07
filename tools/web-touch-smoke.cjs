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
    await page.locator('#touch-fire').tap();
    await page.waitForTimeout(400);
    if ((await canvas.screenshot()).equals(title)) throw new Error('Touching the fire zone did not start the game');
    const cdp = await page.context().newCDPSession(page);
    const point = async keys => {
      const box = await page.locator(`[data-keys="${keys}"]`).boundingBox();
      return { x: Math.round(box.x + box.width / 2), y: Math.round(box.y + box.height / 2) };
    };
    const send = (type, touchPoints) => cdp.send('Input.dispatchTouchEvent', { type, touchPoints });
    const held = () => page.evaluate(() => [...keyCounts.entries()].filter(([, count]) => count > 0).map(([key]) => key).sort());
    const assertHeld = async expected => {
      const actual = await held();
      if (JSON.stringify(actual) !== JSON.stringify([...expected].sort())) throw new Error(`Expected ${expected}; got ${actual}`);
    };
    const gameRect = await canvas.boundingBox();
    const ammoClip = { x: gameRect.x, y: gameRect.y + gameRect.height * .94, width: gameRect.width * .1, height: gameRect.height * .06 };
    const ammoBefore = await page.screenshot({ clip: ammoClip });
    const move = await point('ArrowRight');
    const shoot = await point('Space');
    await send('touchStart', [{ ...move, id: 1 }]);
    await send('touchStart', [{ ...move, id: 1 }, { ...shoot, id: 2 }]);
    await page.waitForTimeout(500);
    if ((await page.screenshot({ clip: ammoClip })).equals(ammoBefore)) throw new Error('Fire zone did not consume ammunition');
    await send('touchEnd', [{ ...move, id: 1 }]);
    await send('touchEnd', []);
    await page.waitForTimeout(120);
    const zones = ['ArrowUp ArrowLeft', 'ArrowUp', 'ArrowUp ArrowRight', 'ArrowLeft', 'ArrowRight', 'ArrowDown ArrowLeft', 'ArrowDown', 'ArrowDown ArrowRight'];
    for (const [width, height] of [[844, 390], [390, 844]]) {
      await page.setViewportSize({ width, height });
      const bounds = await page.evaluate(() => {
        const rect = element => { const r = element.getBoundingClientRect(); return { x: r.x, y: r.y, width: r.width, height: r.height, right: r.right, bottom: r.bottom }; };
        return { canvas: rect(document.querySelector('#canvas')), directions: rect(document.querySelector('#touch-directions')), fire: rect(document.querySelector('#touch-fire')), zones: [...document.querySelectorAll('.touch-zone')].map(element => ({ background: getComputedStyle(element).backgroundColor, border: getComputedStyle(element).boxShadow, label: getComputedStyle(element, '::after').content })) };
      });
      if (bounds.zones.some(zone => zone.background === 'rgba(0, 0, 0, 0)' || zone.border === 'none' || zone.label === 'none' || zone.label === '""')) throw new Error('Touch zone outline or label is missing');
      if (width > height) {
        if (bounds.fire.right > bounds.canvas.x + 1 || bounds.directions.x < bounds.canvas.right - 1 || Math.abs(bounds.directions.width / width - .3085) > .01) throw new Error('Landscape layout differs from the diagram');
      } else if (bounds.directions.y < bounds.canvas.bottom - 1 || bounds.fire.y < bounds.canvas.bottom - 1 || bounds.fire.right > bounds.directions.x + 1 || Math.abs(bounds.directions.width / width - .707) > .01 || Math.abs(bounds.canvas.y + bounds.canvas.height / 2 - height / 2) > 1) throw new Error('Portrait layout differs from the diagram');
      for (const keys of zones) {
        await send('touchStart', [{ ...await point(keys), id: 1 }]);
        await assertHeld(keys.split(' '));
        await send('touchEnd', []);
        await assertHeld([]);
        await page.waitForTimeout(120);
      }
      const upLeft = await point('ArrowUp ArrowLeft');
      const upRight = await point('ArrowUp ArrowRight');
      const fire = await point('Space');
      await send('touchStart', [{ ...upLeft, id: 1 }]);
      await send('touchStart', [{ ...upLeft, id: 1 }, { ...fire, id: 2 }]);
      await assertHeld(['ArrowUp', 'ArrowLeft', 'Space']);
      await send('touchMove', [{ ...upRight, id: 1 }, { ...fire, id: 2 }]);
      await assertHeld(['ArrowUp', 'ArrowRight', 'Space']);
      await send('touchEnd', []);
      await assertHeld([]);
      fs.mkdirSync('artifacts', { recursive: true });
      await page.screenshot({ path: `artifacts/web-touch-${width > height ? 'landscape' : 'portrait'}.png` });
      await page.waitForTimeout(120);
    }
    if (errors.length) throw new Error(errors.join('\n'));
    console.log('PASS: visible control zones, all directions, diagonal sliding, simultaneous fire and both orientations');
  } finally { await browser.close(); }
})().catch(error => { console.error(error); process.exitCode = 1; });
