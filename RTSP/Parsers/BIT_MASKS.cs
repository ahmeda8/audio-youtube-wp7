﻿using System;

namespace RTSP
{
    public class BIT_MASKS
    {
        public const byte VERSION_MASK = 192;
        public const byte PADDING_MASK = 32;
        public const byte EXTENSION_MASK = 16;
        public const byte CC_MASK = 15;
        public const byte MARKER = 128;
        public const byte PT_MASK = 127;
     
    }
}
