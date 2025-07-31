// Program.cs
// Bu dosya, ASP.NET Core Web API uygulamas�n�n ba�lang�� noktas�n� ve servislerin yap�land�rmas�n� i�erir.

using Microsoft.EntityFrameworkCore;
using PLKSYS.Core.Data;
using PLKSYS.Core.Services; // IAuthService, IUserService vb. i�in
using PLKSYS.Core.Interfaces; // AuthService, UserService vb. i�in
using PLKSYS.Core.Models; // User, Reservation modelleri i�in
using PLKSYS.Core.Helpers; // AuthHelper i�in
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization; // ReferenceHandler i�in
using Microsoft.OpenApi.Models; // Swagger UI i�in
using System; // DateTime, TimeSpan i�in
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims; // CreateScope i�in

var builder = WebApplication.CreateBuilder(args);

// Servisleri ba��ml�l�k enjeksiyonu i�in kaydetme
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
            ValidateIssuer = true, // Issuer do�rulamay� etkinle�tir
            ValidateAudience = true, // Audience do�rulamay� etkinle�tir
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidIssuer = builder.Configuration["Jwt:Issuer"], // appsettings.json'dan al
            ValidAudience = builder.Configuration["Jwt:Audience"], // appsettings.json'dan al
            RoleClaimType = ClaimTypes.Role
        };
    });

// Authorization servisini ekle ve rolleri politikalar olarak yap�land�r
builder.Services.AddAuthorization(options =>
{
    // Superadmin departman�na sahip kullan�c�lar i�in politika
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
    options.AddPolicy("SuperadminPolicy", policy => policy.RequireRole("Superadmin"));
    options.AddPolicy("AdminOrSuperadmin", policy => policy.RequireRole("Admin", "Superadmin"));
    options.AddPolicy("AllDepartments", policy => policy.RequireClaim(ClaimTypes.Role)); // Departman claim'i olan herkes

    options.AddPolicy("G�venlikGuardPolicy", policy => policy.RequireRole("G�venlik"));
    options.AddPolicy("MuhasebePolicy", policy => policy.RequireRole("Muhasebe"));
    options.AddPolicy("ServisPolicy", policy => policy.RequireRole("Servis"));
    options.AddPolicy("Sat��Policy", policy => policy.RequireRole("Sat��"));
    options.AddPolicy("SigortaPolicy", policy => policy.RequireRole("Sigorta"));
    options.AddPolicy("Y�kamaPolicy", policy => policy.RequireRole("Y�kama")); // YEN� YIKAMA DEPARTMANI POL�T�KASI
    options.AddPolicy("M��teriTemsilcisiPolicy", policy => policy.RequireRole("M��teriTemsilcisi"));
    options.AddPolicy("M��teriPolicy", policy => policy.RequireRole("M��teri"));

    // Ortak politikalar
    options.AddPolicy("AdminOrMuhasebeOrSuperadmin", policy => policy.RequireRole("Admin", "Muhasebe", "Superadmin"));
    options.AddPolicy("AdminOrG�venlikOrSuperadmin", policy => policy.RequireRole("Admin", "G�venlik", "Superadmin"));
    options.AddPolicy("ServisOrAdminOrSuperadmin", policy => policy.RequireRole("Servis", "Admin", "Superadmin")); // YEN� POL�T�KA
    options.AddPolicy("Y�kamaOrAdminOrSuperadmin", policy => policy.RequireRole("Y�kama", "Admin", "Superadmin")); // YEN� POL�T�KA
    options.AddPolicy("SuperadminOrAdminOrM��teri", policy => policy.RequireRole("Superadmin","Admin","M��teri"));
    options.AddPolicy("AdminOrSuperadminOrServiceOrSalesOrInsuranceOrAccounting", policy => policy.RequireRole("Admin", "Superadmin", "Servis", "Sat��", "Sigorta", "Muhasebe", "M��teriTemsilcisi"));
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
        Console.WriteLine("Veritaban� bo�, ba�lang�� verileri ekleniyor...");

        var adminPassword = "adminpassword";
        var (adminPasswordHash, adminPasswordSalt) = AuthHelper.CreatePasswordHash(adminPassword);
        db.Users.Add(new User
        {
            Id = 1,
            Username = "admin",
            PasswordHash = adminPasswordHash,
            PasswordSalt = adminPasswordSalt,
            FirstName = "Sistem",
            LastName = "Y�netici",
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
            FirstName = "S�per",
            LastName = "Y�netici",
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
            FirstName = "G�venlik",
            LastName = "G�revlisi",
            Department = "G�venlik",
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
            FirstName = "Sat��",
            LastName = "Personeli",
            Department = "Sat��",
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
            FirstName = "Y�kama",
            LastName = "Personeli",
            Department = "Y�kama", 
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
            FirstName = "M��teri",
            LastName = "Giri�i",
            Department = "M��teri", 
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
            FirstName = "M��teri",
            LastName = "temsilcisi",
            Department = "M��teriTemsilcisi",
            CreatedAt = DateTime.UtcNow
        });


        db.Reservations.AddRange(
            new Reservation
            {
                Id = 1,
                PlateNumber = "34XYZ789",
                CustomerName = "Ay�e Y�lmaz",
                ReservationDateTime = DateTime.UtcNow.AddDays(1).Date.AddHours(14),
                ServiceType = "Periyodik Bak�m",
                Notes = "M��teri erken gelmek isteyebilir.",
                Status = "Onayland�",
                CreatedAt = DateTime.UtcNow
            },
            new Reservation
            {
                Id = 2,
                PlateNumber = "06ABC456",
                CustomerName = "Mehmet Can",
                ReservationDateTime = DateTime.UtcNow.AddDays(2).Date.AddHours(10).AddMinutes(30),
                ServiceType = "Hasar Onar�m",
                Notes = "Sa� �n �amurluk hasar�.",
                Status = "Onayland�",
                CreatedAt = DateTime.UtcNow
            },
            new Reservation
            {
                Id = 3,
                PlateNumber = "16DEF123",
                CustomerName = "Zeynep Demir",
                ReservationDateTime = DateTime.UtcNow.AddDays(0).Date.AddHours(DateTime.UtcNow.Hour + 1),
                ServiceType = "Lastik De�i�imi",
                Notes = "Acil lastik de�i�imi, m��teri bekliyor.",
                Status = "Onayland�",
                CreatedAt = DateTime.UtcNow
            },
            new Reservation
            {
                Id = 4,
                PlateNumber = "77XYZ001",
                CustomerName = "Ali Veli",
                ReservationDateTime = DateTime.UtcNow.AddDays(3).Date.AddHours(11).AddMinutes(0),
                ServiceType = "Yeni Ara� Sat��",
                Notes = "Yeni ara� almak istiyor.",
                Status = "Onayland�",
                CreatedAt = DateTime.UtcNow
            },
            new Reservation
            {
                Id = 5,
                PlateNumber = "88ABC002",
                CustomerName = "Canan Y�lmaz",
                ReservationDateTime = DateTime.UtcNow.AddDays(4).Date.AddHours(15).AddMinutes(0),
                ServiceType = "Kasko Yenileme",
                Notes = "Mevcut kaskosu bitiyor.",
                Status = "Onayland�",
                CreatedAt = DateTime.UtcNow
            }
        );

        db.SaveChanges();
        Console.WriteLine("Ba�lang�� verileri ba�ar�yla eklendi.");
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