using Gameloop.Vdf.Linq;
using Gameloop.Vdf;
using SteamVent.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using AngleSharp.Html.Parser;

namespace SteamVent.Web
{
    public class SteamWorkshop
    {
        public static async IAsyncEnumerable<double?> WorkshopStatusFromWebUpdateOnlyAsync(string LibraryPath, UInt32 AppId, DateTime LatestUpdate, SemaphoreSlim DictionaryLock, Dictionary<UInt64, WorkshopItemStatus> WorkshopItems, Dictionary<UInt64, SemaphoreSlim> WorkshopItemLocks)
        {
            int ProgressCounter = 0;

            HttpClient client = new HttpClient();
            HtmlParser parser = new HtmlParser();
            List<(UInt64 workshopId, string workshopTitle, string workshopImage)> HtmlWorkshopItems = new List<(UInt64, string, string)>();
            for (int page = 1; ; page++)
            {
                string workshopUrl = @$"https://steamcommunity.com/workshop/browse/?appid={AppId}&browsesort=lastupdated&section=readytouseitems&updated_date_range_filter_start={((DateTimeOffset)LatestUpdate).ToUnixTimeSeconds() - 1}&actualsort=lastupdated&p={page}";
                var response = await client.GetAsync(workshopUrl);
                string html = await response.Content.ReadAsStringAsync();
                //byte[] bytes = await response.Content.ReadAsByteArrayAsync(); // this might fix 712270362, odd we can't just trust the headers, unless we can?
                //string html = Encoding.UTF8.GetString(bytes);
                if (!html.Contains(@"No items matching your search criteria were found."))
                {
                    var document = parser.ParseDocument(html);
                    foreach (var workshopItem in document.QuerySelectorAll(".workshopItem"))
                    {
                        var link = workshopItem.QuerySelector("a.ugc");
                        UInt64 workshopId = UInt64.Parse(link.GetAttribute("data-publishedfileid"));

                        string workshopTitle = workshopItem.QuerySelector(".workshopItemTitle")?.TextContent?.Trim();

                        string workshopImage = workshopItem.QuerySelector(".workshopItemPreviewImage")?.Attributes["src"]?.Value;
                        if (!string.IsNullOrWhiteSpace(workshopImage) && workshopImage.Contains("?"))
                            workshopImage = workshopImage.Substring(0, workshopImage.IndexOf("?"));

                        HtmlWorkshopItems.Add((workshopId, workshopTitle, workshopImage));
                    }
                    var pages = document.QuerySelectorAll(".workshopBrowsePaging .pagebtn");
                    if (pages.Length < 2 || pages[1].ClassList.Contains("disabled"))
                        break;

                    ProgressCounter++;
                    yield return 1d - (1d / (ProgressCounter + 1));
                }
                else
                {
                    break;
                }
            }
            foreach (var workshopData in HtmlWorkshopItems)
            {
                if (!WorkshopItems.ContainsKey(workshopData.workshopId))
                    continue;
                WorkshopItemStatus thisItem = WorkshopItems[workshopData.workshopId];
                thisItem.Status = "updated required";
                thisItem.HasUpdate = true;
                thisItem.Detection |= WorkshopItemStatus.WorkshopDetectionType.HtmlList;
                thisItem.Title = workshopData.workshopTitle;
                thisItem.Image = workshopData.workshopImage;
            }

            yield return 1d;
        }
    }
}
