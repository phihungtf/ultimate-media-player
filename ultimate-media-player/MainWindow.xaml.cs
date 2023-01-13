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

    public class Playlist: INotifyPropertyChanged
    {
        public String name { get; set; } = "";
        public BindingList<Video> list { get; set; } = new BindingList<Video>();

        public event PropertyChangedEventHandler? PropertyChanged;
    }
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public BindingList<Video> videoList { get; set; }= new BindingList<Video>();
        public Playlist playlist { get; set; }= new Playlist();
        public MainWindow()
        {
            InitializeComponent();

            DispatcherTimer _timer;
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 1, 0); ;
            _timer.Tick += _timer_Tick;

            _timer.Start();
            
            videoList = new BindingList<Video>();
            lvPlayList.ItemsSource = videoList;
        }
        
        private static string TimeSpan2String(TimeSpan timeSpan) {
            if (timeSpan.Hours == 0) {
                return timeSpan.ToString(@"mm\:ss");
            }
            return timeSpan.ToString(@"hh:\mm\:ss");
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
        private bool _isMediaOpened = false;
        private bool mediaPlayerIsPlaying = false;
        private bool mediaPlayerIsPausing = true;
        private bool userIsDraggingSlider = false;

        public event PropertyChangedEventHandler? PropertyChanged;

        private void _timer_Tick(object? sender, EventArgs e) {
            if ((player.Source != null) && (player.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider)) {
                // cập nhật value của slider
                progressSlider.Minimum = 0;
                progressSlider.Maximum = player.NaturalDuration.TimeSpan.TotalSeconds;
                progressSlider.Value = player.Position.TotalSeconds;
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
                Main.Children.Remove(player);
                this.Content = player;
                //this.WindowStyle = WindowStyle.None;  // Nhớ bỏ cmt
                this.WindowState = WindowState.Maximized;
            }
        }
        
        private void Main_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            lvPlayList.Width = Main.ActualWidth;
            progressSlider.Width = Main.ActualWidth - 200;
            double temp = (Main.ActualWidth - 440) / 2;
            mid_controller.Margin = new Thickness(temp+60, 0, temp-100, 0);
            title_column.Width = Main.ActualWidth - 200;
        }
        
        private void AddFileToPlaylist(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Media Files|*.mp3;*.mp4|Video Files|*.mp4|Audio Files|*.mp3";
            openFileDialog.Multiselect = true;
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == true)
                foreach (string filename in openFileDialog.FileNames)
                    addDuration(filename);
        }

        public void addDuration(string namefile )
        {
            Video video = new Video();
            _playing = false;
            video.path = namefile;
            MediaPlayer mediaDuration = new MediaPlayer();
            mediaDuration.Open(new Uri(namefile));
            while (!mediaDuration.NaturalDuration.HasTimeSpan) ;
            video.duration = TimeSpan2String(mediaDuration.NaturalDuration.TimeSpan);
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
                playlist.list.Remove(t);
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            //videoList = new BindingList<Video>();
        }

        private void player_MediaOpened(object sender, RoutedEventArgs e) {
            _isMediaOpened = true;
            durationLabel.Text = TimeSpan2String(player.NaturalDuration.TimeSpan);
        }

        private void progressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            currentPosition.Text = TimeSpan2String(TimeSpan.FromSeconds(progressSlider.Value));
        }

        private void progressSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e) {
            userIsDraggingSlider = true;
        }

        private void progressSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e) {
            userIsDraggingSlider = false;
            player.Position = TimeSpan.FromSeconds(progressSlider.Value);
        }

        private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e) {
            var openMediaDialog = new OpenFileDialog();
            openMediaDialog.FileName = "Videos"; // Default file name
            openMediaDialog.DefaultExt = ".mp3;.mp4"; // Default file extension
            openMediaDialog.Filter = "Media Files|*.mp3;*.mp4|Video Files|*.mp4|Audio Files|*.mp3"; // Filter files by extension

            if (openMediaDialog.ShowDialog() == true) {
                _currentPlaying = openMediaDialog.FileName;
                player.Source = new Uri(_currentPlaying, UriKind.Absolute);
                lvPlayList.Visibility = Visibility.Collapsed;
                player.Visibility = Visibility.Visible;

                //Auto Play
                player.Play();
                playButton.Visibility = Visibility.Collapsed;
                pauseButton.Visibility = Visibility.Visible;
                mediaPlayerIsPlaying = true;
            }
        }

        private void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = (player != null) && (player.Source != null);
        }

        private void Play_Executed(object sender, ExecutedRoutedEventArgs e) {
            player.Play();
            playButton.Visibility = Visibility.Collapsed;
            pauseButton.Visibility = Visibility.Visible;
            mediaPlayerIsPlaying = true;
        }


        private void Pause_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = mediaPlayerIsPlaying;
        }

        private void Pause_Executed(object sender, ExecutedRoutedEventArgs e) {
            player.Pause();
            playButton.Visibility = Visibility.Visible;
            pauseButton.Visibility = Visibility.Collapsed;
        }

        private void Stop_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = mediaPlayerIsPlaying;
        }

        private void Stop_Executed(object sender, ExecutedRoutedEventArgs e) {
            player.Stop();
            playButton.Visibility = Visibility.Visible;
            pauseButton.Visibility = Visibility.Collapsed;
            mediaPlayerIsPlaying = false;
        }

        private void PlaylistNameBtn_Click(object sender, RoutedEventArgs e)
        {
            playlist.name = PlaylistName.Text;
            addPlaylist.Visibility = Visibility.Collapsed;
            Main.Children.Remove(addPlaylist);
            NameList.Text = PlaylistName.Text;
            NamePlaylistCurrent.Visibility = Visibility.Visible;
        }

        private void NewPlaylist_Click(object sender, RoutedEventArgs e)
        {
            addPlaylist.Visibility = Visibility.Visible;
        }

        private void PlayFile_Click(object sender, RoutedEventArgs e)
        {
            if (lvPlayList.SelectedItem == null)  return;
            Video video = (Video)lvPlayList.SelectedItem;
            _currentPlaying = video.title;
            _isMediaOpened = true;
            player.Source = new Uri(video.path);
            lvPlayList.Visibility = Visibility.Collapsed;
            player.Visibility = Visibility.Visible;
            player.Play();
            //player.Stop();

            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 1, 0); ;
            _timer.Tick += _timer_Tick;

            _timer.Start();
        }


    }
}
