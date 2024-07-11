using OpenCvSharp;
using System.Drawing.Imaging;
using System.Drawing;

namespace fTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            ExtractFrames(@"badapple.mp4");
        }

        static double scaleFactor = 0.35;

        public static void ExtractFrames(string videoPath)
        {
            var video = new Video();
            using (var capture = new VideoCapture(videoPath))
            {
                if (!capture.IsOpened())
                {
                    Console.WriteLine("Failed to open video file.");
                    return;
                }

                video.FPS = capture.Fps;
                video.Frames = new Frame[capture.FrameCount];

                int frameNumber = 0;
                Mat frame = new Mat();

                while (true)
                {
                    capture.Read(frame);
                    if (frame.Empty())
                        break;

                    using (Bitmap bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frame))
                    {
                        // Calculate new dimensions
                        int newWidth = (int)(bitmap.Width * scaleFactor);
                        int newHeight = (int)(bitmap.Height * scaleFactor);

                        video.Width = newWidth;
                        video.Height = newHeight;

                        // Create a new bitmap with the new dimensions
                        using (Bitmap resizedBitmap = new Bitmap(newWidth, newHeight))
                        {
                            // Draw the original bitmap onto the new bitmap
                            using (Graphics graphics = Graphics.FromImage(resizedBitmap))
                            {
                                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                graphics.DrawImage(bitmap, 0, 0, newWidth, newHeight);
                            }
                            ConvertFrameToBoolArray(resizedBitmap, frameNumber, ref video);
                        }
                        //SaveFrameAsBitmap(bitmap, frameNumber);
                    }

                    Console.WriteLine($"{frameNumber} / {capture.FrameCount} {((float)frameNumber/(float)capture.FrameCount)*100}%");

                    frameNumber++;
                }
            }

            using (StreamWriter sw = new StreamWriter("badapple50.vid"))
            {
                var ceral = Newtonsoft.Json.JsonConvert.SerializeObject(video);
                sw.WriteLine(ceral);
            }
        }

        public class Frame
        {
            public bool[,] Pixels { get; set; }
        }

        public class Video
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public double FPS { get; set; }
            public Frame[] Frames { get; set; }
        }

        static bool[,] ConvertFrameToBoolArray(Bitmap frame, int frameNumber, ref Video video)
        {
            int width = frame.Width;
            int height = frame.Height;
            bool[,] boolArray = new bool[width, height];

            // Iterate over each pixel to create the boolean array
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixelColor = frame.GetPixel(x, y);
                    boolArray[x, y] = pixelColor.R == 255;
                }
            }

            video.Frames[frameNumber] = new Frame();
            //video.Frames[frameNumber].Pixels = new bool[width,height];
            video.Frames[frameNumber].Pixels = boolArray;

            return boolArray;
        }

        public static void SaveFrameAsBitmap(Bitmap frame, int frameNumber)
        {
            string framePath = $"frame_{frameNumber}.bmp";
            frame.Save(framePath, ImageFormat.Bmp);
            Console.WriteLine($"Saved frame {frameNumber} as {framePath}");
        }
    }
}
