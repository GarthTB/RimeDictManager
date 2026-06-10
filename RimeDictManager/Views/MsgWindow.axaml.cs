namespace RimeDictManager.Views;

using Avalonia.Controls;

public sealed partial class MsgWindow: Window {
    /// <summary> 设计器专用 </summary>
    public MsgWindow(): this("信息", true) {}

    public MsgWindow(string msg, bool isAsk) {
        InitializeComponent();
        Msg.Text = msg;
        if (!isAsk) {
            Title = "提示";
            BtnYes.Content = "知道了";
            BtnYes.Click += (_, _) => Close();
            BtnNo.IsVisible = false;
            return;
        }
        Title = "确认";
        BtnYes.Content = "是";
        BtnYes.Click += (_, _) => Close(true);
        BtnNo.Content = "否";
        BtnNo.Click += (_, _) => Close(false);
    }
}
