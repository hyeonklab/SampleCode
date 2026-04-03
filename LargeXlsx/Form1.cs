using System.Diagnostics;

namespace LargeXlsx
{
    public partial class Form1 : Form
    {
        private static readonly string[] ProductNames = ["노트북", "마우스", "키보드", "모니터", "헤드셋", "웹캠", "USB허브", "SSD", "RAM", "그래픽카드"];
        private static readonly string[] Categories = ["전자기기", "주변기기", "저장장치", "디스플레이", "네트워크"];
        private static readonly string[] Statuses = ["완료", "처리중", "취소", "반품", "배송중"];

        private record OrderRow(
            int OrderId, int CustomerId, string CustomerName, string Email, string Phone,
            string ProductName, string Category, int Quantity, decimal UnitPrice,
            decimal TotalPrice, DateTime OrderDate, string Status);

        private static IEnumerable<OrderRow> GenerateRows(int startId, int count, int processedBefore, int grandTotal, IProgress<int> progress)
        {
            var rng = new Random(startId);
            var baseDate = new DateTime(2023, 1, 1);

            for (int i = 1; i <= count; i++)
            {
                int qty = rng.Next(1, 100);
                decimal unitPrice = Math.Round((decimal)(rng.NextDouble() * 990000 + 10000), 0);
                yield return new OrderRow(
                    processedBefore + i,
                    rng.Next(1000, 10000),
                    $"고객{rng.Next(1, 100000):D5}",
                    $"user{rng.Next(10000, 99999)}@email.com",
                    $"010-{rng.Next(1000, 9999)}-{rng.Next(1000, 9999)}",
                    ProductNames[rng.Next(ProductNames.Length)],
                    Categories[rng.Next(Categories.Length)],
                    qty,
                    unitPrice,
                    unitPrice * qty,
                    baseDate.AddDays(rng.Next(0, 730)),
                    Statuses[rng.Next(Statuses.Length)]);

                if (i % 10000 == 0)
                    progress.Report(processedBefore + i);
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void BtnBrowse_Click(object? sender, EventArgs e)
        {
            using var dlg = new FolderBrowserDialog { Description = "저장 폴더 선택" };
            if (dlg.ShowDialog() == DialogResult.OK)
                txtPath.Text = dlg.SelectedPath;
        }

        private async void BtnExport_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtPath.Text))
            {
                MessageBox.Show("저장 경로를 선택하세요.");
                return;
            }

            btnExport.Enabled = false;
            progressBar.Value = 0;
            lblStatus.Text = "추출 시작...";

            const int grandTotal = 10_000_000;
            const int chunkSize = 1_000_000;
            const int fileCount = grandTotal / chunkSize;
            string folder = txtPath.Text;
            int currentFile = 0;

            string[] headers = ["OrderId", "CustomerId", "CustomerName", "Email", "Phone",
                                 "ProductName", "Category", "Quantity", "UnitPrice", "TotalPrice",
                                 "OrderDate", "Status"];

            var progress = new Progress<int>(processed =>
            {
                int pct = (int)((double)processed / grandTotal * 100);
                progressBar.Value = pct;
                lblStatus.Text = $"파일 {currentFile}/{fileCount} | {processed:N0} / {grandTotal:N0} 처리중... ({pct}%)";
            });

            var sw = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                await Task.Run(() =>
                {
                    for (int f = 0; f < fileCount; f++)
                    {
                        currentFile = f + 1;
                        int startId = f * chunkSize + 1;
                        string filePath = Path.Combine(folder, $"export_{f + 1:D2}.xlsx");

                        using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                        using var writer = new XlsxWriter(stream);

                        writer.BeginWorksheet("Orders");

                        writer.BeginRow();
                        foreach (var h in headers)
                            writer.Write(h);

                        foreach (var row in GenerateRows(startId, chunkSize, f * chunkSize, grandTotal, progress))
                        {
                            writer.BeginRow();
                            writer.Write(row.OrderId);
                            writer.Write(row.CustomerId);
                            writer.Write(row.CustomerName);
                            writer.Write(row.Email);
                            writer.Write(row.Phone);
                            writer.Write(row.ProductName);
                            writer.Write(row.Category);
                            writer.Write(row.Quantity);
                            writer.Write(row.UnitPrice);
                            writer.Write(row.TotalPrice);
                            writer.Write(row.OrderDate);
                            writer.Write(row.Status);
                        }
                    }
                });

                sw.Stop();
                string elapsed = $"{sw.Elapsed:mm\\:ss\\.fff}";
                progressBar.Value = 100;
                lblStatus.Text = $"완료! {grandTotal:N0}건 → {fileCount}개 파일 | 소요시간: {elapsed}";
                MessageBox.Show($"추출 완료!\n{fileCount}개 파일 → {folder}\n소요시간: {elapsed}", "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Process.Start("explorer.exe", folder);
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"오류: {ex.Message}";
                MessageBox.Show($"추출 중 오류가 발생했습니다.\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnExport.Enabled = true;
            }
        }
    }
}
