// Modern Mood Playlist Generator JavaScript Interactions

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    initializeAnimations();
    initializeInteractions();
    initializeFormEnhancements();
});

// Animation Initialization
function initializeAnimations() {
    // Intersection Observer for scroll animations
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add(entry.target.dataset.animation || 'fade-in');
            }
        });
    }, {
        threshold: 0.1
    });

    // Observe elements with animation classes
    document.querySelectorAll('[data-animation]').forEach(el => observer.observe(el));
    
    // Add stagger delay to grid items
    const gridItems = document.querySelectorAll('.row .col-lg-3, .row .col-lg-4, .row .col-lg-6, .row .col-xl-4');
    gridItems.forEach((item, index) => {
        item.style.animationDelay = `${index * 0.1}s`;
    });
}

// Interactive Elements
function initializeInteractions() {
    // Enhanced card hover effects
    const cards = document.querySelectorAll('.card');
    cards.forEach(card => {
        card.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-8px) scale(1.02)';
        });
        
        card.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0) scale(1)';
        });
    });

    // Mood badge interactions
    const moodBadges = document.querySelectorAll('.mood-badge');
    moodBadges.forEach(badge => {
        badge.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-2px) scale(1.1)';
            this.style.boxShadow = '0 8px 25px rgba(0, 0, 0, 0.2)';
        });
        
        badge.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0) scale(1)';
            this.style.boxShadow = '0 4px 15px rgba(0, 0, 0, 0.1)';
        });
    });

    // Button ripple effect
    const buttons = document.querySelectorAll('.btn');
    buttons.forEach(button => {
        button.addEventListener('click', createRipple);
    });
}

// Form Enhancements
function initializeFormEnhancements() {
    // Auto-focus first input
    const firstInput = document.querySelector('form input:not([type="hidden"]):first-of-type');
    if (firstInput) {
        setTimeout(() => firstInput.focus(), 300);
    }

    // Enhanced form validation feedback
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function(e) {
            const submitBtn = form.querySelector('button[type="submit"]');
            if (submitBtn && form.checkValidity()) {
                submitBtn.innerHTML = '<span class="loading-spinner me-2"></span>' + submitBtn.textContent;
                submitBtn.disabled = true;
            }
        });
    });

    // Real-time validation for specific inputs
    const urlInputs = document.querySelectorAll('input[type="url"]');
    urlInputs.forEach(input => {
        input.addEventListener('blur', validateYouTubeUrl);
    });
}

// Utility Functions
function createRipple(event) {
    const button = event.currentTarget;
    const circle = document.createElement('span');
    const diameter = Math.max(button.clientWidth, button.clientHeight);
    const radius = diameter / 2;

    circle.style.width = circle.style.height = `${diameter}px`;
    circle.style.left = `${event.clientX - button.offsetLeft - radius}px`;
    circle.style.top = `${event.clientY - button.offsetTop - radius}px`;
    circle.classList.add('ripple');

    const ripple = button.getElementsByClassName('ripple')[0];
    if (ripple) {
        ripple.remove();
    }

    button.appendChild(circle);
}

function validateYouTubeUrl(event) {
    const input = event.target;
    const url = input.value;
    const isValid = isValidYouTubeUrl(url);
    
    if (url && !isValid) {
        input.classList.add('is-invalid');
        showValidationMessage(input, 'Please enter a valid YouTube URL');
    } else {
        input.classList.remove('is-invalid');
        hideValidationMessage(input);
    }
}

function isValidYouTubeUrl(url) {
    const youtubeRegex = /^(https?:\/\/)?(www\.)?(youtube\.com|youtu\.be)\/.+/;
    return youtubeRegex.test(url);
}

function showValidationMessage(input, message) {
    let feedback = input.parentNode.querySelector('.invalid-feedback');
    if (!feedback) {
        feedback = document.createElement('div');
        feedback.className = 'invalid-feedback';
        input.parentNode.appendChild(feedback);
    }
    feedback.textContent = message;
}

function hideValidationMessage(input) {
    const feedback = input.parentNode.querySelector('.invalid-feedback');
    if (feedback) {
        feedback.remove();
    }
}

// Smooth scroll for anchor links
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        e.preventDefault();
        const target = document.querySelector(this.getAttribute('href'));
        if (target) {
            target.scrollIntoView({
                behavior: 'smooth',
                block: 'start'
            });
        }
    });
});

// Add dynamic search functionality
function initializeDynamicSearch() {
    const searchInput = document.getElementById('search');
    const moodFilter = document.getElementById('moodId');
    
    if (searchInput && moodFilter) {
        let searchTimeout;
        
        searchInput.addEventListener('input', function() {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(() => {
                filterSongs();
            }, 300);
        });
        
        moodFilter.addEventListener('change', filterSongs);
    }
}

function filterSongs() {
    const searchTerm = document.getElementById('search')?.value.toLowerCase() || '';
    const selectedMood = document.getElementById('moodId')?.value || '';
    const songCards = document.querySelectorAll('[data-song-card]');
    
    songCards.forEach(card => {
        const title = card.dataset.title?.toLowerCase() || '';
        const artist = card.dataset.artist?.toLowerCase() || '';
        const moods = card.dataset.moods?.split(',') || [];
        
        const matchesSearch = !searchTerm || title.includes(searchTerm) || artist.includes(searchTerm);
        const matchesMood = !selectedMood || moods.includes(selectedMood);
        
        if (matchesSearch && matchesMood) {
            card.style.display = 'block';
            card.style.animation = 'fadeIn 0.3s ease-out';
        } else {
            card.style.display = 'none';
        }
    });
}

// Initialize dynamic search if on songs page
if (window.location.pathname.includes('/Songs')) {
    document.addEventListener('DOMContentLoaded', initializeDynamicSearch);
}

// CSS Animation Keyframes (injected via JavaScript)
const style = document.createElement('style');
style.textContent = `
    .ripple {
        position: absolute;
        border-radius: 50%;
        transform: scale(0);
        animation: ripple 600ms linear;
        background-color: rgba(255, 255, 255, 0.6);
        pointer-events: none;
    }
    
    @keyframes ripple {
        to {
            transform: scale(4);
            opacity: 0;
        }
    }
    
    .card {
        transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    }
    
    .mood-badge {
        transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    }
`;
document.head.appendChild(style);