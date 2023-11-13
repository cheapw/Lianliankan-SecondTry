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
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Lianliankan_WinUI.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DifficultySettingPage : Page
    {
        public DifficultySettingPage()
        {
            this.InitializeComponent();
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainMenuPage));
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if(verticalSlider!=null && horizontalSlider != null && ShowPad!=null)
            {
                this.ShowPad.Visibility = Visibility.Visible;
                this.OperationPad.Visibility = Visibility.Collapsed;

                FillGridNew((int)verticalSlider.Value, (int)horizontalSlider.Value);
                //Task.Run(() =>
                //{
                //    this.DispatcherQueue.TryEnqueue(() =>
                //    {
                //        FillGridNew((int)verticalSlider.Value, (int)horizontalSlider.Value);
                //    });
                //});
            }
        }

        private void Slider_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            this.ShowPad.Visibility = Visibility.Collapsed;
            this.OperationPad.Visibility = Visibility.Visible;

            var resourceLoader = new Microsoft.Windows.ApplicationModel.Resources.ResourceLoader();
            var gameCustomHint = resourceLoader.GetString("GameCustomHintText");
            var row = resourceLoader.GetString("Row");
            var column = resourceLoader.GetString("Column");
            var rowColumnLinkText = resourceLoader.GetString("RowColumnLinkText");

            this.TbkGameDifficulty.Text = $"{gameCustomHint} {(int)verticalSlider.Value} {row} {rowColumnLinkText} {(int)horizontalSlider.Value} {column}!";
        }

        private async void StartGame_Click(object sender, RoutedEventArgs e)
        {
            int userSetRows, userSetColumns;
            userSetRows = (int)verticalSlider.Value;
            userSetColumns = (int)horizontalSlider.Value;

            if (userSetRows * userSetColumns % 2 == 1)
            {
                var resourceLoader = new Microsoft.Windows.ApplicationModel.Resources.ResourceLoader();
                var systemPrompts = resourceLoader.GetString("SystemPrompts");
                var confirm = resourceLoader.GetString("Confirm");
                var customSettingAlert = resourceLoader.GetString("CustomSettingAlert");

                ContentDialog dialog = new ContentDialog();

                // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
                dialog.XamlRoot = this.XamlRoot;
                dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
                dialog.Title = systemPrompts;
                dialog.Content = customSettingAlert;
                dialog.PrimaryButtonText = confirm;
                dialog.DefaultButton = ContentDialogButton.Primary;

                var _ = await dialog.ShowAsync();
                return;
            }

            Frame.Navigate(typeof(GameWindowPage),new GameOptions() {UserSetRows = userSetRows, UserSetColumns= userSetColumns, GameLevels = GameLevels.Custom });
        }
        private void ReturnToMenu_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainMenuPage));
        }

        #region 用户自定义方法
        private Button CreateButton(int row, int column)
        {
            Button button = new Button
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 45,
                Height = 35,
                Tag = $"{row+1},{column+1}"
            };
            //button.Style=this.Resources.
            Grid.SetColumn(button, column);
            Grid.SetRow(button, row);
            return button;
        }

        private void FillGridNew(int rows, int columns)
        {
            if (ShowPad.RowDefinitions.Count == rows && ShowPad.ColumnDefinitions.Count == columns) return;
            else if(ShowPad.RowDefinitions.Count == rows && ShowPad.ColumnDefinitions.Count != columns)
            {
                if (ShowPad.ColumnDefinitions.Count > columns)
                {
                    int removeCount = ShowPad.ColumnDefinitions.Count - columns;
                    while (removeCount > 0)
                    {
                        var buttons = ShowPad.Children.Where(x => (x as Button).Tag.ToString().Contains("," + ShowPad.ColumnDefinitions.Count));
                        foreach (var button in buttons) ShowPad.Children.Remove(button);

                        ShowPad.ColumnDefinitions.RemoveAt(ShowPad.ColumnDefinitions.Count - 1);

                        removeCount--;
                    }
                }
                else
                {
                    int addCount = columns-ShowPad.ColumnDefinitions.Count;
                    while(addCount > 0)
                    {
                        ColumnDefinition columnDefinition = new ColumnDefinition();
                        columnDefinition.Width = GridLength.Auto;
                        ShowPad.ColumnDefinitions.Add(columnDefinition);

                        for (int i = 0; i < ShowPad.RowDefinitions.Count; i++)
                        {
                            ShowPad.Children.Add(CreateButton(i, ShowPad.ColumnDefinitions.Count - 1));
                        }
                        addCount--;
                    }
                }
            }
            else if(ShowPad.RowDefinitions.Count != rows && ShowPad.ColumnDefinitions.Count == columns)
            {
                if(ShowPad.RowDefinitions.Count> rows)
                {
                    int removeCount = ShowPad.RowDefinitions.Count - rows;
                    while (removeCount > 0)
                    {
                        var buttons = ShowPad.Children.Where(x => (x as Button).Tag.ToString().Contains(ShowPad.RowDefinitions.Count + ","));
                        foreach (var button in buttons) ShowPad.Children.Remove(button);

                        ShowPad.RowDefinitions.RemoveAt(ShowPad.RowDefinitions.Count - 1);

                        removeCount--;
                    }
                }
                else
                {
                    int addCount = rows - ShowPad.RowDefinitions.Count;
                    while (addCount > 0)
                    {
                        RowDefinition rowDefinition = new RowDefinition();
                        rowDefinition.Height = GridLength.Auto;
                        ShowPad.RowDefinitions.Add(rowDefinition);

                        for (int i = 0; i < ShowPad.ColumnDefinitions.Count; i++)
                        {
                            ShowPad.Children.Add(CreateButton(ShowPad.RowDefinitions.Count - 1,i));
                        }
                        addCount--;
                    }
                }
            }
            else
            {
                FillGrid(rows, columns);
            }
        }

        private void FillGrid(int rows, int columns)
        {
            ShowPad.Children.Clear();
            ShowPad.RowDefinitions.Clear();
            ShowPad.ColumnDefinitions.Clear();
            for (int i = 0; i < rows; i++)
            {
                RowDefinition rowDefinition = new RowDefinition();
                rowDefinition.Height = GridLength.Auto;
                ShowPad.RowDefinitions.Add(rowDefinition);
            }
            for (int i = 0; i < columns; i++)
            {
                ColumnDefinition columnDefinition = new ColumnDefinition();
                columnDefinition.Width = GridLength.Auto;
                ShowPad.ColumnDefinitions.Add(columnDefinition);
            }
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    ShowPad.Children.Add(CreateButton(i, j));
                }
            }
        }
        #endregion
    }
}
