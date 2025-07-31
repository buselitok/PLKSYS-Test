using PLKSYS.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLKSYS.Core.Interfaces
{
    public interface IWashingService
    {
        Task<WashingQueueEntry?> AddToWashingQueueAsync(WashingQueueEntryCreateDto createDto, string sentByUserName);
        Task<WashingQueueEntry?> MarkWashingCompletedAsync(Guid entryId, string completedByUserName);
        Task<IEnumerable<WashingQueueEntry>> GetPendingWashingEntriesAsync();
        Task<IEnumerable<WashingQueueEntry>> GetAllWashingEntriesAsync();
        Task<WashingQueueEntry?> GetWashingEntryByIdAsync(Guid entryId);
    }
}
