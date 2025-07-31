using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PLKSYS.Client;
using PLKSYS.Client.Services;
using Microsoft.AspNetCore.Components.Authorization; // AuthenticationStateProvider için
using System;



var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// API'nizin URL'sini buraya girin.
// Eðer API'niz localhost:5028'de çalýþýyorsa bu doðru. Deðilse güncelleyin.
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5028") });



// Kimlik doðrulama servislerini kaydet
builder.Services.AddAuthorizationCore(); // Yetkilendirme için temel servisler
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>(); // Özel Auth State Provider

builder.Services.AddScoped<AuthService>(); // Auth servisimiz
builder.Services.AddScoped<VehicleService>(); // Araç servisimiz
builder.Services.AddScoped<ReservationService>(); // Rezervasyon servisimiz
builder.Services.AddScoped<UserService>(); // Kullanýcý servisimiz
builder.Services.AddScoped<NoteService>();  // Not servisimiz

builder.Services.AddScoped<WashingService>(); // YENÝ CLIENT SERVÝSÝNÝ EKLE

builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();

await builder.Build().RunAsync();