# ClosedXML 100만건 엑셀 추출 Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** WinForms 앱에서 버튼 클릭 시 100만건 주문 더미 데이터를 ClosedXML로 엑셀 파일로 추출하고 ProgressBar로 진행률을 표시한다.

**Architecture:** `Task.Run` + `IProgress<int>`로 UI 블로킹 없이 백그라운드에서 처리. `IEnumerable<object[]>` yield return으로 100만건을 스트리밍 생성하여 `InsertData`로 bulk insert. 1만건마다 프로그레스 업데이트.

**Tech Stack:** .NET 10, WinForms, ClosedXML 0.105.0

---

### Task 1: Form1 UI 컨트롤 구성 (Designer)

**Files:**
- Modify: `ClosedXML/Form1.Designer.cs`

**Step 1: Form1.Designer.cs에 컨트롤 추가**

`InitializeComponent()` 안을 아래로 교체:

```csharp
private void InitializeComponent()
{
    txtPath = new TextBox();
    btnBrowse = new Button();
    btnExport = new Button();
    progressBar = new ProgressBar();
    lblStatus = new Label();
    SuspendLayout();

    // txtPath
    txtPath.Location = new Point(12, 12);
    txtPath.Size = new Size(560, 23);
    txtPath.ReadOnly = true;

    // btnBrowse
    btnBrowse.Location = new Point(578, 11);
    btnBrowse.Size = new Size(94, 25);
    btnBrowse.Text = "찾아보기";
    btnBrowse.Click += BtnBrowse_Click;

    // btnExport
    btnExport.Location = new Point(12, 45);
    btnExport.Size = new Size(120, 30);
    btnExport.Text = "추출 시작";
    btnExport.Click += BtnExport_Click;

    // progressBar
    progressBar.Location = new Point(12, 90);
    progressBar.Size = new Size(760, 23);
    progressBar.Maximum = 100;

    // lblStatus
    lblStatus.Location = new Point(12, 120);
    lblStatus.Size = new Size(760, 23);
    lblStatus.Text = "대기 중";

    // Form1
    ClientSize = new Size(800, 160);
    Text = "ClosedXML 100만건 엑셀 추출";
    Controls.Add(txtPath);
    Controls.Add(btnBrowse);
    Controls.Add(btnExport);
    Controls.Add(progressBar);
    Controls.Add(lblStatus);
    ResumeLayout(false);
    PerformLayout();
}

private TextBox txtPath;
private Button btnBrowse;
private Button btnExport;
private ProgressBar progressBar;
private Label lblStatus;
```

**Step 2: 빌드 확인**

```bash
cd ClosedXML
dotnet build
```
Expected: Build succeeded

**Step 3: Commit**

```bash
git add ClosedXML/Form1.Designer.cs
git commit -m "feat: add UI controls for excel export"
```

---

### Task 2: 더미 데이터 생성기 구현

**Files:**
- Modify: `ClosedXML/Form1.cs`

**Step 1: using 추가 및 더미 데이터 생성 메서드 작성**

Form1.cs 상단에 using 추가:
```csharp
using ClosedXML.Excel;
```

Form1 클래스 안에 정적 배열과 생성 메서드 추가:

```csharp
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
            rng.Next(1000, 9999),
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
```

**Step 2: 빌드 확인**

```bash
dotnet build
```
Expected: Build succeeded

**Step 3: Commit**

```bash
git add ClosedXML/Form1.cs
git commit -m "feat: add dummy data generator with 12 columns"
```

---

### Task 3: 버튼 이벤트 핸들러 구현

**Files:**
- Modify: `ClosedXML/Form1.cs`

**Step 1: BtnBrowse_Click 구현**

```csharp
private void BtnBrowse_Click(object sender, EventArgs e)
{
    using var dlg = new SaveFileDialog
    {
        Filter = "Excel Files (*.xlsx)|*.xlsx",
        FileName = "export_1M.xlsx"
    };
    if (dlg.ShowDialog() == DialogResult.OK)
        txtPath.Text = dlg.FileName;
}
```

**Step 2: BtnExport_Click 구현**

```csharp
private async void BtnExport_Click(object sender, EventArgs e)
{
    if (string.IsNullOrEmpty(txtPath.Text))
    {
        MessageBox.Show("저장 경로를 선택하세요.");
        return;
    }

    btnExport.Enabled = false;
    progressBar.Value = 0;
    lblStatus.Text = "추출 시작...";

    const int total = 1_000_000;
    string path = txtPath.Text;

    var progress = new Progress<int>(count =>
    {
        int pct = (int)((double)count / total * 100);
        progressBar.Value = pct;
        lblStatus.Text = $"{count:N0} / {total:N0} 처리중... ({pct}%)";
    });

    await Task.Run(() =>
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Orders");

        // 헤더
        string[] headers = ["OrderId", "CustomerId", "CustomerName", "Email", "Phone",
                             "ProductName", "Category", "Quantity", "UnitPrice", "TotalPrice",
                             "OrderDate", "Status"];
        for (int c = 0; c < headers.Length; c++)
            ws.Cell(1, c + 1).Value = headers[c];

        ws.Cell(2, 1).InsertData(GenerateRows(total, progress));
        wb.SaveAs(path);
    });

    progressBar.Value = 100;
    lblStatus.Text = $"완료! {total:N0}건 추출 → {path}";
    btnExport.Enabled = true;
    MessageBox.Show($"추출 완료!\n{path}", "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
}
```

**Step 3: 빌드 확인**

```bash
dotnet build
```
Expected: Build succeeded

**Step 4: 수동 테스트**
1. 앱 실행 (`dotnet run` 또는 F5)
2. 찾아보기 클릭 → 저장 경로 선택
3. 추출 시작 클릭
4. ProgressBar가 증가하며 레이블이 업데이트되는지 확인
5. 완료 후 생성된 xlsx 파일 열어 100만행 확인

**Step 5: Commit**

```bash
git add ClosedXML/Form1.cs
git commit -m "feat: implement async excel export with progress bar"
```
