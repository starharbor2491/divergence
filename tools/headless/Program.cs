using System;
using System.Collections.Generic;
using System.Globalization;
using Evo.Core;

/// <summary>
/// Headless experiment runner (PLAN 17.3). Usage:
///   dotnet Headless.dll run [preset-or-seed] [minutes] [reportSeconds] [events=file.txt] [out=dir] [set=Field=value ...]
///   An event script has lines like: at 480 heatwave 0.3 90 x=80 y=45 r=30   (use "world" instead of x= y= r= for the whole World)
///   dotnet Headless.dll calibrate [minutes]
/// </summary>
public static class Program
{
    public static int Main(string[] args)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        string mode = args.Length > 0 ? args[0] : "run";
        if (mode == "calibrate") return Calibrate(args.Length > 1 ? int.Parse(args[1]) : 25);
        string name = args.Length > 1 ? args[1] : "Mild world";
        int minutes = args.Length > 2 ? int.Parse(args[2]) : 10;
        int report = args.Length > 3 ? int.Parse(args[3]) : 60;
        string events = null, outDir = null;
        for (int i = 4; i < args.Length; i++)
        {
            if (args[i].StartsWith("set="))
            {
                string[] kv = args[i].Substring(4).Split('=');
                var f = typeof(SimParameters).GetField(kv[0]);
                if (f == null) throw new ArgumentException("no SimParameters field " + kv[0]);
                f.SetValue(Overrides, Convert.ChangeType(kv[1], f.FieldType, CultureInfo.InvariantCulture));
                continue;
            }
            if (args[i].StartsWith("events=")) events = args[i].Substring(7);
            else if (args[i].StartsWith("out=")) outDir = args[i].Substring(4);
        }
        return Run(name, minutes, report, events, outDir);
    }

    static readonly SimParameters Overrides = new SimParameters();

    class ScriptedEvent { public double Time; public EventSpec Spec; }

    static List<ScriptedEvent> ParseEvents(string path)
    {
        var list = new List<ScriptedEvent>();
        if (path == null) return list;
        foreach (string raw in System.IO.File.ReadAllLines(path))
        {
            string line = raw.Trim();
            if (line.Length == 0 || line.StartsWith("#")) continue;
            string[] t = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var spec = new EventSpec { Radius = 30f, X = 80f, Y = 45f, Intensity = 0.3f, Duration = 60f };
            switch (t[2])
            {
                case "heatwave": spec.Kind = EventKind.HeatWave; break;
                case "iceage": spec.Kind = EventKind.IceAge; break;
                case "famine": spec.Kind = EventKind.Famine; break;
                case "foodbloom": spec.Kind = EventKind.FoodBloom; break;
                case "toxinspill": spec.Kind = EventKind.ToxinSpill; break;
                default: throw new FormatException("unknown event " + t[2]);
            }
            int pos = 0;
            for (int i = 3; i < t.Length; i++)
            {
                if (t[i] == "world") spec.WholeWorld = true;
                else if (t[i].StartsWith("x=")) spec.X = float.Parse(t[i].Substring(2));
                else if (t[i].StartsWith("y=")) spec.Y = float.Parse(t[i].Substring(2));
                else if (t[i].StartsWith("r=")) spec.Radius = float.Parse(t[i].Substring(2));
                else { float v = float.Parse(t[i]); if (pos == 0) spec.Intensity = v; else spec.Duration = v; pos++; }
            }
            list.Add(new ScriptedEvent { Time = double.Parse(t[1]), Spec = spec });
        }
        return list;
    }

    static SetupSettings Setup(string name)
    {
        SetupSettings s = Presets.Get(name);
        if (s == null) { s = Presets.Get("Mild world"); s.SeedText = name; }
        return s;
    }

    static int Run(string name, int minutes, int reportSeconds, string eventsPath, string outDir)
    {
        var sim = Simulation.Create(Setup(name), Overrides);
        List<ScriptedEvent> script = ParseEvents(eventsPath);
        int nextEvent = 0;
        int stepsPerReport = (int)(reportSeconds / sim.P.Dt);
        int total = (int)(minutes * 60 / sim.P.Dt);
        Console.WriteLine("t(s)   cells  food  tox  spec(l/x)  gen(mean/max)  births deaths[eat tmp old stv poi]   size speed sense ptemp tol tres diet rthr  sex");
        for (int i = 1; i <= total; i++)
        {
            while (nextEvent < script.Count && sim.Time >= script[nextEvent].Time)
            {
                bool ok = sim.PlaceEvent(script[nextEvent].Spec);
                Console.WriteLine("-- event {0} at {1:0}s placed={2}", EventInfo.Names[(int)script[nextEvent].Spec.Kind], sim.Time, ok);
                nextEvent++;
            }
            sim.Step();
            if (i % stepsPerReport == 0 || sim.Extinct)
            {
                Print(sim);
                if (sim.Extinct) { Console.WriteLine("EXTINCT"); break; }
            }
        }
        if (outDir != null)
        {
            System.IO.Directory.CreateDirectory(outDir);
            System.IO.File.WriteAllText(System.IO.Path.Combine(outDir, "traits-over-time.csv"), CsvExport.TraitsOverTime(sim));
            System.IO.File.WriteAllText(System.IO.Path.Combine(outDir, "species.csv"), CsvExport.Species(sim));
            Console.WriteLine("wrote CSV to " + outDir);
        }
        return 0;
    }

    static void Print(Simulation sim)
    {
        Sample s = sim.Stats.Samples[sim.Stats.Samples.Count - 1];
        var d = sim.Stats.TotalDeaths;
        // distance of each Cell to its Species centroid
        double sum = 0, max = 0;
        int n = sim.Cells.AliveCount;
        for (int i = 0; i < n; i++)
        {
            Cell c = sim.Cells.Alive[i];
            SpeciesRecord sp = sim.Species.Get(c.SpeciesId);
            if (sp == null) continue;
            double dd = GeneticDistance.Distance(c.G, sp.Centroid, sim.P);
            sum += dd;
            if (dd > max) max = dd;
        }
        double db = 0, dt = 0, dx = 0, da = 0;
        for (int i = 0; i < n; i++) { Cell c = sim.Cells.Alive[i]; db += c.DrainBase; dt += c.DrainTemperature; dx += c.DrainToxin; da += c.DrainAge; }
        if (n > 0) Console.Write("drain[b{0:0.00} t{1:0.00} x{2:0.00} a{3:0.00}] ", db / n, dt / n, dx / n, da / n);
        Console.WriteLine(
            "{0,5:0} {1,5} {2,5} {3,4} {4,3}/{5,-3} {6,5:0.0}/{7,-3} {8,5} d[{9},{10},{11},{12},{13}] D {14:0.000}/{15:0.000} | sz {16:0.00} sp {17:0.00} sn {18:0.0} pt {19:0.00} tl {20:0.00} tr {21:0.00} di {22:0.00} rt {23:0.00} sex {24:0.00}",
            sim.Time, s.Count, sim.FoodCount, sim.ToxCount, s.LivingSpecies, s.ExtinctSpecies, s.MeanGeneration, s.MaxGeneration,
            sim.Stats.TotalBirths, d[0], d[1], d[2], d[3], d[4], n > 0 ? sum / n : 0, max,
            s.TraitMean[0], s.TraitMean[1], s.TraitMean[2], s.TraitMean[3], s.TraitMean[4], s.TraitMean[5], s.TraitMean[6], s.TraitMean[7], s.SexualFraction);
    }

    static int Calibrate(int minutes)
    {
        string[] seeds = { "mild", "a1", "b2", "c3", "d4", "e5", "f6", "g7", "h8", "i9" };
        int survived = 0;
        foreach (string seed in seeds)
        {
            var setup = Presets.Get("Mild world");
            setup.SeedText = seed;
            var sim = Simulation.Create(setup, new SimParameters());
            int total = (int)(minutes * 60 / sim.P.Dt);
            int at5 = -1;
            int minCells = int.MaxValue, maxCells = 0;
            double firstSplit = -1;
            for (int i = 1; i <= total && !sim.Extinct; i++)
            {
                sim.Step();
                if (i == (int)(300 / sim.P.Dt)) at5 = sim.Cells.AliveCount;
                if (i > (int)(300 / sim.P.Dt))
                {
                    minCells = Math.Min(minCells, sim.Cells.AliveCount);
                    maxCells = Math.Max(maxCells, sim.Cells.AliveCount);
                }
                if (firstSplit < 0 && sim.Species.Records.Count > 1) firstSplit = sim.Time;
            }
            if (!sim.Extinct) survived++;
            double genTime = sim.Stats.TotalBirths > 0 ? sim.Time * sim.Cells.AliveCount / Math.Max(1, sim.Stats.TotalBirths) : 0;
            Console.WriteLine("{0,-6} extinct={1,-5} at5min={2,4} range after 5min={3}-{4} species={5} firstSplit={6:0}s maxGen={7}",
                seed, sim.Extinct, at5, minCells == int.MaxValue ? 0 : minCells, maxCells, sim.Species.Records.Count, firstSplit, sim.Stats.MaxGenerationEver);
        }
        Console.WriteLine("survived " + survived + "/" + seeds.Length);
        return 0;
    }
}
