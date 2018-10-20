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
        #region 用户自定义方法
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
            int userSetRows, userSetColumns, picNumber;
            userSetRows = userSetColumns = picNumber = 6;
            if ((string)((sender as Button).Tag)=="1")
            {
                userSetRows = 7;
                userSetColumns = 8;
                picNumber = 9;
            }
            if ((string)((sender as Button).Tag) == "2")
            {
                userSetRows = 8;
                userSetColumns = 9;
                picNumber = 9;
            }
            if ((string)((sender as Button).Tag) == "3")
            {
                DifficultySettingWindow difficultySettingWindow = new DifficultySettingWindow();
                this.Hide();
                difficultySettingWindow.Show();
                //return;
            }
            GameWindow gameWindow = new GameWindow(userSetRows,userSetColumns,picNumber);
            this.Hide();
            gameWindow.Show();
        }
    }
    class RankInfo
    {
        public int Rank { get; set; }
        public int Mark { get; set; }
        public DateTime CompleteTime { get; set; }
    }
}
