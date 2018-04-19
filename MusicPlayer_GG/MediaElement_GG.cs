/*
Copyright (c) <2018> GiGong

See the file license.txt for copying permission.
*/

namespace MusicPlayer_GG
{
    abstract class MediaElement_GG
    {
        [System.Runtime.Serialization.DataMember]
        abstract public string Path { get; set; }

        abstract public string FileName { get; }

        abstract public string Information { get; }
        

        abstract public override string ToString();

        public static implicit operator string(MediaElement_GG m)
        {
            return m.Information;
        }
    }
}
