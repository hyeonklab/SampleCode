using Batch;

Console.WriteLine("Batch processing started...");

//Console.WriteLine();
//Console.WriteLine($"RunBatch A: total {50}");
//var batchLib = new BatchLib<int>([.. Enumerable.Range(1, 50)]);
//batchLib.Run(20, d => Console.WriteLine($"    Action processing: {d}"), 100); // do run with batch size 20 from 1 to 50 samples by milliseconds

//Console.WriteLine();
//Console.WriteLine($"RunBatch B: total {30}");
//batchLib = new BatchLib<int>([.. Enumerable.Range(1, 30)]);
//batchLib.Run(10, d => Console.WriteLine($"    Action processing: {d}"), 100); // do run with batch size 10 from 1 to 30 samples by milliseconds

//=================================================
Console.WriteLine();
Console.WriteLine($"RunBatch C: total {20000}");
var batchLib = new BatchLib<int>([.. Enumerable.Range(1, 20000)])
    .AddTimeRange(TimeSpan.FromHours(12), TimeSpan.FromHours(13))
    .AddTimeRange(TimeSpan.FromHours(21), TimeSpan.FromHours(22))
    .AddTimeRange(TimeSpan.FromHours(23), TimeSpan.FromHours(23))
    .AddTimeRange(TimeSpan.FromHours(24.0), TimeSpan.FromHours(24.05))

    .Run(action: d => Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Action processing: {d}"), 
    batchSize: 100, itemSleepMs: 500, batchSleepMs: 500, hourLimit: 0.1, checkIntervalMin: 1);

Console.WriteLine("Batch processing completed.");
