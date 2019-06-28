﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ImageResizer
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            string sourcePath = Path.Combine(Environment.CurrentDirectory, "images");
            string destinationPath = Path.Combine(Environment.CurrentDirectory, "output"); ;

            ImageProcess imageProcess = new ImageProcess();

            imageProcess.Clean(destinationPath);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            imageProcess.ResizeImages(sourcePath, destinationPath, 2.0);
            sw.Stop();

            Console.WriteLine($"花費時間: {sw.ElapsedMilliseconds} ms");
            // 調整前 3085ms
            // 調整後 2325ms
            // ~75%

            // 調整前 5860ms
            // 調整後 4100ms
            // ~70%
        }
    }
}