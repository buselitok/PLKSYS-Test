using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLKSYS.Core.Models
{
    public class ClaimRequest
    {
        public class ClaimVehicleRequest
        {
            public string PlateNumber { get; set; } = string.Empty;
            public int UserId { get; set; }
            public string UserName { get; set; } = string.Empty;
        }

        public class UnclaimVehicleRequest
        {
            public string PlateNumber { get; set; } = string.Empty;
            public int UserId { get; set; }
        }
    }
}
