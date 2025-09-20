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

        // simple batch processing example
        int batchSize = 1000;
        string joinList;
        for (int i = 0, j = 1; i < list.Count; i += batchSize, j++)
        {
            var batchItems = list.Skip(i).Take(batchSize).ToList();
            joinList = string.Join(",", batchItems);
            Console.WriteLine($"{Environment.NewLine}batch {j} {joinList}");
        }

        // advanced batch processing with action
        batch.Run(item =>
        {
            Console.WriteLine("Processing: " + item);
        });

        Console.WriteLine("Batch processing completed.");
    }
}