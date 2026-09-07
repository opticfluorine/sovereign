-- Tests for the Time module (system clock monotonicity and calendar getters).

local Suite = "TestTime"

local stepMonotonicity
local firstSystemTime = nil

Test.Case("SystemTimePresent", function()
    local now = Time.GetSystemTime()
    Test.AssertTrue(now > 0, "system time should be positive")
    firstSystemTime = now
end)

Test.Case("FutureSystemTime", function()
    local now = Time.GetSystemTime()
    local future = Time.FutureSystemTime(10.0)
    Test.AssertTrue(future > now, "future system time should be ahead of now")
end)

Test.Case("AbsoluteTime", function()
    local absolute = Time.GetAbsoluteTime()
    Test.AssertTrue(absolute >= 0, "absolute game time should be non-negative")
end)

Test.Case("CalendarGetters", function()
    -- Relative calendar values must stay within plausible ranges; exact values
    -- depend on the game clock and configuration.
    Test.AssertTrue(Time.GetYear() >= 0, "year should be non-negative")
    Test.AssertTrue(Time.GetSeason() >= 0, "season should be non-negative")
    Test.AssertTrue(Time.GetMonth() >= 0, "month should be non-negative")
    Test.AssertTrue(Time.GetMonthOfYear() >= 0 and Time.GetMonthOfYear() < 12,
        "month of year should be in [0, 11] for the default calendar")
    Test.AssertTrue(Time.GetWeekOfMonth() >= 0 and Time.GetWeekOfMonth() < 4,
        "week of month should be in [0, 3] for the default calendar")
    Test.AssertTrue(Time.GetDayOfMonth() >= 0 and Time.GetDayOfMonth() < 28,
        "day of month should be in [0, 27] for the default calendar")
    Test.AssertTrue(Time.GetWeek() >= 0, "week should be non-negative")
    Test.AssertTrue(Time.GetDayOfWeek() >= 0 and Time.GetDayOfWeek() < 7,
        "day of week should be in [0, 6] for the default calendar")
    Test.AssertTrue(Time.GetDay() >= 0, "day should be non-negative")
    Test.AssertTrue(Time.GetHourOfDay() >= 0 and Time.GetHourOfDay() < 24,
        "hour of day should be in [0, 23]")
    Test.AssertTrue(Time.GetSecondOfDay() >= 0, "second of day should be non-negative")
end)

Test.Async("SystemTimeMonotonic")

stepMonotonicity = function()
    Test.Step("SystemTimeMonotonic", function()
        local later = Time.GetSystemTime()
        Test.AssertTrue(firstSystemTime ~= nil, "earlier system time should have been recorded")
        Test.AssertTrue(later >= firstSystemTime, "system time should be monotonic")
    end)
    Test.Pass("SystemTimeMonotonic")
end

Scripting.AddTimedCallback(0.5, stepMonotonicity)

Util.LogInfo("[" .. Suite .. "] suite registered")
