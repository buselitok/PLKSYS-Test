/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        './**/*.razor', // T�m .razor dosyalar�n� tara
        './**/*.html',  // T�m .html dosyalar�n� tara
        './wwwroot/index.html', // index.html dosyas�n� da tara
        // Gerekirse di�er CSS veya JS dosyalar�n�z� da buraya ekleyebilirsiniz
    ],
    theme: {
        extend: {},
    },
    plugins: [],
}