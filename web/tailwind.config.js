const path = require("path");

/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./src/**/*.{js,ts,jsx,tsx,mdx}"],
  presets: [require(path.join(__dirname, "../design/tailwind.preset.js"))],
  theme: {
    extend: {},
  },
  plugins: [],
};
