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
                Frame.Navigate(typeof(DifficultySettingPage));
                return;
            }

            Frame.Navigate(typeof(GameWindowPage), new GameParameters
            {
                UserSetRows = userSetRows,
                UserSetColumns = userSetColumns,
            });
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Exit();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            // Read data from a simple setting.
            Object value = localSettings.Values["TestSetting"];

            if (value == null)
            {
                txtOutput.Text = "no such key";
            }
            else
            {
                // Access data in value.
                txtOutput.Text = value as string;
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            // Create a simple setting.
            localSettings.Values["TestSetting"] = txtInput.Text;

        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            // Delete a simple setting.
            if (localSettings.Values.Remove("TestSetting"))
            {
                ContentDialog dialog = new ContentDialog();

                // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
                dialog.XamlRoot = this.XamlRoot;
                dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
                dialog.Title = "系统提示";
                dialog.Content = "清除成功！！";
                dialog.PrimaryButtonText = "确定";
                dialog.DefaultButton = ContentDialogButton.Primary;


                var _ =  dialog.ShowAsync();
            }

        }
    }
}
