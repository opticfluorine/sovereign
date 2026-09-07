-- Tests for the Items module (item template name lookups).
-- Requires a sword item template in the database (Items.FindByName("Sword")).

local Suite = "TestItems"

Test.Case("FindByName", function()
    local swords = Items.FindByName("Sword")
    Test.AssertTrue(swords ~= nil, "FindByName should return a table")
    Test.AssertTrue(#swords > 0, "sword item template should exist in the database")
    Test.AssertTrue(Entities.IsTemplate(swords[1]), "matched entity should be in the template range")

    -- Lookups are case-insensitive.
    local lower = Items.FindByName("sword")
    Test.AssertTrue(lower ~= nil and #lower > 0, "case-insensitive lookup should match")

    -- Unknown names return an empty table.
    local missing = Items.FindByName("ThisItemDoesNotExist12345")
    Test.AssertTrue(missing ~= nil and #missing == 0, "unknown name should return an empty table")
end)

Test.Case("FindByFuzzyName", function()
    local matches = Items.FindByFuzzyName("Sowrd", 5)
    Test.AssertTrue(matches ~= nil, "FindByFuzzyName should return a table")
    Test.AssertTrue(#matches > 0, "fuzzy query should match the sword template")

    local match = matches[1]
    Test.AssertTrue(match.EntityId ~= nil and match.EntityId ~= Entities.ToEntityId(0),
        "match should carry an entity ID")
    Test.AssertTrue(match.Name ~= nil and match.Name:len() > 0, "match should carry a name")
    Test.AssertTrue(match.Score >= 0.0 and match.Score <= 1.0, "score should be in [0, 1]")

    -- Exact query scores 1.0 and sorts first.
    local exact = Items.FindByFuzzyName("Sword", 5)
    Test.AssertTrue(#exact > 0 and exact[1].Name == "Sword" and exact[1].Score == 1.0,
        "exact match should score 1.0 and sort first")
end)

Util.LogInfo("[" .. Suite .. "] suite registered")
