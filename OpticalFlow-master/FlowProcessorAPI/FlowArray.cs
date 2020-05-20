//**************************************************************************************
//
// This header is an inseparable part of the source code.
// This source code is created by György Richárd Bogár.
// It is mandatory to refer to the source in all uses that continuously contain
// at least 10% of the code or any smaller code snippet which makes it identifiable.
//
//**************************************************************************************

using System;
using FlowProcessorAPI.Interfaces;

namespace FlowProcessorAPI
{
    public class FlowArray : IFlowArray
    {
        #region constructors & destructors
        #endregion
        
        #region public fields
        public float[][] Array;
        public float[] U { get { return this.Array[0]; } }
        public float[] V { get { return this.Array[1]; } }

        public int Width;
        public int Height;
        #endregion

        #region public static methods
        public static FlowArray operator+ (FlowArray leftFlowArray, FlowArray rightFlowArray)
        {
            if (leftFlowArray.Width != rightFlowArray.Width || leftFlowArray.Height != rightFlowArray.Height)
            {
                throw new ArgumentException();
            }

            FlowArray result = new FlowArray();
            result.Width = leftFlowArray.Width;
            result.Height = leftFlowArray.Height;
            result.Array = new float[2][];
            result.Array[0] = new float[result.Height * result.Width];
            result.Array[1] = new float[result.Height * result.Width];
            for (int i = 0; i < (result.Height*result.Width); i++)
            {
                result.Array[0][i] = leftFlowArray.Array[0][i] + rightFlowArray.Array[0][i];
                result.Array[1][i] = leftFlowArray.Array[1][i] + rightFlowArray.Array[1][i];
            }

            return result;
        }

        public static FlowArray Expand(FlowArray originalFlow)
        {
            return Expand(originalFlow, (originalFlow.Width * 2), (originalFlow.Height * 2));
        }

        public static FlowArray Expand(FlowArray originalFlow, int width, int height)
        {
            if (width < originalFlow.Width*2 || height < originalFlow.Height*2)
            {
                throw new ArgumentException();
            }

            FlowArray resultArray = new FlowArray();

            resultArray.Width = width;
            resultArray.Height = height;
            resultArray.Array = new float[2][];
            resultArray.Array[0] = new float[resultArray.Width * resultArray.Height];
            resultArray.Array[1] = new float[resultArray.Width * resultArray.Height];

            for (int i = 0; i < originalFlow.Height * originalFlow.Width; i++)
            {
                float u = originalFlow.Array[0][i] * 2;
                float v = originalFlow.Array[1][i] * 2;
                int x = i % originalFlow.Width;
                int y = i / originalFlow.Width;

                resultArray.Array[0][(x * 2) + (y * 2 * resultArray.Width)] = u;
                resultArray.Array[0][(x * 2 + 1) + (y * 2 * resultArray.Width)] = u;
                resultArray.Array[0][(x * 2) + ((y * 2 + 1) * resultArray.Width)] = u;
                resultArray.Array[0][(x * 2 + 1) + ((y * 2 + 1) * resultArray.Width)] = u;

                resultArray.Array[1][(x * 2) + (y * 2 * resultArray.Width)] = v;
                resultArray.Array[1][(x * 2 + 1) + (y * 2 * resultArray.Width)] = v;
                resultArray.Array[1][(x * 2) + ((y * 2 + 1) * resultArray.Width)] = v;
                resultArray.Array[1][(x * 2 + 1) + ((y * 2 + 1) * resultArray.Width)] = v;
            }

            return resultArray;
        }

        public static FlowArray ExpandForEvaluation(FlowArray originalFlow, int imageWidth, int imageHeight)
        {
            if (originalFlow.Width*5 > imageWidth || originalFlow.Height*5 > imageHeight)
            {
                throw new ArgumentException("Parameter has wong size");
            }

            FlowArray resultFlow = new FlowArray();
            resultFlow.Width = imageWidth;
            resultFlow.Height = imageHeight;
            resultFlow.Array = new float[2][];
            resultFlow.Array[0] = new float[resultFlow.Width * resultFlow.Height];
            resultFlow.Array[1] = new float[resultFlow.Width * resultFlow.Height];

            for(int i = 0; i < originalFlow.Width*originalFlow.Height; i++)
            {
                float u = originalFlow.U[i];
                float v = originalFlow.V[i];

                int xBase = i % originalFlow.Width;
                int yBase = i / originalFlow.Width;

                for(int y = 0; y < 5; y++)
                {
                    for(int x = 0; x < 5; x++)
                    {
                        resultFlow.Array[0][(xBase * 5 + x) + ((yBase * 5+ y) * resultFlow.Width)] = u;
                        resultFlow.Array[1][(xBase * 5 + x) + ((yBase * 5 + y) * resultFlow.Width)] = v;
                    }
                }
            }

            return resultFlow;
        }
        #endregion
    }
}