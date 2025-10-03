using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ExcelDataReader;

namespace drilling
{
    public class CsvReaderService
    {
        private readonly List<DrillingRecord> _records;

        public CsvReaderService()
        {
            _records = new List<DrillingRecord>();
        }

        // Backward-compatible method name. Now delegates to ImportFromFile
        public List<DrillingRecord> ImportFromCsv(string filePath)
        {
            return ImportFromFile(filePath);
        }

        public List<DrillingRecord> ImportFromFile(string filePath)
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
                else if (extension == ".xlsx" || extension == ".xls")
                {
                    records = ReadExcelFile(filePath);
                }
                else
                {
                    MessageBox.Show("لطفاً فایل را به فرمت CSV یا Excel (xlsx/xls) انتخاب کنید.", "فرمت پشتیبانی نشده",
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

        private List<DrillingRecord> ReadExcelFile(string filePath)
        {
            var records = new List<DrillingRecord>();

            try
            {
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
                    {
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration
                        {
                            // Assume first row is header and skip it
                            UseHeaderRow = true
                        }
                    });

                    if (dataSet.Tables.Count == 0)
                    {
                        MessageBox.Show("فایل اکسل فاقد داده است.", "هشدار", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return records;
                    }

                    var table = dataSet.Tables[0];
                    foreach (DataRow row in table.Rows)
                    {
                        var record = CreateRecordFromDataRow(row);
                        if (record != null)
                        {
                            records.Add(record);
                        }
                    }
                }

                return records;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در خواندن فایل Excel: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new List<DrillingRecord>();
            }
        }

        private DrillingRecord CreateRecordFromDataRow(DataRow row)
        {
            try
            {
                object[] cells = row.ItemArray;

                DateTime date = ToDateTimeSafe(GetCell(cells, 0));
                string shift = ToStringSafe(GetCell(cells, 1));
                string run = ToStringSafe(GetCell(cells, 2));
                decimal from = ToDecimalSafe(GetCell(cells, 3));
                decimal to = ToDecimalSafe(GetCell(cells, 4));
                TimeSpan startTime = ToTimeSpanSafe(GetCell(cells, 5));
                TimeSpan finishTime = ToTimeSpanSafe(GetCell(cells, 6));
                decimal cr = ToDecimalSafe(GetCell(cells, 7));
                decimal rqd = ToDecimalSafe(GetCell(cells, 8));
                decimal len = ToDecimalSafe(GetCell(cells, 9));
                decimal totalTime = ToDecimalSafe(GetCell(cells, 10));
                string typeOfDrilling = ToStringSafe(GetCell(cells, 11));
                string sizeOfCore = ToStringSafe(GetCell(cells, 12));
                string boxNo = ToStringSafe(GetCell(cells, 13));
                decimal dip = ToDecimalSafe(GetCell(cells, 14));
                decimal az = ToDecimalSafe(GetCell(cells, 15));
                string yesOrNo = ToStringSafe(GetCell(cells, 16));

                var record = new DrillingRecord
                {
                    DATE = date,
                    SHIFT = shift,
                    RUN = run,
                    FROM = from,
                    TO = to,
                    START_TIME = startTime,
                    FINISH_TIME = finishTime,
                    CR = cr,
                    RQD = rqd,
                    LEN = len,
                    TOTAL_TIME = totalTime,
                    TYPE_OF_DRILLING = typeOfDrilling,
                    SIZE_OF_CORE = sizeOfCore,
                    BOX_NO = boxNo,
                    DIP = dip,
                    AZ = az,
                    YES_OR_NO = yesOrNo
                };

                if (record.LEN == 0 && record.TO > record.FROM)
                {
                    record.LEN = record.TO - record.FROM;
                }

                return record;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"خطا در سطر اکسل: {ex.Message}");
                return null;
            }
        }

        private static object GetCell(object[] cells, int index)
        {
            if (cells == null) return null;
            if (index < 0 || index >= cells.Length) return null;
            return cells[index];
        }

        private static string ToStringSafe(object value)
        {
            return value == null || value == DBNull.Value ? string.Empty : value.ToString().Trim();
        }

        private static decimal ToDecimalSafe(object value)
        {
            if (value == null || value == DBNull.Value) return 0;
            try
            {
                switch (value)
                {
                    case decimal m:
                        return m;
                    case double d:
                        return Convert.ToDecimal(d);
                    case float f:
                        return Convert.ToDecimal(f);
                    case int i:
                        return i;
                    case long l:
                        return l;
                }
                if (decimal.TryParse(value.ToString(), out var result)) return result;
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        private static DateTime ToDateTimeSafe(object value)
        {
            if (value == null || value == DBNull.Value) return DateTime.Today;
            if (value is DateTime dt) return dt;
            if (DateTime.TryParse(value.ToString(), out var parsed)) return parsed;
            // Excel numeric date (days since 1899-12-30)
            if (double.TryParse(value.ToString(), out var oa))
            {
                try { return DateTime.FromOADate(oa); } catch { }
            }
            return DateTime.Today;
        }

        private static TimeSpan ToTimeSpanSafe(object value)
        {
            if (value == null || value == DBNull.Value) return TimeSpan.Zero;
            if (value is TimeSpan ts) return ts;
            if (value is DateTime dt) return dt.TimeOfDay;
            if (TimeSpan.TryParse(value.ToString(), out var parsed)) return parsed;
            // Excel time as fraction of a day
            if (double.TryParse(value.ToString(), out var fraction))
            {
                try { return TimeSpan.FromDays(fraction); } catch { }
            }
            return TimeSpan.Zero;
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