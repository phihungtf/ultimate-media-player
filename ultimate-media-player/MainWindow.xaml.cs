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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void ShowVideo_Click(object sender, RoutedEventArgs e)
        {
            if (lvPlayList.Visibility == Visibility.Visible)
            {
                lvPlayList.Visibility = Visibility.Collapsed;
                media.Visibility = Visibility.Visible;
            }
            else
            {
                lvPlayList.Visibility = Visibility.Visible;
                media.Visibility = Visibility.Collapsed;
            }
        }
        string _currentPlaying = "";
        bool _isFullScreen = false;
        private void Show(object sender, RoutedEventArgs e)
        {
            var screen = new OpenFileDialog();
            if (screen.ShowDialog() == true)
            {
                _currentPlaying = screen.FileName;
                media.Source = new Uri(_currentPlaying, UriKind.Absolute);
                media.Play();
            }
        }

        private void FullScreen(object sender, RoutedEventArgs e)
        {
            if (_isFullScreen)
            {
                _isFullScreen = false;
            }
            else
            {
                _isFullScreen = true;
                Main.Children.Remove(media);
                this.Content = media;
                this.WindowStyle = WindowStyle.None;
                this.WindowState = WindowState.Maximized;
            }
        }
    }
}
