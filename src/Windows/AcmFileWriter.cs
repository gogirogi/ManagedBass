﻿using System;
using System.Runtime.InteropServices;

namespace ManagedBass.Enc
{
    public class AcmFileWriter : IAudioWriter
    {
        static BassFlags ToBassFlags(WaveFormatTag WfTag, int BitsPerSample)
        {
            if (WfTag == WaveFormatTag.Pcm && BitsPerSample == 8)
                return BassFlags.Byte;
            if (WfTag == WaveFormatTag.IeeeFloat && BitsPerSample == 32)
                return BassFlags.Float;

            return 0;
        }

        static int GetDummyChannel(WaveFormat Format)
        {
            Bass.Init(0);
            return Bass.CreateStream(Format.SampleRate, Format.Channels, BassFlags.Decode | ToBassFlags(Format.Encoding, Format.BitsPerSample), StreamProcedureType.Push);
        }
        
        readonly int _channel, _handle;
        readonly object _syncLock = new object();
        
        public AcmFileWriter(string FileName, WaveFormatTag Encoding, WaveFormat Format)
        {
            if (FileName == null)
                throw new ArgumentNullException(nameof(FileName));

            _channel = GetDummyChannel(Format);
        
            // Get the Length of the ACMFormat structure
            var suggestedFormatLength = BassEnc.GetACMFormat(0);
            var acmFormat = Marshal.AllocHGlobal(suggestedFormatLength);

            try
            {
                // Retrieve ACMFormat and Init Encoding
                if (BassEnc.GetACMFormat(_channel,
                    acmFormat,
                    suggestedFormatLength,
                    null,
                    // If encoding is Unknown, then let the User choose encoding.
                    Encoding == WaveFormatTag.Unknown ? 0 : ACMFormatFlags.Suggest,
                    Encoding) != 0)
                    _handle = BassEnc.EncodeStartACM(_channel, acmFormat, 0, FileName);
                else throw new Exception(Bass.LastError.ToString());
            }
            finally
            {
                // Free the ACMFormat structure
                Marshal.FreeHGlobal(acmFormat);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() => BassEnc.EncodeStop(_handle, true);

        #region Write
        public bool Write(byte[] Buffer, int Length)
        {
            lock (_syncLock)
            {
                Bass.StreamPutData(_channel, Buffer, Length);

                Bass.ChannelGetData(_channel, Buffer, Length);

                return true;
            }
        }

        public bool Write(short[] Buffer, int Length)
        {
            lock (_syncLock)
            {
                Bass.StreamPutData(_channel, Buffer, Length);

                Bass.ChannelGetData(_channel, Buffer, Length);

                return true;
            }
        }

        public bool Write(float[] Buffer, int Length)
        {
            lock (_syncLock)
            {
                Bass.StreamPutData(_channel, Buffer, Length);

                Bass.ChannelGetData(_channel, Buffer, Length);

                return true;
            }
        }
        #endregion
    }
}