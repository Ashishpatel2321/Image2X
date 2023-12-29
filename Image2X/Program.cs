using System;
using System.Drawing;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Threading;

namespace ImageResizer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter the path of the folder:");
            string folderPath = Console.ReadLine();
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = folderPath;
            watcher.Filter = "*.*"; // Monitor all files
            watcher.EnableRaisingEvents = true;

            watcher.Created += OnFileCreated;
            Console.Clear();
            Console.WriteLine("Monitoring for new image files. Press 'q' to quit.");
            while (Console.Read() != 'q') ;
        }

        private static void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            string imagePath = e.FullPath;

            if (IsSupportedImage(imagePath) && IsResizedImage(imagePath))
            {
                // Add a delay to ensure the file is fully written
                Thread.Sleep(1000); // Adjust the delay time as needed (e.g., 1000 milliseconds)

                try
                {
                    using (Image image = Image.FromFile(imagePath))
                    {
                        int width = image.Width * 2;
                        int height = image.Height * 2;

                        using (Bitmap resizedImage = new Bitmap(width, height))
                        {
                            using (Graphics graphics = Graphics.FromImage(resizedImage))
                            {
                                // Fill the background with white color
                                graphics.Clear(Color.White);

                                // Draw the original transparent image onto the white background
                                graphics.DrawImage(image, 0, 0, width, height);
                            }

                            string newImagePath = GetUniqueResizedImagePath(imagePath);
                            resizedImage.Save(newImagePath);
                            image.Dispose();

                            // Delete the original image
                            File.Delete(imagePath);
                            Console.WriteLine($"Resized image created: {newImagePath}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing {imagePath}: {ex.Message}");
                }
            }
        }

        private static bool IsSupportedImage(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            return extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".gif" || extension == ".bmp";
        }

        private static bool IsResizedImage(string imagePath)
        {
            if (imagePath.Contains("_resized"))
                return false;
            else
                return  true;            
        }

        private static string GetUniqueResizedImagePath(string imagePath)
        {
            string newImagePath = Path.Combine(Path.GetDirectoryName(imagePath), Path.GetFileNameWithoutExtension(imagePath) + "_resized" + Path.GetExtension(imagePath));

            int count = 1;
            while (File.Exists(newImagePath))
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(imagePath);
                string extension = Path.GetExtension(imagePath);
                newImagePath = Path.Combine(Path.GetDirectoryName(imagePath), $"{fileNameWithoutExtension}_resized_{count}{extension}");
                count++;
            }

            return newImagePath;
        }
    }
}
