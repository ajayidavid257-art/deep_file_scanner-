using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileScanner
{
    public class App : Form
    {
        private TextBox txtSearch;
        private TextBox txtOutputPath;
        private TextBox txtScanPath;
        private Button btnOutputBrowse;
        private Button btnScanBrowse;
        private Button btnStart;
        private Button btnStop;
        private Button btnClearLog;
        private CheckBox chkScanAll;
        private CheckBox chkDeepSearch;
        private CheckBox chkTxt;
        private CheckBox chkCsv;
        private CheckBox chkJson;
        private CheckBox chkKey;
        private CheckBox chkPem;
        private CheckBox chkLog;
        private CheckBox chkConfig;
        private CheckBox chkIni;
        private CheckBox chkXml;
        private CheckBox chkEnv;
        private CheckBox chkConf;
        private CheckBox chkXlsx;           // ← NEW

        private CheckBox chkScanByFilename;
        private ComboBox cmbSizeLimit;
        private ProgressBar progressBar;
        private RichTextBox logBox;
        private Label lblStatus;
        private Label lblHelp;

        private string outputFolder = "";
        private int processedFiles = 0;
        private int copiedFiles = 0;
        private bool isScanning = false;

        public App()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "Fast Priority Scanner - Sensitive Files";
            this.Size = new Size(1150, 880);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 10);

            var lblSearch = new Label { Text = "Search terms / phrases:", Location = new Point(20, 20), AutoSize = true };
            txtSearch = new TextBox { Location = new Point(20, 45), Size = new Size(900, 30), Text = "mercury reconstruct, exact phrase here, password" };

            lblHelp = new Label 
            { 
                Text = "Separate terms with commas (,). Spaces are preserved.\n" +
                       "Example: mercury reconstruct, full sentence here, report2025",
                Location = new Point(20, 78), 
                Size = new Size(900, 35),
                ForeColor = Color.LightYellow
            };

            var lblOutput = new Label { Text = "Output Folder:", Location = new Point(20, 125), AutoSize = true };
            txtOutputPath = new TextBox { Location = new Point(20, 150), Size = new Size(770, 30), ReadOnly = true };
            btnOutputBrowse = new Button { Text = "Browse", Location = new Point(800, 150), Size = new Size(110, 30) };
            btnOutputBrowse.Click += BtnOutputBrowse_Click;

            chkScanAll = new CheckBox { Text = "Scan All Drives (Fixed + USB/External)", Location = new Point(20, 190), Checked = true, AutoSize = true };
            chkScanAll.CheckedChanged += ChkScanAll_CheckedChanged;

            chkDeepSearch = new CheckBox { Text = "Deep Search (include hidden files & folders)", Location = new Point(20, 215), AutoSize = true };

            var lblScan = new Label { Text = "Specific Folder:", Location = new Point(20, 245), AutoSize = true };
            txtScanPath = new TextBox { Location = new Point(20, 270), Size = new Size(770, 30), ReadOnly = true };
            btnScanBrowse = new Button { Text = "Browse", Location = new Point(800, 270), Size = new Size(110, 30) };
            btnScanBrowse.Click += BtnScanBrowse_Click;
            btnScanBrowse.Enabled = false;

            // Search Mode & Size Limit
            var lblFilename = new Label { Text = "Search Mode:", Location = new Point(20, 310), AutoSize = true };
            chkScanByFilename = new CheckBox 
            { 
                Text = "Scan by Filename (instead of content)", 
                Location = new Point(130, 310), 
                AutoSize = true 
            };

            var lblSize = new Label { Text = "Max File Size:", Location = new Point(20, 335), AutoSize = true };
            cmbSizeLimit = new ComboBox 
            { 
                Location = new Point(130, 335), 
                Size = new Size(140, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbSizeLimit.Items.AddRange(new string[] { "3 KB", "100 KB", "1 MB", "20 MB", "200 MB" });
            cmbSizeLimit.SelectedIndex = 0;

            // File Types
            var lblTypes = new Label { Text = "File Types:", Location = new Point(20, 365), AutoSize = true };

            // Row 1
            chkTxt    = new CheckBox { Text = ".txt",    Location = new Point(130, 365), Checked = true, AutoSize = true };
            chkCsv    = new CheckBox { Text = ".csv",    Location = new Point(200, 365), Checked = true, AutoSize = true };
            chkJson   = new CheckBox { Text = ".json",   Location = new Point(270, 365), Checked = true, AutoSize = true };
            chkKey    = new CheckBox { Text = ".key",    Location = new Point(350, 365), Checked = true, AutoSize = true };
            chkPem    = new CheckBox { Text = ".pem",    Location = new Point(430, 365), Checked = true, AutoSize = true };
            chkLog    = new CheckBox { Text = ".log",    Location = new Point(510, 365), Checked = true, AutoSize = true };
            chkXlsx   = new CheckBox { Text = ".xlsx",   Location = new Point(590, 365), Checked = true, AutoSize = true }; // ← NEW

            // Row 2
            chkConfig = new CheckBox { Text = ".config", Location = new Point(130, 390), Checked = true, AutoSize = true };
            chkIni    = new CheckBox { Text = ".ini",    Location = new Point(220, 390), Checked = true, AutoSize = true };
            chkXml    = new CheckBox { Text = ".xml",    Location = new Point(290, 390), Checked = true, AutoSize = true };
            chkEnv    = new CheckBox { Text = ".env",    Location = new Point(360, 390), Checked = true, AutoSize = true };
            chkConf   = new CheckBox { Text = ".conf",   Location = new Point(430, 390), Checked = true, AutoSize = true };

            // Buttons
            btnStart = new Button { Text = "Start Scan", Location = new Point(20, 430), Size = new Size(160, 45), BackColor = Color.DodgerBlue, ForeColor = Color.White };
            btnStart.Click += BtnStart_Click;

            btnStop = new Button { Text = "Stop", Location = new Point(190, 430), Size = new Size(120, 45), BackColor = Color.OrangeRed, ForeColor = Color.White };
            btnStop.Click += BtnStop_Click;
            btnStop.Enabled = false;

            btnClearLog = new Button { Text = "Clear Log", Location = new Point(320, 430), Size = new Size(140, 45), BackColor = Color.Gray, ForeColor = Color.White };
            btnClearLog.Click += BtnClearLog_Click;

            progressBar = new ProgressBar 
            { 
                Location = new Point(20, 485), 
                Size = new Size(1100, 25),
                Style = ProgressBarStyle.Blocks     // Will switch to Marquee during scan
            };

            lblStatus = new Label { Text = "Ready", Location = new Point(20, 515), AutoSize = true, ForeColor = Color.Lime };

            logBox = new RichTextBox 
            { 
                Location = new Point(20, 545), 
                Size = new Size(1100, 260), 
                ReadOnly = true, 
                BackColor = Color.FromArgb(20,20,20), 
                ForeColor = Color.LightGray,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };

            this.Controls.AddRange(new Control[] { 
                lblSearch, txtSearch, lblHelp,
                lblOutput, txtOutputPath, btnOutputBrowse,
                chkScanAll, chkDeepSearch, lblScan, txtScanPath, btnScanBrowse,
                lblFilename, chkScanByFilename,
                lblSize, cmbSizeLimit,
                lblTypes, 
                chkTxt, chkCsv, chkJson, chkKey, chkPem, chkLog, chkXlsx,
                chkConfig, chkIni, chkXml, chkEnv, chkConf,
                btnStart, btnStop, btnClearLog, progressBar, lblStatus, logBox 
            });
        }

        private long GetMaxFileSize()
        {
            string selected = cmbSizeLimit.SelectedItem?.ToString() ?? "3 KB";
            return selected switch
            {
                "3 KB"   => 3L * 1024,
                "100 KB" => 100L * 1024,
                "1 MB"   => 1L * 1024 * 1024,
                "20 MB"  => 20L * 1024 * 1024,
                "200 MB" => 200L * 1024 * 1024,
                _        => 3L * 1024
            };
        }

        private async void BtnStart_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(outputFolder))
            {
                MessageBox.Show("Please select an output folder", "Warning");
                return;
            }

            var searchText = txtSearch.Text.Trim();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                MessageBox.Show("Please enter search terms", "Warning");
                return;
            }

            logBox.Clear();
            processedFiles = 0;
            copiedFiles = 0;
            isScanning = true;

            // Progress Bar Improvement: Switch to Marquee (animated)
            if (progressBar.InvokeRequired)
                progressBar.Invoke(new Action(() => progressBar.Style = ProgressBarStyle.Marquee));
            else
                progressBar.Style = ProgressBarStyle.Marquee;

            Log("🚀 Starting scan...", Color.Yellow);

            await Task.Run(() => PerformScan(searchText));

            isScanning = false;
            Log("✅ Scan completed!", Color.Lime);
            ResetProgress();
        }

        private void PerformScan(string searchInput)
        {
            var keywords = searchInput.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(k => k.Trim().ToLower())
                                      .Where(k => !string.IsNullOrWhiteSpace(k))
                                      .ToArray();

            var extensions = new List<string>();
            if (chkTxt.Checked)    extensions.Add(".txt");
            if (chkCsv.Checked)    extensions.Add(".csv");
            if (chkJson.Checked)   extensions.Add(".json");
            if (chkKey.Checked)    extensions.Add(".key");
            if (chkPem.Checked)    extensions.Add(".pem");
            if (chkLog.Checked)    extensions.Add(".log");
            if (chkConfig.Checked) extensions.Add(".config");
            if (chkIni.Checked)    extensions.Add(".ini");
            if (chkXml.Checked)    extensions.Add(".xml");
            if (chkEnv.Checked)    extensions.Add(".env");
            if (chkConf.Checked)   extensions.Add(".conf");
            if (chkXlsx.Checked)   extensions.Add(".xlsx");     // ← NEW

            long maxSize = GetMaxFileSize();

            Log($"Mode: {(chkScanByFilename.Checked ? "Filename Search" : "Content Search")}", Color.Cyan);
            Log($"File types: {string.Join(", ", extensions)}", Color.Cyan);
            Log($"Maximum file size: {maxSize / 1024} KB", Color.Cyan);

            foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
            {
                Log($"Scanning drive: {drive.Name}", Color.Cyan);
                ScanDirectory(drive.Name, keywords, extensions, maxSize);
            }
        }

        private void ScanDirectory(string path, string[] keywords, List<string> extensions, long maxSize)
        {
            if (!string.IsNullOrEmpty(outputFolder) && path.StartsWith(outputFolder, StringComparison.OrdinalIgnoreCase))
                return;

            string lower = path.ToLower();
            if (lower.Contains(@"\windows\") || lower.Contains(@"\program files") || 
                lower.Contains(@"\programdata\") || lower.Contains(@"\appdata\local\temp") ||
                lower.Contains(@"\system32\") || lower.Contains(@"\syswow64\")) 
                return;

            try
            {
                foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(f => extensions.Contains(Path.GetExtension(f).ToLower())))
                {
                    processedFiles++;
                    UpdateProgress();

                    try
                    {
                        var fi = new FileInfo(file);
                        if (fi.Length > maxSize) continue;

                        bool isHidden = (fi.Attributes & FileAttributes.Hidden) != 0;
                        if (!chkDeepSearch.Checked && isHidden) continue;

                        bool match = false;

                        if (chkScanByFilename.Checked)
                        {
                            string fileNameLower = Path.GetFileName(file).ToLower();
                            match = keywords.Any(k => fileNameLower.Contains(k));
                        }
                        else
                        {
                            string content = File.ReadAllText(file).ToLower();
                            match = keywords.Any(k => content.Contains(k));
                        }

                        if (match)
                        {
                            string dest = Path.Combine(outputFolder, Path.GetFileName(file));
                            int count = 1;
                            string name = Path.GetFileNameWithoutExtension(file);
                            string ext = Path.GetExtension(file);

                            while (File.Exists(dest))
                            {
                                dest = Path.Combine(outputFolder, $"{name}_{count}{ext}");
                                count++;
                            }

                            File.Copy(file, dest, true);
                            copiedFiles++;
                            Log($"✓ Copied: {file} ({fi.Length / 1024} KB)", Color.Lime);
                        }
                    }
                    catch { }
                }

                foreach (var dir in Directory.EnumerateDirectories(path))
                {
                    ScanDirectory(dir, keywords, extensions, maxSize);
                }
            }
            catch { }
        }

        private void UpdateProgress()
        {
            if (progressBar.InvokeRequired)
            {
                progressBar.Invoke(new Action(() =>
                {
                    lblStatus.Text = $"Processed: {processedFiles} | Found: {copiedFiles}";
                }));
            }
            else
            {
                lblStatus.Text = $"Processed: {processedFiles} | Found: {copiedFiles}";
            }
        }

        private void ResetProgress()
        {
            if (progressBar.InvokeRequired)
                progressBar.Invoke(new Action(() => 
                { 
                    progressBar.Style = ProgressBarStyle.Blocks;
                    progressBar.Value = 0; 
                    lblStatus.Text = "Ready"; 
                }));
            else 
            { 
                progressBar.Style = ProgressBarStyle.Blocks;
                progressBar.Value = 0; 
                lblStatus.Text = "Ready"; 
            }
        }

        private void Log(string message, Color color)
        {
            if (logBox.InvokeRequired)
            {
                logBox.Invoke(new Action(() =>
                {
                    logBox.SelectionColor = color;
                    logBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
                    logBox.ScrollToCaret();
                }));
            }
            else
            {
                logBox.SelectionColor = color;
                logBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
                logBox.ScrollToCaret();
            }
        }

        private void BtnClearLog_Click(object sender, EventArgs e) => logBox.Clear();

        private void BtnOutputBrowse_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    outputFolder = fbd.SelectedPath;
                    txtOutputPath.Text = outputFolder;
                }
        }

        private void BtnScanBrowse_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
                if (fbd.ShowDialog() == DialogResult.OK)
                    txtScanPath.Text = fbd.SelectedPath;
        }

        private void ChkScanAll_CheckedChanged(object sender, EventArgs e)
        {
            btnScanBrowse.Enabled = !chkScanAll.Checked;
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            Log("Stop requested... (Note: Full stop requires app restart currently)", Color.Orange);
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new App());
        }
    }
}