namespace RimeDictManager.Views;

using Avalonia.Controls;

public sealed partial class MsgWindow: Window {
    /// <summary> 设计器专用 </summary>
    public MsgWindow(): this("标题", "内容", true) {}

    public MsgWindow(string title, string msg, bool isAsk) {
        InitializeComponent();
        Title = title;
        Msg.Text = msg;
        if (!isAsk) {
            BtnYes.Content = "知道了";
            BtnYes.Click += (_, _) => Close();
            BtnNo.IsVisible = false;
            return;
        }
        BtnYes.Content = "是";
        BtnYes.Click += (_, _) => Close(true);
        BtnNo.Click += (_, _) => Close(false);
    }
}
