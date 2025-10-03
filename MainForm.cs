using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace drilling
{
    public partial class MainForm : Form
    {
        private readonly CsvReaderService _csvService;

        public MainForm()
        {
            InitializeComponent();
            _csvService = new CsvReaderService();
            InitializeDataGridView();
        }

        private void InitializeDataGridView()
        {
            dataGridView1.Columns.Clear();

            dataGridView1.Columns.Add("DATE", "تاریخ");
            dataGridView1.Columns.Add("SHIFT", "شیفت");
            dataGridView1.Columns.Add("RUN", "ران");
            dataGridView1.Columns.Add("FROM", "از");
            dataGridView1.Columns.Add("TO", "تا");
            dataGridView1.Columns.Add("START_TIME", "زمان شروع");
            dataGridView1.Columns.Add("FINISH_TIME", "زمان پایان");
            dataGridView1.Columns.Add("CR", "CR%");
            dataGridView1.Columns.Add("RQD", "RQD%");
            dataGridView1.Columns.Add("LEN", "طول (متر)");
            dataGridView1.Columns.Add("TOTAL_TIME", "زمان کل");
            dataGridView1.Columns.Add("TYPE_OF_DRILLING", "نوع حفاری");
            dataGridView1.Columns.Add("SIZE_OF_CORE", "سایز مغزه");
            dataGridView1.Columns.Add("BOX_NO", "شماره باکس");
            dataGridView1.Columns.Add("DIP", "دیپ");
            dataGridView1.Columns.Add("AZ", "آزیموت");
            dataGridView1.Columns.Add("YES_OR_NO", "بله/خیر");

            dataGridView1.Columns["LEN"].Width = 80;
            dataGridView1.Columns["CR"].Width = 60;
            dataGridView1.Columns["RQD"].Width = 60;
            dataGridView1.Columns["DATE"].Width = 90;
            dataGridView1.Columns["FROM"].Width = 70;
            dataGridView1.Columns["TO"].Width = 70;
        }

        private void BtnImportCsv_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "فایل‌های داده|*.csv;*.xlsx;*.xls|CSV (*.csv)|*.csv|Excel (*.xlsx)|*.xlsx|Excel 97-2003 (*.xls)|*.xls|همه فایل‌ها|*.*";
                openFileDialog.Title = "انتخاب فایل داده (CSV/Excel)";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        Cursor = Cursors.WaitCursor;
                        btnImportCsv.Enabled = false;
                        lblStatus.Text = "در حال خواندن فایل...";

                        var records = _csvService.ImportFromFile(openFileDialog.FileName);

                        if (records.Count > 0)
                        {
                            DisplayRecordsInGrid(records);
                            UpdateStatistics();
                            lblStatus.Text = $"فایل با موفقیت بارگذاری شد. تعداد رکوردها: {records.Count}";
                        }
                        else
                        {
                            lblStatus.Text = "هیچ داده معتبری یافت نشد.";
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"خطا: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        lblStatus.Text = "خطا در بارگذاری فایل";
                    }
                    finally
                    {
                        Cursor = Cursors.Default;
                        btnImportCsv.Enabled = true;
                    }
                }
            }
        }

        private void DisplayRecordsInGrid(List<DrillingRecord> records)
        {
            dataGridView1.Rows.Clear();

            foreach (var record in records)
            {
                dataGridView1.Rows.Add(
                    record.DATE.ToShortDateString(),
                    record.SHIFT,
                    record.RUN,
                    record.FROM.ToString("F2"),
                    record.TO.ToString("F2"),
                    record.START_TIME.ToString(@"hh\:mm"),
                    record.FINISH_TIME.ToString(@"hh\:mm"),
                    $"{record.CR}%",
                    $"{record.RQD}%",
                    $"{record.LEN:N2}",
                    $"{record.TOTAL_TIME}",
                    record.TYPE_OF_DRILLING,
                    record.SIZE_OF_CORE,
                    record.BOX_NO,
                    $"{record.DIP}°",
                    $"{record.AZ}°",
                    record.YES_OR_NO
                );
            }
        }

        private void UpdateStatistics()
        {
            decimal totalMeterage = _csvService.CalculateTotalMeterage();
            int recordsCount = _csvService.GetRecordsCount();
            decimal avgCR = _csvService.CalculateAverageCR();
            decimal avgRQD = _csvService.CalculateAverageRQD();

            lblTotalMeterage.Text = $"متراژ کل: {totalMeterage:N2} متر";
            lblRecordsCount.Text = $"تعداد رکوردها: {recordsCount}";
            lblAverageCR.Text = $"میانگین CR: {avgCR:N1}%";
            lblAverageRQD.Text = $"میانگین RQD: {avgRQD:N1}%";

            lblTotalMeterage.ForeColor = totalMeterage > 0 ? System.Drawing.Color.DarkGreen : System.Drawing.Color.Black;
        }

        private void BtnExportReport_Click(object sender, EventArgs e)
        {
            if (_csvService.GetRecordsCount() == 0)
            {
                MessageBox.Show("هیچ داده‌ای برای خروجی‌گیری وجود ندارد.", "هشدار",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "فایل متنی|*.txt";
            saveFileDialog.Title = "ذخیره گزارش";
            saveFileDialog.FileName = $"گزارش_حفاری_{DateTime.Now:yyyy-MM-dd}.txt";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    ExportToTextFile(saveFileDialog.FileName);
                    MessageBox.Show("گزارش با موفقیت ذخیره شد.", "موفقیت",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطا در ذخیره گزارش: {ex.Message}", "خطا",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ExportToTextFile(string filePath)
        {
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(filePath))
            {
                writer.WriteLine("گزارش متراژ حفاری");
                writer.WriteLine("تاریخ تولید: " + DateTime.Now.ToString("yyyy/MM/dd HH:mm"));
                writer.WriteLine("=================================");
                writer.WriteLine($"متراژ کل: {_csvService.CalculateTotalMeterage():N2} متر");
                writer.WriteLine($"تعداد رکوردها: {_csvService.GetRecordsCount()}");
                writer.WriteLine($"میانگین CR: {_csvService.CalculateAverageCR():N1}%");
                writer.WriteLine($"میانگین RQD: {_csvService.CalculateAverageRQD():N1}%");
                writer.WriteLine("=================================");
            }
        }

        private void BtnClearData_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("آیا از پاک کردن تمام داده‌ها مطمئن هستید؟", "تأیید",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                dataGridView1.Rows.Clear();
                _csvService.GetAllRecords().Clear();
                UpdateStatistics();
                lblStatus.Text = "داده‌ها پاک شدند.";
            }
        }

        private void BtnGroupByLength_Click(object sender, EventArgs e)
        {
            if (_csvService.GetRecordsCount() == 0)
            {
                MessageBox.Show("هیچ داده‌ای برای گروه‌بندی وجود ندارد.", "هشدار",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // نمایش فرم برای دریافت متراژ
            using (var lengthForm = new LengthInputForm())
            {
                if (lengthForm.ShowDialog() == DialogResult.OK)
                {
                    decimal targetLength = lengthForm.TargetLength;
                    GroupRecordsByLength(targetLength);
                }
            }
        }

        private void GroupRecordsByLength(decimal targetLength)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                var records = _csvService.GetAllRecords();

                // پاک کردن داده‌های فعلی
                dataGridView1.Rows.Clear();

                decimal currentSum = 0;
                int groupNumber = 1;
                var currentGroupRecords = new List<DrillingRecord>();

                foreach (var record in records)
                {
                    if (currentSum + record.LEN <= targetLength)
                    {
                        // اضافه کردن رکورد کامل به گروه فعلی
                        currentGroupRecords.Add(record);
                        currentSum += record.LEN;
                    }
                    else
                    {
                        // اگر اضافه کردن این رکورد از متراژ هدف بیشتر شود
                        decimal remaining = targetLength - currentSum;

                        if (remaining > 0)
                        {
                            // تقسیم رکورد
                            var partialRecord = new DrillingRecord
                            {
                                DATE = record.DATE,
                                SHIFT = record.SHIFT,
                                RUN = record.RUN + " (تقسیم)",
                                FROM = record.FROM,
                                TO = record.FROM + remaining,
                                START_TIME = record.START_TIME,
                                FINISH_TIME = record.FINISH_TIME,
                                CR = record.CR,
                                RQD = record.RQD,
                                LEN = remaining,
                                TOTAL_TIME = record.TOTAL_TIME * (remaining / record.LEN),
                                TYPE_OF_DRILLING = record.TYPE_OF_DRILLING,
                                SIZE_OF_CORE = record.SIZE_OF_CORE,
                                BOX_NO = record.BOX_NO + "-1",
                                DIP = record.DIP,
                                AZ = record.AZ,
                                YES_OR_NO = record.YES_OR_NO
                            };

                            currentGroupRecords.Add(partialRecord);
                            currentSum += remaining;

                            // نمایش گروه فعلی
                            DisplayGroupInGrid(currentGroupRecords, groupNumber, Color.LightGreen);
                            groupNumber++;
                            currentGroupRecords.Clear();
                            currentSum = 0;

                            // بخش باقیمانده از رکورد تقسیم شده
                            decimal remainingPart = record.LEN - remaining;
                            if (remainingPart > 0)
                            {
                                var remainingRecord = new DrillingRecord
                                {
                                    DATE = record.DATE,
                                    SHIFT = record.SHIFT,
                                    RUN = record.RUN + " (تقسیم)",
                                    FROM = record.FROM + remaining,
                                    TO = record.TO,
                                    START_TIME = record.START_TIME,
                                    FINISH_TIME = record.FINISH_TIME,
                                    CR = record.CR,
                                    RQD = record.RQD,
                                    LEN = remainingPart,
                                    TOTAL_TIME = record.TOTAL_TIME * (remainingPart / record.LEN),
                                    TYPE_OF_DRILLING = record.TYPE_OF_DRILLING,
                                    SIZE_OF_CORE = record.SIZE_OF_CORE,
                                    BOX_NO = record.BOX_NO + "-2",
                                    DIP = record.DIP,
                                    AZ = record.AZ,
                                    YES_OR_NO = record.YES_OR_NO
                                };

                                currentGroupRecords.Add(remainingRecord);
                                currentSum = remainingPart;
                            }
                        }
                        else
                        {
                            // نمایش گروه فعلی
                            DisplayGroupInGrid(currentGroupRecords, groupNumber, Color.LightGreen);
                            groupNumber++;
                            currentGroupRecords.Clear();

                            // شروع گروه جدید با رکورد فعلی
                            currentGroupRecords.Add(record);
                            currentSum = record.LEN;
                        }
                    }
                }

                // نمایش آخرین گروه
                if (currentGroupRecords.Count > 0)
                {
                    DisplayGroupInGrid(currentGroupRecords, groupNumber, Color.LightGreen);
                }

                lblStatus.Text = $"داده‌ها بر اساس متراژ {targetLength} متری گروه‌بندی شدند. تعداد گروه‌ها: {groupNumber}";
                MessageBox.Show($"گروه‌بندی با موفقیت انجام شد.\nتعداد گروه‌ها: {groupNumber}\nمتراژ هر گروه: {targetLength} متر",
                    "گروه‌بندی موفق", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در گروه‌بندی: {ex.Message}", "خطا",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void DisplayGroupInGrid(List<DrillingRecord> records, int groupNumber, Color groupColor)
        {
            // اضافه کردن یک سطر جداکننده
            int separatorIndex = dataGridView1.Rows.Add();
            dataGridView1.Rows[separatorIndex].DefaultCellStyle.BackColor = Color.LightGray;
            dataGridView1.Rows[separatorIndex].Cells[0].Value = $"--- گروه {groupNumber} ---";

            // اضافه کردن رکوردهای گروه
            foreach (var record in records)
            {
                int rowIndex = dataGridView1.Rows.Add();
                var row = dataGridView1.Rows[rowIndex];

                // پر کردن داده‌ها
                row.Cells[0].Value = record.DATE.ToShortDateString();
                row.Cells[1].Value = record.SHIFT;
                row.Cells[2].Value = record.RUN;
                row.Cells[3].Value = record.FROM.ToString("F2");
                row.Cells[4].Value = record.TO.ToString("F2");
                row.Cells[5].Value = record.START_TIME.ToString(@"hh\:mm");
                row.Cells[6].Value = record.FINISH_TIME.ToString(@"hh\:mm");
                row.Cells[7].Value = $"{record.CR}%";
                row.Cells[8].Value = $"{record.RQD}%";
                row.Cells[9].Value = $"{record.LEN:N2}";
                row.Cells[10].Value = $"{record.TOTAL_TIME}";
                row.Cells[11].Value = record.TYPE_OF_DRILLING;
                row.Cells[12].Value = record.SIZE_OF_CORE;
                row.Cells[13].Value = record.BOX_NO;
                row.Cells[14].Value = $"{record.DIP}°";
                row.Cells[15].Value = $"{record.AZ}°";
                row.Cells[16].Value = record.YES_OR_NO;

                // رنگ‌آمیزی سطر
                row.DefaultCellStyle.BackColor = groupColor;
            }
        }
    }
}