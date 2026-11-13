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
    bool DockedSidebar,
    bool ShowAfterburnerDeck,
    bool ShowBentoNoirDeck,
    string TextureType,
    string BadgeText,
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
/// Aplica paletas de cores, texturas de fundo (grade técnica, fibra de carbono, scanlines, gradientes),
/// geometrias (CornerRadius) e layout de mostradores estilo painel de hardware em tempo real.
/// </summary>
public static class ThemeManager
{
    public static bool IsLight { get; private set; }
    public static SkinDefinition CurrentSkin { get; private set; }

    public static IReadOnlyList<SkinDefinition> Skins { get; } = new SkinDefinition[]
    {
        new(
            Id: "obsidian-bento",
            Name: "Obsidian Bento",
            Description: "Bento Grid escuro minimalista com esmeralda, ciano e anéis de precisão",
            SupportsLightDark: false,
            UseTrafficLights: false,
            DockedSidebar: true,
            ShowAfterburnerDeck: false,
            ShowBentoNoirDeck: true,
            TextureType: "obsidian",
            BadgeText: "NOIR BENTO",
            CardRadius: 16,
            ButtonRadius: 16,
            SidebarRadius: 0,
            NavRadius: 10,
            ChipRadius: 14,
            SwatchPrimary: "#10D68E",
            SwatchBg: "#0A0B0D",
            Dark: new(
                Bg: "#0A0B0D", Panel: "#0E1013", Card: "#131519", Hover: "#1B1F24",
                Line: "#1F242B", LineStrong: "#10D68E", Fg: "#F3F5F7", Muted: "#8A94A0",
                Faint: "#525B66", Ink: "#0A0B0D", Accent: "#10D68E", AccentInk: "#06140E",
                AccentSecondary: "#4CC9F0", Gold: "#10D68E", GoldSoft: "#4CC9F0",
                ThumbBrush: "#232830", ThumbHover: "#10D68E", ThumbDrag: "#4CC9F0")
        ),
        new(
            Id: "clean-glass",
            Name: "Clean Glass",
            Description: "Minimalista com cantos suaves e painel flutuante",
            SupportsLightDark: true,
            UseTrafficLights: true,
            DockedSidebar: false,
            ShowAfterburnerDeck: false,
            ShowBentoNoirDeck: false,
            TextureType: "solid",
            BadgeText: "",
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
            Description: "Painel integrado fosco com barra lateral encostada na borda",
            SupportsLightDark: true,
            UseTrafficLights: false,
            DockedSidebar: true,
            ShowAfterburnerDeck: false,
            ShowBentoNoirDeck: false,
            TextureType: "gradient",
            BadgeText: "FLUENT",
            CardRadius: 8,
            ButtonRadius: 6,
            SidebarRadius: 0,
            NavRadius: 6,
            ChipRadius: 8,
            SwatchPrimary: "#0078D4",
            SwatchBg: "#202020",
            Dark: new(
                Bg: "#1B1C20", Panel: "#222429", Card: "#2A2C32", Hover: "#353840",
                Line: "#343740", LineStrong: "#484C58", Fg: "#FFFFFF", Muted: "#ADADAD",
                Faint: "#6E6E6E", Ink: "#1C1C1C", Accent: "#0078D4", AccentInk: "#FFFFFF",
                AccentSecondary: "#60CDFF", Gold: "#60CDFF", GoldSoft: "#99EBFF",
                ThumbBrush: "#3D3D3D", ThumbHover: "#525252", ThumbDrag: "#6E6E6E"),
            Light: new(
                Bg: "#F3F4F6", Panel: "#FAFAFA", Card: "#FFFFFF", Hover: "#EAEAEA",
                Line: "#E5E5E5", LineStrong: "#CCCCCC", Fg: "#202020", Muted: "#606060",
                Faint: "#989898", Ink: "#FFFFFF", Accent: "#0067C0", AccentInk: "#FFFFFF",
                AccentSecondary: "#0078D4", Gold: "#0067C0", GoldSoft: "#4CC2FF",
                ThumbBrush: "#CCCCCC", ThumbHover: "#A6A6A6", ThumbDrag: "#888888")
        ),
        new(
            Id: "tactical-hud",
            Name: "Tactical HUD",
            Description: "Cockpit de telemetria com grade técnica e mostradores âmbar",
            SupportsLightDark: false,
            UseTrafficLights: false,
            DockedSidebar: true,
            ShowAfterburnerDeck: true,
            ShowBentoNoirDeck: false,
            TextureType: "grid",
            BadgeText: "TELEMETRY HUD",
            CardRadius: 3,
            ButtonRadius: 2,
            SidebarRadius: 0,
            NavRadius: 3,
            ChipRadius: 3,
            SwatchPrimary: "#FF7A00",
            SwatchBg: "#0B0D10",
            Dark: new(
                Bg: "#0B0D10", Panel: "#12151A", Card: "#161A22", Hover: "#1F2631",
                Line: "#272E3A", LineStrong: "#FF7A00", Fg: "#E8EDF5", Muted: "#8C97A8",
                Faint: "#5A6475", Ink: "#0B0D10", Accent: "#FF7A00", AccentInk: "#0B0D10",
                AccentSecondary: "#FFB347", Gold: "#FF7A00", GoldSoft: "#FFB347",
                ThumbBrush: "#2C3340", ThumbHover: "#FF7A00", ThumbDrag: "#FFB347")
        ),
        new(
            Id: "night-grid",
            Name: "Night Grid",
            Description: "Deck futurista com scanlines, cantos afiados e mostradores neon",
            SupportsLightDark: false,
            UseTrafficLights: false,
            DockedSidebar: false,
            ShowAfterburnerDeck: true,
            ShowBentoNoirDeck: false,
            TextureType: "scanlines",
            BadgeText: "CYBER DECK",
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
            Description: "Textura fibra de carbono com mostradores e LEDs verde neon",
            SupportsLightDark: false,
            UseTrafficLights: false,
            DockedSidebar: false,
            ShowAfterburnerDeck: true,
            ShowBentoNoirDeck: false,
            TextureType: "carbon",
            BadgeText: "STEALTH MATRIX",
            CardRadius: 6,
            ButtonRadius: 4,
            SidebarRadius: 8,
            NavRadius: 5,
            ChipRadius: 6,
            SwatchPrimary: "#00E640",
            SwatchBg: "#0C0C0C",
            Dark: new(
                Bg: "#0B0C0B", Panel: "#131513", Card: "#191C19", Hover: "#222A22",
                Line: "#283128", LineStrong: "#00E640", Fg: "#EDEDED", Muted: "#9AA39A",
                Faint: "#5E665E", Ink: "#0B0C0B", Accent: "#00E640", AccentInk: "#0B0C0B",
                AccentSecondary: "#4DFF7A", Gold: "#00E640", GoldSoft: "#4DFF7A",
                ThumbBrush: "#2A2E2A", ThumbHover: "#00B332", ThumbDrag: "#00E640")
        ),
        new(
            Id: "crimson-overdrive",
            Name: "Crimson Overdrive",
            Description: "Painel racing diagonal com tacômetros e acentos carmesim",
            SupportsLightDark: false,
            UseTrafficLights: false,
            DockedSidebar: true,
            ShowAfterburnerDeck: true,
            ShowBentoNoirDeck: false,
            TextureType: "diagonal",
            BadgeText: "OVERDRIVE",
            CardRadius: 4,
            ButtonRadius: 2,
            SidebarRadius: 0,
            NavRadius: 3,
            ChipRadius: 4,
            SwatchPrimary: "#FF1E38",
            SwatchBg: "#0F0F12",
            Dark: new(
                Bg: "#0E0E11", Panel: "#16161B", Card: "#1D1D24", Hover: "#2C1E23",
                Line: "#2F2A32", LineStrong: "#FF1E38", Fg: "#F2F2F5", Muted: "#9E9EA8",
                Faint: "#62626E", Ink: "#0E0E11", Accent: "#FF1E38", AccentInk: "#FFFFFF",
                AccentSecondary: "#FF6B35", Gold: "#FF1E38", GoldSoft: "#FF5C70",
                ThumbBrush: "#363640", ThumbHover: "#CC0028", ThumbDrag: "#FF1E38")
        ),
        new(
            Id: "ultraviolet-pulse",
            Name: "Ultraviolet Pulse",
            Description: "Estação de overclock com gradiente metálico e mostradores ciano",
            SupportsLightDark: false,
            UseTrafficLights: false,
            DockedSidebar: false,
            ShowAfterburnerDeck: true,
            ShowBentoNoirDeck: false,
            TextureType: "gradient",
            BadgeText: "OC LAB",
            CardRadius: 10,
            ButtonRadius: 8,
            SidebarRadius: 12,
            NavRadius: 8,
            ChipRadius: 10,
            SwatchPrimary: "#00E5FF",
            SwatchBg: "#111116",
            Dark: new(
                Bg: "#101018", Panel: "#181824", Card: "#1F1F2E", Hover: "#2A2A40",
                Line: "#2F2E46", LineStrong: "#9B51E0", Fg: "#F5F5FF", Muted: "#9A9AB5",
                Faint: "#636380", Ink: "#101018", Accent: "#00E5FF", AccentInk: "#101018",
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

        // Textura de fundo estrutural da janela (muda o padrão visual real da skin)
        res["WindowBgBrush"] = BuildBackgroundBrush(skin, palette);
        res["CardBorderBrush"] = BuildCardBorderBrush(skin, palette);
        res["CardTopAccent"] = skin.ShowAfterburnerDeck
            ? new Thickness(0, 2, 0, 0)
            : new Thickness(0);
    }

    private static Brush BuildBackgroundBrush(SkinDefinition skin, SkinPalette p)
    {
        var baseColor = (Color)ColorConverter.ConvertFromString(p.Bg);
        var accentColor = (Color)ColorConverter.ConvertFromString(p.Accent);
        var secColor = (Color)ColorConverter.ConvertFromString(p.AccentSecondary);

        switch (skin.TextureType)
        {
            case "obsidian":
            {
                var b = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 1)
                };
                b.GradientStops.Add(new GradientStop(Color.FromRgb(10, 12, 14), 0.0));
                b.GradientStops.Add(new GradientStop(Color.FromRgb(12, 18, 17), 0.5));
                b.GradientStops.Add(new GradientStop(Color.FromRgb(11, 11, 16), 1.0));
                b.Freeze();
                return b;
            }
            case "gradient":
            {
                var c2 = IsLight
                    ? Color.FromRgb((byte)Math.Max(0, baseColor.R - 10), (byte)Math.Max(0, baseColor.G - 8), (byte)Math.Max(0, baseColor.B - 4))
                    : Color.FromRgb((byte)Math.Min(255, baseColor.R + 12), (byte)Math.Min(255, baseColor.G + 8), (byte)Math.Min(255, baseColor.B + 26));
                var b = new LinearGradientBrush(baseColor, c2, new Point(0, 0), new Point(1, 1));
                b.Freeze();
                return b;
            }
            case "grid":
            {
                var lineCol = Color.FromArgb(0x16, accentColor.R, accentColor.G, accentColor.B);
                var group = new DrawingGroup();
                group.Children.Add(new GeometryDrawing(new SolidColorBrush(baseColor), null, new RectangleGeometry(new Rect(0, 0, 28, 28))));
                var pen = new Pen(new SolidColorBrush(lineCol), 1);
                group.Children.Add(new GeometryDrawing(null, pen, new LineGeometry(new Point(0, 0), new Point(28, 0))));
                group.Children.Add(new GeometryDrawing(null, pen, new LineGeometry(new Point(0, 0), new Point(0, 28))));
                var db = new DrawingBrush(group)
                {
                    TileMode = TileMode.Tile,
                    Viewport = new Rect(0, 0, 28, 28),
                    ViewportUnits = BrushMappingMode.Absolute
                };
                db.Freeze();
                return db;
            }
            case "scanlines":
            {
                var scanCol = Color.FromArgb(0x14, secColor.R, secColor.G, secColor.B);
                var group = new DrawingGroup();
                group.Children.Add(new GeometryDrawing(new SolidColorBrush(baseColor), null, new RectangleGeometry(new Rect(0, 0, 8, 4))));
                group.Children.Add(new GeometryDrawing(new SolidColorBrush(scanCol), null, new RectangleGeometry(new Rect(0, 0, 8, 1))));
                var db = new DrawingBrush(group)
                {
                    TileMode = TileMode.Tile,
                    Viewport = new Rect(0, 0, 8, 4),
                    ViewportUnits = BrushMappingMode.Absolute
                };
                db.Freeze();
                return db;
            }
            case "carbon":
            {
                var c1 = Color.FromRgb((byte)(baseColor.R + 7), (byte)(baseColor.G + 9), (byte)(baseColor.B + 7));
                var c2 = Color.FromRgb((byte)(baseColor.R + 2), (byte)(baseColor.G + 3), (byte)(baseColor.B + 2));
                var group = new DrawingGroup();
                group.Children.Add(new GeometryDrawing(new SolidColorBrush(baseColor), null, new RectangleGeometry(new Rect(0, 0, 12, 12))));
                group.Children.Add(new GeometryDrawing(new SolidColorBrush(c1), null, new RectangleGeometry(new Rect(0, 0, 6, 6))));
                group.Children.Add(new GeometryDrawing(new SolidColorBrush(c2), null, new RectangleGeometry(new Rect(6, 6, 6, 6))));
                var db = new DrawingBrush(group)
                {
                    TileMode = TileMode.Tile,
                    Viewport = new Rect(0, 0, 10, 10),
                    ViewportUnits = BrushMappingMode.Absolute
                };
                db.Freeze();
                return db;
            }
            case "diagonal":
            {
                var stripe = Color.FromArgb(0x12, accentColor.R, accentColor.G, accentColor.B);
                var group = new DrawingGroup();
                group.Children.Add(new GeometryDrawing(new SolidColorBrush(baseColor), null, new RectangleGeometry(new Rect(0, 0, 16, 16))));
                var pen = new Pen(new SolidColorBrush(stripe), 1.5);
                group.Children.Add(new GeometryDrawing(null, pen, new LineGeometry(new Point(0, 16), new Point(16, 0))));
                var db = new DrawingBrush(group)
                {
                    TileMode = TileMode.Tile,
                    Viewport = new Rect(0, 0, 16, 16),
                    ViewportUnits = BrushMappingMode.Absolute
                };
                db.Freeze();
                return db;
            }
            default:
            {
                var sb = new SolidColorBrush(baseColor);
                sb.Freeze();
                return sb;
            }
        }
    }

    private static Brush BuildCardBorderBrush(SkinDefinition skin, SkinPalette p)
    {
        if (skin.ShowBentoNoirDeck)
        {
            var top = Color.FromRgb(46, 54, 64);
            var bot = Color.FromRgb(23, 26, 32);
            var b = new LinearGradientBrush(top, bot, new Point(0, 0), new Point(1, 1));
            b.Freeze();
            return b;
        }
        if (skin.ShowAfterburnerDeck)
        {
            var line = (Color)ColorConverter.ConvertFromString(p.Line);
            var accent = (Color)ColorConverter.ConvertFromString(p.Accent);
            var topGlow = Color.FromArgb(0x66, accent.R, accent.G, accent.B);
            var b = new LinearGradientBrush(topGlow, line, new Point(0, 0), new Point(0, 1));
            b.Freeze();
            return b;
        }
        var sb = new SolidColorBrush((Color)ColorConverter.ConvertFromString(p.Line));
        sb.Freeze();
        return sb;
    }

    private static void SetBrush(ResourceDictionary res, object key, string hex)
    {
        var color = (Color)ColorConverter.ConvertFromString(hex);
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        res[key] = brush;
    }
}
