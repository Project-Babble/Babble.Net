//using Babble.Core;
//using Microsoft.Maui.Graphics.Platform;

//namespace Babble.Maui;

//public class MouthDrawing : IDrawable
//{
//    public void Draw(ICanvas canvas, RectF dirtyRect)
//    {
//        if (BabbleCore.Instance.LipImage != null)
//        {
//            var image = ImageSource.FromStream(() => new MemoryStream(BabbleCore.Instance.LipImage));
//            // var image = PlatformImage.FromStream();
//            if (image != null)
//            {
//                canvas.DrawImage(image, 0, 0, 256, 256);
//            }
//        }
//    }

//    private MemoryStream ConvertBitmap(byte[] imageData, int width = 256, int height = 256)
//    {
//        byte[] newData = new byte[imageData.Length];

//        for (int x = 0; x < imageData.Length; x += 4)
//        {
//            byte[] pixel = new byte[4];
//            Array.Copy(imageData, x, pixel, 0, 4);

//            byte r = pixel[0];
//            byte g = pixel[1];
//            byte b = pixel[2];
//            byte a = pixel[3];

//            byte[] newPixel = [b, g, r, a];

//            Array.Copy(newPixel, 0, newData, x, 4);
//        }

//        return new MemoryStream(newData);
//    }
//}