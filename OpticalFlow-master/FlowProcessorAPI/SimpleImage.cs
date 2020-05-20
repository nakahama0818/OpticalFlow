//**************************************************************************************
//
// This header is an inseparable part of the source code.
// This source code is created by György Richárd Bogár.
// It is mandatory to refer to the source in all uses that continuously contain
// at least 10% of the code or any smaller code snippet which makes it identifiable.
//
//**************************************************************************************

using OpenCL.Net;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using FlowProcessorAPI.Interfaces;

namespace FlowProcessorAPI
{
    public class SimpleImage : ISimpleImage
    {
        #region constructors
        public SimpleImage(){}
        public SimpleImage(Bitmap image)
        {
            this.ImageWidth = image.Width;
            this.ImageHeight = image.Height;
            
            BitmapData bitmapData = image.LockBits(new Rectangle(0, 0, ImageWidth, ImageHeight), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            this.ImageStride = bitmapData.Stride;
            this.ImageByteSize = bitmapData.Stride * bitmapData.Height;
            this.BitmapWidth = bitmapData.Width;
            this.BitmapHeight = bitmapData.Height;

            this.ByteArray = new byte[this.ImageByteSize];
            Marshal.Copy(bitmapData.Scan0, this.ByteArray, 0, this.ImageByteSize);
            image.UnlockBits(bitmapData);
        }

        public SimpleImage(SimpleImage originalImage, byte[] byteArray)
        {
            this.ImageWidth = originalImage.ImageWidth;
            this.ImageHeight = originalImage.ImageHeight;
            this.ImageStride = originalImage.ImageStride;
            this.ImageByteSize = originalImage.ImageByteSize;
            this.BitmapWidth = originalImage.BitmapWidth;
            this.BitmapHeight = originalImage.BitmapHeight;
            this.ByteArray = byteArray;

        }

        public SimpleImage(SimpleImage originalImage)
        {
            this.ImageWidth = originalImage.ImageWidth;
            this.ImageHeight = originalImage.ImageHeight;
            this.ImageStride = originalImage.ImageStride;
            this.ImageByteSize = originalImage.ImageByteSize;
            this.BitmapWidth = originalImage.BitmapWidth;
            this.BitmapHeight = originalImage.BitmapHeight;
            this.ByteArray = new byte[this.ImageByteSize];
            for (int i = 0; i < this.ImageByteSize; i++)
            {
                this.ByteArray[i] = originalImage.ByteArray[i];
            }
        }
        #endregion

        #region public static fields
        public static IntPtr intPtrSize = (IntPtr)Marshal.SizeOf(typeof(IntPtr));
        public static OpenCL.Net.ImageFormat clImageFormat = new OpenCL.Net.ImageFormat(ChannelOrder.RGBA, ChannelType.Unsigned_Int8);
        public static OpenCL.Net.ImageFormat clFlowImageFormat = new OpenCL.Net.ImageFormat(ChannelOrder.RGB, ChannelType.Unsigned_Int16);
        #endregion

        #region public fields and properties
        public Bitmap Bitmap {  get { return ConvertToBitmap(PixelFormat.Format32bppArgb); } }
        public Bitmap FlowBitmap { get { return ConvertToBitmap(PixelFormat.Format48bppRgb); } }
        public int BitmapWidth;
        public int BitmapHeight;
        public byte[] ByteArray;
        public int ImageWidth, ImageHeight;
        public int ImageByteSize;
        public int ImageStride;
        #endregion

        #region public methods
        public void downSampleAttributes()
        {
            this.ImageHeight /= 2;
            this.ImageWidth /= 2;
            this.ImageStride = this.ImageWidth * 4;

            this.BitmapWidth /= 2;
            this.BitmapHeight /= 2;
            this.ImageByteSize = this.ImageHeight * this.ImageStride;
        }
        #endregion

        #region private methods
        private Bitmap ConvertToBitmap(PixelFormat format)
        {
            Bitmap result;

            GCHandle pinnedOutputArray = GCHandle.Alloc(this.ByteArray, GCHandleType.Pinned);
            IntPtr outputBmpPointer = pinnedOutputArray.AddrOfPinnedObject();
            result = new Bitmap((this.ImageWidth), (this.ImageHeight), (this.ImageStride), format, outputBmpPointer);
            pinnedOutputArray.Free();
            return result;
        }
        #endregion
    }
}
