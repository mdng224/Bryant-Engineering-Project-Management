/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['./index.html', './src/**/*.{vue,js,ts,jsx,tsx}'], // <-- v3 key
  darkMode: 'class', // 'media' or 'class' — NOT true
  theme: {
    extend: {},
  },
  plugins: [],
};
