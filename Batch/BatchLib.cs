using log4net;
using Newtonsoft.Json;

namespace Batch
{
    public class BatchLib<T>
    {
        private readonly List<T> _list;
        private BatchSettings _settings;
        private readonly DateTime _startTime;
        private readonly string _settingsPath;
        private readonly ILog _log;

        public BatchLib(List<T> list, string settingsPath)
        {
            _list = list ?? throw new ArgumentNullException("list");
            _settingsPath = settingsPath ?? throw new ArgumentNullException("settingsPath");
            _settings = LoadSettings();
            _startTime = DateTime.Now;

            _log = LogManager.GetLogger(typeof(BatchLib<T>));
        }

        private BatchSettings LoadSettings()
        {
            if (!File.Exists(_settingsPath))
                throw new FileNotFoundException("Settings file not found: " + _settingsPath);

            string json = File.ReadAllText(_settingsPath);
            var setting = JsonConvert.DeserializeObject<BatchSettings>(json) ?? new BatchSettings();
            return setting;
        }

        private bool IsInTimeRange()
        {
            var now = DateTime.Now.TimeOfDay;
            if (_settings.AllowedTimeRanges.Count == 0) return true;

            foreach (var range in _settings.AllowedTimeRanges)
            {
                var start = range.StartTime;
                var end = range.EndTime;

                if (start <= end)
                {
                    if (now >= start && now <= end) return true;
                }
                else
                {
                    // 자정 넘어가는 구간
                    if (now >= start || now <= end) return true;
                }
            }
            return false;
        }

        private bool IsMaxHoursExceeded()
        {
            var now = DateTime.Now;
            var elapsed = now - _startTime;
            if (elapsed.TotalHours >= _settings.MaxHours)
            {
                _log.Warn("[EXIT] MaxHours exceeded. Now=" + now.ToString("HH:mm:ss") +
                          ", Start=" + _startTime.ToString("HH:mm:ss") +
                          ", Elapsed=" + elapsed.TotalHours.ToString("F2") +
                          "h (limit=" + _settings.MaxHours + "h)");
                return true;
            }
            return false;
        }

        public void Run(Action<T> action)
        {
            if (action == null) throw new ArgumentNullException("action");

            for (int i = 0; i < _list.Count; i += _settings.BatchSize)
            {
                if (IsMaxHoursExceeded()) return;

                while (!IsInTimeRange())
                {
                    if (IsMaxHoursExceeded()) return;
                    _log.Info("Waiting for allowed time range...");
                    Thread.Sleep(TimeSpan.FromMinutes(_settings.CheckIntervalMin));
                }

                var batch = _list.Skip(i).Take(_settings.BatchSize).ToList();
                _log.Info("▶ Processing items " + (i + 1) + " ~ " + (i + batch.Count));

                foreach (var item in batch)
                {
                    action(item);

                    if (IsMaxHoursExceeded()) return;

                    if (_settings.ItemSleepMs > 0)
                        Thread.Sleep(_settings.ItemSleepMs);

                    _log.Info("   -> " + item);
                }

                if (_settings.BatchSleepMs > 0)
                    Thread.Sleep(_settings.BatchSleepMs);
            }

            _log.Info("✅ All items processed.");
        }
    }
}