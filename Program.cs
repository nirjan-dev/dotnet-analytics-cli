using System.Net;
using System.Net.Http;
using Humanizer;
using System.Text.Json;
// using ProgramTypes;
using System.Text;
using site_report_analytics;
using Spectre.Console;
using site_report_venatus;
using site_report;
using System.Globalization;


AnsiConsole.Markup("[underline bold red]Welcome to the analytics CLI\n\n[/]");
var reportDate = DateTime.Now.AddDays(-2).ToLongDateString();
AnsiConsole.Markup($"Report for: [underline bold purple]{reportDate}\n\n[/]");



var ga_analytics = new GA();
var venatusService = new VenatusService();


var siteTrafficReports = await ga_analytics.getAllTrafficMetrics();
var siteGeoReports = await ga_analytics.getAllGeoTrafficMetrics();
var siteSourceMediumReports = await ga_analytics.getAllSourceMediumTrafficMetrics();

var siteAdStats = await venatusService.getSiteAdStatsAsync();
var siteAdStatsWithPlacements = await venatusService.getSiteAdStatsWithPlacementsAsync();




var siteAdAStatsWithPlacementsBySite = siteAdStatsWithPlacements.GroupBy(x => x.site_name);


var siteReports = ReportGenerator.getSiteReports(siteAdStats, siteTrafficReports);
var totalReport = new TotalReport(siteReports);







AnsiConsole.Write(new Rule("[red]GEO Breakdown[/]"));


foreach (var report in siteGeoReports)
{
    Console.WriteLine(report.siteName);

    var breakdownChart = new BreakdownChart().Width(80);

    for (int i = 0; i < report.countries!.Length; i++)
    {
        breakdownChart.AddItem(report.countries[i], Double.Parse(report.sessions![i]), Color.FromInt32(i + 5));
    }
    AnsiConsole.Write(breakdownChart);
}

AnsiConsole.Write(new Rule("[red]Source / Medium Breakdown[/]"));


foreach (var report in siteSourceMediumReports)
{
    Console.WriteLine(report.siteName);

    var breakdownChart = new BreakdownChart().Width(80);

    for (int i = 0; i < report.sourceMedium!.Length; i++)
    {
        breakdownChart.AddItem(report.sourceMedium[i], Double.Parse(report.sessions![i]), Color.FromInt32(i + 5));
    }
    AnsiConsole.Write(breakdownChart);
}


var primaryTable = new Table();
primaryTable.AddColumns("Site Name", "rev", "impressions", "CPM", "sessions",
"rev per 1k ses", "imp per ses", "engagement time");



foreach (var site in siteReports)
{
    primaryTable.AddRow(site.siteName ?? "", site.revenue ?? "", site.impressions ?? "", site.cpm ?? "", site.sessions ?? "", site.revenuePer1kSession ?? "", site.impressionsPerSession ?? "", site.averageEngagementTime ?? "");
}

primaryTable.AddEmptyRow();

primaryTable.AddRow("Total", totalReport.formattedTotalRevenue, totalReport.formattedTotalImpressions, totalReport.formattedAverageCPM, totalReport.formattedTotalSessions, totalReport.formattedAverageRevenuePer1kSession, totalReport.formattedAverageImpressionsPerSession);

primaryTable.BorderColor(Color.Purple);
AnsiConsole.Write(new Rule("[red]Ads Report[/]"));

AnsiConsole.Write(primaryTable);

var secondaryTable = new Table();
secondaryTable.AddColumns("Site Name", "Active Users", "New Users", "Screen Page Views", "Screen Page Views Per Session");

foreach (var site in siteReports)
{
    secondaryTable.AddRow(site.siteName ?? "", site.activeUsers ?? "", site.newUsers ?? "", site.screenPageViews ?? "", site.screenPageViewsPerSession ?? "");
}

secondaryTable.AddEmptyRow();


secondaryTable.AddRow("Total", totalReport.formattedTotalActiveUsers, totalReport.formattedTotalNewUsers, totalReport.formattedTotalScreenPageViews, totalReport.formattedAverageScreenPageViewsPerSession);

secondaryTable.BorderColor(Color.Green);
AnsiConsole.Write(new Rule("[red]Traffic Report[/]"));

AnsiConsole.Write(secondaryTable);


AnsiConsole.Write(new Rule("[red]Ad placement Report[/]"));

foreach (var site in siteAdAStatsWithPlacementsBySite)
{
    var table = new Table();
    table.AddColumns("Site Name", "Placement", "rev", "impressions", "CPM");
    foreach (var placement in site)
    {
        table.AddRow(placement.site_name, placement.placement_name?.Replace('[', ' ').Replace(']', ' ') ?? "placement name", placement.formattedRevenue ?? "", placement.formattedImpressions ?? "", placement.formattedCPM ?? "");

    }
    table.BorderColor(Color.Yellow);
    AnsiConsole.Write(table);
}








