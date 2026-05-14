// ValentynaWeb — базовий JS
// TODO: перенести та очистити з legacy/js/script.js

// Плавний скролл до якорів (на випадок якщо браузер не підтримує CSS scroll-behavior)
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        const target = document.querySelector(this.getAttribute('href'));
        if (target) {
            e.preventDefault();
            target.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
    });
});

// Активний пункт меню при скролі
const navLinks = document.querySelectorAll('.site-nav a[href^="#"]');
const sections = [...navLinks].map(a => document.querySelector(a.getAttribute('href'))).filter(Boolean);

const observer = new IntersectionObserver(entries => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            navLinks.forEach(a => a.classList.remove('active'));
            const active = document.querySelector(`.site-nav a[href="#${entry.target.id}"]`);
            if (active) active.classList.add('active');
        }
    });
}, { threshold: 0.4 });

sections.forEach(s => observer.observe(s));
