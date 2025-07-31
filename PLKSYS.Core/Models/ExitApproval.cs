using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLKSYS.Core.Models
{
    class ExitApproval
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public int ApprovedByUserId { get; set; }
        public DateTime ApprovalTime { get; set; }
        public string? Note { get; set; }
        public bool IsApproved { get; set; }

        // Navigation properties
        public Vehicle Vehicle { get; set; }
        public User ApprovedBy { get; set; }
    }
}
