using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XsHtmlEditor { 
    public static class ClipboardHelper
    {
        public static string GetClipboardImageAsBase64()
        {
            // 检查剪切板中是否包含图像
            if (!Clipboard.ContainsImage())
            {
                MessageBox.Show("剪板中没有图片");
                return "";
                //throw new InvalidOperationException("The clipboard does not contain an image.");
            }

            // 从剪切板中获取图像对象
            Image image = Clipboard.GetImage();

            // 将图像转换为字节数组
            byte[] imageBytes = ImageToBytes(image);

            // 将字节数组转换为 Base64 编码字符串
            string base64String = Convert.ToBase64String(imageBytes);

            // 返回 Base64 编码字符串
            return base64String;
        }

        private static byte[] ImageToBytes(Image image)
        {
            // 创建一个内存流对象
            using (MemoryStream stream = new MemoryStream())
            {
                // 将图像以 PNG 格式保存到内存流中
                image.Save(stream, ImageFormat.Png);

                // 返回内存流中的字节数组
                return stream.ToArray();
            }
        }
    }
}
