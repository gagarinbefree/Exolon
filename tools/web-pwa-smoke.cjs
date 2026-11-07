const { chromium } = require('playwright');
const fs = require('node:fs');
const os = require('node:os');
const path = require('node:path');

(async () => {
  const profile = fs.mkdtempSync(path.join(os.tmpdir(), 'exolon-pwa-'));
  const context = await chromium.launchPersistentContext(profile, { executablePath: process.env.CHROME_PATH || '/usr/bin/google-chrome', headless: true, viewport: { width: 390, height: 844 }, isMobile: true, hasTouch: true, args: ['--no-sandbox', '--use-gl=angle', '--use-angle=swiftshader', '--enable-unsafe-swiftshader'] });
  try {
    const page = context.pages()[0] || await context.newPage();
    const cdp = await context.newCDPSession(page);
    await page.goto(process.env.EXOLON_WEB_URL || 'http://127.0.0.1:8765/');
    let result;
    for (let attempt = 0; attempt < 30; attempt++) {
      result = await cdp.send('Page.getAppManifest');
      if (result.data || result.errors.length) break;
      await page.waitForTimeout(100);
    }
    if (!result.data || result.errors.length) throw new Error('Invalid manifest: ' + JSON.stringify(result.errors));
    const manifest = JSON.parse(result.data);
    if (manifest.name !== 'Exolon' || manifest.start_url !== './' || manifest.scope !== './' || manifest.display !== 'standalone') throw new Error('Wrong PWA identity, scope or display mode');
    for (const icon of manifest.icons) {
      const response = await page.request.get(new URL(icon.src, result.url).href);
      if (!response.ok() || !response.headers()['content-type'].startsWith('image/png')) throw new Error('PWA icon missing: ' + icon.src);
    }
    const apple = await page.locator('link[rel="apple-touch-icon"]').getAttribute('href');
    if (!apple || !(await page.request.get(new URL(apple, page.url()).href)).ok()) throw new Error('Apple touch icon missing');
    const installability = await cdp.send('Page.getInstallabilityErrors');
    if (installability.installabilityErrors.length) throw new Error('Not installable: ' + JSON.stringify(installability.installabilityErrors));
    console.log('PASS: valid installable PWA manifest, app scope and Android/iOS icons');
  } finally {
    await context.close();
    if (path.dirname(profile) === path.resolve(os.tmpdir()) && path.basename(profile).startsWith('exolon-pwa-')) fs.rmSync(profile, { recursive: true, force: true });
  }
})().catch(error => { console.error(error); process.exitCode = 1; });
