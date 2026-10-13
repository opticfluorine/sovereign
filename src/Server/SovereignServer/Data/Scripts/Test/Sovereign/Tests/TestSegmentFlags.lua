-- Tests for the world segment flag API exposed by the Data module.
--
-- Only read paths and constant values are exercised here: flag writes are
-- asynchronous across systems, so the write path is proven by the C# callback
-- unit tests rather than the Lua harness.

local Suite = "TestSegmentFlags"

-- World constant: the side length of a world segment in world units.
local SegmentLength = 32

-- A segment index far from any fixture; the baseline database sets no flags here.
local FarSegment = 1000

Test.Case("FlagConstants", function()
    Test.AssertEqual(0, WorldSegmentFlag.None, "WorldSegmentFlag.None should be 0")
    Test.AssertEqual(1, WorldSegmentFlag.Pvp, "WorldSegmentFlag.Pvp should be 1")
end)

Test.Case("UnflaggedSegmentReadsZero", function()
    local flags = Data.GetSegmentFlags({ X = FarSegment, Y = FarSegment, Z = FarSegment })
    Test.AssertEqual(0, flags, "unflagged segment should read back a zero bitmask")
end)

Test.Case("UnflaggedPositionReadsZero", function()
    local offset = FarSegment * SegmentLength + 16
    local flags = Data.GetSegmentFlagsForPosition({ X = offset, Y = offset, Z = offset })
    Test.AssertEqual(0, flags, "position in an unflagged segment should read back a zero bitmask")
end)

Util.LogInfo("[" .. Suite .. "] suite registered")
