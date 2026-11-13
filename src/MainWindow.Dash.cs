using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using LibreHardwareMonitor.Hardware;

namespace CSKGamingSuite;

/// <summary>
/// Visão geral: painel de blocos com gráfico de área (CPU × GPU), desempenho, tabela de componentes, anéis de
/// memória/disco, eventos recentes e lista de verificação da saúde do PC.
/// </summary>
public partial class MainWindow
{
    public sealed record DashComponent(string Icon, string Name, string Model, string Temp, string Load, string Clock, string Power, string Target);
    public sealed record DashEvent(string Icon, string Title, string Detail, string Value, DateTime At)
    {
        public string When => At.Date == DateTime.Today ? $"hoje {At:HH:mm}" : $"{At:dd/MM HH:mm}";
    }
    public sealed record DashRingItem(Brush Swatch, string Label, string Value);
    public sealed record DashCheck(string Title, string Detail, bool Ok);

    private readonly record struct DashSample(float? CpuT, float? GpuT, float? CpuL, float? GpuL, float? CpuW, float? GpuW);

    private const int DashMax = 900; // 15 min a 1 amostra/s
    private readonly List<DashSample> _dash = new();
    private readonly List<DashEvent> _events = new();
    private List<DashCheck> _health = new();
    private DateTime _healthAt = DateTime.MinValue;

    // ---------------- coleta (a cada segundo, mesmo com a página fechada) ----------------

    private float? MainGpuLoad() => _gpus.Count == 0 ? null :
        _monitor.Rows.FirstOrDefault(r => r.Hardware == _gpus[0].Name && r.Type == SensorType.Load && r.Name == "GPU Core")?.RawValue;

    private float? CpuClockAvg()
    {
        var c = _monitor.Rows.Where(r => r.HardwareType == HardwareType.Cpu && r.Type == SensorType.Clock &&
                                         r.Name.StartsWith("Core #") && r.RawValue > 0).Select(r => r.RawValue!.Value).ToList();
        return c.Count > 0 ? c.Average() : null;
    }

    private void CollectDashSample()
    {
        _dash.Add(new DashSample(_monitor.CpuTemperature, MainGpuTemp(), _monitor.CpuLoad, MainGpuLoad(), _monitor.CpuPower, MainGpuPower()));
        if (_dash.Count > DashMax) _dash.RemoveAt(0);
        if (PageDash.IsVisible) UpdateDashboard(full: false);
    }

    // ---------------- eventos recentes ----------------

    private void AddEvent(string icon, string title, string detail, string value)
    {
        _events.Insert(0, new DashEvent(icon, title, detail, value, DateTime.Now));
        if (_events.Count > 30) _events.RemoveAt(_events.Count - 1);
        if (PageDash.IsVisible) ShowEvents();
    }

    /// <summary>Eventos já registrados antes desta sessão (resultados de benchmark e verificações salvos).</summary>
    private IEnumerable<DashEvent> SavedEvents()
    {
        foreach (var r in _settings.BenchRuns)
        {
            if (r.Multi is double m) yield return new DashEvent("", "CSK BENCH", "Render · multi-core", $"{m:N0} pts", r.Date);
            else if (r.Single is double s) yield return new DashEvent("", "CSK BENCH", "Render · single-core", $"{s:N0} pts", r.Date);
        }
        foreach (var r in _settings.BenchRecords)
        {
            var (icon, title) = r.Test switch
            {
                "gpu" => ("", "Cena de jogo (GPU)"),
                "sim" => ("", "Simulação de jogo"),
                "mem" => ("", "Memória e carregamento"),
                "oc" => ("", "Verificação de estabilidade"),
                "disk" => ("", "Benchmark de disco"),
                _ => ("", r.Test)
            };
            string first = r.Summary.Split('·')[0].Trim();
            string rest = r.Summary.Contains('·') ? r.Summary[(r.Summary.IndexOf('·') + 1)..].Trim() : "";
            yield return new DashEvent(icon, title, rest, first, r.Date);
        }
    }

    private void ShowEvents()
    {
        var list = _events.Concat(SavedEvents()).OrderByDescending(e => e.At).Take(4).ToList();
        DashEvents.ItemsSource = list;
        DashEventsEmpty.Visibility = list.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    // ---------------- atualização ----------------

    private void DashOption_Checked(object sender, RoutedEventArgs e) { if (DashChart != null) DrawDashChart(); }
    private void DashChart_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (DashBentoLayout != null && DashBentoLayout.Visibility == Visibility.Visible) DrawBentoSpark();
        else if (DashMagentaLayout != null && DashMagentaLayout.Visibility == Visibility.Visible) DrawMagentaSpark();
        else DrawDashChart();
    }
    private void DashBench_Click(object sender, RoutedEventArgs e) => NavBench.IsChecked = true;
    private void DashOptimize_Click(object sender, RoutedEventArgs e) => NavOptimize.IsChecked = true;
    private void BentoAllSensors_Click(object sender, RoutedEventArgs e) => NavSensors.IsChecked = true;

    private void DashGo_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: string page }) return;
        var nav = new RadioButton[] { NavSensors, NavGpu, NavSystem, NavDisks, NavGames, NavOptimize, NavBench, NavOc, NavLauncher }
            .FirstOrDefault(n => (string)n.Tag == page);
        if (nav != null) nav.IsChecked = true;
    }

    private void UpdateDashboard(bool full = true)
    {
        if (DashBentoLayout != null && DashBentoLayout.Visibility == Visibility.Visible)
        {
            UpdateBentoDashboard();
            return;
        }
        if (DashCobaltLayout != null && DashCobaltLayout.Visibility == Visibility.Visible)
        {
            UpdateCobaltDashboard();
            return;
        }
        if (DashMagentaLayout != null && DashMagentaLayout.Visibility == Visibility.Visible)
        {
            UpdateMagentaDashboard();
            return;
        }
        DrawDashChart();
        UpdateAfterburnerGauges();
        UpdateDashComponents();
        UpdateDashRings();
        if (full)
        {
            UpdateDashScores();
            ShowEvents();
        }
        if (full || (DateTime.Now - _healthAt).TotalSeconds > 60) UpdateDashHealth();
    }

    private void UpdateBentoDashboard()
    {
        BentoPcLabel.Text = Environment.MachineName;
        BentoUserTitle.Text = Environment.UserName;

        float? cpuT = _monitor.CpuTemperature;
        float? cpuL = _monitor.CpuLoad;
        float? cpuW = _monitor.CpuPower;
        float? gpuT = MainGpuTemp();
        float? gpuL = MainGpuLoad();
        float? gpuW = MainGpuPower();

        // CPU
        BentoCpuModel.Text = string.IsNullOrWhiteSpace(_cpuName) ? "Processador" : GameCatalog.ShortCpuName(_cpuName);
        BentoCpuTempTxt.Text = cpuT is float ct ? $"{ct:0}°" : "—°";
        BentoCpuUtilTxt.Text = cpuL is float cl ? $"{cl:0}%" : "—";
        BentoCpuBar.Width = cpuL is float cl2 ? Math.Clamp(cl2 / 100.0, 0, 1) * 140 : 0;
        var ciano = new SolidColorBrush(Color.FromRgb(76, 201, 240));
        ciano.Freeze();
        DrawBentoRing(BentoCpuRingCanvas, cpuT is float ct2 ? Math.Clamp((ct2 - 20) / 80.0, 0.05, 1) : 0.15, ciano);

        bool tempHot = (cpuT ?? 0) >= 85 || (gpuT ?? 0) >= 85;
        var okCol = new SolidColorBrush(Color.FromRgb(16, 214, 142));
        var warnCol = new SolidColorBrush(Color.FromRgb(245, 158, 11));
        okCol.Freeze(); warnCol.Freeze();
        BentoDotTemp.Fill = tempHot ? warnCol : okCol;
        BentoStatTemp.Text = tempHot ? "Elevada" : "Ideal";
        BentoCpuDot.Fill = (cpuT ?? 0) >= 85 ? warnCol : okCol;
        BentoCpuStatus.Text = (cpuT ?? 0) >= 85 ? "Quente" : "Ideal";

        // GPU
        if (_gpus.Count > 0)
        {
            BentoGpuModel.Text = GameCatalog.ShortGpuName(_gpus[0].Name);
            BentoStatGpu.Text = GameCatalog.ShortGpuName(_gpus[0].Name);
        }
        BentoGpuTempTxt.Text = gpuT is float gt ? $"{gt:0}°" : "—°";
        BentoGpuUtilTxt.Text = gpuL is float gl ? $"{gl:0}%" : "—";
        BentoGpuBar.Width = gpuL is float gl2 ? Math.Clamp(gl2 / 100.0, 0, 1) * 140 : 0;
        DrawBentoRing(BentoGpuRingCanvas, gpuT is float gt2 ? Math.Clamp((gt2 - 20) / 80.0, 0.05, 1) : 0.15, ciano);
        BentoGpuDot.Fill = (gpuT ?? 0) >= 85 ? warnCol : okCol;
        BentoGpuStatus.Text = (gpuT ?? 0) >= 85 ? "Quente" : "Ideal";

        // RAM
        var (total, avail) = Optimizer.MemoryStatus();
        if (total > 0)
        {
            double frac = (double)(total - avail) / total;
            BentoRamSub.Text = $"{total / 1073741824.0:0} GB RAM";
            BentoRamUtilTxt.Text = $"{frac:P0}";
            BentoRamBar.Width = Math.Clamp(frac, 0, 1) * 130;
            BentoStatRam.Text = $"{(total - avail) / 1073741824.0:0.0} / {total / 1073741824.0:0} GB";
        }

        // Disco
        if (_drives.FirstOrDefault(d => d.Letter.StartsWith("C", StringComparison.OrdinalIgnoreCase)) is { } c0)
        {
            double frac = c0.Total > 0 ? (double)(c0.Total - c0.Free) / c0.Total : 0;
            BentoDiskSub.Text = c0.Disk.Length > 0 ? c0.Disk : c0.Kind;
            BentoDiskUtilTxt.Text = $"{frac:P0}";
            BentoDiskBar.Width = Math.Clamp(frac, 0, 1) * 130;
            BentoStatDisk.Text = $"{DriveCard.Fmt(c0.Free)} livres";
        }

        // Benchmark & Controles
        var runs = _settings.BenchRuns.Where(r => r.Multi != null).ToList();
        BentoBenchStat.Text = runs.Count > 0 ? $"Última nota: {runs[0].Multi:N0} pts" : "Pronto para testar";
        BentoChkSidebar.IsChecked = _settings.SidebarExpanded;
        BentoChkMute.IsChecked = _audio.IsMasterMuted();

        // Telemetria ao Vivo (Sparkline Violeta)
        BentoLiveCpuW.Text = cpuW is float cw ? $"CPU {cw:0}W" : "CPU —";
        BentoLiveGpuW.Text = gpuW is float gw ? $"GPU {gw:0}W" : "GPU —";
        DrawBentoSpark();
    }

    private void DrawBentoRing(Canvas canvas, double fraction, Brush stroke)
    {
        canvas.Children.Clear();
        double cx = canvas.Width / 2, cy = canvas.Height / 2, r = 34;
        var track = new SolidColorBrush(Color.FromArgb(0x28, 255, 255, 255));
        track.Freeze();
        canvas.Children.Add(BuildArcPath(cx, cy, r, -90, 359.9, track, 5));
        canvas.Children.Add(BuildArcPath(cx, cy, r, -90, 359.9 * Math.Clamp(fraction, 0.02, 0.99), stroke, 5));
    }

    private void DrawBentoSpark()
    {
        BentoSparkCanvas.Children.Clear();
        double w = BentoSparkCanvas.ActualWidth, h = BentoSparkCanvas.ActualHeight;
        if (w < 20 || h < 20) return;

        var pts = _dash.TakeLast(60).Select(s => (double)(s.CpuL ?? s.GpuL ?? 10)).ToList();
        if (pts.Count < 2) pts = new List<double> { 12, 18, 15, 24, 21, 28, 22 };

        var lineCol = Color.FromRgb(167, 139, 250);
        var polyLine = new Polyline
        {
            Stroke = new SolidColorBrush(lineCol),
            StrokeThickness = 2,
            StrokeLineJoin = PenLineJoin.Round
        };
        var area = new Polygon
        {
            Fill = new LinearGradientBrush(Color.FromArgb(0x66, 139, 92, 246), Color.FromArgb(0x05, 139, 92, 246), new Point(0, 0), new Point(0, 1))
        };

        for (int i = 0; i < pts.Count; i++)
        {
            double x = w * i / Math.Max(1, pts.Count - 1);
            double y = h - 4 - Math.Clamp(pts[i] / 100.0, 0.05, 0.95) * (h - 10);
            polyLine.Points.Add(new Point(x, y));
            area.Points.Add(new Point(x, y));
        }
        area.Points.Add(new Point(w, h));
        area.Points.Add(new Point(0, h));
        BentoSparkCanvas.Children.Add(area);
        BentoSparkCanvas.Children.Add(polyLine);
    }

    private void UpdateCobaltDashboard()
    {
        float? cpuT = _monitor.CpuTemperature;
        float? gpuT = MainGpuTemp();
        var activeBlue = new SolidColorBrush(Color.FromRgb(56, 189, 248));
        activeBlue.Freeze();

        CobaltCpuModel.Text = string.IsNullOrWhiteSpace(_cpuName) ? "Processador" : GameCatalog.ShortCpuName(_cpuName);
        CobaltCpuTempVal.Text = cpuT is float ct ? $"{ct:0}°C" : "—°C";
        DrawSegmentedArc(CobaltCpuArcCanvas, cpuT is float ct2 ? Math.Clamp((ct2 - 20) / 80.0, 0.04, 1) : 0.25, activeBlue);

        CobaltGpuModel.Text = _gpus.Count > 0 ? GameCatalog.ShortGpuName(_gpus[0].Name) : "Placa de Vídeo";
        CobaltGpuTempVal.Text = gpuT is float gt ? $"{gt:0}°C" : "—°C";
        DrawSegmentedArc(CobaltGpuArcCanvas, gpuT is float gt2 ? Math.Clamp((gt2 - 20) / 80.0, 0.04, 1) : 0.2, activeBlue);

        double ramFrac = 0;
        var (total, avail) = Optimizer.MemoryStatus();
        if (total > 0)
        {
            ramFrac = (double)(total - avail) / total;
            CobaltRamSub.Text = $"{total / 1073741824.0:0} GB RAM";
            CobaltRamUtilVal.Text = $"{ramFrac:P0}";
        }
        DrawSegmentedArc(CobaltRamArcCanvas, ramFrac, activeBlue);

        double diskFrac = 0;
        if (_drives.FirstOrDefault(d => d.Letter.StartsWith("C", StringComparison.OrdinalIgnoreCase)) is { } c0)
        {
            diskFrac = c0.Total > 0 ? (double)(c0.Total - c0.Free) / c0.Total : 0;
            CobaltDiskSub.Text = c0.Disk.Length > 0 ? c0.Disk : c0.Kind;
            CobaltDiskUtilVal.Text = $"{diskFrac:P0}";
        }
        DrawSegmentedArc(CobaltDiskArcCanvas, diskFrac, activeBlue);

        // Saúde geral e régua
        int score = 100;
        if ((cpuT ?? 0) >= 85 || (gpuT ?? 0) >= 85) score -= 20;
        if (ramFrac > 0.90) score -= 10;
        if (diskFrac > 0.92) score -= 10;
        CobaltHealthPct.Text = $"{score}%";
        CobaltHealthBar.Height = 210 * (score / 100.0);
        CobaltHealthLabel.Text = score >= 85 ? "Bom" : "Atenção";
        DrawCobaltScale();
    }

    private void DrawSegmentedArc(Canvas canvas, double fraction, Brush active)
    {
        canvas.Children.Clear();
        double cx = canvas.Width / 2, cy = canvas.Height - 16;
        double rIn = 58, rOut = 74;
        const int segments = 24;
        int lit = (int)Math.Round(Math.Clamp(fraction, 0, 1) * segments);
        var dim = new SolidColorBrush(Color.FromArgb(0x28, 255, 255, 255));
        dim.Freeze();

        for (int i = 0; i < segments; i++)
        {
            // De 180° (esquerda) até 0° (direita)
            double deg = 180.0 - (180.0 * i / (segments - 1));
            double rad = deg * Math.PI / 180.0;
            canvas.Children.Add(new Line
            {
                X1 = cx + rIn * Math.Cos(rad),
                Y1 = cy - rIn * Math.Sin(rad),
                X2 = cx + rOut * Math.Cos(rad),
                Y2 = cy - rOut * Math.Sin(rad),
                Stroke = i < lit ? active : dim,
                StrokeThickness = 2.5,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            });
        }
    }

    private void DrawCobaltScale()
    {
        if (CobaltScaleCanvas.Children.Count > 0) return;
        var tick = new SolidColorBrush(Color.FromArgb(0x40, 255, 255, 255));
        tick.Freeze();
        for (int i = 0; i <= 20; i++)
        {
            double y = i * 9.0;
            CobaltScaleCanvas.Children.Add(new Line
            {
                X1 = 0, X2 = i % 5 == 0 ? 14 : 7, Y1 = y, Y2 = y,
                Stroke = tick, StrokeThickness = i % 5 == 0 ? 1.3 : 0.8
            });
        }
    }

    private void UpdateMagentaDashboard()
    {
        MagClockTxt.Text = DateTime.Now.ToString("HH:mm");
        float? cpuL = _monitor.CpuLoad;
        float? cpuT = _monitor.CpuTemperature;
        MagPillCpu.Text = cpuL is float cl ? $"{cl:0}%" : "—";
        MagCpuLoadBig.Text = cpuL is float cl2 ? $"{cl2:0}%" : "—";
        MagCpuTempSmall.Text = cpuT is float ct ? $"{ct:0} °C" : "— °C";

        var up = TimeSpan.FromMilliseconds(Environment.TickCount64);
        string upStr = up.TotalHours >= 1 ? $"{(int)up.TotalHours}h" : $"{up.Minutes}m";
        MagPillUp.Text = upStr;
        MagUptimeVal.Text = upStr;
        MagUptimeSub.Text = $"{up.Days}d {up.Hours}h {up.Minutes}m";

        var mag = (Brush)FindResource("Accent");
        DrawMagentaRing(MagUptimeCanvas, Math.Clamp(up.TotalHours / 24.0, 0.12, 1), mag);

        var (total, avail) = Optimizer.MemoryStatus();
        if (total > 0)
        {
            double usedFrac = (double)(total - avail) / total;
            double freeFrac = 1.0 - usedFrac;
            MagPillRam.Text = $"{usedFrac:P0}";
            MagFreeRamPct.Text = $"{freeFrac:P0}";
            MagFreeRamSub.Text = $"{avail / 1073741824.0:0.0} GB livres";
            MagMemPct.Text = $"{usedFrac:P0}";
            MagMemSub.Text = $"{(total - avail) / 1073741824.0:0.0} / {total / 1073741824.0:0} GB";
            DrawMagentaRing(MagFreeRamCanvas, freeFrac, mag);
            DrawMagentaRing(MagMemCanvas, usedFrac, mag);
        }

        if (_drives.FirstOrDefault(d => d.Letter.StartsWith("C", StringComparison.OrdinalIgnoreCase)) is { } c0)
        {
            double diskFrac = c0.Total > 0 ? (double)(c0.Total - c0.Free) / c0.Total : 0;
            MagDiskPct.Text = $"{diskFrac:P0}";
            MagDiskSub.Text = $"{DriveCard.Fmt(c0.Free)} livres";
            DrawMagentaRing(MagDiskCanvas, diskFrac, mag);
        }

        MagSysHost.Text = Environment.MachineName;
        MagSysCpu.Text = string.IsNullOrWhiteSpace(_cpuName) ? "—" : GameCatalog.ShortCpuName(_cpuName);
        MagSysGpu.Text = _gpus.Count > 0 ? GameCatalog.ShortGpuName(_gpus[0].Name) : "—";
        MagChkMute.IsChecked = _audio.IsMasterMuted();

        var runs = _settings.BenchRuns.Where(r => r.Multi != null).ToList();
        MagBenchTxt.Text = runs.Count > 0 ? $"{runs[0].Multi:N0} pts" : "Pronto";

        DrawMagentaSpark();
    }

    private void DrawMagentaRing(Canvas canvas, double fraction, Brush stroke)
    {
        canvas.Children.Clear();
        double cx = canvas.Width / 2, cy = canvas.Height / 2, r = 29;
        var track = new SolidColorBrush(Color.FromArgb(0x28, 224, 64, 251));
        track.Freeze();
        canvas.Children.Add(BuildArcPath(cx, cy, r, -90, 359.9, track, 6));
        canvas.Children.Add(BuildArcPath(cx, cy, r, -90, 359.9 * Math.Clamp(fraction, 0.03, 0.99), stroke, 6));
    }

    private void DrawMagentaSpark()
    {
        MagCpuSparkCanvas.Children.Clear();
        double w = MagCpuSparkCanvas.ActualWidth, h = MagCpuSparkCanvas.ActualHeight;
        if (w < 20 || h < 20) return;

        var pts = _dash.TakeLast(60).Select(s => (double)(s.CpuL ?? 10)).ToList();
        if (pts.Count < 2) pts = new List<double> { 15, 22, 18, 35, 29, 41, 32 };

        var neon = Color.FromRgb(224, 64, 251);
        var line = new Polyline { Stroke = new SolidColorBrush(neon), StrokeThickness = 2.2, StrokeLineJoin = PenLineJoin.Round };
        var area = new Polygon
        {
            Fill = new LinearGradientBrush(Color.FromArgb(0x55, 224, 64, 251), Color.FromArgb(0x04, 224, 64, 251), new Point(0, 0), new Point(0, 1))
        };

        for (int i = 0; i < pts.Count; i++)
        {
            double x = w * i / Math.Max(1, pts.Count - 1);
            double y = h - 4 - Math.Clamp(pts[i] / 100.0, 0.04, 0.95) * (h - 10);
            line.Points.Add(new Point(x, y));
            area.Points.Add(new Point(x, y));
        }
        area.Points.Add(new Point(w, h));
        area.Points.Add(new Point(0, h));
        MagCpuSparkCanvas.Children.Add(area);
        MagCpuSparkCanvas.Children.Add(line);
    }

    private void UpdateAfterburnerGauges()
    {
        if (DashAfterburnerDeck == null || DashAfterburnerDeck.Visibility != Visibility.Visible) return;
        var accent = (Brush)FindResource("Accent");
        var accent2 = (Brush)FindResource("AccentSecondary");

        float? cpuT = _monitor.CpuTemperature;
        float? cpuL = _monitor.CpuLoad;
        float? cpuW = _monitor.CpuPower;
        float? gpuT = MainGpuTemp();
        float? gpuL = MainGpuLoad();
        uint? gpuClk = (_gpus.Count > 0 && _gpus[0].NvmlHandle != IntPtr.Zero) ? Nvml.Clock(_gpus[0].NvmlHandle, Nvml.ClockGraphics) : null;

        GaugeCpuTempVal.Text = cpuT is float ct ? $"{ct:0}°" : "—";
        GaugeCpuTempSub.Text = cpuW is float cw ? $"{cw:0} W" : "°C";
        DrawArcGauge(GaugeCpuTempCanvas, cpuT is float ct2 ? Math.Clamp((ct2 - 20) / 80.0, 0, 1) : 0, accent);

        GaugeCpuLoadVal.Text = cpuL is float cl ? $"{cl:0}%" : "—";
        GaugeCpuLoadSub.Text = CpuClockAvg() is float cc ? $"{cc:0} MHz" : "uso";
        DrawArcGauge(GaugeCpuLoadCanvas, cpuL is float cl2 ? Math.Clamp(cl2 / 100.0, 0, 1) : 0, accent2);

        GaugeGpuTempVal.Text = gpuT is float gt ? $"{gt:0}°" : "—";
        GaugeGpuTempSub.Text = MainGpuPower() is float gw ? $"{gw:0} W" : "°C";
        DrawArcGauge(GaugeGpuTempCanvas, gpuT is float gt2 ? Math.Clamp((gt2 - 20) / 80.0, 0, 1) : 0, accent);

        GaugeGpuLoadVal.Text = gpuL is float gl ? $"{gl:0}%" : "—";
        GaugeGpuLoadSub.Text = gpuClk is uint gc ? $"{gc} MHz" : "uso";
        DrawArcGauge(GaugeGpuLoadCanvas, gpuL is float gl2 ? Math.Clamp(gl2 / 100.0, 0, 1) : 0, accent2);
    }

    private void DrawArcGauge(Canvas canvas, double fraction, Brush activeBrush)
    {
        canvas.Children.Clear();
        double cx = canvas.Width / 2, cy = canvas.Height / 2 + 4, r = 46;
        const double startDeg = 140; // esquerda inferior
        const double sweepMax = 260; // até direita inferior

        var trackBrush = (Brush)FindResource("Line");
        var tickBrush = (Brush)FindResource("Faint");

        // Marcações de escala estilo tacômetro
        for (int i = 0; i <= 10; i++)
        {
            double ang = (startDeg + (sweepMax * i / 10.0)) * Math.PI / 180.0;
            double r1 = r - 7, r2 = r - 3;
            canvas.Children.Add(new Line
            {
                X1 = cx + r1 * Math.Cos(ang), Y1 = cy + r1 * Math.Sin(ang),
                X2 = cx + r2 * Math.Cos(ang), Y2 = cy + r2 * Math.Sin(ang),
                Stroke = tickBrush, StrokeThickness = i % 5 == 0 ? 1.5 : 0.8
            });
        }

        // Arco de fundo
        canvas.Children.Add(BuildArcPath(cx, cy, r, startDeg, sweepMax, trackBrush, 7));

        // Arco ativo
        if (fraction > 0.01)
            canvas.Children.Add(BuildArcPath(cx, cy, r, startDeg, sweepMax * fraction, activeBrush, 7));
    }

    private static Path BuildArcPath(double cx, double cy, double r, double startDeg, double sweepDeg, Brush stroke, double thickness)
    {
        double a1 = startDeg * Math.PI / 180.0;
        double a2 = (startDeg + sweepDeg) * Math.PI / 180.0;
        var p1 = new Point(cx + r * Math.Cos(a1), cy + r * Math.Sin(a1));
        var p2 = new Point(cx + r * Math.Cos(a2), cy + r * Math.Sin(a2));
        var seg = new ArcSegment(p2, new Size(r, r), 0, sweepDeg > 180, SweepDirection.Clockwise, true);
        var fig = new PathFigure(p1, new[] { seg }, false);
        return new Path
        {
            Stroke = stroke,
            StrokeThickness = thickness,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round,
            Data = new PathGeometry(new[] { fig })
        };
    }

    private static string F(float? v, string fmt, string unit) => v is float x ? x.ToString(fmt) + unit : "—";

    private void UpdateDashComponents()
    {
        var rows = new List<DashComponent>
        {
            new("", LocalizationManager.Get("Processador"), GameCatalog.ShortCpuName(_cpuName),
                F(_monitor.CpuTemperature, "0", " °C"), F(_monitor.CpuLoad, "0", " %"), F(CpuClockAvg(), "0", " MHz"),
                F(_monitor.CpuPower, "0", " W"), "PageSensors"),
        };
        if (_gpus.Count > 0)
        {
            uint? clk = _gpus[0].NvmlHandle != IntPtr.Zero ? Nvml.Clock(_gpus[0].NvmlHandle, Nvml.ClockGraphics) : null;
            rows.Add(new("", LocalizationManager.Get("Placa de vídeo"), GameCatalog.ShortGpuName(_gpus[0].Name),
                         F(MainGpuTemp(), "0", " °C"), F(MainGpuLoad(), "0", " %"), clk is uint c ? $"{c} MHz" : "—",
                         F(MainGpuPower(), "0", " W"), "PageGpu"));
        }
        var (total, avail) = Optimizer.MemoryStatus();
        if (total > 0)
        {
            double used = total - avail;
            string installed = LocalizationManager.CurrentLanguage switch {
                "en" => $"{total / 1073741824.0:0} GB installed",
                "es" => $"{total / 1073741824.0:0} GB instalados",
                "ru" => $"{total / 1073741824.0:0} ГБ установлено",
                "zh" => $"已安装 {total / 1073741824.0:0} GB",
                _ => $"{total / 1073741824.0:0} GB instalados"
            };
            rows.Add(new("", LocalizationManager.Get("Memória RAM"), installed, "—", $"{used / total:P0}", "—",
                         $"{used / 1073741824.0:0.0} GB", "PageSystem"));
        }
        if (_drives.FirstOrDefault(d => d.Letter.StartsWith("C", StringComparison.OrdinalIgnoreCase)) is { } c0)
        {
            string freeStr = LocalizationManager.CurrentLanguage switch {
                "en" => $"{DriveCard.Fmt(c0.Free)} free",
                "es" => $"{DriveCard.Fmt(c0.Free)} libres",
                "ru" => $"{DriveCard.Fmt(c0.Free)} свободно",
                "zh" => $"可用 {DriveCard.Fmt(c0.Free)}",
                _ => $"{DriveCard.Fmt(c0.Free)} livres"
            };
            string diskName = LocalizationManager.CurrentLanguage switch {
                "en" => $"Disk {c0.Letter}",
                "es" => $"Disco {c0.Letter}",
                "ru" => $"Диск {c0.Letter}",
                "zh" => $"磁盘 {c0.Letter}",
                _ => $"Disco {c0.Letter}"
            };
            rows.Add(new("", diskName, c0.Disk.Length > 0 ? c0.Disk : c0.Kind, "—", c0.UsedPercent, "—",
                         freeStr, "PageDisks"));
        }
        DashComponents.ItemsSource = rows;
    }

    private void UpdateDashScores()
    {
        var runs = _settings.BenchRuns.Where(r => r.Multi != null).ToList();
        if (runs.Count > 0)
        {
            DashScore.Text = $"{runs[0].Multi:N0}";
            DashScoreSub.Text = $"pts · render multi-core · {runs[0].Date:dd/MM HH:mm}";
            if (runs.Count > 1)
            {
                double d = runs[0].Multi!.Value - runs[1].Multi!.Value;
                DashScoreDelta.Text = d >= 0 ? $"▲ {d:N0}" : $"▼ {-d:N0}";
            }
            else DashScoreDelta.Text = "";
        }
        else { DashScore.Text = "—"; DashScoreDelta.Text = ""; DashScoreSub.Text = "rode o CSK BENCH para ter uma nota"; }

        string Last(string test) => _settings.BenchRecords.FirstOrDefault(r => r.Test == test)?.Summary.Split('·')[0].Trim() ?? "—";
        var single = _settings.BenchRuns.FirstOrDefault(r => r.Single != null)?.Single;
        DashScores.ItemsSource = new[]
        {
            new KeyValuePair<string, string>("Render single-core", single is double s ? $"{s:N0} pts" : "—"),
            new KeyValuePair<string, string>("Cena de jogo (GPU)", Last("gpu")),
            new KeyValuePair<string, string>("Simulação de jogo", Last("sim")),
        };
        DashCpuName.Text = GameCatalog.ShortCpuName(_cpuName);
        DashGpuName.Text = _gpus.Count > 0 ? GameCatalog.ShortGpuName(_gpus[0].Name) : "—";
    }

    // ---------------- gráfico de área ----------------

    private void DrawDashChart()
    {
        DashChart.Children.Clear();
        double w = DashChart.ActualWidth, h = DashChart.ActualHeight;
        if (w < 80 || h < 60) return;

        int range = int.Parse(CheckedTag(DashRangePanel) ?? "300");
        string metric = CheckedTag(DashMetricPanel) ?? "temp";
        Func<DashSample, float?> cpu = metric switch { "load" => s => s.CpuL, "power" => s => s.CpuW, _ => s => s.CpuT };
        Func<DashSample, float?> gpu = metric switch { "load" => s => s.GpuL, "power" => s => s.GpuW, _ => s => s.GpuT };
        string unit = metric switch { "load" => " %", "power" => " W", _ => " °C" };
        string name = metric switch { "load" => "CARGA", "power" => "CONSUMO", _ => "TEMPERATURA" };
        var data = _dash.Skip(Math.Max(0, _dash.Count - range)).ToList();

        // Cabeçalho: valor atual grande + variação no período (como o par e a cotação da referência).
        float? cNow = data.Count > 0 ? cpu(data[^1]) : null, gNow = data.Count > 0 ? gpu(data[^1]) : null;
        DashChartLabel.Text = $"CPU · {name}";
        DashChartValue.Text = cNow is float cv ? $"{cv:0.#}{unit}" : "—";
        DashChartValue2.Text = gNow is float gv ? $"{gv:0.#}{unit}" : "—";
        var firstC = data.Select(cpu).FirstOrDefault(v => v != null);
        DashChartDelta.Text = cNow is float a && firstC is float b
            ? $"{(a - b >= 0 ? "+" : "")}{a - b:0.#}{unit} em {(range >= 60 ? $"{range / 60} min" : $"{range} s")}"
            : cNow == null && metric != "load" ? "sem leitura da CPU (veja o aviso no menu)" : "coletando…";

        var all = data.SelectMany(s => new[] { cpu(s), gpu(s) }).Where(v => v != null).Select(v => (double)v!.Value).ToList();
        double yMin = metric == "temp" ? 20 : 0;
        double yMax = metric switch
        {
            "load" => 100,
            "power" => Math.Max(50, all.Count > 0 ? all.Max() * 1.2 : 50),
            _ => Math.Max(90, all.Count > 0 ? all.Max() + 5 : 90)
        };
        const double left = 48, bottom = 22, top = 8;
        double plotW = w - left, plotH = h - top - bottom;
        double X(int i) => left + plotW * i / Math.Max(1, range - 1) + plotW * (range - data.Count) / Math.Max(1, range - 1);
        double Y(double v) => top + plotH * (1 - (Math.Clamp(v, yMin, yMax) - yMin) / (yMax - yMin));

        var fg = (SolidColorBrush)FindResource("Fg");
        var faint = (Brush)FindResource("Faint");
        var mono = (FontFamily)FindResource("Mono");

        // Grade tracejada horizontal + rótulos (como na referência)
        for (int k = 0; k <= 2; k++)
        {
            double v = yMin + (yMax - yMin) * k / 2, y = Y(v);
            DashChart.Children.Add(new Line { X1 = left, X2 = w, Y1 = y, Y2 = y, Stroke = (Brush)FindResource("Line"), StrokeDashArray = new DoubleCollection { 5, 5 } });
            var lbl = new TextBlock { Text = $"{v:0}{unit.Trim()}", FontSize = 11, Foreground = faint, FontFamily = mono };
            Canvas.SetLeft(lbl, 0); Canvas.SetTop(lbl, y - 8);
            DashChart.Children.Add(lbl);
        }
        // Eixo de tempo
        string[] xs = { range >= 60 ? $"-{range / 60} min" : $"-{range} s", range >= 120 ? $"-{range / 120} min" : $"-{range / 2} s", "agora" };
        for (int k = 0; k < 3; k++)
        {
            var t = new TextBlock { Text = xs[k], FontSize = 11, Foreground = faint };
            t.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(t, Math.Clamp(left + plotW * k / 2 - t.DesiredSize.Width / 2, left, w - t.DesiredSize.Width));
            Canvas.SetTop(t, h - 16);
            DashChart.Children.Add(t);
        }

        if (data.Count < 2) return;

        // GPU: linha tracejada discreta
        var gLine = new Polyline { Stroke = (Brush)FindResource("Muted"), StrokeThickness = 1.5, StrokeDashArray = new DoubleCollection { 3, 2 }, StrokeLineJoin = PenLineJoin.Round };
        for (int i = 0; i < data.Count; i++) if (gpu(data[i]) is float g) gLine.Points.Add(new Point(X(i), Y(g)));
        DashChart.Children.Add(gLine);

        // CPU: área com degradê monocromático + linha
        var pts = new List<Point>();
        for (int i = 0; i < data.Count; i++) if (cpu(data[i]) is float c) pts.Add(new Point(X(i), Y(c)));
        if (pts.Count < 2) return;
        var col = fg.Color;
        var area = new Polygon
        {
            Fill = new LinearGradientBrush(Color.FromArgb(0x55, col.R, col.G, col.B), Color.FromArgb(0x00, col.R, col.G, col.B), 90)
        };
        foreach (var p in pts) area.Points.Add(p);
        area.Points.Add(new Point(pts[^1].X, top + plotH));
        area.Points.Add(new Point(pts[0].X, top + plotH));
        DashChart.Children.Add(area);
        var line = new Polyline { Stroke = fg, StrokeThickness = 2, StrokeLineJoin = PenLineJoin.Round };
        foreach (var p in pts) line.Points.Add(p);
        DashChart.Children.Add(line);

        // Marcador do valor atual com balão (como o "76,000 B" da referência)
        var last = pts[^1];
        DashChart.Children.Add(new Line { X1 = last.X, X2 = last.X, Y1 = top, Y2 = top + plotH, Stroke = faint, StrokeDashArray = new DoubleCollection { 3, 3 } });
        var dot = new Ellipse { Width = 11, Height = 11, Stroke = fg, StrokeThickness = 2, Fill = (Brush)FindResource("Card") };
        Canvas.SetLeft(dot, last.X - 5.5); Canvas.SetTop(dot, last.Y - 5.5);
        DashChart.Children.Add(dot);
        var bubble = new Border
        {
            Background = (Brush)FindResource("Hover"), CornerRadius = new CornerRadius(8), Padding = new Thickness(10, 5, 10, 5),
            Child = new TextBlock { Text = DashChartValue.Text, FontFamily = mono, FontWeight = FontWeights.SemiBold, Foreground = fg }
        };
        bubble.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        Canvas.SetLeft(bubble, Math.Max(left, last.X - bubble.DesiredSize.Width - 10));
        Canvas.SetTop(bubble, Math.Clamp(last.Y - bubble.DesiredSize.Height - 10, 0, h - bubble.DesiredSize.Height));
        DashChart.Children.Add(bubble);
    }

    // ---------------- anéis concêntricos ----------------

    private void UpdateDashRings()
    {
        var (total, avail) = Optimizer.MemoryStatus();
        double ram = total > 0 ? (double)(total - avail) / total : 0;
        double? vram = null;
        if (_gpus.Count > 0 && _gpus[0].NvmlHandle != IntPtr.Zero && Nvml.MemoryInfo(_gpus[0].NvmlHandle) is Nvml.Memory m && m.Total > 0)
            vram = (double)m.Used / m.Total;
        var c0 = _drives.FirstOrDefault(d => d.Letter.StartsWith("C", StringComparison.OrdinalIgnoreCase));

        DashMemHero.Text = total > 0 ? $"{(total - avail) / 1073741824.0:0.0} GB" : "—";
        string memSubFmt = LocalizationManager.CurrentLanguage switch {
            "en" => $"RAM in use · {total / 1073741824.0:0} GB installed",
            "es" => $"de RAM en uso · {total / 1073741824.0:0} GB instalados",
            "ru" => $"ОЗУ занято · {total / 1073741824.0:0} ГБ установлено",
            "zh" => $"运行内存已占用 · 共 {total / 1073741824.0:0} GB",
            _ => $"de RAM em uso · {total / 1073741824.0:0} GB instalados"
        };
        DashMemSub.Text = total > 0 ? memSubFmt : "";

        Brush fg = (Brush)FindResource("Fg"), muted = (Brush)FindResource("Muted"), faint = (Brush)FindResource("Faint");
        var rings = new List<(double Frac, Brush Brush, string Label)> { (ram, fg, "RAM") };
        if (vram is double v) rings.Add((v, muted, "VRAM"));
        if (c0 != null) rings.Add((c0.UsedFraction, faint, $"Disco {c0.Letter}"));

        DashRings.Children.Clear();
        double cx = DashRings.Width / 2, cy = DashRings.Height / 2;
        for (int i = 0; i < rings.Count; i++)
        {
            double r = 58 - i * 16;
            DashRings.Children.Add(new Ellipse
            {
                Width = r * 2, Height = r * 2, Stroke = (Brush)FindResource("Line"), StrokeThickness = 9,
                Margin = new Thickness(cx - r, cy - r, 0, 0)
            });
            var arc = new Path { Data = RingArc(rings[i].Frac, cx, cy, r), Stroke = rings[i].Brush, StrokeThickness = 9,
                                 StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round };
            DashRings.Children.Add(arc);
        }
        var center = new TextBlock { Text = $"{ram:P0}", FontFamily = (FontFamily)FindResource("Mono"), FontSize = 13, FontWeight = FontWeights.SemiBold,
                                     Foreground = fg };
        center.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        Canvas.SetLeft(center, cx - center.DesiredSize.Width / 2); Canvas.SetTop(center, cy - center.DesiredSize.Height / 2);
        DashRings.Children.Add(center);

        DashRingLegend.ItemsSource = rings.Select(r => new DashRingItem(r.Brush, r.Label, $"{r.Frac:P0}")).ToList();
    }

    private static Geometry RingArc(double frac, double cx, double cy, double r)
    {
        frac = Math.Clamp(frac, 0, 0.9999);
        if (frac <= 0.001) return Geometry.Empty;
        double a = frac * 2 * Math.PI;
        var fig = new PathFigure { StartPoint = new Point(cx, cy - r) };
        fig.Segments.Add(new ArcSegment(new Point(cx + r * Math.Sin(a), cy - r * Math.Cos(a)), new Size(r, r), 0, frac > 0.5, SweepDirection.Clockwise, true));
        var g = new PathGeometry(new[] { fig });
        g.Freeze();
        return g;
    }

    // ---------------- saúde do PC ----------------

    private void UpdateDashHealth()
    {
        _healthAt = DateTime.Now;
        DashCheck Item(bool ok, string title, string okText, string badText) => new(title, ok ? okText : badText, ok);

        var list = new List<DashCheck>();
        if (_monitor.IsOpen) // enquanto detecta o hardware, ainda não dá para saber
        {
            var sensors = _monitor.DiagnoseCpuSensors();
            list.Add(Item(sensors == SensorMonitor.CpuSensorProblem.None,
                          LocalizationManager.Get("Sensores da CPU"),
                          LocalizationManager.Get("Temperatura e consumo disponíveis."),
                          SensorMonitor.Explain(sensors).Split('.')[0] + "."));
        }

        var errs = OcCheck.ErrorsSince(DateTime.Now.AddDays(-7));
        list.Add(Item(errs.Error == null && errs.Total == 0,
                      LocalizationManager.Get("Sem erros de hardware"),
                      LocalizationManager.Get("Nenhum WHEA ou reinício do driver de vídeo em 7 dias."),
                      errs.Error != null ? LocalizationManager.Get("Não foi possível ler o registro do Windows.")
                                         : $"{errs.Whea} WHEA · {errs.GpuResets} resets."));

        var gm = _tweaks.FirstOrDefault(t => t.Id == "gamemode");
        var dvr = _tweaks.FirstOrDefault(t => t.Id == "gamedvr");
        try { gm?.Refresh(); dvr?.Refresh(); } catch { }
        if (gm != null) list.Add(Item(gm.IsOn,
                                     LocalizationManager.Get("Modo de Jogo ligado"),
                                     LocalizationManager.Get("O Windows prioriza o jogo."),
                                     LocalizationManager.Get("Desligado: ligue em CSK OTIMIZA.")));
        if (dvr != null) list.Add(Item(dvr.IsOn,
                                      LocalizationManager.Get("Gravação em segundo plano desligada"),
                                      LocalizationManager.Get("O Game Bar não grava o tempo todo."),
                                      LocalizationManager.Get("O Game DVR está gravando: custa FPS.")));

        _health = list;
        DashHealth.ItemsSource = _health;
    }
}
