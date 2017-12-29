﻿using Microsoft.Win32;
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
        static MediaPlayer media = new MediaPlayer();
        static List<MediaElement> _list = new List<MediaElement>();
        static Random rand = new Random((int)DateTime.Now.Ticks);

        static int playingIndex;
        static double _volume;
        static bool isPause, isPlay;

        public static bool isShuffle, isRepeatOne;

        public static EventHandler //Ended, // Played
                                    Played, Stoped, Paused, Changed;

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
                    //return (int)Math.Round(media.Volume * 100) / 100.0;
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

        public static List<MediaElement> PlayList
        {
            get { return _list; }
        }

        public static string NowTitle
        {
            get
            {
                if (isPlay == true)
                    return (PlayList[playingIndex] as MusicElement).Title;
                return "";
            }
        }

        public static string NowPlaying
        {
            get
            {
                if (isPlay == true)
                    return PlayList[playingIndex];
                return "";
            }
        }

        public static BitmapImage NowAlbumArt
        {
            get
            {
                if (isPlay == true && (PlayList[playingIndex] is MusicElement))
                    return (PlayList[playingIndex] as MusicElement).AlbumArt;
                return null;
            }
        }

        public static int PlayingIndex
        {
            get { return playingIndex; }
        }

        #endregion 

        #region PlayList Setting

        public static void PlayList_New()
        {
            MediaStop();
            playingIndex = -1;
            _list = new List<MediaElement>();

            Changed?.Invoke(PlayList, null);
        }

        public static void PlayList_Save()
        {
            SaveFileDialog dialog = new SaveFileDialog();

            dialog.Filter = "Play List Files (*.gpl)|*.gpl";

            if (dialog.ShowDialog() == true)
            {
                string name = dialog.FileName;
                string[] tList = dialog.FileName.Split('.');

                if (tList[tList.Length - 1] != "gpl")
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
                if (MessageBox.Show("현재 재생목록을 지우시겠습니까?", "Music Player", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    MediaStop();
                    _list = new List<MediaElement>();
                }
                LoadPlayList(dialog.FileName);
                Changed?.Invoke(PlayList, null);
            }
        }

        #endregion

        #region PlayList Control

        public static void MediaAdd()
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

        public static void MediaAdd(string[] lists)
        {
            foreach (string item in lists)
            {
                if (System.IO.Path.GetExtension(item.ToLowerInvariant()) == ".gpl")
                    LoadPlayList(item);
                else
                    PlayList.Add(new MusicElement(item));
            }
            Changed?.Invoke(PlayList, null);
        }

        public static void MediaDelete(int i)
        {
            if (playingIndex == i)
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
            if (playingIndex < 0)
                playingIndex = 0;
            media.Open(new Uri(PlayList[playingIndex].Path));
        }

        public static void MediaSelectPlay(int i)
        {
            if (i < 0 || i >= PlayList.Count)
                throw new ArgumentOutOfRangeException();
            playingIndex = i;

            MediaStop();
            MediaPlay();
        }

        public static void MediaPlay()
        {
            if (PlayList.Count == 0)
                return;

            if (media.Source == null)
                MediaOpen();

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
                playingIndex = rand.Next(0, PlayList.Count);
            }
            else
            {
                playingIndex++;
            }

            if (playingIndex >= PlayList.Count || playingIndex < 0)
                playingIndex = 0;

            MediaPlay();
        }

        public static void MediaPrevious()
        {
            if (PlayList.Count == 1)
                return;

            MediaStop();

            if (isShuffle == true)
            {
                playingIndex = rand.Next(0, PlayList.Count);
            }
            else
            {
                playingIndex--;
            }

            if (playingIndex >= PlayList.Count || playingIndex < 0)
                playingIndex = PlayList.Count - 1;

            MediaPlay();
        }

        #endregion

        #region Events

        static Player()
        {
            Volume = 0.5;
            playingIndex = -1;
            isPause = false;
            isPlay = false;
            isShuffle = false;
            isRepeatOne = false;

            media.MediaEnded += MediaEnded;
        }

        public static void Initiate(EventHandler handler)
        {
            media.MediaOpened += handler;

        }

        public static void Event_Loaded(object sender, EventArgs e)
        {
            LoadSetting();
            LoadPlayList("");
        }

        public static void Event_Closed(object sender, EventArgs e)
        {
            MediaStop();
            SaveSetting();
            SavePlayList("");
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

        #endregion

        #region File Control

        private static void SaveSetting()
        {
            DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(List<object>));

            List<object> tList = new List<object>() { isPause, isPlay, isShuffle, isRepeatOne, _volume.ToString("F2") };

            try
            {
                using (System.IO.FileStream fs = new System.IO.FileStream("setting.gg", System.IO.FileMode.Create, System.IO.FileAccess.Write))
                {
                    dcjs.WriteObject(fs, tList);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private static void LoadSetting()
        {
            DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(List<object>));

            List<object> tList = new List<object>();

            try
            {
                using (System.IO.FileStream fs = new System.IO.FileStream("setting.gg", System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    tList = dcjs.ReadObject(fs) as List<object>;
                }

                isPause = (bool)tList[0];
                isPlay = (bool)tList[1];
                isShuffle = (bool)tList[2];
                isRepeatOne = (bool)tList[3];
                Volume = double.Parse(tList[4].ToString());
            }
            catch (Exception e)
            {
                if (e is System.IO.FileNotFoundException)
                    return;
                else
                    MessageBox.Show(e.Message);
            }

        }

        private static void SavePlayList(string fileName)
        {
            DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(List<string>));

            if (string.IsNullOrWhiteSpace(fileName))
                fileName = "lately.gpl";

            List<string> tList = new List<string>();

            foreach (MediaElement item in _list)
                tList.Add(item.Path);
            tList.Add(playingIndex.ToString());

            try
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                {
                    dcjs.WriteObject(fs, tList);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private static void LoadPlayList(string fileName)
        {
            DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(List<string>));

            if (string.IsNullOrWhiteSpace(fileName))
                fileName = "lately.gpl";

            List<string> tList = new List<string>();

            try
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    tList = dcjs.ReadObject(fs) as List<string>;
                }

                for (int i = 0; i < tList.Count - 1; i++)
                    _list.Add(new MusicElement(tList[i]));
                playingIndex = int.Parse(tList[tList.Count - 1]);
            }
            catch (Exception e)
            {
                if (e is System.IO.FileNotFoundException)
                    return;
                else
                    MessageBox.Show(e.Message);
            }

        }

        #endregion
    }
}
