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
        #region 公共属性，用户传来的一些游戏初始设置
        public int UserSetRows { get; set; }
        public int UserSetColumns { get; set; }
        public int ImageNumbers { get; set; }
        #endregion

        #region 私有字段
        // 一维数组，所有所需的图片，相同的图片可能有数张，数量为偶数，图片总数与按钮总数相同，且按照添加进按钮的顺序排列
        private List<ImageInfo> allImageInfoNeeded = null;
        // 二维数组，记录图片在按钮上显示的位置
        private ImageInfo[,] imageInfoArray = null;
        // 二维数组，记录按钮位置索引，以方便查找
        private Button[,] buttonArray = null;
        // 判断第一个按钮是否按下
        private bool isFirstButtonClicked = false;
        // 记录第一个按下的按钮，以便与第二个按下的按钮进行配对
        private Button previousClickedButton = null;
        // 可用通道，比整个按钮组成的方阵大一圈，该二维数组内部的元素是布尔类型，据此判断两个按钮是否可以消除，
        // 外圈全部为true，有按钮占据的地方全部为false，按钮被消除后相应位置变为true
        private bool[,] availableChannels = null;
        #endregion

        #region 根据用户传来的一些数据进行初始设定，例如Grid 网格的行和列，按钮的个数等
        private void SetGridRows()
        {
            gamePanel.RowDefinitions.Clear();
            for (int i = 0; i < UserSetRows; i++)
            {
                RowDefinition row = new RowDefinition();
                gamePanel.RowDefinitions.Add(row);
            }
        }
        private void SetGridColumns()
        {
            gamePanel.ColumnDefinitions.Clear();
            for (int i = 0; i < UserSetColumns; i++)
            {
                ColumnDefinition column = new ColumnDefinition();
                gamePanel.ColumnDefinitions.Add(column);
            }
        }
        private void AddAndSetButtonPosition()
        {
            gamePanel.Children.Clear();
            for (int i = 0; i < UserSetRows; i++)
            {
                for (int j = 0; j < UserSetColumns; j++)
                {
                    Button button = new Button();
                    button.BorderThickness = new Thickness(0);
                    Grid.SetRow(button, i);
                    Grid.SetColumn(button, j);
                    gamePanel.Children.Add(button);
                }
            }
        }
        #endregion

        #region 将图片随机添加到用户界面，并记录图片的位置
        /// <summary>
        /// 获取图片信息列表，所有的图片均不相同
        /// </summary>
        /// <returns>图片信息列表</returns>
        private List<ImageInfo> GetImageInfoList()
        {
            List<ImageInfo> imageInfos = new List<ImageInfo>();
            int index = 1;
            
            while (index<=ImageNumbers)
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
        /// <summary>
        /// 将图片信息列表加倍，每一个元素后面的位置加上相同的元素
        /// </summary>
        /// <param name="imageInfos"></param>
        /// <returns>加倍后的图片信息列表</returns>
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
        /// <summary>
        /// 获取一个整数类型的随机索引列表，列表内索引元素不重复
        /// </summary>
        /// <param name="count">数量，即需要随机重新排序的列表的元素个数</param>
        /// <returns>随机索引列表</returns>
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
        /// <summary>
        /// 获取按钮列表，仅限进行消除的按钮，不包括其他功能按钮
        /// </summary>
        /// <returns>按钮列表</returns>
        private List<Button> GetButtonList()
        {

            List<Button> buttons = new List<Button>();
            foreach (var item in this.gamePanel.Children)
            {
                buttons.Add((Button)item);
            }
            return buttons;
        }
        /// <summary>
        /// 将图片随机添加到UI按钮上
        /// </summary>
        /// <param name="doubleImageInfos">加倍后的图片信息列表</param>
        /// <param name="imageInfos">图片信息列表</param>
        /// <param name="GetRandomIndexListCallback">回调方法，传入获取随机索引列表的方法</param>
        /// <param name="buttons">按钮列表</param>
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

            List<ImageInfo> allImageInfoNeededRandomly = new List<ImageInfo>();

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
                allImageInfoNeeded.Add(allImageInfoNeededRandomly[allRandomIndex[i]]);
            }
        }
        /// <summary>
        /// 在将图片添加到 UI 上时，记录按钮所处 Grid 布局的行和列，用 ImageInfo 的 Row 和 Column 属性进行存储
        /// </summary>
        /// <param name="button">当前按钮</param>
        /// <param name="imageInfo">当前图片信息</param>
        /// <returns>返回一个图片信息结构</returns>
        private ImageInfo SetLocation(Button button,ImageInfo imageInfo)
        {
            imageInfo.Row = Grid.GetRow(button);
            imageInfo.Column = Grid.GetColumn(button);
            return imageInfo;
        }
        #endregion

        #region 配对消除
        /// <summary>
        /// 获取按钮最大行的索引
        /// </summary>
        /// <param name="buttons">按钮最大行的索引</param>
        /// <returns></returns>
        private int GetMaxRowIndex(List<Button> buttons)
        {
            return Grid.GetRow(buttons[buttons.Count - 1]);
        }
        /// <summary>
        /// 获取按钮最高列的索引
        /// </summary>
        /// <param name="buttons">按钮最高列的索引</param>
        /// <returns></returns>
        private int GetMaxColumnIndex(List<Button> buttons)
        {
            return Grid.GetColumn(buttons[buttons.Count - 1]);
        }
        /// <summary>
        /// 填充记录图片在按钮上显示的位置的二维数组
        /// </summary>
        private void FillImageInfoArray()
        {
            List<Button> buttons = GetButtonList();
            int maxRow = GetMaxRowIndex(buttons);
            int maxColumn = GetMaxColumnIndex(buttons);
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
        private void FillButtonArray()
        {
            List<Button> buttons = GetButtonList();
            int maxRow = GetMaxRowIndex(buttons);
            int maxColumn = GetMaxColumnIndex(buttons);
            buttonArray = new Button[maxRow + 1, maxColumn + 1];
            int index = 0;
            for (int i = 0; i < maxRow + 1; i++)
            {
                for (int j = 0; j < maxColumn + 1; j++)
                {
                    buttonArray[i, j] = buttons[index];
                    index++;
                }
            }
        }
        /// <summary>
        /// 获取给定按钮的行索引
        /// </summary>
        /// <param name="button">给定按钮的行索引</param>
        /// <returns></returns>
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
        /// <summary>
        /// 获取给定按钮的列索引
        /// </summary>
        /// <param name="button">给定按钮的列索引</param>
        /// <returns></returns>
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
        /// <summary>
        /// 填充可用通道，外圈为true，内部为false
        /// </summary>
        private void FillAvailableChannels()
        {
            List<Button> buttons = GetButtonList();
            int row = GetMaxRowIndex(buttons) + 3;
            int column = GetMaxColumnIndex(buttons) + 3;
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
        /// <summary>
        /// 获取可用通道的行数
        /// </summary>
        /// <returns></returns>
        private int GetAvailableRows()
        {
            return GetMaxRowIndex(GetButtonList()) + 3;
        }
        /// <summary>
        /// 获取可用通道的列数
        /// </summary>
        /// <returns></returns>
        private int GetAvailableColumns()
        {
            return GetMaxColumnIndex(GetButtonList()) + 3;
        }
        /// <summary>
        /// 判断两个按钮是否能够消除，大量的代码在此方法中，可以进行一些整合，但会使代码更复杂
        /// </summary>
        /// <param name="previousButton">第一个按钮</param>
        /// <param name="currentButton">第二个按钮</param>
        /// <returns></returns>
        private bool CheckIfMatch(Button previousButton,Button currentButton)
        {
            bool judge = false;

            // 行或列，从零开始，故其最大值为一行或一列上按钮总数减1
            int preRow = GetRow(previousButton);
            int preColumn = GetColumn(previousButton);
            int curRow = GetRow(currentButton);
            int curColumn = GetColumn(currentButton);

            // 可用的行或列，AvailableChannels方阵比所有按钮组成的方阵大一圈，故左右各加1，
            // 由于GetMaxRow方法获取的只是最大的索引，故再加1，总体加3
            int availableRows = GetMaxRowIndex(GetButtonList()) + 3;
            int availableColumns = GetMaxColumnIndex(GetButtonList()) + 3;

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
                    for (int i = preColumn + 2; i < availableColumns; i++)
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
                    for (int i = preColumn + 2; i < availableColumns; i++)
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
            // 4.1 两个按钮处在相邻的列且间隔的行数大于1，以当前的按钮为基准，分四种情况，代表了两个按钮的相对位置，右下，右上，左下，左上
            // 4.1.1 先前点击的按钮在当前按钮的右下方
            if (preColumn - curColumn == 1 && preRow - curRow > 1)
            {
                // 尝试在两按钮右边查找连通通道
                if (availableChannels[curRow + 1, preColumn + 1])
                {
                    for (int i = 0; i < preRow - curRow - 1; i++)
                    {
                        if (availableChannels[i + curRow + 2, preColumn + 1] && i + curRow + 2 == preRow)
                        {
                            return true;
                        }
                        if (!availableChannels[i + curRow + 2, preColumn + 1])
                        {
                            break;
                        }
                    }
                    // 继续向右
                    for (int i = preColumn + 2; i < availableColumns; i++)
                    {
                        if ((!availableChannels[preRow + 1, i]) || (!availableChannels[curRow + 1, i]))
                        {
                            break;
                        }
                        else
                        {
                            for (int j = 0; j < preRow - curRow - 1; j++)
                            {
                                if (!availableChannels[j + curRow + 2, i])
                                {
                                    break;
                                }
                                if (availableChannels[j + curRow + 2, i] && j + 1 + curRow + 1 == preRow)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                // 尝试在两按钮左边查找连通通道
                if (availableChannels[preRow + 1, curColumn + 1])
                {
                    for (int i = 0; i < preRow - curRow - 1; i++)
                    {
                        if (availableChannels[preRow - i, curColumn + 1] && preRow - i == curRow + 2)
                        {
                            return true;
                        }
                        if (!availableChannels[preRow - i, curColumn + 1])
                        {
                            break;
                        }
                    }
                    // 继续向左
                    for (int i = 0; i < curColumn + 1; i++)
                    {
                        if ((!availableChannels[preRow + 1, preColumn - i - 1]) || (!availableChannels[curRow + 1, curColumn - i]))
                        {
                            break;
                        }
                        else
                        {
                            for (int j = 0; j < preRow - curRow - 1; j++)
                            {
                                if (!availableChannels[j + curRow + 2, curColumn - i])
                                {
                                    break;
                                }
                                if (availableChannels[j + curRow + 2, curColumn - i] && j + 1 + curRow + 1 == preRow)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }

                //尝试在两个按钮构成的矩形查找通道
                for (int i = 0; i < preRow - curRow - 1; i++)
                {
                    if (availableChannels[curRow + 2 + i, curColumn + 1])
                    {
                        for (int j = 0; j < preRow - curRow - 1 - i; j++)
                        {
                            if (!availableChannels[curRow + 2 + j + i, preColumn + 1])
                            {
                                break;
                            }
                            if (availableChannels[curRow + 2 + j + i, preColumn + 1] && curRow + 2 + j + i == preRow)
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            // 4.1.2 先前点击的按钮在当前按钮的左上方
            if (curColumn - preColumn == 1 && curRow - preRow > 1)
            {
                // 尝试在两按钮右边查找连通通道
                if (availableChannels[preRow + 1, curColumn + 1])
                {
                    for (int i = 0; i < Math.Abs(preRow - curRow) - 1; i++)
                    {
                        if (availableChannels[i + preRow + 2, curColumn + 1] && i + preRow + 2 == curRow)
                        {
                            return true;
                        }
                        if (!availableChannels[i + preRow + 2, curColumn + 1])
                        {
                            break;
                        }
                    }
                    // 继续向右
                    for (int i = curColumn + 2; i < availableColumns; i++)
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
                // 尝试在两按钮左边查找连通通道
                if (availableChannels[curRow + 1, preColumn + 1])
                {
                    for (int i = 0; i < Math.Abs(preRow - curRow) - 1; i++)
                    {
                        if (availableChannels[curRow - i, preColumn + 1] && curRow - i == preRow + 2)
                        {
                            return true;
                        }
                        if (!availableChannels[curRow - i, preColumn + 1])
                        {
                            break;
                        }
                    }
                    // 继续向左
                    for (int i = 0; i < preColumn + 1; i++)
                    {
                        if ((!availableChannels[preRow + 1, curColumn - i - 1]) || (!availableChannels[curRow + 1, preColumn - i]))
                        {
                            break;
                        }
                        else
                        {
                            for (int j = 0; j < Math.Abs(preRow - curRow) - 1; j++)
                            {
                                if (!availableChannels[j + preRow + 2, preColumn - i])
                                {
                                    break;
                                }
                                if (availableChannels[j + preRow + 2, preColumn - i] && j + 1 + preRow + 1 == curRow)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }

                //尝试在两个按钮构成的矩形查找通道
                for (int i = 0; i < Math.Abs(preRow - curRow) - 1; i++)
                {
                    if (availableChannels[preRow + 2 + i, preColumn + 1])
                    {
                        for (int j = 0; j < Math.Abs(preRow - curRow) - 1 - i; j++)
                        {
                            if (!availableChannels[preRow + 2 + j + i, curColumn + 1])
                            {
                                break;
                            }
                            if (availableChannels[preRow + 2 + j + i, curColumn + 1] && preRow + 2 + j + i == curRow)
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            // 4.1.3 先前点击的按钮在当前按钮的右上方
            if (preColumn - curColumn == 1 && curRow - preRow > 1)
            {
                // 尝试在两按钮右边查找连通通道
                if (availableChannels[curRow + 1, preColumn + 1])
                {
                    for (int i = 0; i < curRow - preRow - 1; i++)
                    {
                        if (availableChannels[i + preRow + 2, preColumn + 1] && i + preRow + 2 == curRow)
                        {
                            return true;
                        }
                        if (!availableChannels[i + preRow + 2, preColumn + 1])
                        {
                            break;
                        }
                    }
                    // 继续向右
                    for (int i = preColumn + 2; i < availableColumns; i++)
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
                // 尝试在两按钮左边查找连通通道
                if (availableChannels[preRow + 1, curColumn + 1])
                {
                    for (int i = 0; i < Math.Abs(preRow - curRow) - 1; i++)
                    {
                        if (availableChannels[curRow - i, curColumn + 1] && curRow - i == preRow + 2)
                        {
                            return true;
                        }
                        if (!availableChannels[curRow - i, curColumn + 1])
                        {
                            break;
                        }
                    }
                    // 继续向左
                    for (int i = 0; i < curColumn + 1; i++)
                    {
                        if ((!availableChannels[preRow + 1, preColumn - i - 1]) || (!availableChannels[curRow + 1, curColumn - i]))
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
                }

                //尝试在两个按钮构成的矩形查找通道
                for (int i = 0; i < Math.Abs(preRow - curRow) - 1; i++)
                {
                    if (availableChannels[preRow + 2 + i, preColumn + 1])
                    {
                        for (int j = 0; j < Math.Abs(preRow - curRow) - 1 - i; j++)
                        {
                            if (!availableChannels[preRow + 2 + j + i, curColumn + 1])
                            {
                                break;
                            }
                            if (availableChannels[preRow + 2 + j + i, curColumn + 1] && preRow + 2 + j + i == curRow)
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            // 4.1.4 先前点击的按钮在当前按钮的左下方
            if (curColumn - preColumn == 1 && preRow - curRow > 1)
            {
                // 尝试在两按钮右边查找连通通道
                if (availableChannels[preRow + 1, curColumn + 1])
                {
                    for (int i = 0; i < Math.Abs(curRow - preRow) - 1; i++)
                    {
                        if (availableChannels[i + curRow + 2, curColumn + 1] && i + curRow + 2 == preRow)
                        {
                            return true;
                        }
                        if (!availableChannels[i + curRow + 2, curColumn + 1])
                        {
                            break;
                        }
                    }
                    // 继续向右
                    for (int i = curColumn + 2; i < availableColumns; i++)
                    {
                        if ((!availableChannels[preRow + 1, i]) || (!availableChannels[curRow + 1, i]))
                        {
                            break;
                        }
                        else
                        {
                            for (int j = 0; j < preRow - curRow - 1; j++)
                            {
                                if (!availableChannels[j + curRow + 2, i])
                                {
                                    break;
                                }
                                if (availableChannels[j + curRow + 2, i] && j + 1 + curRow + 1 == preRow)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                // 尝试在两按钮左边查找连通通道
                if (availableChannels[curRow + 1, preColumn + 1])
                {
                    for (int i = 0; i < preRow - curRow - 1; i++)
                    {
                        if (availableChannels[preRow - i, preColumn + 1] && preRow - i == curRow + 2)
                        {
                            return true;
                        }
                        if (!availableChannels[preRow - i, preColumn + 1])
                        {
                            break;
                        }
                    }
                    // 继续向左
                    for (int i = 0; i < preColumn + 1; i++)
                    {
                        if ((!availableChannels[preRow + 1, preColumn - i]) || (!availableChannels[curRow + 1, curColumn - i - 1]))
                        {
                            break;
                        }
                        else
                        {
                            for (int j = 0; j < preRow - curRow - 1; j++)
                            {
                                if (!availableChannels[j + curRow + 2, preColumn - i])
                                {
                                    break;
                                }
                                if (availableChannels[j + curRow + 2, preColumn - i] && j + 1 + curRow + 1 == preRow)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                //尝试在两个按钮构成的矩形查找通道
                for (int i = 0; i < preRow - curRow - 1; i++)
                {
                    if (availableChannels[curRow + 2 + i, curColumn + 1])
                    {
                        for (int j = 0; j < preRow - curRow - 1 - i; j++)
                        {
                            if (!availableChannels[curRow + 2 + j + i, preColumn + 1])
                            {
                                break;
                            }
                            if (availableChannels[curRow + 2 + j + i, preColumn + 1] && curRow + 2 + j + i == preRow)
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // 4.2 两个按钮处在相邻的行且间隔的列数大于1，以当前的按钮为基准，分四种情况，代表了两个按钮的相对位置，右下，右上，左下，左上
            // 4.2.1 先前点击的按钮在当前按钮的右下方
            if (preRow - curRow == 1 && preColumn - curColumn > 1)
            {
                // 尝试在两按钮上方查找连通通道
                if (availableChannels[curRow + 1, preColumn + 1])
                {
                    for (int i = 0; i < preColumn - curColumn - 1; i++)
                    {
                        if (availableChannels[curRow + 1, i + curColumn + 2] && i + curColumn + 2 == preColumn)
                        {
                            return true;
                        }
                        if (!availableChannels[curRow + 1, i + curColumn + 2])
                        {
                            break;
                        }
                    }
                    // 继续向上
                    for (int i = 0; i < curRow + 1; i++)
                    {
                        if ((!availableChannels[curRow - i, preColumn + 1]) || (!availableChannels[curRow - i, curColumn + 1]))
                        {
                            break;
                        }
                        else
                        {
                            for (int j = 0; j < preColumn - curColumn - 1; j++)
                            {
                                if (!availableChannels[curRow - i, j + curColumn + 2])
                                {
                                    break;
                                }
                                if (availableChannels[curRow - i, j + curColumn + 2] && j + 1 + curColumn + 1 == preColumn)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                // 尝试在两按钮下方查找连通通道
                if (availableChannels[preRow + 1, curColumn + 1])
                {
                    for (int i = 0; i < preColumn - curColumn - 1; i++)
                    {
                        if (availableChannels[preRow + 1, i + curColumn + 2] && curColumn + i + 2 == preColumn)
                        {
                            return true;
                        }
                        if (!availableChannels[preRow + 1, i + curColumn + 2])
                        {
                            break;
                        }
                    }
                    // 继续向下
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
                                if (availableChannels[i, j + curColumn + 2] && j + 1 + curColumn + 1 == preColumn)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }

                //尝试在两个按钮构成的矩形查找通道
                for (int i = 0; i < preColumn - curColumn - 1; i++)
                {
                    if (availableChannels[curRow + 1, i + curColumn + 2])
                    {
                        for (int j = 0; j < preColumn - curColumn - 1 - i; j++)
                        {
                            if (!availableChannels[curRow + 2, j + curColumn + 2 + i])
                            {
                                break;
                            }
                            if (availableChannels[curRow + 2, j + curColumn + 2 + i] && curColumn + 2 + j + i == preColumn)
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            // 4.2.2 先前点击的按钮在当前按钮的左上方
            if (curRow - preRow == 1 && curColumn - preColumn > 1)
            {
                // 尝试在两按钮上方查找连通通道
                if (availableChannels[preRow + 1, curColumn + 1])
                {
                    for (int i = 0; i < Math.Abs(preColumn - curColumn) - 1; i++)
                    {
                        if (availableChannels[preRow + 1, i + preColumn + 2] && i + preColumn + 2 == curColumn)
                        {
                            return true;
                        }
                        if (!availableChannels[preRow + 1, i + preColumn + 2])
                        {
                            break;
                        }
                    }
                    // 继续向上
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
                }
                // 尝试在两按钮下方查找连通通道
                if (availableChannels[curRow + 1, preColumn + 1])
                {
                    for (int i = 0; i < Math.Abs(preColumn - curColumn) - 1; i++)
                    {
                        if (availableChannels[curRow + 1, i + preColumn + 2] && preColumn + i + 2 == curColumn)
                        {
                            return true;
                        }
                        if (!availableChannels[curRow + 1, i + preColumn + 2])
                        {
                            break;
                        }
                    }
                    // 继续向下
                    for (int i = curRow + 2; i < availableRows; i++)
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

                //尝试在两个按钮构成的矩形查找通道
                for (int i = 0; i < Math.Abs(preColumn - curColumn) - 1; i++)
                {
                    if (availableChannels[preRow + 1, i + preColumn + 2])
                    {
                        for (int j = 0; j < Math.Abs(preColumn - curColumn) - 1 - i; j++)
                        {
                            if (!availableChannels[preRow + 2, j + preColumn + 2 + i])
                            {
                                break;
                            }
                            if (availableChannels[preRow + 2, j + preColumn + 2 + i] && preColumn + 2 + j + i == curColumn)
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            // 4.2.3 先前点击的按钮在当前按钮的右上方
            if (curRow - preRow == 1 && preColumn - curColumn > 1)
            {
                // 尝试在两按钮上方查找连通通道
                if (availableChannels[preRow + 1, curColumn + 1])
                {
                    for (int i = 0; i < preColumn - curColumn - 1; i++)
                    {
                        if (availableChannels[preRow + 1, i + curColumn + 2] && i + curColumn + 2 == preColumn)
                        {
                            return true;
                        }
                        if (!availableChannels[preRow + 1, i + curColumn + 2])
                        {
                            break;
                        }
                    }
                    // 继续向上
                    for (int i = 0; i < preRow + 1; i++)
                    {
                        if ((!availableChannels[preRow - i, preColumn + 1]) || (!availableChannels[preRow - i, curColumn + 1]))
                        {
                            break;
                        }
                        else
                        {
                            for (int j = 0; j < preColumn - curColumn - 1; j++)
                            {
                                if (!availableChannels[preRow - i, j + curColumn + 2])
                                {
                                    break;
                                }
                                if (availableChannels[preRow - i, j + curColumn + 2] && j + 1 + curColumn + 1 == preColumn)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                // 尝试在两按钮下方查找连通通道
                if (availableChannels[curRow + 1, preColumn + 1])
                {
                    for (int i = 0; i < preColumn - curColumn - 1; i++)
                    {
                        if (availableChannels[curRow + 1, i + curColumn + 2] && curColumn + i + 2 == preColumn)
                        {
                            return true;
                        }
                        if (!availableChannels[curRow + 1, i + curColumn + 2])
                        {
                            break;
                        }
                    }
                    // 继续向下
                    for (int i = curRow + 2; i < availableRows; i++)
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
                                if (availableChannels[i, j + curColumn + 2] && j + 1 + curColumn + 1 == preColumn)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }

                //尝试在两个按钮构成的矩形查找通道
                for (int i = 0; i < preColumn - curColumn - 1; i++)
                {
                    if (availableChannels[curRow + 1, i + curColumn + 2])
                    {
                        for (int j = 0; j < preColumn - curColumn - 1 - i; j++)
                        {
                            if (!availableChannels[preRow + 1, j + curColumn + 2 + i])
                            {
                                break;
                            }
                            if (availableChannels[preRow + 1, j + curColumn + 2 + i] && curColumn + 2 + j + i == preColumn)
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            // 4.2.4 先前点击的按钮在当前按钮的左下方
            if (preRow - curRow == 1 && curColumn - preColumn > 1)
            {
                // 尝试在两按钮上方查找连通通道
                if (availableChannels[curRow + 1, preColumn + 1])
                {
                    for (int i = 0; i < Math.Abs(preColumn - curColumn) - 1; i++)
                    {
                        if (availableChannels[curRow + 1, i + preColumn + 2] && i + preColumn + 2 == curColumn)
                        {
                            return true;
                        }
                        if (!availableChannels[curRow + 1, i + preColumn + 2])
                        {
                            break;
                        }
                    }
                    // 继续向上
                    for (int i = 0; i < curRow + 1; i++)
                    {
                        if ((!availableChannels[curRow - i, preColumn + 1]) || (!availableChannels[curRow - i, curColumn + 1]))
                        {
                            break;
                        }
                        else
                        {
                            for (int j = 0; j < Math.Abs(preColumn - curColumn) - 1; j++)
                            {
                                if (!availableChannels[curRow - i, j + preColumn + 2])
                                {
                                    break;
                                }
                                if (availableChannels[curRow - i, j + preColumn + 2] && j + 1 + preColumn + 1 == curColumn)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                // 尝试在两按钮下方查找连通通道
                if (availableChannels[preRow + 1, curColumn + 1])
                {
                    for (int i = 0; i < Math.Abs(preColumn - curColumn) - 1; i++)
                    {
                        if (availableChannels[preRow + 1, i + preColumn + 2] && preColumn + i + 2 == curColumn)
                        {
                            return true;
                        }
                        if (!availableChannels[preRow + 1, i + preColumn + 2])
                        {
                            break;
                        }
                    }
                    // 继续向下
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

                //尝试在两个按钮构成的矩形查找通道
                for (int i = 0; i < Math.Abs(preColumn - curColumn) - 1; i++)
                {
                    if (availableChannels[preRow + 1, i + preColumn + 2])
                    {
                        for (int j = 0; j < Math.Abs(preColumn - curColumn) - 1 - i; j++)
                        {
                            if (!availableChannels[curRow + 1, j + preColumn + 2 + i])
                            {
                                break;
                            }
                            if (availableChannels[curRow + 1, j + preColumn + 2 + i] && preColumn + 2 + j + i == curColumn)
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // 5. 两个按钮所处的行和列均大于1
            // 5.1 先前点击的按钮在当前按钮的右下方
            if (preRow - curRow > 1 && preColumn - curColumn > 1)
            {
                // 尝试在两按钮上方查找连通通道
                if (availableChannels[curRow + 1, preColumn + 1])
                {
                    for (int i = 0; i < preColumn - curColumn - 1; i++)
                    {
                        if (availableChannels[curRow + 1, i + curColumn + 2] && i + curColumn + 2 == preColumn)
                        {
                            // 继续向右
                            for (int k = preColumn + 2; k < availableColumns; k++)
                            {
                                if ((!availableChannels[preRow + 1, k]) || (!availableChannels[curRow + 1, k]))
                                {
                                    break;
                                }
                                else
                                {
                                    for (int j = 0; j < preRow - curRow - 1; j++)
                                    {
                                        if (!availableChannels[j + curRow + 2, k])
                                        {
                                            break;
                                        }
                                        if (availableChannels[j + curRow + 2, k] && j + 1 + curRow + 1 == preRow)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                            // 继续完成列上的路径查找
                            for (int j = 0; j < preRow - curRow - 1; j++)
                            {
                                if (availableChannels[j + curRow + 2, preColumn + 1] && j + curRow + 2 == preRow)
                                {
                                    return true;
                                }
                                if (!availableChannels[j + curRow + 2, preColumn + 1])
                                {
                                    break;
                                }
                            }
                        }
                        if (!availableChannels[curRow + 1, i + curColumn + 2])
                        {
                            break;
                        }
                    }
                    // 重新执行列上的查找
                    for (int j = 0; j < preRow - curRow - 1; j++)
                    {
                        if (availableChannels[j + curRow + 2, preColumn + 1] && j + curRow + 2 == preRow)
                        {
                            // 继续向上
                            for (int i = 0; i < curRow + 1; i++)
                            {
                                if ((!availableChannels[curRow - i, preColumn + 1]) || (!availableChannels[curRow - i, curColumn + 1]))
                                {
                                    break;
                                }
                                else
                                {
                                    for (int k = 0; k < preColumn - curColumn - 1; k++)
                                    {
                                        if (!availableChannels[curRow - i,k + curColumn + 2])
                                        {
                                            break;
                                        }
                                        if (availableChannels[curRow - i, k + curColumn + 2] && k + 1 + curColumn + 1 == preColumn)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                        if (!availableChannels[j + curRow + 2, preColumn + 1])
                        {
                            break;
                        }
                    }
                }
                // 尝试在两按钮下方查找连通通道
                if (availableChannels[preRow + 1, curColumn + 1])
                {
                    for (int i = 0; i < preColumn - curColumn - 1; i++)
                    {
                        if (availableChannels[preRow + 1, i + curColumn + 2] && curColumn + i + 2 == preColumn)
                        {
                            // 继续向左
                            for (int k = 0; k < curColumn + 1; k++)
                            {
                                if ((!availableChannels[preRow + 1, curColumn - k]) || (!availableChannels[curRow + 1, curColumn - k]))
                                {
                                    break;
                                }
                                else
                                {
                                    for (int j = 0; j < preRow - curRow - 1; j++)
                                    {
                                        if (!availableChannels[j + curRow + 2, curColumn - k])
                                        {
                                            break;
                                        }
                                        if (availableChannels[j + curRow + 2, curColumn - k] && j + 1 + curRow + 1 == preRow)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                            // 继续完成列上的查找
                            for (int j = 0; j < preRow - curRow - 1; j++)
                            {
                                if (availableChannels[preRow - j, curColumn + 1] && preRow - j == curRow + 2)
                                {
                                    return true;
                                }
                                if (!availableChannels[preRow - j, curColumn + 1])
                                {
                                    break;
                                }
                            }
                        }
                        if (!availableChannels[preRow + 1, i + curColumn + 2])
                        {
                            break;
                        }
                    }
                    // 重新完成列上的查找
                    for (int j = 0; j < preRow - curRow - 1; j++)
                    {
                        if (availableChannels[preRow - j, curColumn + 1] && preRow - j == curRow + 2)
                        {
                            //继续向下
                            for (int i = preRow + 2; i < availableRows; i++)
                            {
                                if ((!availableChannels[i, preColumn + 1]) || (!availableChannels[i, curColumn + 1]))
                                {
                                    break;
                                }
                                else
                                {
                                    for (int k = 0; k < preColumn - curColumn - 1; k++)
                                    {
                                        if (!availableChannels[i, k + curColumn + 2])
                                        {
                                            break;
                                        }
                                        if (availableChannels[i, k + curColumn + 2] && k + 1 + curColumn + 1 == preColumn)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                        if (!availableChannels[preRow - j, curColumn + 1])
                        {
                            break;
                        }
                    }
                }

                //尝试在两个按钮构成的矩形查找通道
                // 从左上角向右下角呈Z字形检索
                for (int i = 0; i < preColumn - curColumn - 1; i++)
                {
                    if (availableChannels[curRow + 1, i + curColumn + 2])
                    {
                        for (int j = 0; j < preRow-curRow; j++)
                        {
                            if (availableChannels[j + curRow + 2, curColumn + 2+i] && j + curRow + 2 == preRow+1)
                            {
                                if (curColumn+2+i==preColumn)
                                {
                                    return true;
                                }
                                for (int k = 0; k < preColumn - curColumn - 1 - i-1; k++)
                                {
                                    if (!availableChannels[preRow + 1, k + curColumn + 3+i])
                                    {
                                        break;
                                    }
                                    if (availableChannels[preRow + 1, k + curColumn + 3+i] && curColumn + 3 + k+i == preColumn)
                                    {
                                        return true;
                                    }
                                }
                            }
                            if (!availableChannels[j + curRow + 2, curColumn + 2+i])
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                // 从左上角向右下角呈倒Z字形检索
                for (int i = 0; i < preRow - curRow - 1; i++)
                {
                    if (availableChannels[i+curRow + 2, curColumn + 1])
                    {
                        for (int j = 0; j < preColumn - curColumn; j++)
                        {
                            if (availableChannels[curRow + 2+i, j+curColumn + 2] && j + curColumn + 2 == preColumn + 1)
                            {
                                if (curRow+2+i==preRow)
                                {
                                    return true;
                                }
                                for (int k = 0; k < preRow - curRow - 1 - i - 1; k++)
                                {
                                    if (!availableChannels[k+curRow +3+i, preColumn +1])
                                    {
                                        break;
                                    }
                                    if (availableChannels[k + curRow + 3 + i, preColumn + 1] && k+curRow + 3 + i == preRow)
                                    {
                                        return true;
                                    }
                                }
                            }
                            if (!availableChannels[curRow + 2+i, j+curColumn + 2])
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            // 5.2 先前点击的按钮在当前按钮的左上方
            if (curRow - preRow > 1 && curColumn - preColumn > 1)
            {
                // 尝试在两按钮上方查找连通通道
                if (availableChannels[preRow + 1, curColumn + 1])
                {
                    for (int i = 0; i < Math.Abs(preColumn - curColumn) - 1; i++)
                    {
                        if (availableChannels[preRow + 1, i + preColumn + 2] && i + preColumn + 2 == curColumn)
                        {
                            // 继续向右
                            for (int k = curColumn + 2; k < availableColumns; k++)
                            {
                                if ((!availableChannels[preRow + 1, k]) || (!availableChannels[curRow + 1, k]))
                                {
                                    break;
                                }
                                else
                                {
                                    for (int j = 0; j < Math.Abs(preRow - curRow) - 1; j++)
                                    {
                                        if (!availableChannels[j + preRow + 2, k])
                                        {
                                            break;
                                        }
                                        if (availableChannels[j + preRow + 2, k] && j + 1 + preRow + 1 == curRow)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                            // 继续完成列上的路径查找
                            for (int j = 0; j < Math.Abs(preRow - curRow) - 1; j++)
                            {
                                if (availableChannels[j + preRow + 2, curColumn + 1] && j + preRow + 2 == curRow)
                                {
                                    return true;
                                }
                                if (!availableChannels[j + preRow + 2, curColumn + 1])
                                {
                                    break;
                                }
                            }
                        }
                        if (!availableChannels[preRow + 1, i + preColumn + 2])
                        {
                            break;
                        }
                    }
                    // 继续完成列上的路径查找
                    for (int j = 0; j < Math.Abs(preRow - curRow) - 1; j++)
                    {
                        if (availableChannels[j + preRow + 2, curColumn + 1] && j + preRow + 2 == curRow)
                        {
                            // 继续向上
                            for (int i = 0; i < preRow + 1; i++)
                            {
                                if ((!availableChannels[preRow - i, preColumn + 1]) || (!availableChannels[preRow - i, curColumn + 1]))
                                {
                                    break;
                                }
                                else
                                {
                                    for (int k = 0; k < Math.Abs(preColumn - curColumn) - 1; k++)
                                    {
                                        if (!availableChannels[preRow - i, k + preColumn + 2])
                                        {
                                            break;
                                        }
                                        if (availableChannels[preRow - i, k + preColumn + 2] && k + 1 + preColumn + 1 == curColumn)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                        if (!availableChannels[j + preRow + 2, curColumn + 1])
                        {
                            break;
                        }
                    }
                }
                // 尝试在两按钮下方查找连通通道
                if (availableChannels[curRow + 1, preColumn + 1])
                {
                    for (int i = 0; i < Math.Abs(preColumn - curColumn) - 1; i++)
                    {
                        if (availableChannels[curRow + 1, i + preColumn + 2] && preColumn + i + 2 == curColumn)
                        {
                            // 继续向左
                            for (int k = 0; k < curColumn + 1; k++)
                            {
                                if ((!availableChannels[preRow + 1, preColumn - k]) || (!availableChannels[curRow + 1, preColumn - k]))
                                {
                                    break;
                                }
                                else
                                {
                                    for (int j = 0; j < Math.Abs(preRow - curRow) - 1; j++)
                                    {
                                        if (!availableChannels[j + preRow + 2, preColumn - k])
                                        {
                                            break;
                                        }
                                        if (availableChannels[j + preRow + 2, preColumn - k] && j + 1 + preRow + 1 == curRow)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                            // 继续完成列上的路径查找
                            for (int j = 0; j < Math.Abs(preRow - curRow) - 1; j++)
                            {
                                if (availableChannels[curRow - j, preColumn + 1] && curRow - j == preRow + 2)
                                {
                                    return true;
                                }
                                if (!availableChannels[curRow - j, preColumn + 1])
                                {
                                    break;
                                }
                            }
                        }
                        if (!availableChannels[curRow + 1, i + preColumn + 2])
                        {
                            break;
                        }
                    }
                    // 继续完成列上的路径查找
                    for (int j = 0; j < Math.Abs(preRow - curRow) - 1; j++)
                    {
                        if (availableChannels[curRow - j, preColumn + 1] && curRow - j == preRow + 2)
                        {
                            //继续向下
                            for (int i = curRow + 2; i < availableRows; i++)
                            {
                                if ((!availableChannels[i, preColumn + 1]) || (!availableChannels[i, curColumn + 1]))
                                {
                                    break;
                                }
                                else
                                {
                                    for (int k = 0; k < Math.Abs(preColumn - curColumn) - 1; k++)
                                    {
                                        if (!availableChannels[i, k + preColumn + 2])
                                        {
                                            break;
                                        }
                                        if (availableChannels[i, k + preColumn + 2] && k + 1 + preColumn + 1 == curColumn)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                        if (!availableChannels[curRow - j, preColumn + 1])
                        {
                            break;
                        }
                    }
                }

                //尝试在两个按钮构成的矩形查找通道
                // 从左上角向右下角呈Z字形检索
                for (int i = 0; i < Math.Abs(preColumn - curColumn) - 1; i++)
                {
                    if (availableChannels[preRow + 1, i + preColumn + 2])
                    {
                        for (int j = 0; j < Math.Abs(preRow - curRow); j++)
                        {
                            if (availableChannels[j + preRow + 2, preColumn + 2 + i] && j + preRow + 2 == curRow + 1)
                            {
                                if (preColumn + 2 + i == curColumn)
                                {
                                    return true;
                                }
                                for (int k = 0; k < Math.Abs(preColumn - curColumn) - 1 - i - 1; k++)
                                {
                                    if (!availableChannels[curRow + 1, k + preColumn + 3 + i])
                                    {
                                        break;
                                    }
                                    if (availableChannels[curRow + 1, k + preColumn + 3 + i] && preColumn + 3 + k + i == curColumn)
                                    {
                                        return true;
                                    }
                                }
                            }
                            if (!availableChannels[j + preRow + 2, preColumn + 2 + i])
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                // 从左上角向右下角呈倒Z字形检索
                for (int i = 0; i < Math.Abs(preRow - curRow) - 1; i++)
                {
                    if (availableChannels[i + preRow + 2, preColumn + 1])
                    {
                        for (int j = 0; j < Math.Abs(preColumn - curColumn); j++)
                        {
                            if (availableChannels[preRow + 2 + i, j + preColumn + 2] && j + preColumn + 2 == curColumn + 1)
                            {
                                if (preRow + 2 + i == curRow)
                                {
                                    return true;
                                }
                                for (int k = 0; k < Math.Abs(preRow - curRow) - 1 - i - 1; k++)
                                {
                                    if (!availableChannels[k + preRow + 3 + i, curColumn + 1])
                                    {
                                        break;
                                    }
                                    if (availableChannels[k + preRow + 3 + i, curColumn + 1] && k + preRow + 3 + i == curRow)
                                    {
                                        return true;
                                    }
                                }
                            }
                            if (!availableChannels[preRow + 2 + i, j + preColumn + 2])
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            // 5.3 先前点击的按钮在当前按钮的右上方
            if (curRow - preRow > 1 && preColumn - curColumn > 1)
            {
                // 尝试在两按钮上方查找连通通道
                if (availableChannels[preRow + 1, curColumn + 1])
                {
                    for (int i = 0; i < preColumn - curColumn - 1; i++)
                    {
                        if (availableChannels[preRow + 1, i + curColumn + 2] && i + curColumn + 2 == preColumn)
                        {
                            // 继续向左
                            for (int k = 0; k < curColumn + 1; k++)
                            {
                                if ((!availableChannels[preRow + 1, curColumn - k]) || (!availableChannels[curRow + 1, curColumn - k]))
                                {
                                    break;
                                }
                                else
                                {
                                    for (int j = 0; j < Math.Abs(preRow - curRow) - 1; j++)
                                    {
                                        if (!availableChannels[j + preRow + 2, curColumn - k])
                                        {
                                            break;
                                        }
                                        if (availableChannels[j + preRow + 2, curColumn - k] && j + 1 + preRow + 1 == curRow)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                            // 继续完成列上的路径查找
                            for (int j = 0; j < Math.Abs(preRow - curRow) - 1; j++)
                            {
                                if (availableChannels[j + preRow + 2, preColumn + 1] && j + preRow + 2 == curRow)
                                {
                                    return true;
                                }
                                if (!availableChannels[j + preRow + 2, preColumn + 1])
                                {
                                    break;
                                }
                            }
                        }
                        if (!availableChannels[preRow + 1, i + curColumn + 2])
                        {
                            break;
                        }
                    }
                    // 继续完成列上的路径查找
                    for (int j = 0; j < Math.Abs(preRow - curRow) - 1; j++)
                    {
                        if (availableChannels[j + preRow + 2, preColumn + 1] && j + preRow + 2 == curRow)
                        {
                            // 继续向上
                            for (int i = 0; i < preRow + 1; i++)
                            {
                                if ((!availableChannels[preRow - i, preColumn + 1]) || (!availableChannels[preRow - i, curColumn + 1]))
                                {
                                    break;
                                }
                                else
                                {
                                    for (int k = 0; k < preColumn - curColumn - 1; k++)
                                    {
                                        if (!availableChannels[preRow - i, k + curColumn + 2])
                                        {
                                            break;
                                        }
                                        if (availableChannels[preRow - i, k + curColumn + 2] && k + 1 + curColumn + 1 == preColumn)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                        if (!availableChannels[j + preRow + 2, preColumn + 1])
                        {
                            break;
                        }
                    }
                }
                // 尝试在两按钮下方查找连通通道
                if (availableChannels[curRow + 1, preColumn + 1])
                {
                    for (int i = 0; i < preColumn - curColumn - 1; i++)
                    {
                        if (availableChannels[curRow + 1, i + curColumn + 2] && curColumn + i + 2 == preColumn)
                        {
                            // 继续向右
                            for (int k = preColumn + 2; k < availableColumns; k++)
                            {
                                if ((!availableChannels[preRow + 1, k]) || (!availableChannels[curRow + 1, k]))
                                {
                                    break;
                                }
                                else
                                {
                                    for (int j = 0; j < Math.Abs(preRow - curRow) - 1; j++)
                                    {
                                        if (!availableChannels[j + preRow + 2, k])
                                        {
                                            break;
                                        }
                                        if (availableChannels[j + preRow + 2, k] && j + 1 + preRow + 1 == curRow)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                            // 继续完成列上的路径查找
                            for (int j = 0; j < Math.Abs(preRow - curRow) - 1; j++)
                            {
                                if (availableChannels[curRow - j, preColumn + 1] && curRow - j == preRow + 2)
                                {
                                    return true;
                                }
                                if (!availableChannels[curRow - j, preColumn + 1])
                                {
                                    break;
                                }
                            }
                        }
                        if (!availableChannels[curRow + 1, i + curColumn + 2])
                        {
                            break;
                        }
                    }
                    // 继续完成列上的路径查找
                    for (int j = 0; j < Math.Abs(preRow - curRow) - 1; j++)
                    {
                        if (availableChannels[curRow - j, preColumn + 1] && curRow - j == preRow + 2)
                        {
                            //继续向下
                            for (int i = curRow + 2; i < availableRows; i++)
                            {
                                if ((!availableChannels[i, preColumn + 1]) || (!availableChannels[i, curColumn + 1]))
                                {
                                    break;
                                }
                                else
                                {
                                    for (int k = 0; k < preColumn - curColumn - 1; k++)
                                    {
                                        if (!availableChannels[i, k + curColumn + 2])
                                        {
                                            break;
                                        }
                                        if (availableChannels[i, k + curColumn + 2] && k + 1 + curColumn + 1 == preColumn)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                        if (!availableChannels[curRow - j, preColumn + 1])
                        {
                            break;
                        }
                    }
                }

                //尝试在两个按钮构成的矩形查找通道
                // 从左下角向右上角呈反Z字形检索
                for (int i = 0; i < preColumn - curColumn - 1; i++)
                {
                    if (availableChannels[curRow + 1, i + curColumn + 2])
                    {
                        for (int j = 0; j < Math.Abs(preRow - curRow); j++)
                        {
                            if (availableChannels[curRow-j, curColumn + 2 + i] && curRow - j == preRow+1)
                            {
                                if (curColumn + 2 + i == preColumn)
                                {
                                    return true;
                                }
                                for (int k = 0; k <Math.Abs(preColumn - curColumn) - 1 - i - 1; k++)
                                {
                                    if (!availableChannels[preRow + 1, k + curColumn + 3 + i])
                                    {
                                        break;
                                    }
                                    if (availableChannels[preRow + 1, k + curColumn + 3 + i] && curColumn + 3 + k + i == preColumn)
                                    {
                                        return true;
                                    }
                                }
                            }
                            if (!availableChannels[curRow -j, curColumn + 2 + i])
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                // 从左下角向右上角呈倒Z字形检索
                for (int i = 0; i < Math.Abs(preRow - curRow) - 1; i++)
                {
                    if (availableChannels[curRow-i, curColumn + 1])
                    {
                        for (int j = 0; j < preColumn - curColumn; j++)
                        {
                            if (availableChannels[curRow -i, j + curColumn + 2] && j + curColumn + 2 == preColumn + 1)
                            {
                                if (curRow -i == preRow+2)
                                {
                                    return true;
                                }
                                for (int k = 0; k < Math.Abs(preRow - curRow) - 1 - i - 1; k++)
                                {
                                    if (!availableChannels[curRow -1- i-k, preColumn + 1])
                                    {
                                        break;
                                    }
                                    if (availableChannels[curRow - 1 - i-k, preColumn + 1] && curRow - 1 - i - k == preRow+2)
                                    {
                                        return true;
                                    }
                                }
                            }
                            if (!availableChannels[curRow- i, j + curColumn + 2])
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            // 5.4 先前点击的按钮在当前按钮的左下方
            if (preRow - curRow > 1 && curColumn - preColumn > 1)
            {
                // 尝试在两按钮上方查找连通通道
                if (availableChannels[curRow + 1, preColumn + 1])
                {
                    for (int i = 0; i < Math.Abs(preColumn - curColumn) - 1; i++)
                    {
                        if (availableChannels[curRow + 1, i + preColumn + 2] && i + preColumn + 2 == curColumn)
                        {
                            // 继续向左
                            for (int k = 0; k < preColumn + 1; k++)
                            {
                                if ((!availableChannels[preRow + 1, preColumn - k]) || (!availableChannels[curRow + 1, preColumn - k]))
                                {
                                    break;
                                }
                                else
                                {
                                    for (int j = 0; j < preRow - curRow - 1; j++)
                                    {
                                        if (!availableChannels[j + curRow + 2, preColumn - k])
                                        {
                                            break;
                                        }
                                        if (availableChannels[j + curRow + 2, preColumn - k] && j + 1 + curRow + 1 == preRow)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                            // 继续完成列上的路径查找
                            for (int j = 0; j < preRow - curRow - 1; j++)
                            {
                                if (availableChannels[j + curRow + 2, preColumn + 1] && j + curRow + 2 == preRow)
                                {
                                    return true;
                                }
                                if (!availableChannels[j + curRow + 2, preColumn + 1])
                                {
                                    break;
                                }
                            }
                        }
                        if (!availableChannels[curRow + 1, i + preColumn + 2])
                        {
                            break;
                        }
                    }
                    // 继续完成列上的路径查找
                    for (int j = 0; j < preRow - curRow - 1; j++)
                    {
                        if (availableChannels[j + curRow + 2, preColumn + 1] && j + curRow + 2 == preRow)
                        {
                            // 继续向上
                            for (int i = 0; i < curRow + 1; i++)
                            {
                                if ((!availableChannels[curRow - i, preColumn + 1]) || (!availableChannels[curRow - i, curColumn + 1]))
                                {
                                    break;
                                }
                                else
                                {
                                    for (int k = 0; k < Math.Abs(preColumn - curColumn) - 1; k++)
                                    {
                                        if (!availableChannels[curRow - i, k + preColumn + 2])
                                        {
                                            break;
                                        }
                                        if (availableChannels[curRow - i, k + preColumn + 2] && k + 1 + preColumn + 1 == curColumn)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                        if (!availableChannels[j + curRow + 2, preColumn + 1])
                        {
                            break;
                        }
                    }
                }
                // 尝试在两按钮下方查找连通通道
                if (availableChannels[preRow + 1, curColumn + 1])
                {
                    for (int i = 0; i < Math.Abs(preColumn - curColumn) - 1; i++)
                    {
                        if (availableChannels[preRow + 1, i +preColumn + 2] && preColumn + i + 2 == curColumn)
                        {
                            // 继续向右
                            for (int k = curColumn + 2; k < availableColumns; k++)
                            {
                                if ((!availableChannels[preRow + 1, k]) || (!availableChannels[curRow + 1, k]))
                                {
                                    break;
                                }
                                else
                                {
                                    for (int j = 0; j < preRow - curRow - 1; j++)
                                    {
                                        if (!availableChannels[j + curRow + 2, k])
                                        {
                                            break;
                                        }
                                        if (availableChannels[j + curRow + 2, k] && j + 1 + curRow + 1 == preRow)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                            // 继续完成列上的路径查找
                            for (int j = 0; j < preRow - curRow - 1; j++)
                            {
                                if (availableChannels[preRow - j, curColumn + 1] && preRow - j == curRow + 2)
                                {
                                    return true;
                                }
                                if (!availableChannels[preRow - j, curColumn + 1])
                                {
                                    break;
                                }
                            }
                        }
                        if (!availableChannels[preRow + 1, i + preColumn + 2])
                        {
                            break;
                        }
                    }
                    // 继续完成列上的路径查找
                    for (int j = 0; j < preRow - curRow - 1; j++)
                    {
                        if (availableChannels[preRow - j, curColumn + 1] && preRow - j == curRow + 2)
                        {
                            //继续向下
                            for (int i = preRow + 2; i < availableRows; i++)
                            {
                                if ((!availableChannels[i, preColumn + 1]) || (!availableChannels[i, curColumn + 1]))
                                {
                                    break;
                                }
                                else
                                {
                                    for (int k = 0; k < Math.Abs(preColumn - curColumn) - 1; k++)
                                    {
                                        if (!availableChannels[i, k + preColumn + 2])
                                        {
                                            break;
                                        }
                                        if (availableChannels[i, k + preColumn + 2] && k + 1 + preColumn + 1 == curColumn)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                        if (!availableChannels[preRow - j, curColumn + 1])
                        {
                            break;
                        }
                    }
                }

                //尝试在两个按钮构成的矩形查找通道
                // 从左下角向右上角呈反Z字形检索
                for (int i = 0; i < Math.Abs(preColumn - curColumn) - 1; i++)
                {
                    if (availableChannels[preRow + 1, i + preColumn + 2])
                    {
                        for (int j = 0; j < preRow - curRow; j++)
                        {
                            if (availableChannels[preRow - j, preColumn + 2 + i] && preRow - j == curRow + 1)
                            {
                                if (preColumn + 2 + i == curColumn)
                                {
                                    return true;
                                }
                                for (int k = 0; k < Math.Abs(preColumn - curColumn) - 1 - i - 1; k++)
                                {
                                    if (!availableChannels[curRow + 1, k + preColumn + 3 + i])
                                    {
                                        break;
                                    }
                                    if (availableChannels[curRow + 1, k + preColumn + 3 + i] && preColumn + 3 + k + i == curColumn)
                                    {
                                        return true;
                                    }
                                }
                            }
                            if (!availableChannels[preRow - j, preColumn + 2 + i])
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                // 从左下角向右上角呈倒Z字形检索
                for (int i = 0; i < preRow - curRow - 1; i++)
                {
                    if (availableChannels[preRow - i, preColumn + 1])
                    {
                        for (int j = 0; j < Math.Abs(preColumn - curColumn); j++)
                        {
                            if (availableChannels[preRow - i, j + preColumn + 2] && j + preColumn + 2 == curColumn + 1)
                            {
                                if (preRow - i == curRow+2)
                                {
                                    return true;
                                }
                                for (int k = 0; k < preRow - curRow - 1 - i - 1; k++)
                                {
                                    if (!availableChannels[preRow - 1 - i - k, curColumn + 1])
                                    {
                                        break;
                                    }
                                    if (availableChannels[preRow - 1 - i - k, curColumn + 1] && preRow - 1 - i - k == curRow + 2)
                                    {
                                        return true;
                                    }
                                }
                            }
                            if (!availableChannels[preRow - i, j + preColumn + 2])
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return judge;
        }
        private List<Button> FindIfBorderThickernessEqualFive(List<Button> buttons)
        {
            List<Button> twoBtn = new List<Button>();
            foreach (var item in buttons)
            {
                if (item.BorderThickness==new Thickness(5))
                {
                    twoBtn.Add(item);
                }
            }
            return twoBtn;
        }
        #endregion

        #region 重新布局
        private List<ImageInfo> GetRemainingImageList()
        {
            List<ImageInfo> remainingImages = new List<ImageInfo>();
            // 可用通道的行数和列数
            int availableRows = GetAvailableRows();
            int availableColumns = GetAvailableColumns();

            for (int i = 1; i <availableRows-1 ; i++)
            {
                for (int j = 1; j < availableColumns-1; j++)
                {
                    if (!availableChannels[i,j])
                    {
                        remainingImages.Add(imageInfoArray[i - 1, j - 1]);
                    }
                }
            }
            return remainingImages;
        }
        private List<Button> GetRemainingButtonList()
        {
            List<Button> remainingButtons = new List<Button>();
            // 可用通道的行数和列数
            int availableRows = GetAvailableRows();
            int availableColumns = GetAvailableColumns();

            for (int i = 1; i < availableRows - 1; i++)
            {
                for (int j = 1; j < availableColumns - 1; j++)
                {
                    if (!availableChannels[i, j])
                    {
                        remainingButtons.Add(buttonArray[i - 1, j - 1]);
                    }
                }
            }
            return remainingButtons;
        }
        private void RearrangeImageToUIRandomly()
        {
            List<ImageInfo> remainingImages = new List<ImageInfo>();
            List<ImageInfo> remainingImagesRandomly = GetRemainingImageList();
            List<Button> remainingButtons = GetRemainingButtonList();
            List<int> randomIndexes = GetRanomIndexList(remainingImagesRandomly.Count);
            for (int i = 0; i <remainingButtons.Count; i++)
            {
                remainingButtons[i].Background = remainingImagesRandomly[randomIndexes[i]].Image;
                remainingImagesRandomly[randomIndexes[i]] = SetLocation(remainingButtons[i], remainingImagesRandomly[randomIndexes[i]]);
                remainingImages.Add(remainingImagesRandomly[randomIndexes[i]]);
            }

            int maxRow = GetMaxRowIndex(GetButtonList());
            int maxColumn = GetMaxColumnIndex(GetButtonList());
            int index = 0;
            for (int i = 0; i < maxRow + 1; i++)
            {
                for (int j = 0; j < maxColumn + 1; j++)
                {
                    if (!availableChannels[i+1, j+1])
                    {
                        imageInfoArray[i, j] = remainingImages[index];
                        index++;
                    } 
                }
            }
        }
        #endregion


        public GameWindow(int userSetRows, int userSetColumns, int imageNumbers, double height, double width)
        {
            InitializeComponent();
            this.UserSetRows = userSetRows;
            this.UserSetColumns = userSetColumns;
            this.ImageNumbers = imageNumbers;
            this.Height = height;
            this.Width = width;

            SetGridRows();
            SetGridColumns();
            AddAndSetButtonPosition();

            List<ImageInfo> imageInfos=GetImageInfoList();
            List<ImageInfo> doubleImageInfo = GetDoubleImageInfoList(imageInfos);
            List<Button> buttons = GetButtonList();
            AddImageToUIRandomly(doubleImageInfo, imageInfos, GetRanomIndexList, buttons);
            FillImageInfoArray();
            FillAvailableChannels();
            FillButtonArray();

            foreach (var item in buttons)
            {
                item.Click += Item_Click;
                item.MouseEnter += Item_MouseEnter;
                item.MouseLeave += Item_MouseLeave;
            }

        }

        private void Item_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            // 判断是否有两个按钮的BorderThickness属性值为5（且为红色），这是用户尝试消除两个未能匹配的按钮造成的，
            // 在用户下一次点击按钮时将此影响消除
            List<Button> buttons = FindIfBorderThickernessEqualFive(GetButtonList());
            if (buttons.Count==2)
            {
                foreach (var item in buttons)
                {
                    item.BorderThickness = new Thickness(0);
                }
            }
            
            // 如果之前没有按钮按下
            if (!isFirstButtonClicked)
            {
                isFirstButtonClicked = true;
                button.BorderBrush = new SolidColorBrush(Colors.LawnGreen);
                button.BorderThickness = new Thickness(5);

                previousClickedButton = button;
            }
            else
            {
                // 如果两次按下的按钮相同，消除之前对按钮的设定
                if (button ==previousClickedButton)
                {
                    button.BorderThickness = new Thickness(0);
                    previousClickedButton = null;
                    isFirstButtonClicked = false;
                }
                // 如果两次按下的按钮不同
                else
                {
                    // 获取先前和当前按钮所附带的图片信息
                    ImageInfo previousImageInfo = imageInfoArray[GetRow(previousClickedButton), GetColumn(previousClickedButton)];
                    ImageInfo currentImageInfo = imageInfoArray[GetRow(button), GetColumn(button)];
                    // 如果两张图片相同，而且CheckIfMatch方法返回True
                    if (previousImageInfo.Id==currentImageInfo.Id&& CheckIfMatch(previousClickedButton, button))
                    {
                        // 在可用通道中将两个按钮相应位置上的元素设为True
                        availableChannels[GetRow(previousClickedButton) + 1, GetColumn(previousClickedButton) + 1] = true;
                        availableChannels[GetRow(button) + 1, GetColumn(button) + 1] = true;

                        // 消除在判断过程中对按钮外观的影响
                        previousClickedButton.Background = new SolidColorBrush(Colors.Transparent);
                        button.Background = new SolidColorBrush(Colors.Transparent);
                        previousClickedButton.BorderThickness = new Thickness(0);
                        button.BorderThickness = new Thickness(0);

                        // 恢复至没有按钮点击过的状态，禁用按钮
                        previousClickedButton.IsEnabled = false;
                        button.IsEnabled = false;
                    }
                    // 如果不能满足消除的条件，在两个按钮上加一层红色的外边框
                    else
                    {
                        button.BorderBrush = new SolidColorBrush(Colors.Red);
                        button.BorderThickness = new Thickness(5);
                        previousClickedButton.BorderBrush = new SolidColorBrush(Colors.Red);
                        previousClickedButton.BorderThickness = new Thickness(5);
                    }

                    // 恢复至没有按钮点击过的状态
                    previousClickedButton = null;
                    isFirstButtonClicked = false;
                }
            }
        }
        private void Item_MouseEnter(object sender, MouseEventArgs e)
        {
            Button button = sender as Button;
            int imageId = imageInfoArray[GetRow(button), GetColumn(button)].Id;
            ImageBrush imageBrush=(ImageBrush)TryFindResource("image" + imageId + "_OneColor");
            if (imageBrush != null)
            {
                button.Background = imageBrush;
            }
            else throw new Exception("内部资源图片丢失！");
        }
        private void Item_MouseLeave(object sender, MouseEventArgs e)
        {
            Button button = sender as Button;
            int imageId = imageInfoArray[GetRow(button), GetColumn(button)].Id;
            ImageBrush imageBrush = (ImageBrush)TryFindResource("image" + imageId + "_FullColor");
            if (imageBrush != null)
            {
                button.Background = imageBrush;
            }
            else throw new Exception("内部资源图片丢失！");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RearrangeImageToUIRandomly();

            List<Button> buttons = FindIfBorderThickernessEqualFive(GetButtonList());
            if (buttons.Count == 1||buttons.Count==2)
            {
                foreach (var item in buttons)
                {
                    item.BorderThickness = new Thickness(0);
                }
            }
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
