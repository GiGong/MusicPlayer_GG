/*
Copyright (c) <2018> GiGong

See the file license.txt for copying permission.
 */
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace MusicPlayer_GG
{
    /* 추가하고 싶은 기능
     * 
     * 재생 목록 임의 재생 시 임의적으로 순서를 만들어서 그 순서대로 재생 -> 임의 재생 시 재생한 곡 다시 재생하지 않게
     * 
     * -------------------------------------------------------
     * 윈도우 시작 시 종료했던 위치에 윈도우 Load
     * 
     * -------------------------------------------------------
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
    public partial class MainWindow : Window
    {
        #region ----Variables----

        private Point startPos;
        System.Windows.Forms.Screen[] screens = System.Windows.Forms.Screen.AllScreens;

        #endregion

        #region ----Constructor----

        public MainWindow()
        {
            InitializeComponent();

            this.Title = Player.PROGRAM_NAME;

            this.Top = Player.Top;
            this.Left = Player.Left;

            this.Width = Player.Width;
            this.Height = Player.Height;

            MaxHeight = SystemParameters.WorkArea.Height;
        }

        #endregion

        #region ----Window Event Handler----

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ggmp.MainWindow = this;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Player.Top = this.Top;
            Player.Left = this.Left;
            Player.Width = this.Width;
            Player.Height = this.Height;

            Player.Event_Closed(sender, e);
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            int sum = 0;
            int margin = 10;

            if (-margin <= this.Top && this.Top <= margin)
            {
                this.Top = 0;
            }

            foreach (var item in screens)
            {
                int term = item.WorkingArea.Width;
                sum += term;
                if (sum - term - margin <= this.Left && this.Left <= sum - term + margin)
                {
                    this.Left = sum - term;
                    break;
                }
                else if (sum - margin <= this.Left + this.Width && this.Left + this.Width <= sum + margin)
                {
                    this.Left = sum - this.Width;
                    break;
                }
            }

            sum = 0;
            foreach (var item in screens)
            {
                sum += item.WorkingArea.Width;
                if (sum >= this.Left + this.Width / 2)
                {
                    this.MaxHeight = item.WorkingArea.Height;
                    break;
                }
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                main.BorderThickness = new Thickness(0);
                rectMax.Visibility = Visibility.Hidden;
                rectMin.Visibility = Visibility.Visible;
            }
            else
            {
                main.BorderThickness = new Thickness(1);
                rectMax.Visibility = Visibility.Visible;
                rectMin.Visibility = Visibility.Hidden;
            }
        }

        #endregion

        #region ----Title Bar Event Handler----

        private void System_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount >= 2)
                {
                    this.WindowState = (this.WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
                }
                else
                {
                    startPos = e.GetPosition(null);
                }
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                var pos = PointToScreen(e.GetPosition(this));
                IntPtr hWnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                IntPtr hMenu = GetSystemMenu(hWnd, false);
                int cmd = TrackPopupMenu(hMenu, 0x100, (int)pos.X, (int)pos.Y, 0, hWnd, IntPtr.Zero);
                if (cmd > 0) SendMessage(hWnd, 0x112, (IntPtr)cmd, IntPtr.Zero);
            }
        }

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
        [DllImport("user32.dll")]
        static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32.dll")]
        static extern int TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);

        private void System_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (this.WindowState == WindowState.Maximized && Math.Abs(startPos.Y - e.GetPosition(null).Y) > 2)
                {
                    var point = PointToScreen(e.GetPosition(null));

                    this.WindowState = WindowState.Normal;

                    this.Left = point.X - this.ActualWidth / 2;
                    this.Top = point.Y - border.ActualHeight / 2;
                }
                DragMove();
            }
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = (this.WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Mimimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        #endregion

    }
}
