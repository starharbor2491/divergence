using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Evo.Core;
using Evo.UnityLayer;
using NUnit.Framework;
using UnityEngine;

namespace Evo.Tests
{
    public class UnityLayerTests
    {
        static string ProjectPath(string relative)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", relative));
        }

        [Test]
        public void TokensUssMatchesGeneratedOutput()
        {
            string committed = File.ReadAllText(ProjectPath("Assets/UI/Theme/tokens.uss"));
            Assert.AreEqual(DesignTokens.GenerateUss(), committed, "tokens.uss is out of date. Run Evo.Editor.ProjectSetup.Apply.");
        }

        [Test]
        public void EveryUiTextEntryHasNameAndText()
        {
            foreach (string id in UiText.Ids)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(UiText.Name(id)), "No name for " + id);
                Assert.IsFalse(string.IsNullOrWhiteSpace(UiText.Body(id)), "No text for " + id);
            }
        }

        [Test]
        public void EveryTooltipIdUsedInCodeExistsInUiText()
        {
            var attach = new Regex("Tooltips\\.Attach\\([^,()]+,\\s*\"([a-z0-9-]*[a-z0-9])\"");
            var other = new Regex("\"((?:tab|speed|event|inspect|legend|chart|trait|axis)-[a-z0-9-]*[a-z0-9])\"");
            var used = new HashSet<string>();
            foreach (string file in Directory.GetFiles(ProjectPath("Assets/Scripts/UnityLayer"), "*.cs", SearchOption.AllDirectories))
            {
                if (file.EndsWith("UiText.cs")) continue;
                string text = File.ReadAllText(file);
                foreach (Match m in attach.Matches(text)) used.Add(m.Groups[1].Value);
                foreach (Match m in other.Matches(text)) used.Add(m.Groups[1].Value);
            }
            Assert.Greater(used.Count, 10, "The scan found too few tooltip ids; the pattern is probably wrong.");
            foreach (string id in used)
                if (!UiText.Has(id)) Assert.Fail("Tooltip id missing from UiText: " + id);
        }

        [Test]
        public void ForbiddenWordsDoNotAppearInInterfaceText()
        {
            var banned = new Regex("\\b(gene|genes|tile|tiles|square|squares|organism|organisms|creature|creatures)\\b", RegexOptions.IgnoreCase);
            foreach (string id in UiText.Ids)
                Assert.IsFalse(banned.IsMatch(UiText.Name(id) + " " + UiText.Body(id)), "Forbidden word in " + id);
            foreach (string body in UiText.HelpBodies)
                Assert.IsFalse(banned.IsMatch(body), "Forbidden word in help text");
        }

        [Test]
        public void SnapshotRoundTripThroughJsonKeepsTheRunIdentical()
        {
            Simulation a = Simulation.Create(Presets.Get("Mild world"), new SimParameters());
            for (int i = 0; i < 1500; i++) a.Step();
            string json = RunFiles.ToJson(a);
            string error;
            Simulation b = RunFiles.FromJson(json, out error);
            Assert.IsNull(error);
            Assert.IsNotNull(b);
            Assert.AreEqual(a.StateHash(), b.StateHash());
            for (int i = 0; i < 600; i++) { a.Step(); b.Step(); }
            Assert.AreEqual(a.StateHash(), b.StateHash(), "A loaded Run must continue exactly like the original.");
        }

        [Test]
        public void BadSaveFilesGiveAPlainMessageAndNoRun()
        {
            string error;
            Assert.IsNull(RunFiles.FromJson("not a save file", out error));
            Assert.IsFalse(string.IsNullOrEmpty(error));
            Assert.IsNull(RunFiles.FromJson("", out error));
            Assert.IsFalse(string.IsNullOrEmpty(error));

            Simulation a = Simulation.Create(Presets.Get("Mild world"), new SimParameters());
            string json = RunFiles.ToJson(a).Replace("\"FormatVersion\":1", "\"FormatVersion\":99");
            Assert.IsNull(RunFiles.FromJson(json, out error));
            Assert.AreEqual(UiText.LoadFailedVersion, error);
        }

        [Test]
        public void FileNamesAreSafeAndStamped()
        {
            Simulation a = Simulation.Create(new SetupSettings { SeedText = "a b/c" }, new SimParameters());
            Assert.AreEqual("evo-run-a-b-c-00m00s.json", RunFiles.RunFileName(a));
            Assert.AreEqual("evo-a-b-c-00m00s.png", RunFiles.ScreenshotFileName(a));
        }
    }
}
