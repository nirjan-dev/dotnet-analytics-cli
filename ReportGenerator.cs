using site_report_analytics;
using site_report_venatus;

namespace site_report
{
    public class ReportGenerator
    {
        static public List<SiteReport> getSiteReports(List<VenatusResult> siteAdStats, List<TrafficStats> siteTrafficReports)
        {
            var siteReports = new List<SiteReport>();

            foreach (var site in siteAdStats)
            {
                var siteReport = new SiteReport();

                var siteTrafficStat = siteTrafficReports.FirstOrDefault(x => x.siteName == site.site_name);

                if (siteTrafficStat != null)
                {
                    siteReport.sessions = siteTrafficStat.sessions;
                    siteReport.activeUsers = siteTrafficStat.activeUsers;
                    siteReport.newUsers = siteTrafficStat.newUsers;
                    siteReport.screenPageViews = siteTrafficStat.screenPageViews;
                    siteReport.screenPageViewsPerSession = siteTrafficStat.screenPageViewsPerSession;
                    siteReport.averageEngagementTime = siteTrafficStat.averageEngagementTime;

                    if (siteReport.sessions != null)
                    {

                        siteReport.revenuePer1kSession = (site.revenue / Decimal.Parse(siteReport.sessions) * 1000).ToString("0.00");

                        siteReport.impressionsPerSession = (site.impressions / Decimal.Parse(siteReport.sessions)).ToString("0.00");
                    }
                }
                siteReport.siteName = site.site_name;
                siteReport.cpm = site.formattedCPM;
                siteReport.revenue = site.formattedRevenue;
                siteReport.impressions = site.formattedImpressions;

                siteReports.Add(siteReport);
            }

            return siteReports;
        }
    }
}