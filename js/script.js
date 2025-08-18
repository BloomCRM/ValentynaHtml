const burgerBtn = document.getElementById('burger-btn');
const closeBtn = document.getElementById('close-btn');
const mobileMenu = document.getElementById('mobile-menu');

burgerBtn.addEventListener('click', () => {
  mobileMenu.classList.add('active');
});

closeBtn.addEventListener('click', () => {
  mobileMenu.classList.remove('active');
});

const langButtons = document.querySelectorAll('.lang-btn');

langButtons.forEach(btn => {
  btn.addEventListener('click', () => {
    langButtons.forEach(b => b.classList.remove('active'));
    btn.classList.add('active');
  });
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

// Initialize gallery slider when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
  new GallerySlider();
  new Lightbox();
});

// Lightbox Functionality
class Lightbox {
  constructor() {
    this.modal = document.getElementById('lightbox-modal');
    this.lightboxImage = document.getElementById('lightbox-image');
    this.closeBtn = document.getElementById('lightbox-close');
    this.prevBtn = document.getElementById('lightbox-prev');
    this.nextBtn = document.getElementById('lightbox-next');
    this.galleryImages = document.querySelectorAll('.gallery-desktop-image');
    this.currentImageIndex = 0;
    
    this.init();
  }

  init() {
    this.bindEvents();
  }

  bindEvents() {
    // Add click events to all gallery images
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
