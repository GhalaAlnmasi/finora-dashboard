using finora_dashboard.Models;
using finora_dashboard.Services;
using Xunit;

namespace finora_dashboard.Tests;

public class DisplayPreferenceTests
{
    [Fact]
    public void Initials_UseFirstAndLastWords()
    {
        Assert.Equal("GA", DemoProfile.InitialsFrom("Ghala Alshammari"));
        Assert.Equal("FN", DemoProfile.InitialsFrom("  "));
        Assert.Equal("SA", DemoProfile.InitialsFrom("Sara"));
    }

    [Fact]
    public void Money_FormatsAccordingToPreference()
    {
        var previous = Money.Style;
        try
        {
            Money.Style = SarDisplayFormat.CodePrefix;
            Assert.Equal("SAR 1,234.50", Money.Sar(1234.5m));

            Money.Style = SarDisplayFormat.CodeSuffix;
            Assert.Equal("1,234.50 SAR", Money.Sar(1234.5m));

            Money.Style = SarDisplayFormat.UngroupedPrefix;
            Assert.Equal("SAR 1234.50", Money.Sar(1234.5m));
        }
        finally
        {
            Money.Style = previous;
        }
    }

    [Fact]
    public void Dates_FormatsAccordingToPreference()
    {
        var previous = Dates.Style;
        var date = new DateOnly(2026, 11, 2);
        try
        {
            Dates.Style = DateDisplayFormat.Medium;
            Assert.Equal("2 Nov 2026", Dates.Format(date));

            Dates.Style = DateDisplayFormat.Long;
            Assert.Equal("2 November 2026", Dates.Format(date));

            Dates.Style = DateDisplayFormat.Iso;
            Assert.Equal("2026-11-02", Dates.Format(date));
        }
        finally
        {
            Dates.Style = previous;
        }
    }
}
