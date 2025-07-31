using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using PLKSYS.Core.Models; // Modeller için

namespace PLKSYS.Client.Services
{
    public class NoteService
    {
        private readonly HttpClient _httpClient;

        public NoteService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Note>> GetAllNotes()
        {
            return await _httpClient.GetFromJsonAsync<List<Note>>("api/Notes") ?? new List<Note>();
        }

        public async Task<List<Note>> GetNotesByPlate(string plateNumber)
        {
            return await _httpClient.GetFromJsonAsync<List<Note>>($"api/Notes/plate/{plateNumber}") ?? new List<Note>();
        }

        public async Task<Note?> CreateNote(NoteCreateDto noteDto)
        {
            // API tarafı, notu oluşturan kullanıcının ID'sini ve departmanını JWT token'ından alacaktır.
            // Bu yüzden Client tarafında bu bilgileri DTO'ya eklemeye gerek yoktur.
            var response = await _httpClient.PostAsJsonAsync("api/Notes", noteDto);
            response.EnsureSuccessStatusCode(); // Başarısız HTTP durum kodunda exception fırlatır
            return await response.Content.ReadFromJsonAsync<Note>();
        }
        
    }
}
