
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.DevTools.V85.Browser;
using System.Drawing;
using System.Security;
using Tesseract;
using System;
using System.Diagnostics.Metrics;

namespace WorkerService1
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (Bitmap bitmap = new Bitmap(@"C:\Users\Arad\Desktop\Telerik.Web.UI.WebResource.png"))
            {
                bool[,] visited = new bool[bitmap.Width, bitmap.Height];

                // Iterate over each pixel in the image
                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        if (!visited[x, y] && !IsWhitePixel(bitmap.GetPixel(x, y)))
                        {
                            int dotSize = GetDotSize(bitmap, x, y, visited);
                            if (dotSize <= 20) // Maximum area for a 3x3 region is 9 pixels
                            {
                                RemoveDot(bitmap, x, y, visited);
                            }
                        }
                    }
                }

                // Save the modified image to a new file
                bitmap.Save(@"C:\Users\Arad\Desktop\ModifiedImage.png");

                Console.WriteLine("Small dots removed. Image saved successfully.");
            }


            while (true)
            {
                using var engine = new TesseractEngine(@"D:\data", "eng", EngineMode.Default);
                using var img = Pix.LoadFromFile(@"C:\Users\Arad\Desktop\ModifiedImage.png");
                using var page = engine.Process(img);
                string text = page.GetText();
                Console.WriteLine("Text: " + "," + text + ",");
                Console.ReadKey();
            }
        }

        static bool IsWhitePixel(Color color)
        {
            // Define what you consider as "white"
            return color.R > 220 && color.G > 100 && color.B > 100;
        }

        static int GetDotSize(Bitmap bitmap, int startX, int startY, bool[,] visited)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            int dotSize = 0;

            // Use a queue for breadth-first search (BFS)
            Queue<(int x, int y)> queue = new Queue<(int x, int y)>();
            queue.Enqueue((startX, startY));
            visited[startX, startY] = true;

            while (queue.Count > 0)
            {
                var (x, y) = queue.Dequeue();
                dotSize++;

                // Explore 4-connected neighbors (up, down, left, right)
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (Math.Abs(i) == Math.Abs(j)) continue;  // Skip diagonals

                        int newX = x + i;
                        int newY = y + j;

                        if (newX >= 0 && newX < width && newY >= 0 && newY < height &&
                            !visited[newX, newY] && !IsWhitePixel(bitmap.GetPixel(newX, newY)))
                        {
                            queue.Enqueue((newX, newY));
                            visited[newX, newY] = true;
                        }
                    }
                }
            }

            return dotSize;
        }

        static void RemoveDot(Bitmap bitmap, int startX, int startY, bool[,] visited)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;

            Queue<(int x, int y)> queue = new Queue<(int x, int y)>();
            queue.Enqueue((startX, startY));

            while (queue.Count > 0)
            {
                var (x, y) = queue.Dequeue();
                bitmap.SetPixel(x, y, Color.White);

                // Explore 4-connected neighbors (up, down, left, right)
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (Math.Abs(i) == Math.Abs(j)) continue;  // Skip diagonals

                        int newX = x + i;
                        int newY = y + j;

                        if (newX >= 0 && newX < width && newY >= 0 && newY < height &&
                            visited[newX, newY])
                        {
                            queue.Enqueue((newX, newY));
                            visited[newX, newY] = false;  // Mark as "processed"
                        }
                    }
                }
            }
        }

    }
}
