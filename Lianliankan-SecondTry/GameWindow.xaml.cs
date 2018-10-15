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

            // 1.两个按钮处在同一行或同一列且相邻的话，直接返回true
            if (preRow==curRow&&Math.Abs(preColumn-curColumn)==1)
            {
                return true;
            }
            if (preColumn==curColumn&&Math.Abs(preRow-curRow)==1)
            {
                return true;
            }

            // 2.如果两个按钮处在同一行或同一列但不相邻

            // 2.1 处在同一行
            if (preRow==curRow)
            {
                // 如果先前的按钮比当前的按钮所处列的索引大
                if (preColumn>curColumn)
                {
                    // 验证两个按钮之间是否有阻挡
                    for (int i = 1; i < preColumn-curColumn; i++)
                    {
                        if (!availableChannels[preRow+1,i+curColumn+1])
                        {
                            break;
                        }
                        if (availableChannels[preRow + 1, i+ curColumn + 1]&&i+curColumn+1==preColumn)
                        {
                            return true;
                        }
                    }
                    // 验证两个按钮上方是否存在连通通道
                    for (int i = 0; i < preRow+1; i++)
                    {
                        if ((!availableChannels[preRow-i,preColumn+1])|| (!availableChannels[preRow-i, curColumn + 1]))
                        {
                            break;
                        }
                        else
                        {
                            for (int j = 0; j < preColumn - curColumn - 1; j++)
                            {
                                if (!availableChannels[preRow-i, j + curColumn + 2])
                                {
                                    break;
                                }
                                if (availableChannels[preRow -i, j + curColumn + 2] && j+1 + curColumn + 1 == preColumn)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    // 验证两个按钮下方是否存在连通通道
                    for (int i = preRow + 2; i < availableRows; i++)
                    {
                        if ((!availableChannels[i, preColumn + 1]) || (!availableChannels[i, curColumn + 1]))
                        {
                            break;
                        }
                        else
                        {
                            for (int j = 0; j < preColumn - curColumn - 1; j++)
                            {
                                if (!availableChannels[i, j + curColumn + 2])
                                {
                                    break;
                                }
                                if (availableChannels[i, j + curColumn + 2] && j+1 + curColumn + 1 == preColumn)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                // 如果先前的按钮比当前的按钮所处列的索引小
                if (preColumn < curColumn)
                {
                    // 验证两个按钮之间是否有阻挡
                    for (int i = 1; i < Math.Abs(preColumn - curColumn); i++)
                    {
                        if (!availableChannels[preRow + 1, i + preColumn + 1])
                        {
                            break;
                        }
                        if (availableChannels[preRow + 1, i + preColumn + 1] && i + preColumn + 1 == curColumn)
                        {
                            return true;
                        }
                    }
                    // 验证两个按钮上方是否存在连通通道
                    for (int i = 0; i < preRow + 1; i++)
                    {
                        if ((!availableChannels[preRow - i, preColumn + 1]) || (!availableChannels[preRow - i, curColumn + 1]))
                        {
                            break;
                        }
                        else
                        {
                            for (int j = 0; j < Math.Abs(preColumn - curColumn) - 1; j++)
                            {
                                if (!availableChannels[preRow - i, j + preColumn + 2])
                                {
                                    break;
                                }
                                if (availableChannels[preRow - i, j + preColumn + 2] && j + 1 + preColumn + 1 == curColumn)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    // 验证两个按钮下方是否存在连通通道
                    for (int i = preRow + 2; i < availableRows; i++)
                    {
                        if ((!availableChannels[i, preColumn + 1]) || (!availableChannels[i, curColumn + 1]))
                        {
                            break;
                        }
                        else
                        {
                            for (int j = 0; j < Math.Abs(preColumn - curColumn) - 1; j++)
                            {
                                if (!availableChannels[i, j + preColumn + 2])
                                {
                                    break;
                                }
                                if (availableChannels[i, j + preColumn + 2] && j + 1 + preColumn + 1 == curColumn)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            // 2.2 处在同一列
            if (preColumn == curColumn)
            {
                // 如果先前的按钮比当前的按钮所处行的索引大
                if (preRow > curRow)
                {
                    // 验证两个按钮之间是否有阻挡
                    for (int i = 1; i < preRow - curRow; i++)
                    {
                        if (!availableChannels[i + curRow + 1, preColumn + 1])
                        {
                            break;
                        }
                        if (availableChannels[i + curRow + 1, preColumn + 1] && i + curRow + 1 == preRow)
                        {
                            return true;
                        }
                    }
                    // 验证两个按钮左方是否存在连通通道
                    for (int i = 0; i < preColumn + 1; i++)
                    {
                        if ((!availableChannels[preRow+1, preColumn -i]) || (!availableChannels[curRow +1, curColumn -i]))
                        {
                            break;
                        }
                        else
                        {
                            for (int j = 0; j < preRow - curRow - 1; j++)
                            {
                                if (!availableChannels[j+curRow +2, curColumn -i])
                                {
                                    break;
                                }
                                if (availableChannels[j+curRow +2,curColumn -i] && j + 1 + curRow + 1 == preRow)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    // 验证两个按钮右方是否存在连通通道
                    for (int i = preColumn + 2; i < availableRows; i++)
                    {
                        if ((!availableChannels[preRow+1, i]) || (!availableChannels[curRow+1, i]))
                        {
                            break;
                        }
                        else
                        {
                            for (int j = 0; j < preRow - curRow - 1; j++)
                            {
                                if (!availableChannels[j+curRow+2, i])
                                {
                                    break;
                                }
                                if (availableChannels[j+curRow+2, i] && j + 1 + curRow + 1 == preRow)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                // 如果先前的按钮比当前的按钮所处行的索引小
                if (preRow < curRow)
                {
                    // 验证两个按钮之间是否有阻挡
                    for (int i = 1; i < Math.Abs(preRow - curRow); i++)
                    {
                        if (!availableChannels[i + preRow + 1, preColumn + 1])
                        {
                            break;
                        }
                        if (availableChannels[i + preRow + 1, preColumn + 1] && i + preRow + 1 == curRow)
                        {
                            return true;
                        }
                    }
                    // 验证两个按钮左方是否存在连通通道
                    for (int i = 0; i < preColumn + 1; i++)
                    {
                        if ((!availableChannels[preRow + 1, preColumn - i]) || (!availableChannels[curRow + 1, curColumn - i]))
                        {
                            break;
                        }
                        else
                        {
                            for (int j = 0; j < Math.Abs(preRow - curRow) - 1; j++)
                            {
                                if (!availableChannels[j + preRow + 2, curColumn - i])
                                {
                                    break;
                                }
                                if (availableChannels[j + preRow + 2, curColumn - i] && j + 1 + preRow + 1 == curRow)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    // 验证两个按钮右方是否存在连通通道
                    for (int i = preColumn + 2; i < availableRows; i++)
                    {
                        if ((!availableChannels[preRow + 1, i]) || (!availableChannels[curRow + 1, i]))
                        {
                            break;
                        }
                        else
                        {
                            for (int j = 0; j < Math.Abs(preRow - curRow) - 1; j++)
                            {
                                if (!availableChannels[j + preRow + 2, i])
                                {
                                    break;
                                }
                                if (availableChannels[j + preRow + 2, i] && j + 1 + preRow + 1 == curRow)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            // 3.处在四个按钮组成的正方形的对角线上
            if (Math.Abs(curColumn - preColumn) == 1 && Math.Abs(curRow - preRow) == 1)
            {
                if (availableChannels[curRow+1,preColumn+1]||availableChannels[preRow+1,curColumn+1])
                {
                    return true;
                }
            }
            
            // 4.两个按钮处在相邻的列且间隔的行数大于1，或者处在相邻的行且间隔的列数大于1

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
                        if (CheckIfMatch(previousClickedButton, button))
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
