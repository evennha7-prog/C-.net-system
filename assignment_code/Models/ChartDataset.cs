using System;
using System.Collections.Generic;
using System.Drawing;

namespace assignment_code.Models
{
    public class BarChartPoint
    {
        public string Label { get; set; }
        public double Value { get; set; }
        public bool IsHighlighted { get; set; }
        public string TooltipText { get; set; }

        public BarChartPoint(string label, double value, bool isHighlighted = false, string tooltipText = null)
        {
            Label = label;
            Value = value;
            IsHighlighted = isHighlighted;
            TooltipText = tooltipText;
        }
    }

    public class SplineSeries
    {
        public string Name { get; set; }
        public Color LineColor { get; set; }
        public List<double> Values { get; set; }

        public SplineSeries(string name, Color lineColor, List<double> values)
        {
            Name = name;
            LineColor = lineColor;
            Values = values ?? new List<double>();
        }
    }

    public class SplineChartDataset
    {
        public List<string> XLabels { get; set; }
        public List<SplineSeries> Series { get; set; }

        public SplineChartDataset()
        {
            XLabels = new List<string>();
            Series = new List<SplineSeries>();
        }
    }
}
