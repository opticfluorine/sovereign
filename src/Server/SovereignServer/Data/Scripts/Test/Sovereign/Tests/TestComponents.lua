-- Tests for the Components module (general component functions and typed helpers).

local Suite = "TestComponents"

local setup, step1VerifyCreated, step2Rename, step3VerifyRename, step4SetVelocity
local step5VerifyVelocity, step6AddPosition, step7VerifyPosition
local step8QuantityAdd, step9VerifyQuantityAdd, step10QuantitySubtract
local step11VerifyQuantitySubtract, step12QuantitySet, step13VerifyQuantitySet
local step14QuantityMultiply, step15VerifyQuantityMultiply
local step16QuantityDivide, step17VerifyQuantityDivide
local fixtureEntityId, quantityEntityId
local positionBeforeAdd

Test.Async("GetAfterCreate")

step1VerifyCreated = function()
    Test.Step("GetAfterCreate", function()
        Test.AssertTrue(Components.Name.Exists(fixtureEntityId), "name component should exist")
        Test.AssertEqual("TestComponentsFixture", Components.Name.Get(fixtureEntityId), "name value")
        Test.AssertTrue(Components.Drawable.Exists(fixtureEntityId), "drawable tag should exist")
        Test.AssertTrue(Components.Drawable.Get(fixtureEntityId), "drawable tag should be set")
    end)
    Test.Pass("GetAfterCreate")
end

Test.Async("SetThenGet")

step2Rename = function()
    Components.Name.Set(fixtureEntityId, "TestComponentsRenamed")
end

step3VerifyRename = function()
    -- Component updates are enqueued and take effect on the following tick.
    Test.Step("SetThenGet", function()
        Test.AssertEqual("TestComponentsRenamed", Components.Name.Get(fixtureEntityId),
            "renamed value should be committed")
    end)
    Test.Pass("SetThenGet")
end

Test.Async("KinematicsSetVelocity")

step4SetVelocity = function()
    Components.Kinematics.SetVelocity(fixtureEntityId, {
        Position = { X = 0.0, Y = 0.0, Z = 0.0 },
        Velocity = { X = 1.5, Y = -2.0, Z = 0.25 }
    })
end

step5VerifyVelocity = function()
    Test.Step("KinematicsSetVelocity", function()
        local kin = Components.Kinematics.Get(fixtureEntityId)
        Test.AssertNear(1.5, kin.Velocity.X, 0.0001, "velocity X after SetVelocity")
        Test.AssertNear(-2.0, kin.Velocity.Y, 0.0001, "velocity Y after SetVelocity")
        Test.AssertNear(0.25, kin.Velocity.Z, 0.0001, "velocity Z after SetVelocity")
    end)
    -- Stop the fixture so that it does not drift during later steps.
    Components.Kinematics.SetVelocity(fixtureEntityId, {
        Position = { X = 0.0, Y = 0.0, Z = 0.0 },
        Velocity = { X = 0.0, Y = 0.0, Z = 0.0 }
    })
    Test.Pass("KinematicsSetVelocity")
end

Test.Async("KinematicsAddPosition")

step6AddPosition = function()
    -- Record the current position so that the shift can be verified relative to it.
    local kin = Components.Kinematics.Get(fixtureEntityId)
    positionBeforeAdd = kin.Position
    Components.Kinematics.AddPosition(fixtureEntityId, {
        Position = { X = 0.0, Y = 0.0, Z = 1.0 },
        Velocity = { X = 0.0, Y = 0.0, Z = 0.0 }
    })
end

step7VerifyPosition = function()
    Test.Step("KinematicsAddPosition", function()
        local kin = Components.Kinematics.Get(fixtureEntityId)
        Test.AssertNear(positionBeforeAdd.X, kin.Position.X, 0.0001, "position X unchanged by AddPosition")
        Test.AssertNear(positionBeforeAdd.Y, kin.Position.Y, 0.0001, "position Y unchanged by AddPosition")
        Test.AssertNear(positionBeforeAdd.Z + 1.0, kin.Position.Z, 0.0001, "position Z shifted by AddPosition")
    end)
    Test.Pass("KinematicsAddPosition")
end

Test.Async("QuantityArithmetic")

step8QuantityAdd = function()
    Components.Quantity.AddNoOverflow(quantityEntityId, 1000000)
end

step9VerifyQuantityAdd = function()
    Test.Step("QuantityArithmetic", function()
        Test.AssertEqual(1000005, Components.Quantity.Get(quantityEntityId),
            "quantity after AddNoOverflow")
    end)
end

step10QuantitySubtract = function()
    Components.Quantity.SubtractNoUnderflow(quantityEntityId, 2000000)
end

step11VerifyQuantitySubtract = function()
    Test.Step("QuantityArithmetic", function()
        Test.AssertEqual(0, Components.Quantity.Get(quantityEntityId),
            "quantity after SubtractNoUnderflow should clamp at zero")
    end)
end

step12QuantitySet = function()
    Components.Quantity.Set(quantityEntityId, 10)
end

step13VerifyQuantitySet = function()
    Test.Step("QuantityArithmetic", function()
        Test.AssertEqual(10, Components.Quantity.Get(quantityEntityId), "quantity after Set")
    end)
end

step14QuantityMultiply = function()
    Components.Quantity.Multiply(quantityEntityId, 3)
end

step15VerifyQuantityMultiply = function()
    Test.Step("QuantityArithmetic", function()
        Test.AssertEqual(30, Components.Quantity.Get(quantityEntityId), "quantity after Multiply")
    end)
end

step16QuantityDivide = function()
    Components.Quantity.Divide(quantityEntityId, 3)
end

step17VerifyQuantityDivide = function()
    Test.Step("QuantityArithmetic", function()
        Test.AssertEqual(10, Components.Quantity.Get(quantityEntityId), "quantity after Divide")
    end)
    Test.Pass("QuantityArithmetic")
end

-- Suite setup. ------------------------------------------------------------------------

-- Fixture creation is deferred to stagger startup across suites; the engine does not
-- currently serialize concurrent engine mutations from parallel script hosts.
setup = function()
    fixtureEntityId = Entities.Create({
        Name = "TestComponentsFixture",
        EntityType = EntityType.Npc,
        NonPersistent = true,
        Drawable = { X = 0.0, Y = 0.0 },
        Kinematics = {
            Position = { X = 0.5, Y = 0.5, Z = 1.0 },
            Velocity = { X = 0.0, Y = 0.0, Z = 0.0 }
        }
    })

    quantityEntityId = Entities.Create({
        Name = "TestComponentsQuantity",
        EntityType = EntityType.Item,
        NonPersistent = true,
        Quantity = 5
    })

    Scripting.AddTimedCallback(0.3, step1VerifyCreated)
    Scripting.AddTimedCallback(0.5, step2Rename)
    Scripting.AddTimedCallback(0.7, step3VerifyRename)
    Scripting.AddTimedCallback(0.9, step4SetVelocity)
    Scripting.AddTimedCallback(1.1, step5VerifyVelocity)
    Scripting.AddTimedCallback(1.3, step6AddPosition)
    Scripting.AddTimedCallback(1.5, step7VerifyPosition)
    Scripting.AddTimedCallback(0.5, step8QuantityAdd)
    Scripting.AddTimedCallback(0.7, step9VerifyQuantityAdd)
    Scripting.AddTimedCallback(0.9, step10QuantitySubtract)
    Scripting.AddTimedCallback(1.1, step11VerifyQuantitySubtract)
    Scripting.AddTimedCallback(1.3, step12QuantitySet)
    Scripting.AddTimedCallback(1.5, step13VerifyQuantitySet)
    Scripting.AddTimedCallback(1.7, step14QuantityMultiply)
    Scripting.AddTimedCallback(1.9, step15VerifyQuantityMultiply)
    Scripting.AddTimedCallback(2.1, step16QuantityDivide)
    Scripting.AddTimedCallback(2.3, step17VerifyQuantityDivide)
end

Scripting.AddTimedCallback(0.3, setup)

Util.LogInfo("[" .. Suite .. "] suite registered")
