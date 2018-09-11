using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace PCMgrUWP
{
    /// <summary>
    /// AboutWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AboutWindow : System.Windows.Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
#if _X64_
            ImageOSBit.Source = new BitmapImage(new Uri("pack://pack://application:,,,/PCMgrUWP64;component/Resources/img_x64.png", UriKind.RelativeOrAbsolute));
            LabelMoreDetals.Content = (string)LabelMoreDetals.Content + "\n64 Bit Version";
#else
            if (NativeMethods.MIs64BitOS())
            {
                ImageOSBit.Source = new BitmapImage(new Uri("pack://application:,,,/PCMgrUWP32;component/Resources/img_x32inx64.png", UriKind.RelativeOrAbsolute));
                LabelMoreDetals.Content = (string)LabelMoreDetals.Content + "\n32 Bit Version But in x64 OS";
            }
            else
            {
                LabelMoreDetals.Content = (string)LabelMoreDetals.Content + "\n32 Bit Version";
                ImageOSBit.Source = new BitmapImage(new Uri("pack://application:,,,/PCMgrUWP32;component/Resources/img_x32.png", UriKind.RelativeOrAbsolute));
            }
#endif

        }

        private void CloseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Close();
        }
        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            Process.Start(new ProcessStartInfo(link.NavigateUri.AbsoluteUri));
        }
        private void MoreHilpers_Click(object sender, RoutedEventArgs e)
        {
            GridMoreHelps.Height = Height;
            GridMoreHelps.Visibility = Visibility.Visible;
        }
        private void ButtonHideMoreHelp_Click(object sender, RoutedEventArgs e)
        {
            GridMoreHelps.Visibility = Visibility.Hidden;
        }
        private void License_Click(object sender, RoutedEventArgs e)
        {
            GridLicense.Height = Height;
            GridLicense.Visibility = Visibility.Visible;
        }
        private void ButtonHideLicense_Click(object sender, RoutedEventArgs e)
        {
            GridLicense.Visibility = Visibility.Hidden;
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            Height = 452;
        }
        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            Height = 507;
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
