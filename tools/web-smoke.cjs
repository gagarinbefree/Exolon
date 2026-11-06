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
  page.on('pageerror', error => errors.push(error.message));
  page.on('console', message => {
    const text = message.text();
    if (text.includes('2dog is running in the browser!')) started = true;
    if (message.type() === 'error') errors.push(text);
    console.log(`[browser:${message.type()}] ${text}`);
  });
  try {
    await page.goto('http://127.0.0.1:8765/', { waitUntil: 'domcontentloaded' });
    const deadline = Date.now() + 120000;
    while (!started && Date.now() < deadline && errors.length === 0) await page.waitForTimeout(500);
    if (!started) throw new Error('The C# game did not reach its startup message');
    await page.keyboard.press('ArrowRight', { delay: 300 });
    await page.waitForTimeout(500);
    if (errors.length) throw new Error(errors.join('\n'));
    console.log('PASS: Godot C# game started in Chromium');
  } finally {
    fs.mkdirSync('artifacts', { recursive: true });
    await page.screenshot({ path: 'artifacts/web-smoke.png' });
    await browser.close();
  }
})().catch(error => { console.error(error); process.exitCode = 1; });
