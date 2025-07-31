using Microsoft.EntityFrameworkCore; // DbContext için bu using olmalı
using Microsoft.EntityFrameworkCore.Diagnostics;
using PLKSYS.Core.Models;

namespace PLKSYS.Core.Data
{
    public class PLKSYSContext : DbContext // DbContext'ten miras almalı
    {
        public PLKSYSContext(DbContextOptions<PLKSYSContext> options) : base(options)
        {
        }

        // Bu metodu ekliyoruz: EF Core uyarılarını yapılandırmak için
        // Bu, PendingModelChangesWarning uyarısını hata olarak fırlatmasını engeller.
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Bu uyarıyı bastırıyoruz çünkü HasData'yı Program.cs'e taşıdık.
            // Eğer hala dinamik değerler kullanılıyorsa, bu uyarıyı görmezden gelmek için kullanılır.
            optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        }

        // Veritabanı tablolarını temsil eden DbSet'ler
        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<WalkInVisit> WalkInVisits { get; set; }
        public DbSet<VehicleActivity> VehicleActivities { get; set; }
        public DbSet<WashingQueueEntry> WashingQueueEntries { get; set; }
        

        //public IEnumerable<object> EntryCard { get; internal set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // İlişkileri tanımlama (Fluent API kullanarak)
            // Bir aracın birden fazla notu olabilir
            modelBuilder.Entity<Vehicle>()
                .HasMany(v => v.Notes)
                .WithOne(n => n.Vehicle)
                .HasForeignKey(n => n.PlateNumber)
                .OnDelete(DeleteBehavior.Cascade); 

            // User modeli için benzersiz kullanıcı adı
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Vehicle modeli için PlateNumber'ı Primary Key olarak ayarla
            modelBuilder.Entity<Vehicle>()
                .HasKey(v => v.PlateNumber);

            // Örnek: Reservation CreatedAt için varsayılan değer
            modelBuilder.Entity<Reservation>()
                .Property(r => r.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP"); // SQLite için

            // Örnek: Note Timestamp için varsayılan değer
            modelBuilder.Entity<Note>()
                .Property(n => n.Timestamp)
                .HasDefaultValueSql("CURRENT_TIMESTAMP"); // SQLite için
            modelBuilder.Entity<WashingQueueEntry>()
                .Property(e => e.SentToWashingTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            modelBuilder.Entity<WashingQueueEntry>()
                .Property(e => e.Status)
                .HasDefaultValue("Beklemede");

            // YENİ: Customer İlişkileri
            

            modelBuilder.Entity<Customer>()
                .Property(c => c.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // YENİ: WalkInVisit İlişkileri
            modelBuilder.Entity<WalkInVisit>()
                .HasOne(w => w.Customer)
                .WithMany(c => c.WalkInVisits)
                .HasForeignKey(w => w.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WalkInVisit>()
                .Property(w => w.VisitTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<WalkInVisit>()
                .Property(w => w.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<WalkInVisit>()
                .Property(w => w.Status)
                .HasDefaultValue("Aktif");

            // YENİ: Vehicle-Customer İlişkisi (Opsiyonel - araç sahibi takibi için)
            modelBuilder.Entity<Vehicle>()
                .HasOne<Customer>()
                .WithMany(c => c.OwnedVehicles)
                .HasForeignKey("CustomerId")
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
  