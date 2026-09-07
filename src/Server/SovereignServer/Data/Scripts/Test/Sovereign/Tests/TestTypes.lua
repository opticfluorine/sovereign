-- Tests for script data types (Vector2, Vector3, GridPosition, Kinematics) via
-- component set/get roundtrips, plus fuzzy match result shapes.

local Suite = "TestTypes"

local setup, step1VerifyVector3, step2VerifyMoving, step3SetDrawable, step4VerifyDrawable
local step5CreateBlock, step6VerifyGridPosition, step7RemoveBlock
local fixtureEntityId, movingEntityId, blockEntityId

Test.Async("Vector3AndKinematics")

step1VerifyVector3 = function()
    Test.Step("Vector3AndKinematics", function()
        local kin = Components.Kinematics.Get(fixtureEntityId)
        Test.AssertNear(1.0, kin.Position.X, 0.0001, "Vector3 position X roundtrip")
        Test.AssertNear(2.0, kin.Position.Y, 0.0001, "Vector3 position Y roundtrip")
        Test.AssertNear(1.5, kin.Position.Z, 0.0001, "Vector3 position Z roundtrip")
        Test.AssertNear(0.0, kin.Velocity.X, 0.0001, "Vector3 velocity X roundtrip")
        Test.AssertNear(0.0, kin.Velocity.Y, 0.0001, "Vector3 velocity Y roundtrip")
        Test.AssertNear(0.0, kin.Velocity.Z, 0.0001, "Vector3 velocity Z roundtrip")
    end)
    Test.Pass("Vector3AndKinematics")
end

Test.Async("KinematicsVelocityRoundtrip")

step2VerifyMoving = function()
    Test.Step("KinematicsVelocityRoundtrip", function()
        -- The position of a moving entity is integrated every tick, so only the
        -- velocity is asserted here.
        local kin = Components.Kinematics.Get(movingEntityId)
        Test.AssertNear(0.5, kin.Velocity.X, 0.0001, "velocity X roundtrip")
        Test.AssertNear(0.25, kin.Velocity.Y, 0.0001, "velocity Y roundtrip")
        Test.AssertNear(-0.125, kin.Velocity.Z, 0.0001, "velocity Z roundtrip")
    end)
    Test.Pass("KinematicsVelocityRoundtrip")
end

Test.Async("Vector2Drawable")

step3SetDrawable = function()
    Components.Drawable.Set(fixtureEntityId, { X = 0.25, Y = 0.75 })
end

step4VerifyDrawable = function()
    Test.Step("Vector2Drawable", function()
        local drawable = Components.Drawable.Get(fixtureEntityId)
        Test.AssertNear(0.25, drawable.X, 0.0001, "Vector2 drawable X roundtrip")
        Test.AssertNear(0.75, drawable.Y, 0.0001, "Vector2 drawable Y roundtrip")
    end)
    Test.Pass("Vector2Drawable")
end

Test.Async("GridPositionBlock")

step5CreateBlock = function()
    blockEntityId = Entities.Create({
        BlockPosition = { X = 10, Y = 10, Z = 10 },
        NonPersistent = true
    })
end

step6VerifyGridPosition = function()
    Test.Step("GridPositionBlock", function()
        Test.AssertTrue(blockEntityId ~= nil and blockEntityId ~= Entities.ToEntityId(0),
            "block entity should be created")
        local pos = Components.BlockPosition.Get(blockEntityId)
        Test.AssertEqual(10, pos.X, "GridPosition X roundtrip")
        Test.AssertEqual(10, pos.Y, "GridPosition Y roundtrip")
        Test.AssertEqual(10, pos.Z, "GridPosition Z roundtrip")
    end)
end

step7RemoveBlock = function()
    Entities.Remove(blockEntityId)
    Test.Pass("GridPositionBlock")
end

Test.Case("ItemTemplateMatchShape", function()
    local matches = Items.FindByFuzzyName("Sword", 1)
    Test.AssertTrue(#matches > 0, "sword item template should exist in the database")
    local match = matches[1]
    Test.AssertTrue(match.EntityId ~= nil, "ItemTemplateMatch should have EntityId")
    Test.AssertTrue(match.Name ~= nil, "ItemTemplateMatch should have Name")
    Test.AssertTrue(match.Score ~= nil, "ItemTemplateMatch should have Score")
end)

Test.Case("PlayerNameMatchShape", function()
    local matches = Players.FindByFuzzyName("Nobody", 1)
    Test.AssertEqual(0, #matches, "PlayerNameMatch list should be empty with no players online")
end)

-- Suite setup. ------------------------------------------------------------------------

-- Fixture creation is deferred to stagger startup across suites; the engine does not
-- currently serialize concurrent engine mutations from parallel script hosts.
setup = function()
    fixtureEntityId = Entities.Create({
        Name = "TestTypesFixture",
        EntityType = EntityType.Npc,
        NonPersistent = true,
        Kinematics = {
            Position = { X = 1.0, Y = 2.0, Z = 1.5 },
            Velocity = { X = 0.0, Y = 0.0, Z = 0.0 }
        }
    })

    movingEntityId = Entities.Create({
        Name = "TestTypesMovingFixture",
        EntityType = EntityType.Npc,
        NonPersistent = true,
        Kinematics = {
            Position = { X = 5.0, Y = 5.0, Z = 1.5 },
            Velocity = { X = 0.5, Y = 0.25, Z = -0.125 }
        }
    })

    Scripting.AddTimedCallback(0.3, step1VerifyVector3)
    Scripting.AddTimedCallback(0.3, step2VerifyMoving)
    Scripting.AddTimedCallback(0.5, step3SetDrawable)
    Scripting.AddTimedCallback(0.7, step4VerifyDrawable)
    Scripting.AddTimedCallback(0.5, step5CreateBlock)
    Scripting.AddTimedCallback(0.7, step6VerifyGridPosition)
    Scripting.AddTimedCallback(0.9, step7RemoveBlock)
end

Scripting.AddTimedCallback(0.6, setup)

Util.LogInfo("[" .. Suite .. "] suite registered")
