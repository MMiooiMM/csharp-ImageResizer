using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace ImageResizer
{
    public class ImageProcess
    {
        /// <summary>
        /// 清空目的目錄下的所有檔案與目錄
        /// </summary>
        /// <param name="destPath">目錄路徑</param>
        public void Clean(string destPath)
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }
            else
            {
                var allImageFiles = Directory.GetFiles(destPath, "*", SearchOption.AllDirectories);

                foreach (var item in allImageFiles)
                {
                    File.Delete(item);
                }
            }
        }

        /// <summary>
        /// 進行圖片的縮放作業
        /// </summary>
        /// <param name="sourcePath">圖片來源目錄路徑</param>
        /// <param name="destPath">產生圖片目的目錄路徑</param>
        /// <param name="scale">縮放比例</param>
        public async Task ResizeImages(string sourcePath, string destPath, double scale)
        {
            var allFiles = FindImages(sourcePath);

            Task[] tasks = new Task[10];

            for (int i = 0; i < 10; i++)
            {
                tasks[i] = Task.Run(() => Dequeue(allFiles, destPath, scale));
            }

            await Task.WhenAll(tasks);
        }

        private async Task Dequeue(ConcurrentQueue<string> allFiles, string destPath, double scale)
        {
            while (allFiles.TryDequeue(out string filePath))
            {
                Console.WriteLine($"{filePath} : {Thread.CurrentThread.ManagedThreadId}");
                Image imgPhoto = Image.FromFile(filePath);
                string imgName = Path.GetFileNameWithoutExtension(filePath);

                int sourceWidth = imgPhoto.Width;
                int sourceHeight = imgPhoto.Height;

                int destionatonWidth = (int)(sourceWidth * scale);
                int destionatonHeight = (int)(sourceHeight * scale);

                Bitmap processedImage = await ProcessBitmap((Bitmap)imgPhoto,
                    sourceWidth, sourceHeight,
                    destionatonWidth, destionatonHeight);

                string destFile = Path.Combine(destPath, imgName + ".jpg");
                await ImageSave(processedImage, destFile, ImageFormat.Jpeg);
            }
        }

        /// <summary>
        /// 找出指定目錄下的圖片
        /// </summary>
        /// <param name="srcPath">圖片來源目錄路徑</param>
        /// <returns></returns>
        public ConcurrentQueue<string> FindImages(string srcPath)
        {
            ConcurrentQueue<string> files = new ConcurrentQueue<string>();
            foreach (var item in Directory.GetFiles(srcPath, "*.png", SearchOption.AllDirectories))
            {
                files.Enqueue(item);
            }
            foreach (var item in Directory.GetFiles(srcPath, "*.jpg", SearchOption.AllDirectories))
            {
                files.Enqueue(item);
            }
            foreach (var item in Directory.GetFiles(srcPath, "*.jpeg", SearchOption.AllDirectories))
            {
                files.Enqueue(item);
            }
            return files;
        }

        /// <summary>
        /// 針對指定圖片進行縮放作業
        /// </summary>
        /// <param name="img">圖片來源</param>
        /// <param name="srcWidth">原始寬度</param>
        /// <param name="srcHeight">原始高度</param>
        /// <param name="newWidth">新圖片的寬度</param>
        /// <param name="newHeight">新圖片的高度</param>
        /// <returns></returns>
        private async Task<Bitmap> ProcessBitmap(Bitmap img, int srcWidth, int srcHeight, int newWidth, int newHeight)
        {
            return await Task.Run(() =>
            {
                Bitmap resizedbitmap = new Bitmap(newWidth, newHeight);
                Graphics g = Graphics.FromImage(resizedbitmap);
                g.InterpolationMode = InterpolationMode.High;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.Clear(Color.Transparent);
                g.DrawImage(img,
                    new Rectangle(0, 0, newWidth, newHeight),
                    new Rectangle(0, 0, srcWidth, srcHeight),
                    GraphicsUnit.Pixel);
                return resizedbitmap;
            });
        }

        /// <summary>
        /// 儲存圖片
        /// </summary>
        /// <param name="bitmap">二進位圖檔</param>
        /// <param name="destFile">目標位置</param>
        /// <param name="imageFormat">圖片類型</param>
        /// <returns></returns>
        private async Task ImageSave(Bitmap bitmap, string destFile, ImageFormat imageFormat)
        {
            await Task.Run(() => {
                bitmap.Save(destFile, imageFormat);
            });
        }
    }
}