-- Tests for the vitals components and the VitalsSystem.

local Suite = "TestVitals"

local setup, step1VerifyCreated, step2StartRegen, step2VerifyRegen, step3VerifyStaminaMana
local step4ZeroHealth, step5VerifyRemoved
local npcEntityId, vitalsEntityId

Test.Async("CreateWithSpec")

step1VerifyCreated = function()
    Test.Step("CreateWithSpec", function()
        Test.AssertTrue(Components.Health.Exists(npcEntityId), "health component should exist")
        local health = Components.Health.Get(npcEntityId)
        Test.AssertEqual(5, health.Value, "health value from Entities.Create spec")
        Test.AssertEqual(10, health.MaxValue, "health max value from Entities.Create spec")
        Test.AssertEqual(1, health.ChangeRate, "health change rate from Entities.Create spec")
        Test.AssertEqual(1000000, health.ChangeInterval, "health change interval from Entities.Create spec")
    end)
    Test.Pass("CreateWithSpec")
end

Test.Async("HealthRegenerates")

step2StartRegen = function()
    -- Enable regeneration at +1 health every 2 ticks.
    Components.Health.Set(npcEntityId, {
        Value = 5, MaxValue = 10, ChangeRate = 1, ChangeInterval = 2
    })
end

step2VerifyRegen = function()
    Test.Step("HealthRegenerates", function()
        local health = Components.Health.Get(npcEntityId)
        Test.AssertEqual(10, health.Value, "health should have regenerated to MaxValue")
    end)
    Test.Pass("HealthRegenerates")
end

Test.Async("StaminaAndManaSpecs")

step3VerifyStaminaMana = function()
    Test.Step("StaminaAndManaSpecs", function()
        Test.AssertTrue(Components.Stamina.Exists(vitalsEntityId), "stamina component should exist")
        Test.AssertEqual(7, Components.Stamina.Get(vitalsEntityId).Value, "stamina value from spec")
        Test.AssertEqual(2, Components.Mana.Get(vitalsEntityId).Value, "mana value from spec")
    end)
    Test.Pass("StaminaAndManaSpecs")
end

Test.Async("DeathRemovesNpc")

step4ZeroHealth = function()
    -- Drop health to zero with no regeneration so the entity is detected as dead.
    Components.Health.Set(npcEntityId, {
        Value = 0, MaxValue = 10, ChangeRate = 0, ChangeInterval = 0
    })
end

step5VerifyRemoved = function()
    Test.Step("DeathRemovesNpc", function()
        Test.AssertNil(Components.Health.Get(npcEntityId), "dead NPC should have been removed")
    end)
    Test.Pass("DeathRemovesNpc")
end

-- Suite setup. ------------------------------------------------------------------------

setup = function()
    npcEntityId = Entities.Create({
        Name = "TestVitalsNpc",
        EntityType = EntityType.Npc,
        NonPersistent = true,
        Kinematics = {
            Position = { X = 0.5, Y = 0.5, Z = 1.0 },
            Velocity = { X = 0.0, Y = 0.0, Z = 0.0 }
        },
        Health = { Value = 5, MaxValue = 10, ChangeRate = 1, ChangeInterval = 1000000 }
    })

    vitalsEntityId = Entities.Create({
        Name = "TestVitalsStamina",
        EntityType = EntityType.Npc,
        NonPersistent = true,
        Stamina = { Value = 7, MaxValue = 9, ChangeRate = 0, ChangeInterval = 0 },
        Mana = { Value = 2, MaxValue = 9, ChangeRate = 0, ChangeInterval = 0 }
    })

    Scripting.AddTimedCallback(0.3, step1VerifyCreated)
    Scripting.AddTimedCallback(0.5, step2StartRegen)
    Scripting.AddTimedCallback(0.8, step2VerifyRegen)
    Scripting.AddTimedCallback(0.6, step3VerifyStaminaMana)
    Scripting.AddTimedCallback(1.1, step4ZeroHealth)
    Scripting.AddTimedCallback(1.6, step5VerifyRemoved)
end

Scripting.AddTimedCallback(0.3, setup)

Util.LogInfo("[" .. Suite .. "] suite registered")
