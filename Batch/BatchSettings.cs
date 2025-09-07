using log4net;
using Newtonsoft.Json;

namespace Batch
{
    public class BatchSettings
    {
        public int BatchSize { get; set; } = 10;
        public int ItemSleepMs { get; set; } = 100;
        public int BatchSleepMs { get; set; } = 100;
        public double MaxHours { get; set; } = 24;
        public int CheckIntervalMin { get; set; } = 5;
        public List<TimeRange> AllowedTimeRanges { get; set; } = new List<TimeRange>();
    }

    public class TimeRange
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(TimeRange));

        public string Start { get; set; } = "00:00:00";
        public string End { get; set; } = "23:59:59";

        [JsonIgnore]
        public TimeSpan StartTime
        {
            get
            {
                if (TimeSpan.TryParse(Start, out TimeSpan ts))
                    return ts;

                _log.Warn("Invalid Start value '" + Start + "', using default 00:00:00");
                return TimeSpan.Zero;
            }
        }

        [JsonIgnore]
        public TimeSpan EndTime
        {
            get
            {
                TimeSpan ts;
                if (TimeSpan.TryParse(End, out ts))
                    return ts;

                _log.Warn("Invalid End value '" + End + "', using default 23:59:59");
                return new TimeSpan(23, 59, 59);
            }
        }
    }
}
