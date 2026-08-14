const path = require("path");

/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./index.html", "./src/**/*.{js,ts,jsx,tsx}"],
  presets: [require(path.join(__dirname, "../design/tailwind.preset.js"))],
  theme: {
    extend: {},
  },
  plugins: [],
};
