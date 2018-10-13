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
    /// GameWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GameWindow : Window
    {
        List<ImageInfo> allImageInfoNeededRandomly = null;
        List<ImageInfo> allImageInfoNeeded = null;
        ImageInfo[,] imageInfoArray = null;
        bool isFirstButtonClicked = false;
        Button previousClickedButton = null;
        bool[,] availableChannels = null;

        #region 将图片随机添加到用户界面，并记录图片的位置
        private List<ImageInfo> GetImageInfoList()
        {
            List<ImageInfo> imageInfos = new List<ImageInfo>();
            int index = 1;
            
            while (true)
            {
                string resourceKey = "image" + index.ToString() + "_FullColor";
                // 尝试通过资源键查找资源，若返回null 就终止循环
                var image = (ImageBrush)TryFindResource(resourceKey);
                if (image == null) break;
                ImageInfo imageInfo = new ImageInfo
                {
                    Image = image,
                    Id = index
                };
                imageInfos.Add(imageInfo);
                index++;
            }
            return imageInfos;
        }
        private List<ImageInfo> GetDoubleImageInfoList(List<ImageInfo> imageInfos)
        {
            List<ImageInfo> doubleImageInfos = new List<ImageInfo>();
            for (int i = 0; i < imageInfos.Count; i++)
            {
                doubleImageInfos.Add(imageInfos[i]);
                doubleImageInfos.Add(imageInfos[i]);
            }
            return doubleImageInfos;
        }
        private List<int> GetRanomIndexList(int count)
        {
            List<int> randomIndexList = new List<int>();
            List<int> temp = new List<int>();
            Random rand = new Random();
            for (int i = 0; i < count; i++)
            {
                temp.Add(i);
            }
            int subtractIndex = count;
            for (int i = 0; i < count; i++)
            {
                int randNum=rand.Next(subtractIndex);
                randomIndexList.Add(temp[randNum]);
                temp.Remove(temp[randNum]);
                subtractIndex--;
            }
            return randomIndexList;
        }
        private List<Button> GetButtonList()
        {

            List<Button> buttons = new List<Button>();
            foreach (var item in (this.Content as Grid).Children)
            {
                buttons.Add((Button)item);
            }
            return buttons;
        }
        private void AddImageToUIRandomly(List<ImageInfo> doubleImageInfos,List<ImageInfo> imageInfos,Func<int,List<int>> GetRandomIndexListCallback,List<Button> buttons)
        {
            int buttonNum = buttons.Count;
            int addRounds = buttonNum / doubleImageInfos.Count;
            int imageLeftover = buttonNum % doubleImageInfos.Count;

            List<int> randLeftoverImageIndex = GetRandomIndexListCallback(imageInfos.Count);
            List<ImageInfo> LeftoverImageNeeded = new List<ImageInfo>();
            for (int i = 0; i < imageLeftover / 2; i++)
            {
                LeftoverImageNeeded.Add(imageInfos[randLeftoverImageIndex[i]]);
                LeftoverImageNeeded.Add(imageInfos[randLeftoverImageIndex[i]]);
            }

            allImageInfoNeededRandomly = new List<ImageInfo>();

            for (int i = 0; i < addRounds; i++)
            {
                for (int j = 0; j < doubleImageInfos.Count; j++)
                {
                    allImageInfoNeededRandomly.Add(doubleImageInfos[j]);
                }
            }

            for (int i = 0; i < LeftoverImageNeeded.Count; i++)
            {
                allImageInfoNeededRandomly.Add(LeftoverImageNeeded[i]);
            }

            List<int> allRandomIndex = GetRandomIndexListCallback(allImageInfoNeededRandomly.Count);
            allImageInfoNeeded = new List<ImageInfo>();
            for (int i = 0; i < allImageInfoNeededRandomly.Count; i++)
            {
                buttons[i].Background = allImageInfoNeededRandomly[allRandomIndex[i]].Image;
                allImageInfoNeededRandomly[allRandomIndex[i]]=SetLocation(buttons[i],allImageInfoNeededRandomly[allRandomIndex[i]]);
                allImageInfoNeeded.Add(SetLocation(buttons[i], allImageInfoNeededRandomly[allRandomIndex[i]]));
            }
        }
        private ImageInfo SetLocation(Button button,ImageInfo imageInfo)
        {
            imageInfo.Row = Grid.GetRow(button);
            imageInfo.Column = Grid.GetColumn(button);
            return imageInfo;
        }
        #endregion
        #region 配对消除
        private int GetMaxRow(List<Button> buttons)
        {
            return Grid.GetRow(buttons[buttons.Count - 1]);
        }
        private int GetMaxColumn(List<Button> buttons)
        {
            return Grid.GetColumn(buttons[buttons.Count - 1]);
        }
        private void FillImageInfoArray()
        {
            List<Button> buttons = GetButtonList();
            int maxRow = GetMaxRow(buttons);
            int maxColumn = GetMaxColumn(buttons);
            imageInfoArray = new ImageInfo[maxRow+1, maxColumn+1];
            int index = 0;
            for (int i = 0; i < maxRow+1; i++)
            {
                for (int j = 0; j < maxColumn+1; j++)
                {
                    imageInfoArray[i, j] = allImageInfoNeeded[index];
                    index++;
                }
            }
        }
        private int GetRow(Button button)
        {
            int row = 0;
            if (button.Parent is Grid)
            {
                row = Grid.GetRow(button);
            }
            else
            {
                throw new Exception("改按钮的容器控件不是Grid，无法获取其所在的行！");
            }
            return row;
        }
        private int GetColumn(Button button)
        {
            int column = 0;
            if (button.Parent is Grid)
            {
                column = Grid.GetColumn(button);
            }
            else
            {
                throw new Exception("改按钮的容器控件不是Grid，无法获取其所在的列！");
            }
            return column;
        }
        private void FillAvailableChannels()
        {
            List<Button> buttons = GetButtonList();
            int row = GetMaxRow(buttons) + 3;
            int column = GetMaxColumn(buttons) + 3;
            availableChannels = new bool[row, column];
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    if (i == 0 || i == row - 1 || j == 0 || j == column - 1)
                    {
                        availableChannels[i, j] = true;
                    }
                    else availableChannels[i, j] = false;
                }
            }
        }
        private bool CheckIfMatch(Button previousButton,Button currentButton)
        {
            bool judge = false;

            int preRow = GetRow(previousButton);
            int preColumn = GetColumn(previousButton);
            int curRow = GetRow(currentButton);
            int curColumn = GetColumn(currentButton);

            int availableRows = GetMaxRow(GetButtonList()) + 3;
            int availableColumns = GetMaxColumn(GetButtonList()) + 3;

            // 如果两个按钮处在同一行或同一列且相邻的话，直接返回true
            if (preRow==curRow&&Math.Abs(preColumn-curColumn)==1)
            {
                return true;
            }
            if (preColumn==curColumn&&Math.Abs(preRow-curRow)==1)
            {
                return true;
            }

            #region 弃用代码
            //if (preRow==curRow)
            //{
            //    if (Math.Abs(preColumn - curColumn)==1)
            //    {
            //        // 两个按钮相邻， 可以直接消除
            //        return true;
            //    }
            //    if (preColumn>curColumn)
            //    {
            //        for (int i = 0; i < availableRows; i++)
            //        {
            //            for (int j = 0; j < preColumn-curColumn+1; j++)
            //            {
            //                if (i== preRow+1&& j == 0)
            //                {
            //                    continue;
            //                }
            //                if (i==preRow+1&& j == preColumn - curColumn)
            //                {
            //                    return true;
            //                }
            //                if (j== preColumn - curColumn&& availableChannels[i, j + curColumn + 1])
            //                {
            //                    if (i < preRow + 1)
            //                    {
            //                        for (int k = 1; k < preRow + 1 - i; k++)
            //                        {
            //                            if ((!availableChannels[k, preColumn + 1]) || (!availableChannels[k,curRow+1]))
            //                            {
            //                                break;
            //                            }
            //                        }
            //                    }
            //                    if (i>preRow+1)
            //                    {
            //                        for (int i = 0; i < length; i++)
            //                        {

            //                        }
            //                    }
            //                    return true;
            //                }
            //                if (!availableChannels[i,j+curColumn+1])
            //                {
            //                    break;
            //                }
            //            }
            //        }
            //    }
            //    else
            //    {
            //        for (int i = 0; i < availableRows; i++)
            //        {
            //            for (int j = 0; j < curColumn - preColumn + 1; j++)
            //            {
            //                if (i == curRow + 1 && j == 0)
            //                {
            //                    continue;
            //                }
            //                if (i==curRow+1&&j==curColumn-preColumn)
            //                {
            //                    return true;
            //                }
            //                if (j == curColumn - preColumn && availableChannels[i, j + preColumn + 1])
            //                {
            //                    return true;
            //                }
            //                if (!availableChannels[i, j + preColumn + 1])
            //                {
            //                    break;
            //                }
            //            }
            //        }
            //    }
            //}
            //if (preColumn==curColumn)
            //{
            //    if (Math.Abs(preRow - curRow) == 1)
            //    {
            //        // 两个按钮相邻， 可以直接消除
            //        return true;
            //    }
            //    if (preRow > curRow)
            //    {
            //        for (int i = 0; i < availableColumns; i++)
            //        {
            //            for (int j = 0; j < preRow - curRow + 1; j++)
            //            {
            //                if (i == preColumn + 1 && j == 0)
            //                {
            //                    continue;
            //                }
            //                if (i==preColumn+1&&j==preRow-curRow)
            //                {
            //                    return true;
            //                }
            //                if (j == preRow - curRow && availableChannels[i, j + curRow + 1])
            //                {
            //                    return true;
            //                }
            //                if (!availableChannels[i, j + curRow + 1])
            //                {
            //                    break;
            //                }
            //            }
            //        }
            //    }
            //    else
            //    {
            //        for (int i = 0; i < availableColumns; i++)
            //        {
            //            for (int j = 0; j < curRow - preRow + 1; j++)
            //            {
            //                if (i == curColumn + 1 && (j == 0 || j == curRow - preRow))
            //                {
            //                    continue;
            //                }
            //                if (i==curColumn+1 && j==curRow-preRow)
            //                {
            //                    return true;
            //                }
            //                if (j == curRow - preRow && availableChannels[i, j + preRow + 1])
            //                {
            //                    return true;
            //                }
            //                if (!availableChannels[i, j + preRow + 1])
            //                {
            //                    break;
            //                }
            //            }
            //        }
            //    }
            //}
            #endregion
            return judge;
        }
        #endregion


        public GameWindow()
        {
            InitializeComponent();

            List<ImageInfo> imageInfos=GetImageInfoList();
            List<ImageInfo> doubleImageInfo = GetDoubleImageInfoList(imageInfos);
            List<Button> buttons = GetButtonList();
            AddImageToUIRandomly(doubleImageInfo, imageInfos, GetRanomIndexList, buttons);
            FillImageInfoArray();
            FillAvailableChannels();
            

            foreach (var item in buttons)
            {
                item.Click += Item_Click;
            }
            //StringBuilder stringBuilder = new StringBuilder();
            //foreach (var item in allImageInfoNeeded)
            //{
            //    stringBuilder.Append(item.Row.ToString() + "X" + item.Column.ToString() + "\n");
            //}
            //MessageBox.Show(stringBuilder.ToString());
        }

        private void Item_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (!isFirstButtonClicked)
            {
                isFirstButtonClicked = true;
                button.Background = new SolidColorBrush(Colors.Green);
                previousClickedButton = button;
            }
            else
            {
                if (button ==previousClickedButton)
                {
                    button.Background = new SolidColorBrush(Colors.Red);
                    previousClickedButton = null;
                    isFirstButtonClicked = false;
                }
                else
                {
                    ImageInfo previousImageInfo = imageInfoArray[GetRow(previousClickedButton), GetColumn(previousClickedButton)];
                    ImageInfo currentImageInfo = imageInfoArray[GetRow(button), GetColumn(button)];
                    if (previousImageInfo.Id==currentImageInfo.Id)
                    {
                        if (UnderSameLineOperate(previousClickedButton, button))
                        {
                            availableChannels[GetRow(previousClickedButton) + 1, GetColumn(previousClickedButton) + 1] = true;
                            availableChannels[GetRow(button) + 1, GetColumn(button) + 1] = true;

                            previousClickedButton.Background = new SolidColorBrush(Colors.Transparent);
                            button.Background = new SolidColorBrush(Colors.Transparent);
                            previousClickedButton.IsEnabled = false;
                            button.IsEnabled = false;
                            previousClickedButton = null;
                            isFirstButtonClicked = false;
                        }
                        


                        
                    }
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.MainWindow.Close();
        }
    }

    struct ImageInfo
    {
        public int Id { get; set; }
        public ImageBrush Image { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
    }
}
