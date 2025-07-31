using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design; // IDesignTimeDbContextFactory için
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration; // ConfigurationBuilder için
using System.IO; // Path için

namespace PLKSYS.Core.Data
{
    public class PLKSYSContextFactory : IDesignTimeDbContextFactory<PLKSYSContext>
    {
        public PLKSYSContext CreateDbContext(string[] args)
        {
            // appsettings.json'ın bulunduğu dizini dinamik olarak bulmaya çalışıyoruz.
            // dotnet ef komutu, hedef proje (PLKSYS.API) dizininden çalışır.
            // Bu yüzden appsettings.json'ı PLKSYS.API projesinin dizininde aramasını sağlamalıyız.

            // Bu, komutun çalıştırıldığı dizinden bir üst dizine (PLKSYS) ve oradan PLKSYS.API'ye gitmeyi dener.
            var apiProjectPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "..", // PLKSYS.API'den PLKSYS'e çık
                "PLKSYS.API" // PLKSYS'ten PLKSYS.API'ye gir
            );

            // Eğer yukarıdaki yol doğru değilse veya bulunamazsa, bir fallback olarak
            // dotnet ef'in çalıştırıldığı mevcut dizini kullanırız (ki bu PLKSYS.API dizini olmalı).
            if (!Directory.Exists(apiProjectPath) || !File.Exists(Path.Combine(apiProjectPath, "appsettings.json")))
            {
                apiProjectPath = Directory.GetCurrentDirectory(); // Bu, PLKSYS.API dizini olmalı
            }

            var configuration = new ConfigurationBuilder()
                .SetBasePath(apiProjectPath) // appsettings.json'ın bulunduğu yeri belirt
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // optional: false ile dosya yoksa hata ver
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<PLKSYSContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("DefaultConnection connection string not found in appsettings.json.");
            }

            // GÖÇLERİN PLKSYS.API assembly'sinde olacağını burada açıkça belirtiyoruz.
            optionsBuilder.UseSqlite(connectionString,
                sqliteOptions => sqliteOptions.MigrationsAssembly("PLKSYS.API"));

            // Burada da uyarıyı bastırıyoruz, çünkü tasarım zamanı araçları bu factory'yi kullanır.
            optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));

            return new PLKSYSContext(optionsBuilder.Options);
        }
    }
}