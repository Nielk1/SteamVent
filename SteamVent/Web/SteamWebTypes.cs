using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace SteamVent.Web
{
    /// <summary>
    /// Configuration for the Steam Web API client. Supports both the official Steam endpoints (set
    /// <see cref="ApiKey"/>) and custom proxies that mimic the Steam Web API (set <see cref="BaseUrl"/>,
    /// leave <see cref="ApiKey"/> empty). Proxies may extend responses with extra fields, which is why the
    /// raw payload is always retained by callers.
    /// </summary>
    public sealed class SteamWebClientOptions
    {
        /// <summary>Absolute URL of the API root, e.g. https://api.steampowered.com/ or a proxy.</summary>
        public Uri BaseUrl { get; init; } = new Uri("https://api.steampowered.com/");
        /// <summary>Steam Web API key. Leave null/empty when talking to a keyless proxy.</summary>
        public string? ApiKey { get; init; } = null;
        public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
        public string? UserAgent { get; init; } = null;
    }

    /// <summary>
    /// A non-2xx answer from the Steam Web API. Carries the (small) error body so a definitive
    /// "not found / access restricted" can be told apart from a transient failure.
    /// </summary>
    public sealed class SteamWebApiException : HttpRequestException
    {
        public HttpStatusCode StatusCode { get; }
        public string? ErrorBody { get; }

        public SteamWebApiException(HttpStatusCode statusCode, string? errorBody)
            : base(HttpRequestError.HttpProtocolError, "The Steam Web API returned " + (int)statusCode + ".", null, statusCode)
        {
            StatusCode = statusCode;
            ErrorBody = errorBody;
        }

        /// <summary>True when the error body is <c>{ "error": expected, ... }</c>.</summary>
        public bool HasErrorCode(string expected)
        {
            if (string.IsNullOrWhiteSpace(ErrorBody)) return false;
            try
            {
                using var document = JsonDocument.Parse(ErrorBody);
                var root = document.RootElement;
                if (root.ValueKind != JsonValueKind.Object) return false;
                return root.TryGetProperty("error", out var error) &&
                       error.ValueKind == JsonValueKind.String &&
                       string.Equals(error.GetString(), expected, StringComparison.Ordinal);
            }
            catch { return false; }
        }
    }

    /// <summary>
    /// A parsed Steam Web API response. The raw payload is always kept (<see cref="RawJson"/>) because a
    /// custom proxy mimicking the Steam Web API may add fields beyond the documented ones; callers can
    /// read those from <see cref="RawJson"/> or the parsed <see cref="Response"/> element.
    /// </summary>
    public sealed class SteamWebResponse
    {
        public byte[] RawBytes { get; }
        public string RawJson { get; }
        public bool IsSuccess { get; }
        public JsonElement? Response { get; }
        public IReadOnlyList<JsonElement> Items { get; }

        public SteamWebResponse(byte[] rawBytes, string rawJson, bool isSuccess, JsonElement? response, IReadOnlyList<JsonElement> items)
        {
            RawBytes = rawBytes;
            RawJson = rawJson;
            IsSuccess = isSuccess;
            Response = response;
            Items = items;
        }

        /// <summary>Reads a string-or-number field as text (matches the Workshop "result"/"publishedfileid" shape).</summary>
        public static string? Text(JsonElement element, string name)
        {
            if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(name, out var value)) return null;
            return value.ValueKind switch
            {
                JsonValueKind.String => value.GetString(),
                JsonValueKind.Number => value.GetRawText(),
                _ => null
            };
        }
    }
}