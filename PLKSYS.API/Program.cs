// Program.cs
// Bu dosya, ASP.NET Core Web API uygulamasýnýn baþlangýç noktasýný ve servislerin yapýlandýrmasýný içerir.

using Microsoft.EntityFrameworkCore;
using PLKSYS.Core.Data;
using PLKSYS.Core.Services; // IAuthService, IUserService vb. için
using PLKSYS.Core.Interfaces; // AuthService, UserService vb. için
using PLKSYS.Core.Models; // User, Reservation modelleri için
using PLKSYS.Core.Helpers; // AuthHelper için
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization; // ReferenceHandler için
using Microsoft.OpenApi.Models; // Swagger UI için
using System; // DateTime, TimeSpan için
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims; // CreateScope için

var builder = WebApplication.CreateBuilder(args);

// Servisleri baðýmlýlýk enjeksiyonu için kaydetme
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<INoteService, NoteService>();
builder.Services.AddScoped<IWashingService, WashingService>();
builder.Services.AddScoped<IVehicleActivityService, VehicleActivityService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

builder.Services.AddDbContext<PLKSYSContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqliteOptions => sqliteOptions.MigrationsAssembly(typeof(Program).Assembly.FullName)));

builder.Services.AddControllers().AddJsonOptions(x =>
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuer = true, // Issuer doðrulamayý etkinleþtir
            ValidateAudience = true, // Audience doðrulamayý etkinleþtir
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidIssuer = builder.Configuration["Jwt:Issuer"], // appsettings.json'dan al
            ValidAudience = builder.Configuration["Jwt:Audience"], // appsettings.json'dan al
            RoleClaimType = ClaimTypes.Role
        };
    });

// Authorization servisini ekle ve rolleri politikalar olarak yapýlandýr
builder.Services.AddAuthorization(options =>
{
    // Superadmin departmanýna sahip kullanýcýlar için politika
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
    options.AddPolicy("SuperadminPolicy", policy => policy.RequireRole("Superadmin"));
    options.AddPolicy("AdminOrSuperadmin", policy => policy.RequireRole("Admin", "Superadmin"));
    options.AddPolicy("AllDepartments", policy => policy.RequireClaim(ClaimTypes.Role)); // Departman claim'i olan herkes

    options.AddPolicy("GüvenlikGuardPolicy", policy => policy.RequireRole("Güvenlik"));
    options.AddPolicy("MuhasebePolicy", policy => policy.RequireRole("Muhasebe"));
    options.AddPolicy("ServisPolicy", policy => policy.RequireRole("Servis"));
    options.AddPolicy("SatýþPolicy", policy => policy.RequireRole("Satýþ"));
    options.AddPolicy("SigortaPolicy", policy => policy.RequireRole("Sigorta"));
    options.AddPolicy("YýkamaPolicy", policy => policy.RequireRole("Yýkama")); // YENÝ YIKAMA DEPARTMANI POLÝTÝKASI
    options.AddPolicy("MüþteriTemsilcisiPolicy", policy => policy.RequireRole("MüþteriTemsilcisi"));
    options.AddPolicy("MüþteriPolicy", policy => policy.RequireRole("Müþteri"));

    // Ortak politikalar
    options.AddPolicy("AdminOrMuhasebeOrSuperadmin", policy => policy.RequireRole("Admin", "Muhasebe", "Superadmin"));
    options.AddPolicy("AdminOrGüvenlikOrSuperadmin", policy => policy.RequireRole("Admin", "Güvenlik", "Superadmin"));
    options.AddPolicy("ServisOrAdminOrSuperadmin", policy => policy.RequireRole("Servis", "Admin", "Superadmin")); // YENÝ POLÝTÝKA
    options.AddPolicy("YýkamaOrAdminOrSuperadmin", policy => policy.RequireRole("Yýkama", "Admin", "Superadmin")); // YENÝ POLÝTÝKA
    options.AddPolicy("SuperadminOrAdminOrMüþteri", policy => policy.RequireRole("Superadmin","Admin","Müþteri"));
    options.AddPolicy("AdminOrSuperadminOrServiceOrSalesOrInsuranceOrAccounting", policy => policy.RequireRole("Admin", "Superadmin", "Servis", "Satýþ", "Sigorta", "Muhasebe", "MüþteriTemsilcisi"));
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp",
        policy => policy.WithOrigins("http://localhost:5276", "https://localhost:5278")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PLKSYS API", Version = "v1" });
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "JWT Authorization header using the Bearer scheme.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = "Bearer",
            Type = ReferenceType.SecurityScheme
        }
    };
    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, new[] { "Bearer" } }
    });
});


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PLKSYSContext>();
    db.Database.Migrate();

    if (!db.Users.Any())
    {
        Console.WriteLine("Veritabaný boþ, baþlangýç verileri ekleniyor...");

        var adminPassword = "adminpassword";
        var (adminPasswordHash, adminPasswordSalt) = AuthHelper.CreatePasswordHash(adminPassword);
        db.Users.Add(new User
        {
            Id = 1,
            Username = "admin",
            PasswordHash = adminPasswordHash,
            PasswordSalt = adminPasswordSalt,
            FirstName = "Sistem",
            LastName = "Yönetici",
            Department = "Admin",
            CreatedAt = DateTime.UtcNow
        });

        var superadminPassword = "superadminpassword";
        var (superadminHash, superadminSalt) = AuthHelper.CreatePasswordHash(superadminPassword);
        db.Users.Add(new User
        {
            Id = 6,
            Username = "superadmin",
            PasswordHash = superadminHash,
            PasswordSalt = superadminSalt,
            FirstName = "Süper",
            LastName = "Yönetici",
            Department = "Superadmin",
            CreatedAt = DateTime.UtcNow
        });


        var securityPassword = "securitypassword";
        var (secPasswordHash, secPasswordSalt) = AuthHelper.CreatePasswordHash(securityPassword);
        db.Users.Add(new User
        {
            Id = 2,
            Username = "security",
            PasswordHash = secPasswordHash,
            PasswordSalt = secPasswordSalt,
            FirstName = "Güvenlik",
            LastName = "Görevlisi",
            Department = "Güvenlik",
            CreatedAt = DateTime.UtcNow
        });

        var servicePassword = "servicepassword";
        var (srvPasswordHash, srvPasswordSalt) = AuthHelper.CreatePasswordHash(servicePassword);
        db.Users.Add(new User
        {
            Id = 3,
            Username = "serviceuser",
            PasswordHash = srvPasswordHash,
            PasswordSalt = srvPasswordSalt,
            FirstName = "Servis",
            LastName = "Personeli",
            Department = "Servis",
            CreatedAt = DateTime.UtcNow
        });

        var salesPassword = "salespassword";
        var (salesHash, salesSalt) = AuthHelper.CreatePasswordHash(salesPassword);
        db.Users.Add(new User
        {
            Id = 4,
            Username = "sales",
            PasswordHash = salesHash,
            PasswordSalt = salesSalt,
            FirstName = "Satýþ",
            LastName = "Personeli",
            Department = "Satýþ",
            CreatedAt = DateTime.UtcNow
        });

        var insurancePassword = "insurancepassword";
        var (insHash, insSalt) = AuthHelper.CreatePasswordHash(insurancePassword);
        db.Users.Add(new User
        {
            Id = 5,
            Username = "insurance",
            PasswordHash = insHash,
            PasswordSalt = insSalt,
            FirstName = "Sigorta",
            LastName = "Personeli",
            Department = "Sigorta",
            CreatedAt = DateTime.UtcNow
        });
        var washingPassword = "washingpassword";
        var (washHash, washSalt) = AuthHelper.CreatePasswordHash(washingPassword);
        db.Users.Add(new User
        {
            Id = 7, // Yeni ID
            Username = "washing",
            PasswordHash = washHash,
            PasswordSalt = washSalt,
            FirstName = "Yýkama",
            LastName = "Personeli",
            Department = "Yýkama", 
            CreatedAt = DateTime.UtcNow
        });
        var customerPassword = "customerpassword";
        var (cusHash, cusSalt) = AuthHelper.CreatePasswordHash(customerPassword);
        db.Users.Add(new User
        {
            Id = 8, // Yeni ID
            Username = "customer",
            PasswordHash = cusHash,
            PasswordSalt = cusSalt,
            FirstName = "Müþteri",
            LastName = "Giriþi",
            Department = "Müþteri", 
            CreatedAt = DateTime.UtcNow
        });
        var accountingPassword = "accountingpassword";
        var (accHash, accSalt) = AuthHelper.CreatePasswordHash(accountingPassword);
        db.Users.Add(new User
        {
            Id = 9, // Yeni ID
            Username = "accounting",
            PasswordHash = accHash,
            PasswordSalt = accSalt,
            FirstName = "Muhasebe",
            LastName = "Personeli",
            Department = "Muhasebe", 
            CreatedAt = DateTime.UtcNow
        });
        var representativePassword = "representativepassword";
        var (repHash, repSalt) = AuthHelper.CreatePasswordHash(representativePassword);
        db.Users.Add(new User
        {
            Id = 10, // Yeni ID
            Username = "representative",
            PasswordHash = repHash,
            PasswordSalt = repSalt,
            FirstName = "Müþteri",
            LastName = "temsilcisi",
            Department = "MüþteriTemsilcisi",
            CreatedAt = DateTime.UtcNow
        });


        db.Reservations.AddRange(
            new Reservation
            {
                Id = 1,
                PlateNumber = "34XYZ789",
                CustomerName = "Ayþe Yýlmaz",
                ReservationDateTime = DateTime.UtcNow.AddDays(1).Date.AddHours(14),
                ServiceType = "Periyodik Bakým",
                Notes = "Müþteri erken gelmek isteyebilir.",
                Status = "Onaylandý",
                CreatedAt = DateTime.UtcNow
            },
            new Reservation
            {
                Id = 2,
                PlateNumber = "06ABC456",
                CustomerName = "Mehmet Can",
                ReservationDateTime = DateTime.UtcNow.AddDays(2).Date.AddHours(10).AddMinutes(30),
                ServiceType = "Hasar Onarým",
                Notes = "Sað ön çamurluk hasarý.",
                Status = "Onaylandý",
                CreatedAt = DateTime.UtcNow
            },
            new Reservation
            {
                Id = 3,
                PlateNumber = "16DEF123",
                CustomerName = "Zeynep Demir",
                ReservationDateTime = DateTime.UtcNow.AddDays(0).Date.AddHours(DateTime.UtcNow.Hour + 1),
                ServiceType = "Lastik Deðiþimi",
                Notes = "Acil lastik deðiþimi, müþteri bekliyor.",
                Status = "Onaylandý",
                CreatedAt = DateTime.UtcNow
            },
            new Reservation
            {
                Id = 4,
                PlateNumber = "77XYZ001",
                CustomerName = "Ali Veli",
                ReservationDateTime = DateTime.UtcNow.AddDays(3).Date.AddHours(11).AddMinutes(0),
                ServiceType = "Yeni Araç Satýþ",
                Notes = "Yeni araç almak istiyor.",
                Status = "Onaylandý",
                CreatedAt = DateTime.UtcNow
            },
            new Reservation
            {
                Id = 5,
                PlateNumber = "88ABC002",
                CustomerName = "Canan Yýlmaz",
                ReservationDateTime = DateTime.UtcNow.AddDays(4).Date.AddHours(15).AddMinutes(0),
                ServiceType = "Kasko Yenileme",
                Notes = "Mevcut kaskosu bitiyor.",
                Status = "Onaylandý",
                CreatedAt = DateTime.UtcNow
            }
        );

        db.SaveChanges();
        Console.WriteLine("Baþlangýç verileri baþarýyla eklendi.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PLKSYS API v1"));
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowBlazorApp");
app.UseAuthentication();
app.UseAuthorization(); // Middleware'i kullan
app.MapControllers();
app.Run();