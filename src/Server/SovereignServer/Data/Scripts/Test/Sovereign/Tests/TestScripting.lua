-- Tests for the Scripting module (callback registration APIs).

local Suite = "TestScripting"

local setup, onTimedWithArg, onTimedNoArg, onTickEvent, onTickCheck, onCollisionSetup
local tickCount = 0
local fixtureEntityId = nil

-- Asynchronous cases. ---------------------------------------------------------------

Test.Async("TimedCallbackWithArg")

onTimedWithArg = function(arg)
    Test.Step("TimedCallbackWithArg", function()
        Test.AssertEqual(42, arg, "timed callback should receive its argument")
    end)
    Test.Pass("TimedCallbackWithArg")
end

Test.Async("TimedCallbackWithoutArg")

onTimedNoArg = function(arg)
    Test.Step("TimedCallbackWithoutArg", function()
        Test.AssertNil(arg, "optional argument should default to nil")
    end)
    Test.Pass("TimedCallbackWithoutArg")
end

Test.Async("EventCallbackFiresOnTick")

onTickEvent = function()
    tickCount = tickCount + 1
end

onTickCheck = function()
    Test.Step("EventCallbackFiresOnTick", function()
        Test.AssertTrue(tickCount > 0, "Core_Tick callback should have fired")
    end)
    Test.Pass("EventCallbackFiresOnTick")
end

Test.Async("CollisionCallbackRoundtrip")

onCollisionSetup = function()
    -- Roundtrip: register a collision callback, then remove it via the returned handle.
    local handle = Scripting.AddCollisionCallback(fixtureEntityId, function() end)
    Test.Step("CollisionCallbackRoundtrip", function()
        Test.AssertTrue(handle ~= nil and handle > 0, "collision callback handle should be positive")
    end)
    Scripting.RemoveCollisionCallback(fixtureEntityId, handle)
    Test.Pass("CollisionCallbackRoundtrip")
end

-- Synchronous cases. -----------------------------------------------------------------

Test.Case("AddEntityParameterHint", function()
    Scripting.AddEntityParameterHint("test_scripting_callback", "TestParameter", "String",
        "Test parameter for the script test harness.")
end)

-- Suite setup. ------------------------------------------------------------------------

-- Fixture creation is deferred to stagger startup across suites; the engine does not
-- currently serialize concurrent engine mutations from parallel script hosts.
setup = function()
    fixtureEntityId = Entities.Create({
        Name = "TestScriptingFixture",
        EntityType = EntityType.Npc,
        NonPersistent = true,
        Kinematics = {
            Position = { X = 0.5, Y = 0.5, Z = 1.0 },
            Velocity = { X = 0.0, Y = 0.0, Z = 0.0 }
        }
    })

    Scripting.AddTimedCallback(0.4, onCollisionSetup)
end

Scripting.AddEventCallback(Events.Core_Tick, onTickEvent)

Scripting.AddTimedCallback(0.1, setup)
Scripting.AddTimedCallback(0.4, onTimedWithArg, 42)
Scripting.AddTimedCallback(0.4, onTimedNoArg)
Scripting.AddTimedCallback(0.6, onTickCheck)

Util.LogInfo("[" .. Suite .. "] suite registered")
