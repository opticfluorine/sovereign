-- Tests for the Data module (global and entity-scoped key-value stores).

local Suite = "TestData"

local setup, stepGlobalWrite, stepGlobalVerify, stepGlobalDelete, stepGlobalVerifyDeleted
local step1EntityDataWrite, step2EntityDataVerify, step3EntityDataDelete, step4EntityDataVerifyDeleted
local stepReservedKeyVerify, stepReservedKeyWrite
local fixtureEntityId

local globalKey = "TestHarnessDataGlobalKey"

Test.Async("GlobalWriteReadDelete")

stepGlobalWrite = function()
    Data.Global[globalKey] = 12345
end

stepGlobalVerify = function()
    Test.Step("GlobalWriteReadDelete", function()
        Test.AssertEqual("12345", Data.Global[globalKey],
            "global key-value should round-trip as a string")
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
        Test.AssertEqual("3.14159", Data.GetEntityData(fixtureEntityId)["TestHarnessEntityKey"],
            "entity key-value should round-trip as a string")
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
    Scripting.AddTimedCallback(0.3, stepReservedKeyWrite)
    Scripting.AddTimedCallback(0.5, stepReservedKeyVerify)
end

Scripting.AddTimedCallback(0.4, setup)

Util.LogInfo("[" .. Suite .. "] suite registered")
