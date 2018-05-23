/*
Copyright (c) <2018> GiGong

See the file license.txt for copying permission.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MusicPlayer_GG
{
    /// <summary>
    /// InformationWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class InformationWindow : Window
    {
        public InformationWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            textVersion.Text = "현재 버전 : " + "v" + version.Major + "." + version.Minor + "." + version.Build;
            
            textCopyright.Text = "Copyright © 2018 GiGong.\nAll Rights Reserved.";
        }

        #region Event For Information

        private void Event_OK(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }

        private void Event_Blog(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://gigong.cf");
            }
            catch { }
        }

        private void Event_Mail(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("mailto: gigong222@gmail.com? subject = SubjectExample & amp; body = BodyExample");
            }
            catch { }
        }

        private void Event_GitHub(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://github.com/GiGong/MusicPlayer_GG");
            }
            catch { }
        }

        #endregion

        #region Region For Custom Title Bar

        private Point startPos;
        System.Windows.Forms.Screen[] screens = System.Windows.Forms.Screen.AllScreens;

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            int sum = 0;
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

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                main.BorderThickness = new Thickness(0);
                main.Margin = new Thickness(7, 7, 7, 0);
                //rectMax.Visibility = Visibility.Hidden;
                //rectMin.Visibility = Visibility.Visible;
            }
            else
            {
                main.BorderThickness = new Thickness(1);
                main.Margin = new Thickness(0);
                //rectMax.Visibility = Visibility.Visible;
                //rectMin.Visibility = Visibility.Hidden;
            }
        }

        #endregion
    }
}
