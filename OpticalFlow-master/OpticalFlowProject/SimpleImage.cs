using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenCL.Net;

namespace OpticalFlowProject
{
    class SimpleImage
    {
        public SimpleImage()
        {
        }

        public static IntPtr intPtrSize = (IntPtr)Marshal.SizeOf(typeof(IntPtr));
        public static ImageFormat clImageFormat = new ImageFormat(ChannelOrder.RGBA, ChannelType.Unsigned_Int8);


        public int BitmapWidth;
        public int BitmapHeight;
        //public byte[] ByteArray;
        //public Mem Image2DBuffer;
        public int ImageWidth, ImageHeight;
        public int ImageByteSize;
        public int ImageStride;
    }
}
