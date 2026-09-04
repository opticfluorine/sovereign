-- Tests for the Events module (event table values and Core_Tick callback firing).

local Suite = "TestEvents"

local stepVerifyTickCount
local tickCount = 0

local onTick = function()
    tickCount = tickCount + 1
end

Test.Case("EventTableValues", function()
    Test.AssertTrue(Events.Core_Tick ~= nil, "Core_Tick should be defined")
    Test.AssertTrue(Events.Core_Network_Logout ~= nil, "Core_Network_Logout should be defined")
    Test.AssertTrue(Events.Server_Persistence_PlayerEnteredWorld ~= nil,
        "Server_Persistence_PlayerEnteredWorld should be defined")
end)

Test.Case("AddEventCallback", function()
    Scripting.AddEventCallback(Events.Core_Tick, onTick)
end)

Test.Async("TickCallbackFires")

stepVerifyTickCount = function()
    Test.Step("TickCallbackFires", function()
        Test.AssertTrue(tickCount > 0, "Core_Tick callback should have fired at least once")
    end)
    Test.Pass("TickCallbackFires")
end

Scripting.AddTimedCallback(0.5, stepVerifyTickCount)

Util.LogInfo("[" .. Suite .. "] suite registered")
