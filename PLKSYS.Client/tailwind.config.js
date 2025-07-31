/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        './**/*.razor', // Tüm .razor dosyalarýný tara
        './**/*.html',  // Tüm .html dosyalarýný tara
        './wwwroot/index.html', // index.html dosyasýný da tara
        // Gerekirse diðer CSS veya JS dosyalarýnýzý da buraya ekleyebilirsiniz
    ],
    theme: {
        extend: {},
    },
    plugins: [],
}