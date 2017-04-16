using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace WatermarkImage
{
    internal class Program
    {
        private static void Main(String[] args)
        {
            var watermark = args.Length == 0 ? null : args?[0];
            if (String.IsNullOrEmpty(watermark) || watermark.ToLower() == "help" || watermark == "?")
            {
                ShowHelp();
            }
            else
            {
                if (args.Length != 3)
                {
                    ShowError("invalid number of arguments");
                    ShowHelp();
                }
                else
                {
                    var path = args[1];
                    var targetPath = args[2];
                    try
                    {
                        ProcessFiles(watermark, GetFiles(path), targetPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occured: {ex.Message}");
                        Console.WriteLine($"Stacktrace:\n{ex.StackTrace}");
                    }
                }
            }
            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        private static void ProcessFiles(String watermarkFile, IEnumerable<String> files, String targetPath)
        {
            Console.WriteLine($"Watermark file: {watermarkFile}");
            var watermark = new Bitmap(watermarkFile);
            foreach (var file in files)
            {
                var sourceFilePath = Path.GetDirectoryName(file);
                var fileName = Path.GetFileName(file);
                var targetDirectory = Path.Combine(sourceFilePath, targetPath);
                var targetFilePath = Path.Combine(targetDirectory, fileName);
                EnsureDirectoryExists(targetDirectory);
                ProcessFile(file, watermark, targetFilePath);
            }
        }

        private static void ProcessFile(String file, Bitmap watermark, String targetFilename)
        {
            Console.WriteLine($"Watermarking started: {file}");
            var img = new Bitmap(file);

            var destination = new Bitmap(img.Width, img.Height, PixelFormat.Format32bppArgb);
            using (var graphics = Graphics.FromImage(destination))
            {
                graphics.CompositingMode = CompositingMode.SourceOver;

                var destinationRect = new Rectangle(0, 0, destination.Width, destination.Height);
                graphics.DrawImage(img, destinationRect);
                graphics.DrawImage(watermark, destinationRect);
                Console.WriteLine($"Target file: {targetFilename}");
                destination.Save(targetFilename, ImageFormat.Png);
                Console.WriteLine($"File created");
            }
        }

        private static void EnsureDirectoryExists(String targetPath)
        {
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }
        }

        private static IEnumerable<String> GetFiles(String path)
        {
            var extensions = new[] { ".png", ".jpg", ".jpeg", ".gif" };
            if (Directory.Exists(path))
            {
                Console.WriteLine("Got a directory as source");
                foreach (var file in Directory.GetFiles(path))
                {
                    var ext = Path.GetExtension(file)?.ToLower();
                    if (extensions.Any(x => x == ext))
                    {
                        yield return file;
                    }
                }
            }
            else if (File.Exists(path))
            {
                Console.WriteLine("Got a single file as source");
                var ext = new FileInfo(path).Extension.ToLower();
                if (extensions.Any(x => x == ext))
                {
                    yield return path;
                }
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine(@"Help
WatermarkImage.exe WatermarkImagePath SourcePath TargetPath");
        }

        private static void ShowError(String message)
        {
            Console.WriteLine(message);
        }
    }
}
