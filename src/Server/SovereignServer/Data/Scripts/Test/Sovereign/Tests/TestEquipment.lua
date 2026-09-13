-- Tests for the equipment system.
-- Requires the stackable "Sword" item template in the database (Items.FindByName).
-- Exercises Inventory.Equip/Unequip/GetEquipment, equipment slot creation via
-- Entities.Create, the Components.EquipmentType binding, and Entities.None.

local Suite = "TestEquipment"

local setup, s1bVerifySlots
local s2AddSword1, s3Equip, s4VerifyEquip
local s5AddSword2, s6EquipSwap, s7VerifySwap
local s8Unequip, s9VerifyUnequip
local s10AddPlain, s11EquipEmptySlot, s12EquipPlain, s13VerifyEquipNoOps
local s14UnequipOccupied, s15UnequipInvalidSlot, s16UnequipNothing, s17VerifyUnequipNoOps
local s18AddHelm, s19EquipNoSlot, s20VerifyNoSlot
local s21CreateSetItem, s22VerifyGet, s23SetOffhand, s24VerifySet
local actorId, actorBId, swordTemplateId
local sword1, sword2, plainItem, helmItem, setItem

Test.Async("EquipmentFixture")

s1bVerifySlots = function()
    Test.Step("EquipmentFixture", function()
        Test.AssertTrue(swordTemplateId ~= nil, "sword item template should exist in the database")
        Test.AssertEqual(4, Inventory.GetSlotCount(actorId), "actor slot count")
        Test.AssertEqual(2, Inventory.GetSlotCount(actorBId), "actor B slot count")
        Test.AssertEqual(Entities.None, Inventory.GetEquipment(actorId, EquipmentType.Weapon),
            "weapon slot should start empty")
    end)
    Test.Pass("EquipmentFixture")
end

Test.Async("EquipMovesItem")

s2AddSword1 = function()
    Test.AssertTrue(Inventory.AddItem(actorId, sword1), "AddItem of sword1 should succeed")
end

s3Equip = function()
    Inventory.Equip(actorId, 1)
end

s4VerifyEquip = function()
    Test.Step("EquipMovesItem", function()
        Test.AssertEqual(sword1, Inventory.GetEquipment(actorId, EquipmentType.Weapon), "equipped weapon")
        Test.AssertEqual(Entities.None, Inventory.GetItem(actorId, 1), "source slot should be empty")
    end)
    Test.Pass("EquipMovesItem")
end

Test.Async("EquipSwapMovesPrevious")

s5AddSword2 = function()
    Test.AssertTrue(Inventory.AddItem(actorId, sword2), "AddItem of sword2 should succeed")
end

s6EquipSwap = function()
    -- sword1 is currently equipped; equipping sword2 swaps sword1 back into the source slot.
    Inventory.Equip(actorId, 1)
end

s7VerifySwap = function()
    Test.Step("EquipSwapMovesPrevious", function()
        Test.AssertEqual(sword2, Inventory.GetEquipment(actorId, EquipmentType.Weapon), "equipped weapon after swap")
        Test.AssertEqual(sword1, Inventory.GetItem(actorId, 1), "previous weapon should move to source slot")
    end)
    Test.Pass("EquipSwapMovesPrevious")
end

Test.Async("UnequipMovesItem")

s8Unequip = function()
    Inventory.Unequip(actorId, EquipmentType.Weapon, 3)
end

s9VerifyUnequip = function()
    Test.Step("UnequipMovesItem", function()
        Test.AssertEqual(Entities.None, Inventory.GetEquipment(actorId, EquipmentType.Weapon),
            "weapon slot should be empty after unequip")
        Test.AssertEqual(sword2, Inventory.GetItem(actorId, 3), "sword2 should move to slot 3")
    end)
    Test.Pass("UnequipMovesItem")
end

Test.Async("EquipNoOps")

s10AddPlain = function()
    Test.AssertTrue(Inventory.AddItem(actorId, plainItem), "AddItem of plain item should succeed")
end

s11EquipEmptySlot = function()
    -- Equipping from an empty slot is a no-op.
    Inventory.Equip(actorId, 4)
end

s12EquipPlain = function()
    -- The plain item has no EquipmentType component, so it cannot be equipped.
    Inventory.Equip(actorId, 2)
end

s13VerifyEquipNoOps = function()
    Test.Step("EquipNoOps", function()
        Test.AssertEqual(plainItem, Inventory.GetItem(actorId, 2), "plain item should remain in slot 2")
        Test.AssertEqual(Entities.None, Inventory.GetItem(actorId, 4), "empty slot should remain empty")
        Test.AssertEqual(Entities.None, Inventory.GetEquipment(actorId, EquipmentType.Weapon),
            "weapon slot should still be empty (sword2 was unequipped earlier)")
    end)
    Test.Pass("EquipNoOps")
end

Test.Async("UnequipNoOps")

s13bReequip = function()
    -- Re-equip sword1 from slot 1 so that a weapon is equipped for the unequip no-op tests.
    Inventory.Equip(actorId, 1)
end

s14UnequipOccupied = function()
    -- The target slot holds the plain item, so the unequip is rejected.
    Inventory.Unequip(actorId, EquipmentType.Weapon, 2)
end

s15UnequipInvalidSlot = function()
    -- The target slot does not exist, so the unequip is a no-op.
    Inventory.Unequip(actorId, EquipmentType.Weapon, 99)
end

s16UnequipNothing = function()
    -- Nothing is equipped in the helmet slot, so the unequip is a no-op.
    Inventory.Unequip(actorId, EquipmentType.Helmet, 4)
end

s17VerifyUnequipNoOps = function()
    Test.Step("UnequipNoOps", function()
        Test.AssertEqual(sword1, Inventory.GetEquipment(actorId, EquipmentType.Weapon),
            "sword1 should remain equipped")
        Test.AssertEqual(Entities.None, Inventory.GetItem(actorId, 1), "slot 1 should be empty after re-equip")
        Test.AssertEqual(plainItem, Inventory.GetItem(actorId, 2), "slot 2 should still hold the plain item")
        Test.AssertEqual(Entities.None, Inventory.GetItem(actorId, 4), "slot 4 should remain empty")
    end)
    Test.Pass("UnequipNoOps")
end

Test.Async("EquipNoMatchingSlot")

s18AddHelm = function()
    Test.AssertTrue(Inventory.AddItem(actorBId, helmItem), "AddItem of helm should succeed")
end

s19EquipNoSlot = function()
    -- Actor B has no equipment slots, so the equip is a no-op.
    Inventory.Equip(actorBId, 1)
end

s20VerifyNoSlot = function()
    Test.Step("EquipNoMatchingSlot", function()
        Test.AssertEqual(helmItem, Inventory.GetItem(actorBId, 1), "helm should remain in slot 1")
        Test.AssertEqual(Entities.None, Inventory.GetEquipment(actorBId, EquipmentType.Helmet),
            "actor B should have nothing equipped")
    end)
    Test.Pass("EquipNoMatchingSlot")
end

Test.Async("EquipmentComponentSanity")

s21CreateSetItem = function()
    setItem = Entities.Create({ Template = swordTemplateId, NonPersistent = true,
        EquipmentType = EquipmentType.Weapon })
end

s22VerifyGet = function()
    Test.Step("EquipmentComponentSanity", function()
        Test.AssertEqual(EquipmentType.Weapon, Components.EquipmentType.Get(setItem), "initial equipment type")
        Test.AssertEqual(Entities.ToEntityId(0), Entities.None, "Entities.None should be entity ID 0")
    end)
end

s23SetOffhand = function()
    Components.EquipmentType.Set(setItem, EquipmentType.Offhand)
end

s24VerifySet = function()
    Test.Step("EquipmentComponentSanity", function()
        Test.AssertEqual(EquipmentType.Offhand, Components.EquipmentType.Get(setItem), "equipment type after Set")
    end)
    Test.Pass("EquipmentComponentSanity")
end

-- Suite setup. ------------------------------------------------------------------------

-- Fixture creation is deferred to stagger startup across suites; the engine does not
-- currently serialize concurrent engine mutations from parallel script hosts.
setup = function()
    local swords = Items.FindByName("Sword")
    swordTemplateId = swords[1]

    actorId = Entities.Create({
        Name = "TestEquipmentActor",
        EntityType = EntityType.Npc,
        NonPersistent = true
    })

    actorBId = Entities.Create({
        Name = "TestEquipmentActorB",
        EntityType = EntityType.Npc,
        NonPersistent = true
    })

    -- One equipment slot per type on actor A.
    local slotTypes = {
        EquipmentType.Weapon, EquipmentType.Offhand, EquipmentType.Helmet, EquipmentType.Chest,
        EquipmentType.Leggings, EquipmentType.Necklace, EquipmentType.LeftRing, EquipmentType.RightRing
    }
    for _, slotType in ipairs(slotTypes) do
        Entities.Create({
            EntityType = EntityType.EquipmentSlot,
            Parent = actorId,
            EquipmentType = slotType,
            NonPersistent = true
        })
    end

    sword1 = Entities.Create({ Template = swordTemplateId, NonPersistent = true,
        EquipmentType = EquipmentType.Weapon })
    sword2 = Entities.Create({ Template = swordTemplateId, NonPersistent = true,
        EquipmentType = EquipmentType.Weapon })
    helmItem = Entities.Create({ Template = swordTemplateId, NonPersistent = true,
        EquipmentType = EquipmentType.Helmet })

    -- The plain item has no EquipmentType component, so it cannot be equipped.
    plainItem = Entities.Create({ Template = swordTemplateId, NonPersistent = true })

    Inventory.AddSlots(actorId, 4)
    Inventory.AddSlots(actorBId, 2)

    Scripting.AddTimedCallback(0.4, s1bVerifySlots)
    Scripting.AddTimedCallback(0.6, s2AddSword1)
    Scripting.AddTimedCallback(0.8, s3Equip)
    Scripting.AddTimedCallback(1.0, s4VerifyEquip)
    Scripting.AddTimedCallback(1.0, s5AddSword2)
    Scripting.AddTimedCallback(1.2, s6EquipSwap)
    Scripting.AddTimedCallback(1.4, s7VerifySwap)
    Scripting.AddTimedCallback(1.6, s8Unequip)
    Scripting.AddTimedCallback(1.8, s9VerifyUnequip)
    Scripting.AddTimedCallback(1.8, s10AddPlain)
    Scripting.AddTimedCallback(2.0, s11EquipEmptySlot)
    Scripting.AddTimedCallback(2.2, s12EquipPlain)
    Scripting.AddTimedCallback(2.4, s13VerifyEquipNoOps)
    Scripting.AddTimedCallback(2.6, s13bReequip)
    Scripting.AddTimedCallback(2.8, s14UnequipOccupied)
    Scripting.AddTimedCallback(3.0, s15UnequipInvalidSlot)
    Scripting.AddTimedCallback(3.2, s16UnequipNothing)
    Scripting.AddTimedCallback(3.4, s17VerifyUnequipNoOps)
    Scripting.AddTimedCallback(0.6, s18AddHelm)
    Scripting.AddTimedCallback(0.8, s19EquipNoSlot)
    Scripting.AddTimedCallback(1.0, s20VerifyNoSlot)
    Scripting.AddTimedCallback(0.6, s21CreateSetItem)
    Scripting.AddTimedCallback(0.8, s22VerifyGet)
    Scripting.AddTimedCallback(1.0, s23SetOffhand)
    Scripting.AddTimedCallback(1.2, s24VerifySet)
end

Scripting.AddTimedCallback(0.5, setup)

Util.LogInfo("[" .. Suite .. "] suite registered")
