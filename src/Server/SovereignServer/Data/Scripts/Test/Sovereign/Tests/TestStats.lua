-- Tests for the Stats component.

local Suite = "TestStats"

local setup, step1VerifyCreated, step2StartAdd, step3VerifyAdd
local step4StartRemove, step5VerifyRemoved
local npcEntityId

Test.Async("CreateWithSpec")

step1VerifyCreated = function()
    Test.Step("CreateWithSpec", function()
        Test.AssertTrue(Components.Stats.Exists(npcEntityId), "stats component should exist")
        local stats = Components.Stats.Get(npcEntityId)
        Test.AssertEqual(3, stats.Strength, "strength from Entities.Create spec")
        Test.AssertEqual(2, stats.Defense, "defense from Entities.Create spec")
        Test.AssertEqual(4, stats.Agility, "agility from Entities.Create spec")
        Test.AssertEqual(5, stats.Intelligence, "intelligence from Entities.Create spec")
        Test.AssertEqual(6, stats.Wisdom, "wisdom from Entities.Create spec")
        Test.AssertEqual(7, stats.Charisma, "charisma from Entities.Create spec")
        Test.AssertEqual(8, stats.Luck, "luck from Entities.Create spec")
    end)
    Test.Pass("CreateWithSpec")
end

Test.Async("FieldwiseAdd")

step2StartAdd = function()
    -- Add fieldwise: Strength 3 + 2 = 5, Luck 8 + 1 = 9, others unchanged.
    Components.Stats.Add(npcEntityId, {
        Strength = 2, Defense = 0, Agility = 0, Intelligence = 0,
        Wisdom = 0, Charisma = 0, Luck = 1
    })
end

step3VerifyAdd = function()
    Test.Step("FieldwiseAdd", function()
        local stats = Components.Stats.Get(npcEntityId)
        Test.AssertEqual(5, stats.Strength, "strength after fieldwise Add")
        Test.AssertEqual(2, stats.Defense, "defense after fieldwise Add")
        Test.AssertEqual(4, stats.Agility, "agility after fieldwise Add")
        Test.AssertEqual(5, stats.Intelligence, "intelligence after fieldwise Add")
        Test.AssertEqual(6, stats.Wisdom, "wisdom after fieldwise Add")
        Test.AssertEqual(7, stats.Charisma, "charisma after fieldwise Add")
        Test.AssertEqual(9, stats.Luck, "luck after fieldwise Add")
    end)
    Test.Pass("FieldwiseAdd")
end

Test.Async("RemoveStats")

step4StartRemove = function()
    Components.Stats.Remove(npcEntityId)
end

step5VerifyRemoved = function()
    Test.Step("RemoveStats", function()
        Test.AssertNil(Components.Stats.Get(npcEntityId), "stats component should be removed")
    end)
    Test.Pass("RemoveStats")
end

-- Suite setup. ------------------------------------------------------------------------

setup = function()
    npcEntityId = Entities.Create({
        Name = "TestStatsNpc",
        EntityType = EntityType.Npc,
        NonPersistent = true,
        Stats = {
            Strength = 3, Defense = 2, Agility = 4, Intelligence = 5,
            Wisdom = 6, Charisma = 7, Luck = 8
        }
    })

    Scripting.AddTimedCallback(0.3, step1VerifyCreated)
    Scripting.AddTimedCallback(0.5, step2StartAdd)
    Scripting.AddTimedCallback(0.8, step3VerifyAdd)
    Scripting.AddTimedCallback(1.0, step4StartRemove)
    Scripting.AddTimedCallback(1.3, step5VerifyRemoved)
end

Scripting.AddTimedCallback(0.3, setup)

Util.LogInfo("[" .. Suite .. "] suite registered")
