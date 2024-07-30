using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XsHtmlEditor
{
    internal class Base64ImageSaver
    {
        public static void SaveBase64ImageToDisk(string base64String, string filePath)
        {
            // 移除base64字符串中的任何数据URL前缀
            if (base64String.Contains(","))
            {
                base64String = base64String.Split(',')[1];
            }

            // 将base64字符串转换为字节数组
            byte[] imageBytes = Convert.FromBase64String(base64String);

            // 使用字节数组创建内存流
            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                // 从内存流中创建图像对象
                using (Image image = Image.FromStream(ms))
                {
                    // 保存图像到文件
                    image.Save(filePath, ImageFormat.Png); // 根据需要选择图像格式
                }
            }
        }
    }
}
