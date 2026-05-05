using System.Globalization;
using System.Text;
using Bogus;
using CsvHelper;
using CsvHelper.Configuration;
using Renci.SshNet;

const int count = 50000;
const string filePath = "sample.csv";

// --- 1. CSV 데이터 생성 ---
Console.WriteLine($"Generating {count} sample...");

var couponFaker = new Faker<Sample>()
    .RuleFor(c => c.ProductCode, f => f.Random.Replace("####-####-####").ToUpper())
    .RuleFor(c => c.Date, f => f.Date.Future(1))
    .RuleFor(c => c.Quantity, f => f.Random.Decimal(1000, 10000))
    .RuleFor(c => c.Price, f => f.Random.Decimal(10000, 50000))
    .RuleFor(c => c.Amount, (f, c) => Math.Round((c.Quantity ?? 0) * ((c.Price ?? 0) / 100), 2));

var coupons = couponFaker.Generate(count);

// --- 2. CSV 파일 저장 ---
CsvConfiguration config = new(CultureInfo.InvariantCulture)
{
    HasHeaderRecord = true,
    Delimiter = ",",
    Encoding = Encoding.UTF8
};

Console.WriteLine("Writing to CSV file...");
using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
using (var csv = new CsvWriter(writer, config))
{
    csv.WriteRecords(coupons);
}
Console.WriteLine($"Successfully created {filePath}");

// --- 3. SFTP 접속 및 업로드 ---
Console.WriteLine("Connecting to SFTP server...");
using SftpClient client = new("localhost", "sftpuser", "Sftp123!");
try
{
    client.Connect();
    Console.WriteLine("Connected to SFTP.");

    // 생성된 파일을 업로드
    using (var fileStream = File.OpenRead(filePath))
    {
        Console.WriteLine($"Uploading {filePath} to server...");
        client.UploadFile(fileStream, $"{filePath}");
    }
    Console.WriteLine("Upload completed.");

    client.ListDirectory(client.WorkingDirectory);

    client.Disconnect();
}
catch (Exception ex)
{
    Console.WriteLine($"Error during SFTP operation: {ex.Message}");
}

// --- 모델 클래스 ---
public class Sample
{
    public string? ProductCode { get; set; }
    public DateTime? Date { get; set; }

    public decimal? Quantity { get; set; }
    public decimal? Price { get; set; }
    public decimal? Amount { get; set; }
}