using System.Globalization;
using System.Text;

namespace Evo.Core
{
    /// <summary>CSV text for the two exported files (PLAN 15.2): comma separated, '.' decimal point, header row, '\n' line endings.</summary>
    public static class CsvExport
    {
        public static readonly string[] TraitColumnNames =
        {
            "size", "speed", "sense_range", "preferred_temperature", "temperature_tolerance",
            "toxin_resistance", "diet", "reproduction_threshold"
        };

        static string F(float v) { return v.ToString("0.####", CultureInfo.InvariantCulture); }
        static string D(double v) { return v.ToString("0.###", CultureInfo.InvariantCulture); }

        public static string TraitsHeader()
        {
            var sb = new StringBuilder();
            sb.Append("time_s,mean_generation,max_generation,population,species_count,births_total,");
            sb.Append("deaths_starved,deaths_eaten,deaths_poisoned,deaths_temperature,deaths_old_age");
            for (int t = 0; t < Traits.ChartCount; t++)
                sb.Append(',').Append(TraitColumnNames[t]).Append("_mean,").Append(TraitColumnNames[t]).Append("_sd");
            return sb.ToString();
        }

        public static string TraitsOverTime(Simulation sim)
        {
            var sb = new StringBuilder();
            sb.Append(TraitsHeader()).Append('\n');
            foreach (Sample s in sim.Stats.Samples)
            {
                sb.Append(F(s.Time)).Append(',').Append(F(s.MeanGeneration)).Append(',').Append(s.MaxGeneration).Append(',')
                  .Append(s.Count).Append(',').Append(s.LivingSpecies).Append(',').Append(s.Births).Append(',')
                  .Append(s.Deaths[(int)DeathCause.Starved]).Append(',').Append(s.Deaths[(int)DeathCause.Eaten]).Append(',')
                  .Append(s.Deaths[(int)DeathCause.Poisoned]).Append(',').Append(s.Deaths[(int)DeathCause.Temperature]).Append(',')
                  .Append(s.Deaths[(int)DeathCause.OldAge]);
                for (int t = 0; t < Traits.ChartCount; t++)
                    sb.Append(',').Append(F(s.TraitMean[t])).Append(',').Append(F(s.TraitSd[t]));
                sb.Append('\n');
            }
            return sb.ToString();
        }

        public static string SpeciesHeader()
        {
            return "id,parent_id,name,birth_time_s,birth_generation,extinct_time_s,peak_population,current_population";
        }

        public static string Species(Simulation sim)
        {
            var sb = new StringBuilder();
            sb.Append(SpeciesHeader()).Append('\n');
            foreach (SpeciesRecord r in sim.Species.Records)
            {
                sb.Append(r.Id).Append(',').Append(r.ParentId).Append(',').Append(r.Name).Append(',').Append(D(r.BirthTime)).Append(',')
                  .Append(r.BirthGeneration).Append(',').Append(r.Extinct ? D(r.ExtinctTime) : "").Append(',')
                  .Append(r.PeakPopulation).Append(',').Append(r.CurrentPopulation).Append('\n');
            }
            return sb.ToString();
        }
    }
}
