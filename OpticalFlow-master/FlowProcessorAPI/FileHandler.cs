//**************************************************************************************
//
// This header is an inseparable part of the source code.
// This source code is created by György Richárd Bogár.
// It is mandatory to refer to the source in all uses that continuously contain
// at least 10% of the code or any smaller code snippet which makes it identifiable.
//
//**************************************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using FlowProcessorAPI.Interfaces;

namespace FlowProcessorAPI
{
    public class FileHandler : IFileHandler
    {
        private static int _fileNamingID = 0;

        #region public methods
        public Bitmap GetImageFromFile(string inputImagePath)
        {
            using (FileStream imageFileStream = new FileStream(inputImagePath, FileMode.Open))
            {
                Bitmap resultImage;

                Image inputImage = Image.FromStream(imageFileStream);
                if (inputImage == null)
                {
                    throw (new Exception("Problem image couldn' be loaded properly"));
                }
                resultImage = new Bitmap(inputImage);

                return resultImage;
            }
        }

        public void GetImageSequenceFromFolder(out Bitmap[] bitmapArray, string filePath, string indexString)
        {
            string directoryPath = Directory.GetParent(filePath).ToString();
            GetSequenceWithoutThreading(out bitmapArray, directoryPath, indexString);
        }

        public string getKernelSourceFromLibrary(string fileName)
        {
            string basePath = Directory.GetParent(Directory.GetParent(Directory.GetParent(System.Environment.CurrentDirectory).FullName).FullName).FullName;

            string kernelPath = System.IO.Path.Combine(basePath, "FlowProcessorAPI", "OpenCL kernels", fileName);

            return getKernelSourceFromPath(kernelPath);
        }

        public string getKernelSourceFromPath(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                throw (new Exception("The kernel file ''" + filePath + "'' does not exist"));
            }


            return System.IO.File.ReadAllText(filePath);
        }
        
        public void WriteFlowArrayToFile(FlowArray flow, string path)
        {
            StringBuilder uBuilder = new StringBuilder();
            StringBuilder vBuilder = new StringBuilder();
            StringBuilder validBuilder = new StringBuilder();

            for (int i = 0; i < flow.Height; i++)
            {
                for (int j = 0; j < flow.Width; j++)
                {
                    string tempstring = flow.Array[0][i * flow.Width + j].ToString();
                    tempstring = tempstring.Replace(',', '.');
                    uBuilder.Append(tempstring + ",");

                    tempstring = flow.Array[1][i * flow.Width + j].ToString();
                    tempstring = tempstring.Replace(',', '.');
                    vBuilder.Append(tempstring + ",");

                    int valid = 1;
                    if (flow.Array[1][i * flow.Width + j] < 1 && flow.Array[0][i * flow.Width + j] < 1)
                    {
                        valid = 0;
                    }
                    validBuilder.Append(valid.ToString() + ",");
                }
                uBuilder.Append("\n");
                vBuilder.Append("\n");
                validBuilder.Append("\n");
            }

            File.WriteAllText(path + _fileNamingID.ToString() + "u.txt", uBuilder.ToString());
            File.WriteAllText(path + _fileNamingID.ToString() + "v.txt", vBuilder.ToString());
            File.WriteAllText(path + _fileNamingID.ToString() + "valid.txt", validBuilder.ToString());

            _fileNamingID++;
        }

        public void WriteFlowArrayToFloFile(FlowArray flow, string path)
        {
            List<byte> byteList = new List<byte>();

            string sanityCheck = "PIEH";
            byte[] tempBytes = Encoding.ASCII.GetBytes(sanityCheck);

            for(int i = 0; i < 4; i++)
            {
                byteList.Add(tempBytes[i]);
            }

            tempBytes = BitConverter.GetBytes(flow.Width);
            for (int i = 0; i < 4; i++)
            {
                byteList.Add(tempBytes[i]);
            }

            tempBytes = BitConverter.GetBytes(flow.Height);
            for (int i = 0; i < 4; i++)
            {
                byteList.Add(tempBytes[i]);
            }

            for (int i = 0; i < flow.Height; i++)
            {
                for (int j = 0; j < flow.Width; j++)
                {
                    byte[] u = BitConverter.GetBytes(flow.Array[0][i * flow.Width + j]);
                    byte[] v = BitConverter.GetBytes(flow.Array[1][i * flow.Width + j]);

                    for (int k = 0; k < 4; k++)
                    {
                        byteList.Add(u[k]);
                    }
                    for (int k = 0; k < 4; k++)
                    {
                        byteList.Add(v[k]);
                    }
                }
            }

            File.WriteAllBytes(path + ".flo", byteList.ToArray());

            _fileNamingID++;
        }

       

        public FlowArray convertFloToFlowArray(string Path)
        {
            FlowArray output = new FlowArray();
            output.Array = new float[2][];

            FileStream file = File.Open(Path, FileMode.Open);
            char c;
            BinaryReader reader = new BinaryReader(file);
            c = reader.ReadChar();
            c = reader.ReadChar();
            c = reader.ReadChar();
            c = reader.ReadChar();
            output.Width = reader.ReadInt32();
            output.Height = reader.ReadInt32();
            output.Array[0] = new float[output.Width * output.Height];
            output.Array[1] = new float[output.Width * output.Height];
            float x;
            for (int i = 0; i < (output.Width * output.Height); i++)
            {
                x = reader.ReadSingle();
                if (float.IsNaN(x) || (x > output.Width))
                {
                    x = 0;
                }
                output.U[i] = (float)x;
                x = reader.ReadSingle();
                if (float.IsNaN(x) || (x > output.Width))
                {
                    x = 0;
                }
                output.V[i] = (float)x;


            }
            reader.Close();
            reader.Dispose();
            return output;
        }



        public void WriteSimpleImageToFile(SimpleImage image, string path)
        {
            StringBuilder builder = new StringBuilder();

            for(int i = 0; i < image.BitmapHeight; i++)
            {
                for (int j = 0; j < image.BitmapWidth; j++)
                {
                    builder.Append(image.ByteArray[i*image.ImageStride+j*4].ToString() + ",");
                }
                builder.Append("\n");
            }

            path += _fileNamingID.ToString() + ".txt";
            File.WriteAllText(path ,builder.ToString());
            _fileNamingID++;
        }
        #endregion

        #region private methods
        private void GetSequenceWithoutThreading(out Bitmap[] array, string DirectoryPath, string indexString)
        {
            List<Bitmap> tempList = new List<Bitmap>();
            string filePath = DirectoryPath + "\\000000" + indexString + ".png";
            int numberOfImage = 1;
            string fileNumberPath;

            while (File.Exists(filePath))
            {
                using (FileStream imageFileStream = new FileStream(filePath, FileMode.Open))
                {
                    Bitmap resultImage;

                    Image inputImage = Image.FromStream(imageFileStream);
                    if (inputImage == null)
                    {
                        throw (new Exception("Problem image couldn' be loaded properly"));
                    }
                    resultImage = new Bitmap(inputImage);

                    tempList.Add(resultImage);
                }

                fileNumberPath = "";
                for (int i = 0; i < 6 - numberOfImage.ToString().Length; i++)
                {
                    fileNumberPath += "0";
                }
                fileNumberPath += numberOfImage.ToString() + indexString + ".png";
                filePath = DirectoryPath + "\\" + fileNumberPath;

                numberOfImage++;
            }

            array = tempList.ToArray();
        }
        #endregion
    }
}
