using System;
using SteamVent.Core;
using Xunit;

namespace SteamVent.Tests
{
    /// <summary>
    /// Pure (no Steam required) tests of the unified <see cref="WorkshopException"/>: the AppID-mismatch factory,
    /// the contextual ToString, and the "box the backend exception" (InnerException) contract.
    /// </summary>
    public class WorkshopExceptionTests
    {
        [Fact]
        public void AppIdMismatch_SetsContextAndMessage()
        {
            WorkshopException ex = WorkshopException.AppIdMismatch(1091500, 624970, "workshop download", 12345);

            Assert.Equal(1091500u, ex.AppId!.Value);
            Assert.Equal(12345ul, ex.PublishedFileId!.Value);
            Assert.Equal("workshop download", ex.Operation);
            Assert.True(ex.IsAppIdMismatch);
            Assert.Contains("1091500", ex.Message);
            Assert.Contains("624970", ex.Message);
            Assert.Null(ex.InnerException);
        }

        [Fact]
        public void ToString_IncludesContext()
        {
            WorkshopException ex = new("boom") { AppId = 1, Operation = "workshop list" };
            string text = ex.ToString();

            Assert.Contains("AppId=1", text);
            Assert.Contains("Operation=workshop list", text);
        }

        [Fact]
        public void BoxesInnerException()
        {
            var inner = new InvalidOperationException("inner detail");
            WorkshopException ex = new("outer detail", inner);

            Assert.Same(inner, ex.InnerException);
            Assert.Contains("inner detail", ex.ToString());
        }

        [Fact]
        public void IsAppIdMismatch_DefaultsFalse()
        {
            WorkshopException ex = new("plain workshop failure") { AppId = 42 };

            Assert.False(ex.IsAppIdMismatch);
            Assert.Equal(42u, ex.AppId!.Value);
        }
    }
}