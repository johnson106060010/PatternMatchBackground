using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Timers;
using System.Threading;
using System.Diagnostics;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.Util;
using Emgu.CV.Features2D;
using Emgu.CV.Flann;
using Emgu.CV.Util;


namespace Emgu
{
    class Program
        
    {

        static void Main(string[] args)
        {
            BackGround bg = new BackGround();
            bg.execute();
            //Image<Bgr, byte> sampleImg = new Image<Bgr, byte>(@"C:\Users\johnson\Desktop\pattern matching\sample cube.png");
            //Image<Bgr, byte> targetImg = new Image<Bgr, byte>(@"C:\Users\johnson\Desktop\pattern matching\target cube.png");

            ////set ROI (region of interest)
            ///*
            //sampleImg.ROI = new Rectangle(200, 200, 500, 500);
            //CvInvoke.Imshow("check roi", sampleImg);
            //*/
            //Image<Gray, float> result = sampleImg.MatchTemplate(targetImg, TemplateMatchingType.CcoeffNormed);

            //double[] minValue = { 0 };
            //double[] maxValue = { 0 };
            //Point[] minPoint = null;
            //Point[] maxPoint = null;
            //double mMinThreshold = 0.5;

            //result.MinMax(out minValue, out maxValue, out minPoint, out maxPoint);

            //for (int i = 0; i < result.Rows; i++)
            //{
            //    for (int j = 0; j < result.Cols; j++)
            //    {
            //        if (result.Data[i, j, 0] > mMinThreshold)
            //        {
            //            CvInvoke.Rectangle(sampleImg, new Rectangle(new Point(j, i), targetImg.Size)
            //            , new MCvScalar(255, 0, 255), 3);
            //        }
            //    }
            //}
            //Console.WriteLine("Maxmimum position: " + maxPoint[0].ToString() + "\n");
            //Console.WriteLine("Maximum Similarity: " + maxValue[0].ToString() + "\n");
            ////Console.WriteLine("Minimum Similarity: " + min.ToString() + "\n");
            ////CvInvoke.Imshow("test", sampleImg);
            ////CvInvoke.WaitKey(0);
            //CvInvoke.DestroyWindow("test");

        }
        
    }

    public class SharedMemoryHandler {
        MemoryMappedFile mSharedMemoryHeader = null;
        MemoryMappedFile mSharedMemorySample = null;
        MemoryMappedFile mSharedMemoryTarget = null;
        MemoryMappedFile mSharedMemoryResult = null;
        Image<Bgr, byte> mSampleImg = null;
        Image<Bgr, byte> mTargetImg = null;
        Image<Bgr, byte> mResultImg = null;
        int mSampleImgWidth = 0, mSampleImgHeight = 0, mTargetImgWidth = 0, mTargetImgHeight = 0;
        Rectangle mROIRange = new Rectangle();
        int mRotationValue = 0;//0 degree for default
        int mResizeValue = 100;//100% for default
        int mOffsetX = 0;
        int mOffsetY = 0;
        double mMinThreshold = 0.5;
        int mContinueModify = 0;//0 for not continue modify 1 for continuing modify
        public SharedMemoryHandler()
        {
            try
            {
                mSharedMemoryHeader = MemoryMappedFile.OpenExisting("smHeader");
                mSharedMemoryResult = MemoryMappedFile.OpenExisting("smResult");
                mSharedMemorySample = MemoryMappedFile.OpenExisting("smSample");
                mSharedMemoryTarget = MemoryMappedFile.OpenExisting("smTarget");
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void ReadImgFromSharedMemory(out Image<Bgr, byte>  sampleImg, out Image<Bgr, byte> targetImg)
        {
  
            //mSampleImg = new Image<Bgr, byte>(576, 335);

            /*
            using(MemoryMappedViewStream stream = mSharedMemoryHeader.CreateViewStream())
            {
                using (BinaryReader br = new BinaryReader(stream))
                {
                    
                    byte[] info = new byte[512];
                    info = br.ReadBytes(512);
                    string infoStr = System.Text.Encoding.UTF8.GetString(info);
                    inst = DecodeHeader(infoStr);
                    //mSampleImg.Bytes = br.ReadBytes(mSampleImgWidth * mSampleImgHeight * 3);//576*335

                    //mTargetImg.Bytes = br.ReadBytes(mTargetImgWidth * mTargetImgHeight * 3);

                }
            }*/
            using(MemoryMappedViewStream stream = mSharedMemorySample.CreateViewStream())
            {
                using (BinaryReader br = new BinaryReader(stream))
                {
                    mSampleImg.Bytes = br.ReadBytes(mSampleImgWidth * mSampleImgHeight * 3);
                }
                //stream.Read(mSampleImg.Bytes, 0, mSampleImgWidth * mSampleImgHeight * 3);
            }

            using (MemoryMappedViewStream stream = mSharedMemoryTarget.CreateViewStream())
            {
                
                //Console.WriteLine("target image memory capacity: {0}", stream.Capacity);
                using (BinaryReader br = new BinaryReader(stream))
                {
                    mTargetImg.Bytes = br.ReadBytes(3 * mTargetImgHeight * mTargetImgWidth);

                    //put in one by one
                    //int offset = 0;
                    //byte[] by = br.ReadBytes(3 * mTargetImgHeight * mTargetImgWidth);
                    //for (int i=0; i<mTargetImgHeight; i++)
                    //{
                    //    for(int j=0; j<mTargetImgWidth; j++)
                    //    {
                    //        for(int k=0; k<3; k++,offset++)
                    //        {
                    //            mTargetImg.Data[i, j, k] = by[offset];
                    //        }                            
                    //    }
                    //}
                    
                }
                //stream.Read(mTargetImg.Bytes, 0, mTargetImgWidth * mTargetImgHeight * 3);
            }
            //CvInvoke.Imshow("sampleImg", mSampleImg);
            //CvInvoke.Imshow("targetImg", mTargetImg);
            //CvInvoke.WaitKey(0);
            sampleImg = mSampleImg;
            targetImg = mTargetImg;

        }
        //if continue 
        public void ReadResultImg(out Image<Bgr, byte> resultImg)
        {
            mResultImg = new Image<Bgr, byte>(mSampleImgWidth, mSampleImgHeight);
            using (MemoryMappedViewStream stream = mSharedMemoryResult.CreateViewStream())
            {
                using (BinaryReader br = new BinaryReader(stream))
                {
                    mResultImg.Bytes = br.ReadBytes(mSampleImgWidth * mSampleImgHeight * 3);
                }
                //stream.Read(mSampleImg.Bytes, 0, mSampleImgWidth * mSampleImgHeight * 3);
            }

            resultImg = mResultImg;
        }
        public string ReadHeaderFromSharedMemory(
            out Rectangle ROI,
            out int rotate,
            out int resize,
            out double threshold,
            out int offsetX,
            out int offsetY,
            out int continueModify
            )
        {
            string inst = "";
            //mSharedMemoryHeader = MemoryMappedFile.OpenExisting("smHeader");
            using (MemoryMappedViewStream stream = mSharedMemoryHeader.CreateViewStream())
            {
                using (BinaryReader br = new BinaryReader(stream))
                {
                    byte[] info = new byte[512];
                    info = br.ReadBytes(512);
                    string infoStr = System.Text.Encoding.UTF8.GetString(info);
                    inst = DecodeHeader(infoStr);
                }
            }
            ROI = mROIRange;
            rotate = mRotationValue;
            resize = mResizeValue;
            threshold = mMinThreshold;
            offsetX = mOffsetX;
            offsetY = mOffsetY;
            continueModify = mContinueModify;
            return inst;
        }
        public string DecodeHeader(string header)
        {
            Console.WriteLine("Header: " + header + "\n");
            string[] infoStrEle = header.Split(',');
            string inst = infoStrEle[0];
            
            if(inst != "1")
            {
                mSampleImgWidth = int.Parse(infoStrEle[1]);
                mSampleImgHeight = int.Parse(infoStrEle[2]);
                mTargetImgWidth = int.Parse(infoStrEle[3]);
                mTargetImgHeight = int.Parse(infoStrEle[4]);
                mSampleImg = new Image<Bgr, byte>(mSampleImgWidth, mSampleImgHeight);
                //holding array is different by 3*width*height by the width and height are not the multiple of 4
                //case 1
                //mTargetImg = new Image<Bgr, byte>(new byte[mTargetImgWidth, mTargetImgHeight, 3]);
                //case 2
                //Bitmap b = new Bitmap(mTargetImgWidth, mTargetImgHeight);
                //mTargetImg = new Image<Bgr, byte>(b);
                //case 3
                //mTargetImg = new Image<Bgr, byte>(mTargetImgWidth, mTargetImgHeight);
                //case 4
                mTargetImg = new Image<Bgr, byte>(new Size(mTargetImgWidth, mTargetImgHeight));

                if (mTargetImg.Bytes.Length != 3 * mTargetImgWidth * mTargetImgHeight)
                {
                    Console.WriteLine("TargetImg Byte Array Length not same as setting size");
                    Console.WriteLine("Width and Height require to be the multiple of 4");
                }
                mROIRange = new Rectangle(
                    int.Parse(infoStrEle[5]),
                    int.Parse(infoStrEle[6]),
                    int.Parse(infoStrEle[7]),
                    int.Parse(infoStrEle[8])
                    );
                mRotationValue = int.Parse(infoStrEle[9]);
                mResizeValue = int.Parse(infoStrEle[10]);
                mMinThreshold = double.Parse(infoStrEle[11]);
                mOffsetX = int.Parse(infoStrEle[12]);
                mOffsetY = int.Parse(infoStrEle[13]);
                mContinueModify = int.Parse(infoStrEle[14]);
                Console.WriteLine(
                    "TargetImg Width: {0}\n TargetImg Height: {1}\n",
                    mTargetImgWidth,
                    mTargetImgHeight
                    );
            }
            
            return inst;
        }

        //write resultImg to shared memory and update resultImg header to header memory
        public void WriteToSharedMemory(Image<Bgr, byte> result, string info)
        {
            byte[] overwriteInfo = new byte[512];
            Array.Clear(overwriteInfo, 0, overwriteInfo.Length);
            //mSharedMemoryHeader = MemoryMappedFile.OpenExisting("smHeader");
            //mSharedMemoryResult = MemoryMappedFile.OpenExisting("smResult");
            Mutex mut = new Mutex(true, "ResultSharedMemoryMutex");

            using (var stream = mSharedMemoryResult.CreateViewStream())
            {
                stream.Position = 0;
                stream.Write(result.Bytes, 0, result.Bytes.Length);
                //stream.Position = 0;
                //using (BinaryReader br = new BinaryReader(stream))
                //{
                //    byte[] b = br.ReadBytes(578880);
                //    //Console.WriteLine(Encoding.UTF8.GetString(b));
                //    Image<Bgr, byte> test = new Image<Bgr, byte>(576,335);
                //    test.Bytes = b;
                //    CvInvoke.Imshow("TEST", test);
                //    CvInvoke.WaitKey(0);
                //}
            }

            using (var stream = mSharedMemoryHeader.CreateViewStream())
            {
                stream.Position = 0;
                using (var bw = new BinaryWriter(stream))
                {
                    bw.Write(overwriteInfo);
                    stream.Position = 0;
                    bw.Write(Encoding.UTF8.GetBytes(info));
                }

            }
            
            mut.ReleaseMutex();
        }

    }

    public class BackGround
    {
        Image<Bgr, byte> mSampleImg = new Image<Bgr, byte>(0, 0);
        Image<Bgr, byte> mTargetImg = new Image<Bgr, byte>(0, 0);
        Image<Bgr, byte> mResultImg = new Image<Bgr, byte>(0, 0);
        double mMinThreshold = 0.5;
        Rectangle mROIRange = new Rectangle();
        int mRotationValue = 0;//0 degree for default
        int mResizeValue = 100;//100% for default
        int mOffsetX = 0;
        int mOffsetY = 0;
        int mContinueModify = 0;
        SharedMemoryHandler mSharedMemoryHandler = null;
        System.Timers.Timer mHeaderReaderTimer = null;
        //execute function start timer to read the header memory
        public BackGround()
        {
            mSharedMemoryHandler = new SharedMemoryHandler();
        }
        //background process will be close if UI get the result img
        public void execute()
        {
            //mHeaderReaderTimer = new System.Timers.Timer(100000);
            //mHeaderReaderTimer.Elapsed += new ElapsedEventHandler(ReadHeader);
            //mHeaderReaderTimer.Start();
            //Console.ReadKey();
            ReadHeader();
        }
        //check the header instruction if inst == 0 do nothing
        //, if inst == 1 it means UI process have not read the resultImg yet
        // if inst == else do sample matching and write resultImg and resultImg header to memory
        private void ReadHeader(object sender, System.Timers.ElapsedEventArgs e)
        {

            string inst = mSharedMemoryHandler.ReadHeaderFromSharedMemory(
                out mROIRange,
                out mRotationValue,
                out mResizeValue,
                out mMinThreshold,
                out mOffsetX,
                out mOffsetY,
                out mContinueModify
                );
            Console.WriteLine("inst: {0}\n", inst);
            Console.WriteLine("Similarity Threshold: " + mMinThreshold.ToString() + "\n");
            if (inst != "1")
            {
                DoInstruction(inst);
            }
            
        }
        private void ReadHeader()
        {
            string inst = mSharedMemoryHandler.ReadHeaderFromSharedMemory(
                out mROIRange,
                out mRotationValue,
                out mResizeValue,
                out mMinThreshold,
                out mOffsetX,
                out mOffsetY,
                out mContinueModify
                );
            //Console.WriteLine("inst: {0}\n", inst);
            Console.WriteLine("Similarity Threshold: " + mMinThreshold.ToString() + "\n");
            if (inst != "1")
            {
                DoInstruction(inst);
            }
            else
            {
                Console.WriteLine("inst is 1");
                Console.ReadKey();
            }
        }
        public void SampleMatching(Image<Bgr, byte> sampleImg, Image<Bgr, byte> targetImg)
        {
            double[] minValue = { 0 };
            double[] maxValue = { 0 };
            Point[] minPoint = null;
            Point[] maxPoint = null;
            Image<Gray, float> result = sampleImg.MatchTemplate(targetImg, TemplateMatchingType.CcoeffNormed);
            //find the most similary pattern
            result.MinMax(out minValue, out maxValue, out minPoint, out maxPoint);
            Console.WriteLine("Maxmimum position: " + maxPoint[0].ToString() + "\n");
            Console.WriteLine("Maximum Similarity: " + maxValue[0].ToString() + "\n");

            //circle every pattern which similarity is > mMinThreshold
            for (int i = 0; i < result.Rows; i++)
            {
                for (int j = 0; j < result.Cols; j++)
                {
                    if (result.Data[i, j, 0] > mMinThreshold)
                    {
                        CvInvoke.Rectangle(mResultImg, new Rectangle(new Point(j, i), targetImg.Size)
                        , new MCvScalar(255, 0, 0), 1);
                    }
                }
            }

            //CvInvoke.Imshow("Result", mResultImg);
            //CvInvoke.WaitKey(0);
            //CvInvoke.DestroyAllWindows();
        }

        public void DoInstruction(string inst)
        {
            long matchTime;
            mSharedMemoryHandler.ReadImgFromSharedMemory(out mSampleImg, out mTargetImg);
            
            if(mContinueModify == 0)//first time call this function with sharedMemoryResult empty
            {
                mResultImg = mSampleImg.Clone();
            }
            else
            {
                mSharedMemoryHandler.ReadResultImg(out mResultImg);
            }
            //SampleMatching(mSampleImg, mTargetImg);
            bool haveHomopraghy = true;
            //modify sample ROI
            mSampleImg.ROI = mROIRange;
            Mat sampleImgMat = mSampleImg.Mat;
            //draw the rectangle area along the target position on result image
            //if still have homography, fill the rectangle on the sampleImgMat and match it again
            while (haveHomopraghy)
            {
                Rectangle targetArea = new Rectangle();
                
                haveHomopraghy = Draw(mTargetImg.Mat, sampleImgMat, out matchTime, out targetArea, mMinThreshold);
                //CvInvoke.Rectangle(, targetArea, new MCvScalar(255, 255, 255), -1);
                Mat test = sampleImgMat.Clone();
                Image<Bgr, byte> img = test.ToImage<Bgr, byte>();
                img.Draw(targetArea, new Bgr(255,255,255), -1);
                sampleImgMat = img.Mat;
                //move target area to the correct place accoring to ROI value
                targetArea = new Rectangle(targetArea.X + mROIRange.X, targetArea.Y + mROIRange.Y, targetArea.Width, targetArea.Height);
                //change the color to identify different target
                if (mContinueModify == 0)
                {
                    CvInvoke.Rectangle(mResultImg, targetArea, new MCvScalar(255, 0, 0), 5);
                }
                else if(mContinueModify == 1)
                {
                    CvInvoke.Rectangle(mResultImg, targetArea, new MCvScalar(0, 255, 0), 5);
                }
                else if(mContinueModify == 2)
                {
                    CvInvoke.Rectangle(mResultImg, targetArea, new MCvScalar(0, 0, 255), 5);
                }
                else if(mContinueModify == 3)
                {
                    CvInvoke.Rectangle(mResultImg, targetArea, new MCvScalar(0, 0, 0), 5);
                }
                else
                {
                    CvInvoke.Rectangle(mResultImg, targetArea, new MCvScalar(255, 255, 0), 5);
                }
                //CvInvoke.Imshow("test", mResultImg);
                //CvInvoke.WaitKey(0);
            }
             
            //Console.WriteLine("Matching Time: " + matchTime.ToString() + "\n");
            //mResultImg = resultMat.ToImage<Bgr, byte>();
            
            string info = "1," + mResultImg.Width + ","
                + mResultImg.Height + ",";
            mSharedMemoryHandler.WriteToSharedMemory(mResultImg, info);
            
        }
        
        private void ModifyTargetImg(string inst)
        {
            //modify ROI
            mSampleImg.ROI = mROIRange;
            Image<Bgr, byte>[] ImageArray = new Image<Bgr, byte>[360];
            for (int i = 0; i < mRotationValue; i++)
            {
                ImageArray[i] = mSampleImg.Rotate(i, new Bgr(0, 0, 0), true);
            }
        }

        public static void FindMatch(Mat modelImage, Mat observedImage, out long matchTime, out VectorOfKeyPoint modelKeyPoints, out VectorOfKeyPoint observedKeyPoints, VectorOfVectorOfDMatch matches, out Mat mask, out Mat homography, double threshold)
        {
            int k = 2;
            double uniquenessThreshold = threshold;

            Stopwatch watch;
            homography = null;

            modelKeyPoints = new VectorOfKeyPoint();
            observedKeyPoints = new VectorOfKeyPoint();

            using (UMat uModelImage = modelImage.GetUMat(AccessType.Read))
            using (UMat uObservedImage = observedImage.GetUMat(AccessType.Read))
            {
                KAZE featureDetector = new KAZE();

                //extract features from the object image
                Mat modelDescriptors = new Mat();
                featureDetector.DetectAndCompute(uModelImage, null, modelKeyPoints, modelDescriptors, false);

                watch = Stopwatch.StartNew();

                // extract features from the observed image
                Mat observedDescriptors = new Mat();
                featureDetector.DetectAndCompute(uObservedImage, null, observedKeyPoints, observedDescriptors, false);

                // Bruteforce, slower but more accurate
                // You can use KDTree for faster matching with slight loss in accuracy
                using (Emgu.CV.Flann.LinearIndexParams ip = new Emgu.CV.Flann.LinearIndexParams())
                using (Emgu.CV.Flann.SearchParams sp = new SearchParams())
                using (DescriptorMatcher matcher = new FlannBasedMatcher(ip, sp))
                {
                    matcher.Add(modelDescriptors);

                    matcher.KnnMatch(observedDescriptors, matches, k, null);
                    mask = new Mat(matches.Size, 1, DepthType.Cv8U, 1);
                    mask.SetTo(new MCvScalar(255));
                    Features2DToolbox.VoteForUniqueness(matches, uniquenessThreshold, mask);

                    int nonZeroCount = CvInvoke.CountNonZero(mask);
                    if (nonZeroCount >= 4)
                    {
                        nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(modelKeyPoints, observedKeyPoints,
                        matches, mask, 1.5, 20);
                        if (nonZeroCount >= 4)
                            homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(modelKeyPoints,
                            observedKeyPoints, matches, mask, 2);
                    }
                }
                watch.Stop();

            }
            matchTime = watch.ElapsedMilliseconds;
        }

        public static bool Draw(Mat modelImage, Mat observedImage, out long matchTime, out Rectangle targetPos, double threshold)
        {
            Mat homography;
            VectorOfKeyPoint modelKeyPoints;
            VectorOfKeyPoint observedKeyPoints;
            
            using (VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch())
            {
                Mat mask;
                FindMatch(modelImage, observedImage, out matchTime, out modelKeyPoints, out observedKeyPoints, matches,
                out mask, out homography, threshold);
                bool haveHomography = false;
                //Draw the matched keypoints
                Mat result = new Mat();
                Features2DToolbox.DrawMatches(modelImage, modelKeyPoints, observedImage, observedKeyPoints,
                matches, result, new MCvScalar(255, 255, 255), new MCvScalar(0, 0, 0), mask);


                #region draw the projected region on the image
                Mat img = new Mat();
                if (homography != null)
                {
                    haveHomography = true;
                    //draw a rectangle along the projected model
                    Rectangle rect = new Rectangle(Point.Empty, modelImage.Size);
                    PointF[] pts = new PointF[]
                    {
                     new PointF(rect.Left, rect.Bottom),
                     new PointF(rect.Right, rect.Bottom),
                     new PointF(rect.Right, rect.Top),
                     new PointF(rect.Left, rect.Top)
                    };
                    pts = CvInvoke.PerspectiveTransform(pts, homography);


                    Point[] points = Array.ConvertAll<PointF, Point>(pts, Point.Round);

                    
                    Rectangle targetArea = new Rectangle(
                        points[0].X,
                        points[0].Y,
                        points[2].X - points[0].X,
                        points[2].Y - points[0].Y);
                    img = observedImage.Clone();
                    using (VectorOfPoint vp = new VectorOfPoint(points))
                    {
                        CvInvoke.Polylines(result, vp, true, new MCvScalar(255, 0, 0, 255), 5);
                    }
                    //CvInvoke.Imshow("re", result);
                    //CvInvoke.WaitKey(0);
                    targetPos = targetArea;
                }
                else
                {
                    targetPos = new Rectangle();
                }
                #endregion

                //return img;
                return haveHomography;
            }
        }

    }
}
