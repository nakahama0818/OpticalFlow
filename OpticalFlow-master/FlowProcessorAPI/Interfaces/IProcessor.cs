//**************************************************************************************
//
// This header is an inseparable part of the source code.
// This source code is created by György Richárd Bogár.
// It is mandatory to refer to the source in all uses that continuously contain
// at least 10% of the code or any smaller code snippet which makes it identifiable.
//
//**************************************************************************************


namespace FlowProcessorAPI.Interfaces
{
    public interface IProcessor
    {
        SimpleImage ProcessKernelOnOneFromPath(SimpleImage inputImage, KernelStrings kernelString, string path);

        SimpleImage ProcessPreBuiltKernelsOnOne(SimpleImage inputImage, string[] kernelNames);

        SimpleImage[] ProcessPreBuiltKernelsOnTwo(SimpleImage left, SimpleImage right, string[] kernelNames);

        SimpleImage DownSample(SimpleImage inputImage);

        SimpleImage[] DownSampleTwo(SimpleImage left, SimpleImage right);

        SimpleImage[] CreatePyramid(SimpleImage input, int pyramidLevel = 4);

        SimpleImage[][] CreatePyramids(SimpleImage left, SimpleImage right, int pyramidLevel = 4);

        SimpleImage Decorateflow(SimpleImage inputImage, FlowArray flow);

        SimpleImage ModifyImageWithFlow(SimpleImage originalImage, SimpleImage rightImage, FlowArray flow);

        FlowArray CalculateFlow(SimpleImage left, SimpleImage right, float correctionalMultiplier = 1.0f, string kernelFileName = "flow");

        FlowArray CalculatePyramidalLK(SimpleImage left, SimpleImage right, int pyramidLevel = 4, int itaretaionNumber = 3, float rounding = 4.0f);

        FlowArray CalculateAdvancedFlow(SimpleImage left, SimpleImage right, FlowArray inputFlow, int isLastCalc, float correctionalMultiplier = 1.0f, string kernelFileName = "advancedFlow");

        FlowArray CalculateAdvancedPyramidalLK(SimpleImage left, SimpleImage right, int pyramidLevel = 5, float noiseReduction = 100000.0f);
    }
}
