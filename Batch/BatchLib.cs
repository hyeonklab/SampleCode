using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Batch
{
    /// <summary>
    /// batch library
    /// </summary>
    public class BatchLib<T>
    {
        private readonly IList<T> _list;
        private readonly List<(TimeSpan Start, TimeSpan End)> _timeRanges;

        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="list"></param>
        public BatchLib(IList<T> list)
        {
            _list = list;
            _timeRanges = new List<(TimeSpan, TimeSpan)>();
        }

        /// <summary>
        /// add allowed time range
        /// </summary>
        public BatchLib<T> AddTimeRange(TimeSpan start, TimeSpan end)
        {
            _timeRanges.Add((start, end));
            return this;
        }

        /// <summary>
        /// Check if current time is within allowed time ranges
        /// </summary>
        private bool IsInTimeRange() => _timeRanges.Count == 0 || _timeRanges.Any(r => DateTime.Now.TimeOfDay >= r.Start && DateTime.Now.TimeOfDay <= r.End);

        /// <summary>
        /// Check if the elapsed time since startTime exceeds the limit
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        private bool IsOverTime(DateTime startTime, TimeSpan limit)
        {
            return DateTime.Now - startTime > limit;
        }

        /// <summary>
        /// run batch process
        /// </summary>
        /// <param name="batchSize"></param>
        /// <param name="action"></param>
        /// <param name="sleepMs"></param>
        public BatchLib<T> Run(int batchSize, Action<T> action, int sleepMs = 0)
        {
            for (int i = 0; i < _list.Count; i += batchSize)
            {
                var batch = _list.Skip(i).Take(batchSize).ToList();
                Console.WriteLine($"Batch start: {i / batchSize + 1}: [{string.Join(", ", batch)}]");

                foreach (var item in batch)
                {
                    action(item);
                    Console.WriteLine($"    Log batch {i / batchSize + 1}: processing {item}");

                    if (sleepMs > 0)
                        Thread.Sleep(sleepMs); // Sleep per item
                }

                Thread.Sleep(1000); // Sleep per batch (option)
            }

            return this;
        }

        /// <summary>
        /// Execute batch processing
        /// </summary>
        /// <param name="batchSize">Number of items per batch</param>
        /// <param name="action">Action to process each item</param>
        /// <param name="itemSleepMs">Sleep time between items (ms)</param>
        /// <param name="batchSleepMs">Sleep time between batches (ms)</param>
        /// <param name="hourLimit">Maximum running hours from start time</param>
        /// <param name="checkIntervalMin">Interval to check allowed time (minutes)</param>
        public BatchLib<T> Run(
            Action<T> action,
            int batchSize,
            int itemSleepMs,
            int batchSleepMs = 0,
            double hourLimit = 24,
            int checkIntervalMin = 5)
        {
            DateTime startTime = DateTime.Now;
            TimeSpan limit = TimeSpan.FromHours(hourLimit);

            for (int i = 0; i < _list.Count; i += batchSize)
            {
                // Check time limit before starting batch
                if (IsOverTime(startTime, limit))
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Time limit exceeded. Remaining batches skipped.");
                    break;
                }

                // Check if within allowed time ranges
                DateTime batchCheckStart = DateTime.Now;
                while (!IsInTimeRange())
                {
                    if (IsOverTime(startTime, limit))
                    {
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Time limit exceeded. Remaining batches skipped.");
                        return this;
                    }

                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Not in allowed time → waiting {checkIntervalMin} minutes...");
                    Thread.Sleep(checkIntervalMin * 60 * 1000); // wait for checkIntervalMin minutes
                }

                var batch = _list.Skip(i).Take(batchSize).ToList();
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Batch start: {i / batchSize + 1}  {i + 1} ~ {i + batch.Count}");

                foreach (var item in batch)
                {
                    // Check time limit during processing
                    if (IsOverTime(startTime, limit))
                    {
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Time limit exceeded. Remaining items skipped.");
                        break;
                    }

                    // waiting for allowed time
                    while (!IsInTimeRange())
                    {
                        if (IsOverTime(startTime, limit))
                        {
                            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Time limit exceeded. Remaining batches skipped.");
                            return this;
                        }

                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Paused - waiting for allowed time...");
                        Thread.Sleep(checkIntervalMin * 60 * 1000);
                    }

                    action(item);

                    if (itemSleepMs > 0)
                        Thread.Sleep(itemSleepMs);
                }

                if (batchSleepMs > 0)
                    Thread.Sleep(batchSleepMs);
            }

            return this;
        }
    }
}
