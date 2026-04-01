using ClosedXML.Excel;

namespace ClosedXML
{
    public partial class Form1 : Form
    {
        private static readonly string[] ProductNames = ["노트북", "마우스", "키보드", "모니터", "헤드셋", "웹캠", "USB허브", "SSD", "RAM", "그래픽카드"];
        private static readonly string[] Categories = ["전자기기", "주변기기", "저장장치", "디스플레이", "네트워크"];
        private static readonly string[] Statuses = ["완료", "처리중", "취소", "반품", "배송중"];

        private static IEnumerable<object[]> GenerateRows(int total, IProgress<int> progress)
        {
            var rng = new Random(42);
            var baseDate = new DateTime(2023, 1, 1);

            for (int i = 1; i <= total; i++)
            {
                int qty = rng.Next(1, 100);
                decimal unitPrice = Math.Round((decimal)(rng.NextDouble() * 990000 + 10000), 0);
                yield return
                [
                    i,
                    rng.Next(1000, 10000),
                    $"고객{rng.Next(1, 100000):D5}",
                    $"user{rng.Next(10000, 99999)}@email.com",
                    $"010-{rng.Next(1000,9999)}-{rng.Next(1000,9999)}",
                    ProductNames[rng.Next(ProductNames.Length)],
                    Categories[rng.Next(Categories.Length)],
                    qty,
                    unitPrice,
                    unitPrice * qty,
                    baseDate.AddDays(rng.Next(0, 730)),
                    Statuses[rng.Next(Statuses.Length)]
                ];

                if (i % 10000 == 0)
                    progress.Report(i);
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void BtnBrowse_Click(object? sender, EventArgs e)
        {
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
        }
    }
}
