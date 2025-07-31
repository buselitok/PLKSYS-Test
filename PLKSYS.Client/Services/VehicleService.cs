using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using PLKSYS.Core.Models;
using static System.Net.WebRequestMethods;


namespace PLKSYS.Client.Services
{
    public class VehicleService
    {
        private readonly HttpClient _httpClient;

        public VehicleService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Vehicle>> GetLiveVehicles()
        {
            return await _httpClient.GetFromJsonAsync<List<Vehicle>>("api/Vehicles/live") ?? new List<Vehicle>();
        }

        public async Task<List<Vehicle>> GetVehicleLogs()
        {
            return await _httpClient.GetFromJsonAsync<List<Vehicle>>("api/Vehicles/logs") ?? new List<Vehicle>();
        }

        public async Task<Vehicle?> GetVehicleByPlate(string plateNumber)
        {
            return await _httpClient.GetFromJsonAsync<Vehicle>($"api/Vehicles/{plateNumber}");
        }

        public async Task<Vehicle?> RegisterVehicleEntry(PlateEntryRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Vehicles/entry", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Vehicle>();
        }

        public async Task<Vehicle?> RegisterVehicleExit(PlateEntryRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Vehicles/exit", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Vehicle>();
        }


        
        public async Task<bool> ClaimVehicleAsync(string plateNumber, int userId, string userName)
        {
            try
            {
                var requestData = new
                {
                    PlateNumber = plateNumber,
                    UserId = userId,
                    UserName = userName
                };

                var response = await _httpClient.PostAsJsonAsync("api/Notification/claim", requestData);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Claim exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UnclaimVehicleAsync(string plateNumber, int userId)
        {
            try
            {
                var requestData = new
                {
                    PlateNumber = plateNumber,
                    UserId = userId
                };

                // URL'yi değiştirin
                var response = await _httpClient.PostAsJsonAsync("api/Notification/unclaim", requestData);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unclaim exception: {ex.Message}");
                return false;
            }
        }

        // YENİ METODLAR
        public async Task<List<Vehicle>?> GetVehiclesPendingExitApproval()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Vehicles/pending-exit-approval");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<Vehicle>>();
                }
                return new List<Vehicle>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting pending vehicles: {ex.Message}");
                return new List<Vehicle>();
            }
        }

        public async Task<bool> ApproveExit(ExitApprovalRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Vehicles/approve-exit", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error approving vehicle exit: {ex.Message}");
                return false;
            }
        }

        public async Task<Vehicle?> RegisterAppointmentEntry(AppointmentEntryRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Vehicles/entry/appointment", request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<Vehicle>();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering appointment entry: {ex.Message}");
                return null;
            }
        }

        public async Task<Vehicle?> RegisterWalkInEntry(WalkInEntryRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Vehicles/entry/walk-in", request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<Vehicle>();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering walk-in entry: {ex.Message}");
                return null;
            }
        }



    }
}