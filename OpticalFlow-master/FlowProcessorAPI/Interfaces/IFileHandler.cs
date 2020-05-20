//**************************************************************************************
//
// This header is an inseparable part of the source code.
// This source code is created by György Richárd Bogár.
// It is mandatory to refer to the source in all uses that continuously contain
// at least 10% of the code or any smaller code snippet which makes it identifiable.
//
//**************************************************************************************

using System.Drawing;

namespace FlowProcessorAPI.Interfaces
{
    public interface IFileHandler
    {
        Bitmap GetImageFromFile(string inputImagePath);

        void GetImageSequenceFromFolder(out Bitmap[] bitmapArray, string filePath, string indexerString);

        string getKernelSourceFromLibrary(string fileName);

        string getKernelSourceFromPath(string filePath);

        void WriteFlowArrayToFile(FlowArray flow, string path);

        void WriteSimpleImageToFile(SimpleImage image, string path);
    }
}
