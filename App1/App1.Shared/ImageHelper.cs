using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace Swooper
{
    class ImageHelper
    {
        public async Task<StorageFile> CreateSaveBitmapAsync(FrameworkElement canvas)
        {
            var renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(canvas);
            var picker = new FileSavePicker
            {
                SuggestedFileName = "Swooper-" + DateTime.Now.ToString().Replace(" ","-"),
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            picker.FileTypeChoices.Add("JPEG Image", new[] { ".jpg" });
            var file = await picker.PickSaveFileAsync();
            if (file == null) return null;
            var pixels = await renderTargetBitmap.GetPixelsAsync();
            using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await
                    BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
                var bytes = pixels.ToArray();
                encoder.SetPixelData(BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Ignore,
                    (uint)canvas.Width, (uint)canvas.Height,
                    96, 96, bytes);

                await encoder.FlushAsync();
            }
            return file;
        }
        public double GetWidthForImage(double widthh, int type)
        {
            var scale = 0;
            var width = widthh;
            var h = Math.Ceiling(Window.Current.Bounds.Height/1.65);
            var w = Math.Ceiling(Window.Current.Bounds.Width/1.8);
          
            while ((width / 1.001) > (type == 1 ? h : w))
            {
                scale++;
                width = width / 1.001;
            }
            width /= 1.1;
            return scale == 0 ? 1 : widthh / width;

        }
        public async Task<byte[]> ReadFile(StorageFile file)
        {
            byte[] fileBytes;
            using (var stream = await file.OpenReadAsync())
            {
                fileBytes = new byte[stream.Size];
                using (var reader = new DataReader(stream))
                {
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(fileBytes);
                }
            }
            return fileBytes;
        }

    }
}
