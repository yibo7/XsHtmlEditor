using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using NHotkey;
using NHotkey.WindowsForms; 
namespace XsHtmlEditor
{
    public class HtmlContent
    {
        public HtmlContent(string data) {
            Data = data;
        }
        public string Data { get; set; }
    }
    public partial class HtmlEditor : UserControl
    {
        private static readonly Keys IncrementKeys = Keys.Shift |Keys.Alt| Keys.V;
        /// <summary>
        /// 提交粘贴图片时调用，并提交图片流
        /// </summary>
        public string ServerUrl { get; set; }   
        public HtmlEditor()
        {
            InitializeComponent(); 

            webView2.NavigationCompleted += Wv2Ctrl_NavigationCompleted;
            webView2.CoreWebView2InitializationCompleted += WebView2_CoreWebView2InitializationCompleted; 
            webView2.EnsureCoreWebView2Async();

            

        }
        /// <summary>
        /// 启动粘贴base64图片的热键
        /// </summary>
        public void EnablePasteImgHotKey()
        {
            try
            {
                HotkeyManager.Current.Remove("IncrementName");
                HotkeyManager.Current.AddOrReplace("IncrementName", IncrementKeys, OnIncrement);

            }
            catch (Exception ex)
            {
                MessageBox.Show("粘贴Base64图片快捷键冲突！" + ex.Message);
            }
        }
        private void OnIncrement(object? sender, HotkeyEventArgs e)
        {
            PasteImgBase64();
        }

        public Action OnInited;

        //public async Task AwaitInitComplate()
        //{
        //    // 等待控件初始化完成
        //    await webView2.EnsureCoreWebView2Async();
        //}

        private void WebView2_CoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            //webView2.CoreWebView2.Navigate("https://www.baidu.com");

            var url = new Uri(@"file:///" + Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"umeditor/xs_index/index.html").Replace('\\', '/'));
            webView2.Source = url;

            webView2.WebMessageReceived += (object? sender, CoreWebView2WebMessageReceivedEventArgs e) =>
            {
                string message = e.TryGetWebMessageAsString();
                 
                if (message == "UpLocImg")
                {
                    var lst = OpenSelFiles("选择图片|*.bmp;*.jpg;*.jpeg; *.png; *.gif;");
                    foreach (var item in lst)
                    {
                        InserImg(item,600); 
                    }
                }else if (message.StartsWith("data:image/png;base64"))
                {
                    // 使用示例 
                    string base64Image = message; // 这里假设message是一个包含图片的Base64字符串
                    string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), @"umeditor\base64images"); // 获取当前目录下的htmlboximages路径
                    string fileName = DateTime.Now.Ticks.ToString() + ".png"; // 使用时间戳作为文件名
                    string filePath = Path.Combine(directoryPath, fileName); // 完整的文件路径

                    // 检查目录是否存在，如果不存在则创建
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    Base64ImageSaver.SaveBase64ImageToDisk(message, filePath);
                    InserImg(filePath);
                }
            };

        }

        private void InserImg(string src,int width=0)
        {
            string sWidth = "";
            if (width > 0)
            {
                sWidth = $"style='width: {width}px; '";
            }
            string img = $"<div class='edui-image-item edui-image-upload-item'><div class='edui-image-close'></div><img src='{src}' class='edui-image-pic' {sWidth}/></div>";
            InsertHtml(img);
        }
        
        /// <summary>
        /// 打开选择文件的窗口
        /// </summary>
        /// <param name="Filter">筛选项，比如 选择视频|*.mp4;*.mov;*.mkv;*.avi;*.flv;*.mpeg;*.ogg;*.vob;*.webm;*.wmv;*.rmvb;</param>
        /// <param name="IsMultiselect"></param>
        /// <returns></returns>
        private string[] OpenSelFiles(string Filter = "All files (*.*)|*.*", bool IsMultiselect = true)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = IsMultiselect;
            fileDialog.Title = "请选择文件";
            fileDialog.Filter = Filter;
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                //string file = fileDialog.FileName;
                return fileDialog.FileNames;
            }
            return new string[] { };
        }
        /// <summary>
        /// 获取文本框html
        /// </summary>
        /// <returns></returns>
        async public Task<string> GetHtmlValue()
        {
            string script = "getContent()";
            string content = await webView2.CoreWebView2.ExecuteScriptAsync(script);
            content = System.Text.RegularExpressions.Regex.Unescape(content);
            //string escaped = System.Text.RegularExpressions.Regex.Escape(input);
            return content.Trim('"');
        }

        /// <summary>
        /// 设置文本框内容
        /// </summary>
        /// <param name="html"></param>
        async public void SetHtmlValue(string html)
        {

            HtmlContent data = new HtmlContent(html);

            string content = JsonConvert.SerializeObject(data, Formatting.Indented);
           
            string script = $"setContent({content},false)";
            await webView2.CoreWebView2.ExecuteScriptAsync(script);

        }
        /// <summary>
        /// 向光标所在位置插入代码
        /// </summary>
        /// <param name="html"></param>
        async public void InsertHtml(string html)
        {

            HtmlContent data = new HtmlContent(html);

            string content = JsonConvert.SerializeObject(data, Formatting.Indented);

            string script = $"insertHtml({content})";
            await webView2.CoreWebView2.ExecuteScriptAsync(script);

        }

        /// <summary>
        /// 向光标位置粘贴图片
        /// </summary>
        /// <param name="html"></param>
        public void PasteImgBase64()
        {
            string base64String = ClipboardHelper.GetClipboardImageAsBase64();
            if (!string.IsNullOrEmpty(base64String))
            {
                InsertHtml($"<img src=\"data:image/png;base64,{base64String}\" alt=\"xs\">");
            }

        }



        /// <summary>
        /// 获取带有换行格式类的人本
        /// </summary>
        /// <returns></returns>
        async public Task<string> GetTextValue()
        {
            string script = "getPlainTxt()";
            string content = await webView2.CoreWebView2.ExecuteScriptAsync(script);
            return content;
        }
        /// <summary>
        /// 获取纯文本
        /// </summary>
        /// <returns></returns>
        async public Task<string> GetText()
        {
            string script = "getContentTxt()";
            string content = await webView2.CoreWebView2.ExecuteScriptAsync(script);
            return content;
        }


        /// <summary>
        /// 当网页加载完成后触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void Wv2Ctrl_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            //SetHtmlValue("");  
            if (OnInited != null)
            {
                OnInited();
            }
        }
    }
}