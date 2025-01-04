using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace RabbitImageView
{
    /// <summary>
    /// AboutRabbit.xaml 的交互逻辑
    /// </summary>
    public partial class AboutRabbit : Window
    {
        public AboutRabbit()
        {
            InitializeComponent();
        }

        //关闭当前窗口
        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //跳转链接
        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            //使用默认浏览器打开
            Process.Start(new ProcessStartInfo(link.NavigateUri.AbsoluteUri));
        }
    }
}
