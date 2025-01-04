using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RabbitImageView.entitys
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    internal class ImageInfo : ViewBaseInfo
    {
        public ImageInfo() { }

        //图片完整路径
        private string filePath;
        //图片所在文件夹路径
        private string fileDirPath;
        //图片长度 像素
        private double width;
        //图片高度 像素
        private double height;
        //图片展示长度 像素
        private double widthView;
        //图片展示高度 像素
        private double heightView;
        //图片文件名称
        private string fileName;
        //图片大小 kb
        private double fileSize;
        //图片大小 展示用
        private double fileSizeView;
        //图片大小单位 展示用
        private string fileSizeUnit;
        //图片控件展示路径
        private string viewImagePath;
        //GIF控件展示路径
        private string viewMediaElementPath;
        //标题栏文本
        private string titleText;
        //旋转角度
        private int angel;

        public int Angel { get => angel; set { angel = value; OnPropChanged(); } }

        public double Width { get => width; set { width = value; OnPropChanged(); } }
        
        public double Height { get => height; set { height = value; OnPropChanged(); } }
                
        public string FileName { get => fileName; set { fileName = value; OnPropChanged(); } }

        public string FilePath { get { return filePath; } set { filePath = value;OnPropChanged();} }

        public string FileDirPath { get => fileDirPath; set => fileDirPath = value; }

        public double FileSize { get => fileSize; set { fileSize = value; OnPropChanged(); } }

        public double FileSizeView { get => fileSizeView; set { fileSizeView = value; OnPropChanged(); } }

        public string FileSizeUnit { get => fileSizeUnit; set { fileSizeUnit = value; OnPropChanged(); } }

        public double WidthView { get => widthView; set { widthView = value; OnPropChanged(); } }

        public double HeightView { get => heightView; set { heightView = value; OnPropChanged(); } }

        public string ViewImagePath { get => viewImagePath; set { viewImagePath = value; OnPropChanged(); } }

        public string ViewMediaElementPath { get => viewMediaElementPath; set { viewMediaElementPath = value; OnPropChanged(); } }

        public string TitleText { get => titleText; set { titleText = value; OnPropChanged(); } }
    }
}
