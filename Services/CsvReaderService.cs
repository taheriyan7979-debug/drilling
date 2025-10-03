using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace drilling
{
    public class CsvReaderService
    {
        private readonly List<DrillingRecord> _records;

        public CsvReaderService()
        {
            _records = new List<DrillingRecord>();
        }

        public List<DrillingRecord> ImportFromCsv(string filePath)
        {
            var records = new List<DrillingRecord>();

            try
            {
                if (!File.Exists(filePath))
                {
                    MessageBox.Show("فایل مورد نظر یافت نشد.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return records;
                }

                string extension = Path.GetExtension(filePath).ToLower();

                if (extension == ".csv")
                {
                    records = ReadCsvFile(filePath);
                }
                else
                {
                    MessageBox.Show("لطفاً فایل را به فرمت CSV انتخاب کنید.", "فرمت پشتیبانی نشده",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return records;
                }

                _records.Clear();
                _records.AddRange(records);

                if (records.Count > 0)
                {
                    MessageBox.Show($"تعداد {records.Count} رکورد با موفقیت خوانده شد.", "موفقیت",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                return records;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در خواندن فایل: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new List<DrillingRecord>();
            }
        }

        private List<DrillingRecord> ReadCsvFile(string filePath)
        {
            var records = new List<DrillingRecord>();

            try
            {
                string[] lines = File.ReadAllLines(filePath);

                if (lines.Length < 2)
                {
                    MessageBox.Show("فایل خالی است یا فقط هدر دارد.", "هشدار", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return records;
                }

                for (int i = 1; i < lines.Length; i++)
                {
                    var record = CreateRecordFromCsvLine(lines[i]);
                    if (record != null)
                    {
                        records.Add(record);
                    }
                }

                return records;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در خواندن فایل CSV: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new List<DrillingRecord>();
            }
        }

        private DrillingRecord CreateRecordFromCsvLine(string line)
        {
            try
            {
                string[] fields = line.Split(',');

                if (fields.Length < 17)
                {
                    Array.Resize(ref fields, 17);
                }

                var record = new DrillingRecord
                {
                    DATE = DateTime.TryParse(fields[0], out DateTime date) ? date : DateTime.Today,
                    SHIFT = fields[1]?.Trim() ?? "",
                    RUN = fields[2]?.Trim() ?? "",
                    FROM = decimal.TryParse(fields[3], out decimal from) ? from : 0,
                    TO = decimal.TryParse(fields[4], out decimal to) ? to : 0,
                    START_TIME = TimeSpan.TryParse(fields[5], out TimeSpan start) ? start : TimeSpan.Zero,
                    FINISH_TIME = TimeSpan.TryParse(fields[6], out TimeSpan finish) ? finish : TimeSpan.Zero,
                    CR = decimal.TryParse(fields[7], out decimal cr) ? cr : 0,
                    RQD = decimal.TryParse(fields[8], out decimal rqd) ? rqd : 0,
                    LEN = decimal.TryParse(fields[9], out decimal len) ? len : 0,
                    TOTAL_TIME = decimal.TryParse(fields[10], out decimal totalTime) ? totalTime : 0,
                    TYPE_OF_DRILLING = fields[11]?.Trim() ?? "",
                    SIZE_OF_CORE = fields[12]?.Trim() ?? "",
                    BOX_NO = fields[13]?.Trim() ?? "",
                    DIP = decimal.TryParse(fields[14], out decimal dip) ? dip : 0,
                    AZ = decimal.TryParse(fields[15], out decimal az) ? az : 0,
                    YES_OR_NO = fields[16]?.Trim() ?? ""
                };

                if (record.LEN == 0 && record.TO > record.FROM)
                {
                    record.LEN = record.TO - record.FROM;
                }

                return record;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"خطا در سطر: {line} - {ex.Message}");
                return null;
            }
        }

        public decimal CalculateTotalMeterage()
        {
            return _records.Sum(r => r.LEN);
        }

        public decimal CalculateAverageCR()
        {
            if (_records.Count == 0) return 0;
            return _records.Average(r => r.CR);
        }

        public decimal CalculateAverageRQD()
        {
            if (_records.Count == 0) return 0;
            return _records.Average(r => r.RQD);
        }

        public int GetRecordsCount()
        {
            return _records.Count;
        }

        public List<DrillingRecord> GetAllRecords()
        {
            return _records;
        }
    }
}