using System.Globalization;
using Google.Analytics.Data.V1Beta;
using Google.Apis.Auth.OAuth2;
using Humanizer;

namespace site_report_analytics
{
    public class TrafficStats
    {
        public string? date { get; set; }
        public string? sessions { get; set; }
        public string? activeUsers { get; set; }
        public string? newUsers { get; set; }
        public string? screenPageViews { get; set; }
        public string? screenPageViewsPerSession { get; set; }

        public string? averageEngagementTime => $"{(decimal.Parse(userEngagementDuration!) / decimal.Parse(sessions!)).ToString("N0")} seconds";

        public string? userEngagementDuration { get; set; }

        public string? siteName { get; set; }
    }

    public class GeoTrafficStats
    {
        public string[]? countries { get; set; }
        public string[]? sessions { get; set; }

        public string? siteName { get; set; }
    }

    public class SourceMediumTrafficStats
    {
        public string[]? sourceMedium { get; set; }
        public string[]? sessions { get; set; }

        public string? siteName { get; set; }
    }


    public class GA
    {

        private static string GFES_ID = "313678913";
        private static string RS_ID = "313668448";
        private static string ES_ID = "313662432";

        private static string SO_ID = "313668449";

        private static string RG_ID = "312419605";

        private static string MTG_ID = "313657407";



        private static BetaAnalyticsDataClient client = BetaAnalyticsDataClient.Create();

        public GA()
        {
            // var credential = GoogleCredential.GetApplicationDefault();
            // TODO: update hard coded credential path
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "/home/nk13/GA-creds.json");
            Console.WriteLine($"Credential: {Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS")}");

        }

        public string getFormattedDate(string gaDateValue)
        {
            string formattedDate;

            var year = gaDateValue.Substring(0, 4);

            var month = gaDateValue.Substring(4, 2);

            var day = gaDateValue.Substring(6, 2);

            var dateObj = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));

            formattedDate = dateObj.ToOrdinalWords();

            return formattedDate;
        }

        public string getFormattedNumber(string gaNumberValue)
        {
            var formattedNumber = int.Parse(gaNumberValue).ToString("N", new CultureInfo("en-US"));
            return formattedNumber;
        }

        public string getFormattedNumberFromDoubleString(string gaNumberValue)
        {
            var formattedNumber = Double.Parse(gaNumberValue).ToString("N", new CultureInfo("en-US"));
            return formattedNumber;
        }

        public string getFormattedMinutesFromSeconds(string gaNumberValue)
        {
            var formattedNumber = Double.Parse(gaNumberValue).ToString("N", new CultureInfo("en-US"));

            var minutes = Double.Parse(formattedNumber) / 60;

            var formattedMinutes = minutes.ToString("N", new CultureInfo("en-US"));

            return formattedMinutes;
        }

        private async Task<TrafficStats> getTrafficMetrics(string propertyId, string siteName)
        {

            // Initialize request argument(s)
            RunReportRequest request = new RunReportRequest
            {
                Property = "properties/" + propertyId,
                Dimensions = { new Dimension { Name = "date" } },
                Metrics = { new Metric { Name = "sessions" }, new Metric { Name = "activeUsers" }, new Metric { Name = "newUsers" }, new Metric { Name = "screenPageViews" }, new Metric { Name = "screenPageViewsPerSession" }, new Metric { Name = "userEngagementDuration" } },
                DateRanges = { new DateRange { StartDate = "2daysAgo", EndDate = "2daysAgo" }, },
                Limit = 10,
            };

            // Make the request
            var response = await client.RunReportAsync(request);

            var gfesReport = new TrafficStats
            {
                date = getFormattedDate(response.Rows[0].DimensionValues[0].Value),
                sessions = getFormattedNumber(response.Rows[0].MetricValues[0].Value),
                activeUsers = getFormattedNumber(response.Rows[0].MetricValues[1].Value),
                newUsers = getFormattedNumber(response.Rows[0].MetricValues[2].Value),
                screenPageViews = getFormattedNumber(response.Rows[0].MetricValues[3].Value),
                screenPageViewsPerSession = getFormattedNumberFromDoubleString(response.Rows[0].MetricValues[4].Value),
                userEngagementDuration = getFormattedNumberFromDoubleString(response.Rows[0].MetricValues[5].Value),
                siteName = siteName
            };





            return gfesReport;
        }


        private async Task<GeoTrafficStats> getGEOTrafficMetrics(string propertyId, string siteName)
        {

            // Initialize request argument(s)
            RunReportRequest request = new RunReportRequest
            {
                Property = "properties/" + propertyId,
                Dimensions = { new Dimension { Name = "country" } },
                Metrics = { new Metric { Name = "sessions" } },
                DateRanges = { new DateRange { StartDate = "2daysAgo", EndDate = "2daysAgo" }, },
                Limit = 10,
            };

            // Make the request
            var response = await client.RunReportAsync(request);

            var report = new GeoTrafficStats
            {
                countries = response.Rows.Select(x => x.DimensionValues[0].Value).ToArray(),
                sessions = response.Rows.Select(x => getFormattedNumber(x.MetricValues[0].Value)).ToArray(),
                siteName = siteName
            };

            return report;
        }

        private async Task<SourceMediumTrafficStats> getSourceMediumTrafficMetrics(string propertyId, string siteName)
        {

            // Initialize request argument(s)
            RunReportRequest request = new RunReportRequest
            {
                Property = "properties/" + propertyId,
                Dimensions = { new Dimension { Name = "sessionSourceMedium" } },
                Metrics = { new Metric { Name = "sessions" } },
                DateRanges = { new DateRange { StartDate = "2daysAgo", EndDate = "2daysAgo" }, },
                Limit = 10,
            };

            // Make the request
            var response = await client.RunReportAsync(request);

            var report = new SourceMediumTrafficStats
            {
                sourceMedium = response.Rows.Select(x => x.DimensionValues[0].Value).ToArray(),
                sessions = response.Rows.Select(x => getFormattedNumber(x.MetricValues[0].Value)).ToArray(),
                siteName = siteName
            };

            return report;
        }


        public async Task<List<TrafficStats>> getAllTrafficMetrics()
        {
            var allTrafficMetrics = new List<TrafficStats>();
            var trafficMetricTasks = new List<Task<TrafficStats>>();


            trafficMetricTasks.Add(getTrafficMetrics(GFES_ID, "GFES"));
            trafficMetricTasks.Add(getTrafficMetrics(RS_ID, "RS"));
            trafficMetricTasks.Add(getTrafficMetrics(ES_ID, "ES"));
            trafficMetricTasks.Add(getTrafficMetrics(SO_ID, "SO"));
            trafficMetricTasks.Add(getTrafficMetrics(RG_ID, "RG"));
            trafficMetricTasks.Add(getTrafficMetrics(MTG_ID, "MTG"));

            var tasks = await Task.WhenAll(trafficMetricTasks);


            allTrafficMetrics.Add(tasks[0]);
            allTrafficMetrics.Add(tasks[1]);
            allTrafficMetrics.Add(tasks[2]);
            allTrafficMetrics.Add(tasks[3]);
            allTrafficMetrics.Add(tasks[4]);
            allTrafficMetrics.Add(tasks[5]);

            return allTrafficMetrics;
        }

        public async Task<List<GeoTrafficStats>> getAllGeoTrafficMetrics()
        {
            var allGeoTrafficMetrics = new List<GeoTrafficStats>();
            var geoTrafficMetricTasks = new List<Task<GeoTrafficStats>>();


            geoTrafficMetricTasks.Add(getGEOTrafficMetrics(GFES_ID, "GFES"));
            geoTrafficMetricTasks.Add(getGEOTrafficMetrics(RS_ID, "RS"));
            geoTrafficMetricTasks.Add(getGEOTrafficMetrics(ES_ID, "ES"));
            geoTrafficMetricTasks.Add(getGEOTrafficMetrics(SO_ID, "SO"));
            geoTrafficMetricTasks.Add(getGEOTrafficMetrics(RG_ID, "RG"));
            geoTrafficMetricTasks.Add(getGEOTrafficMetrics(MTG_ID, "MTG"));

            var tasks = await Task.WhenAll(geoTrafficMetricTasks);


            allGeoTrafficMetrics.Add(tasks[0]);
            allGeoTrafficMetrics.Add(tasks[1]);
            allGeoTrafficMetrics.Add(tasks[2]);
            allGeoTrafficMetrics.Add(tasks[3]);
            allGeoTrafficMetrics.Add(tasks[4]);
            allGeoTrafficMetrics.Add(tasks[5]);

            return allGeoTrafficMetrics;
        }


        public async Task<List<SourceMediumTrafficStats>> getAllSourceMediumTrafficMetrics()
        {
            var allSourceMediumTrafficMetrics = new List<SourceMediumTrafficStats>();
            var sourceMediumTrafficMetricTasks = new List<Task<SourceMediumTrafficStats>>();


            sourceMediumTrafficMetricTasks.Add(getSourceMediumTrafficMetrics(GFES_ID, "GFES"));
            sourceMediumTrafficMetricTasks.Add(getSourceMediumTrafficMetrics(RS_ID, "RS"));
            sourceMediumTrafficMetricTasks.Add(getSourceMediumTrafficMetrics(ES_ID, "ES"));
            sourceMediumTrafficMetricTasks.Add(getSourceMediumTrafficMetrics(SO_ID, "SO"));
            sourceMediumTrafficMetricTasks.Add(getSourceMediumTrafficMetrics(RG_ID, "RG"));
            sourceMediumTrafficMetricTasks.Add(getSourceMediumTrafficMetrics(MTG_ID, "MTG"));

            var tasks = await Task.WhenAll(sourceMediumTrafficMetricTasks);


            allSourceMediumTrafficMetrics.Add(tasks[0]);
            allSourceMediumTrafficMetrics.Add(tasks[1]);
            allSourceMediumTrafficMetrics.Add(tasks[2]);
            allSourceMediumTrafficMetrics.Add(tasks[3]);
            allSourceMediumTrafficMetrics.Add(tasks[4]);
            allSourceMediumTrafficMetrics.Add(tasks[5]);

            return allSourceMediumTrafficMetrics;
        }
    }





}