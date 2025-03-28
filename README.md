# XsHtmlEditor
Rich Text Editor based on .NET Core WinForms

> 前提条件：需要在客户机安装WebView2运行时，或在当前程序的发布目录下创建目录WebView2Runtime，存放WebView2的固定版本运行时。
> WebView2的下载地址：https://developer.microsoft.com/en-us/microsoft-edge/webview2

![File](images/file.png)
# API：
### 1、Set editor content
```cs
htmlEditor.SetHtmlValue(model.news_content);
```
If you want to set content when the program is initialized, use it like this:
```cs
htmlEditor.OnInited += () =>
            {
                htmlEditor.SetHtmlValue(model.news_content);
            };
```

### 2、Get editor content
```cs
string html = await htmlEditor.GetHtmlValue();
```
You can also only get the text information:
```cs
string text = await htmlEditor.GetTextValue();
```
### 3、Insert HTML content at the cursor position
```cs
htmlEditor.InsertHtml("html code");
```
### 4、Paste Base64 image at the cursor position in the editor
```cs
htmlEditor.PasteImgBase64();
```
You can use the shortcut Shift+V. 
If this shortcut is already in use, please modify the default shortcut in the code.

### 5、Paste images at the cursor position in the editor and submit them to the server
```cs
htmlEditor.ServerUrl = "https://github.com/yibo7/XsHtmlEditor";
```
You can use the shortcut Ctrl+V.
ServerUrl is your own service.







