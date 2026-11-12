using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace CSKGamingSuite;

public record WindowSnapTarget(IntPtr Handle, string Title, string ProcessName);

public record MonitorArea(string DeviceName, Rect Bounds, Rect WorkArea, bool IsPrimary);

public static class UltrawideManager
{
    #region Win32 P/Invoke
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const uint SWP_FRAMECHANGED = 0x0020;
    private const uint SWP_SHOWWINDOW = 0x0040;
    private const int SW_RESTORE = 9;

    private const int MONITOR_DEFAULTTONEAREST = 2;

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MONITORINFOEX
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szDevice;
    }

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr GetShellWindow();

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

    private delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsZoomed(IntPtr hWnd);
    #endregion

    public enum UltrawidePreset
    {
        TwoHalves_50_50,              // 50% | 50%
        ThreeColumns_Equal,           // 33.3% | 33.3% | 33.3%
        ThreeColumns_UltrawideMaster, // 25% (WhatsApp) | 50% (Jogo/Sistema) | 25% (Planilhas) -> Perfeito para 34" LG
        ThreeColumns_SidebarLeft,     // 20% | 40% | 40%
        FourColumns_Equal,            // 25% | 25% | 25% | 25%
        TwoByTwo_Grid,                // 4 Quadrantes (2x2)
        GamingFocus_Center            // 20% | 60% Centro Gigante | 20%
    }

    public static List<MonitorArea> GetMonitors()
    {
        var monitors = new List<MonitorArea>();

        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMon, IntPtr hdcMon, ref RECT r, IntPtr d) =>
        {
            var mi = new MONITORINFOEX();
            mi.cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));
            if (GetMonitorInfo(hMon, ref mi))
            {
                var bounds = new Rect(mi.rcMonitor.Left, mi.rcMonitor.Top, mi.rcMonitor.Right - mi.rcMonitor.Left, mi.rcMonitor.Bottom - mi.rcMonitor.Top);
                var work = new Rect(mi.rcWork.Left, mi.rcWork.Top, mi.rcWork.Right - mi.rcWork.Left, mi.rcWork.Bottom - mi.rcWork.Top);
                bool isPrimary = (mi.dwFlags & 1) != 0;
                monitors.Add(new MonitorArea(mi.szDevice, bounds, work, isPrimary));
            }
            return true;
        }, IntPtr.Zero);

        if (monitors.Count == 0)
        {
            // Fallback para tela cheia padrão de 34"
            monitors.Add(new MonitorArea("DISPLAY1", new Rect(0, 0, 3440, 1440), new Rect(0, 0, 3440, 1400), true));
        }

        return monitors;
    }

    public static List<Rect> CalculateZones(UltrawidePreset preset, Rect workArea, int gap = 8)
    {
        var zones = new List<Rect>();
        double x = workArea.X + gap;
        double y = workArea.Y + gap;
        double totalW = Math.Max(100, workArea.Width - (gap * 2));
        double totalH = Math.Max(100, workArea.Height - (gap * 2));

        switch (preset)
        {
            case UltrawidePreset.TwoHalves_50_50:
                {
                    double w = Math.Floor((totalW - gap) / 2.0);
                    zones.Add(new Rect(x, y, w, totalH));
                    zones.Add(new Rect(x + w + gap, y, totalW - w - gap, totalH));
                }
                break;

            case UltrawidePreset.ThreeColumns_Equal:
                {
                    double w = Math.Floor((totalW - (gap * 2)) / 3.0);
                    zones.Add(new Rect(x, y, w, totalH));
                    zones.Add(new Rect(x + w + gap, y, w, totalH));
                    zones.Add(new Rect(x + (w + gap) * 2, y, totalW - (w * 2) - (gap * 2), totalH));
                }
                break;

            case UltrawidePreset.ThreeColumns_UltrawideMaster:
                {
                    // Divisão Dourada Ultrawide (21:9 - 34 polegadas):
                    // Esquerda: 25% (WhatsApp / Discord / Chat)
                    // Centro: 50% (Jogo / Sistema Principal / Navegador)
                    // Direita: 25% (Planilhas / Docs / Tarefas)
                    double sideW = Math.Floor(totalW * 0.25);
                    double centerW = totalW - (sideW * 2) - (gap * 2);
                    zones.Add(new Rect(x, y, sideW, totalH));
                    zones.Add(new Rect(x + sideW + gap, y, centerW, totalH));
                    zones.Add(new Rect(x + sideW + centerW + (gap * 2), y, sideW, totalH));
                }
                break;

            case UltrawidePreset.ThreeColumns_SidebarLeft:
                {
                    // 20% lateral esquerda | 40% centro | 40% direita
                    double sideW = Math.Floor(totalW * 0.20);
                    double remW = Math.Floor((totalW - sideW - (gap * 2)) / 2.0);
                    zones.Add(new Rect(x, y, sideW, totalH));
                    zones.Add(new Rect(x + sideW + gap, y, remW, totalH));
                    zones.Add(new Rect(x + sideW + remW + (gap * 2), y, totalW - sideW - remW - (gap * 2), totalH));
                }
                break;

            case UltrawidePreset.FourColumns_Equal:
                {
                    double w = Math.Floor((totalW - (gap * 3)) / 4.0);
                    for (int i = 0; i < 4; i++)
                    {
                        double currentX = x + (i * (w + gap));
                        double currentW = (i == 3) ? (x + totalW - currentX) : w;
                        zones.Add(new Rect(currentX, y, currentW, totalH));
                    }
                }
                break;

            case UltrawidePreset.TwoByTwo_Grid:
                {
                    double w = Math.Floor((totalW - gap) / 2.0);
                    double h = Math.Floor((totalH - gap) / 2.0);
                    zones.Add(new Rect(x, y, w, h));
                    zones.Add(new Rect(x + w + gap, y, totalW - w - gap, h));
                    zones.Add(new Rect(x, y + h + gap, w, totalH - h - gap));
                    zones.Add(new Rect(x + w + gap, y + h + gap, totalW - w - gap, totalH - h - gap));
                }
                break;

            case UltrawidePreset.GamingFocus_Center:
                {
                    // Centro 60% com 20% em cada lado
                    double sideW = Math.Floor(totalW * 0.20);
                    double centerW = totalW - (sideW * 2) - (gap * 2);
                    zones.Add(new Rect(x, y, sideW, totalH));
                    zones.Add(new Rect(x + sideW + gap, y, centerW, totalH));
                    zones.Add(new Rect(x + sideW + centerW + (gap * 2), y, sideW, totalH));
                }
                break;
        }

        return zones;
    }

    public static List<WindowSnapTarget> GetOpenWindows()
    {
        var list = new List<WindowSnapTarget>();
        IntPtr shellWnd = GetShellWindow();

        EnumWindows((hWnd, lParam) =>
        {
            if (hWnd == shellWnd) return true;
            if (!IsWindowVisible(hWnd)) return true;

            int len = GetWindowTextLength(hWnd);
            if (len == 0) return true;

            var sb = new StringBuilder(len + 1);
            GetWindowText(hWnd, sb, sb.Capacity);
            string title = sb.ToString().Trim();

            if (string.IsNullOrWhiteSpace(title)) return true;
            if (title == "Program Manager" || title == "Windows Input Experience" || title == "CSK Gaming Suite") return true;

            GetWindowThreadProcessId(hWnd, out uint pid);
            string procName = "";
            try
            {
                using var proc = Process.GetProcessById((int)pid);
                procName = proc.ProcessName;
            }
            catch { }

            // Não listar processos secundários/invisíveis do Windows
            if (procName.Equals("TextInputHost", StringComparison.OrdinalIgnoreCase) ||
                procName.Equals("ShellExperienceHost", StringComparison.OrdinalIgnoreCase) ||
                procName.Equals("StartMenuExperienceHost", StringComparison.OrdinalIgnoreCase) ||
                procName.Equals("SearchHost", StringComparison.OrdinalIgnoreCase))
                return true;

            list.Add(new WindowSnapTarget(hWnd, title, procName));
            return true;
        }, IntPtr.Zero);

        return list;
    }

    public static bool SnapWindow(IntPtr hWnd, Rect rect)
    {
        if (hWnd == IntPtr.Zero) return false;

        // Se estiver minimizada ou maximizada, restaurar para permitir reposicionamento com precisão de pixel
        ShowWindow(hWnd, SW_RESTORE);

        return SetWindowPos(
            hWnd,
            IntPtr.Zero,
            (int)rect.X,
            (int)rect.Y,
            (int)rect.Width,
            (int)rect.Height,
            SWP_NOZORDER | SWP_FRAMECHANGED | SWP_SHOWWINDOW
        );
    }

    public static void FocusWindow(IntPtr hWnd)
    {
        if (hWnd != IntPtr.Zero)
        {
            SetForegroundWindow(hWnd);
        }
    }
}
