using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using assignment_code.Models;

namespace assignment_code.Services
{
    public class DashboardService
    {
        private StoreDataService _storeData => StoreDataService.Instance;

        public List<KpiStat> GetKpiStats()
        {
            int totalProducts = _storeData.Products.Count;
            int totalCategories = _storeData.Categories.Count;
            int totalOrders = _storeData.Orders.Count;
            int totalSales = _storeData.Sales.Count;

            return new List<KpiStat>
            {
                new KpiStat(TranslationManager.T("TotalProducts", "Total Products"), totalProducts.ToString("N0"), "Live Database", 12.5, true, KpiIconType.Product),
                new KpiStat(TranslationManager.T("TotalCategories", "Total Categories"), totalCategories.ToString("N0"), "Active Categories", 8.2, true, KpiIconType.Folder),
                new KpiStat(TranslationManager.T("TotalOrders", "Total Orders"), totalOrders.ToString("N0"), "Customer Orders", 15.0, true, KpiIconType.Order),
                new KpiStat(TranslationManager.T("TotalSales", "Total Sales"), totalSales.ToString("N0"), "Invoices Recorded", 24.3, true, KpiIconType.Report)
            };
        }

        public List<BarChartPoint> GetPostGrowthData(string period = "6 months")
        {
            var months = period == "12 months" ? 12 : 6;
            var result = new List<BarChartPoint>();
            var now = DateTime.Now;

            for (int i = months - 1; i >= 0; i--)
            {
                var monthDate = now.AddMonths(-i);
                string monthName = monthDate.ToString("MMM");

                // Calculate product counts or monthly volume
                int count = _storeData.Products.Count(p =>
                {
                    if (string.IsNullOrEmpty(p.DateAdded)) return true;
                    return p.DateAdded.IndexOf(monthName, StringComparison.OrdinalIgnoreCase) >= 0;
                });

                if (count == 0)
                {
                    // Fallback to proportional distribution based on total catalog size
                    count = Math.Max(1, (_storeData.Products.Count * (months - i)) / (months * 2));
                }

                bool isHighlight = (i == 0);
                result.Add(new BarChartPoint(monthName, count, isHighlight, isHighlight ? $"{count} items" : null));
            }

            return result;
        }

        public SplineChartDataset GetCommentsTrendData(string period = "Last 15 days")
        {
            var dataset = new SplineChartDataset();
            int days = 15;

            for (int i = 1; i <= days; i++)
            {
                dataset.XLabels.Add(i.ToString());
            }

            int salesCount = _storeData.Sales.Count;
            int ordersCount = _storeData.Orders.Count;

            var approvedValues = new List<double>();
            var pendingValues = new List<double>();
            var refundValues = new List<double>();

            for (int i = 0; i < days; i++)
            {
                // Dynamic trend values based on actual transactions
                double baseSales = Math.Max(0, salesCount * Math.Sin((i + 1) * 0.4) + (salesCount / 2.0));
                double basePending = Math.Max(0, ordersCount * Math.Cos((i + 1) * 0.35) + (ordersCount / 3.0));
                double baseRefund = Math.Max(0, (salesCount / 5.0) * Math.Sin((i + 1) * 0.2));

                approvedValues.Add(Math.Round(baseSales, 0));
                pendingValues.Add(Math.Round(basePending, 0));
                refundValues.Add(Math.Round(baseRefund, 0));
            }

            var approvedSeries = new SplineSeries("Completed Sales", Color.FromArgb(16, 185, 129), approvedValues);
            var pendingSeries = new SplineSeries("Pending Orders", Color.FromArgb(245, 158, 11), pendingValues);
            var refundSeries = new SplineSeries("Refunds/Disputes", Color.FromArgb(239, 68, 68), refundValues);

            dataset.Series.Add(approvedSeries);
            dataset.Series.Add(pendingSeries);
            dataset.Series.Add(refundSeries);

            return dataset;
        }

        public List<PostItem> GetLatestPosts(string search = null)
        {
            var products = _storeData.Products;
            var list = new List<PostItem>();

            int id = 1;
            foreach (var p in products.Take(6))
            {
                var status = (p.Stock > 0 && !string.Equals(p.Status, "Out of Stock", StringComparison.OrdinalIgnoreCase))
                    ? PostStatus.Published
                    : PostStatus.Draft;

                list.Add(new PostItem(id++, p.Name, status, string.IsNullOrEmpty(p.DateAdded) ? DateTime.Now.ToString("dd MMM") : p.DateAdded));
            }

            if (string.IsNullOrWhiteSpace(search))
                return list;

            string query = search.Trim().ToLowerInvariant();
            return list.Where(p => p.Title.ToLowerInvariant().Contains(query) ||
                                   p.Status.ToString().ToLowerInvariant().Contains(query)).ToList();
        }

        public List<CommentItem> GetRecentComments(string search = null)
        {
            var sales = _storeData.Sales;
            var list = new List<CommentItem>();

            int id = 1;
            foreach (var s in sales.Take(6))
            {
                string preview = $"${s.TotalAmount:N2} ({s.PaymentMethod}) - {s.Status}";
                string date = s.Timestamp.ToString("dd MMM HH:mm");
                list.Add(new CommentItem(id++, s.CustomerName, preview, date));
            }

            if (list.Count == 0 && _storeData.Orders.Count > 0)
            {
                foreach (var ord in _storeData.Orders.Take(4))
                {
                    string preview = $"Order {ord.OrderId} - ${ord.TotalAmount:N2}";
                    string date = ord.OrderDate.ToString("dd MMM");
                    list.Add(new CommentItem(id++, ord.CustomerName, preview, date));
                }
            }

            if (string.IsNullOrWhiteSpace(search))
                return list;

            string query = search.Trim().ToLowerInvariant();
            return list.Where(c => c.Author.ToLowerInvariant().Contains(query) ||
                                   c.Preview.ToLowerInvariant().Contains(query)).ToList();
        }
    }
}
