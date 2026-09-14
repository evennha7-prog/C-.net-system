using System;

namespace assignment_code.Models
{
    public enum KpiIconType
    {
        Document,
        Folder,
        Media,
        Comments,
        Product,
        Order,
        Customer,
        Report
    }

    public class KpiStat
    {
        public string Title { get; set; }
        public string Value { get; set; }
        public string Subtitle { get; set; }
        public double PercentageChange { get; set; }
        public bool IsPositive { get; set; }
        public KpiIconType IconType { get; set; }

        public KpiStat() { }

        public KpiStat(string title, string value, string subtitle, double percentageChange, bool isPositive, KpiIconType iconType)
        {
            Title = title;
            Value = value;
            Subtitle = subtitle;
            PercentageChange = percentageChange;
            IsPositive = isPositive;
            IconType = iconType;
        }
    }
}
