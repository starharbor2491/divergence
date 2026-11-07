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

        // ------------------------------------------------------------ neutrals
        public static readonly Color Bg0 = Hex("#0E1116");
        public static readonly Color Bg1 = Hex("#171B22");
        public static readonly Color Bg2 = Hex("#1F2530");
        public static readonly Color Bg3 = Hex("#2A313D");
        public static readonly Color Bg4 = Hex("#343C4A");
        public static readonly Color Scrim = Hex("#0E1116", 0.7f);
        public static readonly Color Border = Hex("#2C3440");
        public static readonly Color BorderStrong = Hex("#6B7789");
        public static readonly Color Text1 = Hex("#E6EAF0");
        public static readonly Color Text2 = Hex("#A9B2C0");
        public static readonly Color Text3 = Hex("#949FB0");
        public static readonly Color TextDisabled = Hex("#5F6A7A");

        // ------------------------------------------------------------ accent and focus
        public static readonly Color Accent = Hex("#5E9CF0");
        public static readonly Color AccentPressed = Hex("#4A86DA");
        public static readonly Color AccentText = Hex("#8DB9F5");
        public static readonly Color OnAccent = Hex("#0E1116");
        public static readonly Color Focus = Hex("#8DB9F5");

        // ------------------------------------------------------------ status
        public static readonly Color StatusGood = Hex("#0CA30C");
        public static readonly Color StatusWarning = Hex("#FAB219");
        public static readonly Color StatusSerious = Hex("#EC835A");
        public static readonly Color StatusCritical = Hex("#D03B3B");

        // ------------------------------------------------------------ world
        public static readonly Color WorldBg = Hex("#0E1116");
        public static readonly Color WorldWall = Hex("#3A4352");
        public static readonly Color WorldFood = Hex("#6FD6A8");
        public static readonly Color WorldToxin = Hex("#D55181");
        public static readonly Color WorldSelection = Hex("#FFFFFF");
        public static readonly Color WorldEye = Hex("#14181F");
        public static readonly Color WorldPreview = Hex("#5E9CF0");

        // ------------------------------------------------------------ temperature tint stops
        public static readonly Color TempCold = Hex("#3987E5");
        public static readonly Color TempMid = Hex("#4A5565");
        public static readonly Color TempHot = Hex("#E66767");

        // ------------------------------------------------------------ charts
        public static readonly Color ChartSeries1 = Hex("#3987E5");
        public static readonly Color ChartGrid = Hex("#232A34");
        public static readonly Color ChartAxis = Hex("#3A4352");
        public static readonly Color ChartSurface = Hex("#171B22");
        public static readonly Color Bin1 = Hex("#184F95");
        public static readonly Color Bin2 = Hex("#256ABF");
        public static readonly Color Bin3 = Hex("#3987E5");
        public static readonly Color Bin4 = Hex("#6DA7EC");
        public static readonly Color Bin5 = Hex("#9EC5F4");
        public static readonly Color Bin6 = Hex("#CDE2FB");
        public static readonly Color CauseEaten = Hex("#3987E5");
        public static readonly Color CauseTemperature = Hex("#D95926");
        public static readonly Color CauseOldAge = Hex("#199E70");
        public static readonly Color CauseStarved = Hex("#C98500");
        public static readonly Color CausePoisoned = Hex("#D55181");

        // ------------------------------------------------------------ spacing (px at 1920 x 1080)
        public const int Space1 = 4;
        public const int Space2 = 8;
        public const int Space3 = 12;
        public const int Space4 = 16;
        public const int Space5 = 24;
        public const int Space6 = 32;
        public const int Space7 = 48;

        // ------------------------------------------------------------ shape and layout
        public const int ControlHeight = 40;
        public const int Radius = 4;
        public const int RadiusCheckbox = 2;
        public const int BorderWidth = 1;
        public const int TopBarHeight = 56;
        public const int SidePanelMin = 420;
        public const int SidePanelMax = 560;
        public const int SetupCardWidth = 720;
        public const int InspectorWidth = 280;
        public const int DialogWidth = 520;
        public const int ExtinctionWidth = 480;
        public const int HelpWidth = 1100;
        public const int HelpNavWidth = 240;
        public const int TooltipMaxWidth = 320;
        public const int TooltipDelayMs = 400;
        public const int ScrollbarWidth = 8;
        public const int SliderRowHeight = 64;
        public const int LegendRowHeight = 28;
        public const int ReferenceWidth = 1920;
        public const int ReferenceHeight = 1080;
        public const int MinWindowWidth = 1280;
        public const int MinWindowHeight = 720;

        // ------------------------------------------------------------ type (size; line height is the matching Line field)
        public const int TypeDisplaySize = 48;
        public const int TypeDisplayLine = 56;
        public const int TypeHeadingSize = 28;
        public const int TypeHeadingLine = 36;
        public const int TypeTitleSize = 20;
        public const int TypeTitleLine = 28;
        public const int TypeBodySize = 15;
        public const int TypeBodyLine = 22;
        public const int TypeLabelSize = 15;
        public const int TypeLabelLine = 22;
        public const int TypeCaptionSize = 13;
        public const int TypeCaptionLine = 18;

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
