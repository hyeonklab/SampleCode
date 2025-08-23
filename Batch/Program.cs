using Batch;

Console.WriteLine("Batch processing started...");

Console.WriteLine();
Console.WriteLine($"RunBatch 1: total {50}");
var batchLib = new BatchLib<int>([.. Enumerable.Range(1, 50)]);
batchLib.Run(20, d => Console.WriteLine($"    Action processing: {d}"), 100); // do run with batch size 20 from 1 to 50 samples by milliseconds

Console.WriteLine();
Console.WriteLine($"RunBatch 2: total {30}");
batchLib = new BatchLib<int>([.. Enumerable.Range(1, 30)]);
batchLib.Run(10, d => Console.WriteLine($"    Action processing: {d}"), 100); // do run with batch size 10 from 1 to 30 samples by milliseconds

Console.WriteLine("Batch processing completed.");
