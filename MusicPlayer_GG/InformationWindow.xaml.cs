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
using System.Windows.Shapes;

/*
Copyright (c) <2018> ICRL

See the file license.txt for copying permission.
 */
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
    }
}
