using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloodyRewards.DB.Models
{
    public class DayliLoginTimeModel
    {
        public string player { get; set; } = string.Empty;
        public string LastDayConnection { get; set; } = string.Empty;
        public DateTime LastDateTime { get; set; }
    }
}
