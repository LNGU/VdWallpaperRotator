using System.Runtime.InteropServices;

namespace WallpaperRotator.VirtDesktop;

public sealed record VirtualDesktopInfo(int Index, Guid Id, string? Name, string? WallpaperPath);

public sealed class VirtualDesktopService
{
    private readonly IServiceProvider10 _shell;
    private readonly IVirtualDesktopManagerInternal22621? _internal22621;
    private readonly IVirtualDesktopManagerInternal26100? _internal26100;
    public int WindowsBuild { get; }

    public VirtualDesktopService()
    {
        WindowsBuild = Environment.OSVersion.Version.Build;
        if (WindowsBuild < 22621)
            throw new PlatformNotSupportedException($"Per-virtual-desktop wallpaper requires Windows 11 22H2+ (build 22621+). Detected build {WindowsBuild}.");

        _shell = (IServiceProvider10)Activator.CreateInstance(Type.GetTypeFromCLSID(Guids.CLSID_ImmersiveShell)!)!;

        // Windows 11 24H2+ (build 26100+) inserts an extra method into this interface.
        // Build 26200 also uses the 26100 layout.
        if (WindowsBuild >= 26100)
            _internal26100 = (IVirtualDesktopManagerInternal26100)_shell.QueryService(Guids.CLSID_VirtualDesktopManagerInternal, typeof(IVirtualDesktopManagerInternal26100).GUID);
        else
            _internal22621 = (IVirtualDesktopManagerInternal22621)_shell.QueryService(Guids.CLSID_VirtualDesktopManagerInternal, typeof(IVirtualDesktopManagerInternal22621).GUID);
    }

    private void GetDesktops(out IObjectArray desktops)
    {
        if (_internal26100 != null) { _internal26100.GetDesktops(out desktops); return; }
        _internal22621!.GetDesktops(out desktops);
    }

    private void SetDesktopWallpaper(IVirtualDesktop22621 desktop, HString path)
    {
        if (_internal26100 != null) { _internal26100.SetDesktopWallpaper(desktop, path); return; }
        _internal22621!.SetDesktopWallpaper(desktop, path);
    }

    public IReadOnlyList<VirtualDesktopInfo> GetDesktops()
    {
        var desktops = new List<VirtualDesktopInfo>();

        GetDesktops(out var array);
        try
        {
            array.GetCount(out var count);
            var iid = typeof(IVirtualDesktop22621).GUID;

            for (var i = 0; i < count; i++)
            {
                array.GetAt(i, ref iid, out var obj);
                var vd = (IVirtualDesktop22621)obj;

                string? name = null;
                string? wp = null;

                try { name = vd.GetName().ToStringAndDelete(); } catch { }
                try { wp = vd.GetWallpaperPath().ToStringAndDelete(); } catch { }

                desktops.Add(new VirtualDesktopInfo(i, vd.GetId(), name, wp));

                Marshal.ReleaseComObject(vd);
            }
        }
        finally
        {
            Marshal.ReleaseComObject(array);
        }

        return desktops;
    }

    public void SetWallpaper(int desktopIndex, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException(nameof(path));

        if (!File.Exists(path))
            throw new FileNotFoundException($"Wallpaper file not found: {path}", path);

        GetDesktops(out var array);
        try
        {
            var iid = typeof(IVirtualDesktop22621).GUID;
            array.GetAt(desktopIndex, ref iid, out var obj);
            var vd = (IVirtualDesktop22621)obj;

            try
            {
                var hs = HString.FromString(path);
                try 
                { 
                    SetDesktopWallpaper(vd, hs);
                }
                finally { hs.Delete(); }
            }
            finally
            {
                Marshal.ReleaseComObject(vd);
            }
        }
        finally
        {
            Marshal.ReleaseComObject(array);
        }
    }

    /// <summary>
    /// Sets wallpaper for the current desktop using the official IDesktopWallpaper API.
    /// This affects all monitors on the current virtual desktop.
    /// </summary>
    public static void SetWallpaperGlobal(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException(nameof(path));

        if (!File.Exists(path))
            throw new FileNotFoundException($"Wallpaper file not found: {path}", path);

        var wallpaper = (IDesktopWallpaper)Activator.CreateInstance(Type.GetTypeFromCLSID(Guids.CLSID_DesktopWallpaper)!)!;
        try
        {
            // Set wallpaper on all monitors
            wallpaper.GetMonitorDevicePathCount(out var count);
            for (uint i = 0; i < count; i++)
            {
                wallpaper.GetMonitorDevicePathAt(i, out var monitorId);
                wallpaper.SetWallpaper(monitorId, path);
            }
        }
        finally
        {
            Marshal.ReleaseComObject(wallpaper);
        }
    }
}

internal static class Guids
{
    public static readonly Guid CLSID_ImmersiveShell = new("C2F03A33-21F5-47FA-B4BB-156362A2F239");
    public static readonly Guid CLSID_VirtualDesktopManagerInternal = new("C5E0CDCA-7B6E-41B2-9FC4-D93975CC467B");
    public static readonly Guid CLSID_DesktopWallpaper = new("C2CF3110-460E-4FC1-B9D0-8A1C0C9CC4BD");
}

[StructLayout(LayoutKind.Sequential)]
internal struct HString
{
    public IntPtr Handle;

    [DllImport("combase.dll", CharSet = CharSet.Unicode)]
    private static extern int WindowsCreateString(string sourceString, int length, out IntPtr hstring);

    [DllImport("combase.dll")]
    private static extern int WindowsDeleteString(IntPtr hstring);

    [DllImport("combase.dll")]
    private static extern IntPtr WindowsGetStringRawBuffer(IntPtr hstring, out uint length);

    public static HString FromString(string value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var hr = WindowsCreateString(value, value.Length, out var handle);
        if (hr != 0)
            Marshal.ThrowExceptionForHR(hr);

        return new HString { Handle = handle };
    }

    public void Delete()
    {
        if (Handle == IntPtr.Zero)
            return;

        WindowsDeleteString(Handle);
        Handle = IntPtr.Zero;
    }

    public override string ToString()
    {
        if (Handle == IntPtr.Zero)
            return string.Empty;

        var ptr = WindowsGetStringRawBuffer(Handle, out var len);
        return Marshal.PtrToStringUni(ptr, (int)len) ?? string.Empty;
    }
}

internal static class HStringExtensions
{
    public static string? ToStringAndDelete(this HString h)
    {
        try { return h.Handle == IntPtr.Zero ? null : h.ToString(); }
        finally { h.Delete(); }
    }
}

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
[Guid("372E1D3B-38D3-42E4-A15B-8AB2B178F513")]
internal interface IApplicationView { }

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("92CA9DCD-5622-4BBA-A805-5E9F541BD8C9")]
internal interface IObjectArray
{
    void GetCount(out int count);
    void GetAt(int index, ref Guid iid, [MarshalAs(UnmanagedType.Interface)] out object obj);
}

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("6D5140C1-7436-11CE-8034-00AA006009FA")]
internal interface IServiceProvider10
{
    [return: MarshalAs(UnmanagedType.IUnknown)]
    object QueryService(ref Guid service, ref Guid riid);
}

internal static class ServiceProviderExtensions
{
    public static object QueryService(this IServiceProvider10 sp, Guid service, Guid riid)
        => sp.QueryService(ref service, ref riid);
}

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("3F07F4BE-B107-441A-AF0F-39D82529072C")]
internal interface IVirtualDesktop22621
{
    bool IsViewVisible(IApplicationView view);
    Guid GetId();
    HString GetName();
    HString GetWallpaperPath();
    bool IsRemote();
}

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("53F5CA0B-158F-4124-900C-057158060B27")]
internal interface IVirtualDesktopManagerInternal22621
{
    int GetCount();
    void MoveViewToDesktop(IApplicationView view, IVirtualDesktop22621 desktop);
    bool CanViewMoveDesktops(IApplicationView view);
    IVirtualDesktop22621 GetCurrentDesktop();
    void GetDesktops(out IObjectArray desktops);
    [PreserveSig] int GetAdjacentDesktop(IVirtualDesktop22621 from, int direction, out IVirtualDesktop22621 desktop);
    void SwitchDesktop(IVirtualDesktop22621 desktop);

    IVirtualDesktop22621 CreateDesktop();
    void MoveDesktop(IVirtualDesktop22621 desktop, int nIndex);
    void RemoveDesktop(IVirtualDesktop22621 desktop, IVirtualDesktop22621 fallback);
    IVirtualDesktop22621 FindDesktop(ref Guid desktopid);
    void GetDesktopSwitchIncludeExcludeViews(IVirtualDesktop22621 desktop, out IObjectArray unknown1, out IObjectArray unknown2);
    void SetDesktopName(IVirtualDesktop22621 desktop, HString name);
    void SetDesktopWallpaper(IVirtualDesktop22621 desktop, HString path);
    void UpdateWallpaperPathForAllDesktops(HString path);

    void CopyDesktopState(IApplicationView pView0, IApplicationView pView1);
    void CreateRemoteDesktop(HString path, out IVirtualDesktop22621 desktop);
    void SwitchRemoteDesktop(IVirtualDesktop22621 desktop, IntPtr switchtype);
    void SwitchDesktopWithAnimation(IVirtualDesktop22621 desktop);
    void GetLastActiveDesktop(out IVirtualDesktop22621 desktop);
    void WaitForAnimationToComplete();
}

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("53F5CA0B-158F-4124-900C-057158060B27")]
internal interface IVirtualDesktopManagerInternal26100
{
    int GetCount();
    void MoveViewToDesktop(IApplicationView view, IVirtualDesktop22621 desktop);
    bool CanViewMoveDesktops(IApplicationView view);
    IVirtualDesktop22621 GetCurrentDesktop();
    void GetDesktops(out IObjectArray desktops);
    [PreserveSig] int GetAdjacentDesktop(IVirtualDesktop22621 from, int direction, out IVirtualDesktop22621 desktop);
    void SwitchDesktop(IVirtualDesktop22621 desktop);

    // Windows 11 24H2 inserts this method:
    void SwitchDesktopAndMoveForegroundView(IVirtualDesktop22621 desktop);

    IVirtualDesktop22621 CreateDesktop();
    void MoveDesktop(IVirtualDesktop22621 desktop, int nIndex);
    void RemoveDesktop(IVirtualDesktop22621 desktop, IVirtualDesktop22621 fallback);
    IVirtualDesktop22621 FindDesktop(ref Guid desktopid);
    void GetDesktopSwitchIncludeExcludeViews(IVirtualDesktop22621 desktop, out IObjectArray unknown1, out IObjectArray unknown2);
    void SetDesktopName(IVirtualDesktop22621 desktop, HString name);
    void SetDesktopWallpaper(IVirtualDesktop22621 desktop, HString path);
    void UpdateWallpaperPathForAllDesktops(HString path);

    void CopyDesktopState(IApplicationView pView0, IApplicationView pView1);
    void CreateRemoteDesktop(HString path, out IVirtualDesktop22621 desktop);
    void SwitchRemoteDesktop(IVirtualDesktop22621 desktop, IntPtr switchtype);
    void SwitchDesktopWithAnimation(IVirtualDesktop22621 desktop);
    void GetLastActiveDesktop(out IVirtualDesktop22621 desktop);
    void WaitForAnimationToComplete();
}

[ComImport]
[Guid("B92B56A9-8B55-4E14-9A89-0199BBB6F93B")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IDesktopWallpaper
{
    void SetWallpaper([MarshalAs(UnmanagedType.LPWStr)] string monitorID, [MarshalAs(UnmanagedType.LPWStr)] string wallpaper);
    void GetWallpaper([MarshalAs(UnmanagedType.LPWStr)] string monitorID, [MarshalAs(UnmanagedType.LPWStr)] out string wallpaper);
    void GetMonitorDevicePathAt(uint monitorIndex, [MarshalAs(UnmanagedType.LPWStr)] out string monitorID);
    void GetMonitorDevicePathCount(out uint count);
    void GetMonitorRECT([MarshalAs(UnmanagedType.LPWStr)] string monitorID, out tagRECT displayRect);
    void SetBackgroundColor(uint color);
    void GetBackgroundColor(out uint color);
    void SetPosition(int position);
    void GetPosition(out int position);
    void SetSlideshow(IntPtr items);
    void GetSlideshow(out IntPtr items);
    void SetSlideshowOptions(uint options, uint slideshowTick);
    void GetSlideshowOptions(out uint options, out uint slideshowTick);
    void AdvanceSlideshow([MarshalAs(UnmanagedType.LPWStr)] string monitorID, int direction);
    void GetStatus(out int state);
    void Enable(bool enable);
}

[StructLayout(LayoutKind.Sequential)]
internal struct tagRECT
{
    public int left;
    public int top;
    public int right;
    public int bottom;
}
