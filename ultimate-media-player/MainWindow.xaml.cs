using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
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
using Microsoft.VisualBasic;
using Microsoft.Win32;
//using Newtonsoft.Json;

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

    public class Status:INotifyPropertyChanged
    {
        public double position { get; set; } = 0;
        public double volume { get; set; } = 0;
        public String currentPlaying { get; set; } = "";
        public String PlaylistDirection { get; set; } = "";

        public BindingList<Video> recent { get; set; }= new BindingList<Video>();

        public event PropertyChangedEventHandler? PropertyChanged;
    }
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public BindingList<Video> videoList { get; set; }= new BindingList<Video>();
        public Playlist playlist { get; set; }= new Playlist();
        public Status status { get; set; } = new Status();

        public MainWindow()
        {
            InitializeComponent();

            DispatcherTimer _timer;
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            _timer.Tick += _timer_Tick;

            _timer.Start();
            
            //videoList = new BindingList<Video>();
            lvPlayList.ItemsSource = playlist.list;
        }
        
        private static string TimeSpan2String(TimeSpan timeSpan) {
            if (timeSpan.Hours == 0) {
                return timeSpan.ToString(@"mm\:ss");
            }
            return timeSpan.ToString(@"hh\:mm\:ss");
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
        String curent = "";
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
                progressSlider.Maximum = player.NaturalDuration.TimeSpan.TotalMilliseconds;
                progressSlider.Value = player.Position.TotalMilliseconds;
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
                this.WindowState = WindowState.Maximized;
            }
        }
        
        private void Main_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            lvPlayList.Width = Main.ActualWidth;
            progressSlider.Width = Main.ActualWidth - 200;
            double temp = (Main.ActualWidth - 440) / 2;
            mid_controller.Margin = new Thickness(temp-20, 0, temp-100, 0);
            title_column.Width = Main.ActualWidth - 200;
            //lvMenuitem.Width = menulist.ActualWidth;
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
            mediaPlayerIsPlaying = false;
            video.path = namefile;
            MediaPlayer mediaDuration = new MediaPlayer();
            mediaDuration.Open(new Uri(namefile));
            while (!mediaDuration.NaturalDuration.HasTimeSpan) ;
            video.duration = TimeSpan2String(mediaDuration.NaturalDuration.TimeSpan);
            playlist.list.Add(video);
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
            Video t = (Video)lvPlayList.SelectedItem;
            if (t != null)
                playlist.list.Remove(t);
        }

        private void player_MediaOpened(object sender, RoutedEventArgs e) {
            _isMediaOpened = true;
            durationLabel.Text = TimeSpan2String(player.NaturalDuration.TimeSpan);
        }

        private void progressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            double value = progressSlider.Value;
            TimeSpan newPosition = TimeSpan.FromMilliseconds(value);
            currentPosition.Text = TimeSpan2String(newPosition);

            if (userIsDraggingSlider) {
                player.Position = TimeSpan.FromMilliseconds(progressSlider.Value);
            }
        }

        private void progressSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e) {
            userIsDraggingSlider = true;
            player.Pause();
        }

        private void progressSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e) {
            userIsDraggingSlider = false;
            player.Position = TimeSpan.FromMilliseconds(progressSlider.Value);
            player.Play();
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
            _currentPlaying = video.path;
            status.recent.Add(video);
            _isMediaOpened = true;
            player.Source = new Uri(video.path);
            lvPlayList.Visibility = Visibility.Collapsed;
            player.Visibility = Visibility.Visible;
            player.Play();
            mediaPlayerIsPlaying = true;
        }

        private void shuffleMode_Click(object sender, RoutedEventArgs e)
        {
            if(playlist.list.Count()==0) return;
            Random random = new Random();
            int t = random.Next(0,playlist.list.Count());
            Video video = playlist.list[t];
            player.Source = new Uri(video.path);
            _currentPlaying = video.path;
            status.recent.Add(video);
            lvPlayList.Visibility = Visibility.Collapsed;
            player.Visibility = Visibility.Visible;
            player.Play();
        }

        private void prevVideo_click(object sender, RoutedEventArgs e)
        {
            int t = 0;
            if(playlist.list.Count()<=1) return ;
            Video video = new Video();
            foreach (var item in playlist.list)
                if (item.path == _currentPlaying)
                    video = item;
            t = playlist.list.IndexOf(video);
            if(t>=1)
            {   video = playlist.list[t-1];
                player.Source = new Uri(video.path);
                _currentPlaying=video.path;
                status.recent.Add(video);
                lvPlayList.Visibility = Visibility.Collapsed;
                player.Visibility = Visibility.Visible;
                player.Play();
            }
        }
        private void NextVideo_click(object sender, RoutedEventArgs e)
        {
            int t = 0;
            if (playlist.list.Count() <=1) return;
            Video video = new Video();
            foreach (var item in playlist.list)
                if (item.path == _currentPlaying)
                    video = item;
            t = playlist.list.IndexOf(video);
            if (t < playlist.list.Count() - 1)
            {
                video = playlist.list[t+1];
                player.Source = new Uri(video.path);
                _currentPlaying = video.path;
                status.recent.Add(video);
                lvPlayList.Visibility = Visibility.Collapsed;
                player.Visibility = Visibility.Visible;
                player.Play();
            }
        }
        private bool isMute = true;
        private double volumnStorage;

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            player.Volume = (double)sliderVolumn.Value;
            if (sliderVolumn.Value == 0) { 
                MuteBtn.Visibility = Visibility.Visible;
                SoundBtn.Visibility = Visibility.Collapsed;
                isMute = true;
            }
            else
            {
                MuteBtn.Visibility = Visibility.Collapsed;
                SoundBtn.Visibility = Visibility.Visible;
                isMute = false;
            }
        }

        private void Handle_volumn(object sender, RoutedEventArgs e)
        {
            if(isMute)
            {
                sliderVolumn.Value = volumnStorage;
                MuteBtn.Visibility = Visibility.Collapsed;
                SoundBtn.Visibility = Visibility.Visible;
                isMute = false;
            }
            else
            {
                volumnStorage = sliderVolumn.Value;
                sliderVolumn.Value = 0;
                MuteBtn.Visibility = Visibility.Visible;
                SoundBtn.Visibility = Visibility.Collapsed;
                isMute = true;
            }
        }

        private void SavePlaylist_Click(object sender, RoutedEventArgs e)
        {
            string exeFolder = AppDomain.CurrentDomain.BaseDirectory;
            var pathPlaylist = $"{exeFolder}Playlists\\";
            while (playlist.name == "")
            {
                var name = Interaction.InputBox("Name invalid!\nYou should rename for Playlist!\nCancle to exit.", "Notification", "name");
                if (name != "")
                    playlist.name = name;
                return;
            }
            while (File.Exists($"{pathPlaylist}{playlist.name}.json"))
            {
                var result=MessageBox.Show("Playlist existed!\nDo you want to repalce it?","Notificatin",MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    break;
                }
                return;
            }
            Directory.CreateDirectory(pathPlaylist);
            status.currentPlaying = _currentPlaying;
            status.volume = sliderVolumn.Value;
            status.position = progressSlider.Value;
            status.PlaylistDirection = $"{pathPlaylist}{playlist.name}.json";
            string json = JsonSerializer.Serialize(playlist);
            File.WriteAllText(status.PlaylistDirection, json);
            string preload = JsonSerializer.Serialize(status);
            var path = $"{exeFolder}Preload.json";
            File.WriteAllText(path, preload);
        }

        private void OpenPlaylist_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Json files (*.json) | *.json";
            string exeFolder = AppDomain.CurrentDomain.BaseDirectory;
            var path = $"{exeFolder}Playlists\\";
            Directory.CreateDirectory(path);
            openFileDialog.InitialDirectory = path;
            if (openFileDialog.ShowDialog() == true)
            {
                using (StreamReader r = new StreamReader(openFileDialog.FileName))
                {
                    string json = r.ReadToEnd();
                    playlist = JsonSerializer.Deserialize<Playlist>(json);
                    lvPlayList.ItemsSource = playlist.list;
                    NameList.Text = playlist.name;
                    NamePlaylistCurrent.Visibility = Visibility.Visible;
                }
                
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if(playlist.list.Count()!=0||playlist.name!="")
            {
                var result = MessageBox.Show("Do you want to save? ", "Notification", MessageBoxButton.YesNo);
                if (MessageBoxResult.No==result) return;
                this.SavePlaylist_Click(sender, null);
            }    
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            string exeFolder = AppDomain.CurrentDomain.BaseDirectory;
            using (StreamReader r = new StreamReader($"{exeFolder}Preload.json"))
            {
                string json = r.ReadToEnd();
                status = JsonSerializer.Deserialize<Status>(json);
                if (status.currentPlaying != "") player.Source = new Uri(status.currentPlaying, UriKind.Absolute);
                sliderVolumn.Value = status.volume;
                progressSlider.Value = status.position;
                lvMenuitem.ItemsSource = status.recent;
            }
        }
    }
}
