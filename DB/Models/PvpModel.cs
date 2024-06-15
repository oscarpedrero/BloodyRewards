using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloodyRewards.DB.Models
{
    public class PvpModel
    {
        public string killer { get; set; }
        public string target { get; set; }

        public DateTime date { get; set; }
    }
}
