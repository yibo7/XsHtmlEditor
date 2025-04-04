﻿using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using NHotkey;
using NHotkey.WindowsForms;
using System.Diagnostics;
namespace XsHtmlEditor
{
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
            _ = InitWebView2Async();
        }

        private async Task<bool> CheckWebView2Runtime()
        {
            try
            {
                // 尝试创建 CoreWebView2Environment，这是检测运行时是否存在的更轻量级方法
                await CoreWebView2Environment.CreateAsync();
                return true; // 如果能成功创建环境，则运行时很可能存在
            }
            catch (WebView2RuntimeNotFoundException)
            {
                // 明确捕获到未找到运行时的异常
                return false;
            }
            catch (Exception ex)
            {
                // 捕获其他初始化或环境创建过程中可能发生的异常
                Console.WriteLine($"检查 WebView2 运行时时发生错误: {ex.Message}");
                // 您可以根据具体情况决定是否将这些情况也视为运行时不存在
                // 或者返回 false 并记录/显示更详细的错误信息
                return false;
            }
        }


        private async Task InitWebView2Async()
        {
            // 获取 bin 目录下的 WebView2Runtime 文件夹路径
            string runtimePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WebView2Runtime");
            if (Path.Exists(runtimePath))
            {
                var options = new CoreWebView2EnvironmentOptions();
                var environment = await CoreWebView2Environment.CreateAsync(runtimePath, null, options);

                // 使用自定义环境初始化 WebView2
                await webView2.EnsureCoreWebView2Async(environment);
            }
            else
            {
                if (await CheckWebView2Runtime())
                {
                    await webView2.EnsureCoreWebView2Async();
                }
                else
                {
                    DialogResult result = MessageBox.Show(
                        "您需要安装 WebView2 运行时，或将 WebView2 运行时复制到当前程序的 WebView2Runtime 目录下。\n点击【确定】打开下载页面。",
                        "缺少 WebView2 运行时",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Warning
                    );

                    if (result == DialogResult.OK)
                    {
                        try
                        {
                            // 打开 WebView2 运行时下载页面
                            Process.Start(new ProcessStartInfo("https://developer.microsoft.com/microsoft-edge/webview2/") { UseShellExecute = true });
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"无法打开下载页面: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                
            }   
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
            //string img = $"<p class='edui-image-item edui-image-upload-item'><div class='edui-image-close'></div><img src='{src}' class='edui-image-pic' {sWidth}/></p>";
            string img = $"<p class='edui-image-item edui-image-upload-item'><img src='{src}' class='edui-image-pic' {sWidth}/></p>";
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
            //await webView2.CoreWebView2.ExecuteScriptAsync(script);

            if (InvokeRequired) // 是否在线程安全上运行
            {
                this.Invoke((Delegate)(() =>
                {
                    webView2.CoreWebView2.ExecuteScriptAsync(script).GetAwaiter().GetResult();
                }));
            }
            else
            {
                await webView2.CoreWebView2.ExecuteScriptAsync(script);
            }

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

    public class HtmlContent
    {
        public HtmlContent(string data)
        {
            Data = data;
        }
        public string Data { get; set; }
    }
}