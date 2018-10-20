using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Lianliankan_SecondTry
{
    /// <summary>
    /// DifficultySettingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DifficultySettingWindow : Window
    {
        string str = "所选的行和列的乘积不能为奇数，那样的话最后就会剩下一个按钮找不到伴哦！";
        public DifficultySettingWindow()
        {
            InitializeComponent();
        }
    }
}
