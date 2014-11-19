using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
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
        private StorageFile _file;
        public ImageHelper()
            {
                
            }
        public async Task CreateSaveBitmapAsync(FrameworkElement canvas)
        {
            var renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(canvas);

            var picker = new FileSavePicker();
            picker.FileTypeChoices.Add("JPEG Image", new[] { ".jpg" });
            var file = await picker.PickSaveFileAsync();
            if (file != null)
            {
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
                _file = file;
            }

        }
        public double getWidthForImage(double widthh, int type)
        {
            var scale = 0;
            var width = widthh;
            while ((width / 1.1) > (type == 1 ? Window.Current.Bounds.Height - 300 : 600))
            {
                scale++;
                width = width / 1.1;
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
