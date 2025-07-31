using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using PLKSYS.Core.Models; // LoginRequest, LoginResponse ve User modelleri için
using Microsoft.AspNetCore.Components.Authorization; // CustomAuthenticationStateProvider için
using System.Text.Json; // JsonSerializer için
using System.Collections.Generic; // Dictionary için
using System; // Exception için
using System.Linq; // Select ve ToList için
using System.Security.Claims; // ClaimTypes için

namespace PLKSYS.Client.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly CustomAuthenticationStateProvider _authenticationStateProvider;
        private readonly ILocalStorageService _localStorage;

        public AuthService(HttpClient httpClient, AuthenticationStateProvider authenticationStateProvider, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _authenticationStateProvider = (CustomAuthenticationStateProvider)authenticationStateProvider;
            _localStorage = localStorage;
        }

        public async Task<LoginResponse?> Login(LoginRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Auth/login", request);

                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

                    if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
                    {
                        await _localStorage.SetItemAsync("authToken", loginResponse.Token);
                        _authenticationStateProvider.MarkUserAsAuthenticated(loginResponse.Token);
                    }
                    return loginResponse;
                }
                else
                {
                    
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Hata Yanıtı: {errorContent}");
                    

                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(errorContent);
                        if (errorResponse != null && errorResponse.TryGetValue("message", out var message))
                        {
                            throw new Exception(message);
                        }
                    }
                    catch (JsonException)
                    {
                        // JSON ayrıştırma hatası, genel mesaj döndür
                    }

                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new Exception("Kullanıcı adı veya şifre yanlış.");
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        throw new Exception("Bu işleme yetkiniz yok.");
                    }
                    else
                    {
                        throw new Exception($"Giriş sırasında bir hata oluştu: Durum Kodu {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Giriş sırasında beklenmeyen bir hata oluştu: {ex.Message}");
            }
        }

        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("authToken");
            _authenticationStateProvider.MarkUserAsLoggedOut();
        }

        public async Task<List<string>> GetUserDepartment()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            if (user.Identity?.IsAuthenticated ?? false)
            {
                return user.FindAll("department").Select(c => c.Value).ToList();
            }
            return new List<string>();
        }
    }
}