using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Text;
using System; // RandomNumberGenerator için

namespace PLKSYS.Core.Helpers
{
    public static class AuthHelper
    {
        // Şifreyi hash'ler ve bir salt oluşturur
        public static (byte[] passwordHash, byte[] passwordSalt) CreatePasswordHash(string password)
        {
            // 128 bit (16 byte) uzunluğunda rastgele bir salt oluştur
            byte[] passwordSalt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(passwordSalt); // Salt'ı rastgele baytlarla doldur
            }

            // PBKDF2 algoritmasını kullanarak şifreyi hash'le
            byte[] passwordHash = KeyDerivation.Pbkdf2(
                password: password,
                salt: passwordSalt,
                prf: KeyDerivationPrf.HMACSHA512, // HMACSHA512 PRF (Pseudo-random function) kullan
                iterationCount: 10000, // 10.000 iterasyon, brute-force saldırılarını yavaşlatır
                numBytesRequested: 256 / 8); // 256 bit (32 byte) hash boyutu

            return (passwordHash, passwordSalt); // Hash ve salt'ı tuple olarak döndür
        }

        // Girilen şifrenin saklanan hash ile eşleşip eşleşmediğini doğrular
        public static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            // Verilen şifre ve saklanan salt ile yeni bir hash hesapla
            byte[] computedHash = KeyDerivation.Pbkdf2(
                password: password,
                salt: storedSalt,
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 10000,
                numBytesRequested: 256 / 8);

            // Hesaplanan hash ile saklanan hash'i bayt bayt karşılaştır
            // Güvenli bir karşılaştırma için tüm baytları kontrol etmek önemlidir
            // Bu, zamanlama saldırılarına karşı koruma sağlar.
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != storedHash[i]) return false;
            }
            return true; // Hash'ler eşleşiyorsa true döndür
        }
    }
}