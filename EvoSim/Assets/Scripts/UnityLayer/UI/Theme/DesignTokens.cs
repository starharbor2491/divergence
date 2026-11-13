using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Evo.UnityLayer
{
    /// <summary>
    /// Every colour, size and type value of DESIGN.md section 2 as a constant. Components never write a literal.
    /// The USS custom property for a field is `--evo-` plus the kebab-case field name (Bg1 becomes --evo-bg-1).
    /// </summary>
    public static class DesignTokens
    {
        public static Color Hex(string hex, float alpha = 1f)
        {
            int v = int.Parse(hex.Substring(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return new Color(((v >> 16) & 255) / 255f, ((v >> 8) & 255) / 255f, (v & 255) / 255f, alpha);
        }

        // ------------------------------------------------------------ deep aquatic ground & Liquid Glass surfaces
        public static readonly Color Bg0 = Hex("#061019");
        public static readonly Color Bg1 = Hex("#0C1926", 0.58f);
        public static readonly Color Bg2 = Hex("#182D42", 0.56f);
        public static readonly Color Bg3 = Hex("#2A4A68", 0.68f);
        public static readonly Color Bg4 = Hex("#3A6286", 0.78f);
        public static readonly Color GlassModal = Hex("#0A1622", 0.84f);
        public static readonly Color GlassInset = Hex("#06111B", 0.42f);
        public static readonly Color GlassRimTop = Hex("#FFFFFF", 0.42f);
        public static readonly Color GlassRimSide = Hex("#BAE6FD", 0.14f);
        public static readonly Color GlassRimBottom = Hex("#5EEAD4", 0.22f);
        public static readonly Color GlassSheen = Hex("#FFFFFF", 0.06f);
        public static readonly Color GlassShadow = Hex("#02080F", 0.42f);
        public static readonly Color Scrim = Hex("#040A10", 0.62f);
        public static readonly Color Border = Hex("#A5D2F0", 0.15f);
        public static readonly Color BorderStrong = Hex("#CDEBFF", 0.28f);
        public static readonly Color Text1 = Hex("#F2F8FD");
        public static readonly Color Text2 = Hex("#BDD0E2");
        public static readonly Color Text3 = Hex("#8CA7C0");
        public static readonly Color TextDisabled = Hex("#566E84");

        // ------------------------------------------------------------ bioluminescent accent and focus
        public static readonly Color Accent = Hex("#36DFC4");
        public static readonly Color AccentPressed = Hex("#24BFA6");
        public static readonly Color AccentText = Hex("#7CF5E0");
        public static readonly Color AccentSubtle = Hex("#36DFC4", 0.16f);
        public static readonly Color OnAccent = Hex("#04201C");
        public static readonly Color Focus = Hex("#7CF5E0");

        // ------------------------------------------------------------ status
        public static readonly Color StatusGood = Hex("#34D399");
        public static readonly Color StatusWarning = Hex("#FBBF24");
        public static readonly Color StatusSerious = Hex("#FB923C");
        public static readonly Color StatusCritical = Hex("#F87171");

        // ------------------------------------------------------------ living world
        public static readonly Color WorldBg = Hex("#081622");
        public static readonly Color WorldSubstrate = Hex("#122D44");
        public static readonly Color WorldCaustic = Hex("#38BDF8");
        public static readonly Color WorldWall = Hex("#548AB2");
        public static readonly Color WorldFood = Hex("#5CE6A0");
        public static readonly Color WorldFoodGlow = Hex("#34D399");
        public static readonly Color WorldToxin = Hex("#F05A8E");
        public static readonly Color WorldToxinHaze = Hex("#D946EF");
        public static readonly Color WorldPredatorCore = Hex("#FB923C");
        public static readonly Color WorldGrazerCore = Hex("#34D399");
        public static readonly Color WorldSelection = Hex("#FFFFFF");
        public static readonly Color WorldSense = Hex("#7CF5E0");
        public static readonly Color WorldEye = Hex("#09131C");
        public static readonly Color WorldPreview = Hex("#7CF5E0");

        // ------------------------------------------------------------ temperature tint stops
        public static readonly Color TempCold = Hex("#3898F5");
        public static readonly Color TempMid = Hex("#173247");
        public static readonly Color TempHot = Hex("#F26457");

        // ------------------------------------------------------------ charts
        public static readonly Color ChartSeries1 = Hex("#36DFC4");
        public static readonly Color ChartGrid = Hex("#A5D2F0", 0.10f);
        public static readonly Color ChartAxis = Hex("#A5D2F0", 0.24f);
        public static readonly Color ChartSurface = Hex("#07131E", 0.46f);
        public static readonly Color Bin1 = Hex("#114B5F");
        public static readonly Color Bin2 = Hex("#1A6B7C");
        public static readonly Color Bin3 = Hex("#228B94");
        public static readonly Color Bin4 = Hex("#2EB0AC");
        public static readonly Color Bin5 = Hex("#5FD4C4");
        public static readonly Color Bin6 = Hex("#B5F5EC");
        public static readonly Color CauseEaten = Hex("#38BDF8");
        public static readonly Color CauseTemperature = Hex("#FB7185");
        public static readonly Color CauseOldAge = Hex("#34D399");
        public static readonly Color CauseStarved = Hex("#FBBF24");
        public static readonly Color CausePoisoned = Hex("#E879F9");

        // ------------------------------------------------------------ spacing (px at 1920 x 1080)
        public const int Space1 = 4;
        public const int Space2 = 8;
        public const int Space3 = 12;
        public const int Space4 = 16;
        public const int Space5 = 24;
        public const int Space6 = 32;
        public const int Space7 = 48;

        // ------------------------------------------------------------ shape and layout
        public const int ControlHeight = 38;
        public const int Radius = 12;
        public const int RadiusCard = 16;
        public const int RadiusPanel = 22;
        public const int RadiusPill = 20;
        public const int RadiusCheckbox = 6;
        public const int BorderWidth = 1;
        public const int TopBarHeight = 56;
        public const int SidePanelMin = 420;
        public const int SidePanelMax = 520;
        public const int SetupCardWidth = 1040;
        public const int InspectorWidth = 312;
        public const int DialogWidth = 520;
        public const int ExtinctionWidth = 500;
        public const int HelpWidth = 1080;
        public const int HelpNavWidth = 240;
        public const int TooltipMaxWidth = 340;
        public const int TooltipDelayMs = 320;
        public const int ScrollbarWidth = 6;
        public const int SliderRowHeight = 60;
        public const int SliderThumbSize = 18;
        public const int SliderThumbHoverSize = 20;
        public const int TreeRowHeight = 32;
        public const int TreeMinHeight = 460;
        public const int EventChipSize = 22;
        public const int EventChipGap = 4;
        public const int LegendRowHeight = 30;
        public const int ReferenceWidth = 1920;
        public const int ReferenceHeight = 1080;
        public const int MinWindowWidth = 1280;
        public const int MinWindowHeight = 720;

        // ------------------------------------------------------------ type (size; line height is the matching Line field)
        public const int TypeDisplaySize = 44;
        public const int TypeDisplayLine = 52;
        public const int TypeHeadingSize = 26;
        public const int TypeHeadingLine = 34;
        public const int TypeTitleSize = 18;
        public const int TypeTitleLine = 25;
        public const int TypeBodySize = 15;
        public const int TypeBodyLine = 22;
        public const int TypeLabelSize = 14;
        public const int TypeLabelLine = 20;
        public const int TypeCaptionSize = 12;
        public const int TypeCaptionLine = 17;

        // ------------------------------------------------------------ helpers
        public static readonly Color[] Bins = { Bin1, Bin2, Bin3, Bin4, Bin5, Bin6 };
        /// <summary>Death cause colours indexed by Evo.Core.DeathCause (Eaten, Temperature, OldAge, Starved, Poisoned).</summary>
        public static readonly Color[] Causes = { CauseEaten, CauseTemperature, CauseOldAge, CauseStarved, CausePoisoned };

        public static Color WithAlpha(Color c, float a) { return new Color(c.r, c.g, c.b, a); }

        public static string ToKebab(string pascal)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < pascal.Length; i++)
            {
                char ch = pascal[i];
                bool upper = char.IsUpper(ch);
                bool digit = char.IsDigit(ch);
                if (i > 0 && (upper || (digit && !char.IsDigit(pascal[i - 1])))) sb.Append('-');
                sb.Append(char.ToLowerInvariant(ch));
            }
            return sb.ToString();
        }

        static string CssColor(Color c)
        {
            int r = Mathf.RoundToInt(c.r * 255f), g = Mathf.RoundToInt(c.g * 255f), b = Mathf.RoundToInt(c.b * 255f);
            if (c.a >= 0.999f) return string.Format(CultureInfo.InvariantCulture, "#{0:X2}{1:X2}{2:X2}", r, g, b);
            return string.Format(CultureInfo.InvariantCulture, "rgba({0}, {1}, {2}, {3:0.##})", r, g, b, c.a);
        }

        /// <summary>The generated tokens.uss text. An EditMode test compares it with the committed file.</summary>
        public static string GenerateUss()
        {
            var sb = new StringBuilder();
            sb.Append("/* Generated from DesignTokens.cs by Evo.Editor.GenerateTokensUss. Do not edit by hand. */\n");
            sb.Append(":root {\n");
            FieldInfo[] fields = typeof(DesignTokens).GetFields(BindingFlags.Public | BindingFlags.Static);
            Array.Sort(fields, (a, b) => a.MetadataToken.CompareTo(b.MetadataToken));
            foreach (FieldInfo f in fields)
            {
                string name = "--evo-" + ToKebab(f.Name);
                if (f.FieldType == typeof(Color)) sb.Append("    ").Append(name).Append(": ").Append(CssColor((Color)f.GetValue(null))).Append(";\n");
                else if (f.FieldType == typeof(int) && f.IsLiteral) sb.Append("    ").Append(name).Append(": ").Append(((int)f.GetRawConstantValue()).ToString(CultureInfo.InvariantCulture)).Append("px;\n");
            }
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>Names of every token, used by the tests.</summary>
        public static List<string> TokenNames()
        {
            var list = new List<string>();
            foreach (FieldInfo f in typeof(DesignTokens).GetFields(BindingFlags.Public | BindingFlags.Static))
                if (f.FieldType == typeof(Color) || (f.FieldType == typeof(int) && f.IsLiteral)) list.Add(ToKebab(f.Name));
            return list;
        }
    }
}
