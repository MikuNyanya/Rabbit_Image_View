using Microsoft.Win32;
using RabbitImageView.entitys;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;


namespace RabbitImageView
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainView : Window
    {
        //当前显示的图片信息
        ImageInfo viewImage;
        //记录同级图片列表
        private List<string> dirImgList = new List<string>();
        //同级图片数量
        private int dirImgListCount = 0;
        //当前展示图片在列表中的索引
        private int viewNowImgIndex = 0;
        //注册表名称
        private string RegKeyName = "RabbitImageViewReg";

        //显示模式
        //1.默认 按比例填充展示区 该状态下窗口尺寸变更时图片尺寸随之变更
        //2.缩放状态 放大或缩小 该状态图片尺寸不随着窗口尺寸变更而变更
        private int viewMode = 1;
        //1:1查看状态
        private bool originalMode = false;
        //拖拽状态
        private bool imageDragMode = false;
        //拖拽时起始位置
        private Point imageDragPoint = new Point();
        //拖拽时图片起始位置
        private double imageDragOriginalX = 0;
        private double imageDragOriginalY = 0;

        //滚轮缩放倍数
        private double imgMultiple = 0.1;
        //当前缩放倍数
        private double imgMultipleNow = 1;

        //直接加载的图片
        private string initImgPath = null;

        //右键菜单开启状态
        private bool isMainLMenuOpen = false;

        //关于兔子
        AboutRabbit aboutRabbit;

        //windows名称排序
        [System.Runtime.InteropServices.DllImport("Shlwapi.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        private static extern int StrCmpLogicalW(string psz1, string psz2);

        public MainView()
        {
            InitializeComponent();

            viewImage = new ImageInfo();
            this.DataContext = viewImage;
        }

        public MainView(string[] args)
        {
            InitializeComponent();
            
            //读取传入的图片路径
            if (args.Length >= 1)
            {
                initImgPath = args[0];
            }

            viewImage = new ImageInfo();
            this.DataContext = viewImage;
        }

        //文件拖拽完成时
        private void Element_DragDrop(object sender, DragEventArgs e)
        {
            //读取拖拽文件
            string fullPath = ((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();

            doViewImage(fullPath);
        }

        //文件拖拽到控件上时
        private void Element_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Link;
            else
                e.Effects = DragDropEffects.None;
        }

        #region 窗口事件相关
       
        //窗口初始化
        private void Window_Load(object sender, EventArgs e)
        {
            
            //读取上次窗口位置
            RegistryKey regUser = Registry.CurrentUser.OpenSubKey(RegKeyName);
            //如果获取不到注册表说明是第一次打开程序
            if(null != regUser)
            {
                int formMaximized = Convert.ToInt32(regUser.GetValue("formMaximized"));
                //如果窗口为最大化，则不需要继续读取位置信息
                if (formMaximized == 1)
                {
                    this.WindowState = WindowState.Maximized;
                }
                else
                {
                    int formLocationX = Convert.ToInt32(regUser.GetValue("FormLocationX"));
                    int formLocationY = Convert.ToInt32(regUser.GetValue("FormLocationY"));
                    this.Left = formLocationX;
                    this.Top = formLocationY;

                    int formWidth = Convert.ToInt32(regUser.GetValue("FormWidth"));
                    int formHeight = Convert.ToInt32(regUser.GetValue("FormHeigth"));
                    this.Width = formWidth;
                    this.Height = formHeight;
                }
            }
           
            //如果是直接点击文件打开的程序，则加载图片
            if (initImgPath != null)
            {
                doViewImage(initImgPath);
            }
        }

        //窗口标题栏拖拽移动
        private void Windows_DragMove(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        //窗口尺寸变化时
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //自定义最大化的补偿 最大化时微软会有8长度超出屏幕
            if (this.WindowState == WindowState.Normal)
            {
                this.Base_Grid.Margin = new Thickness(0);
            }
            else if (this.WindowState == WindowState.Maximized)
            {
                this.Base_Grid.Margin = new Thickness(8);
            }


            //如果是默认展示模式，居中显示，重新自适应尺寸
            if (viewMode == 1)
            {   
                viewImageRefresh();
                imgViewCenter();
            }

            //刷新图片显示模式
            refreshBitmapScalingMode();
        }

        //窗口最大化切换
        private void Windows_Maximized(object sender, RoutedEventArgs e)
        {  
            if (this.WindowState == WindowState.Normal)
            {
                //MessageBox.Show(this.Width + "," + this.Height + " " + this.Top + "," + this.Left);
                //全屏 遮挡任务栏

                //MessageBox.Show(this.Width + "," + this.Height + " " + this.Top + "," + this.Left);
                //最大化
                //保存当前窗口位置和大小信息
                //normalRc = new Rect(this.Top,this.Left,this.Width,this.Height);
                //获取工作区大小
                //Rect rc = SystemParameters.WorkArea;
                //MessageBox.Show(rc.Width + "," + rc.Height + " " + rc.Top + "," + rc.Left);

                //设置窗口位置
                //this.MaxHeight = rc.Height;
                //this.MaxWidth = rc.Width;
                //this.Top = 100;
                //this.Left = rc.Left;

                //MessageBox.Show(this.Width + "," + this.Height + " " + this.Top + "," + this.Left);

                this.WindowState = WindowState.Maximized;
            }
            else if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
        }

        //窗口最小化
        private void Windows_Minimized(object sender, RoutedEventArgs e)
        {   
            this.WindowState = WindowState.Minimized;
        }

        //关闭窗口
        private void Windows_Exit(object sender, RoutedEventArgs e)
        {
            //保存窗口最后位置信息
            RegistryKey regUser = Registry.CurrentUser;
            RegistryKey rabbitImgViewReg = regUser.CreateSubKey(RegKeyName);
            rabbitImgViewReg.SetValue("FormLocationX", Left);    //窗口位置x
            rabbitImgViewReg.SetValue("FormLocationY", Top);    //窗口位置y
            rabbitImgViewReg.SetValue("FormWidth", this.Width);     //窗口长度
            rabbitImgViewReg.SetValue("FormHeigth", this.Height);   //窗口高度
            rabbitImgViewReg.SetValue("FormMaximized", this.WindowState == WindowState.Maximized ? 1 : 0);   //窗口是否最大化

            //关闭进程
            Application.Current.Shutdown();
        }

        #endregion

        //图片展示
        private void doViewImage(string fileFullName)
        {
            if(null == fileFullName || fileFullName.Length <= 0)
            {
                return;
            }

            try
            {    
                viewImage = getImageFileInfo(fileFullName);

                //显示模式初始化
                viewMode = 1;
                //放大倍率初始化
                imgMultipleNow = 1;
                //旋转角度初始化
                viewImage.Angel = 0;

                //初始化图片控件大小 图片在不拉伸的情况下填充屏幕
                viewImageRefresh();
                //居中显示
                imgViewCenter();
                //刷新图片显示模式
                refreshBitmapScalingMode();

                //读取该文件同级目录下其他图片列表
                //针对一个文件夹下很多图片的情况，可能需要转为异步操作
                DirectoryInfo dirInfo = new DirectoryInfo(viewImage.FileDirPath);
                FileInfo[] fileList = dirInfo.GetFiles();
                //筛选图片文件列表，并保存到内存
                dirImgList.Clear();
                foreach (FileInfo fileTemp in fileList)
                {
                    //只判断后缀即可，无需更精确判断
                   string fileExtension = fileTemp.Extension;
                    bool isImg = false;
                    switch (fileExtension.ToLower())
                    {
                        case ".jpg":
                        case ".jpeg":
                        case ".jfif":
                        case ".png":
                        case ".webp":
                        case ".bmp":
                        case ".gif":
                            isImg = true;
                            break;
                    }
                    if (isImg)
                    {
                        dirImgList.Add(fileTemp.FullName);
                    }
                }

                //按照windows的名称排序方式排序
                dirImgList.Sort(StrCmpLogicalW);

                //获取当前展示的图片索引
                for (int i = 0; i < dirImgList.Count; i++)
                {
                    if (viewImage.FilePath.Equals(dirImgList[i]))
                    {   
                        viewNowImgIndex = i;
                        break;
                    }
                }

                dirImgListCount = dirImgList.Count;

                //改变窗口标题
                //viewImage.TitleText = String.Format("{0}  {1}x{2}  {3:F}{4}  ({5}/{6})",
                //    viewImage.FileName, viewImage.Width, viewImage.Height, viewImage.FileSizeView, viewImage.FileSizeUnit, viewNowImgIndex, dirImgListCount);
                viewImage.TitleText = String.Format("{0}  {1}x{2}  {3:F}{4}  ({5}/{6})",
                    viewImage.FileName, viewImage.Width, viewImage.Height, viewImage.FileSizeView, viewImage.FileSizeUnit, viewNowImgIndex+1, dirImgListCount);

                //刷新图片列表
                refreshImageThumbnailList();
            }
            catch (Exception e)
            {
                //无法展示的图片显示空白
                MessageBox.Show("打开图片异常"+e.Message);
            }
        }
      
        #region 右键菜单相关
        
        //打开图片
        private void Menu_Open_Image_File(object sender, RoutedEventArgs e)
        {
            //https://cloud.tencent.com/developer/article/1342576 gif性能问题

            OpenFileDialog openImageFileDialog = new OpenFileDialog();
            //只允许选择单个文件
            openImageFileDialog.Multiselect = false;
            //限定图片文件
            openImageFileDialog.Filter = "图片|*.BMP;*PNG;*JPEG;*.JPG;*.GIF;*.JFIF;*.WEBP|All files (*.*)|*.*";
            if (openImageFileDialog.ShowDialog() == true)
            {
                //展示图片
                doViewImage(openImageFileDialog.FileName);
            }
        }

        //打开图片所在目录
        private void Menu_Open_File_Dir(object sender, RoutedEventArgs e)
        {
            if(viewImage.FilePath == null || viewImage.FilePath.Length <= 0)
            {
                //当前没有打开的图片
                return;
            }

            System.Diagnostics.Process.Start("Explorer","/select," + viewImage.FilePath);
        }

        //关于兔子
        private void Menu_About_Rabbit(object sender, RoutedEventArgs e)
        {
        
            if (null == aboutRabbit) 
            { 
                aboutRabbit = new AboutRabbit();
            }
            
            aboutRabbit.ShowDialog();
        }

        #endregion

        #region 鼠标相关操作

        //左键按下
        private void Mouse_Left_Down(object sender, MouseButtonEventArgs e)
        {
            //记录当前鼠标相对于窗口展示区位置
            imageDragPoint = e.GetPosition(sender as Border);
            //viewImage.TitleText = imageDragPoint.X + " x " + imageDragPoint.Y;

            //记录当前图片位置
            imageDragOriginalX = Double.Parse(this.Img_View.GetValue(Canvas.LeftProperty).ToString());
            imageDragOriginalY = Double.Parse(this.Img_View.GetValue(Canvas.TopProperty).ToString());
            //锁定鼠标即不让鼠标选中其他元素 以解决鼠标移动过快 超出控件范围导致移动失效问题
            //(sender as Border).CaptureMouse();

            //如果当前为默认展示模式，则1:1放大
            if (viewMode == 1) {
                //标记进入了1:1放大
                originalMode = true;
                //也标记进入拖拽模式
                imageDragMode = true;

                //跟随鼠标所在位置进行放大
                imgZoomForMouse(e, 1, imgMultipleNow);
                return;
            }

            //非默认展示时，准备进入拖拽模式
            //如果点击时右键菜单还打开着，会导致取鼠标当前坐标异常，原因未查明，先绕过去
            if (!isMainLMenuOpen) { 
                imageDragMode = true;
            }
        }

        //左键抬起
        private void Mouse_Left_Up(object sender, MouseButtonEventArgs e)
        {
            //解除拖拽状态
            if (imageDragMode)
            {
                //取消锁定鼠标
                //(sender as Border).ReleaseMouseCapture();
                imageDragMode = false;
            }

            //还原左键按下1:1放大 如果没处于左键1:1放大状态，则不作操作
            if (viewMode == 1 && originalMode)
            {
                originalMode = false;
                //图片适应当前窗口大小
                viewImageRefresh();
                //居中显示
                imgViewCenter();
            }

            //刷新图片显示模式
            refreshBitmapScalingMode();
        }

        //鼠标移动
        private void Mouse_Move(object sender, MouseEventArgs e)
        {
            //viewImage.TitleText = string.Format("鼠标相对窗口：{0}x{1},相对图片：{2}x{3},图片坐标:{4},{5}",
            //    e.GetPosition(this.ImageViewPanel).X,
            //    e.GetPosition(this.ImageViewPanel).Y,
            //    e.GetPosition(this.Img_View).X,
            //    e.GetPosition(this.Img_View).Y,
            //    this.Img_View.GetValue(Canvas.LeftProperty).ToString(),
            //    this.Img_View.GetValue(Canvas.TopProperty).ToString()
            //);
            
            //拖拽状态
            if (imageDragMode)
            {
                //https://www.cnblogs.com/qsnn/p/17069430.html
                //取当前坐标与拖拽前坐标对比
                Point pointNow = e.GetPosition(sender as Border);
                //viewImage.TestText = "o:" + imageDragPoint.X + "." + imageDragPoint.Y + "  " + pointNow.X + "." + pointNow.Y;

                //设置图片位置
                this.Img_View.SetValue(Canvas.LeftProperty, imageDragOriginalX + (pointNow.X - imageDragPoint.X));
                this.Img_View.SetValue(Canvas.TopProperty, imageDragOriginalY + (pointNow.Y - imageDragPoint.Y));
                this.ME_View.SetValue(Canvas.LeftProperty, imageDragOriginalX + (pointNow.X - imageDragPoint.X));
                this.ME_View.SetValue(Canvas.TopProperty, imageDragOriginalY + (pointNow.Y - imageDragPoint.Y));
            }
        }

        //鼠标滚轮 缩放
        private void Mouse_Wheel(object sender, MouseWheelEventArgs e)
        {
            double imgMultipleBefore = imgMultipleNow;
            //变更显示模式
            viewMode = 2;
           
            if (e.Delta > 0)
            {
                //放大
                imgMultipleNow += imgMultiple;
            }
            else if (e.Delta < 0)
            {
                if (imgMultipleNow <= imgMultiple)
                {
                    return;
                }
                //缩小
                imgMultipleNow -= imgMultiple;
            }

            if (imgMultipleNow <= 0.1)
            {
                imgMultipleNow = 0.1;
            }

            imgZoomForMouse(e, imgMultipleNow, imgMultipleBefore);
            //刷新图片显示模式
            refreshBitmapScalingMode();
        }

        private void imgZoomForMouse(MouseEventArgs e,double imgMultipleNow,double imgMultipleBefore)
        {
            double viewX;
            double viewY;

            //图片旋转的时候，不跟随鼠标放大，这各种坐标太诡异了
            if (viewImage.Angel % 360 == 0) { 
                //鼠标相对于图片变化前的位置
                Point mImgPoint;
                if (null == viewImage.ViewImagePath || viewImage.ViewImagePath.Length <= 0)
                {
                    mImgPoint = e.GetPosition(this.ME_View);
                }
                else
                {
                    mImgPoint = e.GetPosition(this.Img_View);
                }
                double mousePointBeforeX = mImgPoint.X;
                double mousePointBeforeY = mImgPoint.Y;

                viewImage.WidthView = viewImage.Width * imgMultipleNow;
                viewImage.HeightView = viewImage.Height * imgMultipleNow;

                //鼠标相对于展示区位置
                Point mWindowPoint = e.GetPosition(this.ImageViewPanel);

                //根据变化前的长宽比，计算出变化后鼠标应该对应图片的所在位置
                double mImgPX = mousePointBeforeX / imgMultipleBefore * imgMultipleNow;
                double mImgPY = mousePointBeforeY / imgMultipleBefore * imgMultipleNow;

                //以当前鼠标位置为中心，重新设置图片位置
                //变化后的长宽
                viewX = mWindowPoint.X - mImgPX;
                viewY = mWindowPoint.Y - mImgPY;
            }
            else
            {
                //图片旋转中，以窗口中心点进行放大
                //获取窗口中心点
                double imgPanelX = this.ImageViewPanel.ActualWidth / 2;
                double imgPanelY = this.ImageViewPanel.ActualHeight / 2;

                //获取中心点相对于图片变化前的位置
                double imgViewX;
                double imgViewY;
                if (viewImage.ViewImagePath != "")
                {
                    imgViewX =  double.Parse(this.Img_View.GetValue(Canvas.LeftProperty).ToString());
                    imgViewY = double.Parse(this.Img_View.GetValue(Canvas.TopProperty).ToString());
                }
                else 
                {
                    imgViewX = double.Parse(this.ME_View.GetValue(Canvas.LeftProperty).ToString());
                    imgViewY = double.Parse(this.ME_View.GetValue(Canvas.TopProperty).ToString());
                }
                double pointBeforeX = imgPanelX - imgViewX;
                double pointBeforeY = imgPanelY - imgViewY;

                //放大图片
                viewImage.WidthView = viewImage.Width * imgMultipleNow;
                viewImage.HeightView = viewImage.Height * imgMultipleNow;

                //计算出变化后图片的所在位置
                double mImgPX = pointBeforeX / imgMultipleBefore * imgMultipleNow;
                double mImgPY = pointBeforeY / imgMultipleBefore * imgMultipleNow;

                //重新设置图片位置
                viewX = imgPanelX - mImgPX;
                viewY = imgPanelY - mImgPY;
            }

            this.Img_View.SetValue(Canvas.LeftProperty, viewX);
            this.Img_View.SetValue(Canvas.TopProperty, viewY);
            this.ME_View.SetValue(Canvas.LeftProperty, viewX);
            this.ME_View.SetValue(Canvas.TopProperty, viewY);

            //刷新图片当前位置
            imageDragOriginalX = viewX;
            imageDragOriginalY = viewY;
        }

        //右键菜单开启
        private void MainLMenu_Open(object sender, RoutedEventArgs e)
        {
            this.isMainLMenuOpen = true;
        }

        //右键菜单关闭
        private void MainLMenu_Close(object sender, RoutedEventArgs e)
        {
            this.isMainLMenuOpen = false;
        }

        #endregion

        //普通图片展示
        private void changeViewImagePath(string path)
        {
            viewImage.ViewImagePath = path;
            viewImage.ViewMediaElementPath = "";
        }

        //gif展示
        private void changeViewGifPath(string path)
        {
            viewImage.ViewImagePath = "";
            viewImage.ViewMediaElementPath = path;
        }

        //窗口尺寸变更时，图片大小变更
        private void viewImageRefresh()
        {
            if (null == viewImage || 0 == viewImage.Width)
            {
                return;
            }

            //只在适应窗口的展示模式下变更图片大小
            if (viewMode != 1)
            {
                return;
            }

            //获取窗口展示区尺寸，并应用于图片大小
            //但如果展示区尺寸大于图片尺寸，只展示图片原尺寸即可
            double viewPanelWidth = this.ImageViewPanel.ActualWidth;
            double viewPanelHeight = this.ImageViewPanel.ActualHeight;

            //1.图片尺寸整体小于展示区
            if (viewPanelWidth >= viewImage.Width && viewPanelHeight >= viewImage.Height)
            {
                //原始尺寸
                viewImage.WidthView = viewImage.Width;
                viewImage.HeightView = viewImage.Height;
                return;
            }

            //MessageBox.Show(viewPanelWidth + " x " + viewPanelHeight);
            //2.图片尺寸大于展示区 拿到合适的缩放比例
            double widthMultiple = viewPanelWidth / viewImage.Width;
            double heightMultiple = viewPanelHeight / viewImage.Height;
            imgMultipleNow = widthMultiple < heightMultiple ? widthMultiple : heightMultiple;
            viewImage.WidthView = viewImage.Width * imgMultipleNow;
            viewImage.HeightView = viewImage.Height * imgMultipleNow;
            //MessageBox.Show(viewImage.WidthView + " x " + viewImage.HeightView+" "+ imgMultipleNow);
        }

        //图片以当前尺寸居中
        private void imgViewCenter()
        {
            //获取图片展示区域的尺寸，目视范围，非实际的范围
            double viewPanelWidth = this.ImageViewPanel.ActualWidth;
            double viewPanelHeight = this.ImageViewPanel.ActualHeight;

            //展示区中心点
            double viewWidthCenter = viewPanelWidth / 2;
            double viewHeightCenter = viewPanelHeight / 2;
            //图片中心点
            double imgWidthCenter = viewImage.WidthView / 2;
            double imgHeightCenter = viewImage.HeightView / 2;

            //计算图片起始坐标
            double pointX = viewWidthCenter - imgWidthCenter;
            double pointY = viewHeightCenter - imgHeightCenter;

            //MessageBox.Show(pointX + " . " + pointY);

            //设置图片位置
            this.Img_View.SetValue(Canvas.LeftProperty, pointX);
            this.Img_View.SetValue(Canvas.TopProperty, pointY);
            this.ME_View.SetValue(Canvas.LeftProperty, pointX);
            this.ME_View.SetValue(Canvas.TopProperty, pointY);
        }

        //gif循环播放
        private void ME_View_MediaEnded(object sender, RoutedEventArgs e)
        {
            ((MediaElement)sender).Position = ((MediaElement)sender).Position.Add(TimeSpan.FromMilliseconds(1));
        }

        //获取图片文件信息
        private ImageInfo getImageFileInfo(string imagePath)
        {
            BitmapDecoder decoder = BitmapDecoder.Create(new Uri(imagePath, UriKind.Absolute), BitmapCreateOptions.DelayCreation, BitmapCacheOption.OnDemand);
            var frame = decoder.Frames[0];
            int imgWitdh = frame.PixelWidth;
            int imgHeight = frame.PixelHeight;

            //获取文件信息
            FileInfo imgFile = new FileInfo(imagePath);
            if (null == imgFile)
            {
                MessageBox.Show("找不到指定的图片文件！");
                return null;
            }
            //获取所在文件夹路径
            string imageDirPath = imgFile.DirectoryName;
            //获取文件名称
            string imageName = imgFile.Name;
            //获取文件大小
            double imgFileLength = imgFile.Length;
            double lengthView = imgFileLength / 1024;
            string lengthUnit = "KB";
            if (lengthView >= 1024)
            {
                lengthView = lengthView / 1024;
                lengthUnit = "MB";
            }
            if (lengthView >= 1024)
            {
                lengthView = lengthView / 1024;
                lengthUnit = "GB";
            }

            string fileExtension = imgFile.Extension;

            //ImageInfo imgInfo = new ImageInfo
            //{
            //    FileName = imageName,
            //    FilePath = imagePath,
            //    FileDirPath = imageDirPath,
            //    FileSize = imgFileLength,
            //    FileSizeView = lengthView,
            //    FileSizeUnit = lengthUnit,
            //    Width = imgWitdh,
            //    Height = imgHeight
            //};

            viewImage.FileName = imageName;
            viewImage.FilePath = imagePath;
            viewImage.FileDirPath = imageDirPath;
            viewImage.FileSize = imgFileLength;
            viewImage.FileSizeView = lengthView;
            viewImage.FileSizeUnit = lengthUnit;
            viewImage.Width = imgWitdh;
            viewImage.Height = imgHeight;
            if (fileExtension.ToLower().Equals(".gif"))
            {
                changeViewGifPath(viewImage.FilePath);
            }
            else
            {
                changeViewImagePath(viewImage.FilePath);
            }

            return viewImage;
        }

        #region 工具栏按钮

        //上一张图片
        private void Btn_left_Click(object sender, RoutedEventArgs e)
        {
            viewNowImgIndex -= 1;
            if (viewNowImgIndex < 0)
            {
                if (dirImgListCount > 0)
                {
                    viewNowImgIndex = dirImgListCount - 1;
                }
                else
                {
                    viewNowImgIndex = 0;
                }
            }

            try
            {
                doViewImage(dirImgList[viewNowImgIndex]);
            }
            catch (Exception)
            {
                MessageBox.Show("图片切换异常");
            }
        }

        //下一张图片
        private void Btn_right_Click(object sender, RoutedEventArgs e)
        {
            viewNowImgIndex += 1;
            if (viewNowImgIndex >= dirImgListCount)
            {
                viewNowImgIndex = 0;
            }

            try
            {
                doViewImage(dirImgList[viewNowImgIndex]);
            }
            catch (Exception)
            {
                MessageBox.Show("图片切换异常");
            }
        }

        //1:1放大
        private void Btn_Zoom_1_1(object sender, RoutedEventArgs e)
        {
            //1:1展示，但是鼠标抬起时不恢复原本图片
            viewMode = 2;
            viewImage.WidthView = viewImage.Width;
            viewImage.HeightView = viewImage.Height;
            imgMultipleNow = 1;

            //居中显示
            imgViewCenter();
            //刷新图片显示模式
            refreshBitmapScalingMode();
        }

        //适应窗口
        private void Btn_Zoom_Normal(object sender, RoutedEventArgs e)
        {
            viewMode = 1;
            //初始化图片控件大小 图片在不拉伸的情况下填充屏幕
            viewImageRefresh();
            //居中显示
            imgViewCenter();
            //刷新图片显示模式
            refreshBitmapScalingMode();
        }
        
        //逆时针旋转
        private void Btn_Rotation_Left(object sender, RoutedEventArgs e)
        {
            viewImage.Angel -= 45;
            
        }

        //顺时针旋转
        private void Btn_Rotation_Right(object sender, RoutedEventArgs e)
        {
            viewImage.Angel += 45;
        }

        #endregion

        #region 图片预览列表相关

        //刷新图片预览列表
        private void refreshImageThumbnailList()
        {
            int listCount = dirImgList.Count;
            this.Img_Thumbnail_1.Source = viewNowImgIndex - 3 >= 0 ? ImageThumbnail(dirImgList[viewNowImgIndex - 3], 40) : null;
            this.Img_Thumbnail_2.Source = viewNowImgIndex - 2 >= 0 ? ImageThumbnail(dirImgList[viewNowImgIndex - 2], 40) : null;
            this.Img_Thumbnail_3.Source = viewNowImgIndex - 1 >= 0 ? ImageThumbnail(dirImgList[viewNowImgIndex - 1], 40) : null;
            this.Img_Thumbnail_4.Source = ImageThumbnail(dirImgList[viewNowImgIndex - 0], 45);
            this.Img_Thumbnail_5.Source = viewNowImgIndex + 1 < listCount ? ImageThumbnail(dirImgList[viewNowImgIndex + 1], 40) : null;
            this.Img_Thumbnail_6.Source = viewNowImgIndex + 2 < listCount ? ImageThumbnail(dirImgList[viewNowImgIndex + 2], 40) : null;
            this.Img_Thumbnail_7.Source = viewNowImgIndex + 3 < listCount ? ImageThumbnail(dirImgList[viewNowImgIndex + 3], 40) : null;
        }

        //创建图片缩略图
        private BitmapImage ImageThumbnail(string imagePath, int thumbnailSize)
        {
            //获取缩放倍率
            return ImageThumbnail(imagePath, thumbnailSize, thumbnailSize);
        }

        //创建图片缩略图
        private BitmapImage ImageThumbnail(string imagePath, int thumbnailWidth,int thumbnailHeight)
        {
            //获取缩放倍率 要保持长宽比
            BitmapImage bitMapTemp = new BitmapImage();
            bitMapTemp.BeginInit();
            bitMapTemp.UriSource = new Uri(imagePath);
            bitMapTemp.EndInit();
            double originalWidth = bitMapTemp.Width;
            double originalHeight = bitMapTemp.Height;
            double widthMultiple = thumbnailWidth / originalWidth;
            double heightMultiple = thumbnailHeight / originalHeight;
            double imgMultiple = widthMultiple < heightMultiple ? widthMultiple : heightMultiple;

            BitmapImage myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(imagePath);
            myBitmapImage.DecodePixelWidth = (int)(originalWidth * imgMultiple);
            myBitmapImage.DecodePixelHeight = (int)(originalHeight * imgMultiple);
            myBitmapImage.EndInit();

            return myBitmapImage;
        }

        //刷新图片显示模式
        private void refreshBitmapScalingMode()
        {
            if (imgMultipleNow < 1) { 
                RenderOptions.SetBitmapScalingMode(this.Img_View,BitmapScalingMode.HighQuality);
            }
            else
            {
                RenderOptions.SetBitmapScalingMode(this.Img_View, BitmapScalingMode.NearestNeighbor);
            }
        }

        //缩略图点击
        private void ImgThumbnail_Click(object sender, RoutedEventArgs e)
        {
            Image temp = getFirstChild<Image>(sender as DependencyObject);
            if (null  == temp || null == temp.Source)
            {
                return;
            }
            string imgPathTemp = temp.Source.ToString();
            if (null == imgPathTemp || imgPathTemp == "")
            {
                return;
            }

            //获取该缩略图中的图片路径
            imgPathTemp = ((BitmapImage)temp.Source).UriSource.LocalPath;
            doViewImage(imgPathTemp);
        }

        //获取指类型的第一个子控件
        private T getFirstChild<T>(DependencyObject reference) where T : DependencyObject
        {
            DependencyObject child;
            T result;

            for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(reference) - 1; i++)
            {
                child = VisualTreeHelper.GetChild(reference, i);

                if (child is T)
                {
                    return (T)child;
                }
                else
                {
                    result = getFirstChild<T> (child);
                    return result;
                }

            }
            return null;
        }

        #endregion

        //按下键盘
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //上一个
            if (e.Key == Key.Left)
            {
                Btn_left_Click(null, null);
            }

            //下一个
            if (e.Key == Key.Right)
            {
                Btn_right_Click(null, null);
            }
        }
    }
}
