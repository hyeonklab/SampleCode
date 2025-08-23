using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Batch
{
    /// <summary>
    /// batch library
    /// </summary>
    public class BatchLib<T>
    {
        private readonly IList<T> _list;

        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="list"></param>
        public BatchLib(IList<T> list)
        {
            _list = list;
        }

        /// <summary>
        /// run batch process
        /// </summary>
        /// <param name="batchSize"></param>
        /// <param name="action"></param>
        /// <param name="sleepMs"></param>
        public void Run(int batchSize, Action<T> action, int sleepMs = 0)
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
        }
    }
}
