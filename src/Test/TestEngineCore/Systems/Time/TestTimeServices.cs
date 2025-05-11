// Sovereign Engine
// Copyright (c) 2025 opticfluorine
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using Microsoft.Extensions.Options;
using Moq;
using Sovereign.EngineCore.Configuration;
using Xunit;

namespace Sovereign.EngineCore.Systems.Time;

/// <summary>
///     Unit tests for TimeServices.
/// </summary>
public class TestTimeServices
{
    private readonly Mock<IGameClock> mockClock;
    private readonly TimeServices timeServices;

    public TestTimeServices()
    {
        mockClock = new Mock<IGameClock>();
        var mockOptions = new Mock<IOptions<TimeOptions>>();
        var timeOptions = new TimeOptions
        {
            SecondsPerHour = 3600,
            HoursPerDay = 24,
            DaysPerWeek = 7,
            WeeksPerMonth = 4,
            MonthsPerSeason = 3
        };
        mockOptions.Setup(o => o.Value).Returns(timeOptions);

        timeServices = new TimeServices(mockClock.Object, mockOptions.Object);
    }

    [Fact]
    public void AbsoluteTime_ReturnsCorrectValue()
    {
        mockClock.Setup(c => c.Time).Returns(123456);
        Assert.Equal(123456u, timeServices.AbsoluteTime);
    }

    [Theory]
    [InlineData(3600 * 24 * 7 * 4 * 3 + 82989, 0)]
    [InlineData(3600 * 24 * 7 * 4 * 12 * 2 + 9692, 2)]
    public void Year_CalculatesCorrectly(uint absTime, uint year)
    {
        mockClock.Setup(c => c.Time).Returns(absTime);
        Assert.Equal(year, timeServices.Year);
    }

    [Theory]
    [InlineData(3600 * 24 * 7 * 4 * 0, Season.Spring)]
    [InlineData(3600 * 24 * 7 * 4 * 3, Season.Summer)]
    [InlineData(3600 * 24 * 7 * 4 * 6, Season.Fall)]
    [InlineData(3600 * 24 * 7 * 4 * 9, Season.Winter)]
    [InlineData(3600 * 24 * 7 * 4 * 12, Season.Spring)]
    public void Season_CalculatesCorrectly(uint absTime, Season season)
    {
        mockClock.Setup(c => c.Time).Returns(absTime);
        Assert.Equal(season, timeServices.Season);
    }

    [Theory]
    [InlineData(3600 * 24 * 7 * 4 * 5, 5)]
    [InlineData(3600 * 24 * 7 * 4 * 14 + 5, 2)]
    public void MonthOfYear_CalculatesCorrectly(uint absTime, uint monthOfYear)
    {
        mockClock.Setup(c => c.Time).Returns(absTime);
        Assert.Equal(monthOfYear, timeServices.MonthOfYear);
    }

    [Theory]
    [InlineData(3600 * 24 * 10, 10)]
    [InlineData(3600 * 24 * 7 * 5, 7)]
    public void DayOfMonth_CalculatesCorrectly(uint absTime, uint dayOfMonth)
    {
        mockClock.Setup(c => c.Time).Returns(absTime);
        Assert.Equal(dayOfMonth, timeServices.DayOfMonth);
    }

    [Theory]
    [InlineData(3600 * 15, 15)]
    [InlineData(3600 * 25 + 32, 1)]
    public void HourOfDay_CalculatesCorrectly(uint absTime, uint hourOfDay)
    {
        mockClock.Setup(c => c.Time).Returns(absTime);
        Assert.Equal(hourOfDay, timeServices.HourOfDay);
    }

    [Fact]
    public void SecondOfDay_CalculatesCorrectly()
    {
        mockClock.Setup(c => c.Time).Returns(3600 * 24 + 12345); // 1 day + 12345 seconds
        Assert.Equal(12345u, timeServices.SecondOfDay);
    }
}