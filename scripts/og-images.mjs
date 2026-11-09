// Картинки для прев'ю посилань (Open Graph): 1200×630 JPEG у ValentynaWeb/wwwroot/img/og/<slug>.jpg.
// Telegram, Viber, Facebook, WhatsApp показують їх, коли ділишся посиланням.
// Малює headless Chrome із фото сторінок (WebP) і шрифту сайту — нічого ставити не треба, лише Chrome і Node 22+.
//
//   node scripts/og-images.mjs                         — усі сторінки (головна + service-pages.json)
//   node scripts/og-images.mjs --only farbuvannia      — одна сторінка
//   node scripts/og-images.mjs --style photo|card      — стиль (за замовчуванням card)
//   node scripts/og-images.mjs --out <тека>            — куди писати (за замовчуванням wwwroot/img/og)
//
// Після зміни фото сторінки — перезапустити скрипт і закомітити нові .jpg.
import { spawn } from 'node:child_process';
import { mkdirSync, readFileSync, writeFileSync, mkdtempSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import path from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const web = path.join(root, 'ValentynaWeb');
const arg = name => { const i = process.argv.indexOf(name); return i > 0 ? process.argv[i + 1] : undefined; };
const style = arg('--style') ?? 'card';
const only = arg('--only');
const outDir = path.resolve(arg('--out') ?? path.join(web, 'wwwroot', 'img', 'og'));
const chrome = process.env.CHROME ?? 'C:/Program Files/Google/Chrome/Application/chrome.exe';

// "img/coloring-1" → file:///…/wwwroot/img/webp/coloring-1-1280.webp (як ResponsiveImageTagHelper)
const photo = p => pathToFileURL(path.join(web, 'wwwroot', 'img', 'webp', `${path.basename(p).replace(/ /g, '-')}-1280.webp`)).href;
const font = pathToFileURL(path.join(web, 'wwwroot', 'fonts', 'Chutz-Bold.woff2')).href;

const servicePages = JSON.parse(readFileSync(path.join(web, 'Content', 'service-pages.json'), 'utf8')).pages;
const pages = [
  // головна: фото першого екрана і гасло
  { slug: 'home', image: 'img/main1', title: 'Час бути собою', focus: '50% 40%' },
  ...servicePages.map(p => ({ slug: p.slug, image: p.heroImage, title: p.h1.ua, focus: '50% 25%' }))
].filter(p => !only || p.slug === only);

const esc = s => s.replace(/&/g, '&amp;').replace(/</g, '&lt;');
const html = p => `<!doctype html><html><head><meta charset="utf-8"><style>
@font-face { font-family: Chutz; src: url('${font}') format('woff2'); }
* { margin: 0; box-sizing: border-box; }
html, body { width: 1200px; height: 630px; overflow: hidden; background: #1f1b19; }
.photo { position: absolute; inset: 0; background: url('${photo(p.image)}') ${p.focus} / cover no-repeat; }
${style === 'card' ? `
.photo { left: 560px; }
.text { position: absolute; left: 0; top: 0; bottom: 0; width: 560px; padding: 64px 56px; display: flex; flex-direction: column; justify-content: space-between; color: #eee6dc; }
.logo { font-family: Chutz, sans-serif; font-size: 34px; letter-spacing: 0.04em; line-height: 1; color: #ddcdb3; }
.logo small { display: block; font-family: 'Roboto Mono', Consolas, monospace; font-weight: 300; font-size: 28px; letter-spacing: 0; margin-top: 6px; }
h1 { font-family: Chutz, sans-serif; font-weight: 700; font-size: ${p.title.length > 16 ? 62 : 76}px; line-height: 1.02; text-transform: uppercase; letter-spacing: 0.03em; color: #fff; }
.where { font: 400 24px/1.4 Montserrat, 'Segoe UI', Arial, sans-serif; color: #ddcdb3; }
` : ''}
</style></head><body><div class="photo"></div>${style === 'card' ? `
<div class="text"><div class="logo">ВАЛЕНТИНА<small>перукарня</small></div><h1>${esc(p.title)}</h1><div class="where">вул. Богдана Хмельницького 94, Київ</div></div>` : ''}
</body></html>`;

// ---------- headless Chrome через DevTools Protocol ----------
const sleep = ms => new Promise(r => setTimeout(r, ms));
const port = 9334;
const profile = mkdtempSync(path.join(tmpdir(), 'og-chrome-'));
const proc = spawn(chrome, ['--headless=new', `--remote-debugging-port=${port}`, `--user-data-dir=${profile}`,
  '--no-first-run', '--no-default-browser-check', '--hide-scrollbars', '--allow-file-access-from-files', 'about:blank'], { stdio: 'ignore' });

async function wsUrl() {
  for (let i = 0; i < 75; i++) {
    try {
      const page = (await (await fetch(`http://127.0.0.1:${port}/json/list`)).json()).find(t => t.type === 'page');
      if (page) return page.webSocketDebuggerUrl;
    } catch { }
    await sleep(200);
  }
  throw new Error('Chrome не запустився: ' + chrome);
}

try {
  const ws = new WebSocket(await wsUrl());
  await new Promise(r => (ws.onopen = r));
  let id = 0;
  const pending = new Map();
  ws.onmessage = e => { const m = JSON.parse(e.data); if (m.id && pending.has(m.id)) { pending.get(m.id)(m); pending.delete(m.id); } };
  const send = (method, params = {}) => new Promise((res, rej) => {
    const i = ++id;
    pending.set(i, m => (m.error ? rej(new Error(`${method}: ${m.error.message}`)) : res(m.result)));
    ws.send(JSON.stringify({ id: i, method, params }));
  });

  await send('Page.enable');
  await send('Emulation.setDeviceMetricsOverride', { width: 1200, height: 630, deviceScaleFactor: 1, mobile: false });
  mkdirSync(outDir, { recursive: true });
  const tmpHtml = path.join(profile, 'og.html');

  for (const p of pages) {
    writeFileSync(tmpHtml, html(p));
    await send('Page.navigate', { url: pathToFileURL(tmpHtml).href });
    await sleep(400);
    // чекаємо шрифт і фото, інакше на картинці буде запасний шрифт або порожній фон
    await send('Runtime.evaluate', {
      awaitPromise: true,
      expression: `(async () => { await document.fonts.ready;
        const url = getComputedStyle(document.querySelector('.photo')).backgroundImage.slice(5, -2);
        await new Promise(r => { const i = new Image(); i.onload = i.onerror = r; i.src = url; }); })()`
    });
    await sleep(150);
    const shot = await send('Page.captureScreenshot', { format: 'jpeg', quality: 82 });
    const file = path.join(outDir, `${p.slug}.jpg`);
    writeFileSync(file, Buffer.from(shot.data, 'base64'));
    console.log(`${p.slug.padEnd(12)} ${(Buffer.byteLength(shot.data, 'base64') / 1024).toFixed(0)} KB  ${file}`);
  }
  ws.close();
} finally {
  proc.kill();
  await sleep(300);
  try { rmSync(profile, { recursive: true, force: true }); } catch { }
}
