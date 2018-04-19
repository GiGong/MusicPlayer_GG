/*
Copyright (c) <2018> GiGong

See the file license.txt for copying permission.
 */
using System;
using System.Windows;

namespace MusicPlayer_GG
{
    /* 추가하고 싶은 기능
     * 
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
        #region Constructor

        public MainWindow()
        {
            this.Title = Player.PROGRAM_NAME;

            this.Top = Player.Top;
            this.Left = Player.Left;
            
            this.Width = Player.Width;
            this.Height = Player.Height;
        }

        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ggmp.mainWindow = this;
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
            /* 윈도우를 화면 가장자리에 가져갔을 때 가장자리에 붙는 기능
             * 윈도우 style = none 이여야 원활히 가능하여 보류
            var screen = SystemParameters.WorkArea;
            //var Right = SystemParameters.FullPrimaryScreenWidth;
            if (this.Top <= 10)
            {
                this.Top = 0;
            }

            if (-10 <=this.Left && this.Left <= 10)
            {
                this.Left = 0;
            }
            else if (-10 <= screen.Right - (this.Left + this.Width) && screen.Right - (this.Left + this.Width) <= 10)
            {
                this.Left = screen.Right - this.Width;
            }
            */
        }
    }
}
