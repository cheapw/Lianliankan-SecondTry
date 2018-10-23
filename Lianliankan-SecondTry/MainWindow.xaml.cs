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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lianliankan_SecondTry
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //GameWindow gameWindow = null;
        DifficultySettingWindow difficultySettingWindow = null;
        #region 用户自定义方法
        internal static int GetImageNumbers(int rows,int column)
        {
            int imaNum = 0;
            // 以下三种情况根据下面的式子计算的值均为1，若图片数量为2会使游戏更丰富
            if ((rows==2&&column==2)||(rows==3&&column==2)||(rows==2&&column==3))
            {
                return 2;
            }
            imaNum = Convert.ToInt32(Math.Round(rows * column/2/2 * 0.9));
            if (imaNum < 1) imaNum = 1;
            return imaNum;
        }
        #endregion
        public MainWindow()
        {
            InitializeComponent();
            List<RankInfo> rankInfos = new List<RankInfo>
            {
                new RankInfo{Rank = 1,Mark = 3000,CompleteTime=DateTime.Now},
                new RankInfo{Rank=2,Mark=2800,CompleteTime=DateTime.Now.AddDays(10) },
                new RankInfo{Rank=3,Mark=500,CompleteTime=DateTime.MaxValue }
            };
            this.lvRank.ItemsSource = rankInfos;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // 简单难度默认值
            int userSetColumns = 8;
            int userSetRows = 6;
            int imageNumbers = 11;

            double width = 500, height = 500;
            // 设定中等难度
            if ((string)((sender as Button).Tag)=="1")
            {
                userSetRows = 7;
                userSetColumns = 10;
            }
            // 设定困难难度
            if ((string)((sender as Button).Tag) == "2")
            {
                userSetRows = 9;
                userSetColumns = 12;
            }
            // 自定义难度跳转到自定义窗口
            if ((string)((sender as Button).Tag) == "3")
            {
                if (difficultySettingWindow==null)
                {
                    difficultySettingWindow = new DifficultySettingWindow();
                }
                this.Hide();
                difficultySettingWindow.Show();
                return;
            }

            imageNumbers = GetImageNumbers(userSetRows, userSetColumns);

            width += (userSetColumns - 6) * 60;
            height += (userSetRows - 6) * 55;

            GameWindow gameWindow = new GameWindow(userSetRows, userSetColumns, imageNumbers, height, width);
            this.Hide();
            gameWindow.Show();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
    class RankInfo
    {
        public int Rank { get; set; }
        public int Mark { get; set; }
        public DateTime CompleteTime { get; set; }
    }
}
