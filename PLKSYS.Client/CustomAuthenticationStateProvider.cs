using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System; // DateTime için
using Microsoft.JSInterop; // IJSRuntime için

namespace PLKSYS.Client
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        /*private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage; // JavaScript interop için

        public CustomAuthenticationStateProvider(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken"); // Yerel depolamadan token'ı al

            var identity = new ClaimsIdentity();
            _httpClient.DefaultRequestHeaders.Authorization = null; // Önceki Auth header'ını temizle

            if (!string.IsNullOrEmpty(token))
            {
                var handler = new JwtSecurityTokenHandler();
                try
                {
                    var jwtToken = handler.ReadJwtToken(token);
                    var expiration = jwtToken.ValidTo;

                   


                    // Token süresi dolduysa veya geçerli değilse
                    if (expiration < DateTime.UtcNow)
                    {
                        await _localStorage.RemoveItemAsync("authToken"); // Token'ı sil
                        return new AuthenticationState(new ClaimsPrincipal(identity)); // Kimliği boş döndür
                    }

                    // Token'dan Claims oluştur
                    identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                catch (Exception)
                {
                    // Token geçersizse veya okunamıyorsa
                    await _localStorage.RemoveItemAsync("authToken"); // Token'ı sil
                }
            }

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        // Kullanıcı giriş yaptığında çağrılır
        public void MarkUserAsAuthenticated(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user))); // Auth durumunu güncelle

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token); // HttpClient'a token'ı ekle
        }

        // Kullanıcı çıkış yaptığında çağrılır
        public void MarkUserAsLoggedOut()
        {
            var identity = new ClaimsIdentity();
            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user))); // Auth durumunu güncelle

            _httpClient.DefaultRequestHeaders.Authorization = null; // HttpClient'dan token'ı kaldır
        }
    }

    // JavaScript interop için basit bir servis
    public interface ILocalStorageService
    {
        ValueTask SetItemAsync<T>(string key, T value);
        ValueTask<T> GetItemAsync<T>(string key);
        ValueTask RemoveItemAsync(string key);
    }

    public class LocalStorageService : ILocalStorageService
    {
        private readonly IJSRuntime _jsRuntime;

        public LocalStorageService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async ValueTask SetItemAsync<T>(string key, T value)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, JsonSerializer.Serialize(value));
        }

        public async ValueTask<T> GetItemAsync<T>(string key)
        {
            var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
            return string.IsNullOrEmpty(json) ? default! : JsonSerializer.Deserialize<T>(json)!;
        }

        public async ValueTask RemoveItemAsync(string key)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }*/

        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;

        public CustomAuthenticationStateProvider(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");

            var identity = new ClaimsIdentity();
            _httpClient.DefaultRequestHeaders.Authorization = null;

            if (!string.IsNullOrEmpty(token))
            {
                var handler = new JwtSecurityTokenHandler();
                try
                {
                    var jwtToken = handler.ReadJwtToken(token);
                    var expiration = jwtToken.ValidTo;

                   
                    
                    
                    

                    // Token süresi kontrolü
                    if (expiration < DateTime.UtcNow)
                    {
                        
                        await _localStorage.RemoveItemAsync("authToken");
                        return new AuthenticationState(new ClaimsPrincipal(identity));
                    }

                    // Claims'leri standart formata dönüştür
                    var claims = new List<Claim>();

                    foreach (var claim in jwtToken.Claims)
                    {
                        // Standart claim tiplerini kontrol et
                        switch (claim.Type.ToLower())
                        {
                            case "sub":
                            case "userid":
                            case "nameid":
                                claims.Add(new Claim(ClaimTypes.NameIdentifier, claim.Value));
                                break;
                            case "name":
                            case "username":
                                claims.Add(new Claim(ClaimTypes.Name, claim.Value));
                                break;
                            case "email":
                                claims.Add(new Claim(ClaimTypes.Email, claim.Value));
                                break;
                            case "role":
                                claims.Add(new Claim(ClaimTypes.Role, claim.Value));
                                break;
                            case "department":
                                claims.Add(new Claim("department", claim.Value));
                                break;
                            default:
                                // Diğer tüm claim'leri olduğu gibi ekle
                                claims.Add(new Claim(claim.Type, claim.Value));
                                break;
                        }
                    }

                    

                    identity = new ClaimsIdentity(claims, "jwt");
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                catch (Exception ex)
                {
                   
                    await _localStorage.RemoveItemAsync("authToken");
                }
            }

            var authState = new AuthenticationState(new ClaimsPrincipal(identity));

            // DEBUG: Final auth state'i logla
            var user = authState.User;
            

            return authState;
        }

        public void MarkUserAsAuthenticated(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Aynı claim dönüştürme işlemini burada da yap
            var claims = new List<Claim>();

            foreach (var claim in jwtToken.Claims)
            {
                switch (claim.Type.ToLower())
                {
                    case "sub":
                    case "userid":
                    case "nameid":
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, claim.Value));
                        break;
                    case "name":
                    case "username":
                        claims.Add(new Claim(ClaimTypes.Name, claim.Value));
                        break;
                    case "email":
                        claims.Add(new Claim(ClaimTypes.Email, claim.Value));
                        break;
                    case "role":
                        claims.Add(new Claim(ClaimTypes.Role, claim.Value));
                        break;
                    case "department":
                        claims.Add(new Claim("department", claim.Value));
                        break;
                    default:
                        claims.Add(new Claim(claim.Type, claim.Value));
                        break;
                }
            }

            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public void MarkUserAsLoggedOut()
        {
            var identity = new ClaimsIdentity();
            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));

            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    // Interface ve Service aynı kalıyor
    public interface ILocalStorageService
    {
        ValueTask SetItemAsync<T>(string key, T value);
        ValueTask<T> GetItemAsync<T>(string key);
        ValueTask RemoveItemAsync(string key);
    }

    public class LocalStorageService : ILocalStorageService
    {
        private readonly IJSRuntime _jsRuntime;

        public LocalStorageService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async ValueTask SetItemAsync<T>(string key, T value)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, JsonSerializer.Serialize(value));
        }

        public async ValueTask<T> GetItemAsync<T>(string key)
        {
            var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
            return string.IsNullOrEmpty(json) ? default! : JsonSerializer.Deserialize<T>(json)!;
        }

        public async ValueTask RemoveItemAsync(string key)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }

    }
}