-- Tests for the combat system: AttackDetails component, Combat.Attack, and
-- the Server_Combat_PlayerKilled scriptable event.

local Suite = "TestCombat"

-- All fixtures are placed in a remote area to avoid interfering with other suites.
local BaseX, BaseY = 1000.5, 1000.0

-- Fixture entity IDs.
local attackerId, victimId
local attacker2Id, behindTargetId, farTargetId
local playerAttackerId, npcVictimId
local npcKillerId, playerVictimId

-- Captured Server_Combat_PlayerKilled event data.
local killedVictim, killedKiller

local step1VerifyComponent, step2Attack, step3VerifyDamage, step4AttackRateLimited
local step5VerifyRateLimited, step6AttackAfterDelay, step7VerifySecondDamage
local step8AttackFiltered, step9VerifyFiltered
local step10AttackToDeath, step11AttackAgain, step12VerifyDead
local step13KillPlayer, step14VerifyPlayerKilled

Test.Async("AttackDetailsComponent")

step1VerifyComponent = function()
    Test.Step("AttackDetailsComponent", function()
        Test.AssertTrue(Components.AttackDetails.Exists(attackerId),
            "AttackDetails component should exist after Set")
        local details = Components.AttackDetails.Get(attackerId)
        Test.AssertEqual(2.5, details.AttackRange, "attack range from Set")
        Test.AssertEqual(250000, details.AttackDelayUs, "attack delay from Set")
    end)
    Test.Pass("AttackDetailsComponent")
end

Test.Async("AttackAppliesDamage")

step2Attack = function()
    Combat.Attack(attackerId)
end

step3VerifyDamage = function()
    Test.Step("AttackAppliesDamage", function()
        Test.AssertEqual(4, Components.Health.Get(victimId).Value,
            "target in front and in range should lose 1 health")
    end)
    Test.Pass("AttackAppliesDamage")
end

Test.Async("AttackRateLimited")

step4AttackRateLimited = function()
    Combat.Attack(attackerId)
end

step5VerifyRateLimited = function()
    Test.Step("AttackRateLimited", function()
        Test.AssertEqual(4, Components.Health.Get(victimId).Value,
            "attack within delay window should be rate limited")
    end)
    Test.Pass("AttackRateLimited")
end

Test.Async("AttackAfterDelay")

step6AttackAfterDelay = function()
    Combat.Attack(attackerId)
end

step7VerifySecondDamage = function()
    Test.Step("AttackAfterDelay", function()
        Test.AssertEqual(3, Components.Health.Get(victimId).Value,
            "attack after the delay should deal another 1 damage")
    end)
    Test.Pass("AttackAfterDelay")
end

Test.Async("AttackFacingAndRangeFilter")

step8AttackFiltered = function()
    -- attacker2 has no AttackDetails component, so this exercises the configured defaults.
    Combat.Attack(attacker2Id)
end

step9VerifyFiltered = function()
    Test.Step("AttackFacingAndRangeFilter", function()
        Test.AssertEqual(5, Components.Health.Get(behindTargetId).Value,
            "target behind the attacker should not be damaged")
        Test.AssertEqual(5, Components.Health.Get(farTargetId).Value,
            "target beyond attack range should not be damaged")
    end)
    Test.Pass("AttackFacingAndRangeFilter")
end

Test.Async("AttackToDeathRemovesNpc")

step10AttackToDeath = function()
    -- npcVictimId has 2 health; two attacks kill it.
    Combat.Attack(playerAttackerId)
end

step11AttackAgain = function()
    Combat.Attack(playerAttackerId)
end

step12VerifyDead = function()
    Test.Step("AttackToDeathRemovesNpc", function()
        Test.AssertNil(Components.Health.Get(npcVictimId),
            "dead NPC entity should have been removed")
    end)
    Test.Pass("AttackToDeathRemovesNpc")
end

Test.Async("PlayerKilledEvent")

step13KillPlayer = function()
    -- playerVictimId has 1 health; one attack kills the player.
    Combat.Attack(npcKillerId)
end

step14VerifyPlayerKilled = function()
    Test.Step("PlayerKilledEvent", function()
        Test.AssertEqual(playerVictimId, killedVictim,
            "PlayerKilled event should name the slain player")
        Test.AssertEqual(npcKillerId, killedKiller,
            "PlayerKilled event should name the killer")
        Test.AssertEqual(10, Components.Health.Get(playerVictimId).Value,
            "dead player should have been respawned with full health")
    end)
    Test.Pass("PlayerKilledEvent")
end

-- Suite setup. ------------------------------------------------------------------------

local function OnPlayerKilled(event)
    killedVictim = event.VictimEntityId
    killedKiller = event.KillerEntityId
end

Test.Case("RegisterPlayerKilledCallback", function()
    Test.AssertTrue(Events.Server_Combat_PlayerKilled ~= nil,
        "Server_Combat_PlayerKilled should be defined in the Events table")
    Scripting.AddEventCallback(Events.Server_Combat_PlayerKilled, OnPlayerKilled)
end)

local function Pos(x, y)
    return {
        Position = { X = BaseX + x, Y = BaseY + y, Z = 1.0 },
        Velocity = { X = 0.0, Y = 0.0, Z = 0.0 }
    }
end

setup = function()
    -- Attacker with explicit attack details facing south (default), victim to the south.
    attackerId = Entities.Create({
        Name = "TestCombatAttacker",
        EntityType = EntityType.Npc,
        NonPersistent = true,
        Kinematics = Pos(0.0, 0.5),
        Health = { Value = 10, MaxValue = 10, ChangeRate = 0, ChangeInterval = 0 }
    })
    Components.AttackDetails.Set(attackerId,
        { AttackRange = 2.5, AttackDelayUs = 250000 })

    victimId = Entities.Create({
        Name = "TestCombatVictim",
        EntityType = EntityType.Npc,
        NonPersistent = true,
        Kinematics = Pos(0.0, 0.0),
        Health = { Value = 5, MaxValue = 10, ChangeRate = 0, ChangeInterval = 0 }
    })

    -- Second attacker with no AttackDetails component (uses configured defaults),
    -- a target behind it (north), and one beyond the default range (far south).
    attacker2Id = Entities.Create({
        Name = "TestCombatAttacker2",
        EntityType = EntityType.Npc,
        NonPersistent = true,
        Kinematics = Pos(2.0, 0.5),
        Health = { Value = 10, MaxValue = 10, ChangeRate = 0, ChangeInterval = 0 }
    })

    behindTargetId = Entities.Create({
        Name = "TestCombatBehind",
        EntityType = EntityType.Npc,
        NonPersistent = true,
        Kinematics = Pos(2.0, 1.0),
        Health = { Value = 5, MaxValue = 10, ChangeRate = 0, ChangeInterval = 0 }
    })

    farTargetId = Entities.Create({
        Name = "TestCombatFar",
        EntityType = EntityType.Npc,
        NonPersistent = true,
        Kinematics = Pos(2.0, -3.0),
        Health = { Value = 5, MaxValue = 10, ChangeRate = 0, ChangeInterval = 0 }
    })

    -- Player attacker and NPC victim for the kill-credit path.
    playerAttackerId = Entities.Create({
        Name = "TestCombatPlayer",
        EntityType = EntityType.Player,
        NonPersistent = true,
        Kinematics = Pos(4.0, 0.5),
        Health = { Value = 10, MaxValue = 10, ChangeRate = 0, ChangeInterval = 0 }
    })
    Components.AttackDetails.Set(playerAttackerId,
        { AttackRange = 2.5, AttackDelayUs = 100000 })

    npcVictimId = Entities.Create({
        Name = "TestCombatNpc",
        EntityType = EntityType.Npc,
        NonPersistent = true,
        Kinematics = Pos(4.0, 0.0),
        Health = { Value = 2, MaxValue = 10, ChangeRate = 0, ChangeInterval = 0 }
    })

    -- NPC killer and player victim for the PlayerKilled event path. Placed well beyond
    -- every other attacker's range so only the NPC killer can reach the player victim.
    npcKillerId = Entities.Create({
        Name = "TestCombatKiller",
        EntityType = EntityType.Npc,
        NonPersistent = true,
        Kinematics = Pos(10.0, 0.5),
        Health = { Value = 10, MaxValue = 10, ChangeRate = 0, ChangeInterval = 0 }
    })
    Components.AttackDetails.Set(npcKillerId,
        { AttackRange = 2.5, AttackDelayUs = 100000 })

    playerVictimId = Entities.Create({
        Name = "TestCombatPlayerVictim",
        EntityType = EntityType.Player,
        NonPersistent = true,
        Kinematics = Pos(10.0, 0.0),
        Health = { Value = 1, MaxValue = 10, ChangeRate = 0, ChangeInterval = 0 }
    })

    -- Schedule the test steps. Component updates and combat events are committed at
    -- tick boundaries, so leave ample time between actions and verifications.
    -- Attack #1 (t=1.0) opens a 0.25 s rate limit window on the attacker; the second
    -- attack (t=1.05) falls inside that window and must be rejected, while the third
    -- attack (t=1.6) falls outside it and must land.
    Scripting.AddTimedCallback(0.5, step1VerifyComponent)

    Scripting.AddTimedCallback(1.0, step2Attack)             -- attack #1 (5 -> 4 health)
    Scripting.AddTimedCallback(1.05, step4AttackRateLimited) -- inside window: rejected
    Scripting.AddTimedCallback(1.3, step3VerifyDamage)
    Scripting.AddTimedCallback(1.4, step5VerifyRateLimited)
    Scripting.AddTimedCallback(1.6, step6AttackAfterDelay)   -- outside window: lands (4 -> 3)
    Scripting.AddTimedCallback(1.9, step7VerifySecondDamage)

    Scripting.AddTimedCallback(2.5, step8AttackFiltered)     -- facing/range filtering
    Scripting.AddTimedCallback(2.8, step9VerifyFiltered)

    Scripting.AddTimedCallback(3.2, step10AttackToDeath)     -- kill sequence (2 -> 1)
    Scripting.AddTimedCallback(3.6, step11AttackAgain)       -- (1 -> 0, NPC removed)
    Scripting.AddTimedCallback(4.2, step12VerifyDead)

    Scripting.AddTimedCallback(4.6, step13KillPlayer)        -- player death (1 -> 0)
    Scripting.AddTimedCallback(5.2, step14VerifyPlayerKilled)
end

Scripting.AddTimedCallback(0.3, setup)

Util.LogInfo("[" .. Suite .. "] suite registered")
