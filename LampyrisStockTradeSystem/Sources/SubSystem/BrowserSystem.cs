/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 模拟浏览器进行操作的实现类，方便实现 股票网页交易自动下单，或者爬取网页上某些数据的功能
*/

namespace LampyrisStockTradeSystem;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;


/// <summary>
/// 模拟浏览器进行操作的实现类，可以拥有多个实例，每个实例的Cookie互不共享
/// </summary>
public class BrowserSystem
{
    private ChromeDriver m_chromeDriver = null;

    /// <summary>
    /// 1
    /// </summary>
    public void Init()
    {
        ChromeOptions Options = new ChromeOptions();
        Options.AddArgument("--headless"); // 设置为Headless模式

        // 这里指定Chrome.exe和ChromeDriver.exe的位置
        Options.BinaryLocation = AppSettings.Instance.chromeProgramPath;
        ChromeDriverService Service = ChromeDriverService.CreateDefaultService(AppSettings.Instance.chromeDriverProgramPath);

        // 是否隐藏DOS窗口
        Service.HideCommandPromptWindow = true; 
        m_chromeDriver = new ChromeDriver(options: Options, service: Service);
        m_chromeDriver.Manage().Window.Size = new Size(1200, 1200);
    }

    public void Close()
    {
        m_chromeDriver.Quit();
    }

    public void Request(string url)
    {
        m_chromeDriver.Url = url;
    }

    public string GetCurrentUrl()
    {
        return m_chromeDriver.Url;
    }

    public void Click(By by)
    {
        m_chromeDriver?.FindElement(by)?.Click();
    }

    public void Input(By by,string input)
    {
        m_chromeDriver?.FindElement(by)?.SendKeys(input);
    }

    public void SaveImg(By by,string savePath,bool isFromSrc = true)
    {
        IWebElement webElement = m_chromeDriver.FindElement(by);
        if(!isFromSrc)
        {
            Point location = webElement.Location;
            Size size = webElement.Size;

            string tempDir = Path.Combine(Path.GetTempPath(), "LampyrisStockTradeSystem");
            if(!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            string tempScreenshotPath = Path.Combine(Path.GetTempPath(), "LampyrisStockTradeSystem/tempScreenshot.png");

            Screenshot screenShot = m_chromeDriver.GetScreenshot();
            screenShot.SaveAsFile(tempScreenshotPath);

            // 打开图片
            using (Bitmap original = new Bitmap(tempScreenshotPath))
            {
                // 定义截取区域（这里截取原图片的左上角100x100像素的区域）
                Rectangle section = new Rectangle(location, size);

                // 截取图片
                using (Bitmap cropped = original.Clone(section, original.PixelFormat))
                {
                    // 保存截取的图片
                    cropped.Save(savePath, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
        }
        else
        {
            // 获取图片的URL
            string imageUrl = webElement.GetAttribute("src");

            // 打印图片的URL
            Console.WriteLine(imageUrl);

            // 同步下载图片
            HttpClient httpClient = new HttpClient();
            byte[] imageBytes = httpClient.GetByteArrayAsync(imageUrl).Result;

            // 写入文件
            File.WriteAllBytes(savePath, imageBytes);
        }
    }
}
