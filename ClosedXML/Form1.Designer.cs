namespace ClosedXML
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

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

        #endregion

        private TextBox txtPath;
        private Button btnBrowse;
        private Button btnExport;
        private ProgressBar progressBar;
        private Label lblStatus;
    }
}
