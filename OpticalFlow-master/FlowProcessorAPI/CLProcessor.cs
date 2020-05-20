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
using System.Collections.Generic;
using FlowProcessorAPI.Interfaces;
using System.Drawing;
using System.IO;
namespace FlowProcessorAPI
{
    public class CLProcessor : IProcessor
    {
        #region constructors & destructors
        public CLProcessor():this(_PreDefinedKernelStrings){}

        public CLProcessor(IKernelStrings[] kernelList)
        {
            preLoadKernels(kernelList);
            if (kernelList != _PreDefinedKernelStrings)
            {
                preLoadKernels(_PreDefinedKernelStrings);
            }

            this.setUp();
        }

        ~CLProcessor()
        {
            Cl.ReleaseProgram(_Program);
            Cl.ReleaseCommandQueue(_CommandQueue);
        }
        #endregion

        #region private fields
        private Context _Context;
        private Device _Device;
        private Dictionary<string, string> _Kernels = new Dictionary<string, string>();
        private FileHandler _FileHandler = new FileHandler();
        private OpenCL.Net.Program _Program;
        private CommandQueue _CommandQueue;
        #endregion

        #region private static fields
        private static IKernelStrings[] _PreDefinedKernelStrings = {
                new KernelStrings("Monochrome.cl", "monochrome"),
                new KernelStrings("Flow.cl", "flow"),
                new KernelStrings("FlowDecorator.cl", "flowDecorator"),
                new KernelStrings("GaussianBlur1.cl", "gaussianBlur1"),
                new KernelStrings("DownSample.cl", "downample"),
                new KernelStrings("ModifyFlowImage.cl", "modifyFlowImage"),
                new KernelStrings("MonochromeTwo.cl", "monochromeTwo"),
                new KernelStrings("DownSampleTwo.cl", "downSampleTwo"),
                new KernelStrings("GaussianBlur1Two.cl", "gaussianBlur1Two"),
                new KernelStrings("CreateDerivatives.cl", "createDerivatives"),
                new KernelStrings("AdvancedFlow.cl", "advancedFlow"),
                new KernelStrings("FlowImageCreator.cl", "flowImageCreator"),
                new KernelStrings("FlowDecoratorColor.cl", "flowDecoratorColor"),
                new KernelStrings("CalcFlowError.cl", "calcFlowError"),
                new KernelStrings("PushImgFlow.cl", "pushImgFlow"),
                new KernelStrings("FlowLength.cl", "flowlength"),
                new KernelStrings("AngularError.cl", "angularError")};
    #endregion

    #region public methods

    #region universal
    public SimpleImage ProcessKernelOnOneFromPath(SimpleImage inputImage, KernelStrings kernelString, string path)
        {
            if (path == null)
            {
                throw new ArgumentException("Invalid path argument");
            }

            ErrorCode error;

            string kernelSource = _FileHandler.getKernelSourceFromPath(path);

            using (OpenCL.Net.Program program = Cl.CreateProgramWithSource(_Context, (uint)1, new[] { kernelSource }, null, out error))
            {
                CheckErr(error, "Cl.CreateProgramWithSource");

                buildProgram(program);

                SimpleImage tempImage = inputImage;

                Mem inputImage2DBuffer = (Mem)Cl.CreateImage2D(_Context, MemFlags.CopyHostPtr | MemFlags.ReadOnly, SimpleImage.clImageFormat, (IntPtr)tempImage.BitmapWidth, (IntPtr)tempImage.BitmapHeight, (IntPtr)0, tempImage.ByteArray, out error);

                CheckErr(error, "Cl.CreateCommandQueue");
                byte[] outputByteArray = null;

                    // Output preparation
                    outputByteArray = new byte[tempImage.ImageByteSize];
                    Mem outputImage2DBuffer = (Mem)Cl.CreateImage2D(_Context, MemFlags.CopyHostPtr | MemFlags.WriteOnly, SimpleImage.clImageFormat, (IntPtr)tempImage.ImageWidth, (IntPtr)tempImage.ImageHeight, (IntPtr)0, outputByteArray, out error);
                    CheckErr(error, "Cl.CreateImage2D output");

                Kernel currentKernel = Cl.CreateKernel(program, kernelString.FunctionName, out error);
                CheckErr(error, "Cl.CreateKernel");

                    // Kernel arguiment declaration
                    error = Cl.SetKernelArg(currentKernel, 0, SimpleImage.intPtrSize, inputImage2DBuffer);
                    error |= Cl.SetKernelArg(currentKernel, 1, SimpleImage.intPtrSize, outputImage2DBuffer);
                    error |= Cl.SetKernelArg(currentKernel, 2, SimpleImage.intPtrSize, tempImage.ImageWidth);
                    error |= Cl.SetKernelArg(currentKernel, 3, SimpleImage.intPtrSize, tempImage.ImageHeight);
                    CheckErr(error, "Cl.SetkernelArgs");

                    Event clevent;

                    // Enqueueing the input image
                    IntPtr[] originPtr = new IntPtr[] { (IntPtr)0, (IntPtr)0, (IntPtr)0 };
                    IntPtr[] regionPtr = new IntPtr[] { (IntPtr)tempImage.ImageWidth, (IntPtr)tempImage.ImageHeight, (IntPtr)1 };
                    IntPtr[] workGroupSizePtr = new IntPtr[] { (IntPtr)tempImage.ImageWidth, (IntPtr)tempImage.ImageHeight, (IntPtr)1 };
                    error = Cl.EnqueueWriteImage(_CommandQueue, inputImage2DBuffer, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, tempImage.ByteArray, 0, null, out clevent);
                    CheckErr(error, "Cl.EnqueueWriteBuffer");

                    // Enqueueing the kernel
                    error = Cl.EnqueueNDRangeKernel(_CommandQueue, currentKernel, 2, null, workGroupSizePtr, null, 0, null, out clevent);
                    CheckErr(error, "Cl.EnqueueNDRangeKernel");

                    error = Cl.Finish(_CommandQueue);
                    CheckErr(error, "Cl.Finish");

                    // Enqueueing the output image
                    error = Cl.EnqueueReadImage(_CommandQueue, outputImage2DBuffer, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, outputByteArray, 0, null, out clevent);
                    CheckErr(error, "Cl.EnqueueReadImage");

                    //Releasing the image buffers
                    Cl.ReleaseMemObject(inputImage2DBuffer);
                    Cl.ReleaseMemObject(outputImage2DBuffer);

                    //inputByteArray = outputByteArray;
                    tempImage = new SimpleImage(tempImage, outputByteArray);
                    inputImage2DBuffer = (Mem)Cl.CreateImage2D(_Context, MemFlags.CopyHostPtr | MemFlags.ReadOnly, SimpleImage.clImageFormat, (IntPtr)tempImage.BitmapWidth, (IntPtr)tempImage.BitmapHeight, (IntPtr)0, tempImage.ByteArray, out error);
                Cl.ReleaseMemObject(inputImage2DBuffer);

                Cl.ReleaseKernel(currentKernel);


                return tempImage;
            }
        }

        public SimpleImage ProcessPreBuiltKernelsOnOne(SimpleImage inputImage, string[] kernelNames)
        {
            ErrorCode error;

            SimpleImage tempImage = inputImage;

            Mem inputImage2DBuffer = (Mem)Cl.CreateImage2D(_Context, MemFlags.CopyHostPtr | MemFlags.ReadOnly, SimpleImage.clImageFormat, (IntPtr)tempImage.BitmapWidth, (IntPtr)tempImage.BitmapHeight, (IntPtr)0, tempImage.ByteArray, out error);

            CheckErr(error, "Cl.CreateCommandQueue");
            byte[] outputByteArray = null;

                for (int i = 0; i < kernelNames.Length; i++)
                {
                    // Output preparation
                    outputByteArray = new byte[tempImage.ImageByteSize];
                    Mem outputImage2DBuffer = (Mem)Cl.CreateImage2D(_Context, MemFlags.CopyHostPtr | MemFlags.WriteOnly, SimpleImage.clImageFormat, (IntPtr)tempImage.ImageWidth, (IntPtr)tempImage.ImageHeight, (IntPtr)0, outputByteArray, out error);
                    CheckErr(error, "Cl.CreateImage2D output");

                    Kernel currentKernel = Cl.CreateKernel(_Program, kernelNames[i], out error);

                    // Kernel arguiment declaration
                    error = Cl.SetKernelArg(currentKernel, 0, SimpleImage.intPtrSize, inputImage2DBuffer);
                    error |= Cl.SetKernelArg(currentKernel, 1, SimpleImage.intPtrSize, outputImage2DBuffer);
                    error |= Cl.SetKernelArg(currentKernel, 2, SimpleImage.intPtrSize, tempImage.ImageWidth);
                    error |= Cl.SetKernelArg(currentKernel, 3, SimpleImage.intPtrSize, tempImage.ImageHeight);
                    CheckErr(error, "Cl.SetkernelArgs");

                    Event clevent;

                    // Enqueueing the input image
                    IntPtr[] originPtr = new IntPtr[] { (IntPtr)0, (IntPtr)0, (IntPtr)0 };
                    IntPtr[] regionPtr = new IntPtr[] { (IntPtr)tempImage.ImageWidth, (IntPtr)tempImage.ImageHeight, (IntPtr)1 };
                    IntPtr[] workGroupSizePtr = new IntPtr[] { (IntPtr)tempImage.ImageWidth, (IntPtr)tempImage.ImageHeight, (IntPtr)1 };
                    error = Cl.EnqueueWriteImage(_CommandQueue, inputImage2DBuffer, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, tempImage.ByteArray, 0, null, out clevent);
                    CheckErr(error, "Cl.EnqueueWriteBuffer");

                    // Enqueueing the kernel
                    error = Cl.EnqueueNDRangeKernel(_CommandQueue, currentKernel, 2, null, workGroupSizePtr, null, 0, null, out clevent);
                    CheckErr(error, "Cl.EnqueueNDRangeKernel");

                    error = Cl.Finish(_CommandQueue);
                    CheckErr(error, "Cl.Finish");

                    // Enqueueing the output image
                    error = Cl.EnqueueReadImage(_CommandQueue, outputImage2DBuffer, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, outputByteArray, 0, null, out clevent);
                    CheckErr(error, "Cl.EnqueueReadImage");

                    //Releasing the image buffers
                    Cl.ReleaseMemObject(inputImage2DBuffer);
                    Cl.ReleaseMemObject(outputImage2DBuffer);

                    //inputByteArray = outputByteArray;
                    tempImage = new SimpleImage(tempImage, outputByteArray);
                    inputImage2DBuffer = (Mem)Cl.CreateImage2D(_Context, MemFlags.CopyHostPtr | MemFlags.ReadOnly, SimpleImage.clImageFormat, (IntPtr)tempImage.BitmapWidth, (IntPtr)tempImage.BitmapHeight, (IntPtr)0, tempImage.ByteArray, out error);

                Cl.ReleaseKernel(currentKernel);
                }
                Cl.ReleaseMemObject(inputImage2DBuffer);
                //Cl.ReleaseCommandQueue(commandQueue);

                return tempImage;
        }

        public SimpleImage[] ProcessPreBuiltKernelsOnTwo(SimpleImage left, SimpleImage right, string[] kernelNames)
        {
            ErrorCode error;

                SimpleImage leftTempImage = left;
                SimpleImage rightTempIMage = right;

                Mem leftInputImage2DBuffer = (Mem)Cl.CreateImage2D(_Context, MemFlags.CopyHostPtr | MemFlags.ReadOnly, SimpleImage.clImageFormat, (IntPtr)leftTempImage.BitmapWidth, (IntPtr)leftTempImage.BitmapHeight, (IntPtr)0, leftTempImage.ByteArray, out error);
                CheckErr(error, "Cl.CreateImage2D");
                Mem rightInputImage2DBuffer = (Mem)Cl.CreateImage2D(_Context, MemFlags.CopyHostPtr | MemFlags.ReadOnly, SimpleImage.clImageFormat, (IntPtr)rightTempIMage.BitmapWidth, (IntPtr)rightTempIMage.BitmapHeight, (IntPtr)0, rightTempIMage.ByteArray, out error);
                CheckErr(error, "Cl.CreateImage2D");

                //CommandQueue commandQueue = Cl.CreateCommandQueue(_Context, _Device, (CommandQueueProperties)0, out error);
                CheckErr(error, "Cl.CreateCommandQueue");
                byte[] leftOutputByteArray = null;
                byte[] rightOutputByteArray = null;

                for (int i = 0; i < kernelNames.Length; i++)
                {
                    // Output preparation
                    leftOutputByteArray = new byte[leftTempImage.ImageByteSize];
                    rightOutputByteArray = new byte[rightTempIMage.ImageByteSize];

                    Mem leftOutputImage2DBuffer = (Mem)Cl.CreateImage2D(_Context, MemFlags.CopyHostPtr | MemFlags.WriteOnly, SimpleImage.clImageFormat, (IntPtr)leftTempImage.ImageWidth, (IntPtr)leftTempImage.ImageHeight, (IntPtr)0, leftOutputByteArray, out error);
                    CheckErr(error, "Cl.CreateImage2D output");
                    Mem rightOutputImage2DBuffer = (Mem)Cl.CreateImage2D(_Context, MemFlags.CopyHostPtr | MemFlags.WriteOnly, SimpleImage.clImageFormat, (IntPtr)rightTempIMage.ImageWidth, (IntPtr)rightTempIMage.ImageHeight, (IntPtr)0, rightOutputByteArray, out error);
                    CheckErr(error, "Cl.CreateImage2D output");

                    Kernel currentKernel =Cl.CreateKernel(_Program, kernelNames[i], out error);
                    CheckErr(error, "Cl.CreateKernel");

                    // Kernel arguiment declaration
                    error  = Cl.SetKernelArg(currentKernel, 0, SimpleImage.intPtrSize, leftInputImage2DBuffer);
                    error |= Cl.SetKernelArg(currentKernel, 1, SimpleImage.intPtrSize, rightInputImage2DBuffer);
                    error |= Cl.SetKernelArg(currentKernel, 2, SimpleImage.intPtrSize, leftOutputImage2DBuffer);
                    error |= Cl.SetKernelArg(currentKernel, 3, SimpleImage.intPtrSize, rightOutputImage2DBuffer);
                    error |= Cl.SetKernelArg(currentKernel, 4, SimpleImage.intPtrSize, leftTempImage.ImageWidth);
                    error |= Cl.SetKernelArg(currentKernel, 5, SimpleImage.intPtrSize, leftTempImage.ImageHeight);
                    CheckErr(error, "Cl.SetkernelArgs");

                    Event clevent;

                    // Enqueueing the input image
                    IntPtr[] originPtr = new IntPtr[] { (IntPtr)0, (IntPtr)0, (IntPtr)0 };
                    IntPtr[] regionPtr = new IntPtr[] { (IntPtr)leftTempImage.ImageWidth, (IntPtr)leftTempImage.ImageHeight, (IntPtr)1 };
                    IntPtr[] workGroupSizePtr = new IntPtr[] { (IntPtr)leftTempImage.ImageWidth, (IntPtr)leftTempImage.ImageHeight, (IntPtr)1 };

                    error = Cl.EnqueueWriteImage(_CommandQueue, leftInputImage2DBuffer, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, leftTempImage.ByteArray, 0, null, out clevent);
                    CheckErr(error, "Cl.EnqueueWriteBuffer");
                    error = Cl.EnqueueWriteImage(_CommandQueue, rightInputImage2DBuffer, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, rightTempIMage.ByteArray, 0, null, out clevent);
                    CheckErr(error, "Cl.EnqueueWriteBuffer");

                    // Enqueueing the kernel
                    error = Cl.EnqueueNDRangeKernel(_CommandQueue, currentKernel, 2, null, workGroupSizePtr, null, 0, null, out clevent);
                    CheckErr(error, "Cl.EnqueueNDRangeKernel");

                    error = Cl.Finish(_CommandQueue);
                    CheckErr(error, "Cl.Finish");

                    // Enqueueing the output image
                    error = Cl.EnqueueReadImage(_CommandQueue, leftOutputImage2DBuffer, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, leftOutputByteArray, 0, null, out clevent);
                    CheckErr(error, "Cl.EnqueueReadImage");
                    error = Cl.EnqueueReadImage(_CommandQueue, rightOutputImage2DBuffer, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, rightOutputByteArray, 0, null, out clevent);
                    CheckErr(error, "Cl.EnqueueReadImage");

                    //Releasing the image buffers
                    Cl.ReleaseMemObject(leftInputImage2DBuffer);
                    Cl.ReleaseMemObject(rightInputImage2DBuffer);
                    Cl.ReleaseMemObject(leftOutputImage2DBuffer);
                    Cl.ReleaseMemObject(rightOutputImage2DBuffer);

                    //inputByteArray = outputByteArray;
                    leftTempImage = new SimpleImage(leftTempImage, leftOutputByteArray);
                    rightTempIMage = new SimpleImage(rightTempIMage, rightOutputByteArray);
                    leftInputImage2DBuffer = (Mem)Cl.CreateImage2D(_Context, MemFlags.CopyHostPtr | MemFlags.ReadOnly, SimpleImage.clImageFormat, (IntPtr)leftTempImage.BitmapWidth, (IntPtr)leftTempImage.BitmapHeight, (IntPtr)0, leftTempImage.ByteArray, out error);
                    rightInputImage2DBuffer = (Mem)Cl.CreateImage2D(_Context, MemFlags.CopyHostPtr | MemFlags.ReadOnly, SimpleImage.clImageFormat, (IntPtr)rightTempIMage.BitmapWidth, (IntPtr)rightTempIMage.BitmapHeight, (IntPtr)0, rightTempIMage.ByteArray, out error);
                Cl.ReleaseKernel(currentKernel);
                }
                Cl.ReleaseMemObject(leftInputImage2DBuffer);
                Cl.ReleaseMemObject(rightInputImage2DBuffer);
                //Cl.ReleaseCommandQueue(commandQueue);

                return new SimpleImage[] { leftTempImage, rightTempIMage };
        }
        #endregion

        #region downSampling
        public SimpleImage DownSample(SimpleImage inputImage)
        {
            ErrorCode error;

            KernelStrings kernelString = new KernelStrings("DownSample.cl", "downSample");
            string kernelSource = getKernelSource(kernelString);

            using (OpenCL.Net.Program program = Cl.CreateProgramWithSource(_Context, 1, new[] { kernelSource }, null, out error))
            {
                CheckErr(error, "Cl.CreateProgramWithSource");

                buildProgram(program);

                Kernel currentKernel;
                currentKernel = Cl.CreateKernel(program, kernelString.FunctionName, out error);
                CheckErr(error, "Cl.CreateKernel");

                Mem inputImage2DBuffer = (Mem)Cl.CreateImage2D(_Context, MemFlags.CopyHostPtr | MemFlags.ReadOnly, SimpleImage.clImageFormat, (IntPtr)inputImage.BitmapWidth, (IntPtr)inputImage.BitmapHeight, (IntPtr)0, inputImage.ByteArray, out error);

                CommandQueue commandQueue = Cl.CreateCommandQueue(_Context, _Device, (CommandQueueProperties)0, out error);
                CheckErr(error, "Cl.CreateCommandQueue");

                byte[] outputByteArray = null;

                // Output preparation
                outputByteArray = new byte[inputImage.ImageByteSize / 2];
                Mem outputImage2DBuffer = (Mem)Cl.CreateImage2D(_Context, MemFlags.CopyHostPtr | MemFlags.WriteOnly, SimpleImage.clImageFormat, (IntPtr)(inputImage.ImageWidth / 2), (IntPtr)(inputImage.ImageHeight / 2), (IntPtr)0, outputByteArray, out error);
                CheckErr(error, "Cl.CreateImage2D output");

                // Setting kernel arguments
                error = Cl.SetKernelArg(currentKernel, 0, SimpleImage.intPtrSize, inputImage2DBuffer);
                error |= Cl.SetKernelArg(currentKernel, 1, SimpleImage.intPtrSize, outputImage2DBuffer);
                CheckErr(error, "Cl.SetkernelArgs");

                Event clevent;

                // Enqueueing the input image
                IntPtr[] originPtr = new IntPtr[] { (IntPtr)0, (IntPtr)0, (IntPtr)0 };
                IntPtr[] regionPtr = new IntPtr[] { (IntPtr)inputImage.ImageWidth, (IntPtr)inputImage.ImageHeight, (IntPtr)1 };
                IntPtr[] workGroupSizePtr = new IntPtr[] { (IntPtr)(inputImage.ImageWidth / 2), (IntPtr)(inputImage.ImageHeight / 2), (IntPtr)1 };

                error = Cl.EnqueueWriteImage(commandQueue, inputImage2DBuffer, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, inputImage.ByteArray, 0, null, out clevent);
                CheckErr(error, "Cl.EnqueueWriteBuffer");

                // Enqueueing the kernel
                error = Cl.EnqueueNDRangeKernel(commandQueue, currentKernel, 2, null, workGroupSizePtr, null, 0, null, out clevent);
                CheckErr(error, "Cl.EnqueueNDRangeKernel");

                error = Cl.Finish(commandQueue);
                CheckErr(error, "Cl.Finish");

                // Enqueueing the output image
                regionPtr = new IntPtr[] { (IntPtr)(inputImage.ImageWidth / 2), (IntPtr)(inputImage.ImageHeight / 2), (IntPtr)1 };
                error = Cl.EnqueueReadImage(commandQueue, outputImage2DBuffer, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, outputByteArray, 0, null, out clevent);
                CheckErr(error, "Cl.EnqueueReadImage");

                //Releasing the image buffers
                Cl.ReleaseMemObject(inputImage2DBuffer);
                Cl.ReleaseMemObject(outputImage2DBuffer);
                Cl.ReleaseKernel(currentKernel);
                Cl.ReleaseCommandQueue(commandQueue);

                SimpleImage outputSimpleImage = new SimpleImage(inputImage, outputByteArray);
                outputSimpleImage.downSampleAttributes();

                return outputSimpleImage;
            }
        }

        public SimpleImage[] DownSampleTwo(SimpleImage left, SimpleImage right)
        {
            ErrorCode error;

            KernelStrings kernelString = new KernelStrings("DownSampleTwo.cl", "downSampleTwo");
            string kernelSource = getKernelSource(kernelString);

                Kernel currentKernel;
                currentKernel = Cl.CreateKernel(_Program, kernelString.FunctionName, out error);
                CheckErr(error, "Cl.CreateKernel");

                Mem leftInputImage2DBuffer = (Mem)Cl.CreateImage2D(_Context, MemFlags.CopyHostPtr | MemFlags.ReadOnly, SimpleImage.clImageFormat, (IntPtr)left.BitmapWidth, (IntPtr)left.BitmapHeight, (IntPtr)0, left.ByteArray, out error);
                CheckErr(error, "Cl.CreateImage2D");
                Mem rightInputImage2DBuffer = (Mem)Cl.CreateImage2D(_Context, MemFlags.CopyHostPtr | MemFlags.ReadOnly, SimpleImage.clImageFormat, (IntPtr)right.BitmapWidth, (IntPtr)right.BitmapHeight, (IntPtr)0, right.ByteArray, out error);
                CheckErr(error, "Cl.CreateImage2D");

                //CommandQueue commandQueue = Cl.CreateCommandQueue(_Context, _Device, (CommandQueueProperties)0, out error);
                CheckErr(error, "Cl.CreateCommandQueue");

                // Output preparation
                byte[] leftOutputByteArray = new byte[left.ImageByteSize / 2];
                Mem leftOutputImage2DBuffer = (Mem)Cl.CreateImage2D(_Context, MemFlags.CopyHostPtr | MemFlags.WriteOnly, SimpleImage.clImageFormat, (IntPtr)(left.ImageWidth / 2), (IntPtr)(left.ImageHeight / 2), (IntPtr)0, leftOutputByteArray, out error);
                CheckErr(error, "Cl.CreateImage2D output");
                byte[] rightOutputByteArray = new byte[right.ImageByteSize / 2];
                Mem rightOutputImage2DBuffer = (Mem)Cl.CreateImage2D(_Context, MemFlags.CopyHostPtr | MemFlags.WriteOnly, SimpleImage.clImageFormat, (IntPtr)(right.ImageWidth / 2), (IntPtr)(right.ImageHeight / 2), (IntPtr)0, rightOutputByteArray, out error);
                CheckErr(error, "Cl.CreateImage2D output");

                // Setting kernel arguments
                error  = Cl.SetKernelArg(currentKernel, 0, SimpleImage.intPtrSize, leftInputImage2DBuffer);
                error |= Cl.SetKernelArg(currentKernel, 1, SimpleImage.intPtrSize, rightInputImage2DBuffer);
                error |= Cl.SetKernelArg(currentKernel, 2, SimpleImage.intPtrSize, leftOutputImage2DBuffer);
                error |= Cl.SetKernelArg(currentKernel, 3, SimpleImage.intPtrSize, rightOutputImage2DBuffer);
                CheckErr(error, "Cl.SetkernelArgs");

                Event clevent;

                // Enqueueing the input image
                IntPtr[] originPtr = new IntPtr[] { (IntPtr)0, (IntPtr)0, (IntPtr)0 };
                IntPtr[] regionPtr = new IntPtr[] { (IntPtr)left.ImageWidth, (IntPtr)left.ImageHeight, (IntPtr)1 };
                IntPtr[] workGroupSizePtr = new IntPtr[] { (IntPtr)(left.ImageWidth / 2), (IntPtr)(left.ImageHeight / 2), (IntPtr)1 };

                error = Cl.EnqueueWriteImage(_CommandQueue, leftInputImage2DBuffer, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, left.ByteArray, 0, null, out clevent);
                CheckErr(error, "Cl.EnqueueWriteBuffer");
                error = Cl.EnqueueWriteImage(_CommandQueue, rightInputImage2DBuffer, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, right.ByteArray, 0, null, out clevent);
                CheckErr(error, "Cl.EnqueueWriteBuffer");

                // Enqueueing the kernel
                error = Cl.EnqueueNDRangeKernel(_CommandQueue, currentKernel, 2, null, workGroupSizePtr, null, 0, null, out clevent);
                CheckErr(error, "Cl.EnqueueNDRangeKernel");

                error = Cl.Finish(_CommandQueue);
                CheckErr(error, "Cl.Finish");

                // Enqueueing the output image
                regionPtr = new IntPtr[] { (IntPtr)(left.ImageWidth / 2), (IntPtr)(left.ImageHeight / 2), (IntPtr)1 };
                error = Cl.EnqueueReadImage(_CommandQueue, leftOutputImage2DBuffer, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, leftOutputByteArray, 0, null, out clevent);
                CheckErr(error, "Cl.EnqueueReadImage");
                error = Cl.EnqueueReadImage(_CommandQueue, rightOutputImage2DBuffer, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, rightOutputByteArray, 0, null, out clevent);
                CheckErr(error, "Cl.EnqueueReadImage");

                //Releasing the image buffers
                Cl.ReleaseMemObject(leftInputImage2DBuffer);
                Cl.ReleaseMemObject(rightInputImage2DBuffer);
                Cl.ReleaseMemObject(leftOutputImage2DBuffer);
                Cl.ReleaseMemObject(rightOutputImage2DBuffer);
                Cl.ReleaseKernel(currentKernel);
                //Cl.ReleaseCommandQueue(commandQueue);

                SimpleImage leftOutputSimpleImage = new SimpleImage(left, leftOutputByteArray);
                leftOutputSimpleImage.downSampleAttributes();
                SimpleImage rightOutputSimpleImage = new SimpleImage(right, rightOutputByteArray);
                rightOutputSimpleImage.downSampleAttributes();

                return new[] { leftOutputSimpleImage, rightOutputSimpleImage };
        }
        #endregion

        #region pyramids
        public SimpleImage[] CreatePyramid(SimpleImage input, int pyramidLevel = 4)
        {
            string[] monochromeKernelString = { "monochrome" };
            string[] gaussKernelString = { "gaussianBlur1" };

            SimpleImage greyScaleInput = ProcessPreBuiltKernelsOnOne(input, monochromeKernelString);
            SimpleImage[] resultPyramid = new SimpleImage[pyramidLevel];
            resultPyramid[0] = greyScaleInput;

            SimpleImage temp = greyScaleInput;
            for(int i = 1; i < pyramidLevel; i++)
            {
                temp = ProcessPreBuiltKernelsOnOne(temp, gaussKernelString);
                temp = this.DownSample(temp);
                resultPyramid[i] = temp;
            }

            return resultPyramid;
        }

        public SimpleImage[][] CreatePyramids(SimpleImage left, SimpleImage right, int pyramidLevel = 4)
        {
            string[] monochromeKernelName = new string[] {"monochromeTwo"};
            string[] gaussKernelName = new string[] {"gaussianBlur1Two"};

            SimpleImage[][] results = new SimpleImage[2][];
            SimpleImage[] greyScaleInputs = ProcessPreBuiltKernelsOnTwo(left, right, monochromeKernelName);

            results[0] = new SimpleImage[pyramidLevel];
            results[1] = new SimpleImage[pyramidLevel];
            results[0][0] = greyScaleInputs[0];
            results[1][0] = greyScaleInputs[1];


            SimpleImage[] temps = greyScaleInputs;

            for (int i = 1; i < pyramidLevel; i++)
            {
                temps = this.ProcessPreBuiltKernelsOnTwo(temps[0], temps[1], gaussKernelName);
                temps = this.DownSampleTwo(temps[0], temps[1]);

                results[0][i] = temps[0];
                results[1][i] = temps[1];
            }

            return results;
        }
        #endregion

        #region decorate & modify
        public SimpleImage Decorateflow(SimpleImage inputImage, FlowArray flow)
        {
            if (flow.Width*5 > inputImage.ImageWidth || flow.Height*5 > inputImage.ImageHeight)
            {
                throw new ArgumentException("The image cannot be decorated. The flow array is too large.");
            }
            ErrorCode error;
            Event writeUBufferFinishedEvent;
            Event writeVBufferFinishedEvent;
            Event ReadImageBufferFinishedEvent;
            Event NDRangeKernelFinished;
            SimpleImage outputImage = new SimpleImage(inputImage);

            KernelStrings kernelString = new KernelStrings("FlowDecorator.cl", "flowDecorator");
            string kernelSource = getKernelSource(kernelString);

                Kernel currentKernel;
                currentKernel = Cl.CreateKernel(_Program, kernelString.FunctionName, out error);
                CheckErr(error, "Cl.CreateKernel");

                Mem outputImage2DBuffer = (Mem)Cl.CreateImage2D(_Context, MemFlags.CopyHostPtr | MemFlags.ReadWrite, SimpleImage.clImageFormat, (IntPtr)outputImage.BitmapWidth, (IntPtr)outputImage.BitmapHeight, (IntPtr)0, outputImage.ByteArray, out error);
                CheckErr(error, "Cl.CreateImage2D");
                IMem<float> flowUMemObject = Cl.CreateBuffer<float>(_Context, MemFlags.ReadOnly, flow.Width * flow.Height*sizeof(float), out error);
                CheckErr(error, "Cl.CreateBuffer");
                IMem<float> flowVMemObject = Cl.CreateBuffer<float>(_Context, MemFlags.ReadOnly, flow.Width * flow.Height*sizeof(float), out error);
                CheckErr(error, "Cl.CreateBuffer");

                //CommandQueue commandQueue = Cl.CreateCommandQueue(_Context, _Device, (CommandQueueProperties)0, out error);
                CheckErr(error, "Cl.CreateCommandQueue");

                error = Cl.EnqueueWriteBuffer<float>(_CommandQueue, flowUMemObject, Bool.True, flow.Array[0], 0, null, out writeUBufferFinishedEvent);
                CheckErr(error, "Cl.EnqueueCommandQueue");
                error = Cl.EnqueueWriteBuffer<float>(_CommandQueue, flowVMemObject, Bool.True, flow.Array[1], 0, null, out writeVBufferFinishedEvent);
                CheckErr(error, "Cl.EnqueueCommandQueue");

                // Setting kernel arguments
                error  = Cl.SetKernelArg(currentKernel, 0, outputImage2DBuffer);
                error |= Cl.SetKernelArg(currentKernel, 1, flow.Width);
                error |= Cl.SetKernelArg(currentKernel, 2, flow.Height);
                error |= Cl.SetKernelArg<float>(currentKernel, 3, flowUMemObject);
                error |= Cl.SetKernelArg<float>(currentKernel, 4, flowVMemObject);
                CheckErr(error, "Cl.SetkernelArgs");

                // Enqueueing the input image
                IntPtr[] originPtr = new IntPtr[] { (IntPtr)0, (IntPtr)0, (IntPtr)0 };
                IntPtr[] regionPtr = new IntPtr[] { (IntPtr)outputImage.ImageWidth, (IntPtr)outputImage.ImageHeight, (IntPtr)1 };
                IntPtr[] workGroupSizePtr = new IntPtr[] { (IntPtr)(flow.Width*flow.Height)};

                // Enqueueing the kernel
                error = Cl.EnqueueNDRangeKernel(_CommandQueue, currentKernel, 1, null, workGroupSizePtr, null, 2, new Event[] { writeUBufferFinishedEvent, writeVBufferFinishedEvent }, out NDRangeKernelFinished);
                CheckErr(error, "Cl.EnqueueNDRangeKernel");

                // Enqueueing the output image
                error = Cl.EnqueueReadImage(_CommandQueue, outputImage2DBuffer, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, outputImage.ByteArray, 0, null, out ReadImageBufferFinishedEvent);
                CheckErr(error, "Cl.EnqueueReadImage");

                //Releasing the image buffers
                Cl.ReleaseMemObject(flowUMemObject);
                Cl.ReleaseMemObject(flowVMemObject);
                Cl.ReleaseMemObject(outputImage2DBuffer);
                Cl.ReleaseKernel(currentKernel);
                //Cl.ReleaseCommandQueue(commandQueue);

                return outputImage;
        }

        public SimpleImage decorateFlowColor(Bitmap inputImage, FlowArray flow, float level, float flowtreshold)
        {

            ErrorCode error;
            Event writeUBufferFinishedEvent;
            Event writeVBufferFinishedEvent;
            Event ReadImageBufferFinishedEvent;
            Event NDRangeKernelFinished;
            SimpleImage outputImage = new SimpleImage(inputImage);

            Kernel FlowDecKernel = Cl.CreateKernel(_Program, "flowDecoratorColor", out error);



            Mem outputImage2DBuffer = (Mem)Cl.CreateImage2D(_Context, MemFlags.CopyHostPtr | MemFlags.ReadWrite, SimpleImage.clImageFormat, (IntPtr)outputImage.BitmapWidth, (IntPtr)outputImage.BitmapHeight, (IntPtr)0, outputImage.ByteArray, out error);

            IMem<float> flowUMemObject = Cl.CreateBuffer<float>(_Context, MemFlags.ReadOnly, flow.Width * flow.Height * sizeof(float), out error);

            IMem<float> flowVMemObject = Cl.CreateBuffer<float>(_Context, MemFlags.ReadOnly, flow.Width * flow.Height * sizeof(float), out error);


            //CommandQueue commandQueue = Cl.CreateCommandQueue(_Context, _Device, (CommandQueueProperties)0, out error);

            error = Cl.EnqueueWriteBuffer<float>(_CommandQueue, flowUMemObject, Bool.True, flow.Array[0], 0, null, out writeUBufferFinishedEvent);

            error = Cl.EnqueueWriteBuffer<float>(_CommandQueue, flowVMemObject, Bool.True, flow.Array[1], 0, null, out writeVBufferFinishedEvent);


            // Setting kernel arguments
            error = Cl.SetKernelArg(FlowDecKernel, 0, outputImage2DBuffer);
            error |= Cl.SetKernelArg(FlowDecKernel, 1, flow.Width);
            error |= Cl.SetKernelArg(FlowDecKernel, 2, flow.Height);
            error |= Cl.SetKernelArg<float>(FlowDecKernel, 3, flowUMemObject);
            error |= Cl.SetKernelArg<float>(FlowDecKernel, 4, flowVMemObject);
            error |= Cl.SetKernelArg<float>(FlowDecKernel, 5, level);
            error |= Cl.SetKernelArg<float>(FlowDecKernel, 6, flowtreshold);

            // Enqueueing the input image
            IntPtr[] originPtr = new IntPtr[] { (IntPtr)0, (IntPtr)0, (IntPtr)0 };
            IntPtr[] regionPtr = new IntPtr[] { (IntPtr)outputImage.ImageWidth, (IntPtr)outputImage.ImageHeight, (IntPtr)1 };
            IntPtr[] workGroupSizePtr = new IntPtr[] { (IntPtr)(flow.Width * flow.Height) };

            // Enqueueing the kernel
            error = Cl.EnqueueNDRangeKernel(_CommandQueue, FlowDecKernel, 1, null, workGroupSizePtr, null, 2, new Event[] { writeUBufferFinishedEvent, writeVBufferFinishedEvent }, out NDRangeKernelFinished);

            // Enqueueing the output image
            error = Cl.EnqueueReadImage(_CommandQueue, outputImage2DBuffer, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, outputImage.ByteArray, 0, null, out ReadImageBufferFinishedEvent);


            //Releasing the image buffers
            Cl.ReleaseMemObject(flowUMemObject);
            Cl.ReleaseMemObject(flowVMemObject);
            Cl.ReleaseMemObject(outputImage2DBuffer);
            Cl.ReleaseKernel(FlowDecKernel);
            //Cl.ReleaseCommandQueue(commandQueue);

            return outputImage;
        }
        public SimpleImage pushImagewithFlow(SimpleImage input, FlowArray Flow)
        {
            ErrorCode error;
            Event writeUBufferFinishedEvent;
            Event writeVBufferFinishedEvent;
            SimpleImage tempimage = new SimpleImage();
            SimpleImage tempImage = input;

            Mem inputImage2DBuffer = (Mem)Cl.CreateImage2D(_Context, MemFlags.CopyHostPtr | MemFlags.ReadOnly, SimpleImage.clImageFormat, (IntPtr)tempImage.BitmapWidth, (IntPtr)tempImage.BitmapHeight, (IntPtr)0, tempImage.ByteArray, out error);

            IMem<float> flowUMemObject = Cl.CreateBuffer<float>(_Context, MemFlags.ReadOnly, Flow.Width * Flow.Height * sizeof(float), out error);

            IMem<float> flowVMemObject = Cl.CreateBuffer<float>(_Context, MemFlags.ReadOnly, Flow.Width * Flow.Height * sizeof(float), out error);

            byte[] outputByteArray = null;

            // Output preparation
            outputByteArray = new byte[tempImage.ImageByteSize];
            Mem outputImage2DBuffer = (Mem)Cl.CreateImage2D(_Context, MemFlags.CopyHostPtr | MemFlags.WriteOnly, SimpleImage.clImageFormat, (IntPtr)tempImage.ImageWidth, (IntPtr)tempImage.ImageHeight, (IntPtr)0, outputByteArray, out error);

            Kernel currentKernel = Cl.CreateKernel(_Program, "pushImgFlow", out error);

            // Kernel arguiment declaration
            error = Cl.SetKernelArg(currentKernel, 0, SimpleImage.intPtrSize, inputImage2DBuffer);
            error |= Cl.SetKernelArg(currentKernel, 1, SimpleImage.intPtrSize, outputImage2DBuffer);
            error |= Cl.SetKernelArg<float>(currentKernel, 2, flowUMemObject);
            error |= Cl.SetKernelArg<float>(currentKernel, 3, flowVMemObject);
            error |= Cl.SetKernelArg(currentKernel, 4, SimpleImage.intPtrSize, tempImage.ImageWidth);
            error |= Cl.SetKernelArg(currentKernel, 5, SimpleImage.intPtrSize, tempImage.ImageHeight);

            Event clevent;

            // Enqueueing the input image
            IntPtr[] originPtr = new IntPtr[] { (IntPtr)0, (IntPtr)0, (IntPtr)0 };
            IntPtr[] regionPtr = new IntPtr[] { (IntPtr)tempImage.ImageWidth, (IntPtr)tempImage.ImageHeight, (IntPtr)1 };
            IntPtr[] workGroupSizePtr = new IntPtr[] { (IntPtr)Flow.Width, (IntPtr)Flow.Height, (IntPtr)1 };
            error = Cl.EnqueueWriteImage(_CommandQueue, inputImage2DBuffer, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, tempImage.ByteArray, 0, null, out clevent);

            error = Cl.EnqueueWriteBuffer<float>(_CommandQueue, flowUMemObject, Bool.True, Flow.Array[0], 0, null, out writeUBufferFinishedEvent);

            error = Cl.EnqueueWriteBuffer<float>(_CommandQueue, flowVMemObject, Bool.True, Flow.Array[1], 0, null, out writeVBufferFinishedEvent);

            // Enqueueing the kernel
            error = Cl.EnqueueNDRangeKernel(_CommandQueue, currentKernel, 2, null, workGroupSizePtr, null, 0, null, out clevent);

            error = Cl.Finish(_CommandQueue);


            // Enqueueing the output image
            error = Cl.EnqueueReadImage(_CommandQueue, outputImage2DBuffer, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, outputByteArray, 0, null, out clevent);


            //Releasing the image buffers


            //inputByteArray = outputByteArray;
            tempImage = new SimpleImage(tempImage, outputByteArray);

            Cl.ReleaseKernel(currentKernel);
            Cl.ReleaseMemObject(inputImage2DBuffer);
            Cl.ReleaseMemObject(outputImage2DBuffer);
            Cl.ReleaseMemObject(flowUMemObject);
            Cl.ReleaseMemObject(flowVMemObject);
            //Cl.ReleaseCommandQueue(commandQueue);

            return tempImage;

        }
        public SimpleImage ModifyImageWithFlow(SimpleImage originalImage, SimpleImage rightImage, FlowArray flow)
        {
            ErrorCode error;
            Event clEvent;

            KernelStrings modifyKernelStrings = new KernelStrings("ModifyFlowImage.cl","modifyFlowImage");
            string modifyKernelSource = getKernelSource(modifyKernelStrings);

                Kernel modifierKernel = Cl.CreateKernel(_Program, modifyKernelStrings.FunctionName, out error);
                CheckErr(error, "Cl.CreateKernel");

                byte[] outputByteArray = new byte[originalImage.ByteArray.Length];
                for (int i = 0; i < originalImage.ByteArray.Length; i++)
                {
                    outputByteArray[i] = 0;
                    if (rightImage != null) {
                        outputByteArray[i] = rightImage.ByteArray[i];
                    }
                }

                SimpleImage outputImage = new SimpleImage(originalImage, outputByteArray);

                //CommandQueue commandQueue = Cl.CreateCommandQueue(_Context, _Device, CommandQueueProperties.None, out error);
                CheckErr(error, "Cl.CreateCommandQueue");

                Mem inputImageMemObject = (Mem)Cl.CreateImage2D(_Context, MemFlags.ReadOnly | MemFlags.CopyHostPtr, SimpleImage.clImageFormat, (IntPtr)originalImage.ImageWidth, (IntPtr)originalImage.ImageHeight, (IntPtr)0, originalImage.ByteArray, out error);
                CheckErr(error, "Cl.CreateImage2D");
                Mem outputImageMemObject = (Mem)Cl.CreateImage2D(_Context, MemFlags.WriteOnly | MemFlags.CopyHostPtr, SimpleImage.clImageFormat, (IntPtr)outputImage.ImageWidth, (IntPtr)outputImage.ImageHeight, (IntPtr)0, outputImage.ByteArray, out error);
                CheckErr(error, "Cl.CreateImage2D");
                IMem<float> flowUMemObject = Cl.CreateBuffer<float>(_Context, MemFlags.ReadOnly, flow.Height * flow.Width * sizeof(float), out error);
                CheckErr(error, "Cl.CreateBuffer");
                IMem<float> flowVMemObject = Cl.CreateBuffer<float>(_Context, MemFlags.ReadOnly, flow.Height * flow.Width * sizeof(float), out error);
                CheckErr(error, "Cl.CreateBuffer");

                error  = Cl.SetKernelArg(modifierKernel, 0, inputImageMemObject);
                error |= Cl.SetKernelArg(modifierKernel, 1, outputImageMemObject);
                error |= Cl.SetKernelArg<float>(modifierKernel, 2, flowUMemObject);
                error |= Cl.SetKernelArg<float>(modifierKernel, 3, flowVMemObject);
                error |= Cl.SetKernelArg(modifierKernel, 4, flow.Width);
                error |= Cl.SetKernelArg(modifierKernel, 5, flow.Height);
                CheckErr(error, "Cl.SetKernelArg");

                error = Cl.EnqueueWriteBuffer<float>(_CommandQueue, flowUMemObject, Bool.True, flow.Array[0], 0, null, out clEvent);
                CheckErr(error, "Cl.EnqueueWriteBuffer");
                error = Cl.EnqueueWriteBuffer<float>(_CommandQueue, flowVMemObject, Bool.True, flow.Array[1], 0, null, out clEvent);
                CheckErr(error, "Cl.EnqueueWriteBuffer");

                IntPtr[] origin = new IntPtr[] {(IntPtr)0, (IntPtr)0, (IntPtr)0 };
                IntPtr[] region = new IntPtr[] {(IntPtr)originalImage.ImageWidth, (IntPtr)originalImage.ImageHeight, (IntPtr)1 };

                error = Cl.EnqueueWriteImage(_CommandQueue, inputImageMemObject, Bool.True, origin, region, (IntPtr)0, (IntPtr)0, originalImage.ByteArray, 0, null, out clEvent);
                CheckErr(error, "Cl.EnqueueWriteImage");

                IntPtr[] globalWorkSize = new IntPtr[] {(IntPtr)(flow.Width*flow.Height) };
                error = Cl.EnqueueNDRangeKernel(_CommandQueue, modifierKernel, 1, null, globalWorkSize, null, 0, null, out clEvent);
                CheckErr(error, "Cl.EnqueueNDRangeKernel");

                Cl.Finish(_CommandQueue);
                CheckErr(error, "Cl.Finish");

                error = Cl.EnqueueReadImage(_CommandQueue, outputImageMemObject, Bool.True, origin, region, (IntPtr)0, (IntPtr)0, outputImage.ByteArray, 0, null, out clEvent);
                CheckErr(error, "Cl.EnqueueReadImage");

                Cl.ReleaseMemObject(inputImageMemObject);
                Cl.ReleaseMemObject(outputImageMemObject);
                Cl.ReleaseMemObject(flowUMemObject);
                Cl.ReleaseMemObject(flowVMemObject);
                Cl.ReleaseKernel(modifierKernel);
                //Cl.ReleaseCommandQueue(commandQueue);

                return outputImage;
            //}
        }
        #endregion

        #region flowCalculations
        public FlowArray CalculateFlow(SimpleImage left, SimpleImage right, float noiseReduction = 1.0f, string kernelFunctionName = "flow")
        {
            FlowArray resultFlowArray = null;

            try
            {
                if (left.ImageHeight != right.ImageHeight || left.ImageWidth != right.ImageWidth)
                {
                    throw (new ArgumentException("The size of the left and right images are not the same!"));
                }

                ErrorCode error;

                resultFlowArray = new FlowArray();
                resultFlowArray.Array = new float[2][];
                resultFlowArray.Width = left.ImageWidth / 5;
                resultFlowArray.Height = left.ImageHeight / 5;
                resultFlowArray.Array[0] = new float[resultFlowArray.Width * resultFlowArray.Height];
                resultFlowArray.Array[1] = new float[resultFlowArray.Width * resultFlowArray.Height];


                    Kernel flowKernel = Cl.CreateKernel(_Program, kernelFunctionName, out error);

                    Mem leftImageMemObject = (Mem)Cl.CreateImage2D(_Context, MemFlags.ReadOnly | MemFlags.CopyHostPtr, SimpleImage.clImageFormat, (IntPtr)left.ImageWidth, (IntPtr)left.ImageHeight, (IntPtr)0, left.ByteArray, out error);
                    CheckErr(error, "Cl.CreateImage2D");
                    Mem rightImageMemObject = (Mem)Cl.CreateImage2D(_Context, MemFlags.ReadOnly | MemFlags.CopyHostPtr, SimpleImage.clImageFormat, (IntPtr)right.ImageWidth, (IntPtr)right.ImageHeight, (IntPtr)0, right.ByteArray, out error);
                    CheckErr(error, "Cl.CreateImage2D");
                    IMem<float> uTranslationMemObject = Cl.CreateBuffer<float>(_Context, MemFlags.WriteOnly, resultFlowArray.Width * resultFlowArray.Height * sizeof(float), out error);
                    CheckErr(error, "Cl.CreateBuffer");
                    IMem<float> vTranslationMemObject = Cl.CreateBuffer<float>(_Context, MemFlags.WriteOnly, resultFlowArray.Width * resultFlowArray.Height * sizeof(float), out error);
                    CheckErr(error, "Cl.CreateBuffer");

                    error = Cl.SetKernelArg(flowKernel, 0, leftImageMemObject);
                    error |= Cl.SetKernelArg(flowKernel, 1, rightImageMemObject);
                    error |= Cl.SetKernelArg<float>(flowKernel, 2, uTranslationMemObject);
                    error |= Cl.SetKernelArg<float>(flowKernel, 3, vTranslationMemObject);
                    error |= Cl.SetKernelArg(flowKernel, 4, resultFlowArray.Width);
                    error |= Cl.SetKernelArg(flowKernel, 5, resultFlowArray.Height);
                    error |= Cl.SetKernelArg(flowKernel, 6, noiseReduction);
                    CheckErr(error, "Cl.SetKernelArg");

                    Event clEvent;

                    IntPtr[] originPtr = new IntPtr[] { (IntPtr)0, (IntPtr)0, (IntPtr)0 };
                    IntPtr[] regionPtr = new IntPtr[] { (IntPtr)left.ImageWidth, (IntPtr)left.ImageHeight, (IntPtr)1 };
                    IntPtr[] workGroupSizePtr = new IntPtr[] { (IntPtr)(resultFlowArray.Height * resultFlowArray.Width) };

                    error = Cl.EnqueueWriteImage(_CommandQueue, leftImageMemObject, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, left.ByteArray, 0, null, out clEvent);
                    CheckErr(error, "Cl.EnqueueWriteBuffer");
                    error = Cl.EnqueueWriteImage(_CommandQueue, rightImageMemObject, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, right.ByteArray, 0, null, out clEvent);
                    CheckErr(error, "Cl.EnqueueWriteBuffer");


                    // Enqueueing the kernel
                    error = Cl.EnqueueNDRangeKernel(_CommandQueue, flowKernel, 1, null, workGroupSizePtr, null, 0, null, out clEvent);
                    CheckErr(error, "Cl.EnqueueNDRangeKernel");
                    error = Cl.Finish(_CommandQueue);
                    CheckErr(error, "Cl.Finish");

                    Cl.EnqueueReadBuffer<float>(_CommandQueue, uTranslationMemObject, Bool.True, 0, (resultFlowArray.Width * resultFlowArray.Height), resultFlowArray.Array[0], 0, null, out clEvent);
                    error = Cl.Finish(_CommandQueue);
                    CheckErr(error, "Cl.Finish");

                    Cl.EnqueueReadBuffer<float>(_CommandQueue, vTranslationMemObject, Bool.True, 0, (resultFlowArray.Width * resultFlowArray.Height), resultFlowArray.Array[1], 0, null, out clEvent);
                    error = Cl.Finish(_CommandQueue);
                    CheckErr(error, "Cl.EnqueueReadBuffer");

                    Cl.ReleaseMemObject(leftImageMemObject);
                    Cl.ReleaseMemObject(rightImageMemObject);
                    Cl.ReleaseMemObject(uTranslationMemObject);
                    Cl.ReleaseMemObject(vTranslationMemObject);
                    Cl.ReleaseCommandQueue(_CommandQueue);
                    Cl.ReleaseKernel(flowKernel);
            }
            catch (Exception e)
            {
                throw (new Exception("Flow calculation error", e));
            }
            return resultFlowArray;
        }

        public FlowArray CalculatePyramidalLK(SimpleImage left, SimpleImage right, int pyramidLevel = 5, int iterationPerLevel = 3, float noiseReduction = 10000000.0f)
        {
            SimpleImage[][] pyramids = CreatePyramids(left, right, pyramidLevel);
            SimpleImage[] leftPyramid = pyramids[0];
            SimpleImage[] rightPyramid = pyramids[1];

            FlowArray[] flows = new FlowArray[pyramidLevel];
            flows[pyramidLevel-1] = CalculateFlow(leftPyramid[pyramidLevel-1], rightPyramid[pyramidLevel-1], noiseReduction);

            SimpleImage modifiedImage;
            FlowArray correction;

            for (int i = 1; i < pyramidLevel; i++) {

                FlowArray expanded = FlowArray.Expand(flows[pyramidLevel - i], leftPyramid[pyramidLevel - (i + 1)].ImageWidth/5, leftPyramid[pyramidLevel - (i + 1)].ImageHeight/5);
                flows[pyramidLevel - (i + 1)] = expanded;

                for (int j = 0; j < iterationPerLevel; j++)
                {
                    modifiedImage = ModifyImageWithFlow(leftPyramid[pyramidLevel - (i + 1)], rightPyramid[pyramidLevel - (i + 1)], expanded);
                    correction = CalculateFlow(modifiedImage, rightPyramid[pyramidLevel - (i + 1)], noiseReduction);

                    flows[pyramidLevel - (i + 1)] = flows[pyramidLevel - (i + 1)] + correction;
                }
            }

            return flows[0];
        }

        public FlowArray CalculateAdvancedFlow(SimpleImage left, SimpleImage right, FlowArray inputFlow, int isLastCalc, float correctionalMultiplier = 1.0f, string kernelFunctionName = "advancedFlow")
        {
            FlowArray resultFlowArray = null;

            try
            {
                if (left.ImageHeight != right.ImageHeight || left.ImageWidth != right.ImageWidth)
                {
                    throw (new ArgumentException("The size of the left and right images are not the same!"));
                }

                ErrorCode error;

                FlowArray inFlow = inputFlow;

                resultFlowArray = new FlowArray();
                resultFlowArray.Array = new float[2][];
                if (inputFlow == null)
                {
                    inFlow = new FlowArray();
                    inFlow.Width = left.ImageWidth / 5;
                    inFlow.Height = left.ImageHeight / 5;
                    inFlow.Array = new float[2][];
                    inFlow.Array[0] = new float[inFlow.Width * inFlow.Height];
                    inFlow.Array[1] = new float[inFlow.Width * inFlow.Height];

                            inFlow.Array[0][inFlow.Width] = 1.0f;
                            inFlow.Array[1][inFlow.Width] = 1.0f;
                }
                if (isLastCalc == 0)
                {
                    resultFlowArray.Width = left.BitmapWidth*2 / 5; //inFlow.width*2;
                    resultFlowArray.Height = left.BitmapHeight*2 / 5; //inFlow.height*2;
                }
                else
                {
                    resultFlowArray.Width = inFlow.Width;
                    resultFlowArray.Height = inFlow.Height;
                }
                resultFlowArray.Array[0] = new float[resultFlowArray.Width * resultFlowArray.Height];
                resultFlowArray.Array[1] = new float[resultFlowArray.Width * resultFlowArray.Height];

                Kernel flowKernel = Cl.CreateKernel(_Program, kernelFunctionName, out error);

                Mem leftImageMemObject = (Mem)Cl.CreateImage2D(_Context, MemFlags.ReadOnly | MemFlags.CopyHostPtr, SimpleImage.clImageFormat, (IntPtr)left.ImageWidth, (IntPtr)left.ImageHeight, (IntPtr)0, left.ByteArray, out error);
                CheckErr(error, "Cl.CreateImage2D");
                Mem rightImageMemObject = (Mem)Cl.CreateImage2D(_Context, MemFlags.ReadOnly | MemFlags.CopyHostPtr, SimpleImage.clImageFormat, (IntPtr)right.ImageWidth, (IntPtr)right.ImageHeight, (IntPtr)0, right.ByteArray, out error);
                CheckErr(error, "Cl.CreateImage2D");
                IMem<float> uInputFlowMemObject = Cl.CreateBuffer<float>(_Context, MemFlags.ReadOnly, inFlow.Width * inFlow.Height * sizeof(float), out error);
                CheckErr(error, "Cl.CreateBuffer");
                IMem<float> vInputFlowMemObject = Cl.CreateBuffer<float>(_Context, MemFlags.ReadOnly, inFlow.Width * inFlow.Height * sizeof(float), out error);
                CheckErr(error, "Cl.CreateBuffer");
                IMem<float> uOutputFlowMemObject = Cl.CreateBuffer<float>(_Context, MemFlags.WriteOnly, resultFlowArray.Width * resultFlowArray.Height * sizeof(float), out error);
                CheckErr(error, "Cl.CreateBuffer");
                IMem<float> vOutputlowMemObject = Cl.CreateBuffer<float>(_Context, MemFlags.WriteOnly, resultFlowArray.Width * resultFlowArray.Height * sizeof(float), out error);
                CheckErr(error, "Cl.CreateBuffer");

                CheckErr(error, "Cl.CreateCommandQueue");

                error = Cl.SetKernelArg(flowKernel, 0, leftImageMemObject);
                error |= Cl.SetKernelArg(flowKernel, 1, rightImageMemObject);
                error |= Cl.SetKernelArg<float>(flowKernel, 2, uInputFlowMemObject);
                error |= Cl.SetKernelArg<float>(flowKernel, 3, vInputFlowMemObject);
                error |= Cl.SetKernelArg<float>(flowKernel, 4, uOutputFlowMemObject);
                error |= Cl.SetKernelArg<float>(flowKernel, 5, vOutputlowMemObject);
                error |= Cl.SetKernelArg(flowKernel, 6, inFlow.Width);
                error |= Cl.SetKernelArg(flowKernel, 7, inFlow.Height);
                error |= Cl.SetKernelArg(flowKernel, 8, correctionalMultiplier);
                error |= Cl.SetKernelArg(flowKernel, 9, isLastCalc);
                CheckErr(error, "Cl.SetKernelArg");

                Event clEvent;

                IntPtr[] originPtr = new IntPtr[] { (IntPtr)0, (IntPtr)0, (IntPtr)0 };
                IntPtr[] regionPtr = new IntPtr[] { (IntPtr)left.ImageWidth, (IntPtr)left.ImageHeight, (IntPtr)1 };
                IntPtr[] workGroupSizePtr = new IntPtr[] { (IntPtr)(inFlow.Height * inFlow.Width) };

                error = Cl.EnqueueWriteImage(_CommandQueue, leftImageMemObject, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, left.ByteArray, 0, null, out clEvent);
                CheckErr(error, "Cl.EnqueueWriteBuffer");
                error = Cl.EnqueueWriteImage(_CommandQueue, rightImageMemObject, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, right.ByteArray, 0, null, out clEvent);
                CheckErr(error, "Cl.EnqueueWriteBuffer");

                error = Cl.EnqueueWriteBuffer<float>(_CommandQueue, uInputFlowMemObject, Bool.True, inFlow.Array[0], 0, null, out clEvent);
                CheckErr(error, "Cl.EnqueueWriteBuffer");
                error = Cl.EnqueueWriteBuffer<float>(_CommandQueue, vInputFlowMemObject, Bool.True, inFlow.Array[1], 0, null, out clEvent);
                CheckErr(error, "Cl.EnqueueWriteBuffer");

                // Enqueueing the kernel
                error = Cl.EnqueueNDRangeKernel(_CommandQueue, flowKernel, 1, null, workGroupSizePtr, null, 0, null, out clEvent);
                CheckErr(error, "Cl.EnqueueNDRangeKernel");
                error = Cl.Finish(_CommandQueue);
                CheckErr(error, "Cl.Finish");

                Cl.EnqueueReadBuffer<float>(_CommandQueue, uOutputFlowMemObject, Bool.True, 0, (resultFlowArray.Width * resultFlowArray.Height), resultFlowArray.Array[0], 0, null, out clEvent);
                error = Cl.Finish(_CommandQueue);
                CheckErr(error, "Cl.Finish");
                Cl.EnqueueReadBuffer<float>(_CommandQueue, vOutputlowMemObject, Bool.True, 0, (resultFlowArray.Width * resultFlowArray.Height), resultFlowArray.Array[1], 0, null, out clEvent);
                error = Cl.Finish(_CommandQueue);
                CheckErr(error, "Cl.EnqueueReadBuffer");

                Cl.ReleaseMemObject(leftImageMemObject);
                Cl.ReleaseMemObject(rightImageMemObject);
                Cl.ReleaseMemObject(uInputFlowMemObject);
                Cl.ReleaseMemObject(vInputFlowMemObject);
                Cl.ReleaseMemObject(uOutputFlowMemObject);
                Cl.ReleaseMemObject(vOutputlowMemObject);
                Cl.ReleaseKernel(flowKernel);
            }
            catch (Exception e)
            {
                throw (new Exception("Flow calculation error", e));
            }
            return resultFlowArray;
        }

        public FlowArray CalculateAdvancedPyramidalLK(SimpleImage left, SimpleImage right, int pyramidLevel = 5, float noiseReduction = 100000.0f)
        {
            FlowArray result = null;

            SimpleImage[][] pyramids = CreatePyramids(left, right, pyramidLevel);
            SimpleImage[] leftPyramid = pyramids[0];
            SimpleImage[] rightPyramid = pyramids[1];
            SimpleImage nextModified = leftPyramid[pyramidLevel - 1];

            for (int i = 1; i < pyramidLevel; i++)
            {
                result = CalculateAdvancedFlow(leftPyramid[pyramidLevel - i] , rightPyramid[pyramidLevel - i], result, 0, noiseReduction);
            }

            result = CalculateAdvancedFlow(leftPyramid[0], rightPyramid[0], result, 1, noiseReduction);

            return result;
        }


        public float[] calcFlowDist(FlowArray inputFlow1, FlowArray inputFlow2)
        {
            ErrorCode error;


            float[] output = new float[inputFlow1.Width * inputFlow1.Height];

            IMem<float> u1InputFlowMemObject = Cl.CreateBuffer<float>(_Context, MemFlags.ReadOnly, inputFlow1.Width * inputFlow1.Height * sizeof(float), out error);

            IMem<float> v1InputFlowMemObject = Cl.CreateBuffer<float>(_Context, MemFlags.ReadOnly, inputFlow1.Width * inputFlow1.Height * sizeof(float), out error);


            IMem<float> u2InputFlowMemObject = Cl.CreateBuffer<float>(_Context, MemFlags.ReadOnly, inputFlow2.Width * inputFlow2.Height * sizeof(float), out error);

            IMem<float> v2InputFlowMemObject = Cl.CreateBuffer<float>(_Context, MemFlags.ReadOnly, inputFlow2.Width * inputFlow2.Height * sizeof(float), out error);

            IMem<float> uOutputFlowMemObject = Cl.CreateBuffer<float>(_Context, MemFlags.WriteOnly, inputFlow1.Width * inputFlow1.Height * sizeof(float), out error);




            Kernel _Kernel = Cl.CreateKernel(_Program, "calcFlowError", out error);

            error |= Cl.SetKernelArg<float>(_Kernel, 0, u1InputFlowMemObject);
            error |= Cl.SetKernelArg<float>(_Kernel, 1, v1InputFlowMemObject);
            error |= Cl.SetKernelArg<float>(_Kernel, 2, u2InputFlowMemObject);
            error |= Cl.SetKernelArg<float>(_Kernel, 3, v2InputFlowMemObject);
            error |= Cl.SetKernelArg<float>(_Kernel, 4, uOutputFlowMemObject);
            error |= Cl.SetKernelArg(_Kernel, 5, inputFlow1.Width);
            error |= Cl.SetKernelArg(_Kernel, 6, inputFlow1.Height);

            Event _event;

            IntPtr[] workGroupSizePtr = new IntPtr[] { (IntPtr)(inputFlow1.Height * inputFlow1.Width) };

            error = Cl.EnqueueWriteBuffer<float>(_CommandQueue, u1InputFlowMemObject, Bool.True, inputFlow1.Array[0], 0, null, out _event);

            error = Cl.EnqueueWriteBuffer<float>(_CommandQueue, v1InputFlowMemObject, Bool.True, inputFlow1.Array[1], 0, null, out _event);

            error = Cl.EnqueueWriteBuffer<float>(_CommandQueue, u2InputFlowMemObject, Bool.True, inputFlow2.Array[0], 0, null, out _event);

            error = Cl.EnqueueWriteBuffer<float>(_CommandQueue, v2InputFlowMemObject, Bool.True, inputFlow2.Array[1], 0, null, out _event);

            error = Cl.EnqueueNDRangeKernel(_CommandQueue, _Kernel, 1, null, workGroupSizePtr, null, 0, null, out _event);
            error = Cl.Finish(_CommandQueue);


            Cl.EnqueueReadBuffer<float>(_CommandQueue, uOutputFlowMemObject, Bool.True, 0, (inputFlow1.Width * inputFlow1.Height), output, 0, null, out _event);
            error = Cl.Finish(_CommandQueue);


            Cl.ReleaseMemObject(u1InputFlowMemObject);
            Cl.ReleaseMemObject(v1InputFlowMemObject);
            Cl.ReleaseMemObject(u2InputFlowMemObject);
            Cl.ReleaseMemObject(v2InputFlowMemObject);
            Cl.ReleaseMemObject(uOutputFlowMemObject);
            Cl.ReleaseKernel(_Kernel);


            return output;
        }


        public float[] calcAngularError(FlowArray inputFlow1, FlowArray inputFlow2)
        {
            ErrorCode error;


            float[] output = new float[inputFlow1.Width * inputFlow1.Height];

            IMem<float> u1InputFlowMemObject = Cl.CreateBuffer<float>(_Context, MemFlags.ReadOnly, inputFlow1.Width * inputFlow1.Height * sizeof(float), out error);

            IMem<float> v1InputFlowMemObject = Cl.CreateBuffer<float>(_Context, MemFlags.ReadOnly, inputFlow1.Width * inputFlow1.Height * sizeof(float), out error);


            IMem<float> u2InputFlowMemObject = Cl.CreateBuffer<float>(_Context, MemFlags.ReadOnly, inputFlow2.Width * inputFlow2.Height * sizeof(float), out error);

            IMem<float> v2InputFlowMemObject = Cl.CreateBuffer<float>(_Context, MemFlags.ReadOnly, inputFlow2.Width * inputFlow2.Height * sizeof(float), out error);

            IMem<float> uOutputFlowMemObject = Cl.CreateBuffer<float>(_Context, MemFlags.WriteOnly, inputFlow1.Width * inputFlow1.Height * sizeof(float), out error);




            Kernel _Kernel = Cl.CreateKernel(_Program, "angularError", out error);

            error |= Cl.SetKernelArg<float>(_Kernel, 0, u1InputFlowMemObject);
            error |= Cl.SetKernelArg<float>(_Kernel, 1, v1InputFlowMemObject);
            error |= Cl.SetKernelArg<float>(_Kernel, 2, u2InputFlowMemObject);
            error |= Cl.SetKernelArg<float>(_Kernel, 3, v2InputFlowMemObject);
            error |= Cl.SetKernelArg<float>(_Kernel, 4, uOutputFlowMemObject);
            error |= Cl.SetKernelArg(_Kernel, 5, inputFlow1.Width);
            error |= Cl.SetKernelArg(_Kernel, 6, inputFlow1.Height);

            Event _event;

            IntPtr[] workGroupSizePtr = new IntPtr[] { (IntPtr)(inputFlow1.Height * inputFlow1.Width) };

            error = Cl.EnqueueWriteBuffer<float>(_CommandQueue, u1InputFlowMemObject, Bool.True, inputFlow1.Array[0], 0, null, out _event);

            error = Cl.EnqueueWriteBuffer<float>(_CommandQueue, v1InputFlowMemObject, Bool.True, inputFlow1.Array[1], 0, null, out _event);

            error = Cl.EnqueueWriteBuffer<float>(_CommandQueue, u2InputFlowMemObject, Bool.True, inputFlow2.Array[0], 0, null, out _event);

            error = Cl.EnqueueWriteBuffer<float>(_CommandQueue, v2InputFlowMemObject, Bool.True, inputFlow2.Array[1], 0, null, out _event);

            error = Cl.EnqueueNDRangeKernel(_CommandQueue, _Kernel, 1, null, workGroupSizePtr, null, 0, null, out _event);
            error = Cl.Finish(_CommandQueue);


            Cl.EnqueueReadBuffer<float>(_CommandQueue, uOutputFlowMemObject, Bool.True, 0, (inputFlow1.Width * inputFlow1.Height), output, 0, null, out _event);
            error = Cl.Finish(_CommandQueue);


            Cl.ReleaseMemObject(u1InputFlowMemObject);
            Cl.ReleaseMemObject(v1InputFlowMemObject);
            Cl.ReleaseMemObject(u2InputFlowMemObject);
            Cl.ReleaseMemObject(v2InputFlowMemObject);
            Cl.ReleaseMemObject(uOutputFlowMemObject);
            Cl.ReleaseKernel(_Kernel);


            return output;
        }


        
        public float[] calcFlowLength(FlowArray inputFlow)
        {
            ErrorCode error;

            float[] output = new float[inputFlow.Width * inputFlow.Height];
          

            IMem<float> uInputFlowMemObject = Cl.CreateBuffer<float>(_Context, MemFlags.ReadOnly, inputFlow.Width * inputFlow.Height * sizeof(float), out error);

            IMem<float> vInputFlowMemObject = Cl.CreateBuffer<float>(_Context, MemFlags.ReadOnly, inputFlow.Width * inputFlow.Height * sizeof(float), out error);

            IMem<float> uOutputFlowMemObject = Cl.CreateBuffer<float>(_Context, MemFlags.WriteOnly, inputFlow.Width * inputFlow.Height * sizeof(float), out error);




            Kernel _Kernel = Cl.CreateKernel(_Program, "flowlength", out error);

            error |= Cl.SetKernelArg<float>(_Kernel, 0, uInputFlowMemObject);
            error |= Cl.SetKernelArg<float>(_Kernel, 1, vInputFlowMemObject);
            error |= Cl.SetKernelArg<float>(_Kernel, 2, uOutputFlowMemObject);

            error |= Cl.SetKernelArg(_Kernel, 3, inputFlow.Width);
            error |= Cl.SetKernelArg(_Kernel, 4, inputFlow.Height);

            Event _event;

            IntPtr[] workGroupSizePtr = new IntPtr[] { (IntPtr)(inputFlow.Height * inputFlow.Width) };

            error = Cl.EnqueueWriteBuffer<float>(_CommandQueue, uInputFlowMemObject, Bool.True, inputFlow.Array[0], 0, null, out _event);

            error = Cl.EnqueueWriteBuffer<float>(_CommandQueue, vInputFlowMemObject, Bool.True, inputFlow.Array[1], 0, null, out _event);

            error = Cl.EnqueueNDRangeKernel(_CommandQueue, _Kernel, 1, null, workGroupSizePtr, null, 0, null, out _event);
            error = Cl.Finish(_CommandQueue);


            Cl.EnqueueReadBuffer<float>(_CommandQueue, uOutputFlowMemObject, Bool.True, 0, (inputFlow.Width * inputFlow.Height), output, 0, null, out _event);
            error = Cl.Finish(_CommandQueue);




            Cl.ReleaseMemObject(uInputFlowMemObject);
            Cl.ReleaseMemObject(vInputFlowMemObject);
            Cl.ReleaseMemObject(uOutputFlowMemObject);
            Cl.ReleaseKernel(_Kernel);


            return output;

        }
        #endregion

        #region evaluation
        public Bitmap Create48bitFlowImage(FlowArray flow, SimpleImage left)
        {
            int width = flow.Width * 6;
            int height = flow.Height;
            int fullSize = width*height;
            byte[] resultArray = new byte[fullSize];
            Bitmap res = new Bitmap("C:\\Users\\Ricsi\\Desktop\\BME\\2019_20_1\\FlowDatasets\\KITTI\\devkit\\matlab\\data\\realTest.png");


            for (int i = 0; i < height; i++)
            {
                for(int j = 0; j < width; j = j + 6)
                {
                        resultArray[i * width + j] = 1;
                        resultArray[i * width + j + 1] = 0;

                        UInt16 newV = (UInt16)(Math.Max(Math.Min(Math.Pow(2, 15) + flow.V[i * height + (j / 6)] * 64.0, Math.Pow(2,15)-1),0));
                        byte[] v = BitConverter.GetBytes(newV);
                        resultArray[i * width + j + 2] = v[0];
                        resultArray[i * width + j + 3] = v[1];

                        UInt16 newU = (UInt16)(Math.Max(Math.Min(Math.Pow(2, 15) + flow.U[i * height + (j / 6)] * 64.0, Math.Pow(2, 15) - 1), 0));
                        byte[] u = BitConverter.GetBytes(newU);
                        resultArray[i * width + j + 4] = u[0];
                        resultArray[i * width + j + 5] = u[1];
                }
            }

            SimpleImage resultSimpleImage = new SimpleImage();
            resultSimpleImage.ByteArray = resultArray;
            resultSimpleImage.ImageWidth = flow.Width;
            resultSimpleImage.ImageHeight = flow.Height;
            resultSimpleImage.ImageStride = width;

            return resultSimpleImage.FlowBitmap;
        }
        #endregion

        #endregion

        #region private methods

        private void setUp()
        {
            ErrorCode error;
            Platform[] platforms = Cl.GetPlatformIDs(out error);
            List<Device> devices = new List<Device>();

            CheckErr(error, "Cl.GetPlaftormIDs");

            foreach (Platform platform in platforms)
            {
                string platformName = Cl.GetPlatformInfo(platform, PlatformInfo.Name, out error).ToString();
                Console.WriteLine("Platform: " + platformName);
                CheckErr(error, "Cl.GetPlafromInfo");

                foreach (Device device in Cl.GetDeviceIDs(platform, DeviceType.All, out error))
                {
                    string deviceName = Cl.GetDeviceInfo(device, DeviceInfo.Name, out error).ToString();
                    Console.WriteLine("\tDevice: " + deviceName);
                    CheckErr(error, "Cl.GetDeviceInfo");
                    devices.Add(device);
                }
            }

            if (devices.Count <= 0)
            {
                Console.WriteLine("There are no available devices found in any of the platforms");
                return;
            }

            _Device = devices[0];
            Console.WriteLine("Chosen device is " + Cl.GetDeviceInfo(_Device, DeviceInfo.Name, out error));

            if ((Cl.GetDeviceInfo(_Device, DeviceInfo.ImageSupport, out error).CastTo<Bool>()) == Bool.False)
            {
                Console.WriteLine("There is no image support on the chosen dveice");
                return;
            }

            _Context = Cl.CreateContext(null, 1, new[] { _Device }, ContextNotify, IntPtr.Zero, out error);
            CheckErr(error, "Cl.CreateContext");

            string[] kernelSources = new string[_Kernels.Count];
            _Kernels.Values.CopyTo(kernelSources, 0);
            _Program = Cl.CreateProgramWithSource(_Context, (uint)_Kernels.Count, kernelSources, null, out error);
            CheckErr(error, "Cl.CreateProgramwithSource");
            buildProgram(_Program);
            _CommandQueue = Cl.CreateCommandQueue(_Context, _Device, (CommandQueueProperties)0, out error);

        }

        private void CheckErr(ErrorCode err, string name)
        {
            if (err != ErrorCode.Success)
            {
                Console.WriteLine("ERROR: " + name + " (" + err.ToString() + ")");
            }
        }

        private void ContextNotify(string errInfo, byte[] data, IntPtr cb, IntPtr userData)
        {
            Console.WriteLine("OpenCL Notification: " + errInfo);
        }

        private string getKernelSource(KernelStrings kernelStrings)
        {
            string result;

            if (!this._Kernels.TryGetValue(kernelStrings.FunctionName, out result))
            {
                result = this._FileHandler.getKernelSourceFromLibrary(kernelStrings.FileName);
                this._Kernels.Add(kernelStrings.FunctionName, result);
            }

            return result;
        }

        private void preLoadKernels(IKernelStrings[] kernelList)
        {
            for (int i = 0; i < kernelList.Length; i++)
            {
                if (!_Kernels.ContainsKey(kernelList[i].FunctionName))
                {
                }
                    _Kernels.Add(kernelList[i].FunctionName, this._FileHandler.getKernelSourceFromLibrary(kernelList[i].FileName));
            }
        }

        private void buildProgram(OpenCL.Net.Program program)
        {
            ErrorCode error;
            error = Cl.BuildProgram(program, 1, new[] { _Device }, string.Empty, null, IntPtr.Zero);
            CheckErr(error, "Cl.BuildProgram");

            if (Cl.GetProgramBuildInfo(program, _Device, ProgramBuildInfo.Status, out error).CastTo<BuildStatus>() != BuildStatus.Success)
            {
                CheckErr(error, "Cl.GetProgramBuildInfo");
                Console.WriteLine("Program build status is not succeeded");
                string info = Cl.GetProgramBuildInfo(program, _Device, ProgramBuildInfo.Log, out error).ToString();
                Console.WriteLine(info);
                throw (new Exception(info));
            }
        }
        #endregion
    }
}