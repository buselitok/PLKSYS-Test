using PLKSYS.Core.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PLKSYS.Client.Services
{
    public class WashingService
    {
        private readonly HttpClient _httpClient;

        public WashingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<WashingQueueEntry?> SendToWashingAsync(WashingQueueEntryCreateDto createDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Washing/send-to-washing", createDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<WashingQueueEntry>();
        }

        public async Task<WashingQueueEntry?> MarkWashingCompletedAsync(Guid entryId)
        {
            var response = await _httpClient.PutAsync($"api/Washing/{entryId}/complete", null);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<WashingQueueEntry>();
        }

        public async Task<IEnumerable<WashingQueueEntry>?> GetPendingWashingEntriesAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<WashingQueueEntry>>("api/Washing/pending");
        }

        public async Task<IEnumerable<WashingQueueEntry>?> GetAllWashingEntriesAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<WashingQueueEntry>>("api/Washing/all");
        }

        public async Task<WashingQueueEntry?> GetWashingEntryByIdAsync(Guid entryId)
        {
            return await _httpClient.GetFromJsonAsync<WashingQueueEntry>($"api/Washing/{entryId}");
        }
    }
}