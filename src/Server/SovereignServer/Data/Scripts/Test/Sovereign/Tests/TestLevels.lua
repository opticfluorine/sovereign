-- Tests for the Level and Experience components.

local Suite = "TestLevels"

local setup, step1VerifyCreated, step2StartAdd, step3VerifyAdd
local step4StartRemove, step5VerifyRemoved
local npcEntityId, npcNoLevelsEntityId

Test.Async("CreateWithSpec")

step1VerifyCreated = function()
    Test.Step("CreateWithSpec", function()
        Test.AssertTrue(Components.Level.Exists(npcEntityId), "level component should exist")
        Test.AssertEqual(3, Components.Level.Get(npcEntityId), "level from Entities.Create spec")
        Test.AssertTrue(Components.Experience.Exists(npcEntityId), "experience component should exist")
        Test.AssertEqual(100, Components.Experience.Get(npcEntityId), "experience from Entities.Create spec")
        Test.AssertNil(Components.Level.Get(npcNoLevelsEntityId), "entity without level spec should have no level")
        Test.AssertNil(Components.Experience.Get(npcNoLevelsEntityId), "entity without experience spec should have no experience")
    end)
    Test.Pass("CreateWithSpec")
end

Test.Async("IntAdd")

step2StartAdd = function()
    -- Add fieldwise: Level 3 + 2 = 5, Experience 100 + 50 = 150.
    Components.Level.Add(npcEntityId, 2)
    Components.Experience.Add(npcEntityId, 50)
end

step3VerifyAdd = function()
    Test.Step("IntAdd", function()
        Test.AssertEqual(5, Components.Level.Get(npcEntityId), "level after Add")
        Test.AssertEqual(150, Components.Experience.Get(npcEntityId), "experience after Add")
    end)
    Test.Pass("IntAdd")
end

Test.Async("RemoveLevelAndExperience")

step4StartRemove = function()
    Components.Level.Remove(npcEntityId)
    Components.Experience.Remove(npcEntityId)
end

step5VerifyRemoved = function()
    Test.Step("RemoveLevelAndExperience", function()
        Test.AssertNil(Components.Level.Get(npcEntityId), "level component should be removed")
        Test.AssertNil(Components.Experience.Get(npcEntityId), "experience component should be removed")
    end)
    Test.Pass("RemoveLevelAndExperience")
end

-- Suite setup. ------------------------------------------------------------------------

setup = function()
    npcEntityId = Entities.Create({
        Name = "TestLevelsNpc",
        EntityType = EntityType.Npc,
        NonPersistent = true,
        Level = 3,
        Experience = 100
    })
    npcNoLevelsEntityId = Entities.Create({
        Name = "TestLevelsNpcNoLevels",
        EntityType = EntityType.Npc,
        NonPersistent = true
    })

    Scripting.AddTimedCallback(0.3, step1VerifyCreated)
    Scripting.AddTimedCallback(0.5, step2StartAdd)
    Scripting.AddTimedCallback(0.8, step3VerifyAdd)
    Scripting.AddTimedCallback(1.0, step4StartRemove)
    Scripting.AddTimedCallback(1.3, step5VerifyRemoved)
end

Scripting.AddTimedCallback(0.3, setup)

Util.LogInfo("[" .. Suite .. "] suite registered")
