using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using FlowProcessorAPI;
using OpenCL.Net;


namespace OpticalFlowMethods
{
    

    class LGFlow
    {


        private ErrorCode error;
        private Device[] devices;
        private Device device;

        private Context context;
        private OpenCL.Net.Program program;
        private CommandQueue commandQueue;
        private FileHandler File_Handler = new FileHandler();
        private string[] sources = new string[5];
        private CLProcessor cLProcessor = new CLProcessor();
        public float alpha, w;
        public int mode;
        public int kernelSize;
        public float sigma;
        public int pyramidLevel;
        public int iteration;
        public float flowInterval;
        public float threshold;
        public float epsilon;

        public LGFlow()
        {

            alpha = (float)15;
            mode = 1;
            w = (float)1.9;
            kernelSize = 3;
            sigma = 0;
            pyramidLevel = 5;
            iteration = 1;
            flowInterval = 2;
            threshold = (float)0.5;
            sources[0] = File_Handler.getKernelSourceFromLibrary("PushImgFlow.cl");
            sources[1] = File_Handler.getKernelSourceFromLibrary("LocalGlobalFlow1.cl");
            sources[2] = File_Handler.getKernelSourceFromLibrary("LocalGlobalFlow2.cl");
            sources[3] = File_Handler.getKernelSourceFromLibrary("LocalGlobalFlow_J.cl");
            sources[4] = File_Handler.getKernelSourceFromLibrary("LocalGlobalFlow3.cl");
            Platform[] platforms = Cl.GetPlatformIDs(out error);
            Console.WriteLine("Error code: " + error.ToString());
            devices = Cl.GetDeviceIDs(platforms[0], DeviceType.Gpu, out error);
            Console.WriteLine("Error code: " + error.ToString());
            device = devices[0];
            context = Cl.CreateContext(null, 1, devices, null, IntPtr.Zero, out error);
            Console.WriteLine("Error code: " + error.ToString());
            program = Cl.CreateProgramWithSource(context, 5, sources, null, out error);
            Console.WriteLine("Error code: " + error.ToString());
            Cl.BuildProgram(program, 1, devices, string.Empty, null, IntPtr.Zero);
            commandQueue = Cl.CreateCommandQueue(context, device, CommandQueueProperties.None, out error);
            Console.WriteLine("Error code: " + error.ToString());


        }


        private float[][] Gauss_J(float[][] J_in, int width, int height)

        {


            float[][] J = new float[6][];

            J[0] = new float[width * height];
            J[1] = new float[width * height];
            J[2] = new float[width * height];
            J[3] = new float[width * height];
            J[4] = new float[width * height];
            J[5] = new float[width * height];


            IMem<float> J1in = Cl.CreateBuffer<float>(context, MemFlags.ReadOnly, width * height * sizeof(float), out error);

            IMem<float> J2in = Cl.CreateBuffer<float>(context, MemFlags.ReadOnly, width * height * sizeof(float), out error);

            IMem<float> J3in = Cl.CreateBuffer<float>(context, MemFlags.ReadOnly, width * height * sizeof(float), out error);

            IMem<float> J4in = Cl.CreateBuffer<float>(context, MemFlags.ReadOnly, width * height * sizeof(float), out error);

            IMem<float> J5in = Cl.CreateBuffer<float>(context, MemFlags.ReadOnly, width * height * sizeof(float), out error);

            IMem<float> J6in = Cl.CreateBuffer<float>(context, MemFlags.ReadOnly, width * height * sizeof(float), out error);


            IMem<float> J1out = Cl.CreateBuffer<float>(context, MemFlags.WriteOnly, width * height * sizeof(float), out error);
            IMem<float> J2out = Cl.CreateBuffer<float>(context, MemFlags.WriteOnly, width * height * sizeof(float), out error);
            IMem<float> J3out = Cl.CreateBuffer<float>(context, MemFlags.WriteOnly, width * height * sizeof(float), out error);
            IMem<float> J4out = Cl.CreateBuffer<float>(context, MemFlags.WriteOnly, width * height * sizeof(float), out error);
            IMem<float> J5out = Cl.CreateBuffer<float>(context, MemFlags.WriteOnly, width * height * sizeof(float), out error);
            IMem<float> J6out = Cl.CreateBuffer<float>(context, MemFlags.WriteOnly, width * height * sizeof(float), out error);


            Kernel _Kernel = Cl.CreateKernel(program, "localGlobalFlow2", out error);

            error |= Cl.SetKernelArg(_Kernel, 0, kernelSize);
            error |= Cl.SetKernelArg<float>(_Kernel, 1, sigma);
            error |= Cl.SetKernelArg<float>(_Kernel, 2, J1in);
            error |= Cl.SetKernelArg<float>(_Kernel, 3, J2in);
            error |= Cl.SetKernelArg<float>(_Kernel, 4, J3in);
            error |= Cl.SetKernelArg<float>(_Kernel, 5, J4in);
            error |= Cl.SetKernelArg<float>(_Kernel, 6, J5in);
            error |= Cl.SetKernelArg<float>(_Kernel, 7, J6in);
            error |= Cl.SetKernelArg<float>(_Kernel, 8, J1out);
            error |= Cl.SetKernelArg<float>(_Kernel, 9, J2out);
            error |= Cl.SetKernelArg<float>(_Kernel, 10, J3out);
            error |= Cl.SetKernelArg<float>(_Kernel, 11, J4out);
            error |= Cl.SetKernelArg<float>(_Kernel, 12, J5out);
            error |= Cl.SetKernelArg<float>(_Kernel, 13, J6out);
            error |= Cl.SetKernelArg(_Kernel, 14, width);
            error |= Cl.SetKernelArg(_Kernel, 15, height);

            Event _event;
            IntPtr[] workGroupSizePtr = new IntPtr[] { (IntPtr)(height * width) };

            error = Cl.EnqueueWriteBuffer<float>(commandQueue, J1in, Bool.True, J_in[0], 0, null, out _event);

            error = Cl.EnqueueWriteBuffer<float>(commandQueue, J2in, Bool.True, J_in[1], 0, null, out _event);

            error = Cl.EnqueueWriteBuffer<float>(commandQueue, J3in, Bool.True, J_in[2], 0, null, out _event);

            error = Cl.EnqueueWriteBuffer<float>(commandQueue, J4in, Bool.True, J_in[3], 0, null, out _event);

            error = Cl.EnqueueWriteBuffer<float>(commandQueue, J5in, Bool.True, J_in[4], 0, null, out _event);

            error = Cl.EnqueueWriteBuffer<float>(commandQueue, J6in, Bool.True, J_in[5], 0, null, out _event);

            error = Cl.EnqueueNDRangeKernel(commandQueue, _Kernel, 1, null, workGroupSizePtr, null, 0, null, out _event);
            error = Cl.Finish(commandQueue);

            Cl.EnqueueReadBuffer<float>(commandQueue, J1out, Bool.True, 0, (width * height), J[0], 0, null, out _event);
            error = Cl.Finish(commandQueue);
            Cl.EnqueueReadBuffer<float>(commandQueue, J2out, Bool.True, 0, (width * height), J[1], 0, null, out _event);
            error = Cl.Finish(commandQueue);
            Cl.EnqueueReadBuffer<float>(commandQueue, J3out, Bool.True, 0, (width * height), J[2], 0, null, out _event);
            error = Cl.Finish(commandQueue);
            Cl.EnqueueReadBuffer<float>(commandQueue, J4out, Bool.True, 0, (width * height), J[3], 0, null, out _event);
            error = Cl.Finish(commandQueue);
            Cl.EnqueueReadBuffer<float>(commandQueue, J5out, Bool.True, 0, (width * height), J[4], 0, null, out _event);
            error = Cl.Finish(commandQueue);
            Cl.EnqueueReadBuffer<float>(commandQueue, J6out, Bool.True, 0, (width * height), J[5], 0, null, out _event);
            error = Cl.Finish(commandQueue);


            Cl.ReleaseMemObject(J1in);
            Cl.ReleaseMemObject(J2in);
            Cl.ReleaseMemObject(J3in);
            Cl.ReleaseMemObject(J4in);
            Cl.ReleaseMemObject(J5in);
            Cl.ReleaseMemObject(J6in);
            Cl.ReleaseMemObject(J1out);
            Cl.ReleaseMemObject(J2out);
            Cl.ReleaseMemObject(J3out);
            Cl.ReleaseMemObject(J4out);
            Cl.ReleaseMemObject(J5out);
            Cl.ReleaseMemObject(J6out);



            Cl.ReleaseKernel(_Kernel);


            return J;



        }

        private float[][] calc_J(SimpleImage I0, SimpleImage I1d)

        {
            float[][] J = new float[6][];

            J[0] = new float[I0.ImageHeight * I0.ImageWidth];
            J[1] = new float[I0.ImageHeight * I0.ImageWidth];
            J[2] = new float[I0.ImageHeight * I0.ImageWidth];
            J[3] = new float[I0.ImageHeight * I0.ImageWidth];
            J[4] = new float[I0.ImageHeight * I0.ImageWidth];
            J[5] = new float[I0.ImageHeight * I0.ImageWidth];


            Mem leftImageMemObject = (Mem)Cl.CreateImage2D(context, MemFlags.ReadOnly | MemFlags.CopyHostPtr, SimpleImage.clImageFormat, (IntPtr)I0.ImageWidth, (IntPtr)I0.ImageHeight, (IntPtr)0, I0.ByteArray, out error);

            Mem rightImageMemObject = (Mem)Cl.CreateImage2D(context, MemFlags.ReadOnly | MemFlags.CopyHostPtr, SimpleImage.clImageFormat, (IntPtr)I1d.ImageWidth, (IntPtr)I1d.ImageHeight, (IntPtr)0, I1d.ByteArray, out error);



            IMem<float> J1 = Cl.CreateBuffer<float>(context, MemFlags.WriteOnly, I0.ImageHeight * I0.ImageWidth * sizeof(float), out error);
            IMem<float> J2 = Cl.CreateBuffer<float>(context, MemFlags.WriteOnly, I0.ImageHeight * I0.ImageWidth * sizeof(float), out error);
            IMem<float> J3 = Cl.CreateBuffer<float>(context, MemFlags.WriteOnly, I0.ImageHeight * I0.ImageWidth * sizeof(float), out error);
            IMem<float> J4 = Cl.CreateBuffer<float>(context, MemFlags.WriteOnly, I0.ImageHeight * I0.ImageWidth * sizeof(float), out error);
            IMem<float> J5 = Cl.CreateBuffer<float>(context, MemFlags.WriteOnly, I0.ImageHeight * I0.ImageWidth * sizeof(float), out error);
            IMem<float> J6 = Cl.CreateBuffer<float>(context, MemFlags.WriteOnly, I0.ImageHeight * I0.ImageWidth * sizeof(float), out error);

            Kernel _Kernel = Cl.CreateKernel(program, "localGlobalFlow_J", out error);

            error |= Cl.SetKernelArg(_Kernel, 0, leftImageMemObject);
            error |= Cl.SetKernelArg(_Kernel, 1, rightImageMemObject);
            error |= Cl.SetKernelArg<float>(_Kernel, 2, J1);
            error |= Cl.SetKernelArg<float>(_Kernel, 3, J2);
            error |= Cl.SetKernelArg<float>(_Kernel, 4, J3);
            error |= Cl.SetKernelArg<float>(_Kernel, 5, J4);
            error |= Cl.SetKernelArg<float>(_Kernel, 6, J5);
            error |= Cl.SetKernelArg<float>(_Kernel, 7, J6);
            error |= Cl.SetKernelArg(_Kernel, 8, I0.ImageWidth);
            error |= Cl.SetKernelArg(_Kernel, 9, I0.ImageHeight);

            Event _event;
            IntPtr[] originPtr = new IntPtr[] { (IntPtr)0, (IntPtr)0, (IntPtr)0 };
            IntPtr[] regionPtr = new IntPtr[] { (IntPtr)I0.ImageWidth, (IntPtr)I0.ImageHeight, (IntPtr)1 };
            IntPtr[] workGroupSizePtr = new IntPtr[] { (IntPtr)(I0.ImageWidth * I0.ImageHeight) };


            error = Cl.EnqueueWriteImage(commandQueue, leftImageMemObject, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, I0.ByteArray, 0, null, out _event);

            error = Cl.EnqueueWriteImage(commandQueue, rightImageMemObject, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, I1d.ByteArray, 0, null, out _event);

            error = Cl.EnqueueNDRangeKernel(commandQueue, _Kernel, 1, null, workGroupSizePtr, null, 0, null, out _event);
            error = Cl.Finish(commandQueue);

            Cl.EnqueueReadBuffer<float>(commandQueue, J1, Bool.True, 0, (I0.ImageWidth * I0.ImageHeight), J[0], 0, null, out _event);
            error = Cl.Finish(commandQueue);

            Cl.EnqueueReadBuffer<float>(commandQueue, J2, Bool.True, 0, (I0.ImageWidth * I0.ImageHeight), J[1], 0, null, out _event);
            error = Cl.Finish(commandQueue);

            Cl.EnqueueReadBuffer<float>(commandQueue, J3, Bool.True, 0, (I0.ImageWidth * I0.ImageHeight), J[2], 0, null, out _event);
            error = Cl.Finish(commandQueue);

            Cl.EnqueueReadBuffer<float>(commandQueue, J4, Bool.True, 0, (I0.ImageWidth * I0.ImageHeight), J[3], 0, null, out _event);

            Cl.EnqueueReadBuffer<float>(commandQueue, J5, Bool.True, 0, (I0.ImageWidth * I0.ImageHeight), J[4], 0, null, out _event);
            error = Cl.Finish(commandQueue);

            Cl.EnqueueReadBuffer<float>(commandQueue, J6, Bool.True, 0, (I0.ImageWidth * I0.ImageHeight), J[5], 0, null, out _event);
            error = Cl.Finish(commandQueue);


            Cl.ReleaseMemObject(leftImageMemObject);
            Cl.ReleaseMemObject(rightImageMemObject);
            Cl.ReleaseMemObject(J1);
            Cl.ReleaseMemObject(J2);
            Cl.ReleaseMemObject(J3);
            Cl.ReleaseMemObject(J4);
            Cl.ReleaseMemObject(J5);
            Cl.ReleaseMemObject(J6);
            Cl.ReleaseKernel(_Kernel);


            return J;

        }

       private FlowArray calcLocalGlobalOpticalFlow_withSmoothed_J(float[][] J, FlowArray Flow)
        {

            FlowArray resultFlowArray = new FlowArray();
            resultFlowArray.Array = new float[2][];
            resultFlowArray.Width = Flow.Width;
            resultFlowArray.Height = Flow.Height;
            resultFlowArray.Array[0] = new float[resultFlowArray.Width * resultFlowArray.Height];
            resultFlowArray.Array[1] = new float[resultFlowArray.Width * resultFlowArray.Height];


            IMem<float> uInputFlowMemObject = Cl.CreateBuffer<float>(context, MemFlags.ReadOnly, Flow.Width * Flow.Height * sizeof(float), out error);

            IMem<float> vInputFlowMemObject = Cl.CreateBuffer<float>(context, MemFlags.ReadOnly, Flow.Width * Flow.Height * sizeof(float), out error);

            IMem<float> J1in = Cl.CreateBuffer<float>(context, MemFlags.ReadOnly, Flow.Width * Flow.Height * sizeof(float), out error);

            IMem<float> J2in = Cl.CreateBuffer<float>(context, MemFlags.ReadOnly, Flow.Width * Flow.Height * sizeof(float), out error);

            IMem<float> J3in = Cl.CreateBuffer<float>(context, MemFlags.ReadOnly, Flow.Width * Flow.Height * sizeof(float), out error);

            IMem<float> J4in = Cl.CreateBuffer<float>(context, MemFlags.ReadOnly, Flow.Width * Flow.Height * sizeof(float), out error);

            IMem<float> J5in = Cl.CreateBuffer<float>(context, MemFlags.ReadOnly, Flow.Width * Flow.Height * sizeof(float), out error);

            IMem<float> J6in = Cl.CreateBuffer<float>(context, MemFlags.ReadOnly, Flow.Width * Flow.Height * sizeof(float), out error);

            IMem<float> OutFlow_U = Cl.CreateBuffer<float>(context, MemFlags.WriteOnly, Flow.Height * Flow.Width * sizeof(float), out error);
            IMem<float> OutFlow_V = Cl.CreateBuffer<float>(context, MemFlags.WriteOnly, Flow.Height * Flow.Width * sizeof(float), out error);

            Kernel currentKernel = Cl.CreateKernel(program, "localGlobalFlow3", out error);

            // Kernel arguiment declaration
            error |= Cl.SetKernelArg<float>(currentKernel, 0, J1in);
            error |= Cl.SetKernelArg<float>(currentKernel, 1, J2in);
            error |= Cl.SetKernelArg<float>(currentKernel, 2, J3in);
            error |= Cl.SetKernelArg<float>(currentKernel, 3, J4in);
            error |= Cl.SetKernelArg<float>(currentKernel, 4, J5in);
            error |= Cl.SetKernelArg<float>(currentKernel, 5, J6in);
            error |= Cl.SetKernelArg<float>(currentKernel, 6, uInputFlowMemObject);
            error |= Cl.SetKernelArg<float>(currentKernel, 7, vInputFlowMemObject);
            error |= Cl.SetKernelArg<float>(currentKernel, 8, OutFlow_U);
            error |= Cl.SetKernelArg<float>(currentKernel, 9, OutFlow_V);
            error |= Cl.SetKernelArg<float>(currentKernel, 10, alpha);
            error |= Cl.SetKernelArg<float>(currentKernel, 11, threshold);
            error |= Cl.SetKernelArg(currentKernel, 12, Flow.Width);
            error |= Cl.SetKernelArg(currentKernel, 13, Flow.Height);



            Event _event;
            IntPtr[] originPtr = new IntPtr[] { (IntPtr)0, (IntPtr)0, (IntPtr)0 };
            IntPtr[] workGroupSizePtr = new IntPtr[] { (IntPtr)(Flow.Height * Flow.Width) };

            error = Cl.EnqueueWriteBuffer<float>(commandQueue, uInputFlowMemObject, Bool.True, Flow.Array[0], 0, null, out _event);

            error = Cl.EnqueueWriteBuffer<float>(commandQueue, vInputFlowMemObject, Bool.True, Flow.Array[1], 0, null, out _event);

            error = Cl.EnqueueWriteBuffer<float>(commandQueue, J1in, Bool.True, J[0], 0, null, out _event);

            error = Cl.EnqueueWriteBuffer<float>(commandQueue, J2in, Bool.True, J[1], 0, null, out _event);

            error = Cl.EnqueueWriteBuffer<float>(commandQueue, J3in, Bool.True, J[2], 0, null, out _event);

            error = Cl.EnqueueWriteBuffer<float>(commandQueue, J4in, Bool.True, J[3], 0, null, out _event);

            error = Cl.EnqueueWriteBuffer<float>(commandQueue, J5in, Bool.True, J[4], 0, null, out _event);

            error = Cl.EnqueueWriteBuffer<float>(commandQueue, J6in, Bool.True, J[5], 0, null, out _event);



            error = Cl.EnqueueNDRangeKernel(commandQueue, currentKernel, 1, null, workGroupSizePtr, null, 0, null, out _event);
            error = Cl.Finish(commandQueue);

            Cl.EnqueueReadBuffer<float>(commandQueue, OutFlow_U, Bool.True, 0, (resultFlowArray.Width * resultFlowArray.Height), resultFlowArray.Array[0], 0, null, out _event);
            error = Cl.Finish(commandQueue);

            Cl.EnqueueReadBuffer<float>(commandQueue, OutFlow_V, Bool.True, 0, (resultFlowArray.Width * resultFlowArray.Height), resultFlowArray.Array[1], 0, null, out _event);
            error = Cl.Finish(commandQueue);


            Cl.ReleaseMemObject(uInputFlowMemObject);
            Cl.ReleaseMemObject(vInputFlowMemObject);
            Cl.ReleaseMemObject(J1in);
            Cl.ReleaseMemObject(J2in);
            Cl.ReleaseMemObject(J3in);
            Cl.ReleaseMemObject(J4in);
            Cl.ReleaseMemObject(J5in);
            Cl.ReleaseMemObject(J6in);
            Cl.ReleaseMemObject(OutFlow_U);
            Cl.ReleaseMemObject(OutFlow_V);
            Cl.ReleaseKernel(currentKernel);


            return resultFlowArray;



        }


        private FlowArray calcLocalGlobalOpticalFlow(SimpleImage Image0, SimpleImage Image1, FlowArray Flow)
        {

            FlowArray resultFlowArray = new FlowArray();
            resultFlowArray.Array = new float[2][];
            resultFlowArray.Width = Flow.Width;
            resultFlowArray.Height = Flow.Height;
            resultFlowArray.Array[0] = new float[resultFlowArray.Width * resultFlowArray.Height];
            resultFlowArray.Array[1] = new float[resultFlowArray.Width * resultFlowArray.Height];



            Mem leftImageMemObject = (Mem)Cl.CreateImage2D(context, MemFlags.ReadOnly | MemFlags.CopyHostPtr, SimpleImage.clImageFormat, (IntPtr)Image0.ImageWidth, (IntPtr)Image0.ImageHeight, (IntPtr)0, Image0.ByteArray, out error);

            Mem rightImageMemObject = (Mem)Cl.CreateImage2D(context, MemFlags.ReadOnly | MemFlags.CopyHostPtr, SimpleImage.clImageFormat, (IntPtr)Image1.ImageWidth, (IntPtr)Image1.ImageHeight, (IntPtr)0, Image1.ByteArray, out error);

            IMem<float> uInputFlowMemObject = Cl.CreateBuffer<float>(context, MemFlags.ReadOnly, Flow.Width * Flow.Height * sizeof(float), out error);

            IMem<float> vInputFlowMemObject = Cl.CreateBuffer<float>(context, MemFlags.ReadOnly, Flow.Width * Flow.Height * sizeof(float), out error);

            IMem<float> OutFlow_U = Cl.CreateBuffer<float>(context, MemFlags.WriteOnly, Flow.Height * Flow.Width * sizeof(float), out error);
            IMem<float> OutFlow_V = Cl.CreateBuffer<float>(context, MemFlags.WriteOnly, Flow.Height * Flow.Width * sizeof(float), out error);

            Kernel currentKernel = Cl.CreateKernel(program, "localGlobalFlow1", out error);

            // Kernel arguiment declaration
            error = Cl.SetKernelArg(currentKernel, 0, SimpleImage.intPtrSize, leftImageMemObject);
            error |= Cl.SetKernelArg(currentKernel, 1, SimpleImage.intPtrSize, rightImageMemObject);
            error |= Cl.SetKernelArg<float>(currentKernel, 2, uInputFlowMemObject);
            error |= Cl.SetKernelArg<float>(currentKernel, 3, vInputFlowMemObject);
            error |= Cl.SetKernelArg<float>(currentKernel, 4, OutFlow_U);
            error |= Cl.SetKernelArg<float>(currentKernel, 5, OutFlow_V);
            error |= Cl.SetKernelArg<float>(currentKernel, 6, alpha);
            
            error |= Cl.SetKernelArg<float>(currentKernel, 7, threshold);
            error |= Cl.SetKernelArg(currentKernel, 8, Flow.Width);
            error |= Cl.SetKernelArg(currentKernel, 9, Flow.Height);



            Event _event;
            IntPtr[] originPtr = new IntPtr[] { (IntPtr)0, (IntPtr)0, (IntPtr)0 };
            IntPtr[] regionPtr = new IntPtr[] { (IntPtr)Image0.ImageWidth, (IntPtr)Image0.ImageHeight, (IntPtr)1 };
            IntPtr[] workGroupSizePtr = new IntPtr[] { (IntPtr)(Flow.Height * Flow.Width) };


            error = Cl.EnqueueWriteImage(commandQueue, leftImageMemObject, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, Image0.ByteArray, 0, null, out _event);

            error = Cl.EnqueueWriteImage(commandQueue, rightImageMemObject, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, Image1.ByteArray, 0, null, out _event);

            error = Cl.EnqueueWriteBuffer<float>(commandQueue, uInputFlowMemObject, Bool.True, Flow.Array[1], 0, null, out _event);

            error = Cl.EnqueueWriteBuffer<float>(commandQueue, vInputFlowMemObject, Bool.True, Flow.Array[0], 0, null, out _event);

            error = Cl.EnqueueNDRangeKernel(commandQueue, currentKernel, 1, null, workGroupSizePtr, null, 0, null, out _event);
            error = Cl.Finish(commandQueue);

            Cl.EnqueueReadBuffer<float>(commandQueue, OutFlow_U, Bool.True, 0, (resultFlowArray.Width * resultFlowArray.Height), resultFlowArray.Array[0], 0, null, out _event);
            error = Cl.Finish(commandQueue);

            Cl.EnqueueReadBuffer<float>(commandQueue, OutFlow_V, Bool.True, 0, (resultFlowArray.Width * resultFlowArray.Height), resultFlowArray.Array[1], 0, null, out _event);
            error = Cl.Finish(commandQueue);


            Cl.ReleaseMemObject(leftImageMemObject);
            Cl.ReleaseMemObject(rightImageMemObject);
            Cl.ReleaseMemObject(uInputFlowMemObject);
            Cl.ReleaseMemObject(vInputFlowMemObject);
            Cl.ReleaseMemObject(OutFlow_U);
            Cl.ReleaseMemObject(OutFlow_V);
            Cl.ReleaseKernel(currentKernel);


            return resultFlowArray;



        }



        public FlowArray calcOpticalFlow(Bitmap image0, Bitmap image1, out int realiteration)

        {
            SimpleImage Image0 = new SimpleImage(image0);
            SimpleImage Image1 = new SimpleImage(image1);
            FlowArray opticalFlow = new FlowArray();

            opticalFlow.Height = Image1.ImageHeight;
            opticalFlow.Width = Image1.ImageWidth;
            opticalFlow.Array = new float[2][];
            opticalFlow.Array[0] = new float[opticalFlow.Width * opticalFlow.Height];
            opticalFlow.Array[1] = new float[opticalFlow.Width * opticalFlow.Height];


            float[][] J = new float[6][];
            J[0] = new float[opticalFlow.Width * opticalFlow.Height];
            J[1] = new float[opticalFlow.Width * opticalFlow.Height];
            J[2] = new float[opticalFlow.Width * opticalFlow.Height];
            J[3] = new float[opticalFlow.Width * opticalFlow.Height];
            J[4] = new float[opticalFlow.Width * opticalFlow.Height];

           
            
            float errorValue=threshold;
            int i = 0;
            if (sigma == 0)
            {
                while (!((i == iteration)) )
                {
                   
                    opticalFlow = calcLocalGlobalOpticalFlow(Image0, Image1, opticalFlow);

                    i++;
                }
            }

            if (sigma != 0)
            {
                J = calc_J(Image0, Image1);
                J = Gauss_J(J, Image0.ImageWidth, Image0.ImageHeight);


                while (!((i == iteration))) 
                {
                   
                    opticalFlow = calcLocalGlobalOpticalFlow_withSmoothed_J(J, opticalFlow);
                   
                  
                    i++;
                }  
            }
            realiteration = i;
            return opticalFlow;

        }

        public FlowArray LG_pyramidical_opticalFlow(SimpleImage Image0, SimpleImage Image1)
        {
            SimpleImage[][] pyramids = cLProcessor.CreatePyramids(Image0, Image1, pyramidLevel);
            SimpleImage[] leftPyramid = pyramids[0];
            SimpleImage[] rightPyramid = pyramids[1];

            FlowArray[] flows = new FlowArray[pyramidLevel];

            flows[pyramidLevel - 1] = calcOpticalFlow(leftPyramid[pyramidLevel - 1].Bitmap, rightPyramid[pyramidLevel - 1].Bitmap, out int k);

            SimpleImage warpImage = new SimpleImage();
            for (int i = 1; i < pyramidLevel; i++)
            {

                flows[pyramidLevel - (i + 1)] = FlowArray.Expand(flows[pyramidLevel - i], leftPyramid[pyramidLevel - (i + 1)].ImageWidth, leftPyramid[pyramidLevel - (i + 1)].ImageHeight);
                warpImage = cLProcessor.pushImagewithFlow(leftPyramid[pyramidLevel - (i + 1)], flows[pyramidLevel - (i + 1)]);
                FlowArray correction = calcOpticalFlow(warpImage.Bitmap, rightPyramid[pyramidLevel - (i + 1)].Bitmap, out k);
                flows[pyramidLevel - (i + 1)] = flows[pyramidLevel - (i + 1)] + correction;


            }

            return flows[0];
        }

    }

}
