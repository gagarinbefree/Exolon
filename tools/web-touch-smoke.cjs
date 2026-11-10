const { chromium } = require('playwright');
const fs = require('node:fs');
const path = require('node:path');

(async () => {
  const browser = await chromium.launch({ executablePath: process.env.CHROME_PATH || '/usr/bin/google-chrome', headless: true, args: ['--no-sandbox', '--use-gl=angle', '--use-angle=swiftshader', '--enable-unsafe-swiftshader'] });
  try {
    const page = await browser.newPage({ viewport: { width: 844, height: 390 }, isMobile: true, hasTouch: true, deviceScaleFactor: 1 });
    const errors = [];
    page.on('pageerror', error => errors.push(error.message));
    const base = process.env.EXOLON_WEB_URL || 'http://127.0.0.1:8765/';
    const html = fs.readFileSync(path.join('Exolon.web', 'AppBundle', 'index.html'), 'utf8')
      .replace("'args': [],", "'args': ['--script', 'res://tests/web_touch_fixture.gd'],");
    if (!html.includes('web_touch_fixture.gd')) throw new Error('Unable to prepare web touch fixture');
    await page.route('**/touch-fixture.html', route => route.fulfill({ status: 200, contentType: 'text/html', body: html }));
    await page.goto(new URL('touch-fixture.html', base).href);
    await page.waitForSelector('#touch-controls.ready', { timeout: 120000 });
    const canvas = page.locator('#canvas');
    await page.evaluate(() => window.dispatchEvent(new Event('beforeinstallprompt', { cancelable: true })));
    if (!await page.locator('#install-app').isVisible()) throw new Error('Install prompt was not shown on the title screen');
    const title = await canvas.screenshot();
    await page.locator('#touch-fire').tap();
    await page.waitForFunction(() => window.exolonTouchReady === true && Number.isInteger(window.exolonTouchAmmo), null, { timeout: 30000 });
    if ((await canvas.screenshot()).equals(title)) throw new Error('Touching the fire zone did not start the game');
    if (await page.locator('#install-app').isVisible()) throw new Error('Install prompt remained visible after starting');
    await page.evaluate(() => window.dispatchEvent(new Event('beforeinstallprompt', { cancelable: true })));
    if (await page.locator('#install-app').isVisible()) throw new Error('Install prompt reappeared during gameplay');
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
    const ammoBefore = await page.evaluate(() => window.exolonTouchAmmo);
    const move = await point('ArrowRight');
    const shoot = await point('Space');
    await send('touchStart', [{ ...move, id: 1 }]);
    await send('touchStart', [{ ...move, id: 1 }, { ...shoot, id: 2 }]);
    await assertHeld(['ArrowRight', 'Space']);
    try {
      await page.waitForFunction(before => window.exolonTouchAmmo === before - 1, ammoBefore, { timeout: 5000 });
    } catch {
      fs.mkdirSync('artifacts', { recursive: true });
      await page.screenshot({ path: 'artifacts/web-touch-failure.png' });
      const ammoAfter = await page.evaluate(() => window.exolonTouchAmmo);
      throw new Error(`Fire zone did not consume one round: ${ammoBefore} -> ${ammoAfter}`);
    }
    await send('touchEnd', [{ ...move, id: 1 }]);
    await send('touchEnd', []);
    await page.waitForTimeout(120);
    const zones = ['ArrowUp ArrowLeft', 'ArrowUp', 'ArrowUp ArrowRight', 'ArrowLeft', 'ArrowRight', 'ArrowDown'];
    for (const [width, height] of [[844, 390], [1024, 768], [390, 844]]) {
      await page.setViewportSize({ width, height });
      const bounds = await page.evaluate(() => {
        const rect = element => { const r = element.getBoundingClientRect(); return { x: r.x, y: r.y, width: r.width, height: r.height, right: r.right, bottom: r.bottom }; };
        return { canvas: rect(document.querySelector('#canvas')), directions: rect(document.querySelector('#touch-directions')), fire: rect(document.querySelector('#touch-fire')), separator: getComputedStyle(document.querySelector('#touch-directions')).boxShadow, zones: [...document.querySelectorAll('.touch-zone')].map(element => ({ background: getComputedStyle(element).backgroundColor, icon: !!element.querySelector('svg'), label: element.getAttribute('aria-label') })) };
      });
      if (bounds.zones.some(zone => zone.background === 'rgba(0, 0, 0, 0)' || !zone.icon || !zone.label)) throw new Error('Touch zone background, icon or accessible label is missing');
      if (width > height) {
        const tablet = width / height <= 1.6;
        const gameMin = Math.min(256, Math.max(0, (width - 40) / 3));
        const controlMin = Math.min(tablet ? 400 : 160, Math.max(0, width - 40 - gameMin));
        const available = Math.max(0, width - controlMin - 40);
        const gameWidth = Math.min(height * 4 / 3, available);
        const controlsWidth = width - gameWidth - 40;
        const expectedFire = controlsWidth * 0.3;
        const expectedDirections = controlsWidth * 0.7;
        if (Math.abs(bounds.canvas.height - gameWidth * 3 / 4) > 1 || Math.abs(bounds.canvas.width - gameWidth) > 1 || Math.abs(bounds.fire.width - expectedFire) > 1 || Math.abs(bounds.directions.width - expectedDirections) > 1 || bounds.separator !== 'none') throw new Error('Landscape game or control size is incorrect');
        if (bounds.fire.right > bounds.canvas.x - 19 || bounds.directions.x < bounds.canvas.right + 19) throw new Error('Landscape game margin is missing');
        if (bounds.directions.width <= bounds.fire.width * 2) throw new Error('Direction area should be wider than the fire area');
      } else if (bounds.directions.y < bounds.canvas.bottom + 19 || bounds.fire.y < bounds.canvas.bottom + 19 || bounds.fire.right > bounds.directions.x + 1 || Math.abs(bounds.directions.width / width - .707) > .01 || Math.abs(bounds.canvas.y + bounds.canvas.height / 2 - height / 2) > 1 || bounds.separator === 'none') throw new Error('Portrait layout or game margin differs from the diagram');
      for (const keys of zones) {
        await send('touchStart', [{ ...await point(keys), id: 1 }]);
        await assertHeld(keys.split(' '));
        await send('touchEnd', []);
        await assertHeld([]);
        await page.waitForTimeout(120);
      }
      for (const keys of ['ArrowLeft', 'ArrowRight']) {
        const box = await page.locator(`[data-keys="${keys}"]`).boundingBox();
        await send('touchStart', [{ x: Math.round(box.x + box.width / 2), y: Math.round(box.y + box.height * .9), id: 1 }]);
        await assertHeld([keys]);
        await send('touchEnd', []);
        await page.waitForTimeout(120);
      }
      const up = await page.locator('[data-keys="ArrowUp"]').boundingBox();
      const left = await page.locator('[data-keys="ArrowLeft"]').boundingBox();
      const right = await page.locator('[data-keys="ArrowRight"]').boundingBox();
      if (up.width >= left.width || up.width >= right.width || await page.locator('[data-keys^="ArrowDown "]').count()) throw new Error('Direction zones do not match the new diagram');
      if (width === 1024 && height === 768 && up.width < 70) throw new Error('Tablet direction buttons are too narrow');
      const diagonal = await page.locator('[data-keys="ArrowUp ArrowLeft"]').boundingBox();
      if (Math.abs(diagonal.height / bounds.directions.height - .4) > .01) throw new Error('Upper diagonal zones should occupy 40% of the height');
      const borders = await page.evaluate(() => [...document.querySelectorAll('.touch-zone')].map(element => {
        const r = element.getBoundingClientRect(), style = getComputedStyle(element), outer = document.querySelector('#touch-directions').getBoundingClientRect();
        return { fire: element.id === 'touch-fire', left: r.left === outer.left && style.borderLeftWidth !== '0px', right: r.right === outer.right && style.borderRightWidth !== '0px', top: r.top === outer.top && style.borderTopWidth !== '0px', bottom: r.bottom === outer.bottom && style.borderBottomWidth !== '0px' };
      }));
      if (borders.some(border => border.left || border.right || border.top || border.bottom)) throw new Error('Outer control border is visible');
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
      const orientation = width > height ? (width === 844 ? 'landscape' : 'landscape-tablet') : 'portrait';
      await page.screenshot({ path: `artifacts/web-touch-${orientation}.png` });
      await page.waitForTimeout(120);
    }
    if (errors.length) throw new Error(errors.join('\n'));
    console.log('PASS: visible control zones, all directions, diagonal sliding, simultaneous fire and both orientations');
  } finally { await browser.close(); }
})().catch(error => { console.error(error); process.exitCode = 1; });
