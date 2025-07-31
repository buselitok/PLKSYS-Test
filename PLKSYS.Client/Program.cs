using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PLKSYS.Client;
using PLKSYS.Client.Services;
using Microsoft.AspNetCore.Components.Authorization; // AuthenticationStateProvider i�in
using System;



var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// API'nizin URL'sini buraya girin.
// E�er API'niz localhost:5028'de �al���yorsa bu do�ru. De�ilse g�ncelleyin.
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5028") });



// Kimlik do�rulama servislerini kaydet
builder.Services.AddAuthorizationCore(); // Yetkilendirme i�in temel servisler
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>(); // �zel Auth State Provider

builder.Services.AddScoped<AuthService>(); // Auth servisimiz
builder.Services.AddScoped<VehicleService>(); // Ara� servisimiz
builder.Services.AddScoped<ReservationService>(); // Rezervasyon servisimiz
builder.Services.AddScoped<UserService>(); // Kullan�c� servisimiz
builder.Services.AddScoped<NoteService>();  // Not servisimiz

builder.Services.AddScoped<WashingService>(); // YEN� CLIENT SERV�S�N� EKLE

builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();

await builder.Build().RunAsync();