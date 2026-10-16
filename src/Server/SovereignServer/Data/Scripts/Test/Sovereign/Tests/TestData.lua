-- Tests for the Data module (global and entity-scoped key-value stores).

local Suite = "TestData"

local setup, stepGlobalWrite, stepGlobalVerify, stepGlobalDelete, stepGlobalVerifyDeleted
local step1EntityDataWrite, step2EntityDataVerify, step3EntityDataDelete, step4EntityDataVerifyDeleted
local stepReservedKeyVerify, stepReservedKeyWrite
local stepVector3Write, stepVector3Verify
local stepSegmentDataWriteRead
local fixtureEntityId

local globalKey = "TestHarnessDataGlobalKey"
local vectorKey = "TestHarnessDataVectorKey"
local segmentDataKey = "TestHarnessSegmentDataKey"

Test.Async("GlobalWriteReadDelete")

stepGlobalWrite = function()
    Data.Global[globalKey] = 12345
end

stepGlobalVerify = function()
    Test.Step("GlobalWriteReadDelete", function()
        Test.AssertEqual(12345, Data.Global[globalKey],
            "global key-value should round-trip as an integer")
    end)
end

stepGlobalDelete = function()
    Data.Global[globalKey] = nil
end

stepGlobalVerifyDeleted = function()
    Test.Step("GlobalWriteReadDelete", function()
        Test.AssertNil(Data.Global[globalKey], "deleted global key should read back nil")
    end)
    Test.Pass("GlobalWriteReadDelete")
end

Test.Async("EntityDataStore")

step1EntityDataWrite = function()
    Data.GetEntityData(fixtureEntityId)["TestHarnessEntityKey"] = 3.14159
end

step2EntityDataVerify = function()
    Test.Step("EntityDataStore", function()
        Test.AssertEqual(3.14159, Data.GetEntityData(fixtureEntityId)["TestHarnessEntityKey"],
            "entity key-value should round-trip as a float")
    end)
end

step3EntityDataDelete = function()
    Data.GetEntityData(fixtureEntityId)["TestHarnessEntityKey"] = nil
end

step4EntityDataVerifyDeleted = function()
    Test.Step("EntityDataStore", function()
        Test.AssertNil(Data.GetEntityData(fixtureEntityId)["TestHarnessEntityKey"],
            "deleted entity key should read back nil")
    end)
    Test.Pass("EntityDataStore")
end

Test.Async("Vector3Value")

stepVector3Write = function()
    Data.Global[vectorKey] = { X = 1.5, Y = 2.5, Z = 3.5 }
end

stepVector3Verify = function()
    Test.Step("Vector3Value", function()
        local value = Data.Global[vectorKey]
        Test.AssertEqual(1.5, value.X, "Vector3 X component should round-trip")
        Test.AssertEqual(2.5, value.Y, "Vector3 Y component should round-trip")
        Test.AssertEqual(3.5, value.Z, "Vector3 Z component should round-trip")
    end)
    Data.Global[vectorKey] = nil
    Test.Pass("Vector3Value")
end

Test.Async("SegmentDataStore")

stepSegmentDataWriteRead = function()
    local segment = { X = 0, Y = 0, Z = 0 }
    local segmentData = Data.GetSegmentData(segment)

    -- Read-your-write is immediate: the value is visible in the same callback, with no
    -- tick or Step boundary between the write and the read.
    segmentData[segmentDataKey] = fixtureEntityId
    Test.Step("SegmentDataStore", function()
        Test.AssertEqual(fixtureEntityId, segmentData[segmentDataKey],
            "segment key-value should round-trip as an EntityId immediately after write")
    end)

    segmentData[segmentDataKey] = 7
    Test.Step("SegmentDataStore", function()
        Test.AssertEqual(7, segmentData[segmentDataKey],
            "segment key-value should round-trip as an integer immediately after write")
    end)

    -- Clean up so the test database is not left with a stray row.
    segmentData[segmentDataKey] = nil
    Test.Pass("SegmentDataStore")
end

Test.Async("ReservedKeysIgnored")

stepReservedKeyWrite = function()
    -- Keys starting with double underscores are reserved for the engine; writes are
    -- rejected (logged) without raising and must not modify the store.
    Data.Global["__TestHarnessReserved"] = "nope"
end

stepReservedKeyVerify = function()
    Test.Step("ReservedKeysIgnored", function()
        Test.AssertNil(Data.Global["__TestHarnessReserved"],
            "reserved key write should have been ignored")
    end)
    Test.Pass("ReservedKeysIgnored")
end

-- Suite setup. ------------------------------------------------------------------------

-- Fixture creation is deferred to stagger startup across suites; the engine does not
-- currently serialize concurrent engine mutations from parallel script hosts.
setup = function()
    fixtureEntityId = Entities.Create({
        Name = "TestDataFixture",
        EntityType = EntityType.Npc,
        NonPersistent = true,
        Kinematics = {
            Position = { X = 0.5, Y = 0.5, Z = 1.0 },
            Velocity = { X = 0.0, Y = 0.0, Z = 0.0 }
        }
    })

    Scripting.AddTimedCallback(0.3, stepGlobalWrite)
    Scripting.AddTimedCallback(0.5, stepGlobalVerify)
    Scripting.AddTimedCallback(0.7, stepGlobalDelete)
    Scripting.AddTimedCallback(0.9, stepGlobalVerifyDeleted)
    Scripting.AddTimedCallback(0.3, step1EntityDataWrite)
    Scripting.AddTimedCallback(0.5, step2EntityDataVerify)
    Scripting.AddTimedCallback(0.7, step3EntityDataDelete)
    Scripting.AddTimedCallback(0.9, step4EntityDataVerifyDeleted)
    Scripting.AddTimedCallback(0.3, stepVector3Write)
    Scripting.AddTimedCallback(0.5, stepVector3Verify)
    Scripting.AddTimedCallback(0.5, stepSegmentDataWriteRead)
    Scripting.AddTimedCallback(0.3, stepReservedKeyWrite)
    Scripting.AddTimedCallback(0.5, stepReservedKeyVerify)
end

Scripting.AddTimedCallback(0.4, setup)

Util.LogInfo("[" .. Suite .. "] suite registered")
