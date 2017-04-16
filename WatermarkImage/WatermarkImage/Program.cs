using System;
using System.Collections.Generic;
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
            var watermark = args?[0];
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
        }

        private static void ProcessFiles(String watermarkFile, IEnumerable<String> files, String targetPath)
        {
            Console.WriteLine($"Watermark file: {watermarkFile}");
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }
            var watermark = new Bitmap(watermarkFile);
            foreach (var file in files)
            {
                Console.WriteLine($"Watermarking started: {file}");
                var img = new Bitmap(file);

                var destination = new Bitmap(img.Width, img.Height, PixelFormat.Format32bppArgb);
                using (var graphics = Graphics.FromImage(destination))
                {
                    graphics.CompositingMode = CompositingMode.SourceOver; // this is the default, but just to be clear

                    var destinationRect = new Rectangle(0, 0, destination.Width, destination.Height);
                    graphics.DrawImage(img, destinationRect);
                    graphics.DrawImage(watermark, destinationRect);

                    var targetFilename = Path.Combine(targetPath, Path.GetFileName(file));
                    //target.Save(targetFilename, GetImageFormatFromExtension(Path.GetExtension(file)));
                    Console.WriteLine($"Target file: {targetFilename}");
                    destination.Save(targetFilename, ImageFormat.Png);
                    Console.WriteLine($"File created");
                }
            }
        }

        private static ImageFormat GetImageFormatFromExtension(String ext)
        {
            switch (ext.ToLower())
            {
                case ".jpg":
                case ".jpeg":
                    return ImageFormat.Jpeg;
                case ".png":
                    return ImageFormat.Png;
                case ".gif":
                    return ImageFormat.Png;
                default:
                    throw new InvalidOperationException();
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
