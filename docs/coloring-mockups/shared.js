// Спільні секції для трьох макетів сторінки «Фарбування».
// Ціни — з Content/services.json, адреса й години — з Content/salon.json.
// Секції вставляються на місце <div data-s="назва"></div>.

const IMG = '../../ValentynaWeb/wwwroot/img/webp/';
const pic = (name, w = 768) => `${IMG}${name}-${w}.webp`;
const TG = 'https://t.me/valentyna_perukarnia';
const TEL = '+380637250589';
const TEL_SHOWN = '+380 63 725 05 89';
const MAPS = 'https://maps.app.goo.gl/BBhDLBSZe6BwSAXX9';
const INSTA = 'https://www.instagram.com/valentyna_perukarnia/';

const ICON = {
  tg: '<svg viewBox="0 0 18 18" aria-hidden="true"><path fill="currentColor" fill-rule="evenodd" clip-rule="evenodd" d="M17.9821 2.22666C18.0212 1.9646 17.996 1.69575 17.9091 1.44812C17.8222 1.2005 17.6769 0.98316 17.4882 0.818751C17.2996 0.654343 17.0745 0.548881 16.8364 0.513355C16.5983 0.477828 16.356 0.513535 16.1347 0.616758L0.821211 7.74547C-0.246253 8.2425 -0.30088 9.94943 0.821211 10.4643C1.93245 10.9762 3.05973 11.4445 4.20091 11.8682C5.24689 12.2504 6.38778 12.6039 7.30211 12.7039C7.55196 13.0346 7.86629 13.3534 8.18689 13.6445C8.67674 14.09 9.26599 14.5484 9.87495 14.988C11.0947 15.8692 12.4478 16.7118 13.3594 17.2603C14.4493 17.9138 15.7541 17.1613 15.9493 15.8474L17.9821 2.22666ZM2.53793 9.09497L16.0818 2.79003L14.1851 15.5029C13.2905 14.9653 12.0018 14.1603 10.8538 13.3306C10.3246 12.955 9.81378 12.5488 9.32331 12.1138C9.19223 11.9954 9.06498 11.8719 8.94181 11.7435L12.489 7.8227C12.657 7.63705 12.7515 7.3852 12.7516 7.12255C12.7516 6.85991 12.6573 6.60799 12.4894 6.4222C12.3215 6.23642 12.0937 6.132 11.8562 6.1319C11.6186 6.13181 11.3907 6.23606 11.2227 6.42171L7.33883 10.7158C6.68151 10.6227 5.76629 10.3534 4.76151 9.98705C4.01349 9.7115 3.27233 9.4137 2.53882 9.09398L2.53793 9.09497Z"/></svg>',
  phone: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72c.13.96.36 1.9.7 2.81a2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45c.91.34 1.85.57 2.81.7A2 2 0 0 1 22 16.92z"/></svg>'
};

// [назва, від, до, примітка]; відсортовано за ціною «від»
const PRICES = [
  ['Тонування', 1500, 4000],
  ['Пасма', 1500, 2500, 'ціна залежить від кількості пасм'],
  ['Коріння', 2000, 2500],
  ['Однотонне фарбування', 2500, 6000],
  ['Висвітлення коренів і тонування', 3000, 4500],
  ['Вихід з чорного або виправлення невдалого фарбування', 4500, 12000],
  ['Складні техніки фарбування', 4500, 13000]
];
const PRICE_MAX = 13000;

const num = n => String(n).replace(/\B(?=(\d{3})+(?!\d))/g, ' ');
const range = (a, b) => `${num(a)}–${num(b)} грн`;

const WORKS = ['coloring-3', 'coloring-12', 'coloring-5', 'coloring-2', 'coloring-13', 'coloring-7', 'coloring-16', 'coloring-14'];

const PRICE_NOTE = 'Ціна залежить від довжини й густоти волосся, його стану і рангу майстра. Точну суму майстер назве після фото або на консультації.';

const S = {
  header: () => `
    <header class="topbar"><div class="wrap topbar-in">
      <a class="logo" href="#" aria-label="Перукарня Валентина, на головну">ВАЛЕНТИНА<span>перукарня</span></a>
      <nav class="topnav" aria-label="Основне меню">
        <a class="topnav-link" href="#">Усі послуги</a>
        <span class="lang"><a href="#" aria-current="true">UA</a><a href="#">EN</a></span>
        <a class="topphone" href="tel:${TEL}" aria-label="Подзвонити">${ICON.phone}<span>${TEL_SHOWN}</span></a>
        <a class="btn btn-main btn-sm topcta" href="${TG}" target="_blank" rel="noopener">${ICON.tg}Надіслати фото</a>
      </nav>
    </div></header>`,

  works: d => `
    <section class="block ${d.tone || ''}" id="works"><div class="wrap">
      <h2 class="sec-title">${d.title || 'Роботи'}</h2>
      <div class="works-grid">
        ${WORKS.slice(0, +(d.count || 8)).map(n => `<img src="${pic(n, 480)}" alt="Фарбування в Перукарні Валентина" loading="lazy">`).join('')}
      </div>
      <a class="more" href="${INSTA}" target="_blank" rel="noopener">Більше робіт в Instagram</a>
    </div></section>`,

  prices: () => `
    <section class="block block-sand" id="prices"><div class="wrap">
      <h2 class="sec-title">Ціни</h2>
      <ul class="price-list">
        ${PRICES.map(([name, a, b, note]) => `<li><span>${name}${note ? `<small>${note}</small>` : ''}</span><span class="p">${range(a, b)}</span></li>`).join('')}
      </ul>
      <p class="price-note">${PRICE_NOTE}</p>
    </div></section>`,

  scale: () => `
    <ul class="scale" aria-label="Ціни на фарбування">
      <li class="scale-axis" aria-hidden="true">${[0, 5000, 10000].map(v => `<span style="left:${v / PRICE_MAX * 100}%">${v ? num(v) : 0}${v === 10000 ? ' грн' : ''}</span>`).join('')}</li>
      ${PRICES.map(([name, a, b, note]) => `
        <li>
          <div class="scale-head"><span>${name}${note ? ` <span class="muted">(${note})</span>` : ''}</span><b>${range(a, b)}</b></div>
          <div class="scale-track" aria-hidden="true"><span class="scale-fill" style="left:${a / PRICE_MAX * 100}%;width:${(b - a) / PRICE_MAX * 100}%"></span></div>
        </li>`).join('')}
    </ul>`,

  scaleSection: () => `
    <section class="block block-sand" id="prices"><div class="wrap">
      <h2 class="sec-title">Ціни</h2>
      <div class="scale-box scale-box-wide">
        ${S.scale()}
        <p class="price-note">${PRICE_NOTE}</p>
      </div>
    </div></section>`,

  steps: d => `
    <section class="block ${d.tone || ''}" id="how"><div class="wrap">
      <h2 class="sec-title">Як це працює</h2>
      <ol class="steps">
        <li><span class="n" aria-hidden="true">1</span><h3>Надішліть фото волосся в Telegram</h3><p>Краще при денному світлі, спереду і ззаду. Напишіть, який колір хочете.</p></li>
        <li><span class="n" aria-hidden="true">2</span><h3>Майстер підкаже техніку й ціну</h3><p>Відповідаємо щодня з 10:00 до 21:00.</p></li>
        <li><span class="n" aria-hidden="true">3</span><h3>Консультація або одразу фарбування</h3><p>Можна спершу прийти на консультацію в салон, а можна одразу записатися на фарбування.</p></li>
      </ol>
    </div></section>`,

  masters: () => `
    <section class="block" id="masters"><div class="wrap">
      <h2 class="sec-title">Майстри з фарбування</h2>
      <p class="need">Потрібно від Валика: фото, ім'я, ранг, стаж, навчання й нагороди кожного майстра</p>
      <div class="masters">
        ${[1, 2, 3].map(() => `
          <article class="master">
            <div class="master-photo">Фото майстра</div>
            <h3>Ім'я майстра</h3>
            <p>Ранг, стаж, на чому спеціалізується</p>
          </article>`).join('')}
      </div>
    </div></section>`,

  reviews: () => `
    <section class="block block-ink" id="reviews"><div class="wrap">
      <h2 class="sec-title">Відгуки про фарбування</h2>
      <p class="need">Потрібно: 2–3 цитати з Google-відгуків, де клієнти пишуть саме про фарбування</p>
      <div class="quotes">
        <blockquote class="quote"><p>Тут буде цитата з Google-відгуку про фарбування, одне-два речення</p><footer>Ім'я автора з Google</footer></blockquote>
        <blockquote class="quote"><p>Друга цитата: краще та, де згадано конкретну техніку або майстра</p><footer>Ім'я автора з Google</footer></blockquote>
      </div>
      <a class="rating-link" href="${MAPS}" target="_blank" rel="noopener"><b>5,0</b> Усі відгуки в Google</a>
    </div></section>`,

  faq: () => `
    <section class="block" id="faq"><div class="wrap">
      <h2 class="sec-title">Питання</h2>
      <div class="faq">
        <details open><summary>Скільки коштує консультація?</summary><p><span class="todo">Валик: безкоштовна чи платна, скільки триває</span></p></details>
        <details><summary>Чи можна вийти з чорного в блонд?</summary><p>Так, це окрема послуга. Скільки сеансів знадобиться, майстер скаже після фото: це залежить від того, як довго і чим фарбували волосся.</p></details>
        <details><summary>Скільки триває фарбування?</summary><p><span class="todo">Валик: орієнтовно для коріння, однотонного і складних технік</span></p></details>
        <details><summary>Якими фарбами працюєте?</summary><p><span class="todo">Валик: бренди фарб і догляду</span></p></details>
      </div>
    </div></section>`,

  contacts: () => `
    <section class="block block-sand" id="contacts"><div class="wrap final">
      <div>
        <h2 class="sec-title">Не знаєте, що обрати?</h2>
        <p class="lead">Надішліть фото, і майстер підкаже, що підійде саме вам.</p>
        <div class="btn-row">
          <a class="btn btn-main" href="${TG}" target="_blank" rel="noopener">${ICON.tg}Надіслати фото в Telegram</a>
          <a class="btn btn-line" href="tel:${TEL}">${ICON.phone}Подзвонити</a>
        </div>
      </div>
      <address class="where">
        <b>Перукарня Валентина</b>
        <span>вул. Богдана Хмельницького 94, Київ</span>
        <span>Щодня з 10:00 до 21:00</span>
        <a href="${MAPS}" target="_blank" rel="noopener">Прокласти маршрут</a>
        <a href="#">Усі послуги перукарні</a>
      </address>
    </div></section>`,

  sticky: () => `
    <div class="sticky">
      <a class="btn btn-main" href="${TG}" target="_blank" rel="noopener" aria-label="Надіслати фото в Telegram">${ICON.tg}Надіслати фото</a>
      <a class="btn btn-icon" href="tel:${TEL}" aria-label="Подзвонити">${ICON.phone}</a>
    </div>`
};

document.querySelectorAll('[data-s]').forEach(el => {
  el.outerHTML = S[el.dataset.s](el.dataset);
});

// Іконки для розмітки в самих макетах: <i data-icon="tg"></i>
document.querySelectorAll('[data-icon]').forEach(el => {
  el.outerHTML = ICON[el.dataset.icon];
});
