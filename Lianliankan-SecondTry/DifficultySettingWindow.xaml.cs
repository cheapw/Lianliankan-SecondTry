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
        #region 用户自定义方法
        private Button CreateButton(int row,int column)
        {
            Button button = new Button
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 45,
                Height = 35
            };
            Grid.SetColumn(button, column);
            Grid.SetRow(button, row);
            return button;
        }
        private void FillGrid(int rows,int columns)
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
        private void CreateSomeUI()
        {
            ShowPad.Children.Clear();
            ShowPad.RowDefinitions.Clear();
            ShowPad.ColumnDefinitions.Clear();
            #region 已弃用
            //StackPanel stackPanel01 = new StackPanel();
            ////stackPanel01.MouseEnter += ShowPad_MouseEnter;

            //TextBlock textBlock01 = new TextBlock();

            //textBlock01.Text = string.Format($"你的选择是：{(int)verticalSlider.Value}行 X {(int)horizontalSlider.Value}列 !");
            //textBlock01.Style= (Style)FindResource("textBlockStyle");

            //TextBlock textBlock02 = new TextBlock
            //{
            //    Text = "马上开始游戏吧!",
            //    Style = (Style)FindResource("textBlockStyle")
            //};

            //stackPanel01.Children.Add(textBlock01);
            //stackPanel01.Children.Add(textBlock02);

            //StackPanel stackPanel02 = new StackPanel
            //{
            //    Orientation = Orientation.Horizontal
            //};
            ////stackPanel02.MouseEnter += ShowPad_MouseEnter;

            //Button returnToMenu = new Button
            //{
            //    Content = "返回主菜单",
            //    Style = (Style)FindResource("buttonStyle")
            //};
            //Button startGame = new Button
            //{
            //    Content = "开始游戏",
            //    Style = (Style)FindResource("buttonStyle")
            //};
            //stackPanel02.Children.Add(returnToMenu);
            //stackPanel02.Children.Add(startGame);
            //stackPanel01.Children.Add(stackPanel02);
            //ShowPad.Children.Add(stackPanel01);
            #endregion

            for (int i = 0; i < 2; i++)
            {
                ColumnDefinition columnDefinition = new ColumnDefinition();
                ShowPad.ColumnDefinitions.Add(columnDefinition);
            }
            for (int i = 0; i < 3; i++)
            {
                RowDefinition rowDefinition = new RowDefinition();
                ShowPad.RowDefinitions.Add(rowDefinition);
            }

            TextBlock textBlock01 = new TextBlock();

            textBlock01.Text = string.Format($"你的选择是：{(int)verticalSlider.Value}行 X {(int)horizontalSlider.Value}列 !");
            textBlock01.Style = (Style)FindResource("textBlockStyle");
            Grid.SetColumn(textBlock01, 0);
            Grid.SetRow(textBlock01, 0);
            Grid.SetColumnSpan(textBlock01, 2);

            TextBlock textBlock02 = new TextBlock
            {
                Text = "马上开始游戏吧!",
                Style = (Style)FindResource("textBlockStyle")
            };
            Grid.SetColumn(textBlock02, 0);
            Grid.SetRow(textBlock02, 1);
            Grid.SetColumnSpan(textBlock01, 2);

            Button returnToMenu = new Button
            {
                Content = "返回主菜单",
                Style = (Style)FindResource("buttonStyle")
            };
            Grid.SetColumn(returnToMenu, 0);
            Grid.SetRow(returnToMenu, 2);
            returnToMenu.Click += ReturnToMenu_Click;

            Button startGame = new Button
            {
                Content = "开始游戏",
                Style = (Style)FindResource("buttonStyle")
            };
            Grid.SetColumn(startGame, 1);
            Grid.SetRow(startGame, 2);
            startGame.Click += StartGame_Click;

            ShowPad.Children.Add(textBlock01);
            ShowPad.Children.Add(textBlock02);
            ShowPad.Children.Add(returnToMenu);
            ShowPad.Children.Add(startGame);
        }

        #endregion
        public DifficultySettingWindow()
        {
            InitializeComponent();
            horizontalSlider.ValueChanged += slider1_ValueChanged;
            verticalSlider.ValueChanged += slider1_ValueChanged;
        }

        private void ReturnToMenu_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Show();
            this.Hide();
        }
        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            int userSetRows, userSetColumns, imageNumbers,timeAvailable;
            userSetRows = (int)verticalSlider.Value;
            userSetColumns = (int)horizontalSlider.Value;

            if (userSetRows * userSetColumns % 2 == 1)
            {
                MessageBox.Show("所选的行和列的乘积不能为奇数，那样的话最后就会剩下一个按钮找不到伴哦！");
                return;
            }

            timeAvailable = MainWindow.GetAvailableTime(userSetRows, userSetColumns);
            double width = 400, height = 400;
            width += (userSetColumns - 6) * 60;
            height += (userSetRows - 6) * 55;

            imageNumbers = MainWindow.GetImageNumbers(userSetRows, userSetColumns);

            GameWindow gameWindow = new GameWindow(userSetRows, userSetColumns, imageNumbers, timeAvailable, height, width);
            this.Hide();
            gameWindow.Show();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            FillGrid((int)verticalSlider.Value, (int)horizontalSlider.Value);
        }

        private void Slider_MouseLeave(object sender, MouseEventArgs e)
        {
            CreateSomeUI();
        }
    }
}
