using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MusicPlayer_GG;

namespace GiGong
{
    /// <summary>
    /// DragDropListBox.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ListBoxDragDrop : UserControl
    {
        List<MediaElement_GG> listDrag;

        bool isDrag = false;
        bool isOut = true;
        bool isKeyDown = false;

        Point startPoint;

        public ListBox ListBoxDD
        {
            get { return box; }
            set { box = value; }
        }

        internal List<MediaElement_GG> ListSource { get; private set; }

        public ListBoxDragDrop()
        {
            InitializeComponent();
            
            box.KeyDown += (s, e) => { isKeyDown = true; };
            box.KeyUp += (s, e) => { isKeyDown = false; };
        }

        internal void SetSource(List<MediaElement_GG> source)
        {
            ListSource = source;
            box.ItemsSource = ListSource;
        }

        #region Music

        private void Event_Play(object sender, RoutedEventArgs e)
        {
            if (box.SelectedIndex > -1)
                Player.MediaSelectPlay(box.SelectedIndex);
        }

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
            while (box.SelectedIndex > -1)
                Player.MediaDelete(box.SelectedIndex);
        }


        #endregion

        #region Event ListBox

        /// <summary>
        /// 재생목록에서 받아들인 키 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    if (box.SelectedIndex > -1)
                        Player.MediaSelectPlay(box.SelectedIndex);
                    break;

                case Key.Delete:
                    while (box.SelectedIndex > -1)
                        Player.MediaDelete(box.SelectedIndex);
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
        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (box.SelectedIndex > -1)
                Player.MediaSelectPlay(box.SelectedIndex);
        }

            #region Drag Drop

        private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isKeyDown == true)
                return;

            if (e.ClickCount == 2)
            {
                ListBox_MouseDoubleClick(sender, e);
                return;
            }

            startPoint = e.GetPosition(null);
            var listBoxItem = FindAncestor<ListBoxItem>((DependencyObject)(box.InputHitTest(e.GetPosition(box))));

            if (listBoxItem != null)
            {
                foreach (var item in box.SelectedItems)
                {
                    if (item == listBoxItem.Content)
                    {
                        e.Handled = true;
                        break;
                    }
                }
                isDrag = true;
            }
            else
                isDrag = false;
        }

        private void ListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var listBoxItem = FindAncestor<ListBoxItem>((DependencyObject)(box.InputHitTest(e.GetPosition(box))));

            if (listBoxItem == null)
                return;

            Vector diff = startPoint - e.GetPosition(null);

            if (isKeyDown == false && diff.X == 0 && diff.Y == 0)
            {
                box.SelectedIndex = ListSource.IndexOf(listBoxItem.Content as MediaElement_GG);
            }
        }

        private void ListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if ((e.LeftButton == MouseButtonState.Pressed) && (isDrag == true) && (isOut == true))
            {
                var listBoxItem = FindAncestor<ListBoxItem>((DependencyObject)(box.InputHitTest(e.GetPosition(box))));
                if (listBoxItem == null)
                    return;

                listDrag = new List<MediaElement_GG>(box.SelectedItems.Count);

                foreach (MediaElement_GG item in ListSource)
                {
                    if (box.SelectedItems.Contains(item))
                        listDrag.Add(item);
                }

                DataObject data = new DataObject("MediaElement_GG", listDrag);
                DragDrop.DoDragDrop(listBoxItem, data, DragDropEffects.Move);
            }
        }

        private void ListBox_DragOver(object sender, DragEventArgs e)
        {
            bool dropEnabled = true;
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                string[] filenames = e.Data.GetData(DataFormats.FileDrop, true) as string[];
                e.Effects = DragDropEffects.Copy;

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
            else if (e.Data.GetDataPresent("MediaElement_GG"))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                dropEnabled = false;
            }

            if (dropEnabled == false)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            // -------------------- Scroll Viewer --------------------

            ScrollViewer scrollViewer = FindVisualChild<ScrollViewer>(box);

            double tolerance = 10;
            double verticalPos = e.GetPosition(box).Y;
            double offset = 1;

            if (verticalPos < tolerance) // Top of visible list?
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - offset); //Scroll up.
            }
            else if (verticalPos > box.ActualHeight - tolerance) //Bottom of visible list?
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + offset); //Scroll down.    
            }
        }

        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("MediaElement_GG"))
            {
                int index = -1;
                var target = FindAncestor<ListBoxItem>((DependencyObject)(box.InputHitTest(e.GetPosition(box))));

                if (target != null && listDrag.Contains(target.Content as MediaElement_GG))
                {
                    index = ListSource.IndexOf(target.Content as MediaElement_GG);
                    target = null;
                }

                foreach (var item in listDrag)
                {
                    ListSource.Remove(item);
                }

                if (target == null && index == -1)
                    // 만일 빈 곳에 Drop을 했을 경우 처리
                    index = ListSource.Count;
                else if (index == -1)
                    index = ListSource.IndexOf(target.Content as MediaElement_GG);
                else if (index > ListSource.Count)
                    index = ListSource.Count;

                for (int i = 1; i <= listDrag.Count; i++)
                {
                    ListSource.Insert(index, listDrag[listDrag.Count - i]);
                }
                box.Items.Refresh();

                listDrag = null;
                isDrag = false;
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                Player.MediaAdd(files);
            }

        }

        /// <summary>
        /// 파일 확장자가 Audio파일이 맞는가 확인
        /// </summary>
        /// <param name="extension">extension to check</param>
        /// <returns></returns>
        private bool IsAudioExtension(string extension)
        {
            List<string> Extensions = new List<string>() { ".aac", ".ac3", ".aif", ".aiff", ".ape", ".cda", ".flac", ".m4a", ".mid", ".midi", ".mod", ".mp2", ".mp3", ".mpc", ".ofs", ".ogg", ".rmi", ".tak", ".wav", ".wma", ".wv" };

            foreach (string item in Extensions)
                if (item == extension)
                    return true;

            return false;
        }

            #endregion

        #endregion

        #region Event Item

        private void ListBoxItem_DragOver(object sender, DragEventArgs e)
        {
            var item = sender as ListBoxItem;

            if (listDrag.Contains(item.Content as MediaElement_GG))
                return;

            item.Background = Brushes.PowderBlue;

            item.BorderThickness = (new Thickness { Top = 2 });
        }

        private void ListBoxItem_DragLeave(object sender, DragEventArgs e)
        {
            var item = sender as ListBoxItem;

            if (listDrag.Contains(item.Content as MediaElement_GG))
                return;

            item.Background = Brushes.Transparent;
            item.BorderThickness = (new Thickness(0));
        }

        private void ListBoxItem_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            var item = sender as ListBoxItem;

            Point pos = e.GetPosition(item);
            Point diff = new Point(item.ActualWidth - pos.X, item.ActualHeight - pos.Y);

            double tolerance = 2;

            if ((pos.X < tolerance) || (pos.Y < tolerance) || (diff.X < tolerance) || (diff.Y < tolerance))
                isOut = true;
            else
                isOut = false;
        }

        private void ListBoxItem_MouseEnter(object sender, MouseEventArgs e)
        {
            isOut = false;
        }

        #endregion

        #region Tree Function

        /// <summary>
        /// Helper to search up the VisualTree
        /// (https://wpftutorial.net/DragAndDrop.html)
        /// </summary>
        /// <typeparam name="T">Type to look for</typeparam>
        /// <param name="obj">start object</param>
        /// <returns></returns>
        private static T FindAncestor<T>(DependencyObject obj)
                    where T : DependencyObject
        {
            do
            {
                if (obj is T)
                {
                    return (T)obj;
                }
                obj = VisualTreeHelper.GetParent(obj);
            }
            while (obj != null);
            return null;
        }

        /// <summary>
        /// Search immediate children first (breadth-first)
        /// (https://stackoverflow.com/questions/1316251/wpf-listbox-auto-scroll-while-dragging)
        /// </summary>
        /// <typeparam name="T">Type to look for</typeparam>
        /// <param name="obj">start object</param>
        /// <returns></returns>
        private static T FindVisualChild<T>(DependencyObject obj)
                    where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);

                if (child != null && child is T)
                    return (T)child;

                else
                {
                    T childOfChild = FindVisualChild<T>(child);

                    if (childOfChild != null)
                        return childOfChild;
                }
            }

            return null;
        }

        #endregion
    }
}
