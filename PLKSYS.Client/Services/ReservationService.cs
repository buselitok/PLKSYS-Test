using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

using System;
using PLKSYS.Core.Models;
using static System.Net.WebRequestMethods;

namespace PLKSYS.Client.Services
{
    public class ReservationService
    {
        private readonly HttpClient _httpClient;

        public ReservationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Reservation>> GetAllReservations()
        {
            return await _httpClient.GetFromJsonAsync<List<Reservation>>("api/Reservations") ?? new List<Reservation>();
        }

        public async Task<List<Reservation>> GetUpcomingReservations(DateTime? date = null)
        {
            string url = "api/Reservations/upcoming";
            if (date.HasValue)
            {
                url += $"?date={date.Value.ToString("yyyy-MM-dd")}";
            }
            return await _httpClient.GetFromJsonAsync<List<Reservation>>(url) ?? new List<Reservation>();
        }

        public async Task<Reservation?> GetReservationById(int id)
        {
            return await _httpClient.GetFromJsonAsync<Reservation>($"api/Reservations/{id}");
        }

        public async Task<Reservation?> CreateReservation(ReservationCreateDto reservationDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Reservations", reservationDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Reservation>();
        }

        public async Task UpdateReservation(ReservationUpdateDto reservationDto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Reservations/{reservationDto.Id}", reservationDto);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteReservation(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Reservations/{id}");
            response.EnsureSuccessStatusCode();
        }

       

    }
}