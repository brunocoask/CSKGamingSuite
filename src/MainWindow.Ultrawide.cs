using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CSKGamingSuite;

/// <summary>
/// Gerenciador de Divisão de Janelas e Produtividade para Monitores Ultrawide (ex: LG UltraGear 34" 21:9).
/// </summary>
public partial class MainWindow
{
    private UltrawideManager.UltrawidePreset _currentPreset = UltrawideManager.UltrawidePreset.ThreeColumns_UltrawideMaster;
    private List<WindowSnapTarget> _cachedOpenWindows = new();
    private int _selectedZoneIndex = 0;
    private int _ultrawideGap = 8;

    private void InitUltrawide()
    {
        RefreshUltrawideMonitors();
        RefreshUltrawideWindows();
        RenderUltrawideZonesPreview();
    }

    private void RefreshUltrawideMonitors()
    {
        var monitors = UltrawideManager.GetMonitors();
        var primary = monitors.FirstOrDefault(m => m.IsPrimary) ?? monitors[0];

        TxtMonitorInfo.Text = $"{primary.DeviceName} ({primary.Bounds.Width:0}x{primary.Bounds.Height:0})";
        TxtMonitorAspect.Text = (primary.Bounds.Width / primary.Bounds.Height > 2.0)
            ? "Ultrawide 21:9 Detectado · Perfeito para multitarefas e jogos"
            : "Widescreen 16:9 Detectado";
    }

    private void RefreshUltrawideWindows()
    {
        _cachedOpenWindows = UltrawideManager.GetOpenWindows();
        ListUltrawideWindows.ItemsSource = _cachedOpenWindows;
    }

    private void RenderUltrawideZonesPreview()
    {
        CanvasZonesPreview.Children.Clear();
        var monitors = UltrawideManager.GetMonitors();
        var primary = monitors.FirstOrDefault(m => m.IsPrimary) ?? monitors[0];

        var zones = UltrawideManager.CalculateZones(_currentPreset, primary.WorkArea, _ultrawideGap);

        double canvasW = CanvasZonesPreview.ActualWidth > 0 ? CanvasZonesPreview.ActualWidth : 740;
        double canvasH = CanvasZonesPreview.ActualHeight > 0 ? CanvasZonesPreview.ActualHeight : 180;

        double scaleX = canvasW / primary.WorkArea.Width;
        double scaleY = canvasH / primary.WorkArea.Height;

        for (int i = 0; i < zones.Count; i++)
        {
            var z = zones[i];
            int zoneIdx = i;

            double left = (z.X - primary.WorkArea.X) * scaleX;
            double top = (z.Y - primary.WorkArea.Y) * scaleY;
            double w = z.Width * scaleX;
            double h = z.Height * scaleY;

            bool isSelected = (i == _selectedZoneIndex);

            var border = new Border
            {
                Width = Math.Max(10, w - 4),
                Height = Math.Max(10, h - 4),
                CornerRadius = new CornerRadius(10),
                Background = isSelected 
                    ? new SolidColorBrush(System.Windows.Media.Color.FromArgb(80, 0, 113, 227)) 
                    : new SolidColorBrush(System.Windows.Media.Color.FromArgb(40, 255, 255, 255)),
                BorderBrush = isSelected 
                    ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 113, 227)) 
                    : new SolidColorBrush(System.Windows.Media.Color.FromArgb(60, 255, 255, 255)),
                BorderThickness = new Thickness(isSelected ? 2 : 1),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            var stack = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            string label = GetZoneLabel(_currentPreset, i);
            stack.Children.Add(new TextBlock
            {
                Text = $"ZONA {i + 1}",
                FontWeight = FontWeights.Bold,
                FontSize = 13,
                Foreground = (Brush)FindResource(isSelected ? "Accent" : "Fg"),
                HorizontalAlignment = HorizontalAlignment.Center
            });

            stack.Children.Add(new TextBlock
            {
                Text = label,
                FontSize = 11,
                Foreground = (Brush)FindResource("Muted"),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 4, 0, 0)
            });

            stack.Children.Add(new TextBlock
            {
                Text = $"{z.Width:0} x {z.Height:0}",
                FontSize = 10,
                FontFamily = (System.Windows.Media.FontFamily)FindResource("Mono"),
                Foreground = (Brush)FindResource("Faint"),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 2, 0, 0)
            });

            border.Child = stack;

            border.MouseLeftButtonDown += (_, _) =>
            {
                _selectedZoneIndex = zoneIdx;
                TxtSelectedZoneStatus.Text = $"Zona {zoneIdx + 1} Selecionada ({label})";
                RenderUltrawideZonesPreview();
            };

            Canvas.SetLeft(border, left + 2);
            Canvas.SetTop(border, top + 2);

            CanvasZonesPreview.Children.Add(border);
        }
    }

    private string GetZoneLabel(UltrawideManager.UltrawidePreset preset, int index)
    {
        return preset switch
        {
            UltrawideManager.UltrawidePreset.ThreeColumns_UltrawideMaster => index switch
            {
                0 => "Lateral (25%)\nWhatsApp / Chat",
                1 => "Principal (50%)\nJogo / Navegador",
                _ => "Lateral (25%)\nPlanilhas / Docs"
            },
            UltrawideManager.UltrawidePreset.TwoHalves_50_50 => index switch
            {
                0 => "Metade Esquerda (50%)",
                _ => "Metade Direita (50%)"
            },
            UltrawideManager.UltrawidePreset.ThreeColumns_Equal => $"Coluna {index + 1} (33%)",
            UltrawideManager.UltrawidePreset.FourColumns_Equal => $"Coluna {index + 1} (25%)",
            UltrawideManager.UltrawidePreset.TwoByTwo_Grid => index switch
            {
                0 => "Topo Esquerda",
                1 => "Topo Direita",
                2 => "Inferior Esquerda",
                _ => "Inferior Direita"
            },
            UltrawideManager.UltrawidePreset.GamingFocus_Center => index switch
            {
                0 => "Aba Esquerda (20%)",
                1 => "Foco Jogo (60%)",
                _ => "Aba Direita (20%)"
            },
            _ => $"Zona {index + 1}"
        };
    }

    private void Preset_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string tag)
        {
            _currentPreset = tag switch
            {
                "TwoHalves" => UltrawideManager.UltrawidePreset.TwoHalves_50_50,
                "ThreeEqual" => UltrawideManager.UltrawidePreset.ThreeColumns_Equal,
                "ThreeMaster" => UltrawideManager.UltrawidePreset.ThreeColumns_UltrawideMaster,
                "GamingFocus" => UltrawideManager.UltrawidePreset.GamingFocus_Center,
                "FourColumns" => UltrawideManager.UltrawidePreset.FourColumns_Equal,
                "Grid2x2" => UltrawideManager.UltrawidePreset.TwoByTwo_Grid,
                _ => UltrawideManager.UltrawidePreset.ThreeColumns_UltrawideMaster
            };

            _selectedZoneIndex = 0;
            TxtSelectedZoneStatus.Text = $"Zona 1 Selecionada";
            RenderUltrawideZonesPreview();
        }
    }

    private void BtnSnapWindowToZone_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is WindowSnapTarget target)
        {
            var monitors = UltrawideManager.GetMonitors();
            var primary = monitors.FirstOrDefault(m => m.IsPrimary) ?? monitors[0];
            var zones = UltrawideManager.CalculateZones(_currentPreset, primary.WorkArea, _ultrawideGap);

            if (_selectedZoneIndex >= 0 && _selectedZoneIndex < zones.Count)
            {
                var targetZone = zones[_selectedZoneIndex];
                UltrawideManager.SnapWindow(target.Handle, targetZone);
                Toast("Janela Posicionada", $"{target.Title} encaixada na Zona {_selectedZoneIndex + 1}.", "");
            }
        }
    }

    private void BtnAutoArrange_Click(object sender, RoutedEventArgs e)
    {
        var monitors = UltrawideManager.GetMonitors();
        var primary = monitors.FirstOrDefault(m => m.IsPrimary) ?? monitors[0];
        var zones = UltrawideManager.CalculateZones(_currentPreset, primary.WorkArea, _ultrawideGap);

        RefreshUltrawideWindows();
        var windows = _cachedOpenWindows.Take(zones.Count).ToList();

        for (int i = 0; i < windows.Count && i < zones.Count; i++)
        {
            UltrawideManager.SnapWindow(windows[i].Handle, zones[i]);
        }

        Toast("Janelas Organizadas", $"{windows.Count} janelas foram ajustadas automaticamente para a divisão {_currentPreset}.", "");
    }

    private void BtnRefreshUltrawideWindows_Click(object sender, RoutedEventArgs e)
    {
        RefreshUltrawideWindows();
    }

    private void CanvasZonesPreview_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        RenderUltrawideZonesPreview();
    }
}
