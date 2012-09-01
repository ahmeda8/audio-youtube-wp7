using System;
using System.Net.Sockets;
using System.Collections;

namespace RTSP
{
    public class RTP : IDisposable
    {
        public struct RTP_HEADER
        {
            public byte version;
            public byte p;
            public byte x;
            public byte cc;
            public byte m;
            public byte pt;
            public UInt16 seq;
            public UInt32 ts;
            public UInt32 ssrc;
            public UInt32[] csrc;
        }

        public byte[] _payload;
        public RTP_HEADER _header;
        private byte[] DATA;
        private int BytesTransferred = 0;


        public RTP()
        {
             //_header = new RTP_HEADER();
             
        }

        public void Save(byte[] e,int bt)
        {
            DATA = e;
            BytesTransferred = bt;
        }

        public byte[] PAYLOAD
        {
            get 
            {
                int header_size = 12;
                int bytes_to_copy = BytesTransferred - header_size;
                byte[] pl = new byte[bytes_to_copy];
                for (int i = 0; i < bytes_to_copy; i++)
                    pl[i] = DATA[header_size++];
                
                return pl;
            }
        }

        public RTP_HEADER HEADER
        {
            get
            {
                RTP_HEADER head = new RTP_HEADER();
                head.version = (byte)(DATA[0] & BIT_MASKS.VERSION_MASK);
                head.version = (byte)(head.version >> 6);
                head.p = (byte)(DATA[0] & BIT_MASKS.PADDING_MASK);
                head.p = (byte)(head.p >> 5);
                head.x = (byte)(DATA[0] & BIT_MASKS.EXTENSION_MASK);
                head.x = (byte)(head.p >> 4);
                head.cc = (byte)(DATA[0] & BIT_MASKS.CC_MASK);
                head.m = (byte)(DATA[1] & BIT_MASKS.MARKER);
                head.m = (byte)(head.m >> 7);
                head.pt = (byte)(DATA[1] & BIT_MASKS.PT_MASK);
                head.seq = DATA[2];
                head.seq = (UInt16)(head.seq << 8);
                head.seq = (UInt16)(head.seq | DATA[3]);

                int buffer_progress = 4;
                head.ts = DATA[buffer_progress++];
                int next_start = buffer_progress;
                int next_stop = buffer_progress + 3;
                for (int i = next_start; i < next_stop; i++)
                {
                    head.ts = head.ts << 8;
                    head.ts = head.ts | DATA[i];
                    buffer_progress++;
                }

                head.ssrc = DATA[buffer_progress++];
                next_start = buffer_progress;
                next_stop = buffer_progress + 3;
                for (int i = next_start; i < next_stop; i++)
                {
                    head.ssrc = head.ssrc << 8;
                    head.ssrc = head.ssrc | DATA[i];
                    buffer_progress++;
                }

                head.csrc = new UInt32[head.cc];
                for (int i = 0; i < head.cc; i++)
                {
                    head.csrc[i] = DATA[buffer_progress++];
                    next_start = buffer_progress;
                    next_stop = buffer_progress + 3;
                    for (int j = next_start; j < next_stop; j++)
                    {
                        head.ssrc = head.ssrc << 8;
                        head.ssrc = head.ssrc | DATA[j];
                        buffer_progress++;
                    }

                }
                return head;
            }
        }

        public void Parse(byte[] e)
        {
            
            _header.version = (byte)(e[0] & BIT_MASKS.VERSION_MASK);
            _header.version = (byte)(_header.version >> 6);
            _header.p = (byte)(e[0] & BIT_MASKS.PADDING_MASK);
            _header.p = (byte)(_header.p >> 5);
            _header.x = (byte)(e[0] & BIT_MASKS.EXTENSION_MASK);
            _header.x = (byte)(_header.p >> 4);
            _header.cc = (byte)(e[0] & BIT_MASKS.CC_MASK);
            _header.m = (byte)(e[1] & BIT_MASKS.MARKER);
            _header.m = (byte)(_header.m >> 7);
            _header.pt = (byte)(e[1] & BIT_MASKS.PT_MASK);
            _header.seq = e[2];
            _header.seq = (UInt16)(_header.seq << 8);
            _header.seq = (UInt16)(_header.seq | e[3]);

            int buffer_progress = 4;
            _header.ts = e[buffer_progress++];
            int next_start = buffer_progress;
            int next_stop = buffer_progress + 3;
            for (int i = next_start; i < next_stop; i++)
            {
                _header.ts = _header.ts << 8;
                _header.ts = _header.ts | e[i];
                buffer_progress++;
            }

            _header.ssrc = e[buffer_progress++];
            next_start = buffer_progress;
            next_stop = buffer_progress + 3;
            for (int i = next_start; i < next_stop; i++)
            {
                _header.ssrc = _header.ssrc << 8;
                _header.ssrc = _header.ssrc | e[i];
                buffer_progress++;
            }

            _header.csrc = new UInt32[_header.cc];
            for (int i = 0; i < _header.cc; i++)
            {
                _header.csrc[i] = e[buffer_progress++];
                next_start = buffer_progress;
                next_stop = buffer_progress + 3;
                for (int j = next_start; j < next_stop; j++)
                {
                    _header.ssrc = _header.ssrc << 8;
                    _header.ssrc = _header.ssrc | e[j];
                    buffer_progress++;
                }
 
            }

            _payload = new byte[e.Length - buffer_progress];
            int payload_init = 0;
            next_start = buffer_progress + 1;
            next_stop = e.Length;
            for (int i = next_start; i <= next_stop; i++)
            {
                _payload[payload_init++] = e[buffer_progress++];
                
            }
        }


        public void Dispose()
        {
            _payload = null;
        }
    }
}
