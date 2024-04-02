/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 港股通全景图左下角的交易小窗口
*/
using ImGuiNET;
using System.Numerics;

namespace LampyrisStockTradeSystem;

[UniqueWidget]
public class HKChaseRiseTradeSubWindow : Widget
{
    private EastMoneyPositionInfo m_positionInfo;
    public override string Name => "港股通交易";

    public override ImGuiWindowFlags windowFlags => ImGuiWindowFlags.None;

    public override void OnAwake()
    {
        base.OnAwake();
        pos = new Vector2(0, ImGui.GetIO().DisplaySize.Y - 200);
        size = new Vector2(400, 200);

        LifecycleManager.Instance.Get<EventManager>().AddEventHandler(EventType.PositionUpdate, OnPositionUpdate);
        LifecycleManager.Instance.Get<EventManager>().AddEventHandler(EventType.RevokeUpdate, OnRevokeUpdate);
    }

    public override void OnDestroy()
    {
        LifecycleManager.Instance.Get<EventManager>().RemoveEventHandler(EventType.PositionUpdate, OnPositionUpdate);
        LifecycleManager.Instance.Get<EventManager>().RemoveEventHandler(EventType.RevokeUpdate, OnRevokeUpdate);
        base.OnDestroy();
    }

    public void OnPositionUpdate(object[] parameters)
    {
        m_positionInfo = (EastMoneyPositionInfo)parameters[0];
    }

    public void OnRevokeUpdate(object[] parameters)
    {

    }

    public override void OnAfterGUI()
    {
        base.OnAfterGUI();
        if (ImGui.IsWindowCollapsed())
            pos = new Vector2(0, ImGui.GetIO().DisplaySize.Y - 20);
        else
            pos = new Vector2(0, ImGui.GetIO().DisplaySize.Y - 200);
    }

    public override void OnGUI()
    {
        if(!EastMoneyTradeManager.Instance.isLoggedIn)
        {
            ImGui.Text("暂未登录东方财富交易,请登录!");
            if(ImGui.Button("登录"))
            {
                EastMoneyTradeManager.Login();
            }
            return;
        }
        // 创建滚动区域
        ImGui.BeginChild("滚动区域", new Vector2(0, 0), true);
        {
            if (ImGui.CollapsingHeader("持仓"))
            {
                if (m_positionInfo != null)
                {
                    ImGui.Text("总市值:" + m_positionInfo.totalMoney);
                    ImGui.SameLine();

                    ImGui.Text("持仓市值" + m_positionInfo.positionMoney);
                    ImGui.SameLine();

                    ImGui.Text("持仓盈亏" + m_positionInfo.positionProfitLose);

                    ImGui.Text("当日盈亏" + m_positionInfo.todayProfitLose);
                    ImGui.SameLine();
                    ImGui.Text("可用资金" + m_positionInfo.canUseMoney);

                    if (m_positionInfo.stockInfos.Count > 0)
                    {
                        if (ImGui.BeginTable("HKChaseRiseSubWinOrder", 6))
                        {
                            ImGui.TableSetupColumn("代码");
                            ImGui.TableSetupColumn("名称");
                            ImGui.TableSetupColumn("成本");
                            ImGui.TableSetupColumn("数量");
                            ImGui.TableSetupColumn("浮盈");
                            ImGui.TableSetupColumn("");
                            ImGui.TableHeadersRow();

                            foreach (EastMoneyPositionStockInfo stockInfo in m_positionInfo.stockInfos)
                            {
                                ImGui.TableNextRow();

                                ImGui.TableNextColumn();
                                ImGui.Text(stockInfo.stockCode);
                                ImGui.TableNextColumn();
                                ImGui.Text(stockInfo.stockName);
                                ImGui.TableNextColumn();
                                ImGui.Text(stockInfo.costPrice);
                                ImGui.TableNextColumn();
                                ImGui.Text(stockInfo.count);
                                ImGui.TableNextColumn();
                                ImGui.Text(stockInfo.profitLose);
                                ImGui.TableNextColumn();
                                ImGui.Button("卖出");
                            }

                            ImGui.EndTable();
                        }
                    }
                    else
                    {
                        ImGui.Text("暂无持仓股票");
                    }
                }
                else
                {
                    ImGui.Text("暂无持仓数据");
                }
            }
            if (ImGui.CollapsingHeader("委托"))
            {
                if (ImGui.BeginTable("HKChaseRiseSubWinOwnStock", 6))
                {
                    ImGui.TableSetupColumn("代码");
                    ImGui.TableSetupColumn("名称");
                    ImGui.TableSetupColumn("方向");
                    ImGui.TableSetupColumn("价格");
                    ImGui.TableSetupColumn("数量");
                    ImGui.TableSetupColumn("");
                    ImGui.TableHeadersRow();

                    for (int i = 0; i < 20; i++)
                    {
                        ImGui.TableNextRow();

                        ImGui.TableNextColumn();
                        ImGui.Text("600000");
                        ImGui.TableNextColumn();
                        ImGui.Text("浦发银行");
                        ImGui.TableNextColumn();
                        ImGui.Text("买入");
                        ImGui.TableNextColumn();
                        ImGui.Text("7.00");
                        ImGui.TableNextColumn();
                        ImGui.Text("60000");
                        ImGui.TableNextColumn();
                        ImGui.PushID($"HKChaseRiseSubWinOwnStockRevoke{i}");
                        if (ImGui.Button("撤单"))
                        {
                            int a = 1;
                        }
                        ImGui.PopID();
                    }

                    ImGui.EndTable();
                }
            }
        }
        // 结束滚动区域
        ImGui.EndChild();
    }
}