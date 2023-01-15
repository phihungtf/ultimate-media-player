using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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
using System.Xml.Linq;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using Microsoft.WindowsAPICodePack.Shell;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;
//using Newtonsoft.Json;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public class Video: INotifyPropertyChanged
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

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public class Playlist: INotifyPropertyChanged
    {
        public String name { get; set; } = "";
        public BindingList<Video> list { get; set; } = new BindingList<Video>();

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public class Status: INotifyPropertyChanged
    {
        public double position { get; set; } = 0;
        public double volume { get; set; } = 0;
        public String currentPlaying { get; set; } = "";
        public String PlaylistDirection { get; set; } = "";

        public BindingList<Video> recent { get; set; } = new BindingList<Video>();

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public Playlist playlist { get; set; } = new Playlist();
        public Status status { get; set; } = new Status();

        public MainWindow()
        {
            InitializeComponent();

            DispatcherTimer _timer;
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            _timer.Tick += _timer_Tick;

            _timer.Start();
            
            lvPlayList.ItemsSource = playlist.list;
        }
        
        private static string TimeSpan2String(TimeSpan timeSpan) {
            if (timeSpan.Hours == 0) {
                return timeSpan.ToString(@"mm\:ss");
            }
            return timeSpan.ToString(@"hh\:mm\:ss");
        }

        private static TimeSpan GetVideoDuration(string filePath) {
            using (var shell = ShellObject.FromParsingName(filePath)) {
                IShellProperty prop = shell.Properties.System.Media.Duration;
                var t = (ulong)prop.ValueAsObject;
                return TimeSpan.FromTicks((long)t);
            }
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
        
        Random random = new Random();
        String curent = "";
        string _currentPlaying = "";
        bool _isFullScreen = false;
        private bool _isMediaOpened = false;
        private bool mediaPlayerIsPlaying = false;
        private bool mediaPlayerIsPausing = false;
        private bool userIsDraggingSlider = false;
        private bool isRandom = false;
        private bool isLoop = false;
        WindowState windowState;

        public event PropertyChangedEventHandler? PropertyChanged;

        private void _timer_Tick(object? sender, EventArgs e) {
            if ((player.Source != null) && (player.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider)) {
                // cập nhật value của slider
                progressSlider.Minimum = 0;
                progressSlider.Maximum = player.NaturalDuration.TimeSpan.TotalMilliseconds;
                progressSlider.Value = player.Position.TotalMilliseconds;
            }
        }
        
        private void Main_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            lvPlayList.Width = Main.ActualWidth;
            progressSlider.Width = Main.ActualWidth - 200;
            //double temp = (Main.ActualWidth - 440) / 2;
            //mid_controller.Margin = new Thickness(temp-20, 0, temp-100, 0);
            title_column.Width = Main.ActualWidth - 210;
            btns.Width = Main.ActualWidth - 50;
            //lvMenuitem.Width = menulist.ActualWidth;
        }

        public void addToPlaylist(string namefile)
        {
            Video video = new Video();
            video.path = namefile;
            var duration = GetVideoDuration(namefile);
            video.duration = TimeSpan2String(duration);
            playlist.list.Add(video);
        }

        private void addFile_Click(object sender, RoutedEventArgs e)
        {
            var screen = new OpenFileDialog();
            if (screen.ShowDialog() == true)
            {
                var select = screen.FileName;
                addToPlaylist(select);
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
            sliderVolumn.Value = player.Volume;
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
            openMediaDialog.DefaultExt = ".mp3;.mp4"; // Default file extension
            openMediaDialog.Filter = "Media Files|*.mp3;*.mp4|Video Files|*.mp4|Audio Files|*.mp3"; // Filter files by extension

            if (openMediaDialog.ShowDialog() == true) {
                _currentPlaying = openMediaDialog.FileName;
                player.Source = new Uri(_currentPlaying, UriKind.Absolute);
                Video video = new Video() { path = _currentPlaying};
                status.recent.Add(video);
                lvPlayList.Visibility = Visibility.Collapsed;
                player.Visibility = Visibility.Visible;

                //Auto Play
                player.Play();
                playButton.Visibility = Visibility.Collapsed;
                pauseButton.Visibility = Visibility.Visible;
                mediaName.Text = video.title.Length <= 30 ? video.title : (video.title.Substring(0, 30) + "...");
                mediaPlayerIsPlaying = true;
            }
        }

        private void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = (player != null) && (player.Source != null) && mediaPlayerIsPausing;
        }

        private void Play_Executed(object sender, ExecutedRoutedEventArgs e) {
            player.Play();
            playButton.Visibility = Visibility.Collapsed;
            pauseButton.Visibility = Visibility.Visible;
            mediaPlayerIsPlaying = true;
            mediaPlayerIsPausing = false;
        }

        private void Pause_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = mediaPlayerIsPlaying && !mediaPlayerIsPausing;
        }

        private void Pause_Executed(object sender, ExecutedRoutedEventArgs e) {
            player.Pause();
            playButton.Visibility = Visibility.Visible;
            pauseButton.Visibility = Visibility.Collapsed;
            mediaPlayerIsPausing = true;
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
            mediaName.Text = playlist.name + "/" + video.title;
            playButton.Visibility = Visibility.Collapsed;
            pauseButton.Visibility = Visibility.Visible;
            mediaPlayerIsPlaying = true;
            mediaPlayerIsPausing = false;
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

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if(playlist.list.Count()!=0||playlist.name!="")
            {
                var result = MessageBox.Show("Do you want to save? ", "Notification", MessageBoxButton.YesNo);
                if (MessageBoxResult.No==result) return;
                SavePlaylist();
            }    
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            string exeFolder = AppDomain.CurrentDomain.BaseDirectory;
            if(File.Exists($"{exeFolder}Preload.json"))
            using (StreamReader r = new StreamReader($"{exeFolder}Preload.json"))
            {
                string json = r.ReadToEnd();
                status = JsonSerializer.Deserialize<Status>(json);
                if (status.currentPlaying != "") player.Source = new Uri(status.currentPlaying, UriKind.Absolute);
                sliderVolumn.Value = status.volume;
                progressSlider.Value = status.position;
                lvMenuitem.ItemsSource = status.recent;
            }
            else
            {
                string preload = JsonSerializer.Serialize(status);
                var path = $"{exeFolder}Preload.json";
                File.WriteAllText(path, preload);
            }
        }

        private void Erase_RecentVideo(object sender, RoutedEventArgs e)
        {
            status.recent.Clear();
        }

        private void FullScreen_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = mediaPlayerIsPlaying;
        }

        private void FullScreen_Executed(object sender, ExecutedRoutedEventArgs e) {
            if (_isFullScreen) {
                _isFullScreen = false;
                menu.Visibility = Visibility.Visible;
                controller.Visibility = Visibility.Visible;
                this.WindowStyle = WindowStyle.SingleBorderWindow;
                this.WindowState = windowState;
            } else {
                _isFullScreen = true;
                menu.Visibility = Visibility.Collapsed;
                //controller.Visibility = Visibility.Collapsed;
                this.WindowStyle = WindowStyle.None;
                windowState = this.WindowState;
                this.WindowState = WindowState.Minimized;
                this.WindowState = WindowState.Maximized;
                // Toggle Video
                lvPlayList.Visibility = Visibility.Collapsed;
                player.Visibility = Visibility.Visible;
            }
        }

        private void MuteVolume_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = mediaPlayerIsPlaying;
        }

        private void MuteVolume_Executed(object sender, ExecutedRoutedEventArgs e) {
            if (isMute) {
                sliderVolumn.Value = volumnStorage;
                MuteBtn.Visibility = Visibility.Collapsed;
                SoundBtn.Visibility = Visibility.Visible;
                isMute = false;
            } else {
                volumnStorage = sliderVolumn.Value;
                sliderVolumn.Value = 0;
                MuteBtn.Visibility = Visibility.Visible;
                SoundBtn.Visibility = Visibility.Collapsed;
                isMute = true;
            }
        }

        private void NewPlaylist_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = playlist.name == "";
        }

        private void NewPlaylist_Executed(object sender, ExecutedRoutedEventArgs e) {
            var name = Interaction.InputBox("Name your playlist.\nCancel to exit.", "Notification", "name");
            if (name == "") return;
            playlist.name = name;
            mediaName.Text = name;
        }

        private void AddToPlaylist_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = playlist.name != "";
        }

        private void AddToPlaylist_Executed(object sender, ExecutedRoutedEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Media Files|*.mp3;*.mp4|Video Files|*.mp4|Audio Files|*.mp3";
            openFileDialog.DefaultExt = ".mp3;.mp4"; // Default file extension
            openFileDialog.Multiselect = true;
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == true)
                foreach (string filename in openFileDialog.FileNames)
                    addToPlaylist(filename);
        }

        private void SavePlaylist() {
            string exeFolder = AppDomain.CurrentDomain.BaseDirectory;
            var pathPlaylist = $"{exeFolder}Playlists\\";

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

        private void SavePlaylist_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = playlist.name != "";
        }

        private void SavePlaylist_Executed(object sender, ExecutedRoutedEventArgs e) {
            SavePlaylist();
        }

        private void OpenPlaylist_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        private void OpenPlaylist_Executed(object sender, ExecutedRoutedEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Json files (*.json) | *.json";
            string exeFolder = AppDomain.CurrentDomain.BaseDirectory;
            var path = $"{exeFolder}Playlists\\";
            Directory.CreateDirectory(path);
            openFileDialog.InitialDirectory = path;
            if (openFileDialog.ShowDialog() == true) {
                using (StreamReader r = new StreamReader(openFileDialog.FileName)) {
                    string json = r.ReadToEnd();
                    playlist = JsonSerializer.Deserialize<Playlist>(json);
                    lvPlayList.ItemsSource = playlist.list;
                    NameList.Text = playlist.name;
                    NamePlaylistCurrent.Visibility = Visibility.Visible;
                }
            }
        }

        private void Next() {
            int t = 0;
            if (playlist.list.Count() <= 1) return;
            Video video = new Video();
            foreach (var item in playlist.list)
                if (item.path == _currentPlaying)
                    video = item;
            t = playlist.list.IndexOf(video) + 1;
            if (isRandom) t = random.Next(0, playlist.list.Count());
            if (t < playlist.list.Count()) {
                video = playlist.list[t];
                player.Source = new Uri(video.path);
                _currentPlaying = video.path;
                status.recent.Add(video);
                lvPlayList.Visibility = Visibility.Collapsed;
                player.Visibility = Visibility.Visible;
                player.Play();
                playButton.Visibility = Visibility.Collapsed;
                pauseButton.Visibility = Visibility.Visible;
                mediaName.Text = video.title.Length <= 30 ? video.title : (video.title.Substring(0, 30) + "...");
                mediaPlayerIsPlaying = true;
            }
        }

        private void Next_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = playlist.name != "";
        }

        private void Next_Executed(object sender, ExecutedRoutedEventArgs e) {
            Next();
        }

        private void Previous() {
            int t = 0;
            if (playlist.list.Count() <= 1) return;
            Video video = new Video();
            foreach (var item in playlist.list)
                if (item.path == _currentPlaying)
                    video = item;
            t = playlist.list.IndexOf(video) - 1;
            if (isRandom) t = random.Next(0, playlist.list.Count());
            if (t >= 0) {
                video = playlist.list[t];
                player.Source = new Uri(video.path);
                _currentPlaying = video.path;
                status.recent.Add(video);
                lvPlayList.Visibility = Visibility.Collapsed;
                player.Visibility = Visibility.Visible;
                player.Play();
                playButton.Visibility = Visibility.Collapsed;
                pauseButton.Visibility = Visibility.Visible;
                mediaName.Text = video.title.Length <= 30 ? video.title : (video.title.Substring(0, 30) + "...");
                mediaPlayerIsPlaying = true;
            }
        }

        private void Previous_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = playlist.name != "";
        }

        private void Previous_Executed(object sender, ExecutedRoutedEventArgs e) {
            Previous();
        }

        private void Random_Checked(object sender, RoutedEventArgs e) {
            isRandom = true;
        }

        private void Random_Unchecked(object sender, RoutedEventArgs e) {
            isRandom = false;
        }

        private void player_MediaEnded(object sender, RoutedEventArgs e) {
            if (isLoop) {
                player.Position = TimeSpan.FromMilliseconds(0);
                player.Play();
                playButton.Visibility = Visibility.Collapsed;
                pauseButton.Visibility = Visibility.Visible;
                mediaPlayerIsPlaying = true;
                return;
            }
            Next();
        }

        private void Loop_Checked(object sender, RoutedEventArgs e) {
            isLoop = true;
        }

        private void Loop_Unchecked(object sender, RoutedEventArgs e) {
            isLoop = false;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e) {
            MessageBox.Show("Thành viên nhóm:\n- 20120474: Lê Kim Hiếu\n- 20120488: Thái Nguyễn Việt Hùng\n- 20120489: Võ Phi Hùng\n- 20120496: Nguyễn Cảnh Huy", "About");
        }
    }

    public static class CustomCommands {
        public static readonly RoutedUICommand Play = new RoutedUICommand(
            "Play",
            "Play",
            typeof(CustomCommands),
            new InputGestureCollection() {
                new KeyGesture(Key.Space)
            }
        );
        public static readonly RoutedUICommand Pause = new RoutedUICommand(
            "Pause",
            "Pause",
            typeof(CustomCommands),
            new InputGestureCollection() {
                new KeyGesture(Key.Space)
            }
        );
        public static readonly RoutedUICommand Stop = new RoutedUICommand(
            "Stop",
            "Stop",
            typeof(CustomCommands),
            new InputGestureCollection() {
                new KeyGesture(Key.S, ModifierKeys.Alt)
            }
        );
        public static readonly RoutedUICommand MuteVolume = new RoutedUICommand(
            "MuteVolume",
            "MuteVolume",
            typeof(CustomCommands),
            new InputGestureCollection() {
                new KeyGesture(Key.M, ModifierKeys.Control)
            }
        );
        public static readonly RoutedUICommand FullScreen = new RoutedUICommand (
            "FullScreen",
            "FullScreen",
            typeof(CustomCommands),
            new InputGestureCollection() {
                new KeyGesture(Key.F11),
                new KeyGesture(Key.Escape)
            }
        );
        public static readonly RoutedUICommand NewPlaylist = new RoutedUICommand(
            "NewPlaylist",
            "NewPlaylist",
            typeof(CustomCommands),
            new InputGestureCollection() {
                new KeyGesture(Key.N, ModifierKeys.Control)
            }
        );
        public static readonly RoutedUICommand OpenPlaylist = new RoutedUICommand(
            "OpenPlaylist",
            "OpenPlaylist",
            typeof(CustomCommands),
            new InputGestureCollection() {
                new KeyGesture(Key.O, ModifierKeys.Control | ModifierKeys.Shift)
            }
        );
        public static readonly RoutedUICommand AddToPlaylist = new RoutedUICommand(
            "AddToPlaylist",
            "AddToPlaylist",
            typeof(CustomCommands),
            new InputGestureCollection() {
                new KeyGesture(Key.A, ModifierKeys.Control)
            }
        );
        public static readonly RoutedUICommand SavePlaylist = new RoutedUICommand(
            "SavePlaylist",
            "SavePlaylist",
            typeof(CustomCommands),
            new InputGestureCollection() {
                new KeyGesture(Key.S, ModifierKeys.Control)
            }
        );
        public static readonly RoutedUICommand Next = new RoutedUICommand(
            "Next",
            "Next",
            typeof(CustomCommands),
            new InputGestureCollection() {
                new KeyGesture(Key.Right, ModifierKeys.Control)
            }
        );
        public static readonly RoutedUICommand Previous = new RoutedUICommand(
            "Previous",
            "Previous",
            typeof(CustomCommands),
            new InputGestureCollection() {
                new KeyGesture(Key.Left, ModifierKeys.Control)
            }
        );
    }
}
