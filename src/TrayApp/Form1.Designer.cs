#nullable enable
namespace WallpaperRotator.TrayApp;

partial class Form1
{
    private System.ComponentModel.IContainer? components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(300, 150);
        Text = "Vd Wallpaper Rotator";
        ShowInTaskbar = false;
        WindowState = FormWindowState.Minimized;
    }
}

