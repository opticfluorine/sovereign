-- Tests for the Players module (offline-safe lookups; no players are connected).

local Suite = "TestPlayers"

Test.Case("FindByNameNoPlayers", function()
    local playerId = Players.FindByName("TestHarnessNobody")
    Test.AssertEqual(0, playerId, "lookup of a non-online player should return 0")
end)

Test.Case("FindByFuzzyNameNoPlayers", function()
    local matches = Players.FindByFuzzyName("TestHarnessNobody", 5)
    Test.AssertTrue(matches ~= nil, "FindByFuzzyName should return a table")
    Test.AssertEqual(0, #matches, "fuzzy lookup with no players online should be empty")
end)

Util.LogInfo("[" .. Suite .. "] suite registered")
