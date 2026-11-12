using System.Windows;
using System.Windows.Media;

namespace CSKGamingSuite;

/// <summary>
/// Troca entre modo escuro e claro substituindo os pincéis da paleta (Theme.xaml). O XAML os referencia com DynamicResource,
/// então a troca vale na hora, sem recarregar janelas. Overlay e aviso de dispositivo ficam sempre escuros (aparecem sobre jogos).
/// </summary>
public static class ThemeManager
{
    public static bool IsLight { get; private set; }

    private static readonly (object Key, string Dark, string Light)[] Palette =
    {
        ("Bg", "#1E1E22", "#F2F3F7"),
        ("Panel", "#28282D", "#FFFFFF"),
        ("Card", "#2C2C32", "#FFFFFF"),
        ("Hover", "#383840", "#EAECEF"),
        ("Line", "#3A3A42", "#E1E4EA"),
        ("LineStrong", "#484852", "#D2D6DF"),
        ("Fg", "#F5F5F7", "#1D1D1F"),
        ("Muted", "#A1A1A6", "#86868B"),
        ("Faint", "#636366", "#AEAEB2"),
        ("Ink", "#1D1D1F", "#FFFFFF"),
        ("Accent", "#0A84FF", "#0071E3"),
        ("AccentSecondary", "#5E5CE6", "#5856D6"),
        ("Gold", "#0A84FF", "#0071E3"),
        ("GoldSoft", "#64D2FF", "#47A3FF"),
        ("ThumbBrush", "#3A3A42", "#D1D1D6"),
        ("ThumbHover", "#484852", "#AEAEB2"),
        ("ThumbDrag", "#636366", "#8E8E93"),
        (SystemColors.ControlBrushKey, "#1E1E22", "#F2F3F7"),
        (SystemColors.HighlightBrushKey, "#383840", "#EAECEF"),
        (SystemColors.InactiveSelectionHighlightBrushKey, "#383840", "#EAECEF"),
    };

    public static void Apply(bool light)
    {
        IsLight = light;
        var res = Application.Current.Resources;
        foreach (var (key, dark, lightHex) in Palette)
        {
            var color = (Color)ColorConverter.ConvertFromString(light ? lightHex : dark);
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            res[key] = brush; // o XAML usa DynamicResource: trocar o pincel atualiza todos os controles na hora
        }
    }
}
