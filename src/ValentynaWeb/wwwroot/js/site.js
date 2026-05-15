const burgerBtn = document.getElementById('burger-btn');
const closeBtn = document.getElementById('close-btn');
const mobileMenu = document.getElementById('mobile-menu');

burgerBtn.addEventListener('click', () => {
  mobileMenu.classList.add('active');
});

closeBtn.addEventListener('click', () => {
  mobileMenu.classList.remove('active');
});

// Language translations
const translations = {
  ua: {
    // Navigation
    'nav-home': 'Головна',
    'nav-about': 'Про нас',
    'nav-price': 'Прайс',
    'nav-gallery': 'Галерея',
    'nav-contacts': 'Контакти',
    'nav-services': 'Послуги',
    
    // Hero section
    'hero-title': 'Час бути собою',
    'hero-book': 'Записатися',
    'hero-services': 'Перейти до послуг',
    'telegram-btn': 'написати в Telegram',
    
    // Logo
    'logo-main': 'ВАЛЕНТИНА',
    'logo-sub': 'перукарня',
    
    // Services
    'services-title': 'ПОСЛУГИ',
    'services-cuts': 'СТРИЖКИ',
    'services-styling': 'УКЛАДКИ',
    'services-care': 'ДОГЛЯД',
    'services-chemical': 'ХІМ ТА БІО ЗАВИВКА',
    'services-coloring': 'ФАРБУВАННЯ',
    
    // About
    'about-title': 'ПРО НАС',
    'about-text-desktop': 'Я створив місце, де зібрав усе, що люблю і вмію. Хочу зробити найкращу перукарню в Києві, і вкладаю в цей проєкт 101% себе. Це місце, де можна стати щасливішим. За годину (а з фарбуванням і за 4-7 годин) можна змінити не просто зачіску, а настрій людини. У нас класна музика, затишна атмосфера, смачна кава – все, щоб ти почувався комфортно. Люблю динамічні стрижки та експерименти. Мій досвід дозволяє створювати форму, яка природно виглядає та гарно відростає. Волосся – це не просто стрижка, а частина образу, яка потребує уваги. Тому догляд, стайлінг і сушка феном – must-have, щоб розкрити його потенціал.',
    'about-text-mobile': 'Я створив місце, де зібрав усе, що люблю і вмію. Хочу зробити найкращу перукарню в Києві, і вкладаю в цей проєкт 101% себе. Це місце, де можна стати щасливішим. За годину (а з фарбуванням і за 4-7 годин) можна змінити не просто зачіску, а настрій людини. У нас класна музика, затишна атмосфера, смачна кава – все, щоб ти почувався комфортно. Люблю динамічні стрижки та експерименти. Мій досвід дозволяє створювати форму, яка природно виглядає та гарно відростає.',
    'author-name': 'Валентин Галицький',
    'author-role': 'Засновник Перукарні Валентина',
    
    // Cosmetics
    'cosmetics-title': 'Косметика, яку ми використовуємо',
    'cosmetics-description': 'Засоби для догляду за волоссям пропонують індивідуальну систему догляду, створену спеціально для вас. Використовуючи натуральні інгредієнти, науково доведено, що ця преміальна лінія забезпечує найкращий догляд за вашим волоссям. ми надаємо професійні рекомендації щодо догляду за волоссям, допомагаючи вам обрати ідеальні засоби, які відповідають вашому типу та стилю волосся',
    
    // Gallery
    'gallery-title': 'ГАЛЕРЕЯ РОБІТ',
    'gallery-more': 'More photos in Instagram',
    
    // Price
    'price-title': 'ПОСЛУГИ ТА ЦІНИ',
    'price-description': 'Ми Перукарня Валентина. Інклюзивна перукарня, де є місце для кожного. Місцева перукарня, що робить стиль без меж.',
    'price-cuts': 'Стрижки',
    'price-coloring': 'Фарбування',
    'price-care': 'Миття, укладки та догляд',
    
    // Price items
    'short-cut': 'Коротка стрижка',
    'male-cut': 'Чоловіча стрижка',
    'shoulder-cut': 'Стрижка волосся довжиною до плечей (2 довжина)',
    'back-cut': 'Стрижка волосся довжиною до лопаток (3 довжина)',
    'long-cut': 'Стрижка довжиною нижче лопаток (4 довжина)',
    'bangs-cut': 'Стрижка чубчика',
    'machine-cut': 'Стрижка машинкою',
    'beard-cut': 'Стрижка бороди',
    'male-beard': 'Чоловіча стрижка + борода',
    
    'roots': 'Коріння',
    'highlights-roots': 'Висвітлення коренів та тонування',
    'toning': 'Тонування',
    'single-color': 'Однотонне фарбування',
    'black-exit': 'Вихід з чорного/виправлення невдалого фарбування',
    'highlights': 'Пасма (ціна залежить від кількості пасм)',
    'complex-techniques': 'Складні техніки фарбування',
    'consultation': 'Консультація перед фарбуванням або укладкою 3го рівня',
    'free': 'БЕЗКОШТОВНО',
    
    'hair-wash': 'Миття голови',
    'styling-1': 'Укладка 1го рівня',
    'styling-2': 'Укладка 2го рівня',
    'styling-3': 'Укладка 3го рівня',
    'care-k18': 'Догляд К18',
    'care-oil-therapy': 'Догляд LA Biosthetique oil therapy:',
    'length-1': '1 Довжина',
    'length-2': '2 Довжина',
    'length-3': '3 Довжина',
    'length-4': '4 Довжина',
    'from-price': 'Від',
    
    // News
    'news-title': 'ПОДІЇ ТА НОВИТИ У ПЕРУКАРНІ ВАЛЕНТИНА',
    'news-subtitle': 'Завітайте до нас на цікаві події та користуйтесь акційними пропозиціями',
    'news-music': 'МУЗИЧНИЙ ВЕЧІР',
    'news-music-desc': '9 березня о 18:00 у перукарні «Валентина» відбудеться затишний музичний вечір. За інструментом – талановитий Міша Качур. Приходьте насолодитися живою музикою та приємною атмосферою!',
    'news-valentine': 'ДЕНЬ ЗАКОХАНИХ У ВАЛЕНТИНІ',
    'news-valentine-desc': 'Запрошуємо вас 14 лютого на святкування Дня Закоханих у перукарні «Валентина»! На вас чекають смачні мочі, гарячий рамен і музична атмосфера від DJ Emergencyloop. 30% прибутку буде передано на підтримку ЗСУ',
    'news-filter': 'ФІЛЬТР ЗА ДОНАТ',
    'news-filter-desc': 'До 14 лютого у «Валентині» триває благодійна акція «Фільтр за донат» у співпраці з CaféBoutique. Усі зібрані кошти підуть на підтримку FPV-лабораторії. Приєднуйтесь та зробіть свій внесок у важливу справу!',
    'news-details': 'Детальніше»',
    'news-date-1': '8 березня 2025',
    'news-date-2': '14 лютого 2025',
    
    // Reviews
    'reviews-title': 'ВІДГУКИ НАШИХ КЛІЄНТІВ',
    'reviews-subtitle': 'Зазирніть у наше портфоліо вражаючих перевтілень та знайдіть натхнення для свого нового образу',
    'reviews-subtitle-tablet': 'Наші клієнти – наш головний пріоритет! Довіра, якість та стиль – те, що ми створюємо разом. Ознайомтесь із враженнями наших гостей і приєднуйтесь до спільноти задоволених клієнтів',
    'review-1': 'Вперше відвідала цю перукарню – і це було найкраще рішення! Майстер врахував усі мої побажання, підібрав ідеальний відтінок фарбування, а атмосфера просто чудова! Дякую за професіоналізм!',
    'review-2': 'Дуже стильна стрижка! Майстер відразу зрозумів, що мені потрібно, та зробив усе ідеально. Окрема подяка за дружню атмосферу та смачну каву. Однозначно повернусь',
    'review-3': 'Це місце, де справді люблять свою справу! Відчувається увага до деталей – від підбору стрижки до рекомендацій по догляду. Вийшла з салону з відчуттям оновлення! Рекомендую всім!',
    
    // Footer
    'footer-title': 'Перукарня Валентина',
    'footer-text': 'Створюємо стиль, який підкреслює вашу індивідуальність. Наша команда професіоналів допоможе вам знайти ідеальний образ.',
    'footer-links-title': 'Швидкі посилання',
    'footer-schedule-title': 'Графік роботи',
    'footer-schedule-weekdays': 'Пн-Пт 10:00 - 20:00',
    'footer-schedule-weekend': 'Сб-Нд 11:00 - 18:00',
    'footer-social-title': 'Ми в соцмережах',
    'footer-location-title': 'Місцезнаходження',
    'footer-address': 'вул. Богдана Хмельницького 94, Київ'
  },
  
  en: {
    // Navigation
    'nav-home': 'Home',
    'nav-about': 'About',
    'nav-price': 'Prices',
    'nav-gallery': 'Gallery',
    'nav-contacts': 'Contacts',
    'nav-services': 'Services',
    
    // Hero section
    'hero-title': 'Time to be yourself',
    'hero-book': 'Book Now',
    'hero-services': 'View Services',
    'telegram-btn': 'write in Telegram',
    
    // Logo
    'logo-main': 'VALENTYNA',
    'logo-sub': 'hair salon',
    
    // Services
    'services-title': 'SERVICES',
    'services-cuts': 'HAIRCUTS',
    'services-styling': 'STYLING',
    'services-care': 'HAIR CARE',
    'services-chemical': 'CHEMICAL & BIO PERM',
    'services-coloring': 'COLORING',
    
    // About
    'about-title': 'ABOUT US',
    'about-text-desktop': 'I created a place where I gathered everything I love and can do. I want to make the best hair salon in Kyiv, and I put 101% of myself into this project. This is a place where you can become happier. In an hour (and with coloring in 4-7 hours) you can change not just your hairstyle, but a person\'s mood. We have great music, cozy atmosphere, delicious coffee - everything to make you feel comfortable. I love dynamic haircuts and experiments. My experience allows me to create a shape that looks natural and grows beautifully. Hair is not just a haircut, but part of an image that needs attention. That\'s why care, styling and blow-drying are must-haves to reveal its potential.',
    'about-text-mobile': 'I created a place where I gathered everything I love and can do. I want to make the best hair salon in Kyiv, and I put 101% of myself into this project. This is a place where you can become happier. In an hour (and with coloring in 4-7 hours) you can change not just your hairstyle, but a person\'s mood. We have great music, cozy atmosphere, delicious coffee - everything to make you feel comfortable. I love dynamic haircuts and experiments. My experience allows me to create a shape that looks natural and grows beautifully.',
    'author-name': 'Valentyn Halytskyi',
    'author-role': 'Founder of Valentyna Hair Salon',
    
    // Cosmetics
    'cosmetics-title': 'Cosmetics we use',
    'cosmetics-description': 'Hair care products offer an individual care system created specifically for you. Using natural ingredients, it is scientifically proven that this premium line provides the best care for your hair. We provide professional hair care recommendations, helping you choose the perfect products that match your hair type and style.',
    
    // Gallery
    'gallery-title': 'WORK GALLERY',
    'gallery-more': 'More photos in Instagram',
    
    // Price
    'price-title': 'SERVICES & PRICES',
    'price-description': 'We are Valentyna Hair Salon. An inclusive hair salon where there is a place for everyone. A local hair salon that makes style without boundaries.',
    'price-cuts': 'Haircuts',
    'price-coloring': 'Coloring',
    'price-care': 'Washing, Styling & Care',
    
    // Price items
    'short-cut': 'Short haircut',
    'male-cut': 'Men\'s haircut',
    'shoulder-cut': 'Haircut shoulder length (2 length)',
    'back-cut': 'Haircut back length (3 length)',
    'long-cut': 'Haircut below back length (4 length)',
    'bangs-cut': 'Bangs cut',
    'machine-cut': 'Machine haircut',
    'beard-cut': 'Beard trim',
    'male-beard': 'Men\'s haircut + beard',
    
    'roots': 'Roots',
    'highlights-roots': 'Root highlights & toning',
    'toning': 'Toning',
    'single-color': 'Single color dyeing',
    'black-exit': 'Black exit/correction of failed dyeing',
    'highlights': 'Highlights (price depends on number of highlights)',
    'complex-techniques': 'Complex dyeing techniques',
    'consultation': 'Consultation before dyeing or 3rd level styling',
    'free': 'FREE',
    
    'hair-wash': 'Hair wash',
    'styling-1': '1st level styling',
    'styling-2': '2nd level styling',
    'styling-3': '3rd level styling',
    'care-k18': 'K18 care',
    'care-oil-therapy': 'LA Biosthetique oil therapy care:',
    'length-1': '1 Length',
    'length-2': '2 Length',
    'length-3': '3 Length',
    'length-4': '4 Length',
    'from-price': 'From',
    
    // News
    'news-title': 'EVENTS & NEWS AT VALENTYNA HAIR SALON',
    'news-subtitle': 'Visit us for interesting events and take advantage of promotional offers',
    'news-music': 'MUSIC EVENING',
    'news-music-desc': 'On March 9th at 18:00, a cozy music evening will take place at Valentyna Hair Salon. At the instrument - talented Misha Kachur. Come enjoy live music and pleasant atmosphere!',
    'news-valentine': 'VALENTINE\'S DAY AT VALENTYNA',
    'news-valentine-desc': 'We invite you on February 14th to celebrate Valentine\'s Day at Valentyna Hair Salon! Delicious mochi, hot ramen and musical atmosphere from DJ Emergencyloop await you. 30% of profits will be donated to support the Armed Forces of Ukraine',
    'news-filter': 'FILTER FOR DONATION',
    'news-filter-desc': 'Until February 14th, the charity campaign "Filter for donation" continues at Valentyna in cooperation with CaféBoutique. All collected funds will go to support the FPV laboratory. Join and make your contribution to an important cause!',
    'news-details': 'Details»',
    'news-date-1': 'March 8, 2025',
    'news-date-2': 'February 14, 2025',
    
    // Reviews
    'reviews-title': 'OUR CLIENTS\' REVIEWS',
    'reviews-subtitle': 'Take a look at our portfolio of amazing transformations and find inspiration for your new look',
    'reviews-subtitle-tablet': 'Our clients are our top priority! Trust, quality and style - that\'s what we create together. Check out our guests\' impressions and join the community of satisfied clients',
    'review-1': 'I visited this hair salon for the first time - and it was the best decision! The master took into account all my wishes, picked the perfect shade for coloring, and the atmosphere is just wonderful! Thank you for your professionalism!',
    'review-2': 'Very stylish haircut! The master immediately understood what I needed and did everything perfectly. Special thanks for the friendly atmosphere and delicious coffee. I will definitely return',
    'review-3': 'This is a place where they truly love their work! Attention to detail is felt - from choosing a haircut to care recommendations. I left the salon feeling renewed! I recommend to everyone!',
    
    // Footer
    'footer-title': 'Valentyna Hair Salon',
    'footer-text': 'We create style that emphasizes your individuality. Our team of professionals will help you find the perfect look.',
    'footer-links-title': 'Quick Links',
    'footer-schedule-title': 'Working Hours',
    'footer-schedule-weekdays': 'Mon-Fri 10:00 - 20:00',
    'footer-schedule-weekend': 'Sat-Sun 11:00 - 18:00',
    'footer-social-title': 'We are on social media',
    'footer-location-title': 'Location',
    'footer-address': 'Bohdana Khmelnytskoho 94, Kyiv'
  }
};

// Language switching functionality
const langButtons = document.querySelectorAll('.lang-btn');

function switchLanguage(lang) {
  // Update active button
  langButtons.forEach(btn => {
    btn.classList.remove('active');
    if (btn.dataset.lang === lang) {
      btn.classList.add('active');
    }
  });
  
  // Update all translatable elements
  document.querySelectorAll('[data-translate]').forEach(element => {
    const key = element.dataset.translate;
    if (translations[lang] && translations[lang][key]) {
      element.textContent = translations[lang][key];
    }
  });
  
  // Update HTML lang attribute
  document.documentElement.lang = lang;
  
  // Save language preference
  localStorage.setItem('preferred-language', lang);
}

langButtons.forEach(btn => {
  btn.addEventListener('click', () => {
    const lang = btn.dataset.lang;
    switchLanguage(lang);
  });
});

// Initialize language on page load
document.addEventListener('DOMContentLoaded', () => {
  const savedLang = localStorage.getItem('preferred-language') || 'ua';
  switchLanguage(savedLang);
});

// Initialize gallery slider when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
  new GallerySlider();
  new Lightbox();
});

const container = document.querySelector('.news-scroll-container');
const dots = document.querySelectorAll('.dot');

container.addEventListener('scroll', () => {
  const scrollLeft = container.scrollLeft;
  const cardWidth = container.offsetWidth;

  const index = Math.round(scrollLeft / cardWidth);

  dots.forEach(dot => dot.classList.remove('activ'));
  if (dots[index]) {
    dots[index].classList.add('activ');
  }
});

const reviewContainer = document.querySelector('.reviews-cards');
const reviewDots = document.querySelectorAll('.dot-review');

reviewContainer.addEventListener('scroll', () => {
  const scrollLeft = reviewContainer.scrollLeft;
  const cardWidth = reviewContainer.offsetWidth;
  const index = Math.round(scrollLeft / cardWidth);

  reviewDots.forEach(dot => dot.classList.remove('active'));
  if (reviewDots[index]) {
    reviewDots[index].classList.add('active');
  }
});

// Gallery Slider Functionality
class GallerySlider {
  constructor() {
    this.currentSlide = 0;
    this.slides = document.querySelectorAll('.gallery-slide');
    this.indicators = document.querySelectorAll('.gallery-indicator');
    this.prevBtn = document.querySelector('.gallery-prev');
    this.nextBtn = document.querySelector('.gallery-next');
    this.totalSlides = this.slides.length;
    this.autoPlayInterval = null;
    
    this.init();
  }

  init() {
    if (this.slides.length === 0) return;
    
    this.bindEvents();
    this.startAutoPlay();
  }

  bindEvents() {
    this.prevBtn.addEventListener('click', () => this.prevSlide());
    this.nextBtn.addEventListener('click', () => this.nextSlide());
    
    this.indicators.forEach((indicator, index) => {
      indicator.addEventListener('click', () => this.goToSlide(index));
    });

    // Pause auto-play on hover
    const sliderContainer = document.querySelector('.gallery-slider');
    sliderContainer.addEventListener('mouseenter', () => this.stopAutoPlay());
    sliderContainer.addEventListener('mouseleave', () => this.startAutoPlay());

    // Keyboard navigation
    document.addEventListener('keydown', (e) => {
      if (e.key === 'ArrowLeft') {
        this.prevSlide();
      } else if (e.key === 'ArrowRight') {
        this.nextSlide();
      }
    });
  }

  goToSlide(index) {
    if (index < 0 || index >= this.totalSlides) return;
    
    // Remove active class from current slide and indicator
    this.slides[this.currentSlide].classList.remove('active');
    this.indicators[this.currentSlide].classList.remove('active');
    
    // Update current slide
    this.currentSlide = index;
    
    // Add active class to new slide and indicator
    this.slides[this.currentSlide].classList.add('active');
    this.indicators[this.currentSlide].classList.add('active');
  }

  nextSlide() {
    const nextIndex = (this.currentSlide + 1) % this.totalSlides;
    this.goToSlide(nextIndex);
  }

  prevSlide() {
    const prevIndex = this.currentSlide === 0 ? this.totalSlides - 1 : this.currentSlide - 1;
    this.goToSlide(prevIndex);
  }

  startAutoPlay() {
    this.stopAutoPlay(); // Clear any existing interval
    this.autoPlayInterval = setInterval(() => {
      this.nextSlide();
    }, 4000); // Change slide every 4 seconds
  }

  stopAutoPlay() {
    if (this.autoPlayInterval) {
      clearInterval(this.autoPlayInterval);
      this.autoPlayInterval = null;
    }
  }
}

// Lightbox Functionality
class Lightbox {
  constructor() {
    this.modal = document.getElementById('lightbox-modal');
    this.lightboxImage = document.getElementById('lightbox-image');
    this.closeBtn = document.getElementById('lightbox-close');
    this.prevBtn = document.getElementById('lightbox-prev');
    this.nextBtn = document.getElementById('lightbox-next');
    this.galleryImages = document.querySelectorAll('.gallery-desktop-image, .gallery-mobile-image');
    this.currentImageIndex = 0;
    
    // Touch/swipe variables
    this.touchStartX = 0;
    this.touchStartY = 0;
    this.touchEndX = 0;
    this.touchEndY = 0;
    this.minSwipeDistance = 50; // Minimum distance for a swipe to be registered
    
    this.init();
  }

  init() {
    this.bindEvents();
  }

  bindEvents() {
    // Add click events to all gallery images (desktop and mobile)
    this.galleryImages.forEach((image, index) => {
      image.addEventListener('click', () => this.openLightbox(index));
    });

    // Close lightbox events
    this.closeBtn.addEventListener('click', () => this.closeLightbox());
    this.modal.addEventListener('click', (e) => {
      if (e.target === this.modal) {
        this.closeLightbox();
      }
    });

    // Navigation events
    this.prevBtn.addEventListener('click', () => this.prevImage());
    this.nextBtn.addEventListener('click', () => this.nextImage());

    // Touch events for mobile swipe
    this.modal.addEventListener('touchstart', (e) => this.handleTouchStart(e), { passive: true });
    this.modal.addEventListener('touchmove', (e) => this.handleTouchMove(e), { passive: true });
    this.modal.addEventListener('touchend', (e) => this.handleTouchEnd(e), { passive: true });

    // Keyboard events
    document.addEventListener('keydown', (e) => {
      if (!this.modal.classList.contains('active')) return;
      
      switch(e.key) {
        case 'Escape':
          this.closeLightbox();
          break;
        case 'ArrowLeft':
          this.prevImage();
          break;
        case 'ArrowRight':
          this.nextImage();
          break;
      }
    });
  }

  // Touch event handlers for swipe functionality
  handleTouchStart(e) {
    this.touchStartX = e.changedTouches[0].screenX;
    this.touchStartY = e.changedTouches[0].screenY;
  }

  handleTouchMove(e) {
    // Prevent default to avoid scrolling while swiping
    e.preventDefault();
  }

  handleTouchEnd(e) {
    this.touchEndX = e.changedTouches[0].screenX;
    this.touchEndY = e.changedTouches[0].screenY;
    this.handleSwipe();
  }

  handleSwipe() {
    const deltaX = this.touchStartX - this.touchEndX;
    const deltaY = this.touchStartY - this.touchEndY;
    const distance = Math.sqrt(deltaX * deltaX + deltaY * deltaY);

    // Only process swipe if distance is greater than minimum threshold
    if (distance > this.minSwipeDistance) {
      // Determine if swipe is more horizontal than vertical
      if (Math.abs(deltaX) > Math.abs(deltaY)) {
        // Horizontal swipe
        if (deltaX > 0) {
          // Swipe left - go to next image
          this.nextImage();
        } else {
          // Swipe right - go to previous image
          this.prevImage();
        }
      }
      // Vertical swipes could be used for closing lightbox if needed
      // else if (Math.abs(deltaY) > Math.abs(deltaX)) {
      //   if (deltaY > 0) {
      //     // Swipe up - could close lightbox
      //     this.closeLightbox();
      //   }
      // }
    }
  }

  openLightbox(index) {
    this.currentImageIndex = index;
    this.updateImage();
    this.modal.classList.add('active');
    document.body.style.overflow = 'hidden'; // Prevent background scrolling
  }

  closeLightbox() {
    this.modal.classList.remove('active');
    document.body.style.overflow = ''; // Restore scrolling
  }

  updateImage() {
    const currentImage = this.galleryImages[this.currentImageIndex];
    this.lightboxImage.src = currentImage.src;
    this.lightboxImage.alt = currentImage.alt;
  }

  prevImage() {
    this.currentImageIndex = this.currentImageIndex === 0 
      ? this.galleryImages.length - 1 
      : this.currentImageIndex - 1;
    this.updateImage();
  }

  nextImage() {
    this.currentImageIndex = (this.currentImageIndex + 1) % this.galleryImages.length;
    this.updateImage();
  }
}
