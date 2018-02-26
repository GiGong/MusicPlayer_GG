/*
Copyright (c) <2018> GiGong

See the file license.txt for copying permission.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer_GG
{
    abstract class MediaElement
    {
        [System.Runtime.Serialization.DataMember]
        abstract public string Path { get; set; }

        abstract public string Information { get; }


        abstract public override string ToString();

        public static implicit operator string(MediaElement m)
        {
            return m.Information;
        }
    }
}
