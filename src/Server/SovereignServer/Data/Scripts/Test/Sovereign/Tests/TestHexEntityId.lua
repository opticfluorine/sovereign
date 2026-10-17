-- Test suite for hexadecimal text encoding of EntityId-typed key-value data.
--
-- The canonical text encoding of an EntityId value is 16 lowercase hexadecimal
-- digits with no 0x prefix (e.g. 7fff000000000000 for the first persisted entity
-- ID). This suite verifies the codec through a live server:
--
-- If the key is absent, the first persisted entity ID (0x7FFF000000000000) is
-- written as an EntityId under segment (0,0,0) and the immediate read-your-write
-- round trip is verified; the key is left in place for inspection of the
-- canonical hex text stored by the persistence layer.
--
-- If the key is present (loaded from the database at startup), reading it back
-- exercises hex decoding; the value must equal the original ID. The key is then
-- deleted to clean up.

local key = "TestHexOwner"
local segment = { X = 0, Y = 0, Z = 0 }
local expected = Entities.ToEntityId(0x7FFF000000000000)

Test.Async("HexEntityIdRoundTrip")

local step

step = function()
    local segmentData = Data.GetSegmentData(segment)
    Test.Step("HexEntityIdRoundTrip", function()
        local existing = segmentData[key]
        if existing == nil then
            segmentData[key] = expected
            Test.AssertEqual(expected, segmentData[key],
                "EntityId value should round-trip immediately after write")
            Util.LogInfo("[TestHexEntityId] wrote first persisted ID as hex: "
                .. Entities.FormatEntityId(expected))
        else
            Test.AssertEqual(expected, existing,
                "persisted EntityId should decode back to the original ID")
            segmentData[key] = nil
            Test.AssertNil(segmentData[key], "key should be deleted after cleanup")
            Util.LogInfo("[TestHexEntityId] persisted round trip decoded "
                .. Entities.FormatEntityId(existing))
        end
        Test.Pass("HexEntityIdRoundTrip")
    end)
end

Scripting.AddTimedCallback(0.5, step)
