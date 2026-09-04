-- Self-test for the script test harness (Test library).
-- Covers synchronous cases, all assertion helpers (success and failure paths),
-- and asynchronous tests completed via Test.Step and Test.Pass.

local Suite = "TestHarnessSelfTest"

local onAsyncReady, onStepChain, onStepChainDone

-- Synchronous cases. ---------------------------------------------------------------

Test.Case("SyncCasePasses", function()
    Test.AssertTrue(true, "true should be true")
    Test.AssertEqual(1 + 1, 2, "arithmetic should work")
    Test.AssertNear(0.333, 1.0 / 3.0, 0.001, "near comparison")
    Test.AssertNear(3.14159, math.pi, 0.00001, "pi")
    Test.AssertNil(nil, "nil is nil")
end)

Test.Case("AssertionsRaiseErrors", function()
    -- Failure paths of the assertion helpers must raise Lua errors.
    local okTrue = pcall(function() Test.AssertTrue(false, "expect raise") end)
    Test.AssertTrue(not okTrue, "AssertTrue(false) should raise")

    local okEqual = pcall(function() Test.AssertEqual(1, 2, "expect raise") end)
    Test.AssertTrue(not okEqual, "AssertEqual mismatch should raise")

    local okNear = pcall(function() Test.AssertNear(0.0, 1.0, 0.1, "expect raise") end)
    Test.AssertTrue(not okNear, "AssertNear outside epsilon should raise")

    local okNil = pcall(function() Test.AssertNil(42, "expect raise") end)
    Test.AssertTrue(not okNil, "AssertNil on non-nil should raise")

    local okFailed = pcall(function() Test.AssertFailed("unconditional") end)
    Test.AssertTrue(not okFailed, "AssertFailed should raise")
end)

Test.Case("UnknownTestCompletionIsSafe", function()
    -- Completing an unknown test logs a warning but must not raise.
    local ok = pcall(function() Test.Pass("NoSuchTestAnywhere") end)
    Test.AssertTrue(ok, "Test.Pass on unknown test should not raise")
end)

-- Asynchronous cases. ---------------------------------------------------------------

Test.Async("AsyncPassInCallback")

onAsyncReady = function()
    Util.LogInfo("[" .. Suite .. "] completing AsyncPassInCallback")
    Test.Pass("AsyncPassInCallback")
end

Test.Async("AsyncStepChain")

onStepChain = function()
    Test.Step("AsyncStepChain", function()
        Test.AssertTrue(1 < 2, "step assertion should hold")
    end)
end

onStepChainDone = function()
    Test.Pass("AsyncStepChain")
end

Scripting.AddTimedCallback(0.3, onAsyncReady)
Scripting.AddTimedCallback(0.3, onStepChain)
Scripting.AddTimedCallback(0.7, onStepChainDone)

Util.LogInfo("[" .. Suite .. "] suite registered")
