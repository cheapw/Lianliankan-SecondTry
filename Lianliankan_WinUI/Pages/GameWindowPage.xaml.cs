using Lianliankan_WinUI.Models;
using Microsoft.UI;
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
using System.Text.Json;
using Windows.Devices.Input;
using Windows.Foundation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Lianliankan_WinUI.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GameWindowPage : Page
    {
        #region �������ԣ��û�������һЩ��Ϸ��ʼ����
        public int UserSetRows { get; set; }
        public int UserSetColumns { get; set; }
        public int ImageNumbers { get; set; }
        public int TimeAvailable { get; set; }
        public int RemainingSeconds { get; set; }
        #endregion

        #region ˽���ֶ�
        // ��ʱ��
        DispatcherTimer timer = null;
        // �����Ƿ��˳��������ڴ˴���ÿ�ιر�ʱ���ᴥ��Closed�¼������ô�bool�ֶξ���Closed������ʱ�Ƿ��˳�����
        private bool IsWantToExitApp = true;
        // �ж��Ƿ�����Ϸ�������棬�������Ƿ����һЩ��ť
        private bool IsInGameComplete = false;
        // һά���飬���������ͼƬ����ͬ��ͼƬ���������ţ�����Ϊż����ͼƬ�����밴ť������ͬ���Ұ�����ӽ���ť��˳������
        private List<ImageInfo> allImageInfoNeeded = null;
        // ��ά���飬��¼ͼƬ�ڰ�ť����ʾ��λ��
        private ImageInfo[,] imageInfoArray = null;
        // ��ά���飬��¼��ťλ���������Է������
        private Button[,] buttonArray = null;
        // �жϵ�һ����ť�Ƿ���
        private bool isFirstButtonClicked = false;
        // ��¼��һ�����µİ�ť���Ա���ڶ������µİ�ť�������
        private Button previousClickedButton = null;
        // ����ͨ������������ť��ɵķ����һȦ���ö�ά�����ڲ���Ԫ���ǲ������ͣ��ݴ��ж�������ť�Ƿ����������
        // ��Ȧȫ��Ϊtrue���а�ťռ�ݵĵط�ȫ��Ϊfalse����ť����������Ӧλ�ñ�Ϊtrue
        private bool[,] availableChannels = null;

        private GameOptions m_GameParameter;
        #endregion

        #region �����û�������һЩ���ݽ��г�ʼ�趨������Grid ������к��У���ť�ĸ�����
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
                    button.HorizontalAlignment = HorizontalAlignment.Stretch;
                    button.VerticalAlignment = VerticalAlignment.Stretch;
                    Grid.SetRow(button, i);
                    Grid.SetColumn(button, j);
                    gamePanel.Children.Add(button);
                }
            }
        }
        #endregion

        #region ��ͼƬ�����ӵ��û����棬����¼ͼƬ��λ��
        /// <summary>
        /// ��ȡͼƬ��Ϣ�б����е�ͼƬ������ͬ
        /// </summary>
        /// <returns>ͼƬ��Ϣ�б�</returns>
        private List<ImageInfo> GetImageInfoList()
        {
            List<ImageInfo> imageInfos = new List<ImageInfo>();
            int index = 1;

            while (index <= ImageNumbers)
            {
                string resourceKey = "image" + index.ToString() + "_FullColor";
                // ����ͨ����Դ��������Դ��������null ����ֹѭ��
                //var image = (ImageBrush)TryFindResource(resourceKey);
                ImageBrush image = null;
                if (this.Resources.TryGetValue(resourceKey, out object obj))
                {
                    image = obj as ImageBrush ?? new ImageBrush();
                }
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
        /// ��ͼƬ��Ϣ�б�ӱ���ÿһ��Ԫ�غ����λ�ü�����ͬ��Ԫ��
        /// </summary>
        /// <param name="imageInfos"></param>
        /// <returns>�ӱ����ͼƬ��Ϣ�б�</returns>
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
        /// ��ȡһ���������͵���������б��б�������Ԫ�ز��ظ�
        /// </summary>
        /// <param name="count">����������Ҫ�������������б��Ԫ�ظ���</param>
        /// <returns>��������б�</returns>
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
                int randNum = rand.Next(subtractIndex);
                randomIndexList.Add(temp[randNum]);
                temp.Remove(temp[randNum]);
                subtractIndex--;
            }
            return randomIndexList;
        }
        /// <summary>
        /// ��ȡ��ť�б����޽��������İ�ť���������������ܰ�ť
        /// </summary>
        /// <returns>��ť�б�</returns>
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
        /// ��ͼƬ�����ӵ�UI��ť��
        /// </summary>
        /// <param name="doubleImageInfos">�ӱ����ͼƬ��Ϣ�б�</param>
        /// <param name="imageInfos">ͼƬ��Ϣ�б�</param>
        /// <param name="GetRandomIndexListCallback">�ص������������ȡ��������б�ķ���</param>
        /// <param name="buttons">��ť�б�</param>
        private void AddImageToUIRandomly(List<ImageInfo> doubleImageInfos, List<ImageInfo> imageInfos, Func<int, List<int>> GetRandomIndexListCallback, List<Button> buttons)
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
                allImageInfoNeededRandomly[allRandomIndex[i]] = SetLocation(buttons[i], allImageInfoNeededRandomly[allRandomIndex[i]]);
                allImageInfoNeeded.Add(allImageInfoNeededRandomly[allRandomIndex[i]]);
            }
        }
        /// <summary>
        /// �ڽ�ͼƬ��ӵ� UI ��ʱ����¼��ť���� Grid ���ֵ��к��У��� ImageInfo �� Row �� Column ���Խ��д洢
        /// </summary>
        /// <param name="button">��ǰ��ť</param>
        /// <param name="imageInfo">��ǰͼƬ��Ϣ</param>
        /// <returns>����һ��ͼƬ��Ϣ�ṹ</returns>
        private ImageInfo SetLocation(Button button, ImageInfo imageInfo)
        {
            imageInfo.Row = Grid.GetRow(button);
            imageInfo.Column = Grid.GetColumn(button);
            return imageInfo;
        }
        #endregion

        #region �������
        /// <summary>
        /// ��ȡ��ť����е�����
        /// </summary>
        /// <param name="buttons">��ť����е�����</param>
        /// <returns></returns>
        private int GetMaxRowIndex(List<Button> buttons)
        {
            return Grid.GetRow(buttons[buttons.Count - 1]);
        }
        /// <summary>
        /// ��ȡ��ť����е�����
        /// </summary>
        /// <param name="buttons">��ť����е�����</param>
        /// <returns></returns>
        private int GetMaxColumnIndex(List<Button> buttons)
        {
            return Grid.GetColumn(buttons[buttons.Count - 1]);
        }
        /// <summary>
        /// ����¼ͼƬ�ڰ�ť����ʾ��λ�õĶ�ά����
        /// </summary>
        private void FillImageInfoArray()
        {
            List<Button> buttons = GetButtonList();
            int maxRow = GetMaxRowIndex(buttons);
            int maxColumn = GetMaxColumnIndex(buttons);
            imageInfoArray = new ImageInfo[maxRow + 1, maxColumn + 1];
            int index = 0;
            for (int i = 0; i < maxRow + 1; i++)
            {
                for (int j = 0; j < maxColumn + 1; j++)
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
        /// ��ȡ������ť��������
        /// </summary>
        /// <param name="button">������ť��������</param>
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
                throw new Exception("�İ�ť�������ؼ�����Grid���޷���ȡ�����ڵ��У�");
            }
            return row;
        }
        /// <summary>
        /// ��ȡ������ť��������
        /// </summary>
        /// <param name="button">������ť��������</param>
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
                throw new Exception("�İ�ť�������ؼ�����Grid���޷���ȡ�����ڵ��У�");
            }
            return column;
        }
        /// <summary>
        /// ������ͨ������ȦΪtrue���ڲ�Ϊfalse
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
        /// ��ȡ����ͨ��������
        /// </summary>
        /// <returns></returns>
        private int GetAvailableRows()
        {
            return GetMaxRowIndex(GetButtonList()) + 3;
        }
        /// <summary>
        /// ��ȡ����ͨ��������
        /// </summary>
        /// <returns></returns>
        private int GetAvailableColumns()
        {
            return GetMaxColumnIndex(GetButtonList()) + 3;
        }
        /// <summary>
        /// �ж�������ť�Ƿ��ܹ������������Ĵ����ڴ˷����У����Խ���һЩ���ϣ�����ʹ���������
        /// </summary>
        /// <param name="previousButton">��һ����ť</param>
        /// <param name="currentButton">�ڶ�����ť</param>
        /// <returns></returns>
        private bool CheckIfMatch(Button previousButton, Button currentButton)
        {
            bool judge = false;

            // �л��У����㿪ʼ���������ֵΪһ�л�һ���ϰ�ť������1
            int preRow = GetRow(previousButton);
            int preColumn = GetColumn(previousButton);
            int curRow = GetRow(currentButton);
            int curColumn = GetColumn(currentButton);

            // ���õ��л��У�AvailableChannels��������а�ť��ɵķ����һȦ�������Ҹ���1��
            // ����GetMaxRow������ȡ��ֻ���������������ټ�1�������3
            int availableRows = GetMaxRowIndex(GetButtonList()) + 3;
            int availableColumns = GetMaxColumnIndex(GetButtonList()) + 3;

            // 1.������ť����ͬһ�л�ͬһ�������ڵĻ���ֱ�ӷ���true
            if (preRow == curRow && Math.Abs(preColumn - curColumn) == 1)
            {
                return true;
            }
            if (preColumn == curColumn && Math.Abs(preRow - curRow) == 1)
            {
                return true;
            }

            // 2.���������ť����ͬһ�л�ͬһ�е�������
            // 2.1 ����ͬһ��
            if (preRow == curRow)
            {
                // �����ǰ�İ�ť�ȵ�ǰ�İ�ť�����е�������
                if (preColumn > curColumn)
                {
                    // ��֤������ť֮���Ƿ����赲
                    for (int i = 1; i < preColumn - curColumn; i++)
                    {
                        if (!availableChannels[preRow + 1, i + curColumn + 1])
                        {
                            break;
                        }
                        if (availableChannels[preRow + 1, i + curColumn + 1] && i + curColumn + 1 == preColumn)
                        {
                            return true;
                        }
                    }
                    // ��֤������ť�Ϸ��Ƿ������ͨͨ��
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
                    // ��֤������ť�·��Ƿ������ͨͨ��
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
                // �����ǰ�İ�ť�ȵ�ǰ�İ�ť�����е�����С
                if (preColumn < curColumn)
                {
                    // ��֤������ť֮���Ƿ����赲
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
                    // ��֤������ť�Ϸ��Ƿ������ͨͨ��
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
                    // ��֤������ť�·��Ƿ������ͨͨ��
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
            // 2.2 ����ͬһ��
            if (preColumn == curColumn)
            {
                // �����ǰ�İ�ť�ȵ�ǰ�İ�ť�����е�������
                if (preRow > curRow)
                {
                    // ��֤������ť֮���Ƿ����赲
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
                    // ��֤������ť���Ƿ������ͨͨ��
                    for (int i = 0; i < preColumn + 1; i++)
                    {
                        if ((!availableChannels[preRow + 1, preColumn - i]) || (!availableChannels[curRow + 1, curColumn - i]))
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
                    // ��֤������ť�ҷ��Ƿ������ͨͨ��
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
                // �����ǰ�İ�ť�ȵ�ǰ�İ�ť�����е�����С
                if (preRow < curRow)
                {
                    // ��֤������ť֮���Ƿ����赲
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
                    // ��֤������ť���Ƿ������ͨͨ��
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
                    // ��֤������ť�ҷ��Ƿ������ͨͨ��
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

            // 3.�����ĸ���ť��ɵ������εĶԽ�����
            if (Math.Abs(curColumn - preColumn) == 1 && Math.Abs(curRow - preRow) == 1)
            {
                if (availableChannels[curRow + 1, preColumn + 1] || availableChannels[preRow + 1, curColumn + 1])
                {
                    return true;
                }
            }

            // 4.������ť�������ڵ����Ҽ������������1�����ߴ������ڵ����Ҽ������������1
            // 4.1 ������ť�������ڵ����Ҽ������������1���Ե�ǰ�İ�ťΪ��׼�������������������������ť�����λ�ã����£����ϣ����£�����
            // 4.1.1 ��ǰ����İ�ť�ڵ�ǰ��ť�����·�
            if (preColumn - curColumn == 1 && preRow - curRow > 1)
            {
                // ����������ť�ұ߲�����ͨͨ��
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
                    // ��������
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
                // ����������ť��߲�����ͨͨ��
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
                    // ��������
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

                //������������ť���ɵľ��β���ͨ��
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
            // 4.1.2 ��ǰ����İ�ť�ڵ�ǰ��ť�����Ϸ�
            if (curColumn - preColumn == 1 && curRow - preRow > 1)
            {
                // ����������ť�ұ߲�����ͨͨ��
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
                    // ��������
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
                // ����������ť��߲�����ͨͨ��
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
                    // ��������
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

                //������������ť���ɵľ��β���ͨ��
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
            // 4.1.3 ��ǰ����İ�ť�ڵ�ǰ��ť�����Ϸ�
            if (preColumn - curColumn == 1 && curRow - preRow > 1)
            {
                // ����������ť�ұ߲�����ͨͨ��
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
                    // ��������
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
                // ����������ť��߲�����ͨͨ��
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
                    // ��������
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

                //������������ť���ɵľ��β���ͨ��
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
            // 4.1.4 ��ǰ����İ�ť�ڵ�ǰ��ť�����·�
            if (curColumn - preColumn == 1 && preRow - curRow > 1)
            {
                // ����������ť�ұ߲�����ͨͨ��
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
                    // ��������
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
                // ����������ť��߲�����ͨͨ��
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
                    // ��������
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
                //������������ť���ɵľ��β���ͨ��
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

            // 4.2 ������ť�������ڵ����Ҽ������������1���Ե�ǰ�İ�ťΪ��׼�������������������������ť�����λ�ã����£����ϣ����£�����
            // 4.2.1 ��ǰ����İ�ť�ڵ�ǰ��ť�����·�
            if (preRow - curRow == 1 && preColumn - curColumn > 1)
            {
                // ����������ť�Ϸ�������ͨͨ��
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
                    // ��������
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
                // ����������ť�·�������ͨͨ��
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
                    // ��������
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

                //������������ť���ɵľ��β���ͨ��
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
            // 4.2.2 ��ǰ����İ�ť�ڵ�ǰ��ť�����Ϸ�
            if (curRow - preRow == 1 && curColumn - preColumn > 1)
            {
                // ����������ť�Ϸ�������ͨͨ��
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
                    // ��������
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
                // ����������ť�·�������ͨͨ��
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
                    // ��������
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

                //������������ť���ɵľ��β���ͨ��
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
            // 4.2.3 ��ǰ����İ�ť�ڵ�ǰ��ť�����Ϸ�
            if (curRow - preRow == 1 && preColumn - curColumn > 1)
            {
                // ����������ť�Ϸ�������ͨͨ��
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
                    // ��������
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
                // ����������ť�·�������ͨͨ��
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
                    // ��������
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

                //������������ť���ɵľ��β���ͨ��
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
            // 4.2.4 ��ǰ����İ�ť�ڵ�ǰ��ť�����·�
            if (preRow - curRow == 1 && curColumn - preColumn > 1)
            {
                // ����������ť�Ϸ�������ͨͨ��
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
                    // ��������
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
                // ����������ť�·�������ͨͨ��
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
                    // ��������
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

                //������������ť���ɵľ��β���ͨ��
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

            // 5. ������ť�������к��о�����1
            // 5.1 ��ǰ����İ�ť�ڵ�ǰ��ť�����·�
            if (preRow - curRow > 1 && preColumn - curColumn > 1)
            {
                // ����������ť�Ϸ�������ͨͨ��
                if (availableChannels[curRow + 1, preColumn + 1])
                {
                    for (int i = 0; i < preColumn - curColumn - 1; i++)
                    {
                        if (availableChannels[curRow + 1, i + curColumn + 2] && i + curColumn + 2 == preColumn)
                        {
                            // ��������
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
                            // ����������ϵ�·������
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
                    // ����ִ�����ϵĲ���
                    for (int j = 0; j < preRow - curRow - 1; j++)
                    {
                        if (availableChannels[j + curRow + 2, preColumn + 1] && j + curRow + 2 == preRow)
                        {
                            // ��������
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
                                        if (!availableChannels[curRow - i, k + curColumn + 2])
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
                // ����������ť�·�������ͨͨ��
                if (availableChannels[preRow + 1, curColumn + 1])
                {
                    for (int i = 0; i < preColumn - curColumn - 1; i++)
                    {
                        if (availableChannels[preRow + 1, i + curColumn + 2] && curColumn + i + 2 == preColumn)
                        {
                            // ��������
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
                            // ����������ϵĲ���
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
                    // ����������ϵĲ���
                    for (int j = 0; j < preRow - curRow - 1; j++)
                    {
                        if (availableChannels[preRow - j, curColumn + 1] && preRow - j == curRow + 2)
                        {
                            //��������
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

                //������������ť���ɵľ��β���ͨ��
                // �����Ͻ������½ǳ�Z���μ���
                for (int i = 0; i < preColumn - curColumn - 1; i++)
                {
                    if (availableChannels[curRow + 1, i + curColumn + 2])
                    {
                        for (int j = 0; j < preRow - curRow; j++)
                        {
                            if (availableChannels[j + curRow + 2, curColumn + 2 + i] && j + curRow + 2 == preRow + 1)
                            {
                                if (curColumn + 2 + i == preColumn)
                                {
                                    return true;
                                }
                                for (int k = 0; k < preColumn - curColumn - 1 - i - 1; k++)
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
                            if (!availableChannels[j + curRow + 2, curColumn + 2 + i])
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
                // �����Ͻ������½ǳʵ�Z���μ���
                for (int i = 0; i < preRow - curRow - 1; i++)
                {
                    if (availableChannels[i + curRow + 2, curColumn + 1])
                    {
                        for (int j = 0; j < preColumn - curColumn; j++)
                        {
                            if (availableChannels[curRow + 2 + i, j + curColumn + 2] && j + curColumn + 2 == preColumn + 1)
                            {
                                if (curRow + 2 + i == preRow)
                                {
                                    return true;
                                }
                                for (int k = 0; k < preRow - curRow - 1 - i - 1; k++)
                                {
                                    if (!availableChannels[k + curRow + 3 + i, preColumn + 1])
                                    {
                                        break;
                                    }
                                    if (availableChannels[k + curRow + 3 + i, preColumn + 1] && k + curRow + 3 + i == preRow)
                                    {
                                        return true;
                                    }
                                }
                            }
                            if (!availableChannels[curRow + 2 + i, j + curColumn + 2])
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
            // 5.2 ��ǰ����İ�ť�ڵ�ǰ��ť�����Ϸ�
            if (curRow - preRow > 1 && curColumn - preColumn > 1)
            {
                // ����������ť�Ϸ�������ͨͨ��
                if (availableChannels[preRow + 1, curColumn + 1])
                {
                    for (int i = 0; i < Math.Abs(preColumn - curColumn) - 1; i++)
                    {
                        if (availableChannels[preRow + 1, i + preColumn + 2] && i + preColumn + 2 == curColumn)
                        {
                            // ��������
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
                            // ����������ϵ�·������
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
                    // ����������ϵ�·������
                    for (int j = 0; j < Math.Abs(preRow - curRow) - 1; j++)
                    {
                        if (availableChannels[j + preRow + 2, curColumn + 1] && j + preRow + 2 == curRow)
                        {
                            // ��������
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
                // ����������ť�·�������ͨͨ��
                if (availableChannels[curRow + 1, preColumn + 1])
                {
                    for (int i = 0; i < Math.Abs(preColumn - curColumn) - 1; i++)
                    {
                        if (availableChannels[curRow + 1, i + preColumn + 2] && preColumn + i + 2 == curColumn)
                        {
                            // ��������
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
                            // ����������ϵ�·������
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
                    // ����������ϵ�·������
                    for (int j = 0; j < Math.Abs(preRow - curRow) - 1; j++)
                    {
                        if (availableChannels[curRow - j, preColumn + 1] && curRow - j == preRow + 2)
                        {
                            //��������
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

                //������������ť���ɵľ��β���ͨ��
                // �����Ͻ������½ǳ�Z���μ���
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
                // �����Ͻ������½ǳʵ�Z���μ���
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
            // 5.3 ��ǰ����İ�ť�ڵ�ǰ��ť�����Ϸ�
            if (curRow - preRow > 1 && preColumn - curColumn > 1)
            {
                // ����������ť�Ϸ�������ͨͨ��
                if (availableChannels[preRow + 1, curColumn + 1])
                {
                    for (int i = 0; i < preColumn - curColumn - 1; i++)
                    {
                        if (availableChannels[preRow + 1, i + curColumn + 2] && i + curColumn + 2 == preColumn)
                        {
                            // ��������
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
                            // ����������ϵ�·������
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
                        if (!availableChannels[preRow + 1, i + curColumn + 2])
                        {
                            break;
                        }
                    }
                    // ����������ϵ�·������
                    for (int j = 0; j < Math.Abs(preRow - curRow) - 1; j++)
                    {
                        if (availableChannels[j + preRow + 2, curColumn + 1] && j + preRow + 2 == curRow)
                        {
                            // ��������
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
                        if (!availableChannels[j + preRow + 2, curColumn + 1])
                        {
                            break;
                        }
                    }
                }
                // ����������ť�·�������ͨͨ��
                if (availableChannels[curRow + 1, preColumn + 1])
                {
                    for (int i = 0; i < preColumn - curColumn - 1; i++)
                    {
                        if (availableChannels[curRow + 1, i + curColumn + 2] && curColumn + i + 2 == preColumn)
                        {
                            // ��������
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
                            // ����������ϵ�·������
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
                    // ����������ϵ�·������
                    for (int j = 0; j < Math.Abs(preRow - curRow) - 1; j++)
                    {
                        if (availableChannels[curRow - j, preColumn + 1] && curRow - j == preRow + 2)
                        {
                            //��������
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

                //������������ť���ɵľ��β���ͨ��
                // �����½������Ͻǳʷ�Z���μ���
                for (int i = 0; i < preColumn - curColumn - 1; i++)
                {
                    if (availableChannels[curRow + 1, i + curColumn + 2])
                    {
                        for (int j = 0; j < Math.Abs(preRow - curRow); j++)
                        {
                            if (availableChannels[curRow - j, curColumn + 2 + i] && curRow - j == preRow + 1)
                            {
                                if (curColumn + 2 + i == preColumn)
                                {
                                    return true;
                                }
                                for (int k = 0; k < Math.Abs(preColumn - curColumn) - 1 - i - 1; k++)
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
                            if (!availableChannels[curRow - j, curColumn + 2 + i])
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
                // �����½������Ͻǳʵ�Z���μ���
                for (int i = 0; i < Math.Abs(preRow - curRow) - 1; i++)
                {
                    if (availableChannels[curRow - i, curColumn + 1])
                    {
                        for (int j = 0; j < preColumn - curColumn; j++)
                        {
                            if (availableChannels[curRow - i, j + curColumn + 2] && j + curColumn + 2 == preColumn + 1)
                            {
                                if (curRow - i == preRow + 2)
                                {
                                    return true;
                                }
                                for (int k = 0; k < Math.Abs(preRow - curRow) - 1 - i - 1; k++)
                                {
                                    if (!availableChannels[curRow - 1 - i - k, preColumn + 1])
                                    {
                                        break;
                                    }
                                    if (availableChannels[curRow - 1 - i - k, preColumn + 1] && curRow - 1 - i - k == preRow + 2)
                                    {
                                        return true;
                                    }
                                }
                            }
                            if (!availableChannels[curRow - i, j + curColumn + 2])
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
            // 5.4 ��ǰ����İ�ť�ڵ�ǰ��ť�����·�
            if (preRow - curRow > 1 && curColumn - preColumn > 1)
            {
                // ����������ť�Ϸ�������ͨͨ��
                if (availableChannels[curRow + 1, preColumn + 1])
                {
                    for (int i = 0; i < Math.Abs(preColumn - curColumn) - 1; i++)
                    {
                        if (availableChannels[curRow + 1, i + preColumn + 2] && i + preColumn + 2 == curColumn)
                        {
                            // ��������
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
                            // ����������ϵ�·������
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
                    // ����������ϵ�·������
                    for (int j = 0; j < preRow - curRow - 1; j++)
                    {
                        if (availableChannels[j + curRow + 2, preColumn + 1] && j + curRow + 2 == preRow)
                        {
                            // ��������
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
                // ����������ť�·�������ͨͨ��
                if (availableChannels[preRow + 1, curColumn + 1])
                {
                    for (int i = 0; i < Math.Abs(preColumn - curColumn) - 1; i++)
                    {
                        if (availableChannels[preRow + 1, i + preColumn + 2] && preColumn + i + 2 == curColumn)
                        {
                            // ��������
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
                            // ����������ϵ�·������
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
                    // ����������ϵ�·������
                    for (int j = 0; j < preRow - curRow - 1; j++)
                    {
                        if (availableChannels[preRow - j, curColumn + 1] && preRow - j == curRow + 2)
                        {
                            //��������
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

                //������������ť���ɵľ��β���ͨ��
                // �����½������Ͻǳʷ�Z���μ���
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
                // �����½������Ͻǳʵ�Z���μ���
                for (int i = 0; i < preRow - curRow - 1; i++)
                {
                    if (availableChannels[preRow - i, preColumn + 1])
                    {
                        for (int j = 0; j < Math.Abs(preColumn - curColumn); j++)
                        {
                            if (availableChannels[preRow - i, j + preColumn + 2] && j + preColumn + 2 == curColumn + 1)
                            {
                                if (preRow - i == curRow + 2)
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
                if (item.BorderThickness == new Thickness(5))
                {
                    twoBtn.Add(item);
                }
            }
            return twoBtn;
        }
        #endregion

        #region ���´��Ҳ���
        private List<ImageInfo> GetRemainingImageList()
        {
            List<ImageInfo> remainingImages = new List<ImageInfo>();
            // ����ͨ��������������
            int availableRows = GetAvailableRows();
            int availableColumns = GetAvailableColumns();

            for (int i = 1; i < availableRows - 1; i++)
            {
                for (int j = 1; j < availableColumns - 1; j++)
                {
                    if (!availableChannels[i, j])
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
            // ����ͨ��������������
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
            for (int i = 0; i < remainingButtons.Count; i++)
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
                    if (!availableChannels[i + 1, j + 1])
                    {
                        imageInfoArray[i, j] = remainingImages[index];
                        index++;
                    }
                }
            }
        }
        #endregion

        #region ��ʾ��ť
        private Button[] TryToPrompt()
        {
            Button button1, button2;
            ImageInfo image1, image2;

            List<Button> remainingButtons = new List<Button>();
            // ����ͨ��������������
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

            for (int i = 0; i < remainingButtons.Count; i++)
            {
                for (int j = 0; j < remainingButtons.Count; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }
                    button1 = remainingButtons[i];
                    button2 = remainingButtons[j];
                    image1 = imageInfoArray[GetRow(button1), GetColumn(button1)];
                    image2 = imageInfoArray[GetRow(button2), GetColumn(button2)];
                    if (image1.Id == image2.Id)
                    {
                        bool isMatched = CheckIfMatch(button1, button2);
                        if (isMatched)
                        {
                            return new Button[] { button1, button2 };
                        }
                    }

                }
            }
            return null;
        }
        #endregion

        #region ��ʱ��
        private void CountDown(int timeAvailable)
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            int seconds = timeAvailable * 60;
            DateTime timeLeftover = new DateTime(2018, 10, 25, 0, timeAvailable, 0);
            //MessageBox.Show(timeLeftover.ToString());
            timeLeftover.ToLocalTime();
            timer.Tick += (sender, e) =>
            {
                seconds--;
                if (seconds == 0)
                {
                    CreateGameCompluteUI(false);
                    timer.Stop();
                }

                RemainingSeconds = seconds;

                // ʵʱ����progressbar����
                double progress = 100 - (double)seconds / (timeAvailable * 60) * 100;
                if (progress >= 0 && progress <= 100)
                {
                    progressBar.Value = progress;
                }
                timeLeftover = timeLeftover - new TimeSpan(0, 0, 1);
                this.textbockTimeDown.Text = timeLeftover.ToString("mm:ss");
            };
            timer.Start();
        }

        #endregion

        #region ��Ϸ����������ʾ
        private bool CheckIfAllElementEliminated()
        {
            int availableRows = GetAvailableRows();
            int availableColumns = GetAvailableColumns();

            for (int i = 1; i < availableRows - 1; i++)
            {
                for (int j = 1; j < availableColumns - 1; j++)
                {
                    if (availableChannels[i, j] == false)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        private void CreateGameCompluteUI(bool isWin)
        {
            gamePanel.ColumnDefinitions.Clear();
            gamePanel.RowDefinitions.Clear();
            gamePanel.Children.Clear();

            for (int i = 0; i < 2; i++)
            {
                RowDefinition row = new RowDefinition();
                gamePanel.RowDefinitions.Add(row);
            }
            for (int i = 0; i < 2; i++)
            {
                ColumnDefinition column = new ColumnDefinition();
                gamePanel.ColumnDefinitions.Add(column);
            }

            TextBlock textBlock = new TextBlock();
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.FontSize = 30d;
            Grid.SetColumnSpan(textBlock, 2);

            var resourceLoader = new Microsoft.Windows.ApplicationModel.Resources.ResourceLoader();
            var backToMenu = resourceLoader.GetString("BackToMenu");
            var tryAgainPrompt = resourceLoader.GetString("TryAgainPrompt");



            if (isWin)
            {
                var successPrompt = resourceLoader.GetString("SuccessPrompt");
                textBlock.Text = successPrompt;
                SaveRankInfo();
            }
            else 
            {
                var failPrompt = resourceLoader.GetString("FailPrompt");
                textBlock.Text = failPrompt; 
            }

            Button returnToMenu = new Button();

            if(Application.Current.Resources.TryGetValue("CustomButtonStyle", out object obj))
            {
                returnToMenu.Style = obj as Style?? new Style();
            }

            returnToMenu.Content = backToMenu;
            Grid.SetRow(returnToMenu, 1);
            returnToMenu.Click += ReturnToManu_Click;

            Button retry = new Button();
            retry.Style = obj as Style ?? new Style();
            retry.Content = tryAgainPrompt;
            Grid.SetRow(retry, 1);
            Grid.SetColumn(retry, 1);
            retry.Click += (sender, e) =>
            {
                SetGridColumns();
                SetGridRows();
                Restart_Click(sender, e);
            };

            gamePanel.Children.Add(textBlock);
            gamePanel.Children.Add(returnToMenu);
            gamePanel.Children.Add(retry);
            IsInGameComplete = true;

            timer.Stop();
        }

        private void SaveRankInfo()
        {
            var rows = m_GameParameter.UserSetRows;
            var columns = m_GameParameter.UserSetColumns;
            var gameLevels = m_GameParameter.GameLevels;
            var totalTime = TimeAvailable*60;
            var actualTime = TimeAvailable*60 - RemainingSeconds;
            

            var resourceLoader = new Microsoft.Windows.ApplicationModel.Resources.ResourceLoader();
            var levelDesc = resourceLoader.GetString($"{Enum.GetName(typeof(GameLevels), gameLevels)}/Content");

            var rankInfo = new RankInfo
            {
                UserSetColumn =columns,
                UserSetRow =rows,
                ActualTime = actualTime,
                AvailableTime = totalTime,
                PlayTime = DateTime.Now,
                GameLevel = gameLevels,
                LevelDescription = $"{levelDesc}({rows}X{columns})",
                Score = (int)Math.Ceiling((rows * columns * 10) * ((double)RemainingSeconds / totalTime))+ (rows * columns),
                
            };

            var currentRank = AppData.RankInfoes.Count(x => x.Score > rankInfo.Score) + 1;

            AppData.RankInfoes.Add(rankInfo);

            AppData.RankInfoes = AppData.RankInfoes.OrderByDescending(x => x.Score).ToList();

            for (int i = 1; i <= AppData.RankInfoes.Count; i++)
            {
                AppData.RankInfoes[i-1].Rank = i;
            }

            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            localSettings.Values["RankInfoes"] = JsonSerializer.Serialize(AppData.RankInfoes.Take(10).ToList());
        }
        #endregion

        #region �Զ��巽��
        /// <summary>
        /// �趨��Ϸ��ͼƬ����
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        internal static int GetImageNumbers(int rows, int column)
        {
            int imaNum = 0;
            // ��������������������ʽ�Ӽ����ֵ��Ϊ1����ͼƬ����Ϊ2��ʹ��Ϸ���ḻ
            if ((rows == 2 && column == 2) || (rows == 3 && column == 2) || (rows == 2 && column == 3))
            {
                return 2;
            }
            imaNum = Convert.ToInt32(Math.Round(rows * column / 2 / 2 * 0.9));
            if (imaNum < 1) imaNum = 1;
            return imaNum;
        }
        /// <summary>
        /// ��ȡ��Ϸ�Ŀ���ʱ��
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        internal static int GetAvailableTime(int rows, int column)
        {
            int product = rows * column;
            int timeAvailable = Convert.ToInt32(Math.Round((double)product / 15));
            return timeAvailable;
        }
        #endregion

        public GameWindowPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            m_GameParameter = e.Parameter as GameOptions;
            this.UserSetRows = m_GameParameter.UserSetRows;
            this.UserSetColumns = m_GameParameter.UserSetColumns;

            this.Width += (this.UserSetColumns - 6) * 60;
            this.Height += (this.UserSetColumns - 6) * 55;

            this.ImageNumbers = GetImageNumbers(UserSetRows, UserSetColumns);
            this.TimeAvailable = GetAvailableTime(UserSetRows, UserSetColumns);

            SetGridRows();
            SetGridColumns();
            AddAndSetButtonPosition();

            List<ImageInfo> imageInfos = GetImageInfoList();
            List<ImageInfo> doubleImageInfo = GetDoubleImageInfoList(imageInfos);
            List<Button> buttons = GetButtonList();
            AddImageToUIRandomly(doubleImageInfo, imageInfos, GetRanomIndexList, buttons);
            FillImageInfoArray();
            FillAvailableChannels();
            FillButtonArray();

            foreach (var item in buttons)
            {
                item.Click += Item_Click;
                item.PointerEntered += Item_MouseEnter;
                item.PointerExited += Item_MouseLeave;
            }
            CountDown(TimeAvailable);

            base.OnNavigatedTo(e);
        }



        private void Item_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            // �ж��Ƿ���������ť��BorderThickness����ֵΪ5����Ϊ��ɫ���������û�������������δ��ƥ��İ�ť��ɵģ�
            // ���û���һ�ε����ťʱ����Ӱ������
            List<Button> buttons = FindIfBorderThickernessEqualFive(GetButtonList());
            if (buttons.Count == 2)
            {
                foreach (var item in buttons)
                {
                    item.BorderThickness = new Thickness(0);
                }
            }

            // ���֮ǰû�а�ť����
            if (!isFirstButtonClicked)
            {
                isFirstButtonClicked = true;
                button.BorderBrush = new SolidColorBrush(Colors.LawnGreen);
                button.BorderThickness = new Thickness(5);

                previousClickedButton = button;
            }
            else
            {
                // ������ΰ��µİ�ť��ͬ������֮ǰ�԰�ť���趨
                if (button == previousClickedButton)
                {
                    button.BorderThickness = new Thickness(0);
                    previousClickedButton = null;
                    isFirstButtonClicked = false;
                }
                // ������ΰ��µİ�ť��ͬ
                else
                {
                    // ��ȡ��ǰ�͵�ǰ��ť��������ͼƬ��Ϣ
                    ImageInfo previousImageInfo = imageInfoArray[GetRow(previousClickedButton), GetColumn(previousClickedButton)];
                    ImageInfo currentImageInfo = imageInfoArray[GetRow(button), GetColumn(button)];
                    // �������ͼƬ��ͬ������CheckIfMatch��������True
                    if (previousImageInfo.Id == currentImageInfo.Id && CheckIfMatch(previousClickedButton, button))
                    {
                        // �ڿ���ͨ���н�������ť��Ӧλ���ϵ�Ԫ����ΪTrue
                        availableChannels[GetRow(previousClickedButton) + 1, GetColumn(previousClickedButton) + 1] = true;
                        availableChannels[GetRow(button) + 1, GetColumn(button) + 1] = true;

                        // �������жϹ����ж԰�ť��۵�Ӱ��
                        previousClickedButton.Background = new SolidColorBrush(Colors.Transparent);
                        button.Background = new SolidColorBrush(Colors.Transparent);
                        previousClickedButton.BorderThickness = new Thickness(0);
                        button.BorderThickness = new Thickness(0);

                        // �ָ���û�а�ť�������״̬�����ð�ť
                        previousClickedButton.IsEnabled = false;
                        button.IsEnabled = false;
                    }
                    // �������������������������������ť�ϼ�һ���ɫ����߿�
                    else
                    {
                        button.BorderBrush = new SolidColorBrush(Colors.Red);
                        button.BorderThickness = new Thickness(5);
                        previousClickedButton.BorderBrush = new SolidColorBrush(Colors.Red);
                        previousClickedButton.BorderThickness = new Thickness(5);
                    }

                    // �ָ���û�а�ť�������״̬
                    previousClickedButton = null;
                    isFirstButtonClicked = false;
                }
            }

            // ����Ƿ����еİ�ť�ѱ�����
            if (CheckIfAllElementEliminated())
            {
                CreateGameCompluteUI(true);
            }

        }
        private void Item_MouseEnter(object sender, PointerRoutedEventArgs e)
        {
            Button button = sender as Button;
            int imageId = 0;
            try
            {
                imageId = imageInfoArray[GetRow(button), GetColumn(button)].Id;
            }
            catch (Exception)
            {
                return;
            }
            var resourceKey = "image" + imageId + "_OneColor";
            if (this.Resources.TryGetValue(resourceKey, out object obj))
            {
                button.Background = obj as ImageBrush ?? new ImageBrush();
            }
        }
        private void Item_MouseLeave(object sender, PointerRoutedEventArgs e)
        {
            Button button = sender as Button;
            int imageId = 0;
            try
            {
                imageId = imageInfoArray[GetRow(button), GetColumn(button)].Id;
            }
            catch (Exception)
            {
                return;
            }

            var resourceKey = "image" + imageId + "_FullColor";
            if (this.Resources.TryGetValue(resourceKey, out object obj))
            {
                button.Background = obj as ImageBrush ?? new ImageBrush();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (IsWantToExitApp)
            {
                Application.Current.Exit();
            }
        }

        private void Relayout_Click(object sender, RoutedEventArgs e)
        {
            if (IsInGameComplete)
            {
                return;
            }

            RearrangeImageToUIRandomly();

            List<Button> buttons = FindIfBorderThickernessEqualFive(GetButtonList());
            if (buttons.Count == 1 || buttons.Count == 2)
            {
                foreach (var item in buttons)
                {
                    item.BorderThickness = new Thickness(0);
                    previousClickedButton = null;
                    isFirstButtonClicked = false;
                }
            }
        }

        private void Prompt_Click(object sender, RoutedEventArgs e)
        {
            if (IsInGameComplete)
            {
                return;
            }
            Button[] buttons = TryToPrompt();
            if (buttons != null)
            {
                buttons[0].BorderBrush = new SolidColorBrush(Colors.Blue);
                buttons[0].BorderThickness = new Thickness(5);
                buttons[1].BorderBrush = new SolidColorBrush(Colors.Blue);
                buttons[1].BorderThickness = new Thickness(5);
            }
            else
            {
                //MessageBox.Show("�Ѿ�û�п�����Եİ�ť����ϵͳ�Զ�Ϊ�����˳��");
                Relayout_Click(sender, e);
            }
        }

        private void ReturnToManu_Click(object sender, RoutedEventArgs e)
        {
            //this.Hide();
            //Application.Current.MainWindow.Show();
            //IsWantToExitApp = false;
            //this.Close();
            Frame.Navigate(typeof(MainMenuPage));
        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            //progressBar.Value = 0;
            // ��ʼ��˽���ֶ�
            allImageInfoNeeded = null;
            imageInfoArray = null;
            buttonArray = null;
            isFirstButtonClicked = false;
            previousClickedButton = null;
            availableChannels = null;

            SetGridRows();
            SetGridColumns();
            AddAndSetButtonPosition();

            List<ImageInfo> imageInfos = GetImageInfoList();
            List<ImageInfo> doubleImageInfo = GetDoubleImageInfoList(imageInfos);
            List<Button> buttons = GetButtonList();
            AddImageToUIRandomly(doubleImageInfo, imageInfos, GetRanomIndexList, buttons);
            FillImageInfoArray();
            FillAvailableChannels();
            FillButtonArray();

            foreach (var item in buttons)
            {
                item.Click += Item_Click;
                item.PointerEntered += Item_MouseEnter;
                item.PointerExited += Item_MouseLeave;
            }
            timer.Stop();
            CountDown(TimeAvailable);

            IsInGameComplete = false;
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
