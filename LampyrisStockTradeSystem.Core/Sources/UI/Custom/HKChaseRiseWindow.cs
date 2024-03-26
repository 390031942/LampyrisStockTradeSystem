/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 港股追涨窗口，窗口内会实时更新，并显示符合追涨策略的股票，并提供快速响应的界面交互的功能
*/

using ImGuiNET;

namespace LampyrisStockTradeSystem;

public struct HKChaseRiseQuoteData
{
    public QuoteData quoteData;

    public Bitmap klineImage;
    public int klineTextureId;

    public Bitmap todayImage;
    public int todayImageTextureId;
}

[UniqueWidget]
public class HKChaseRiseWindow:Widget
{
    [MenuItem("港股/追涨全景图")]
    public static void ShowHKChaseRiseWindow()
    {
        WidgetManagement.GetWidget<HKChaseRiseWindow>();
    }

    /// <summary>
    /// 刷新港股通所有股票的行情
    /// </summary>
    [PlannedTask(executeTime ="09:29-16:10",executeMode = PlannedTaskExecuteMode.ExecuteDuringTime)]
    public static void RefreshHKStockQuote()
    {
        HttpRequest.Get(StockQuoteInterface.Instance.GetQuoteUrl(StockQuoteInterfaceType.HKLink), (string json) => { 
        
        });
    }

    // 假设你已经有了一个窗口创建和ImGui渲染循环的基础设施
    // 下面的方法应该在你的渲染循环中被调用

    private HttpClient httpClient = new HttpClient();
    private static List<HKChaseRiseQuoteData> m_stockData = new List<HKChaseRiseQuoteData>();

    public override void OnGUI()
    {
        ImGui.Begin("港股通追涨全景图"); // 开始一个新的ImGui窗口

        if (ImGui.BeginTable("Your Table", 3)) // 创建一个有3列的表格
        {
            ImGui.TableSetupColumn("股票名称");
            ImGui.TableSetupColumn("分时/K线图");
            ImGui.TableSetupColumn("操作按钮");
            ImGui.TableHeadersRow();

            foreach (var item in m_stockData)
            {
                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                ImGui.Text(item.quoteData.name); // 显示股票名称

                ImGui.TableNextColumn();
                if (image != null)
                {
                    IntPtr imageId = image.ToImGuiTexture(); // 这需要你实现一个扩展方法来将System.Drawing的Image转换为ImGui可用的纹理
                    ImGui.Image(imageId, new System.Numerics.Vector2(50, 50)); // 显示图片，这里的尺寸你可以根据需要调整
                }

                ImGui.TableNextColumn();
                if (ImGui.Button("Download Image")) // 显示按钮
                {
                    // 下载图片
                    var imageUrl = item.ImageUrl; // 假设每个item都有一个ImageUrl属性
                    var imageBytes = httpClient.GetByteArrayAsync(imageUrl).Result ;
                    using (var ms = new MemoryStream(imageBytes))
                    {
                        Bitmap bitmap = (Bitmap)Bitmap.FromStream(ms);

                    }
                }
            }

            ImGui.EndTable();
        }

        ImGui.End(); // 结束ImGui窗口
    }

}
