using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lianliankan_WinUI.Models
{
    class RankInfo
    {
        public string UserName { get; set; }

        public int Rank { get; set; }
        public int Mark { get; set; }
        public string Difficulty { get; set; }
        public int ActualTime { get; set; }
        public int RemaindTime { get; set; }
        public int UserSetRow { get; set; }
        public int UserSetColumn { get; set; }
        public DateTime CompleteTime { get; set; }
    }
}
