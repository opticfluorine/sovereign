-- Tests for the Util module (logging functions and ToBool).

local Suite = "TestUtil"

Test.Case("LogFunctionsDoNotThrow", function()
    local msg = "[" .. Suite .. "] log function smoke test"
    Util.LogTrace(msg)
    Util.LogDebug(msg)
    Util.LogInfo(msg)
    Util.LogWarn(msg)
    Util.LogError(msg)
    Util.LogCrit(msg)
end)

Test.Case("ToBoolValidInput", function()
    Test.AssertEqual(true, Util.ToBool("True"), "'True' should convert to true")
    Test.AssertEqual(true, Util.ToBool("true"), "'true' should convert to true")
    Test.AssertEqual(false, Util.ToBool("False"), "'False' should convert to false")
    Test.AssertEqual(false, Util.ToBool("false"), "'false' should convert to false")
end)

Test.Case("ToBoolInvalidInput", function()
    Test.AssertNil(Util.ToBool("Hello World!"), "invalid string should yield nil")
    Test.AssertNil(Util.ToBool(""), "empty string should yield nil")
end)

Util.LogInfo("[" .. Suite .. "] suite registered")
