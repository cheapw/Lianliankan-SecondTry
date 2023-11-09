using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lianliankan_WinUI.Models
{
    class RankInfo
    {
        public string Player { get; set; }

        public int Rank { get; set; }
        public int Score { get; set; }
        public string LevelDescription { get; set; }
        public int AvailableTime { get; set; }
        public int ActualTime { get; set; }
        public int UserSetRow { get; set; }
        public int UserSetColumn { get; set; }
        public GameLevels GameLevel { get; set; }
        public DateTime PlayTime { get; set; }
    }
}
