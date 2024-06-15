using System.Globalization;

namespace site_report
{
    public class SiteReport
    {
        public string? siteName { get; set; }
        public string? sessions { get; set; }
        public string? activeUsers { get; set; }
        public string? newUsers { get; set; }
        public string? screenPageViews { get; set; }
        public string? screenPageViewsPerSession { get; set; }

        public string? averageEngagementTime { get; set; }

        public string? cpm;

        public string? revenue;

        public string? impressions;

        public string? revenuePer1kSession;

        public string? impressionsPerSession;

    }

    public class TotalReport
    {

        public decimal totalRevenue = 0;
        public decimal totalCPM = 0;
        public decimal totalImpressions = 0;
        public decimal totalSessions = 0;
        public decimal totalActiveUsers = 0;
        public decimal totalNewUsers = 0;
        public decimal totalScreenPageViews = 0;
        public decimal totalScreenPageViewsPerSession = 0;
        public decimal totalRevenuePer1kSession = 0;
        public decimal totalImpressionsPerSession = 0;

        public decimal averageCPM = 0;
        public decimal averageRevenuePer1kSession = 0;
        public decimal averageImpressionsPerSession = 0;
        public decimal averageScreenPageViewsPerSession = 0;

        public string formattedTotalRevenue => totalRevenue.ToString("C2");
        public string formattedTotalCPM => totalCPM.ToString("C2");
        public string formattedTotalImpressions => totalImpressions.ToString("N0");
        public string formattedTotalSessions => totalSessions.ToString("N0");
        public string formattedTotalActiveUsers => totalActiveUsers.ToString("N0");
        public string formattedTotalNewUsers => totalNewUsers.ToString("N0");
        public string formattedTotalScreenPageViews => totalScreenPageViews.ToString("N0");
        public string formattedTotalScreenPageViewsPerSession => totalScreenPageViewsPerSession.ToString("N0");
        public string formattedTotalRevenuePer1kSession => totalRevenuePer1kSession.ToString("C2");
        public string formattedTotalImpressionsPerSession => totalImpressionsPerSession.ToString("N0");

        public string formattedAverageCPM => averageCPM.ToString("C2");
        public string formattedAverageRevenuePer1kSession => averageRevenuePer1kSession.ToString("C2");
        public string formattedAverageImpressionsPerSession => averageImpressionsPerSession.ToString("N0");
        public string formattedAverageScreenPageViewsPerSession => averageScreenPageViewsPerSession.ToString("N0");

        public TotalReport(List<SiteReport> siteReports)
        {
            foreach (var site in siteReports)
            {
                totalRevenue += Decimal.Parse(site.revenue ?? "$0", NumberStyles.AllowCurrencySymbol | NumberStyles.Number);

                totalImpressions += Decimal.Parse(site.impressions ?? "0");

                totalCPM += Decimal.Parse(site.cpm ?? "0", NumberStyles.AllowCurrencySymbol | NumberStyles.Number);
                totalSessions += Decimal.Parse(site.sessions ?? "0");
                totalRevenuePer1kSession += Decimal.Parse(site.revenuePer1kSession ?? "0", NumberStyles.AllowCurrencySymbol | NumberStyles.Number);
                totalImpressionsPerSession += Decimal.Parse(site.impressionsPerSession ?? "0");
                totalNewUsers += Decimal.Parse(site.newUsers ?? "0");
                totalActiveUsers += Decimal.Parse(site.activeUsers ?? "0");
                totalScreenPageViews += Decimal.Parse(site.screenPageViews ?? "0");
                totalScreenPageViewsPerSession += Decimal.Parse(site.screenPageViewsPerSession ?? "0");
            }



            decimal numberOfSites = siteReports.Count;


            averageImpressionsPerSession = totalImpressionsPerSession / numberOfSites;
            averageRevenuePer1kSession = totalRevenuePer1kSession / numberOfSites;
            averageCPM = totalCPM / numberOfSites;
            averageScreenPageViewsPerSession = totalScreenPageViewsPerSession / numberOfSites;
        }

    }


}
