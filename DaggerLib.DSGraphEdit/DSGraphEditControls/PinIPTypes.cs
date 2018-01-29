using System;
using System.Collections.Generic;
using System.Text;

using DirectShowLib;

namespace DaggerLib.DSGraphEdit.PinTypes
{
    public sealed class PinDataTypes
    {
        /// Because DaggerLib uses .Net Types as Information Packets for DAGs, we'll create empty
        /// interfaces to represent the Major Media Types that DirectShow IPins hold.  Since all the interfaces
        /// implement IPinIPType, DaggerLib will see them as discreet IP ( information packet ) types 
        /// that are still interchangeable.  

        public interface IPinIPType
        {
        }

        public interface Unknown : IPinIPType { }
        public interface Null : IPinIPType { }
        public interface Video : IPinIPType { }
        public interface Audio : IPinIPType { }
        public interface Interleaved : IPinIPType { }
        public interface Text : IPinIPType { }
        public interface Stream : IPinIPType { }
        public interface VBI : IPinIPType { }
        public interface Midi : IPinIPType { }
        public interface File : IPinIPType { }
        public interface ScriptCommand : IPinIPType { }
        public interface AuxLine21Data : IPinIPType { }
        public interface Timecode : IPinIPType { }
        public interface LMRT : IPinIPType { }
        public interface URLStream : IPinIPType { }
        public interface AnalogVideo : IPinIPType { }
        public interface AnalogAudio : IPinIPType { }
        public interface Mpeg2Sections : IPinIPType { }
        public interface DTVCCData : IPinIPType { }
        public interface MSTVCaption : IPinIPType { }
        public interface DVDEncryptedPack : IPinIPType { }
        public interface MPEG1SystemStream : IPinIPType { }

        // don't allow instantiation of this class
        private PinDataTypes()
        { }

        /// <summary>
        /// Convert an IPin's major media type to it's corrisponding IPinIPType
        /// </summary>
        /// <param name="majorType"></param>
        /// <returns></returns>
        public static Type GetMajorPinType(Guid majorType)
        {
            if (majorType == MediaType.Null) return typeof(Null);
            if (majorType == MediaType.Video) return typeof(Video);
            if (majorType == MediaType.Audio) return typeof(Audio);
            if (majorType == MediaType.Interleaved) return typeof(Interleaved);
            if (majorType == MediaType.Texts) return typeof(Text);
            if (majorType == MediaType.Stream) return typeof(Stream);
            if (majorType == MediaType.VBI) return typeof(VBI);
            if (majorType == MediaType.Midi) return typeof(Midi);
            if (majorType == MediaType.File) return typeof(File);
            if (majorType == MediaType.ScriptCommand) return typeof(ScriptCommand);
            if (majorType == MediaType.AuxLine21Data) return typeof(AuxLine21Data);
            if (majorType == MediaType.Timecode) return typeof(Timecode);
            if (majorType == MediaType.LMRT) return typeof(LMRT);
            if (majorType == MediaType.URLStream) return typeof(URLStream);
            if (majorType == MediaType.AnalogVideo) return typeof(AnalogVideo);
            if (majorType == MediaType.AnalogAudio) return typeof(AnalogAudio);
            if (majorType == MediaType.Mpeg2Sections) return typeof(Mpeg2Sections);
            if (majorType == MediaType.DTVCCData) return typeof(DTVCCData);
            if (majorType == MediaType.MSTVCaption) return typeof(MSTVCaption);
            if (majorType == new Guid("ed0b916a-044d-11d1-aa78-00c04fc31d60")) return typeof(DVDEncryptedPack);
            if (majorType == new Guid(0xe436eb82, 0x524f, 0x11ce, 0x9f, 0x53, 0x00, 0x20, 0xaf, 0x0b, 0xa7, 0x70))
                return typeof(MPEG1SystemStream);
            return typeof(Unknown);
        }
    }
}
