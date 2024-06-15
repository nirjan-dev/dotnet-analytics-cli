using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Csv;

namespace site_report_venatus
{

    public class FilterBy
    {
        public string? from { get; set; }
        public string? to { get; set; }

    }

    public class OrderBy
    {
        public string? revenue { get; set; }
    }


    public class VenatusRequest
    {
        public FilterBy? filter_by { get; set; }
        public string[]? group_by { get; set; }

        public OrderBy? order_by { get; set; }
    };

    public class VenatusLoginRequest
    {
        public string? username { get; set; }
        public string? password { get; set; }
    }

    public class VenatusResult
    {
        public decimal cpm { get; set; }
        public decimal revenue { get; set; }
        public int impressions { get; set; }
        public string? site_id { get; set; }

        public string? placement_id { get; set; }

        public string? placement_name { get; set; }

        public string? formattedCPM => (revenue / impressions * 1000).ToString("C2");

        public string? formattedRevenue => revenue.ToString("C2");

        public string? formattedImpressions => impressions.ToString("N0");

        public string site_name
        {
            get
            {
                if (site_id == "5e78f8996a9be55c3ef22469")
                {
                    return "RS";
                }
                if (site_id == "5e78f4d86a9be55c3ef22466")
                {
                    return "GFES";
                }
                if (site_id == "5ff5823d1a35210945f44016")
                {
                    return "ES";
                }
                if (site_id == "5f364b5d72e701565245a610")
                {
                    return "RG";
                }
                if (site_id == "5e78f4ea1c957813c16edea9")
                {
                    return "SO";
                }
                if (site_id == "605a061ae8fdeb39324f4bed")
                {
                    return "MTG";
                }


                return "unknown";
            }
            set { }
        }
    }

    public class VenatusResponse
    {
        public List<VenatusResult>? result { get; set; }

        public List<VenatusResult> getGDMResult()
        {
            var gdmResult = new List<VenatusResult>();
            if (result != null)
            {
                foreach (VenatusResult item in result)
                {
                    if (item.site_name == "RS" || item.site_name == "GFES" || item.site_name == "ES" || item.site_name == "RG" || item.site_name == "SO" || item.site_name == "MTG")
                    {
                        gdmResult.Add(item);
                    }
                }
            }


            return gdmResult.ToList();
        }


    }


    public class VenatusService
    {
        private Dictionary<string, string> placementDictES = new Dictionary<string, string>();
        private Dictionary<string, string> placementDictGFES = new Dictionary<string, string>();

        private Dictionary<string, string> placementDictRS = new Dictionary<string, string>();

        private Dictionary<string, string> placementDictMTG = new Dictionary<string, string>();

        private Dictionary<string, string> placementDictRG = new Dictionary<string, string>();

        private Dictionary<string, string> placementDictSO = new Dictionary<string, string>();


        private bool isLoggedIn = false;


        static HttpClient httpClient = new HttpClient();


        public VenatusService()
        {
            placementDictES = getPlacementDictionary("ES");
            placementDictGFES = getPlacementDictionary("GFES");
            placementDictRS = getPlacementDictionary("RS");
            placementDictMTG = getPlacementDictionary("MTG");
            placementDictRG = getPlacementDictionary("RG");
            placementDictSO = getPlacementDictionary("SO");
        }

        private Dictionary<string, string> getPlacementDictionary(string siteName)
        {
            var csv = File.ReadAllText($"./csv/{siteName}.csv");

            var placementDictionary = new Dictionary<string, string>();

            foreach (var line in CsvReader.ReadFromText(csv))
            {
                var placementId = line["placementid"];
                var placementName = line["placement"];

                placementDictionary.Add(placementId, placementName);
            }

            return placementDictionary;
        }


        // TODO: this won't actually work because the venatus account doesn't exist anymore
        public async Task loginToVenatus()
        {
            var loginEndpoint = "https://api.venatusmedia.com/login";
            var loginRequest = new VenatusLoginRequest
            {
                username = "nirjan.khadka@gfinity.net",
                password = "o4VLpUf9Tl"
            };
            var loginRequestJson = JsonSerializer.Serialize(loginRequest);

            var loginRequestMessage = new StringContent(loginRequestJson, Encoding.UTF8, "application/json");

            var loginResponse = await httpClient.PostAsync(loginEndpoint, loginRequestMessage);
            var loginResponseJson = await loginResponse.Content.ReadAsStringAsync();

            var userIDCookie = loginResponse.Headers.GetValues("set-cookie").FirstOrDefault();

            httpClient.DefaultRequestHeaders.Add("Cookie", userIDCookie);

            isLoggedIn = true;

        }

        private string getTwoDaysAgo()
        {
            var twoDaysAgo = DateTime.Now.AddDays(-2);
            return $"{twoDaysAgo.ToString("yyyy-MM-dd")} 00:00:00";
        }

        private string getYesterday()
        {
            var yesterday = DateTime.Now.AddDays(-1);
            return $"{yesterday.ToString("yyyy-MM-dd")} 00:00:00";
        }

        public async Task<List<VenatusResult>> getSiteAdStatsAsync()
        {
            if (!isLoggedIn)
            {
                await loginToVenatus();
            }

            var venatusEndpoint = new Uri("https://api.venatusmedia.com/user/reporting/ui/5e78f4616a9be55c3ef22464");
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, venatusEndpoint);

            var venatusRequest = new VenatusRequest
            {
                filter_by = new FilterBy
                {
                    from = getTwoDaysAgo(),
                    to = getYesterday()
                },
                group_by = new string[] { "site_id" },
                order_by = new OrderBy
                {
                    revenue = "DESC"
                }
            };
            var venatusRequestJSON = JsonSerializer.Serialize(venatusRequest);

            httpRequestMessage.Content = new StringContent(venatusRequestJSON, Encoding.UTF8, "application/json");



            var responseMessage = await httpClient.SendAsync(httpRequestMessage);
            var venatusResponse = JsonSerializer.Deserialize<VenatusResponse>(await responseMessage.Content.ReadAsStringAsync());


            if (venatusResponse == null || venatusResponse.result == null)
            {
                return new List<VenatusResult>();
            }
            else
            {
                return venatusResponse.getGDMResult();
            }


        }


        public async Task<List<VenatusResult>> getSiteAdStatsWithPlacementsAsync()
        {
            if (!isLoggedIn)
            {
                await loginToVenatus();
            }

            var venatusEndpoint = new Uri("https://api.venatusmedia.com/user/reporting/ui/5e78f4616a9be55c3ef22464");
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, venatusEndpoint);

            var venatusRequest = new VenatusRequest
            {
                filter_by = new FilterBy
                {
                    from = getTwoDaysAgo(),
                    to = getYesterday()
                },
                group_by = new string[] { "site_id", "placement_id" },
                order_by = new OrderBy
                {
                    revenue = "DESC"
                }
            };
            var venatusRequestJSON = JsonSerializer.Serialize(venatusRequest);

            httpRequestMessage.Content = new StringContent(venatusRequestJSON, Encoding.UTF8, "application/json");



            var responseMessage = await httpClient.SendAsync(httpRequestMessage);
            var venatusResponse = JsonSerializer.Deserialize<VenatusResponse>(await responseMessage.Content.ReadAsStringAsync());


            var placementStats = new List<VenatusResult>();

            if (venatusResponse == null || venatusResponse.result == null)
            {
                placementStats = new List<VenatusResult>();
            }
            else
            {
                placementStats = venatusResponse.getGDMResult();
            }

            foreach (var placementStatsItem in placementStats)
            {
                var siteName = placementStatsItem.site_name;
                var placementId = placementStatsItem.placement_id ?? "";

                if (siteName == "ES")
                {
                    try { placementStatsItem.placement_name = placementDictES[placementId]; }
                    catch
                    {
                        placementStatsItem.placement_name = "Unknown";
                    }

                }
                else if (siteName == "GFES")
                {
                    try
                    {
                        placementStatsItem.placement_name = placementDictGFES[placementId];
                    }
                    catch (System.Exception)
                    {
                        placementStatsItem.placement_name = "unknown";
                    }

                }
                else if (siteName == "RS")
                {
                    try
                    {
                        placementStatsItem.placement_name = placementDictRS[placementId];
                    }
                    catch (System.Exception)
                    {
                        placementStatsItem.placement_name = "unknown";
                    }
                }
                else if (siteName == "MTG")
                {
                    try
                    {
                        placementStatsItem.placement_name = placementDictMTG[placementId];
                    }
                    catch (System.Exception)
                    {
                        placementStatsItem.placement_name = "unknown";
                    }
                }
                else if (siteName == "RG")
                {
                    try
                    {
                        placementStatsItem.placement_name = placementDictRG[placementId];
                    }
                    catch (System.Exception)
                    {
                        placementStatsItem.placement_name = "unknown";
                    }

                }
                else if (siteName == "SO")
                {
                    try { placementStatsItem.placement_name = placementDictSO[placementId]; }
                    catch
                    {
                        placementStatsItem.placement_name = "Unknown";
                    }
                }
            }


            return placementStats;


        }

    }
}




