using UnityEngine;

namespace Evo.UnityLayer
{
    public static class DesignTokens
    {
        public static Color Hex(string hex, float alpha = 1f)
        {
            int v = int.Parse(hex.Substring(1), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
            return new Color(((v >> 16) & 255) / 255f, ((v >> 8) & 255) / 255f, (v & 255) / 255f, alpha);
        }

        // Seamless deep water ground & neutral smoked optical glass
        public static readonly Color Bg0 = Hex("#0C1117");
        public static readonly Color Bg1 = Hex("#151B23", 0.56f);
        public static readonly Color Bg2 = Hex("#FFFFFF", 0.06f);
        public static readonly Color Bg3 = Hex("#FFFFFF", 0.12f);
        public static readonly Color Bg4 = Hex("#FFFFFF", 0.18f);
        public static readonly Color ActivePill = Hex("#2A3342", 0.95f);
        public static readonly Color GlassModal = Hex("#141920", 1.0f);
        public static readonly Color GlassInset = Hex("#080B10", 0.42f);
        public static readonly Color GlassRimTop = Hex("#FFFFFF", 0.24f);
        public static readonly Color GlassRimSide = Hex("#FFFFFF", 0.08f);
        public static readonly Color GlassRimBottom = Hex("#FFFFFF", 0.10f);
        public static readonly Color GlassSheen = Hex("#FFFFFF", 0.03f);
        public static readonly Color GlassShadow = Hex("#000000", 0.42f);
        public static readonly Color Scrim = Hex("#05070A", 0.76f);
        public static readonly Color Border = Hex("#FFFFFF", 0.09f);
        public static readonly Color BorderStrong = Hex("#FFFFFF", 0.18f);
        public static readonly Color Text1 = Hex("#EEF1F6");
        public static readonly Color Text2 = Hex("#A6B0BE");
        public static readonly Color Text3 = Hex("#758091");
        public static readonly Color TextDisabled = Hex("#4B5361");

        // Off-white primary action, crimson danger action, sage data accent
        public static readonly Color Accent = Hex("#EDEFF3");
        public static readonly Color AccentPressed = Hex("#CFD5DF");
        public static readonly Color AccentText = Hex("#7BD4B2");
        public static readonly Color AccentSubtle = Hex("#7BD4B2", 0.13f);
        public static readonly Color OnAccent = Hex("#0C0F14");
        public static readonly Color Danger = Hex("#D64D4D");
        public static readonly Color DangerHover = Hex("#E36262");
        public static readonly Color DangerPressed = Hex("#B83B3B");
        public static readonly Color Focus = Hex("#9CC2F5");

        public static readonly Color StatusGood = Hex("#4CC38A");
        public static readonly Color StatusWarning = Hex("#E5A93B");
        public static readonly Color StatusSerious = Hex("#E57F50");
        public static readonly Color StatusCritical = Hex("#E55A5A");

        // World (matches Bg0 seamlessly so there are no harsh letterbox bars)
        public static readonly Color WorldBg = Hex("#0C1117");
        public static readonly Color WorldSubstrate = Hex("#15212E");
        public static readonly Color WorldCaustic = Hex("#4B7FA3");
        public static readonly Color WorldWall = Hex("#FFFFFF");
        public static readonly Color WorldFood = Hex("#68CC9B");
        public static readonly Color WorldFoodGlow = Hex("#4CC38A");
        public static readonly Color WorldToxin = Hex("#D95B82");
        public static readonly Color WorldToxinHaze = Hex("#B84A78");
        public static readonly Color WorldPredatorCore = Hex("#E58A4B");
        public static readonly Color WorldGrazerCore = Hex("#58B88A");
        public static readonly Color WorldSelection = Hex("#FFFFFF");
        public static readonly Color WorldSense = Hex("#9EB3C8");
        public static readonly Color WorldEye = Hex("#0B0E13");
        public static readonly Color WorldPreview = Hex("#D5DEE9");

        // Temperature ramp
        public static readonly Color TempCold = Hex("#3F8AE0");
        public static readonly Color TempMid = Hex("#182330");
        public static readonly Color TempHot = Hex("#DE6459");

        // Charts
        public static readonly Color ChartSeries1 = Hex("#6EC9A8");
        public static readonly Color ChartGrid = Hex("#FFFFFF", 0.06f);
        public static readonly Color ChartAxis = Hex("#FFFFFF", 0.16f);
        public static readonly Color ChartSurface = Hex("#080B10", 0.44f);
        public static readonly Color Bin1 = Hex("#1C3B36");
        public static readonly Color Bin2 = Hex("#26574E");
        public static readonly Color Bin3 = Hex("#347668");
        public static readonly Color Bin4 = Hex("#4B9986");
        public static readonly Color Bin5 = Hex("#73BEA8");
        public static readonly Color Bin6 = Hex("#B8E5D6");
        public static readonly Color CauseEaten = Hex("#4C9EEB");
        public static readonly Color CauseTemperature = Hex("#E0665A");
        public static readonly Color CauseOldAge = Hex("#4CC38A");
        public static readonly Color CauseStarved = Hex("#DDA03A");
        public static readonly Color CausePoisoned = Hex("#D4629A");

        public const int Space1 = 4;
        public const int Space2 = 8;
        public const int Space3 = 12;
        public const int Space4 = 16;
        public const int Space5 = 24;
        public const int Space6 = 32;
        public const int Space7 = 48;

        public const int ControlHeight = 36;
        public const int Radius = 8;
        public const int RadiusCard = 12;
        public const int RadiusPanel = 16;
        public const int RadiusPill = 10;
        public const int RadiusCheckbox = 5;
        public const int BorderWidth = 1;
        public const int TopBarHeight = 50;
        public const int SidePanelMin = 412;
        public const int SidePanelMax = 496;
        public const int SetupCardWidth = 960;
        public const int InspectorWidth = 296;
        public const int DialogWidth = 480;
        public const int ExtinctionWidth = 460;
        public const int HelpWidth = 840;
        public const int HelpNavWidth = 204;
        public const int TooltipMaxWidth = 300;
        public const int TooltipDelayMs = 320;
        public const int ScrollbarWidth = 6;
        public const int SliderRowHeight = 56;
        public const int SliderThumbSize = 15;
        public const int SliderThumbHoverSize = 17;
        public const int TreeRowHeight = 32;
        public const int TreeMinHeight = 460;
        public const int EventChipSize = 20;
        public const int EventChipGap = 4;
        public const int LegendRowHeight = 28;
        public const int ReferenceWidth = 1920;
        public const int ReferenceHeight = 1080;
        public const int MinWindowWidth = 1280;
        public const int MinWindowHeight = 720;

        public const int TypeDisplaySize = 38;
        public const int TypeDisplayLine = 46;
        public const int TypeHeadingSize = 22;
        public const int TypeHeadingLine = 30;
        public const int TypeTitleSize = 16;
        public const int TypeTitleLine = 22;
        public const int TypeBodySize = 14;
        public const int TypeBodyLine = 20;
        public const int TypeLabelSize = 13;
        public const int TypeLabelLine = 18;
        public const int TypeCaptionSize = 12;
        public const int TypeCaptionLine = 17;

        public static readonly Color[] Bins = { Bin1, Bin2, Bin3, Bin4, Bin5, Bin6 };
        public static readonly Color[] Causes = { CauseEaten, CauseTemperature, CauseOldAge, CauseStarved, CausePoisoned };

        public static Color WithAlpha(Color c, float a) { return new Color(c.r, c.g, c.b, a); }
    }
}
