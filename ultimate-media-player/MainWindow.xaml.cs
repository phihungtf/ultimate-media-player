using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
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
using static System.Net.Mime.MediaTypeNames;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public class Video
    {
        public String path { get; set; } = "";
        public String title
        {
            get
            {
                var infor = new FileInfo(path);
                return infor.Name;
            }
        }
        public String duration { get; set; } = "";
    }
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        

        public BindingList<Video> videoList { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            videoList = new BindingList<Video>();
            lvPlayList.ItemsSource = videoList;
        }
        
        private static string formatTimerString(int hours, int minutes, int seconds) {
            if (hours == 0) {
                return $"{minutes.ToString("00")}:{seconds.ToString("00")}";
            }
            return $"{hours.ToString("00")}:{minutes.ToString("00")}:{seconds.ToString("00")}";
        }
        
        private void ToggleVideo(object sender, RoutedEventArgs e)
        {
            if (lvPlayList.Visibility == Visibility.Visible)
            {
                lvPlayList.Visibility = Visibility.Collapsed;
                player.Visibility = Visibility.Visible;
            }
            else
            {
                lvPlayList.Visibility = Visibility.Visible;
                player.Visibility = Visibility.Collapsed;
            }
        }
        
        string _currentPlaying = "";
        bool _isFullScreen = false;
        DispatcherTimer _timer;
        private bool _playing = false;
        private bool _isMediaOpened = false;

        public event PropertyChangedEventHandler? PropertyChanged;

        private void openFile(object sender, RoutedEventArgs e)
        {
            var openMediaDialog = new OpenFileDialog();
            openMediaDialog.FileName = "Videos"; // Default file name
            openMediaDialog.DefaultExt = ".mp3;.mp4"; // Default file extension
            openMediaDialog.Filter = "Media Files|*.mp3;*.mp4|Video Files|*.mp4|Audio Files|*.mp3"; // Filter files by extension
            if (openMediaDialog.ShowDialog() == true)
            {
                _currentPlaying = openMediaDialog.FileName;
                player.Source = new Uri(_currentPlaying, UriKind.Absolute);
                lvPlayList.Visibility = Visibility.Collapsed;
                player.Visibility = Visibility.Visible;
                player.Play();
                player.Stop();

                _timer = new DispatcherTimer();
                _timer.Interval = new TimeSpan(0, 0, 0, 1, 0); ;
                _timer.Tick += _timer_Tick;

                playOrPause(null, null);
            }
        }

        private void _timer_Tick(object? sender, EventArgs e) {
            int hours = player.Position.Hours;
            int minutes = player.Position.Minutes;
            int seconds = player.Position.Seconds;
            currentPosition.Text = formatTimerString(hours, minutes, seconds);
            //Title = $"{hours}:{minutes}:{seconds}";
        }
        
        private void playOrPause(object? sender, RoutedEventArgs? e) {
            if (!_isMediaOpened) return;
            if (_playing) {
                player.Pause();
                _playing = false;
                playButton.Content = "\uE102";
                //Title = $"Stopped playing: {_shortName}";
                _timer.Stop();
            } else {
                _playing = true;
                player.Play();
                playButton.Content = "\uE103";
                //Title = $"Playing: {_shortName}";

                _timer.Start();
            }

            //var bitmap = new BitmapImage();
            //bitmap.BeginInit();
            //bitmap.UriSource = new Uri(@"Images/plus.png", UriKind.Relative);
            //bitmap.EndInit();

            //browseButtonIcon.Source = bitmap;

            //test.Source = bitmap;
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
                Main.Children.Remove(player);
                this.Content = player;
                //this.WindowStyle = WindowStyle.None;  // Nhớ bỏ cmt
                this.WindowState = WindowState.Maximized;
            }
        }
        
        private void Main_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            lvPlayList.Width = Main.ActualWidth;
            slider_video.Width = Main.ActualWidth - 200;
            double temp = (Main.ActualWidth - 440) / 2;
            mid_controller.Margin = new Thickness(temp+60, 0, temp-100, 0);
            title_column.Width = Main.ActualWidth - 200;
        }
        
        private void AddFileToPlaylist(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.Filter = 
            openFileDialog.Multiselect = true;
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == true)
                foreach (string filename in openFileDialog.FileNames)
                    addDuration(filename);
        }

        public void addDuration(string namefile )
        {
            Video video = new Video();
            video.path = namefile;
            MediaPlayer mediaDuration = new MediaPlayer();
            mediaDuration.Open(new Uri(namefile));
            while (!mediaDuration.NaturalDuration.HasTimeSpan) ;
            int hours = mediaDuration.NaturalDuration.TimeSpan.Hours;
            int minutes = mediaDuration.NaturalDuration.TimeSpan.Minutes;
            int seconds = mediaDuration.NaturalDuration.TimeSpan.Seconds;
            video.duration = formatTimerString(hours, minutes, seconds);
            videoList.Add(video);
        }

        private void addFile_Click(object sender, RoutedEventArgs e)
        {
            var screen = new OpenFileDialog();
            if (screen.ShowDialog() == true)
            {
                var select = screen.FileName;
                addDuration(select);
            }
        }

        private void DeleteFile_Click(object sender, RoutedEventArgs e)
        {
            Video t = (Video) lvPlayList.SelectedItem;
            if (t != null)
                videoList.Remove(t);
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            //videoList = new BindingList<Video>();
        }

        private void player_MediaOpened(object sender, RoutedEventArgs e) {
            _isMediaOpened = true;
            int hours = player.NaturalDuration.TimeSpan.Hours;
            int minutes = player.NaturalDuration.TimeSpan.Minutes;
            int seconds = player.NaturalDuration.TimeSpan.Seconds;
            durationLabel.Text = formatTimerString(hours, minutes, seconds);

            // cập nhật max value của slider
            //progressSlider.Maximum = player.NaturalDuration.TimeSpan.TotalSeconds;
        }
    }
}
