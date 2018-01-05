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

/*
Copyright (c) <2018> ICRL

See the file license.txt for copying permission.
 */
namespace MusicPlayer_GG
{
    /// <summary>
    /// MainWindow for Music Player
    /// made by Gigong
    /// email : gigong222@gmail.com
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region ----Variables----

        /// <summary>
        /// 재생 시간이 지남에 따라 정보를 업데이트 시켜줄 Timer
        /// </summary>
        DispatcherTimer _timer = new DispatcherTimer();
        string _maxTime;
        TimeSpan _maxTimeSpan;
        bool isDown;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region ----Property----

        /// <summary>
        /// 임의 재생 할 것인가
        /// </summary>
        public bool IsShuffle
        {
            get { return Player.isShuffle; }
            set
            {
                Player.isShuffle = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsShuffle"));
            }
        }

        /// <summary>
        /// 한곡 반복을 할 것인가
        /// </summary>
        public bool IsRepeatOnce
        {
            get { return Player.isRepeatOne; }
            set
            {
                Player.isRepeatOne = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRepeatOnce"));
            }
        }

        /// <summary>
        /// Volume값
        /// double을 int형으로 변환
        /// floating point 에러 없도록 처리
        /// </summary>
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

        /// <summary>
        /// 음악의 총 재생 시간 관련 작업
        /// string형 변환, slider 값 갱신
        /// </summary>
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

        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// MainWindow 첫 로드시, 관련 작업
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Timer 설정
            _timer.Interval = TimeSpan.FromMilliseconds(500);
            _timer.Tick += _timer_Tick;

            // Player 클래스에 Event 등록
            Player.Changed += (s, e1) => { listPlay.Items.Refresh(); };
            Player.Played += Music_Played;
            Player.Stoped += Music_Stoped;
            Player.Paused += Music_Paused;

            // Player 클래스 초기화
            Player.Initiate(Music_Opened);

            // Volume Slider에 마우스 관련 이벤트 추가
            sdrVol.MouseEnter += (s, e1) => { lblVol.Content = Volume + " %"; };
            sdrVol.MouseLeave += (s, e1) => { lblVol.Content = ""; };

            // 프로그램 종료 시 Player에도 종료 이벤트
            // 모두 닫기 전 Player를 먼저 정리
            this.Closed += Player.Event_Closed;

            // Data Binding
            this.DataContext = this;

            // Player Load (설정, 재생목록 Load)
            Player.Event_Loaded(sender, e);

            // Load한 값 설정
            listPlay.ItemsSource = Player.PlayList;
            listPlay.SelectedIndex = Player.PlayingIndex;
            sdrVol.Value = Volume;
        }

        /// <summary>
        /// Timer의 주기마다 현재 재생 시간을 업데이트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _timer_Tick(object sender, EventArgs e)
        {
            // Label에 보이는 컨텐츠 업데이트
            lblTime.Content = Player.Position.ToString(@"mm\ \:\ ss") + " / " + _maxTime;

            // 마우스로 탐색 Slider를 누르지 않을 경우
            // Slider의 값 업데이트
            if (isDown == false)
                sdrPlay.Value = (int)Player.Position.TotalSeconds;
        }

        /// <summary>
        /// 단축키를 입력 받음
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                /* Command로 대체
                case Key.F1:
                    // Show_Help();
                    break;
                    */

                #region Media

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

                #endregion

                #region Volume
                    
                case Key.Up:
                    Volume += 2;
                    break;

                case Key.Down:
                    Volume -= 2;
                    break;

                #endregion

                #region Play Time

                case Key.Right:
                    SdrPlay_ValueChanged(5);
                    break;

                case Key.Left:
                    SdrPlay_ValueChanged(-5);
                    break;

                    #endregion
            }
        }

        /// <summary>
        /// 프로그램 정보 윈도우 호출
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Information_Click(object sender, RoutedEventArgs e)
        {
            InformationWindow informationWindow = new InformationWindow();

            // 새 윈도우를 현재 윈도우의 중앙에 위치하도록 Top, Left 조정
            informationWindow.Top = this.Top + (this.ActualHeight - informationWindow.Height) / 2;
            informationWindow.Left = this.Left + (this.ActualWidth - informationWindow.Width) / 2;

            // Dialog 형식으로 호출
            informationWindow.ShowDialog();
        }

        /// <summary>
        /// 마우스 휠로 볼륨 조절 가능
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SdrVol_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                Volume += 2;
            else
                Volume -= 2;
        }

        /// <summary>
        /// 프로그램 종료를 원할 때 Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Event_Exit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #region ----Event for Change----

        /// <summary>
        /// 음악이 열렸을 때 Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Music_Opened(object sender, EventArgs e)
        {
            // 총 재생 시간 업데이트
            MaxTime = Player.TotalPlayTime;
        }

        /// <summary>
        /// 음악이 시작될 때 Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Music_Played(object sender, EventArgs e)
        {
            // 음악 제목, 음악 정보 업데이트
            lblTitle.Content = Player.NowTitle;
            lblMusic.Content = Player.NowPlaying;

            // 앨범아트를 읽어들였다면 업데이트
            if (Player.NowAlbumArt != null)
                imgArt.Source = Player.NowAlbumArt;

            // Stop -> Play시에 총 재생 시간 업데이트
            // 의미 없음
            // MaxTime = Player.TotalPlayTime;

            // Playlist에 선택된 Item 변경
            listPlay.SelectedIndex = Player.PlayingIndex;

            // 타이머 시작
            _timer.Start();
        }

        /// <summary>
        /// 음악이 Stop(or 종료)되었을 때 Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 음악이 일시정지 되었을 때 Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Music_Paused(object sender, EventArgs e)
        {
            _timer.Stop();
        }

        #endregion

        #region ----Event for Music----

        private void Event_Play(object sender, RoutedEventArgs e)
        {
            if (listPlay.SelectedIndex > -1)
                Player.MediaSelectPlay(listPlay.SelectedIndex);
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

        #region ----Event for File----

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

        #region ----Event for PlayList----

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

        #region ----Event for listPlay----

        private void ListPlay_PreviewKeyDown(object sender, KeyEventArgs e)
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

        private void ListPlay_KeyDown(object sender, KeyEventArgs e)
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

        private void ListPlay_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listPlay.SelectedIndex > -1)
                Player.MediaSelectPlay(listPlay.SelectedIndex);
        }

        #endregion

        #region ----Event Context----

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

        #region ----Event for sdrPlay----

        private void SdrPlay_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Player.IsOpened == false)
            {
                sdrPlay.Value = 0;
                return;
            }

            isDown = true;
        }

        private void SdrPlay_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Player.IsOpened == false)
            {
                sdrPlay.Value = 0;
                return;
            }

            SdrPlay_ValueChanged();
            isDown = false;
        }

        private void SdrPlay_ValueChanged(int i = 0)
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

        private void SdrPlay_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                SdrPlay_ValueChanged(5);
            else
                SdrPlay_ValueChanged(-5);
        }

        #endregion

        #region ----Drag and Drop----

        private void ListPlay_DragOver(object sender, DragEventArgs e)
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

        private void ListPlay_Drop(object sender, DragEventArgs e)
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

        #region Test

        private void Test_Help(object sender, EventArgs e)
        {
            MessageBox.Show("help message");
        }

        /*
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
        */

        #endregion

    }
}
