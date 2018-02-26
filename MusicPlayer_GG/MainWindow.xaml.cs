﻿/*
Copyright (c) <2018> GiGong

See the file license.txt for copying permission.
 */
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
    /* 추가하고 싶은 기능
     * 
     * 음악 재생 중 프로그램 종료 후 다시 프로그램을 켰을 경우
     * 원래 재생하던 부분 부터 다시 재생 가능 - from aimp
     * 
     * 위 기능 이전에 필요한 기능
     * - 프로그램 실행 시 마지막에 재생하던 재생목록을 로드
     */

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
        private DispatcherTimer _timer = new DispatcherTimer();

        /// <summary>
        /// 파일의 총 재생 시간 (문자형)
        /// </summary>
        private string _maxTime;

        /// <summary>
        /// 파일의 총 재생 시간 (시간형)
        /// </summary>
        private TimeSpan _maxTimeSpan;

        /// <summary>
        /// 마우스가 시간 탐색 Slider를 눌렀는지 확인
        /// </summary>
        private bool isDown;
        // private static bool isToast = false;

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
            Player.Played += Update_Informaion;
            Player.Stoped += Music_Stoped;
            Player.Paused += Music_Paused;

            // Player 클래스 초기화
            Player.Initiate(Update_TotalPlayTime, Music_Failed);

            // Volume Slider에 마우스 관련 이벤트 추가
            sdrVol.MouseEnter += (s, e1) => { lblVol.Content = Volume + " %"; };
            sdrVol.MouseLeave += (s, e1) => { lblVol.Content = ""; };

            // 프로그램 종료 시 Player에도 종료 이벤트
            // 모두 닫기 전 Player를 먼저 정리
            this.Closing += Player.Event_Closed;

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
            lblTime.Content = Player.Position.ToString(@"mm\ \:\ ss") + " / " + _maxTime;

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

#if DEBUG
                case Key.Back:
                    // ToastMessage("Test Toast 메시지 입니다. 헤헤 히 후");
                    break;
#endif

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
        /// 총 재생 시간 업데이트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Update_TotalPlayTime(object sender, EventArgs e)
        {
            MaxTime = Player.TotalPlayTime;
        }

        /// <summary>
        /// 음악이 시작될 때 정보들 업데이트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Update_Informaion(object sender, EventArgs e)
        {
            lblTitle.Content = Player.NowTitle;
            lblMusic.Content = Player.NowPlaying;

            if (Player.NowAlbumArt != null)
                imgArt.Source = Player.NowAlbumArt;

            listPlay.SelectedIndex = Player.PlayingIndex;

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

        /// <summary>
        /// 음악을 재생할 수 없을 때 Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Music_Failed(object sender, ExceptionEventArgs e)
        {
            if (listPlay.Items.Count <= 1)
                Event_Stop(sender, null);
            else
                Event_Next(sender, null);

            // ToastMessage("해당 파일을 재생할 수 없습니다.");
        }

        #endregion

        #region ----Event for Music----

        /// <summary>
        /// 플레이 리스트에 선택된 음악 재생
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Event_Play(object sender, RoutedEventArgs e)
        {
            if (listPlay.SelectedIndex > -1)
                Player.MediaSelectPlay(listPlay.SelectedIndex);
        }

        /// <summary>
        /// 음악 정지
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Event_Stop(object sender, RoutedEventArgs e)
        {
            Player.MediaStop();
        }

        /// <summary>
        /// 음악 일시정지
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Event_Pause(object sender, RoutedEventArgs e)
        {
            Player.MediaPause();
        }

        /// <summary>
        /// 다음곡 재생
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Event_Next(object sender, RoutedEventArgs e)
        {
            Player.MediaNext();
        }

        /// <summary>
        /// 이전곡 재생
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Event_Previous(object sender, RoutedEventArgs e)
        {
            Player.MediaPrevious();
        }

        #endregion

        #region ----Event for File----

        /// <summary>
        /// 재생목록에 파일 추가
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Event_Add(object sender, RoutedEventArgs e)
        {
            Player.MediaAdd();
        }

        /// <summary>
        /// 재생목록에서 파일 제거
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Event_Delete(object sender, RoutedEventArgs e)
        {
            while (listPlay.SelectedIndex > -1)
                Player.MediaDelete(listPlay.SelectedIndex);
        }

        #endregion

        #region ----Event for PlayList----

        /// <summary>
        /// 재생목록 새로 만들기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Event_PlayListNew(object sender, RoutedEventArgs e)
        {
            Player.PlayList_New();
            listPlay.ItemsSource = Player.PlayList;
            listPlay.SelectedIndex = Player.PlayingIndex;
        }

        /// <summary>
        /// 재생목록 따로 저장하기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Event_PlayListSave(object sender, RoutedEventArgs e)
        {
            Player.PlayList_Save();
        }

        /// <summary>
        /// 재생목록 불러오기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Event_PlayListLoad(object sender, RoutedEventArgs e)
        {
            Player.PlayList_Load();
            listPlay.ItemsSource = Player.PlayList;
            listPlay.SelectedIndex = Player.PlayingIndex;
        }

        #endregion

        #region ----Event for listPlay----

        /// <summary>
        /// 재생목록에 입력되는 키 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListPlay_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                case Key.Down:

                case Key.Enter:
                case Key.Delete:
                    break;

                // 위에 입력되는 키들은 재생목록에서 받아들임
                // 그 외 키들은 Window에 전달

                default:
                    e.Handled = true;
                    Window_KeyDown(sender, e);
                    break;
            }
        }

        /// <summary>
        /// 재생목록에서 받아들인 키 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 마우스로 더블클릭시 해당 음악 재생
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListPlay_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listPlay.SelectedIndex > -1)
                Player.MediaSelectPlay(listPlay.SelectedIndex);
        }

        #endregion

        #region ----Event for sdrPlay----

        /// <summary>
        /// 시간 탐색 Slider에 마우스 버튼이 눌리기 전
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SdrPlay_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Player.IsOpened == false)
            {
                e.Handled = true;
                return;
            }

            isDown = true;
        }

        /// <summary>
        /// 시간 탐색 Slider에 마우스 버튼이 떼지기 전
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SdrPlay_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Player.IsOpened == false)
            {
                e.Handled = true;
                return;
            }

            SdrPlay_ValueChanged();
            isDown = false;
        }

        /// <summary>
        /// 시간 탐색 Slider에 값이 변할 경우
        /// </summary>
        /// <param name="i">변한 값 (0 : label 업데이트 용)</param>
        private void SdrPlay_ValueChanged(int i = 0)
        {
            int value = (int)sdrPlay.Value + i;

            int hours = value / 3600;
            int minutes = value % 3600 / 60;
            int seconds = value % 3600 % 60;

            Player.Position = new TimeSpan(hours, minutes, seconds);

            lblTime.Content = Player.Position.ToString(@"mm\ \:\ ss") + " / " + _maxTime;

            if (i != 0)
                sdrPlay.Value = value;
        }

        /// <summary>
        /// 시간 탐색 Slider에 마우스 휠
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SdrPlay_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Player.IsOpened == false)
            {
                e.Handled = true;
                return;
            }

            if (e.Delta > 0)
                SdrPlay_ValueChanged(5);
            else
                SdrPlay_ValueChanged(-5);
        }

        #endregion

        #region ----Drag and Drop----

        /// <summary>
        /// 플레이리스트에 파일을 드래그, 올려놓았을 시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 플레이리스트에 파일을 드래그, 드롭 했을 시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListPlay_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                Player.MediaAdd(files);
            }
        }

        /// <summary>
        /// 파일 확장자가 Audio파일이 맞는가 확인
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
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

        /* Event_Test
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

        /* Toast Message
        private void ToastMessage(string message)
        {
            lblToast.Content = message;

            var showToast = new System.Threading.Thread(new System.Threading.ThreadStart(() => {
                while (isToast == true) System.Threading.Thread.Sleep(1000);

                var da1 = new System.Windows.Media.Animation.DoubleAnimation();
                da1.From = 0;
                da1.To = 1;
                da1.Duration = new Duration(TimeSpan.FromSeconds(0.5));

                var da2 = new System.Windows.Media.Animation.DoubleAnimation();
                da2.From = 1;
                da2.To = 0;
                da2.Duration = new Duration(TimeSpan.FromSeconds(1));
                da2.BeginTime = TimeSpan.FromSeconds(1);

                da1.Completed += (s1, e1) => { bdrToast.BeginAnimation(OpacityProperty, da2); };
                da2.Completed += (s2, e2) => { bdrToast.Visibility = Visibility.Hidden; isToast = false; };

                // bdrToast.Visibility = Visibility.Visible;

                isToast = true;
                //bdrToast.BeginAnimation(OpacityProperty, da1);


                if (bdrToast.Dispatcher.CheckAccess())
                    StartToast(da1);
                else
                    bdrToast.Dispatcher.BeginInvoke(new VisibilityDelegate(StartToast), da1);

            }));

            showToast.Start();
        }

        private delegate void VisibilityDelegate(System.Windows.Media.Animation.DoubleAnimation da);
        private void StartToast(System.Windows.Media.Animation.DoubleAnimation da)
        {
            var animation = ;
            bdrToast.Visibility = Visibility.Visible;
            bdrToast.BeginAnimation(OpacityProperty, animation);
        }
        */

        #endregion

    }
}
