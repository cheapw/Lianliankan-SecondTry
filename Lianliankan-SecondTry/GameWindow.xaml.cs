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
        List<ImageInfo> allImageInfoNeeded = null;

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
            int substractIndex = count;
            for (int i = 0; i < count; i++)
            {
                int randNum=rand.Next(substractIndex);
                randomIndexList.Add(temp[randNum]);
                temp.Remove(temp[randNum]);
                substractIndex--;
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

            allImageInfoNeeded = new List<ImageInfo>();

            for (int i = 0; i < addRounds; i++)
            {
                for (int j = 0; j < doubleImageInfos.Count; j++)
                {
                    allImageInfoNeeded.Add(doubleImageInfos[j]);
                }
            }

            for (int i = 0; i < LeftoverImageNeeded.Count; i++)
            {
                allImageInfoNeeded.Add(LeftoverImageNeeded[i]);
            }

            List<int> allRandomIndex = GetRandomIndexListCallback(allImageInfoNeeded.Count);
            for (int i = 0; i < allImageInfoNeeded.Count; i++)
            {
                buttons[i].Background = allImageInfoNeeded[allRandomIndex[i]].Image;
                allImageInfoNeeded[allRandomIndex[i]]=SetLocation(buttons[i],allImageInfoNeeded[allRandomIndex[i]]);
            }
        }
        private ImageInfo SetLocation(Button button,ImageInfo imageInfo)
        {
            imageInfo.Row = Grid.GetRow(button);
            imageInfo.Column = Grid.GetColumn(button);
            return imageInfo;
        }
        #endregion
        public GameWindow()
        {
            InitializeComponent();
            //StringBuilder stringBuilder = new StringBuilder();
            //foreach (var item in GetRanomIndexList(5))
            //{
            //    stringBuilder.Append(item.ToString() + "  ");
            //}
            //MessageBox.Show(stringBuilder.ToString());
            List<ImageInfo> imageInfos=GetImageInfoList();
            List<ImageInfo> doubleImageInfo = GetDoubleImageInfoList(imageInfos);
            List<Button> buttons = GetButtonList();
            AddImageToUIRandomly(doubleImageInfo, imageInfos, GetRanomIndexList, buttons);

            //StringBuilder stringBuilder = new StringBuilder();
            //foreach (var item in allImageInfoNeeded)
            //{
            //    stringBuilder.Append(item.Row.ToString() + "X"+item.Column.ToString()+"\n");
            //}
            //MessageBox.Show(stringBuilder.ToString());
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.MainWindow.Close();
        }
    }
    //class ImageInfo
    //{
    //    public ImageBrush Image { get; set; }
    //    //public int Row { get; set; }
    //    //public int Column { get; set; }
    //    //public Point Location { get; set; }
    //    public List<Point> LocationList { get; set; }
    //    public int Id { get; set; }
    //}
    struct ImageInfo
    {
        public int Id { get; set; }
        public ImageBrush Image { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
    }
}
