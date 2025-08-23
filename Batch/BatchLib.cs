using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Batch
{
    public class BatchLib
    {
        /// <summary>
        /// do run batch
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="batchSize"></param>
        /// <param name="fromSeconds"></param>
        /// <param name="fromMilliseconds"></param>
        public void RunBatch<T>(IEnumerable<T> list, int batchSize, int fromSeconds, int fromMilliseconds)
        {
            RunBatch(list, batchSize, TimeSpan.FromSeconds(fromSeconds), TimeSpan.FromMilliseconds(fromMilliseconds));
        }

        /// <summary>
        /// do run batch
        /// </summary>
        /// <param name="batchSize"></param>
        public void RunBatch<T>(IEnumerable<T> list, int batchSize, TimeSpan timeSpan, TimeSpan timePerItem)
        {
            Console.WriteLine($"batch size: {batchSize}");

            var items = list.ToList();

            for (int i = 0; i < items.Count; i += batchSize)
            {
                var batch = items.Skip(i).Take(batchSize).ToList();

                Console.WriteLine($"Batch start: {i / batchSize + 1}");

                foreach (var item in batch)
                {
                    Console.WriteLine($"  processing: {item}");
                    Thread.Sleep(timePerItem); // Simulate some processing time per item
                }

                Thread.Sleep(timeSpan); // Simulate some processing time
            }
        }
    }
}
