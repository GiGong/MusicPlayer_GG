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
using System.ComponentModel;
using System.Windows.Threading;
using System.Runtime.Serialization.Json;

namespace MusicPlayer_GG
{
    /// <summary>
    /// 2017.12.22 18:33 레포지토리 변경내용 확인
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        DispatcherTimer _timer = new DispatcherTimer();
        string _maxTime;
        TimeSpan _maxTimeSpan;
        bool isDown;

        public event PropertyChangedEventHandler PropertyChanged;


        public bool IsShuffle
        {
            get { return Player.isShuffle; }
            set
            {
                Player.isShuffle = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsShuffle"));
            }
        }

        public bool IsRepeatOnce
        {
            get { return Player.isRepeatOne; }
            set
            {
                Player.isRepeatOne = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRepeatOnce"));
            }
        }

        public int Volume
        {
            get { return (int)Math.Round(Player.Volume * 100); }
            set
            {
                Player.Volume = value / 100.0;
                lblVol.Content = Volume + " %";
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Volume"));
            }
        }

        public TimeSpan MaxTime
        {
            get { return _maxTimeSpan; }
            set
            {
                _maxTime = value.ToString(@"mm\ \:\ ss");
                sdrPlay.Maximum = (int)value.TotalSeconds;
                sdrPlay.LargeChange = (int)value.TotalSeconds / 50;
                _maxTimeSpan = value;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _timer.Interval = TimeSpan.FromMilliseconds(500);
            _timer.Tick += _timer_Tick;

            Player.Changed += (s, e1) => { listPlay.Items.Refresh(); };
            Player.Played += Music_Played;
            Player.Stoped += Music_Stoped;
            Player.Paused += Music_Paused;

            Player.Initiate(Music_Opened);

            sdrVol.MouseEnter += (s, e1) => { lblVol.Content = Volume + " %"; };
            sdrVol.MouseLeave += (s, e1) => { lblVol.Content = ""; };

            this.Closed += Player.Event_Closed;

            this.DataContext = this;

            Player.Event_Loaded(sender, e);
            listPlay.ItemsSource = Player.PlayList;
            listPlay.SelectedIndex = Player.PlayingIndex;
            sdrVol.Value = Volume;
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            lblTime.Content = Player.Position.ToString(@"mm\ \:\ ss") + " / " + _maxTime;

            if (isDown == false)
                sdrPlay.Value = (int)Player.Position.TotalSeconds;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F1:
                    // Show_Help();
                    break;

                case Key.Space:
                    Player.MediaPause();
                    break;

                case Key.Escape:
                    Player.MediaStop();
                    break;

                case Key.PageUp:
                    Player.MediaPrevious();
                    break;

                case Key.PageDown:
                    Player.MediaNext();
                    break;

                case Key.Up:
                    Volume += 2;
                    break;

                case Key.Down:
                    Volume -= 2;
                    break;

                case Key.Right:
                    sdrPlay_ValueChanged(5);
                    break;

                case Key.Left:
                    sdrPlay_ValueChanged(-5);
                    break;
#if DEBUG
                default:

                    break;
#endif
            }
        }

        private void sdrVol_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                Volume += 2;
            else
                Volume -= 2;
        }


        /// <summary>
        /// Eventhandler For Test
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Event_Test(object sender, EventArgs e)
        {
            if (sender is MenuItem)
            {
                var item = sender as MenuItem;

                if (item.InputGestureText == "Up")
                    Volume += 2;
            }
        }

        #region Event for Change

        private void Music_Opened(object sender, EventArgs e)
        {
            MaxTime = Player.TotalPlayTime;
        }

        private void Music_Played(object sender, EventArgs e)
        {
            lblTitle.Content = Player.NowTitle;
            lblMusic.Content = Player.NowPlaying;

            if (Player.NowAlbumArt != null)
                imgArt.Source = Player.NowAlbumArt;

            MaxTime = Player.TotalPlayTime;
            listPlay.SelectedIndex = Player.PlayingIndex;
            _timer.Start();
        }

        private void Music_Stoped(object sender, EventArgs e)
        {
            _timer.Stop();
            lblTitle.Content = Player.NowTitle;
            lblMusic.Content = Player.NowPlaying;

            imgArt.Source = null;
            MaxTime = TimeSpan.FromSeconds(100);
            sdrPlay.Value = 0;
            lblTime.Content = "";
        }

        private void Music_Paused(object sender, EventArgs e)
        {
            _timer.Stop();
        }

        private void Event_Exit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion

        #region Event for Music

        private void Event_Play(object sender, RoutedEventArgs e)
        {
            Player.MediaPlay();
        }

        private void Event_Stop(object sender, RoutedEventArgs e)
        {
            Player.MediaStop();
        }

        private void Event_Pause(object sender, RoutedEventArgs e)
        {
            Player.MediaPause();
        }

        private void Event_Next(object sender, RoutedEventArgs e)
        {
            Player.MediaNext();
        }

        private void Event_Previous(object sender, RoutedEventArgs e)
        {
            Player.MediaPrevious();
        }

        #endregion

        #region Event for File

        private void Event_Add(object sender, RoutedEventArgs e)
        {
            Player.MediaAdd();
        }

        private void Event_Delete(object sender, RoutedEventArgs e)
        {
            while (listPlay.SelectedIndex > -1)
                Player.MediaDelete(listPlay.SelectedIndex);
        }

        #endregion

        #region Event for PlayList

        private void Event_PlayListNew(object sender, RoutedEventArgs e)
        {
            Player.PlayList_New();
            listPlay.ItemsSource = Player.PlayList;
            listPlay.SelectedIndex = Player.PlayingIndex;
        }

        private void Event_PlayListSave(object sender, RoutedEventArgs e)
        {
            Player.PlayList_Save();
        }

        private void Event_PlayListLoad(object sender, RoutedEventArgs e)
        {
            Player.PlayList_Load();
            listPlay.ItemsSource = Player.PlayList;
            listPlay.SelectedIndex = Player.PlayingIndex;
        }

        #endregion

        #region Event for listPlay

        private void listPlay_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                case Key.Down:

                case Key.Enter:
                case Key.Delete:
                    break;

                default:
                    e.Handled = true;
                    Window_KeyDown(sender, e);
                    break;
            }
        }

        private void listPlay_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    if (listPlay.SelectedIndex > -1)
                        Player.MediaSelectPlay(listPlay.SelectedIndex);
                    break;

                case Key.Delete:
                    while (listPlay.SelectedIndex > -1)
                        Player.MediaDelete(listPlay.SelectedIndex);
                    break;

                case Key.Insert:
                    // 노래 추가
                    break;
            }
        }

        private void listPlay_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listPlay.SelectedIndex > -1)
                Player.MediaSelectPlay(listPlay.SelectedIndex);
        }

        #endregion

        #region Event Context

        private void Event_ContextPlay(object sender, RoutedEventArgs e)
        {
            if (listPlay.SelectedIndex > -1)
                Player.MediaSelectPlay(listPlay.SelectedIndex);
        }

        private void Event_ContextDelete(object sender, RoutedEventArgs e)
        {
            while (listPlay.SelectedIndex > -1)
                Player.MediaDelete(listPlay.SelectedIndex);
        }

        #endregion

        #region Event for sdrPlay

        private void sdrPlay_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Player.IsOpened == false)
            {
                sdrPlay.Value = 0;
                return;
            }

            isDown = true;
        }

        private void sdrPlay_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Player.IsOpened == false)
            {
                sdrPlay.Value = 0;
                return;
            }

            sdrPlay_ValueChanged();
            isDown = false;
        }

        private void sdrPlay_ValueChanged(int i = 0)
        {
            if (Player.IsOpened == false)
            {
                sdrPlay.Value = 0;
                return;
            }

            int value = (int)sdrPlay.Value + i;

            int hours = value / 3600;
            int minutes = value % 3600 / 60;
            int seconds = value % 3600 % 60;

            Player.Position = new TimeSpan(hours, minutes, seconds);

            lblTime.Content = Player.Position.ToString(@"mm\ \:\ ss") + " / " + _maxTime;

            if (i != 0)
                sdrPlay.Value = value;
        }

        private void sdrPlay_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                sdrPlay_ValueChanged(5);
            else
                sdrPlay_ValueChanged(-5);
        }

        #endregion

        #region Drag and Drop

        private void listPlay_DragOver(object sender, DragEventArgs e)
        {
            bool dropEnabled = true;
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                string[] filenames = e.Data.GetData(DataFormats.FileDrop, true) as string[];

                foreach (string filename in filenames)
                {
                    string extension = System.IO.Path.GetExtension(filename).ToLowerInvariant();
                    if (IsAudioExtension(extension) == false && (extension != ".gpl"))
                    {
                        dropEnabled = false;
                        break;
                    }
                }
            }
            else
            {
                dropEnabled = false;
            }

            if (!dropEnabled)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        private void listPlay_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                Player.MediaAdd(files);
            }
        }

        private bool IsAudioExtension(string ex)
        {
            List<string> Extensions = new List<string>() { ".aac", ".ac3", ".aif", ".aiff", ".ape", ".cda", ".flac", ".m4a", ".mid", ".midi", ".mod", ".mp2", ".mp3", ".mpc", ".ofs", ".ogg", ".rmi", ".tak", ".wav", ".wma", ".wv" };

            foreach (string item in Extensions)
                if (item == ex)
                    return true;

            return false;
        }


        #endregion

    }
}
