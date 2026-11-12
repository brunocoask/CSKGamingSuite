using System.Windows;
using System.Windows.Media;

namespace CSKGamingSuite;

public sealed record SkinPalette(
    string Bg,
    string Panel,
    string Card,
    string Hover,
    string Line,
    string LineStrong,
    string Fg,
    string Muted,
    string Faint,
    string Ink,
    string Accent,
    string AccentInk,
    string AccentSecondary,
    string Gold,
    string GoldSoft,
    string ThumbBrush,
    string ThumbHover,
    string ThumbDrag);

public sealed record SkinDefinition(
    string Id,
    string Name,
    string Description,
    bool SupportsLightDark,
    bool UseTrafficLights,
    double CardRadius,
    double ButtonRadius,
    double SidebarRadius,
    double NavRadius,
    double ChipRadius,
    string SwatchPrimary,
    string SwatchBg,
    SkinPalette Dark,
    SkinPalette? Light = null);

/// <summary>
/// Motor de skins e temas do CSK Gaming Suite.
/// Substitui pincéis e geometrias (CornerRadius) no ResourceDictionary da aplicação via DynamicResource,
/// permitindo troca instantânea sem recarregar janelas. Overlay e aviso de dispositivo ficam sempre escuros.
/// </summary>
public static class ThemeManager
{
    public static bool IsLight { get; private set; }
    public static SkinDefinition CurrentSkin { get; private set; }

    public static IReadOnlyList<SkinDefinition> Skins { get; } = new SkinDefinition[]
    {
        new(
            Id: "clean-glass",
            Name: "Clean Glass",
            Description: "Minimalista com cantos suaves e vidro limpo",
            SupportsLightDark: true,
            UseTrafficLights: true,
            CardRadius: 14,
            ButtonRadius: 10,
            SidebarRadius: 16,
            NavRadius: 12,
            ChipRadius: 14,
            SwatchPrimary: "#0A84FF",
            SwatchBg: "#1E1E22",
            Dark: new(
                Bg: "#1E1E22", Panel: "#28282D", Card: "#2C2C32", Hover: "#383840",
                Line: "#3A3A42", LineStrong: "#484852", Fg: "#F5F5F7", Muted: "#A1A1A6",
                Faint: "#636366", Ink: "#1D1D1F", Accent: "#0A84FF", AccentInk: "#FFFFFF",
                AccentSecondary: "#5E5CE6", Gold: "#0A84FF", GoldSoft: "#64D2FF",
                ThumbBrush: "#3A3A42", ThumbHover: "#484852", ThumbDrag: "#636366"),
            Light: new(
                Bg: "#F2F3F7", Panel: "#FFFFFF", Card: "#FFFFFF", Hover: "#EAECEF",
                Line: "#E1E4EA", LineStrong: "#D2D6DF", Fg: "#1D1D1F", Muted: "#86868B",
                Faint: "#AEAEB2", Ink: "#FFFFFF", Accent: "#0071E3", AccentInk: "#FFFFFF",
                AccentSecondary: "#5856D6", Gold: "#0071E3", GoldSoft: "#47A3FF",
                ThumbBrush: "#D1D6DF", ThumbHover: "#AEAEB2", ThumbDrag: "#8E8E93")
        ),
        new(
            Id: "fluent-slate",
            Name: "Fluent Slate",
            Description: "Superfícies foscas modernas com cantos equilibrados",
            SupportsLightDark: true,
            UseTrafficLights: false,
            CardRadius: 8,
            ButtonRadius: 6,
            SidebarRadius: 10,
            NavRadius: 6,
            ChipRadius: 8,
            SwatchPrimary: "#0078D4",
            SwatchBg: "#202020",
            Dark: new(
                Bg: "#1C1C1C", Panel: "#242424", Card: "#2B2B2B", Hover: "#363636",
                Line: "#333333", LineStrong: "#454545", Fg: "#FFFFFF", Muted: "#ADADAD",
                Faint: "#6E6E6E", Ink: "#1C1C1C", Accent: "#0078D4", AccentInk: "#FFFFFF",
                AccentSecondary: "#60CDFF", Gold: "#60CDFF", GoldSoft: "#99EBFF",
                ThumbBrush: "#3D3D3D", ThumbHover: "#525252", ThumbDrag: "#6E6E6E"),
            Light: new(
                Bg: "#F3F3F3", Panel: "#FAFAFA", Card: "#FFFFFF", Hover: "#EAEAEA",
                Line: "#E5E5E5", LineStrong: "#CCCCCC", Fg: "#202020", Muted: "#606060",
                Faint: "#989898", Ink: "#FFFFFF", Accent: "#0067C0", AccentInk: "#FFFFFF",
                AccentSecondary: "#0078D4", Gold: "#0067C0", GoldSoft: "#4CC2FF",
                ThumbBrush: "#CCCCCC", ThumbHover: "#A6A6A6", ThumbDrag: "#888888")
        ),
        new(
            Id: "tactical-hud",
            Name: "Tactical HUD",
            Description: "Painel de telemetria industrial âmbar e grafite",
            SupportsLightDark: false,
            UseTrafficLights: false,
            CardRadius: 3,
            ButtonRadius: 2,
            SidebarRadius: 4,
            NavRadius: 3,
            ChipRadius: 3,
            SwatchPrimary: "#FF7A00",
            SwatchBg: "#0B0D10",
            Dark: new(
                Bg: "#0B0D10", Panel: "#12151A", Card: "#171B22", Hover: "#202630",
                Line: "#272D38", LineStrong: "#3A4352", Fg: "#E8EDF5", Muted: "#8C97A8",
                Faint: "#5A6475", Ink: "#0B0D10", Accent: "#FF7A00", AccentInk: "#0B0D10",
                AccentSecondary: "#FFB347", Gold: "#FF7A00", GoldSoft: "#FFB347",
                ThumbBrush: "#2C3340", ThumbHover: "#FF7A00", ThumbDrag: "#FFB347")
        ),
        new(
            Id: "night-grid",
            Name: "Night Grid",
            Description: "Interface futurista de alto contraste amarelo e ciano",
            SupportsLightDark: false,
            UseTrafficLights: false,
            CardRadius: 2,
            ButtonRadius: 0,
            SidebarRadius: 2,
            NavRadius: 2,
            ChipRadius: 2,
            SwatchPrimary: "#FCEE0C",
            SwatchBg: "#0D0E15",
            Dark: new(
                Bg: "#0D0E15", Panel: "#131522", Card: "#181B2C", Hover: "#222740",
                Line: "#2A3150", LineStrong: "#00F0FF", Fg: "#F4F6FF", Muted: "#939BC2",
                Faint: "#59628A", Ink: "#0D0E15", Accent: "#FCEE0C", AccentInk: "#0D0E15",
                AccentSecondary: "#00F0FF", Gold: "#00F0FF", GoldSoft: "#FCEE0C",
                ThumbBrush: "#2A3150", ThumbHover: "#00F0FF", ThumbDrag: "#FCEE0C")
        ),
        new(
            Id: "venom-matrix",
            Name: "Venom Matrix",
            Description: "Preto profundo com iluminação verde neon",
            SupportsLightDark: false,
            UseTrafficLights: false,
            CardRadius: 6,
            ButtonRadius: 4,
            SidebarRadius: 8,
            NavRadius: 5,
            ChipRadius: 6,
            SwatchPrimary: "#00E640",
            SwatchBg: "#0C0C0C",
            Dark: new(
                Bg: "#0C0C0C", Panel: "#141414", Card: "#1A1A1A", Hover: "#232823",
                Line: "#2A2E2A", LineStrong: "#3A443A", Fg: "#EDEDED", Muted: "#9AA39A",
                Faint: "#5E665E", Ink: "#0C0C0C", Accent: "#00E640", AccentInk: "#0C0C0C",
                AccentSecondary: "#00B332", Gold: "#00E640", GoldSoft: "#4DFF7A",
                ThumbBrush: "#2A2E2A", ThumbHover: "#00B332", ThumbDrag: "#00E640")
        ),
        new(
            Id: "crimson-overdrive",
            Name: "Crimson Overdrive",
            Description: "Chassi obsidian esportivo com acentos carmesim",
            SupportsLightDark: false,
            UseTrafficLights: false,
            CardRadius: 4,
            ButtonRadius: 2,
            SidebarRadius: 4,
            NavRadius: 3,
            ChipRadius: 4,
            SwatchPrimary: "#FF1E38",
            SwatchBg: "#0F0F12",
            Dark: new(
                Bg: "#0F0F12", Panel: "#17171C", Card: "#1E1E24", Hover: "#2B2226",
                Line: "#2E2E36", LineStrong: "#4A2830", Fg: "#F2F2F5", Muted: "#9E9EA8",
                Faint: "#62626E", Ink: "#0F0F12", Accent: "#FF1E38", AccentInk: "#FFFFFF",
                AccentSecondary: "#CC0028", Gold: "#FF1E38", GoldSoft: "#FF5C70",
                ThumbBrush: "#363640", ThumbHover: "#CC0028", ThumbDrag: "#FF1E38")
        ),
        new(
            Id: "ultraviolet-pulse",
            Name: "Ultraviolet Pulse",
            Description: "Overclock escuro com ciano elétrico e violeta",
            SupportsLightDark: false,
            UseTrafficLights: false,
            CardRadius: 10,
            ButtonRadius: 8,
            SidebarRadius: 12,
            NavRadius: 8,
            ChipRadius: 10,
            SwatchPrimary: "#00E5FF",
            SwatchBg: "#111116",
            Dark: new(
                Bg: "#111116", Panel: "#191922", Card: "#20202B", Hover: "#2B2B3A",
                Line: "#2E2E3E", LineStrong: "#464060", Fg: "#F5F5FF", Muted: "#9A9AB5",
                Faint: "#636380", Ink: "#111116", Accent: "#00E5FF", AccentInk: "#111116",
                AccentSecondary: "#9B51E0", Gold: "#9B51E0", GoldSoft: "#00E5FF",
                ThumbBrush: "#333346", ThumbHover: "#9B51E0", ThumbDrag: "#00E5FF")
        ),
    };

    static ThemeManager()
    {
        CurrentSkin = Skins[0];
    }

    public static void Apply(bool light) => Apply(CurrentSkin.Id, light);

    public static void Apply(string? skinId, bool light)
    {
        var skin = Skins.FirstOrDefault(s => s.Id == skinId) ?? Skins[0];
        CurrentSkin = skin;
        IsLight = skin.SupportsLightDark && light && skin.Light != null;

        var palette = IsLight ? skin.Light! : skin.Dark;
        var res = Application.Current.Resources;

        SetBrush(res, "Bg", palette.Bg);
        SetBrush(res, "Panel", palette.Panel);
        SetBrush(res, "Card", palette.Card);
        SetBrush(res, "Hover", palette.Hover);
        SetBrush(res, "Line", palette.Line);
        SetBrush(res, "LineStrong", palette.LineStrong);
        SetBrush(res, "Fg", palette.Fg);
        SetBrush(res, "Muted", palette.Muted);
        SetBrush(res, "Faint", palette.Faint);
        SetBrush(res, "Ink", palette.Ink);
        SetBrush(res, "Accent", palette.Accent);
        SetBrush(res, "AccentInk", palette.AccentInk);
        SetBrush(res, "AccentSecondary", palette.AccentSecondary);
        SetBrush(res, "Gold", palette.Gold);
        SetBrush(res, "GoldSoft", palette.GoldSoft);
        SetBrush(res, "ThumbBrush", palette.ThumbBrush);
        SetBrush(res, "ThumbHover", palette.ThumbHover);
        SetBrush(res, "ThumbDrag", palette.ThumbDrag);

        SetBrush(res, SystemColors.ControlBrushKey, palette.Bg);
        SetBrush(res, SystemColors.HighlightBrushKey, palette.Hover);
        SetBrush(res, SystemColors.InactiveSelectionHighlightBrushKey, palette.Hover);

        res["CardRadius"] = new CornerRadius(skin.CardRadius);
        res["ButtonRadius"] = new CornerRadius(skin.ButtonRadius);
        res["SidebarRadius"] = new CornerRadius(skin.SidebarRadius);
        res["NavRadius"] = new CornerRadius(skin.NavRadius);
        res["ChipRadius"] = new CornerRadius(skin.ChipRadius);
    }

    private static void SetBrush(ResourceDictionary res, object key, string hex)
    {
        var color = (Color)ColorConverter.ConvertFromString(hex);
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        res[key] = brush;
    }
}
