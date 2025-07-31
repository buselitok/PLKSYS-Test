using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System; // Exception için eklendi
using System.Text.Json;
using PLKSYS.Core.Models; // JsonSerializer için eklendi

namespace PLKSYS.Client.Services
{
    public class UserService
    {
        private readonly HttpClient _httpClient;

        public UserService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<User>> GetUsers()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<User>>("api/Users") ?? new List<User>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kullanıcılar getirilirken hata: {ex.Message}");
                throw; // Hatayı tekrar fırlat
            }
        }

        public async Task<User?> GetUserById(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<User>($"api/Users/{id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kullanıcı ID {id} getirilirken hata: {ex.Message}");
                throw;
            }
        }

        public async Task<User?> CreateUser(UserCreateDto userDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Users", userDto);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<User>();
            }
            else
            {
                // Hata durumunda API'den gelen mesajı oku
                var errorContent = await response.Content.ReadAsStringAsync();
                string errorMessage = "Kullanıcı eklenirken bir hata oluştu.";
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(errorContent);
                    if (errorResponse != null && errorResponse.TryGetValue("message", out var message))
                    {
                        errorMessage = message;
                    }
                }
                catch (JsonException)
                {
                    // JSON ayrıştırma hatası, varsayılan mesajı kullan
                }
                throw new Exception(errorMessage); // Özel hata mesajı fırlat
            }
        }

        public async Task UpdateUser(UserUpdateDto userDto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Users/{userDto.Id}", userDto);

            if (!response.IsSuccessStatusCode)
            {
                // Hata durumunda API'den gelen mesajı oku
                var errorContent = await response.Content.ReadAsStringAsync();
                string errorMessage = "Kullanıcı güncellenirken bir hata oluştu.";
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(errorContent);
                    if (errorResponse != null && errorResponse.TryGetValue("message", out var message))
                    {
                        errorMessage = message;
                    }
                }
                catch (JsonException)
                {
                    // JSON ayrıştırma hatası, varsayılan mesajı kullan
                }
                throw new Exception(errorMessage); // Özel hata mesajı fırlat
            }
        }

        public async Task DeleteUser(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Users/{id}");

            if (!response.IsSuccessStatusCode)
            {
                // Hata durumunda API'den gelen mesajı oku
                var errorContent = await response.Content.ReadAsStringAsync();
                string errorMessage = "Kullanıcı silinirken bir hata oluştu.";
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(errorContent);
                    if (errorResponse != null && errorResponse.TryGetValue("message", out var message))
                    {
                        errorMessage = message;
                    }
                }
                catch (JsonException)
                {
                    // JSON ayrıştırma hatası, varsayılan mesajı kullan
                }
                throw new Exception(errorMessage); // Özel hata mesajı fırlat
            }
        }
    }
}