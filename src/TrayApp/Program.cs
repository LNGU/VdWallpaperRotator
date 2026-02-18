using WallpaperRotator.Core;

namespace WallpaperRotator.TrayApp;

static class Program
{
    [STAThread]
    static void Main()
    {
        AppPaths.EnsureAppDataDir();
        
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (s, e) => Logger.Error(e.Exception, "UI thread exception");
        AppDomain.CurrentDomain.UnhandledException += (s, e) => 
        {
            if (e.ExceptionObject is Exception ex)
                Logger.Error(ex, "Unhandled exception");
        };

        try
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Fatal exception in Main");
            throw;
        }
    }
}