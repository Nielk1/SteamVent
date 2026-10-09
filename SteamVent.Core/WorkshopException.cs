using System;
using System.Collections.Generic;

namespace SteamVent.Core;

/// <summary>
/// A unified, backend-agnostic exception for workshop operations (subscribe / download / unsubscribe, workshop
/// reads, and the AppID-context checks around them). Callers catch this single type no matter which backend
/// answered: the in-process Steamworks layer surfaces it directly, and the Bridge wraps its rich
/// <c>SteamVent.Bridge.BridgeException</c> in it (as <see cref="InnerException"/>) so the caller can drop down to
/// the bridge diagnostics when it wants them. When the failure is a contract violation - most importantly an AppID
/// mismatch, where the operation was requested for an AppID Steam is not actually running under - there is no
/// backend exception to wrap, so this exception stands in for one.
/// </summary>
public class WorkshopException : Exception
{
    public WorkshopException(string message)
        : base(message)
    {
    }

    public WorkshopException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>The Steam AppID the operation was requested for, if known.</summary>
    public UInt32? AppId { get; init; }

    /// <summary>The workshop item the operation was for, if known.</summary>
    public ulong? PublishedFileId { get; init; }

    /// <summary>The operation that failed (e.g. <c>"workshop download 123"</c>), if known.</summary>
    public string? Operation { get; init; }

    /// <summary>
    /// True when the failure was an AppID-context mismatch: workshop work was requested for a specific AppID that
    /// Steam is not actually running under - the "sleeping appid file" / wrong-context condition (a stale
    /// <c>steam_appid.txt</c>, another game, or a leftover appid file got in the way of the forced context).
    /// </summary>
    public bool IsAppIdMismatch { get; init; }

    public override string ToString()
    {
        List<string> parts = new();
        if (AppId is { } appId) parts.Add($"AppId={appId}");
        if (PublishedFileId is { } publishedFileId) parts.Add($"Item={publishedFileId}");
        if (Operation is not null) parts.Add($"Operation={Operation}");
        if (IsAppIdMismatch) parts.Add("AppIdMismatch");

        string baseText = base.ToString()!;
        return parts.Count > 0 ? $"{baseText}  [{string.Join(", ", parts)}]" : baseText;
    }

    /// <summary>
    /// Builds a self-describing <see cref="WorkshopException"/> for the case where workshop work was requested for
    /// <paramref name="requested"/> but Steam is actually running under <paramref name="current"/>. This is the
    /// condition a stale <c>steam_appid.txt</c> (or another game) produces: the forced AppID context did not take.
    /// </summary>
    public static WorkshopException AppIdMismatch(UInt32 requested, UInt32 current, string operation, ulong? publishedFileId = null)
        => new(
            $"Workshop work was requested for AppID {requested}, but Steam is actually running under AppID {current}. " +
            "Something else is overriding the forced AppID context - a stale steam_appid.txt, another game, or a " +
            "leftover appid file. Remove the conflicting appid file (or run under the correct AppID) and try again.",
            innerException: null)
        {
            AppId = requested,
            PublishedFileId = publishedFileId,
            Operation = operation,
            IsAppIdMismatch = true,
        };
}