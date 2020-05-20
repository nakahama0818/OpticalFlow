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
    class Horn_Schunck
    {

        private ErrorCode error;
        private Device[] devices;
        private Device device;

        private Context context;
        private OpenCL.Net.Program program;
        private CommandQueue commandQueue;
        private FileHandler File_Handler = new FileHandler();
        private string[] sources = new string[1];
        public int maxIteration;
        public float alpha;
        public float flowInterval;
        public float threshold;
        public float eps;
       public int pyramidLevel;
        public int pyramidOn;
        public int warps;
        public float w;
        CLProcessor cLProcessor = new CLProcessor();
        public Horn_Schunck()
        {
            maxIteration = 10;
            alpha = 5;
            flowInterval = 3;
            w = 1;
            eps = (float)0.01;
            threshold = (float)0.5;
            pyramidLevel = 0;
            pyramidOn = 0;
            warps = 1;
            sources[0] = File_Handler.getKernelSourceFromLibrary("HornSchunck.cl");
           
            Platform[] platforms = Cl.GetPlatformIDs(out error);
            Console.WriteLine("Error code: " + error.ToString());
            devices = Cl.GetDeviceIDs(platforms[0], DeviceType.Gpu, out error);
            Console.WriteLine("Error code: " + error.ToString());
            device = devices[0];
            context = Cl.CreateContext(null, 1, devices, null, IntPtr.Zero, out error);
            Console.WriteLine("Error code: " + error.ToString());
            program = Cl.CreateProgramWithSource(context, 1, sources, null, out error);
            Console.WriteLine("Error code: " + error.ToString());
            Cl.BuildProgram(program, 1, devices, string.Empty, null, IntPtr.Zero);
            commandQueue = Cl.CreateCommandQueue(context, device, CommandQueueProperties.None, out error);
            Console.WriteLine("Error code: " + error.ToString());
        

        }

        ~Horn_Schunck()
        {
            Cl.ReleaseProgram(program);
            Cl.ReleaseCommandQueue(commandQueue);
        }

        private FlowArray HS_OpticalFlow(Bitmap input1, Bitmap input2, FlowArray inputFlow, float alpha)
        {

            SimpleImage left = new SimpleImage(input1);
            SimpleImage right = new SimpleImage(input2);



            try
            {
                if (left.ImageHeight != right.ImageHeight || left.ImageWidth != right.ImageWidth)
                {
                    throw (new ArgumentException("The size of the left and right images are not the same!"));
                }

                ErrorCode error;


                FlowArray resultFlowArray = new FlowArray();
                resultFlowArray.Array = new float[2][];
                resultFlowArray.Width = input1.Width;
                resultFlowArray.Height = input1.Height;
                resultFlowArray.Array[0] = new float[resultFlowArray.Width * resultFlowArray.Height];
                resultFlowArray.Array[1] = new float[resultFlowArray.Width * resultFlowArray.Height];

                Kernel flowKernel = Cl.CreateKernel(program, "hornSchunck", out error);

                Mem leftImageMemObject = (Mem)Cl.CreateImage2D(context, MemFlags.ReadOnly | MemFlags.CopyHostPtr, SimpleImage.clImageFormat, (IntPtr)left.ImageWidth, (IntPtr)left.ImageHeight, (IntPtr)0, left.ByteArray, out error);

                Mem rightImageMemObject = (Mem)Cl.CreateImage2D(context, MemFlags.ReadOnly | MemFlags.CopyHostPtr, SimpleImage.clImageFormat, (IntPtr)right.ImageWidth, (IntPtr)right.ImageHeight, (IntPtr)0, right.ByteArray, out error);




                IMem<float> uInputFlowMemObject = Cl.CreateBuffer<float>(context, MemFlags.ReadOnly, inputFlow.Width * inputFlow.Height * sizeof(float), out error);

                IMem<float> vInputFlowMemObject = Cl.CreateBuffer<float>(context, MemFlags.ReadOnly, inputFlow.Width * inputFlow.Height * sizeof(float), out error);

                IMem<float> uOutputFlowMemObject = Cl.CreateBuffer<float>(context, MemFlags.WriteOnly, resultFlowArray.Width * resultFlowArray.Height * sizeof(float), out error);

                IMem<float> vOutputlowMemObject = Cl.CreateBuffer<float>(context, MemFlags.WriteOnly, resultFlowArray.Width * resultFlowArray.Height * sizeof(float), out error);

                error = Cl.SetKernelArg(flowKernel, 0, leftImageMemObject);
                error |= Cl.SetKernelArg(flowKernel, 1, rightImageMemObject);
                error |= Cl.SetKernelArg<float>(flowKernel, 2, uInputFlowMemObject);
                error |= Cl.SetKernelArg<float>(flowKernel, 3, vInputFlowMemObject);
                error |= Cl.SetKernelArg<float>(flowKernel, 4, uOutputFlowMemObject);
                error |= Cl.SetKernelArg<float>(flowKernel, 5, vOutputlowMemObject);
                error |= Cl.SetKernelArg<float>(flowKernel, 6, alpha);
                error |= Cl.SetKernelArg<float>(flowKernel, 7, threshold);
                error |= Cl.SetKernelArg(flowKernel, 8, inputFlow.Width);
                error |= Cl.SetKernelArg(flowKernel, 9, inputFlow.Height);
            

                Event clEvent;

                IntPtr[] originPtr = new IntPtr[] { (IntPtr)0, (IntPtr)0, (IntPtr)0 };
                IntPtr[] regionPtr = new IntPtr[] { (IntPtr)left.ImageWidth, (IntPtr)left.ImageHeight, (IntPtr)1 };
                IntPtr[] workGroupSizePtr = new IntPtr[] { (IntPtr)(inputFlow.Height * inputFlow.Width) };

                error = Cl.EnqueueWriteImage(commandQueue, leftImageMemObject, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, left.ByteArray, 0, null, out clEvent);

                error = Cl.EnqueueWriteImage(commandQueue, rightImageMemObject, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, right.ByteArray, 0, null, out clEvent);


                error = Cl.EnqueueWriteBuffer<float>(commandQueue, uInputFlowMemObject, Bool.True, inputFlow.Array[0], 0, null, out clEvent);

                error = Cl.EnqueueWriteBuffer<float>(commandQueue, vInputFlowMemObject, Bool.True, inputFlow.Array[1], 0, null, out clEvent);


                // Enqueueing the kernel
                error = Cl.EnqueueNDRangeKernel(commandQueue, flowKernel, 1, null, workGroupSizePtr, null, 0, null, out clEvent);

                error = Cl.Finish(commandQueue);

                Cl.EnqueueReadBuffer<float>(commandQueue, uOutputFlowMemObject, Bool.True, 0, (resultFlowArray.Width * resultFlowArray.Height), resultFlowArray.Array[0], 0, null, out clEvent);
                error = Cl.Finish(commandQueue);

                Cl.EnqueueReadBuffer<float>(commandQueue, vOutputlowMemObject, Bool.True, 0, (resultFlowArray.Width * resultFlowArray.Height), resultFlowArray.Array[1], 0, null, out clEvent);
                error = Cl.Finish(commandQueue);


                Cl.ReleaseMemObject(leftImageMemObject);
                Cl.ReleaseMemObject(rightImageMemObject);
                Cl.ReleaseMemObject(uInputFlowMemObject);
                Cl.ReleaseMemObject(vInputFlowMemObject);
                Cl.ReleaseMemObject(uOutputFlowMemObject);
                Cl.ReleaseMemObject(vOutputlowMemObject);
                Cl.ReleaseKernel(flowKernel);


                return resultFlowArray;
            }
            catch (Exception e)
            {
                throw (new Exception("Flow calculation error", e));
            }


        }

        public FlowArray calculateOpticalFlow(Bitmap image0, Bitmap image1, out int realIteration, FlowArray opticalFlow)


        {
            SimpleImage inputImage_1 = new SimpleImage(image0);
            SimpleImage inputImage_2 = new SimpleImage(image1);

            opticalFlow = new FlowArray();
            FlowArray opticalFlowNext = new FlowArray();
            FlowArray finalFlow = new FlowArray();
            FlowArray medFlow = new FlowArray();
            FlowArray tempFlow = new FlowArray();
            FlowArray flowError = new FlowArray();
            SimpleImage Id = new SimpleImage();
            if (opticalFlow == null)
            {
                opticalFlow = new FlowArray();
                opticalFlow.Height = inputImage_1.ImageHeight;
                opticalFlow.Width = inputImage_1.ImageWidth;
                opticalFlow.Array = new float[2][];
                opticalFlow.Array[0] = new float[opticalFlow.Width * opticalFlow.Height];
                opticalFlow.Array[1] = new float[opticalFlow.Width * opticalFlow.Height];
            }

            opticalFlowNext = opticalFlow;
            opticalFlowNext.Height = inputImage_1.ImageHeight;
            opticalFlowNext.Width = inputImage_1.ImageWidth;
            opticalFlowNext.Array = new float[2][];
            opticalFlowNext.Array[0] = new float[opticalFlowNext.Width * opticalFlowNext.Height];
            opticalFlowNext.Array[1] = new float[opticalFlowNext.Width * opticalFlowNext.Height];

            tempFlow.Array = new float[2][];
            tempFlow.Height = inputImage_1.ImageHeight;
            tempFlow.Width = inputImage_1.ImageWidth;
            tempFlow.Array[0] = new float[medFlow.Width * medFlow.Height];
            tempFlow.Array[1] = new float[medFlow.Width * medFlow.Height];


            flowError.Array = new float[1][];
            flowError.Height = inputImage_1.ImageHeight;
            flowError.Width = inputImage_1.ImageWidth;
            flowError.Array[0] = new float[flowError.Width * flowError.Height];


           

            float thresholdin = eps * eps;
            float errorValue = thresholdin;

            int i = 0;
           
            //if (pyramidOn == 1)
            //{
              //while (!((i == maxIteration) || (thresholdin > errorValue)))
            while (!((i == maxIteration)))
                  {
                    tempFlow = opticalFlow;
                    opticalFlow = HS_OpticalFlow(inputImage_1.Bitmap, inputImage_2.Bitmap, opticalFlow, alpha);
                    //float [] error= cLProcessor.calcFlowDist(opticalFlow, tempFlow);
                   // errorValue = (error.Average());
               // Console.WriteLine(errorValue);
                    i++;
                }
                realIteration = i;
            

            return opticalFlow;
            
        }



        public FlowArray HS_pyramidical_opticalFlow(SimpleImage Image0, SimpleImage Image1)
        {
            
            SimpleImage[][] pyramids = cLProcessor.CreatePyramids(Image0, Image1, pyramidLevel);
            SimpleImage[] leftPyramid = pyramids[0];
            SimpleImage[] rightPyramid = pyramids[1];

            FlowArray[] flows = new FlowArray[pyramidLevel];

            flows[pyramidLevel - 1] = calculateOpticalFlow(leftPyramid[pyramidLevel - 1].Bitmap, rightPyramid[pyramidLevel - 1].Bitmap, out int r, null);

            SimpleImage warpImage = new SimpleImage();
            for (int i = 1; i < pyramidLevel; i++)
            {
               
                flows[pyramidLevel - (i + 1)] = FlowArray.Expand(flows[pyramidLevel - i], leftPyramid[pyramidLevel - (i + 1)].ImageWidth, leftPyramid[pyramidLevel - (i + 1)].ImageHeight);
                warpImage = cLProcessor.pushImagewithFlow(leftPyramid[pyramidLevel - (i + 1)], flows[pyramidLevel - (i + 1)]);
                FlowArray correction = calculateOpticalFlow(warpImage.Bitmap, rightPyramid[pyramidLevel - (i + 1)].Bitmap, out r, null);
                flows[pyramidLevel - (i + 1)] = flows[pyramidLevel - (i + 1)] + correction;
              

            }

            return flows[0];
        }


    }
}
