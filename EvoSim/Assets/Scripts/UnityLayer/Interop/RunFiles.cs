using System.Text;
using Evo.Core;
using UnityEngine;

namespace Evo.UnityLayer
{
    /// <summary>File names and contents for saving a Run, exporting CSV and taking screenshots (PLAN 15).</summary>
    public static class RunFiles
    {
        public static string Stamp(Simulation sim)
        {
            int s = Mathf.Max(0, (int)sim.Time);
            return string.Format("{0:00}m{1:00}s", s / 60, s % 60);
        }

        public static string SafeSeed(Simulation sim)
        {
            var sb = new StringBuilder();
            foreach (char c in sim.Setup.SeedText)
                sb.Append(char.IsLetterOrDigit(c) || c == '-' || c == '_' ? c : '-');
            return sb.Length == 0 ? "run" : sb.ToString();
        }

        public static string RunFileName(Simulation sim) { return "evo-run-" + SafeSeed(sim) + "-" + Stamp(sim) + ".json"; }

        public static string ScreenshotFileName(Simulation sim) { return "evo-" + SafeSeed(sim) + "-" + Stamp(sim) + ".png"; }

        public static string ToJson(Simulation sim)
        {
            return JsonUtility.ToJson(sim.ToSnapshot());
        }

        /// <summary>Reads a saved Run. On failure returns null and a plain-language message; the caller keeps the current Run.</summary>
        public static Simulation FromJson(string json, out string error)
        {
            error = null;
            if (string.IsNullOrEmpty(json) || json.TrimStart().Length == 0 || json.TrimStart()[0] != '{')
            {
                error = UiText.LoadFailedGeneric;
                return null;
            }
            try
            {
                SimSnapshot snap = JsonUtility.FromJson<SimSnapshot>(json);
                if (snap == null || snap.Setup == null || snap.Params == null || snap.Cells == null)
                {
                    error = UiText.LoadFailedGeneric;
                    return null;
                }
                if (snap.FormatVersion != SimSnapshot.CurrentFormatVersion)
                {
                    error = UiText.LoadFailedVersion;
                    return null;
                }
                return Simulation.FromSnapshot(snap);
            }
            catch (System.Exception)
            {
                error = UiText.LoadFailedGeneric;
                return null;
            }
        }

        public static void DownloadText(string name, string mime, string text)
        {
            FileBridge.Download(name, mime, new UTF8Encoding(false).GetBytes(text));
        }

        public static void ExportCsv(Simulation sim)
        {
            DownloadText("traits-over-time.csv", "text/csv", CsvExport.TraitsOverTime(sim));
            DownloadText("species.csv", "text/csv", CsvExport.Species(sim));
        }
    }
}
