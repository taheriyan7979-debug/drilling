using System;

namespace drilling
{
    public class DrillingRecord
    {
        public DateTime DATE { get; set; }
        public string SHIFT { get; set; }
        public string RUN { get; set; }
        public decimal FROM { get; set; }
        public decimal TO { get; set; }
        public TimeSpan START_TIME { get; set; }
        public TimeSpan FINISH_TIME { get; set; }
        public decimal CR { get; set; }
        public decimal RQD { get; set; }
        public decimal LEN { get; set; }
        public decimal TOTAL_TIME { get; set; }
        public string TYPE_OF_DRILLING { get; set; }
        public string SIZE_OF_CORE { get; set; }
        public string BOX_NO { get; set; }
        public decimal DIP { get; set; }
        public decimal AZ { get; set; }
        public string YES_OR_NO { get; set; }
    }
}