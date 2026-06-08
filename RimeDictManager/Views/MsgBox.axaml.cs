namespace RimeDictManager.Views;

using Avalonia.Controls;

public sealed partial class MsgBox: Window {
    /// <summary> 设计器专用 </summary>
    public MsgBox(): this("信息", true) {}

    public MsgBox(string msg, bool isAsk) {
        InitializeComponent();
        Msg.Text = msg;
        if (isAsk) {
            Title = "确认";
            BtnYes.Content = "是";
            BtnYes.Click += (_, _) => Close(true);
            BtnNo.Content = "否";
            BtnNo.Click += (_, _) => Close(false);
        } else {
            Title = "提示";
            BtnYes.Content = "知道了";
            BtnYes.Click += (_, _) => Close();
            BtnNo.IsVisible = false;
        }
    }
}
