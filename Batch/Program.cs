using Batch;
using log4net.Config;
internal class Program
{
    private static void Main(string[] args)
    {
        // 1) log4net 초기화 (App.config 사용)
        XmlConfigurator.Configure();

        // 2) 로그 월 폴더 생성 (logs/YYYYMM)
        var monthDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", DateTime.Now.ToString("yyyyMM"));
        if (!Directory.Exists(monthDir))
            Directory.CreateDirectory(monthDir);

        Console.WriteLine("Batch processing started...");
        Console.WriteLine();
        Console.WriteLine($"RunBatch total {20000}");

        string settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "batchSettings.json");

        //var list = new List<string> { "A", "B", "C", "D", "E", "F", "G" };
        List<int> list = new(Enumerable.Range(1, 20000));
        var batch = new BatchLib<int>(list, settingsPath);

        batch.Run(item =>
        {
            Console.WriteLine("Processing: " + item);
        });

        Console.WriteLine("Batch processing completed.");
    }
}