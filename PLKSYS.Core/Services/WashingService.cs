using Microsoft.EntityFrameworkCore;
using PLKSYS.Core.Data;
using PLKSYS.Core.Interfaces;
using PLKSYS.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLKSYS.Core.Services
{
    public class WashingService : IWashingService
    {
        private readonly PLKSYSContext _context;

        public WashingService(PLKSYSContext context)
        {
            _context = context;
        }

        public async Task<WashingQueueEntry?> AddToWashingQueueAsync(WashingQueueEntryCreateDto createDto, string sentByUserName)
        {
            // Plaka zaten bekleyenler arasında mı kontrol et
            // YENİ KONTROL: Araç plazada mı ve mevcut mu kontrol et
            var vehicle = await _context.Vehicles
                                        .FirstOrDefaultAsync(v => v.PlateNumber == createDto.PlateNumber && v.CurrentStatus == "in");

            if (vehicle == null)
            {
                throw new InvalidOperationException($"Plaka numarası '{createDto.PlateNumber}' plazada bulunamadı veya durumu 'in' değil.");
            }

            // Araç zaten yıkama kuyruğunda mı kontrol et (Status = "Pending" veya "In Progress" gibi)
            var existingEntry = await _context.WashingQueueEntries
                                              .FirstOrDefaultAsync(e => e.PlateNumber == createDto.PlateNumber && e.Status == "Beklemede");

            if (existingEntry != null)
            {
                throw new InvalidOperationException($"Plaka numarası '{createDto.PlateNumber}' zaten yıkama kuyruğunda.");
            }

            var entry = new WashingQueueEntry
            {
                PlateNumber = createDto.PlateNumber.ToUpperInvariant(), // Plakayı büyük harfe dönüştür
                SentToWashingTime = DateTime.UtcNow,
                
                SentByUserName = sentByUserName,
                Status = "Beklemede"
            };

            _context.WashingQueueEntries.Add(entry);
            await _context.SaveChangesAsync();
            return entry;
        }

        public async Task<WashingQueueEntry?> MarkWashingCompletedAsync(Guid entryId, string completedByUserName)
        {
            var entry = await _context.WashingQueueEntries.FindAsync(entryId);
            if (entry == null || entry.Status == "Tamamlandı")
            {
                return null; // Giriş bulunamadı veya zaten tamamlanmış
            }

            entry.WashingCompletedTime = DateTime.UtcNow;
            entry.CompletedByUserName = completedByUserName;
            
            entry.Status = "Tamamlandı";

            _context.WashingQueueEntries.Update(entry);
            await _context.SaveChangesAsync();
            return entry;
        }

        public async Task<IEnumerable<WashingQueueEntry>> GetPendingWashingEntriesAsync()
        {
            return await _context.WashingQueueEntries
                                 .Where(e => e.Status == "Beklemede")
                                 .OrderBy(e => e.SentToWashingTime)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<WashingQueueEntry>> GetAllWashingEntriesAsync()
        {
            return await _context.WashingQueueEntries.OrderByDescending(e => e.SentToWashingTime).ToListAsync();
        }

        public async Task<WashingQueueEntry?> GetWashingEntryByIdAsync(Guid entryId)
        {
            return await _context.WashingQueueEntries.FindAsync(entryId);
        }

        
    }
}
