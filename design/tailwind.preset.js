/**
 * Shared Tailwind preset for Vitabu Vitabu v4 (web + admin).
 * Import in app tailwind.config: presets: [require('../design/tailwind.preset.js')]
 * Keep hex in sync with tokens.css and docs/design-system.md
 */

/** @type {import('tailwindcss').Config} */
module.exports = {
  theme: {
    extend: {
      fontFamily: {
        poppins: ['Poppins', 'sans-serif'],
        lato: ['Lato', 'sans-serif'],
      },
      colors: {
        primary: {
          50: '#F5F3F0',
          100: '#E8E3DC',
          500: '#6D4C41',
          600: '#5C3E35',
          700: '#4B3229',
          800: '#3A261D',
          900: '#2A1B11',
        },
        secondary: {
          50: '#E8F5E8',
          100: '#C8E6C9',
          500: '#2C5F2D',
          600: '#1B4332',
          700: '#081C15',
          800: '#052E16',
          900: '#031A0B',
        },
        accent: {
          50: '#FFF3E0',
          100: '#FFE0B2',
          500: '#E57C23',
          600: '#CC6900',
          700: '#B25600',
          800: '#994400',
          900: '#7F3300',
        },
        gold: {
          50: '#FEF7E0',
          100: '#FAECC1',
          500: '#C88D36',
          600: '#B8802A',
          700: '#A8731E',
          800: '#986612',
          900: '#885906',
        },
        neutral: {
          50: '#F9F5F2',
          100: '#F5E1DA',
          200: '#E8DDD6',
          300: '#D4C4B8',
          400: '#C0AB9A',
          500: '#A0877A',
          600: '#806D60',
          700: '#605346',
          800: '#40392C',
          900: '#201F12',
        },
      },
      backgroundImage: {
        'kitenge-pattern':
          "url('data:image/svg+xml,%3Csvg width=\"60\" height=\"60\" viewBox=\"0 0 60 60\" xmlns=\"http://www.w3.org/2000/svg\"%3E%3Cg fill=\"none\" fill-rule=\"evenodd\"%3E%3Cg fill=\"%23E57C23\" fill-opacity=\"0.1\"%3E%3Cpath d=\"M30 30c0-11.046-8.954-20-20-20s-20 8.954-20 20 8.954 20 20 20 20-8.954 20-20zm0 0c0 11.046 8.954 20 20 20s20-8.954 20-20-8.954-20-20-20-20 8.954-20 20z\"/%3E%3C/g%3E%3C/g%3E%3C/svg%3E')",
      },
      animation: {
        'bounce-subtle': 'bounce 2s infinite',
        'fade-in': 'fadeIn 0.5s ease-in-out',
        'slide-up': 'slideUp 0.3s ease-out',
        confetti: 'confetti 0.6s ease-out',
      },
    },
  },
};
