using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Media.Imaging;

/*
Copyright (c) <2018> ICRL

See the file license.txt for copying permission.
*/
namespace MusicPlayer_GG
{
    [DataContract]
    class MusicElement : MediaElement
    {
        string fullPath;
        string artist, title;
        BitmapImage albumArt = null;

        [DataMember]
        public override string Path
        {
            get { return fullPath; }
            set { fullPath = value; }
        }

        public string Artist
        {
            get { return artist; }
        }

        public string Title
        {
            get { return title; }
        }

        public override string Information
        {
            get
            {
                if (string.IsNullOrWhiteSpace(artist) == true || string.IsNullOrWhiteSpace(title) == true)
                    return System.IO.Path.GetFileNameWithoutExtension(fullPath);
                else
                    return artist + " - " + title;
            }
        }

        public BitmapImage AlbumArt
        {
            get { return albumArt; }
        }


        public MusicElement(string full)
        {
            fullPath = full;

            if (System.IO.Path.GetExtension(full).ToLowerInvariant() == ".mp3")
                GetTagNotTagLib(full);
        }


        #region Manage Tag

        private void GetTagWithTagLib(string path)
        {
            /*
            TagLib.File tag = new TagLib.Mpeg.AudioFile(full);

            //if (System.IO.Path.GetExtension(full).ToLowerInvariant() == ".mp3")
            if (tag.Tag.IsEmpty == false)
            {
                //TagLib.File tag = new TagLib.Mpeg.AudioFile(full);
                artist = tag.Tag.FirstAlbumArtist;
                title = tag.Tag.Title;

                if (tag.Tag.Pictures != null && tag.Tag.Pictures.Length > 0)
                {
                    TagLib.IPicture pic = tag.Tag.Pictures[0];
                    var ms = new System.IO.MemoryStream(pic.Data.Data);
                    ms.Seek(0, System.IO.SeekOrigin.Begin);

                    albumArt = new BitmapImage();

                    albumArt.BeginInit();
                    albumArt.StreamSource = ms;
                    albumArt.EndInit();
                }
            }
            */

            throw new System.NotImplementedException("In GetTagWithTagLib(string path)");
        }

        private void GetTagNotTagLib(string path)
        {
            BinaryReader br;

            try
            {
                FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read);
                br = new BinaryReader(fs, Encoding.Unicode);
            }
            catch (IOException)
            {
                // System.Windows.MessageBox.Show("파일이 이미 사용중입니다.", "경고", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            try
            {
                byte[] tag = new byte[3];
                byte[] version = new byte[2];
                byte[] flags = new byte[1];
                byte[] size = new byte[4];

                int Size = 0;

                while (true)
                {
                    br.Read(tag, 0, tag.Length);
                    br.Read(version, 0, version.Length);
                    br.Read(flags, 0, flags.Length);
                    br.Read(size, 0, size.Length);

                    if (tag[0] != 'I' || tag[1] != 'D' || tag[2] != '3')
                        return;

                    Size = (size[0] & 127) << 7 * 3 | (size[1] & 127) << 7 * 2 | (size[2] & 127) << 7 * 1 | (size[0] & 127) << 7 * 0;

                    if (version[0] == 4)
                    {// ID3 V1 일 경우
                        byte[] temp = new byte[Size];
                        br.Read(temp, 0, Size);

                        continue;
                    }
                    else if (version[0] == 3)
                        // ID3 V2 일 경우
                        break;
                    else
                        throw new System.Exception("Error in getting ID3 Tag");
                }

                if ((flags[0] >> 6 & 1) == 1)
                {
                    byte[] exSize = new byte[4];
                    byte[] numFlags = new byte[1];
                    byte[] exFlags = new byte[1];

                    br.Read(exSize, 0, exSize.Length);
                    br.Read(numFlags, 0, numFlags.Length);
                    br.Read(exFlags, 0, exFlags.Length);
                }

                while (br.BaseStream.Position < Size)
                {
                    byte[] frameID = new byte[4];
                    byte[] frameSize = new byte[4];
                    byte[] frameFlags = new byte[2];

                    br.Read(frameID, 0, frameID.Length);
                    int temp = frameID[0] + frameID[1] + frameID[2] + frameID[3];
                    if (temp == 0)
                        continue;

                    br.Read(frameSize, 0, frameSize.Length);
                    br.Read(frameFlags, 0, frameFlags.Length);

                    string StrframeID = "" + (char)frameID[0] + (char)frameID[1] + (char)frameID[2] + (char)frameID[3];
                    int FrameSize = frameSize[0] << 8 * 3 | frameSize[1] << 8 * 2 | frameSize[2] << 8 * 1 | frameSize[3] << 8 * 0;

                    if (StrframeID == "APIC")
                    {
                        byte[] data = new byte[FrameSize];
                        br.Read(data, 0, data.Length);
                        // string t = ByteToString(data);

                        int i = 0;
                        if (i < data.Length)
                            i++;
                        for (; i < data.Length; i++)
                        {
                            if (data[i] == 0)
                                break;
                        }
                        if (i < data.Length)
                            i++;
                        for (; i < data.Length; i++)
                        {
                            if (data[i] == 0)
                            {
                                i++;
                                break;
                            }
                            else if (data[i] == 0xff)
                                break;
                        }

                        try
                        {
                            MemoryStream ms = new MemoryStream(data, i, data.Length - i);

                            albumArt = new BitmapImage();
                            albumArt.BeginInit();
                            albumArt.StreamSource = ms;
                            albumArt.EndInit();
                        }
                        catch (System.Exception e)
                        {
                            System.Console.WriteLine(e.Message);
                            albumArt = null;
                        }
                    }
                    else if (StrframeID == "TIT2")
                    {
                        byte[] data = new byte[FrameSize];
                        br.Read(data, 0, data.Length);

                        title = ConvertToString(data);
                    }
                    else if (StrframeID == "TPE1")
                    {
                        byte[] data = new byte[FrameSize];
                        br.Read(data, 0, data.Length);

                        artist = ConvertToString(data);
                    }
                    else
                    {
                        byte[] data = new byte[FrameSize];
                        br.Read(data, 0, data.Length);
                    }

                }
            }
            catch (System.Exception) { }
            finally
            {
                br.Close();
            }
        }

        static string ConvertToString(byte[] arr)
        {
            List<byte> list = new List<byte>(arr);

            if (IsConvertToString(arr) == false)
            {
                int i = 0;
                while (arr[i++] != ' ') ;
                list.RemoveRange(0, i);

                return Encoding.GetEncoding(Encoding.Default.CodePage).GetString(list.ToArray());
            }

            for (int i = 0; i < list.Count - 1; i++)
            {
                if (list[i] == 255 && list[i + 1] == 254)
                {
                    list.RemoveRange(0, i + 2);
                    i = -1;
                }
            }

            return Encoding.GetEncoding(Encoding.Unicode.CodePage).GetString(list.ToArray());
        }

        static bool IsConvertToString(byte[] arr)
        {
            for (int i = 0; i < arr.Length - 1; i++)
            {
                if (arr[i] == 255 && arr[i + 1] == 254)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        private string ByteToString(byte[] data)
        {
            string str = "";

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == 0)
                    str += "\n";
                else
                    str += (char)data[i];
            }

            return str;
        }

        #region String

        public override string ToString()
        {
            return Information;
        }

        public static implicit operator string(MusicElement m)
        {
            return m.Information;
        }

        #endregion
    }
}
