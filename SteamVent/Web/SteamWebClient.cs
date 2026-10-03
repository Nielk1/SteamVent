using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SteamVent.Web
{
    /// <summary>
    /// A small, dependency-free client for the Steam Web API (and proxies that mimic it). The raw response
    /// is always returned alongside the parsed shape so custom proxies that add extra fields are not lost.
    /// Supports a configurable base URL and an optional (omittable) API key for no-key proxies.
    /// </summary>
    public sealed class SteamWebClient : IDisposable
    {
        public SteamWebClientOptions Options { get; }
        public HttpClient Http { get; }
        private readonly bool _ownsHttp;

        public SteamWebClient(SteamWebClientOptions? options = null, HttpClient? http = null)
        {
            Options = options ?? new SteamWebClientOptions();
            if (http != null)
            {
                Http = http;
                _ownsHttp = false;
            }
            else
            {
                Http = new HttpClient { Timeout = Options.Timeout };
                if (!string.IsNullOrEmpty(Options.UserAgent))
                    Http.DefaultRequestHeaders.UserAgent.ParseAdd(Options.UserAgent);
                _ownsHttp = true;
            }
        }

        /// <summary>Builds the full request URL: base + path + <c>input_json</c> + optional <c>key</c>.</summary>
        public string BuildUrl(string path, object? parameters = null)
        {
            var prefix = Options.BaseUrl;
            if (prefix == null ||
                (prefix.Scheme != Uri.UriSchemeHttp && prefix.Scheme != Uri.UriSchemeHttps) ||
                !string.IsNullOrEmpty(prefix.Query) || !string.IsNullOrEmpty(prefix.Fragment))
                throw new InvalidOperationException("BaseUrl must be an absolute http(s) URL to the API root.");

            string? query = parameters == null ? null : "?input_json=" + Uri.EscapeDataString(JsonSerializer.Serialize(parameters));
            if (!string.IsNullOrEmpty(Options.ApiKey))
                query += (query == null ? "?" : "&") + "key=" + Uri.EscapeDataString(Options.ApiKey);

            return prefix.AbsoluteUri.TrimEnd('/') + "/" + path.TrimStart('/') + (query ?? "");
        }

        /// <summary>Performs the request and returns the raw response body. Non-2xx throws <see cref="SteamWebApiException"/>.</summary>
        public async Task<byte[]> FetchRawAsync(string path, object? parameters = null, CancellationToken cancellationToken = default)
        {
            using var response = await Http.GetAsync(BuildUrl(path, parameters), cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                string? body = null;
                try
                {
                    using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                    using var reader = new StreamReader(stream, Encoding.UTF8, false, 8192, leaveOpen: true);
                    var buffer = new char[8192];
                    var count = reader.Read(buffer, 0, buffer.Length);
                    body = new string(buffer, 0, count);
                }
                catch (OperationCanceledException) { throw; }
                catch { /* a missing body is fine; the status code still drives handling */ }
                throw new SteamWebApiException(response.StatusCode, body);
            }
            return await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Performs the request and returns the raw + parsed response.</summary>
        public async Task<SteamWebResponse> GetJsonAsync(string path, object? parameters = null, CancellationToken cancellationToken = default)
        {
            var bytes = await FetchRawAsync(path, parameters, cancellationToken).ConfigureAwait(false);
            var json = Encoding.UTF8.GetString(bytes);
            JsonElement? response = null;
            var items = new List<JsonElement>();
            bool success = false;
            try
            {
                using var document = JsonDocument.Parse(bytes);
                if (document.RootElement.TryGetProperty("response", out var r) && r.ValueKind == JsonValueKind.Object)
                {
                    response = r.Clone();
                    string? result = SteamWebResponse.Text(r, "result");
                    success = result == null || result == "1";
                    if (r.TryGetProperty("publishedfiledetails", out var pfd) && pfd.ValueKind == JsonValueKind.Array)
                        foreach (var item in pfd.EnumerateArray()) items.Add(item.Clone());
                }
            }
            catch { /* keep the raw payload even if it cannot be parsed */ }
            return new SteamWebResponse(bytes, json, success, response, items);
        }

        /// <summary>Workshop search (QueryFiles), most-recently-updated, tag-ORed.</summary>
        public Task<SteamWebResponse> QueryFilesAsync(int appid, string[]? tags = null, int numPerPage = 50, string cursor = "*", CancellationToken cancellationToken = default)
        {
            var request = new Dictionary<string, object>
            {
                ["query_type"] = 21,
                ["page"] = 1,
                ["cursor"] = cursor,
                ["numperpage"] = numPerPage,
                ["appid"] = appid,
                ["filetype"] = 0,
            };
            if (tags != null && tags.Length > 0)
            {
                request["requiredtags"] = tags;
                request["match_all_tags"] = false;
            }
            return GetJsonAsync("IPublishedFileService/QueryFiles/v1/", request, cancellationToken);
        }

        /// <summary>Workshop item details (GetDetails), including immediate children.</summary>
        public Task<SteamWebResponse> GetDetailsAsync(string publishedFileId, int appid = 0, CancellationToken cancellationToken = default)
        {
            var request = new Dictionary<string, object>
            {
                ["publishedfileids"] = new[] { publishedFileId },
                ["includechildren"] = true,
            };
            if (appid > 0) request["appid"] = appid;
            return GetJsonAsync("IPublishedFileService/GetDetails/v1/", request, cancellationToken);
        }

        public void Dispose()
        {
            if (_ownsHttp) Http.Dispose();
        }
    }
}