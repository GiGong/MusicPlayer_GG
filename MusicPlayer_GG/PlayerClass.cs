/*
Copyright (c) <2018> GiGong

See the file license.txt for copying permission.
*/
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Runtime.Serialization.Json;
using System.Windows.Media.Imaging;

namespace MusicPlayer_GG
{
    static class Player
    {
        public const string PROGRAM_NAME = "MusicPlayer_GG";

        #region Variables

        private static MediaPlayer media = new MediaPlayer();
        private static Random rand = new Random((int)DateTime.Now.Ticks);
        private static double _volume;
        private static bool isPause, isPlay;
        // isPlay는 명확히 사용되는 곳이 없으나 제거하기에는 복잡해지기에 우선 유지 - 버전 1 될 시 제거
        private static long _position;

        public static double Top, Left, Width, Height;

        public static bool isShuffle, isRepeatOne;

        public static EventHandler //Ended
                                   Opened, Played, Stoped, Paused, Changed, Failed;

        #endregion

        #region Property

        public static bool IsOpened
        {
            get
            {
                if (media.Source == null)
                    return false;
                else
                    return true;
            }
        }

        public static double Volume
        {
            get
            {
                if ((int)(media.Volume * 100) == 99)
                    return 1;
                else
                    return (int)Math.Round(_volume * 100) / 100.0;
            }
            set
            {
                if (value >= 1)
                    media.Volume = 0.99;
                else if (value < 0)
                    media.Volume = 0;
                else
                    media.Volume = value;
                _volume = media.Volume;
            }
        }

        public static bool IsMuted { get => media.IsMuted; set => media.IsMuted = value; }

        public static TimeSpan Position
        {
            get { return media.Position; }
            set { media.Position = value; }
        }

        public static TimeSpan TotalPlayTime
        {
            get
            {
                if (media.NaturalDuration.HasTimeSpan)
                    return media.NaturalDuration.TimeSpan;
                return TimeSpan.FromSeconds(100);
            }
        }

        public static List<MediaElement_GG> PlayList { get; private set; }

        public static string PlayListPath { get; private set; }

        public static string NowTitle
        {
            get
            {
                return (PlayList[PlayingIndex] as MusicElement);
            }
        }

        public static string NowPlaying
        {
            get
            {
                return PlayList[PlayingIndex].FileName;
            }
        }

        public static BitmapImage NowAlbumArt
        {
            get
            {
                if (PlayList[PlayingIndex] is MusicElement)
                    return (PlayList[PlayingIndex] as MusicElement).AlbumArt;
                return null;
            }
        }

        public static int PlayingIndex { get; private set; }

        #endregion

        #region Events

        static Player()
        {
            Volume = 0.5;
            PlayingIndex = -1;
            isPause = false;
            isPlay = false;
            isShuffle = false;
            isRepeatOne = false;
            _position = 0;
            Top = Left = Width = Height = 100;

            PlayList = new List<MediaElement_GG>();
            PlayListPath = "lately.gpl";

            media.MediaOpened += Event_Opened;
            media.MediaEnded += MediaEnded;
            media.MediaFailed += Event_Failed;

            LoadSetting();
            LoadPlayList(PlayListPath);
        }

        private static void Event_Opened(object sender, EventArgs e)
        {
            Opened?.Invoke(sender, e);
        }

        public static void Event_Closed(object sender, EventArgs e)
        {
            SaveSetting();
            SavePlayList();
            MediaStop();
        }

        private static void Event_Failed(object sender, EventArgs e)
        {
            Failed?.Invoke(sender, e);
        }

        private static void MediaEnded(object sender, EventArgs e)
        {
            if (isRepeatOne == true)
            {
                media.Close();
                MediaPlay();
            }
            else
                MediaNext();
        }

        private static void PlayList_Loaded(object sender, EventArgs e)
        {
            // 마지막으로 재생한 곡, 재생 위치
            if (_position > 0)
            {
                Opened += Load_Position;
                MediaOpen();

                void Load_Position(object sender1, EventArgs e1)
                {// 이 이벤트 핸들러가 MainWindow에서 Position을 사용하는 핸들러들 보다 먼저 Opened에 추가되어야 먼저 호출됨
                 // 따라서 Event_Loaded가 MainWindow의 Player.Opened += Update_TotalPlayTime; Player.Opened += Update_Informaion; 보다 위에 있어야 함
                    media.Position = new TimeSpan(_position);
                    isPause = true;

                    Opened -= Load_Position;

                    Opened?.Invoke(sender1, e1);
                }
            }
        }

        #endregion

        #region PlayList Setting

        public static void PlayList_New()
        {
            MediaStop();
            PlayingIndex = -1;
            PlayList = new List<MediaElement_GG>();

            Changed?.Invoke(PlayList, null);
        }

        public static void PlayList_Save()
        {
            SavePlayList();
        }

        public static void PlayList_SaveAs()
        {
            SaveFileDialog dialog = new SaveFileDialog();

            dialog.Filter = "Play List Files (*.gpl)|*.gpl";

            if (dialog.ShowDialog() == true)
            {
                string name = dialog.FileName;
                string[] splitList = dialog.FileName.Split('.');

                if (splitList[splitList.Length - 1] != "gpl")
                    name += ".gpl";

                SavePlayList(name);
            }
        }

        public static void PlayList_Load()
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Filter = "Play List Files (*.gpl)|*.gpl";

            if (dialog.ShowDialog() == true)
            {
                MediaStop();
                LoadPlayList(dialog.FileName);
                Changed?.Invoke(PlayList, null);
            }
        }

        #endregion

        #region PlayList Control

        public static void MediaAddDialog()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.Filter = "Audio Files|*.aac;*.ac3;*.aif;*.aiff;*.ape;*.cda;*.flac;*.m4a;*.mid;*.midi;*.mod;*.mp2;*.mp3;*.mpc;*.ofs;*.ogg;*.rmi;*.tak;*.wav;*.wma;*.wv|" +
                  "MP3 Files|*.mp3|FLAC Files|*.flac|WMA Files|*.wma|WAV Files|*.wav|AAC Files|*.aac|AMR Files|*.amr|ALAC Files|*.alac|AIFF Files|*.aiff|All Files|*.*";

            if (dialog.ShowDialog() == true)
            {
                foreach (var item in dialog.FileNames)
                    PlayList.Add(new MusicElement(item));
                Changed?.Invoke(PlayList, null);
            }
        }

        public static void MediaInsert(string[] lists, int index = -1)
        {
            foreach (string item in lists)
            {
                if (System.IO.Path.GetExtension(item.ToLowerInvariant()) == ".gpl")
                {
                    InsertPlayList(item, index);
                }
                else
                    PlayList.Insert(index, new MusicElement(item));
            }
            Changed?.Invoke(PlayList, null);
        }

        public static void MediaDelete(int i)
        {
            if (PlayingIndex == i)
            {
                MediaStop();
            }
            if (i < 0 || i >= PlayList.Count)
                throw new ArgumentOutOfRangeException();
            PlayList.RemoveAt(i);
            Changed?.Invoke(PlayList, null);
        }

        #endregion

        #region Media Control

        private static void MediaOpen()
        {
            if (PlayingIndex < 0)
                PlayingIndex = 0;
            media.Open(new Uri(PlayList[PlayingIndex].Path));
        }

        public static void MediaSelectPlay(int i)
        {
            if (i < 0 || i >= PlayList.Count)
                throw new ArgumentOutOfRangeException();

            if (isPause == false || PlayingIndex != i)
            {
                PlayingIndex = i;
                MediaStop();
            }

            MediaPlay();
        }

        public static void MediaPlay()
        {
            if (PlayList.Count == 0)
                return;

            if (media.Source == null)
            {
                MediaOpen();
            }

            media.Play();
            Volume = _volume;

            isPlay = true;
            isPause = false;

            Played?.Invoke(media, null);
        }

        public static void MediaStop()
        {
            if (media.Source != null)
            {
                media.Stop();
                isPlay = false;
                media.Close();
                Stoped?.Invoke(media, null);
            }
        }

        public static void MediaPause()
        {
            if (media.CanPause == true)
            {
                if (isPause == true)
                    MediaPlay();
                else
                {
                    media.Pause();
                    isPlay = false;
                    isPause = true;
                    Paused?.Invoke(media, null);
                }
            }
        }

        public static void MediaNext()
        {
            if (PlayList.Count == 1)
                return;

            MediaStop();

            if (isShuffle == true)
            {
                PlayingIndex = rand.Next(0, PlayList.Count);
            }
            else
            {
                PlayingIndex++;
            }

            if (PlayingIndex >= PlayList.Count || PlayingIndex < 0)
                PlayingIndex = 0;

            MediaPlay();
        }

        public static void MediaPrevious()
        {
            if (PlayList.Count == 1)
                return;

            MediaStop();

            if (isShuffle == true)
            {
                PlayingIndex = rand.Next(0, PlayList.Count);
            }
            else
            {
                PlayingIndex--;
            }

            if (PlayingIndex >= PlayList.Count || PlayingIndex < 0)
                PlayingIndex = PlayList.Count - 1;

            MediaPlay();
        }

        #endregion

        #region File Control

        #region Setting

        private static void SaveSetting()
        {
            DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(List<object>));

            // List<object> writeList = new List<object>() { isPause, isPlay, isShuffle, isRepeatOne, _volume.ToString("F2") };
            // 0.1.3 까지 저장 방식

            List<object> writeList = new List<object>() { isPause, isPlay, isShuffle, isRepeatOne, _volume.ToString("F2"), PlayListPath, Top, Left, Width, Height };

            try
            {
                using (System.IO.FileStream fs = new System.IO.FileStream("setting.gg", System.IO.FileMode.Create, System.IO.FileAccess.Write))
                {
                    dcjs.WriteObject(fs, writeList);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, PROGRAM_NAME);
            }
        }

        private static void LoadSetting()
        {
            DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(List<object>));

            List<object> readList = new List<object>();

            try
            {
                using (System.IO.FileStream fs = new System.IO.FileStream("setting.gg", System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    readList = dcjs.ReadObject(fs) as List<object>;
                }

                isPause = (bool)readList[0];
                isPlay = (bool)readList[1];
                isShuffle = (bool)readList[2];
                isRepeatOne = (bool)readList[3];

                //Volume = double.Parse(readList[4].ToString());
                Volume = Convert.ToDouble(readList[4]);

                // 0.1.3 다음 버전을 위한 Load
                if (readList.Count > 5)
                {
                    PlayListPath = readList[5].ToString();
                    Top = Convert.ToDouble(readList[6]);
                    Left = Convert.ToDouble(readList[7]);
                    Width = Convert.ToDouble(readList[8]);
                    Height = Convert.ToDouble(readList[9]);
                }
            }
            catch (Exception e)
            {
                if (e is System.IO.FileNotFoundException)
                    return;
                else
                    MessageBox.Show(e.Message, PROGRAM_NAME);
            }

        }

        #endregion

        #region PlayList

        private static void SavePlayList(string fileName = null)
        {
            DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(List<string>));

            if (string.IsNullOrWhiteSpace(fileName) == false)
                PlayListPath = fileName;

            List<string> writeList = new List<string>();

            writeList.Add(PlayList.Count.ToString());

            foreach (MediaElement_GG item in PlayList)
                writeList.Add(item.Path);

            writeList.Add(PlayingIndex.ToString());

            // ver 0.1.3 이후
            _position = media.Position.Ticks;

            writeList.Add(_position.ToString());

            try
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(PlayListPath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                {
                    dcjs.WriteObject(fs, writeList);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, PROGRAM_NAME);
            }
        }

        private static void LoadPlayList(string fileName)
        {
            DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(List<string>));

            List<string> readList = new List<string>();
            PlayList = new List<MediaElement_GG>();

            PlayListPath = fileName;

            try
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    readList = dcjs.ReadObject(fs) as List<string>;
                }

                if (int.TryParse(readList[0], out int countMusic) == false)
                {
                    for (int i = 0; i < readList.Count - 1; i++)
                    {
                        PlayList.Add(new MusicElement(readList[i]));
                    }

                    PlayingIndex = Convert.ToInt32(readList[readList.Count - 1]);
                }
                else
                {
                    for (int i = 1; i <= countMusic; i++)
                    {
                        PlayList.Add(new MusicElement(readList[i]));
                    }

                    PlayingIndex = Convert.ToInt32(readList[countMusic + 1]);
                    _position = Convert.ToInt64(readList[countMusic + 2]);
                }
            }
            catch (Exception e)
            {
                if (e is System.IO.FileNotFoundException)
                { // 재생목록 파일이 없을 경우 ?
                    // LoadPlayList("lately.gpl"); lately.gpl로 할 것인가?
                    return;
                }
                else
                    MessageBox.Show(e.Message, PROGRAM_NAME);
            }

            PlayList_Loaded(PlayList, null);
        }

        private static void InsertPlayList(string fileName, int index = -1)
        {
            DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(List<string>));

            List<string> readList = new List<string>();

            if (index < 0)
                index = PlayList.Count;

            try
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    readList = dcjs.ReadObject(fs) as List<string>;
                }

                var musicList = new List<MusicElement>();
                if (int.TryParse(readList[0], out int countMusic) == false)
                {
                    for (int i = 0; i < readList.Count - 1; i++)
                    {
                        musicList.Add(new MusicElement(readList[i]));
                    }

                    PlayList.InsertRange(index, musicList);
                }
                else
                {
                    for (int i = 1; i <= countMusic; i++)
                    {
                        musicList.Add(new MusicElement(readList[i]));
                    }

                    PlayList.InsertRange(index, musicList);
                }
            }
            catch (Exception e)
            {
                if (e is System.IO.FileNotFoundException)
                    return;
                else
                    MessageBox.Show(e.Message, PROGRAM_NAME);
            }
        }

        #endregion

        #endregion
    }
}
