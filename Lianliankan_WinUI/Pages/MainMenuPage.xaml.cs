using Lianliankan_WinUI.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Lianliankan_WinUI.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainMenuPage : Page
    {
        DifficultySettingPage difficultySettingPage = null;
        #region 自定义方法
        internal static int GetImageNumbers(int rows, int column)
        {
            int imaNum = 0;
            // 以下三种情况根据下面的式子计算的值均为1，若图片数量为2会使游戏更丰富
            if ((rows == 2 && column == 2) || (rows == 3 && column == 2) || (rows == 2 && column == 3))
            {
                return 2;
            }
            imaNum = Convert.ToInt32(Math.Round(rows * column / 2 / 2 * 0.9));
            if (imaNum < 1) imaNum = 1;
            return imaNum;
        }
        internal static int GetAvailableTime(int rows, int column)
        {
            int product = rows * column;
            int timeAvailable = Convert.ToInt32(Math.Round((double)product / 15));
            return timeAvailable;
        }
        #endregion
        public MainMenuPage()
        {
            this.InitializeComponent();
            List<RankInfo> rankInfos = new List<RankInfo>
            {
                new RankInfo{Rank = 1,Mark = 3000,CompleteTime=DateTime.Now},
                new RankInfo{Rank=2,Mark=2800,CompleteTime=DateTime.Now.AddDays(10) },
                new RankInfo{Rank=3,Mark=500,CompleteTime=DateTime.MaxValue }
            };
            this.dgRank.ItemsSource = rankInfos;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // 简单难度默认值
            int userSetColumns = 8;
            int userSetRows = 6;
            int imageNumbers = 11;
            int timeAvailable = 3;

            double width = 500, height = 500;
            // 设定中等难度
            if ((string)((sender as Button).Tag) == "1")
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
                if (difficultySettingPage == null)
                {
                    difficultySettingPage = new DifficultySettingPage();
                }
                Frame.Navigate(typeof(DifficultySettingPage));
                return;
            }

            imageNumbers = GetImageNumbers(userSetRows, userSetColumns);
            timeAvailable = GetAvailableTime(userSetRows, userSetColumns);

            width += (userSetColumns - 6) * 60;
            height += (userSetRows - 6) * 55;


            Frame.Navigate(typeof(GameWindowPage), new GameParameters
            {
                UserSetRows = userSetRows,
                UserSetColumns = userSetColumns,
                ImageNumbers = imageNumbers,
                TimeAvailable = timeAvailable,
                Height = height,
                Width = width
            });
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Exit();
        }
    }
}
