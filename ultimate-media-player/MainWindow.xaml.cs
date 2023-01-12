using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using System.Windows.Threading;
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
                lvPlayList.Visibility = Visibility.Collapsed;
                media.Visibility = Visibility.Visible;
                media.Play();
                media.Stop();
            }
        }

        private void FullScreen(object sender, RoutedEventArgs e)
        {
            if (_isFullScreen)
            {
                _isFullScreen = false;
                this.WindowState = WindowState.Normal;
            }
            else
            {
                _isFullScreen = true;
                Main.Children.Remove(media);
                this.Content = media;
                //this.WindowStyle = WindowStyle.None;
                this.WindowState = WindowState.Maximized;
            }
        }

        private void Main_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            lvPlayList.Width = Main.ActualWidth;
            slider_video.Width = Main.ActualWidth - 200;
            double temp = (Main.ActualWidth - 440) / 2;
            mid_controller.Margin = new Thickness(temp+60, 0, temp-100, 0);
            title_column.Width = Main.ActualWidth-200;
        }

        public class Video{
            public String path { get; set; } = "";
            public String title  {
                get {
                    var infor = new FileInfo(path);
                    return infor.Name;
                }
            }
            public String duration { get; set; } = "";

        }

        public BindingList<Video> videoList { get; set; } = new BindingList<Video>();

        private void AddFileToPlaylist(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.Filter = 
            openFileDialog.Multiselect = true;
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string filename in openFileDialog.FileNames)
                {
                    Video video = new Video();
                    video.path = filename;
                    MediaPlayer mediaDuration = new MediaPlayer();
                    mediaDuration.Open(new Uri(filename));
                    while (!mediaDuration.NaturalDuration.HasTimeSpan) ;
                    int hours = mediaDuration.NaturalDuration.TimeSpan.Hours;
                    int minutes = mediaDuration.NaturalDuration.TimeSpan.Minutes;
                    int seconds = mediaDuration.NaturalDuration.TimeSpan.Seconds;
                    video.duration = $"{hours}:{minutes}:{seconds}";
                    videoList.Add(video);
                }
                lvPlayList.ItemsSource = videoList;
            }
        }

        private void media_MediaOpened(object sender, RoutedEventArgs e)
        {
            int hours = media.NaturalDuration.TimeSpan.Hours;
            int minutes = media.NaturalDuration.TimeSpan.Minutes;
            int seconds = media.NaturalDuration.TimeSpan.Seconds;
            duration.Text = $"{hours}:{minutes}:{seconds}";
        }
    }
}
