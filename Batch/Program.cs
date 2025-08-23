using Batch;

Console.WriteLine("Batch processing started...");

Console.WriteLine();
Console.WriteLine("RunBatch 1");
BatchLib batchLib = new BatchLib();
batchLib.RunBatch(Enumerable.Range(1, 50).ToList(), 10, 1, 100); // do run with batch size 10 from 1 to 50 samples by seconds and milliseconds

Console.WriteLine();
Console.WriteLine("RunBatch 2");
batchLib.RunBatch(Enumerable.Range(1, 50).ToList(), 10, TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(100)); // do run with batch size 10 from 1 to 50 samples by timespan

Console.WriteLine("Batch processing completed.");
