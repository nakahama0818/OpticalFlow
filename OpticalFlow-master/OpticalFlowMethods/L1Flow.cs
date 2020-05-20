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
    class L1Flow
    {

        private ErrorCode error;
        private Device[] devices;
        private Device device;

        private Context context;
        private OpenCL.Net.Program program;
        private CommandQueue commandQueue;
        private FileHandler File_Handler = new FileHandler();
        private string[] sources = new string[3];
        private CLProcessor cLProcessor = new CLProcessor();
        public float tau, lambda, theta, epsilon, maxiter, warps;
        public float threshold, flowInterval;
        public int pyramidLevel;
        public L1Flow()
        {
            tau = (float)0.25;
            lambda = (float)0.15;
            theta = (float)0.3;
            epsilon = (float)0.01;
            maxiter = 1;
            warps = 1;
            pyramidLevel = 2;
            threshold = (float)0.5;
            flowInterval = 2;
            sources[0] = File_Handler.getKernelSourceFromLibrary("TvL1gradrho.cl");
            sources[1] = File_Handler.getKernelSourceFromLibrary("TvL1_divP_Flow.cl");
            sources[2] = File_Handler.getKernelSourceFromLibrary("TvL1_calcP.cl");
            Platform[] platforms = Cl.GetPlatformIDs(out error);
            Console.WriteLine("Error code: " + error.ToString());
            devices = Cl.GetDeviceIDs(platforms[0], DeviceType.Gpu, out error);
            Console.WriteLine("Error code: " + error.ToString());
            device = devices[0];
            context = Cl.CreateContext(null, 1, devices, null, IntPtr.Zero, out error);
            Console.WriteLine("Error code: " + error.ToString());
            program = Cl.CreateProgramWithSource(context, 3, sources, null, out error);
            Console.WriteLine("Error code: " + error.ToString());
            Cl.BuildProgram(program, 1, devices, string.Empty, null, IntPtr.Zero);
            commandQueue = Cl.CreateCommandQueue(context, device, CommandQueueProperties.None, out error);
            Console.WriteLine("Error code: " + error.ToString());

        }
        ~L1Flow()
        {
            Cl.ReleaseProgram(program);
            Cl.ReleaseCommandQueue(commandQueue);
        }

        public float[][] calc_grad_rho_c(SimpleImage I0, SimpleImage I1d, FlowArray Flow)

        {
            float[][] arrays = new float[4][];

            arrays[0] = new float[I0.ImageHeight * I0.ImageWidth];
            arrays[1] = new float[I0.ImageHeight * I0.ImageWidth];
            arrays[2] = new float[I0.ImageHeight * I0.ImageWidth];
            arrays[3] = new float[I0.ImageHeight * I0.ImageWidth];


            Mem leftImageMemObject = (Mem)Cl.CreateImage2D(context, MemFlags.ReadOnly | MemFlags.CopyHostPtr, SimpleImage.clImageFormat, (IntPtr)I0.ImageWidth, (IntPtr)I0.ImageHeight, (IntPtr)0, I0.ByteArray, out error);

            Mem rightImageMemObject = (Mem)Cl.CreateImage2D(context, MemFlags.ReadOnly | MemFlags.CopyHostPtr, SimpleImage.clImageFormat, (IntPtr)I1d.ImageWidth, (IntPtr)I1d.ImageHeight, (IntPtr)0, I1d.ByteArray, out error);

            IMem<float> uInputFlowMemObject = Cl.CreateBuffer<float>(context, MemFlags.ReadOnly, Flow.Width * Flow.Height * sizeof(float), out error);

            IMem<float> vInputFlowMemObject = Cl.CreateBuffer<float>(context, MemFlags.ReadOnly, Flow.Width * Flow.Height * sizeof(float), out error);

            IMem<float> gradXBuf = Cl.CreateBuffer<float>(context, MemFlags.WriteOnly, Flow.Height * Flow.Width * sizeof(float), out error);
            IMem<float> gradYBuf = Cl.CreateBuffer<float>(context, MemFlags.WriteOnly, Flow.Height * Flow.Width * sizeof(float), out error);
            IMem<float> grad_2Buf = Cl.CreateBuffer<float>(context, MemFlags.WriteOnly, Flow.Height * Flow.Width * sizeof(float), out error);
            IMem<float> rho_cBuf = Cl.CreateBuffer<float>(context, MemFlags.WriteOnly, Flow.Height * Flow.Width * sizeof(float), out error);

            Kernel _Kernel = Cl.CreateKernel(program, "gradRho", out error);

            error |= Cl.SetKernelArg(_Kernel, 0, leftImageMemObject);
            error |= Cl.SetKernelArg(_Kernel, 1, rightImageMemObject);
            error |= Cl.SetKernelArg<float>(_Kernel, 2, uInputFlowMemObject);
            error |= Cl.SetKernelArg<float>(_Kernel, 3, vInputFlowMemObject);
            error |= Cl.SetKernelArg<float>(_Kernel, 4, gradXBuf);
            error |= Cl.SetKernelArg<float>(_Kernel, 5, gradYBuf);
            error |= Cl.SetKernelArg<float>(_Kernel, 6, grad_2Buf);
            error |= Cl.SetKernelArg<float>(_Kernel, 7, rho_cBuf);
            error |= Cl.SetKernelArg(_Kernel, 8, I0.ImageWidth);
            error |= Cl.SetKernelArg(_Kernel, 9, I0.ImageHeight);

            Event _event;
            IntPtr[] originPtr = new IntPtr[] { (IntPtr)0, (IntPtr)0, (IntPtr)0 };
            IntPtr[] regionPtr = new IntPtr[] { (IntPtr)I0.ImageWidth, (IntPtr)I0.ImageHeight, (IntPtr)1 };
            IntPtr[] workGroupSizePtr = new IntPtr[] { (IntPtr)(Flow.Height * Flow.Width) };


            error = Cl.EnqueueWriteImage(commandQueue, leftImageMemObject, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, I0.ByteArray, 0, null, out _event);

            error = Cl.EnqueueWriteImage(commandQueue, rightImageMemObject, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, I1d.ByteArray, 0, null, out _event);



            error = Cl.EnqueueWriteBuffer<float>(commandQueue, uInputFlowMemObject, Bool.True, Flow.Array[0], 0, null, out _event);

            error = Cl.EnqueueWriteBuffer<float>(commandQueue, vInputFlowMemObject, Bool.True, Flow.Array[1], 0, null, out _event);

            error = Cl.EnqueueNDRangeKernel(commandQueue, _Kernel, 1, null, workGroupSizePtr, null, 0, null, out _event);
            error = Cl.Finish(commandQueue);

            Cl.EnqueueReadBuffer<float>(commandQueue, gradXBuf, Bool.True, 0, (Flow.Width * Flow.Height), arrays[0], 0, null, out _event);
            error = Cl.Finish(commandQueue);

            Cl.EnqueueReadBuffer<float>(commandQueue, gradYBuf, Bool.True, 0, (Flow.Width * Flow.Height), arrays[1], 0, null, out _event);
            error = Cl.Finish(commandQueue);

            Cl.EnqueueReadBuffer<float>(commandQueue, grad_2Buf, Bool.True, 0, (Flow.Width * Flow.Height), arrays[2], 0, null, out _event);
            error = Cl.Finish(commandQueue);

            Cl.EnqueueReadBuffer<float>(commandQueue, rho_cBuf, Bool.True, 0, (Flow.Width * Flow.Height), arrays[3], 0, null, out _event);
            error = Cl.Finish(commandQueue);


            Cl.ReleaseMemObject(uInputFlowMemObject);
            Cl.ReleaseMemObject(vInputFlowMemObject);
            Cl.ReleaseMemObject(leftImageMemObject);
            Cl.ReleaseMemObject(rightImageMemObject);
            Cl.ReleaseMemObject(gradXBuf);
            Cl.ReleaseMemObject(gradYBuf);
            Cl.ReleaseMemObject(grad_2Buf);
            Cl.ReleaseMemObject(rho_cBuf);
            Cl.ReleaseKernel(_Kernel);


            return arrays;

        }


        public FlowArray calc_divP_Flow(float[] Idx, float[] Idy, float[] grad_2, float[] rho_c, FlowArray inFlow, FlowArray P1, FlowArray P2)

        {
            FlowArray outFlow = new FlowArray();
            outFlow.Array = new float[2][];
            outFlow.Width = inFlow.Width;
            outFlow.Height = inFlow.Height;
            outFlow.Array[0] = new float[outFlow.Width * outFlow.Height];
            outFlow.Array[1] = new float[outFlow.Width * outFlow.Height];


            IMem<float> grad_2Buf = Cl.CreateBuffer<float>(context, MemFlags.ReadOnly, inFlow.Width * inFlow.Height * sizeof(float), out error);

            IMem<float> rho_cBuf = Cl.CreateBuffer<float>(context, MemFlags.ReadOnly, inFlow.Width * inFlow.Height * sizeof(float), out error);

            IMem<float> IdxBuf = Cl.CreateBuffer<float>(context, MemFlags.ReadOnly, inFlow.Width * inFlow.Height * sizeof(float), out error);

            IMem<float> IdyBuf = Cl.CreateBuffer<float>(context, MemFlags.ReadOnly, inFlow.Width * inFlow.Height * sizeof(float), out error);

            IMem<float> InFlow_U = Cl.CreateBuffer<float>(context, MemFlags.ReadOnly, inFlow.Width * inFlow.Height * sizeof(float), out error);

            IMem<float> InFlow_V = Cl.CreateBuffer<float>(context, MemFlags.ReadOnly, inFlow.Width * inFlow.Height * sizeof(float), out error);

            IMem<float> divP11 = Cl.CreateBuffer<float>(context, MemFlags.ReadOnly, inFlow.Width * inFlow.Height * sizeof(float), out error);
            IMem<float> divP12 = Cl.CreateBuffer<float>(context, MemFlags.ReadOnly, inFlow.Width * inFlow.Height * sizeof(float), out error);
            IMem<float> divP21 = Cl.CreateBuffer<float>(context, MemFlags.ReadOnly, inFlow.Width * inFlow.Height * sizeof(float), out error);
            IMem<float> divP22 = Cl.CreateBuffer<float>(context, MemFlags.ReadOnly, inFlow.Width * inFlow.Height * sizeof(float), out error);

            IMem<float> OutFlow_U = Cl.CreateBuffer<float>(context, MemFlags.WriteOnly, inFlow.Height * inFlow.Width * sizeof(float), out error);
            IMem<float> OutFlow_V = Cl.CreateBuffer<float>(context, MemFlags.WriteOnly, inFlow.Height * inFlow.Width * sizeof(float), out error);


            Kernel _Kernel = Cl.CreateKernel(program, "divP_Flow", out error);

            error |= Cl.SetKernelArg(_Kernel, 0, rho_cBuf);
            error |= Cl.SetKernelArg(_Kernel, 1, IdxBuf);
            error |= Cl.SetKernelArg<float>(_Kernel, 2, IdyBuf);
            error |= Cl.SetKernelArg<float>(_Kernel, 3, InFlow_U);
            error |= Cl.SetKernelArg<float>(_Kernel, 4, InFlow_V);
            error |= Cl.SetKernelArg<float>(_Kernel, 5, OutFlow_U);
            error |= Cl.SetKernelArg<float>(_Kernel, 6, OutFlow_V);
            error |= Cl.SetKernelArg<float>(_Kernel, 7, this.theta);
            error |= Cl.SetKernelArg<float>(_Kernel, 8, this.lambda);
            error |= Cl.SetKernelArg<float>(_Kernel, 9, grad_2Buf);
            error |= Cl.SetKernelArg<float>(_Kernel, 10, divP11);
            error |= Cl.SetKernelArg<float>(_Kernel, 11, divP12);
            error |= Cl.SetKernelArg<float>(_Kernel, 12, divP21);
            error |= Cl.SetKernelArg<float>(_Kernel, 13, divP22);
            error |= Cl.SetKernelArg<float>(_Kernel, 14, threshold);
            error |= Cl.SetKernelArg(_Kernel, 15, inFlow.Width);
            error |= Cl.SetKernelArg(_Kernel, 16, inFlow.Height);

            Event _event;
            IntPtr[] workGroupSizePtr = new IntPtr[] { (IntPtr)(inFlow.Height * inFlow.Width) };

            error = Cl.EnqueueWriteBuffer<float>(commandQueue, rho_cBuf, Bool.True, rho_c, 0, null, out _event);

            error = Cl.EnqueueWriteBuffer<float>(commandQueue, IdxBuf, Bool.True, Idx, 0, null, out _event);

            error = Cl.EnqueueWriteBuffer<float>(commandQueue, IdyBuf, Bool.True, Idy, 0, null, out _event);

            error = Cl.EnqueueWriteBuffer<float>(commandQueue, grad_2Buf, Bool.True, grad_2, 0, null, out _event);


            error = Cl.EnqueueWriteBuffer<float>(commandQueue, InFlow_U, Bool.True, inFlow.Array[0], 0, null, out _event);

            error = Cl.EnqueueWriteBuffer<float>(commandQueue, InFlow_V, Bool.True, inFlow.Array[1], 0, null, out _event);

            error = Cl.EnqueueWriteBuffer<float>(commandQueue, divP11, Bool.True, P1.Array[0], 0, null, out _event);
            error = Cl.EnqueueWriteBuffer<float>(commandQueue, divP12, Bool.True, P1.Array[1], 0, null, out _event);

            error = Cl.EnqueueWriteBuffer<float>(commandQueue, divP21, Bool.True, P2.Array[0], 0, null, out _event);
            error = Cl.EnqueueWriteBuffer<float>(commandQueue, divP22, Bool.True, P2.Array[1], 0, null, out _event);



            error = Cl.EnqueueNDRangeKernel(commandQueue, _Kernel, 1, null, workGroupSizePtr, null, 0, null, out _event);
            error = Cl.Finish(commandQueue);

            Cl.EnqueueReadBuffer<float>(commandQueue, OutFlow_U, Bool.True, 0, (outFlow.Width * outFlow.Height), outFlow.Array[0], 0, null, out _event);
            error = Cl.Finish(commandQueue);

            Cl.EnqueueReadBuffer<float>(commandQueue, OutFlow_V, Bool.True, 0, (outFlow.Width * outFlow.Height), outFlow.Array[1], 0, null, out _event);
            error = Cl.Finish(commandQueue);



            Cl.ReleaseMemObject(grad_2Buf);
            Cl.ReleaseMemObject(rho_cBuf);
            Cl.ReleaseMemObject(IdxBuf);
            Cl.ReleaseMemObject(IdyBuf);
            Cl.ReleaseMemObject(InFlow_U);
            Cl.ReleaseMemObject(InFlow_V);
            Cl.ReleaseMemObject(divP11);
            Cl.ReleaseMemObject(divP12);
            Cl.ReleaseMemObject(divP21);
            Cl.ReleaseMemObject(divP22);
            Cl.ReleaseMemObject(OutFlow_U);
            Cl.ReleaseMemObject(OutFlow_V);



            Cl.ReleaseKernel(_Kernel);


            return outFlow;

        }


        public FlowArray[] calc_P_field(FlowArray Flow, FlowArray P1, FlowArray P2)
        {
            ErrorCode error;

            FlowArray outputFlow1 = new FlowArray();
            outputFlow1.Array = new float[2][];
            outputFlow1.Width = P1.Width;
            outputFlow1.Height = P1.Height;
            outputFlow1.Array[0] = new float[outputFlow1.Width * outputFlow1.Height];
            outputFlow1.Array[1] = new float[outputFlow1.Width * outputFlow1.Height];

            FlowArray outputFlow2 = new FlowArray();
            outputFlow2.Array = new float[2][];
            outputFlow2.Width = P2.Width;
            outputFlow2.Height = P2.Height;
            outputFlow2.Array[0] = new float[outputFlow2.Width * outputFlow2.Height];
            outputFlow2.Array[1] = new float[outputFlow2.Width * outputFlow2.Height];


            IMem<float> uInputFlowMemObject = Cl.CreateBuffer<float>(context, MemFlags.ReadOnly, Flow.Width * Flow.Height * sizeof(float), out error);

            IMem<float> vInputFlowMemObject = Cl.CreateBuffer<float>(context, MemFlags.ReadOnly, Flow.Width * Flow.Height * sizeof(float), out error);

            IMem<float> P11_input = Cl.CreateBuffer<float>(context, MemFlags.ReadOnly, P1.Width * P1.Height * sizeof(float), out error);

            IMem<float> P12_input = Cl.CreateBuffer<float>(context, MemFlags.ReadOnly, P1.Width * P1.Height * sizeof(float), out error);

            IMem<float> P21_input = Cl.CreateBuffer<float>(context, MemFlags.ReadOnly, P2.Width * P2.Height * sizeof(float), out error);

            IMem<float> P22_input = Cl.CreateBuffer<float>(context, MemFlags.ReadOnly, P2.Width * P2.Height * sizeof(float), out error);



            IMem<float> P11_output = Cl.CreateBuffer<float>(context, MemFlags.WriteOnly, outputFlow1.Width * outputFlow1.Height * sizeof(float), out error);

            IMem<float> P12_output = Cl.CreateBuffer<float>(context, MemFlags.WriteOnly, outputFlow1.Width * outputFlow1.Height * sizeof(float), out error);

            IMem<float> P21_output = Cl.CreateBuffer<float>(context, MemFlags.WriteOnly, outputFlow2.Width * outputFlow2.Height * sizeof(float), out error);

            IMem<float> P22_output = Cl.CreateBuffer<float>(context, MemFlags.WriteOnly, outputFlow2.Width * outputFlow2.Height * sizeof(float), out error);


            Kernel _Kernel = Cl.CreateKernel(program, "calcP", out error);

            error |= Cl.SetKernelArg<float>(_Kernel, 0, uInputFlowMemObject);
            error |= Cl.SetKernelArg<float>(_Kernel, 1, vInputFlowMemObject);
            error |= Cl.SetKernelArg<float>(_Kernel, 2, this.tau);
            error |= Cl.SetKernelArg<float>(_Kernel, 3, this.theta);
            error |= Cl.SetKernelArg<float>(_Kernel, 4, P11_input);
            error |= Cl.SetKernelArg<float>(_Kernel, 5, P12_input);
            error |= Cl.SetKernelArg<float>(_Kernel, 6, P21_input);
            error |= Cl.SetKernelArg<float>(_Kernel, 7, P22_input);
            error |= Cl.SetKernelArg<float>(_Kernel, 8, P11_output);
            error |= Cl.SetKernelArg<float>(_Kernel, 9, P12_output);
            error |= Cl.SetKernelArg<float>(_Kernel, 10, P21_output);
            error |= Cl.SetKernelArg<float>(_Kernel, 11, P22_output);
            error |= Cl.SetKernelArg(_Kernel, 12, Flow.Width);
            error |= Cl.SetKernelArg(_Kernel, 13, Flow.Height);

            Event _event;

            IntPtr[] workGroupSizePtr = new IntPtr[] { (IntPtr)(outputFlow1.Height * outputFlow1.Width) };

            error = Cl.EnqueueWriteBuffer<float>(commandQueue, uInputFlowMemObject, Bool.True, Flow.Array[0], 0, null, out _event);

            error = Cl.EnqueueWriteBuffer<float>(commandQueue, vInputFlowMemObject, Bool.True, Flow.Array[1], 0, null, out _event);

            error = Cl.EnqueueWriteBuffer<float>(commandQueue, P11_input, Bool.True, P1.Array[0], 0, null, out _event);

            error = Cl.EnqueueWriteBuffer<float>(commandQueue, P12_input, Bool.True, P1.Array[1], 0, null, out _event);

            error = Cl.EnqueueWriteBuffer<float>(commandQueue, P21_input, Bool.True, P2.Array[0], 0, null, out _event);

            error = Cl.EnqueueWriteBuffer<float>(commandQueue, P22_input, Bool.True, P2.Array[1], 0, null, out _event);

            error = Cl.EnqueueNDRangeKernel(commandQueue, _Kernel, 1, null, workGroupSizePtr, null, 0, null, out _event);
            error = Cl.Finish(commandQueue);

            Cl.EnqueueReadBuffer<float>(commandQueue, P11_output, Bool.True, 0, (outputFlow1.Width * outputFlow1.Height), outputFlow1.Array[0], 0, null, out _event);
            error = Cl.Finish(commandQueue);

            Cl.EnqueueReadBuffer<float>(commandQueue, P12_output, Bool.True, 0, (outputFlow1.Width * outputFlow1.Height), outputFlow1.Array[1], 0, null, out _event);
            error = Cl.Finish(commandQueue);

            Cl.EnqueueReadBuffer<float>(commandQueue, P21_output, Bool.True, 0, (outputFlow2.Width * outputFlow2.Height), outputFlow2.Array[0], 0, null, out _event);
            error = Cl.Finish(commandQueue);

            Cl.EnqueueReadBuffer<float>(commandQueue, P22_output, Bool.True, 0, (outputFlow2.Width * outputFlow2.Height), outputFlow2.Array[1], 0, null, out _event);
            error = Cl.Finish(commandQueue);

            Cl.ReleaseMemObject(uInputFlowMemObject);
            Cl.ReleaseMemObject(vInputFlowMemObject);
            Cl.ReleaseMemObject(P11_input);
            Cl.ReleaseMemObject(P12_input);
            Cl.ReleaseMemObject(P21_input);
            Cl.ReleaseMemObject(P22_input);
            Cl.ReleaseMemObject(P11_output);
            Cl.ReleaseMemObject(P12_output);
            Cl.ReleaseMemObject(P21_output);
            Cl.ReleaseMemObject(P22_output);
            Cl.ReleaseKernel(_Kernel);

            FlowArray[] OutputFlows = new FlowArray[2];
            OutputFlows[0] = outputFlow1;
            OutputFlows[1] = outputFlow2;
            return OutputFlows;
        }


        public FlowArray TV_L1_opticalFlow(Bitmap image0, Bitmap image1, out int realIteration)

        {
            SimpleImage Image0 = new SimpleImage(image0);
            SimpleImage Image1 = new SimpleImage(image1);

            float[][] flowDatas = new float[4][];
            SimpleImage Id = new SimpleImage();
            FlowArray opticalFlow = new FlowArray();
            FlowArray P1 = new FlowArray();
            FlowArray P2 = new FlowArray();
            opticalFlow.Height = Image1.ImageHeight;
            opticalFlow.Width = Image1.ImageWidth;
            opticalFlow.Array = new float[2][];
            opticalFlow.Array[0] = new float[opticalFlow.Width * opticalFlow.Height];
            opticalFlow.Array[1] = new float[opticalFlow.Width * opticalFlow.Height];
            P1.Height = Image1.ImageHeight;
            P1.Width = Image1.ImageWidth;
            P1.Array = new float[2][];
            P1.Array[0] = new float[P1.Width * P1.Height];
            P1.Array[1] = new float[P1.Width * P1.Height];

            P2.Height = Image1.ImageHeight;
            P2.Width = Image1.ImageWidth;
            P2.Array = new float[2][];
            P2.Array[0] = new float[P2.Width * P2.Height];
            P2.Array[1] = new float[P2.Width * P2.Height];
            FlowArray[] P = new FlowArray[2];
            
         
            int i = 0;
            Id = Image0;

            for (int j = 0; j < warps; j++)
            {
                if (warps > 1)
                {
                   // Id = cLProcessor.pushImagewithFlow(Image0, opticalFlow);
                }
               

                flowDatas = calc_grad_rho_c(Id, Image1, opticalFlow);
                float[] I1dx = flowDatas[0];
                float[] I1dy = flowDatas[1];
                float[] grad_2 = flowDatas[2];
                float[] rho_c = flowDatas[3];
                float thresholdin = this.epsilon * this.epsilon;
                float errorValue = threshold;
                

                while (!((i == this.maxiter) ))
                {
                    opticalFlow = calc_divP_Flow(flowDatas[0], flowDatas[1], flowDatas[2], flowDatas[3], opticalFlow, P1, P2);
                  
                    P = calc_P_field(opticalFlow, P1, P2);
                    P1 = P[0];
                    P2 = P[1];
                    i++;
                    

                }

            }
            realIteration = i;
            return opticalFlow;


        }



        public FlowArray TV_L1_pyramidical_opticalFlow(SimpleImage Image0, SimpleImage Image1)
        {
           
            SimpleImage[][] pyramids = cLProcessor.CreatePyramids(Image0, Image1, pyramidLevel);
            SimpleImage[] leftPyramid = pyramids[0];
            SimpleImage[] rightPyramid = pyramids[1];

            FlowArray[] flows = new FlowArray[pyramidLevel];

            flows[pyramidLevel - 1] = TV_L1_opticalFlow(leftPyramid[pyramidLevel - 1].Bitmap, rightPyramid[pyramidLevel - 1].Bitmap, out int k);


            SimpleImage warpImage = new SimpleImage();
            for (int i = 1; i < pyramidLevel; i++)
            {

                flows[pyramidLevel - (i + 1)] = FlowArray.Expand(flows[pyramidLevel - i], leftPyramid[pyramidLevel - (i + 1)].ImageWidth, leftPyramid[pyramidLevel - (i + 1)].ImageHeight);
                warpImage = cLProcessor.pushImagewithFlow(leftPyramid[pyramidLevel - (i + 1)], flows[pyramidLevel - (i + 1)]);

                FlowArray correction = TV_L1_opticalFlow(warpImage.Bitmap, rightPyramid[pyramidLevel - (i + 1)].Bitmap, out k);
                flows[pyramidLevel - (i + 1)] = flows[pyramidLevel - (i + 1)] + correction;
               

            }

            return flows[0];
        }
    }





}
