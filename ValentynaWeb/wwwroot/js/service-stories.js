// Фото послуг у форматі сторіз: клік по картці в секції «Послуги» відкриває її фото.
// Телефон: тап зліва — назад, по центру — пауза/відтворення, справа — далі, утримання — пауза.
// Після останнього фото послуги переходить до наступної. Дані — з #service-stories-data.
(() => {
  const dialog = document.getElementById('service-stories');
  const dataEl = document.getElementById('service-stories-data');
  if (!dialog || !dataEl || typeof dialog.showModal !== 'function') return;

  const categories = JSON.parse(dataEl.textContent);
  const DURATION = 4000;
  const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  const $ = id => document.getElementById(id);

  const photoEl = $('ss-photo');
  const barsEl = $('ss-bars');
  const playBtn = $('ss-play');
  const flash = $('ss-flash');
  const nextCatBtn = $('ss-next-cat');

  // stopped — пауза кнопкою або тапом по центру; holding — поки тримають палець/мишку
  let cat = 0, photo = 0, elapsed = 0, last = 0, stopped = false, holding = false, frame = 0, downAt = 0;

  const lang = () => (document.documentElement.lang === 'en' ? 'en' : 'ua');
  const t = (key, fallback) => (typeof translations !== 'undefined' && translations[lang()]?.[key]) || fallback;
  const titleOf = c => (lang() === 'en' ? t(c.titleKey, c.title) : c.title);

  const srcset = base => `${base}-768.webp 768w, ${base}-1280.webp 1280w, ${base}-1920.webp 1920w`;

  const setStopped = value => {
    stopped = value;
    playBtn.setAttribute('aria-pressed', String(value));
    playBtn.setAttribute('aria-label', value ? 'Відтворити' : 'Пауза');
  };

  const render = () => {
    const c = categories[cat];
    const base = c.photos[photo];
    photoEl.srcset = srcset(base);
    photoEl.sizes = '(min-width: 900px) 70vh, 100vw';
    photoEl.src = `${base}-1280.webp`;
    photoEl.alt = titleOf(c);
    $('ss-avatar').src = `${c.cover}-480.webp`;
    $('ss-title').textContent = titleOf(c);
    $('ss-bg').style.backgroundImage = `url("${base}-480.webp")`;

    const nextCat = categories[cat + 1];
    nextCatBtn.hidden = !nextCat;
    if (nextCat) nextCatBtn.textContent = `${t('stories-next', 'Далі')}: ${titleOf(nextCat)} ›`;

    barsEl.innerHTML = c.photos.map(() => '<div class="ss-bar"><i></i></div>').join('');
    [...barsEl.children].forEach((b, k) => { b.firstChild.style.width = k < photo ? '100%' : '0'; });
    elapsed = 0;

    // підвантажуємо наступне фото заздалегідь, щоб не блимало
    const upcoming = c.photos[photo + 1] || categories[cat + 1]?.photos[0];
    if (upcoming) new Image().src = `${upcoming}-1280.webp`;
  };

  const go = (c, p) => {
    if (c >= categories.length) { dialog.close(); return; }
    cat = Math.max(0, c);
    photo = Math.max(0, p);
    render();
  };

  const next = () => (photo + 1 < categories[cat].photos.length ? go(cat, photo + 1) : go(cat + 1, 0));
  const prev = () => (photo > 0 ? go(cat, photo - 1)
    : cat > 0 ? go(cat - 1, categories[cat - 1].photos.length - 1) : go(0, 0));

  const toggle = () => {
    setStopped(!stopped);
    // значок нового стану: ⏸ — поставили на паузу, ▶ — запустили
    flash.innerHTML = playBtn.querySelector(stopped ? '.ss-icon-pause' : '.ss-icon-play').outerHTML;
    flash.classList.remove('is-shown');
    void flash.offsetWidth;
    flash.classList.add('is-shown');
  };

  const tick = now => {
    if (!dialog.open) return;
    if (!stopped && !holding && !reduceMotion) elapsed += now - last;
    last = now;
    const bar = barsEl.children[photo];
    if (bar) bar.firstChild.style.width = `${Math.min(100, (elapsed / DURATION) * 100)}%`;
    if (elapsed >= DURATION) next();
    frame = requestAnimationFrame(tick);
  };

  const open = index => {
    setStopped(false);
    dialog.showModal();
    go(index, 0);
    last = performance.now();
    cancelAnimationFrame(frame);
    frame = requestAnimationFrame(tick);
    if (typeof gtag === 'function') gtag('event', 'service_stories_open', { service: categories[index].title });
  };

  document.querySelectorAll('[data-story-index]').forEach(card => {
    card.addEventListener('click', () => open(Number(card.dataset.storyIndex)));
  });

  // довге утримання — це пауза, а не тап: після нього фото не перемикаємо.
  // detail === 0 — клік із клавіатури, утримання там не буває
  const tapOnly = fn => e => { if (e.detail === 0 || performance.now() - downAt < 250) fn(); };
  $('ss-tap-prev').addEventListener('click', tapOnly(prev));
  $('ss-tap-toggle').addEventListener('click', tapOnly(toggle));
  $('ss-tap-next').addEventListener('click', tapOnly(next));
  dialog.querySelectorAll('.ss-tap').forEach(zone => {
    zone.addEventListener('pointerdown', () => { holding = true; downAt = performance.now(); });
  });
  ['pointerup', 'pointercancel', 'pointerleave'].forEach(e => dialog.addEventListener(e, () => { holding = false; }));

  $('ss-side-prev').addEventListener('click', prev);
  $('ss-side-next').addEventListener('click', next);
  playBtn.addEventListener('click', () => setStopped(!stopped));
  nextCatBtn.addEventListener('click', () => go(cat + 1, 0));
  $('ss-close').addEventListener('click', () => dialog.close());
  // клік по темному фону навколо кадру (десктоп) — закрити
  dialog.addEventListener('click', e => { if (e.target === dialog || e.target.id === 'ss-bg') dialog.close(); });
  dialog.addEventListener('close', () => cancelAnimationFrame(frame));

  // Safari на iPhone ігнорує частину touch-action: глушимо зум двома пальцями і подвійним тапом у сторіз
  dialog.addEventListener('gesturestart', e => e.preventDefault());
  dialog.addEventListener('dblclick', e => e.preventDefault());
  let lastTouchEnd = 0;
  dialog.addEventListener('touchend', e => {
    const now = Date.now();
    if (now - lastTouchEnd < 350) {
      e.preventDefault();
      // preventDefault гасить і click, тому другий швидкий тап обробляємо самі
      const zone = e.target.closest('.ss-tap, .ss-side, .ss-play, .ss-next-cat, .ss-close, .ss-book');
      if (zone) zone.click();
    }
    lastTouchEnd = now;
  }, { passive: false });
  dialog.addEventListener('keydown', e => {
    if (e.key === 'ArrowRight') next();
    if (e.key === 'ArrowLeft') prev();
    if (e.key === ' ') { e.preventDefault(); toggle(); }
  });
})();
