using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lianliankan_WinUI.Models
{
    public partial class AppData:ObservableObject
    {
        [ObservableProperty]
        public partial List<RankInfo> RankInfoes { get; set; } = new List<RankInfo>();
    }
}
