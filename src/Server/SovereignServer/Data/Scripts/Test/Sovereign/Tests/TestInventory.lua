-- Tests for the Inventory module.
-- Requires item templates in the database: a stackable "Sword" and a
-- non-stackable "Shield" (Items.FindByName). The actor accesses chest
-- inventories through the Chest NPC flag, mirroring player behavior.

local Suite = "TestInventory"

local setup, s1CreateItems, s1bVerifySlots, s2AddSword1, s3AddShield1, s4AddSword2
local s5VerifyContents, s6Swap, s7VerifySwap, s8SwapAsActor, s9VerifySwapAsActor
local s10AddSword3, s11VerifySword3, s12SwapQuantity, s13VerifySwapQuantity
local s14Merge, s15VerifyMerge
local s16AddSword4, s17Consume, s18VerifyConsume
local s19AddShield2, s20RemoveItem, s21VerifyRemove
local s22AddTool, s23VerifyTool, s24AddPlainItem, s25UseOutOfRange
local s26UseWithoutUseRange, s27UseInRangeNoKeys
local actorId, chestAId, chestBId, swordTemplateId, shieldTemplateId
local sword1, sword2, sword3, sword4, shield1, shield2
local useItemActor, toolItem, plainItem, interactTarget, farTarget

Test.Async("AddSlotsAndSlotCount")

s1bVerifySlots = function()
    Test.Step("AddSlotsAndSlotCount", function()
        Test.AssertTrue(swordTemplateId ~= nil, "sword item template should exist in the database")
        Test.AssertTrue(shieldTemplateId ~= nil, "shield item template should exist in the database")
        Test.AssertEqual(8, Inventory.GetSlotCount(actorId), "actor slot count")
        Test.AssertEqual(4, Inventory.GetSlotCount(chestAId), "chest A slot count")
        Test.AssertEqual(4, Inventory.GetSlotCount(chestBId), "chest B slot count")
    end)
    Test.Pass("AddSlotsAndSlotCount")
end

Test.Async("AddItemAndGetItem")

s2AddSword1 = function()
    Test.AssertTrue(Inventory.AddItem(actorId, sword1), "first AddItem should succeed")
end

s3AddShield1 = function()
    Test.AssertTrue(Inventory.AddItem(actorId, shield1), "second AddItem should succeed")
end

s4AddSword2 = function()
    Test.AssertTrue(Inventory.AddItem(actorId, sword2), "third AddItem should succeed")
end

s5VerifyContents = function()
    Test.Step("AddItemAndGetItem", function()
        Test.AssertEqual(sword1, Inventory.GetItem(actorId, 1), "slot 1 should hold sword1")
        Test.AssertEqual(shield1, Inventory.GetItem(actorId, 2), "slot 2 should hold shield1")
        Test.AssertEqual(sword2, Inventory.GetItem(actorId, 3), "slot 3 should hold sword2")
        Test.AssertEqual(Entities.ToEntityId(0), Inventory.GetItem(chestAId, 1), "chest A should start empty")
        Test.AssertEqual(2, Inventory.GetSlotIndexForItem(actorId, shield1), "shield1 slot index")
        Test.AssertEqual(sword1, Inventory.FindFirstMatchingItem(actorId, swordTemplateId),
            "first matching sword should be sword1")
        -- The generated binding reads the entity ID from the top of the stack, so the
        -- entity ID is the second argument; the first argument is a placeholder.
        local inv = Inventory.GetInventory({}, actorId)
        Test.AssertEqual(sword1, inv[1], "GetInventory slot 1")
        Test.AssertEqual(shield1, inv[2], "GetInventory slot 2")
        Test.AssertEqual(sword2, inv[3], "GetInventory slot 3")
        Test.AssertEqual(Entities.ToEntityId(0), inv[4], "GetInventory empty slot")
    end)
    Test.Pass("AddItemAndGetItem")
end

Test.Async("Swap")

s6Swap = function()
    -- Different templates, full stacks: the slots exchange contents.
    Inventory.Swap(actorId, 1, 2)
end

s7VerifySwap = function()
    Test.Step("Swap", function()
        Test.AssertEqual(shield1, Inventory.GetItem(actorId, 1), "slot 1 after swap")
        Test.AssertEqual(sword1, Inventory.GetItem(actorId, 2), "slot 2 after swap")
    end)
    Test.Pass("Swap")
end

Test.Async("SwapAsActor")

s8SwapAsActor = function()
    -- Move shield1 (full stack) from the actor's inventory to an empty slot in chest A.
    Inventory.SwapAsActor(actorId, actorId, 1, chestAId, 1)
end

s9VerifySwapAsActor = function()
    Test.Step("SwapAsActor", function()
        Test.AssertEqual(shield1, Inventory.GetItem(chestAId, 1), "shield1 should move to chest A")
        Test.AssertEqual(Entities.ToEntityId(0), Inventory.GetItem(actorId, 1), "actor slot 1 should be empty")
    end)
    Test.Pass("SwapAsActor")
end

Test.Async("SwapQuantityAsActor")

s10AddSword3 = function()
    Test.AssertTrue(Inventory.AddItem(chestAId, sword3), "AddItem to chest A should succeed")
end

s11VerifySword3 = function()
    Test.Step("SwapQuantityAsActor", function()
        Test.AssertEqual(sword3, Inventory.GetItem(chestAId, 2), "sword3 should be in chest A slot 2")
    end)
end

s12SwapQuantity = function()
    -- Move part of the sword1 stack (quantity 10) from the actor's slot 2 to the
    -- empty chest B slot 3.
    Inventory.SwapQuantityAsActor(actorId, actorId, 2, chestBId, 3, 4)
end

s13VerifySwapQuantity = function()
    Test.Step("SwapQuantityAsActor", function()
        local splitItem = Inventory.GetItem(chestBId, 3)
        Test.AssertTrue(splitItem ~= Entities.ToEntityId(0), "destination slot should hold the split stack")
        Test.AssertEqual(4, Components.Quantity.Get(splitItem), "split stack quantity")
        Test.AssertEqual(sword1, Inventory.GetItem(actorId, 2), "source slot should still hold sword1")
        Test.AssertEqual(6, Components.Quantity.Get(sword1), "source stack quantity after split")
    end)
    Test.Pass("SwapQuantityAsActor")
end

Test.Async("SwapMerge")

s14Merge = function()
    -- Swapping two stacks of the same template merges them: the source stack is
    -- destroyed and the destination quantity increments.
    Inventory.SwapAsActor(actorId, actorId, 2, chestAId, 2)
end

s15VerifyMerge = function()
    Test.Step("SwapMerge", function()
        Test.AssertEqual(Entities.ToEntityId(0), Inventory.GetItem(actorId, 2), "merged source slot should be empty")
        Test.AssertEqual(sword3, Inventory.GetItem(chestAId, 2), "merged destination slot should hold sword3")
        Test.AssertEqual(7, Components.Quantity.Get(sword3), "sword3 quantity after merge (1 + 6)")
    end)
    Test.Pass("SwapMerge")
end

Test.Async("ConsumeItem")

s16AddSword4 = function()
    Test.AssertTrue(Inventory.AddItem(actorId, sword4), "AddItem of sword4 should succeed")
end

s17Consume = function()
    Test.Step("ConsumeItem", function()
        Test.AssertTrue(Inventory.ConsumeItem(actorId, swordTemplateId),
            "ConsumeItem should consume a sword")
        Test.AssertTrue(not Inventory.ConsumeItemQuantity(actorId, swordTemplateId, 11),
            "ConsumeItemQuantity with insufficient swords should fail")
        Test.AssertTrue(Inventory.ConsumeItemQuantity(actorId, swordTemplateId, 1),
            "ConsumeItemQuantity should consume the remaining sword")
        Test.AssertTrue(not Inventory.ConsumeItemQuantity(actorId, swordTemplateId, 1),
            "ConsumeItemQuantity with no swords left should fail")
    end)
end

s18VerifyConsume = function()
    Test.Step("ConsumeItem", function()
        Test.AssertEqual(Entities.ToEntityId(0), Inventory.GetItem(actorId, 1), "slot 1 should be empty after consumption")
        Test.AssertEqual(Entities.ToEntityId(0), Inventory.GetItem(actorId, 3), "slot 3 should be empty after consumption")
    end)
    Test.Pass("ConsumeItem")
end

Test.Async("RemoveItem")

s19AddShield2 = function()
    Test.AssertTrue(Inventory.AddItem(actorId, shield2), "AddItem of shield2 should succeed")
end

s20RemoveItem = function()
    Test.Step("RemoveItem", function()
        Test.AssertEqual(shield2, Inventory.GetItem(actorId, 1), "shield2 should be in slot 1")
    end)
    Inventory.RemoveItem(actorId, 1)
end

s21VerifyRemove = function()
    Test.Step("RemoveItem", function()
        Test.AssertEqual(Entities.ToEntityId(0), Inventory.GetItem(actorId, 1), "slot 1 should be empty after RemoveItem")
    end)
    Test.Pass("RemoveItem")
end

Test.Async("UseItem")

s22AddTool = function()
    Test.AssertTrue(Inventory.AddItem(useItemActor, toolItem), "AddItem of tool should succeed")
end

s23VerifyTool = function()
    Test.Step("UseItem", function()
        Test.AssertEqual(toolItem, Inventory.GetItem(useItemActor, 1), "tool should be in hotbar slot 1")
        Test.AssertNear(5.0, Components.UseRange.Get(toolItem), 0.0001, "tool use range")
        Test.AssertTrue(not Components.UseRange.Exists(sword1),
            "item should not inherit UseRange from a template that lacks it")
    end)
end

s24AddPlainItem = function()
    Test.AssertTrue(Inventory.AddItem(useItemActor, plainItem), "AddItem of plain item should succeed")
end

s25UseOutOfRange = function()
    -- Tool is in the hotbar with a UseRange component, but the target is out of range.
    -- The use is rejected; this step completes as long as the rejection is graceful.
    Inventory.UseItem(useItemActor, toolItem, farTarget)
end

s26UseWithoutUseRange = function()
    -- The plain item has no UseRange component, so its use is rejected.
    Inventory.UseItem(useItemActor, plainItem, interactTarget)
end

s27UseInRangeNoKeys = function()
    -- In-range use of the tool against a target with no interact keys is a silent no-op.
    Inventory.UseItem(useItemActor, toolItem, interactTarget)
    Test.Step("UseItem", function()
        Test.AssertEqual(toolItem, Inventory.GetItem(useItemActor, 1), "tool should remain in slot 1")
        Test.AssertEqual(plainItem, Inventory.GetItem(useItemActor, 2), "plain item should remain in slot 2")
    end)
    Test.Pass("UseItem")
end

-- Suite setup. ------------------------------------------------------------------------

-- Fixture creation is deferred to stagger startup across suites; the engine does not
-- currently serialize concurrent engine mutations from parallel script hosts.
setup = function()
    local swords = Items.FindByName("Sword")
    swordTemplateId = swords[1]
    local shields = Items.FindByName("Shield")
    shieldTemplateId = shields[1]

    actorId = Entities.Create({
        Name = "TestInventoryActor",
        EntityType = EntityType.Npc,
        NonPersistent = true,
        Kinematics = {
            Position = { X = 0.5, Y = 0.5, Z = 1.0 },
            Velocity = { X = 0.0, Y = 0.0, Z = 0.0 }
        }
    })

    chestAId = Entities.Create({
        Name = "TestInventoryChestA",
        EntityType = EntityType.Npc,
        NonPersistent = true,
        NpcFlags = NpcFlag.Chest,
        Kinematics = {
            Position = { X = 1.5, Y = 0.5, Z = 1.0 },
            Velocity = { X = 0.0, Y = 0.0, Z = 0.0 }
        }
    })

    chestBId = Entities.Create({
        Name = "TestInventoryChestB",
        EntityType = EntityType.Npc,
        NonPersistent = true,
        NpcFlags = NpcFlag.Chest,
        Kinematics = {
            Position = { X = 2.5, Y = 0.5, Z = 1.0 },
            Velocity = { X = 0.0, Y = 0.0, Z = 0.0 }
        }
    })

    useItemActor = Entities.Create({
        Name = "TestInventoryUseItemActor",
        EntityType = EntityType.Npc,
        NonPersistent = true,
        Kinematics = {
            Position = { X = 10.5, Y = 0.5, Z = 1.0 },
            Velocity = { X = 0.0, Y = 0.0, Z = 0.0 }
        }
    })

    -- The UseRange spec key exercises the scriptable entity builder action.
    toolItem = Entities.Create({
        Template = swordTemplateId,
        NonPersistent = true,
        UseRange = 5.0
    })

    interactTarget = Entities.Create({
        Name = "TestInventoryInteractTarget",
        EntityType = EntityType.Npc,
        NonPersistent = true,
        Kinematics = {
            Position = { X = 11.5, Y = 0.5, Z = 1.0 },
            Velocity = { X = 0.0, Y = 0.0, Z = 0.0 }
        }
    })

    farTarget = Entities.Create({
        Name = "TestInventoryFarTarget",
        EntityType = EntityType.Npc,
        NonPersistent = true,
        Kinematics = {
            Position = { X = 20.5, Y = 0.5, Z = 1.0 },
            Velocity = { X = 0.0, Y = 0.0, Z = 0.0 }
        }
    })

    Inventory.AddSlots(actorId, 8)
    Inventory.AddSlots(chestAId, 4)
    Inventory.AddSlots(chestBId, 4)
    Inventory.AddSlots(useItemActor, 4)

    Scripting.AddTimedCallback(0.4, s1CreateItems)
    Scripting.AddTimedCallback(0.6, s1bVerifySlots)
    Scripting.AddTimedCallback(0.9, s2AddSword1)
    Scripting.AddTimedCallback(1.1, s3AddShield1)
    Scripting.AddTimedCallback(1.3, s4AddSword2)
    Scripting.AddTimedCallback(1.5, s5VerifyContents)
    Scripting.AddTimedCallback(1.7, s6Swap)
    Scripting.AddTimedCallback(1.9, s7VerifySwap)
    Scripting.AddTimedCallback(2.1, s8SwapAsActor)
    Scripting.AddTimedCallback(2.3, s9VerifySwapAsActor)
    Scripting.AddTimedCallback(2.5, s10AddSword3)
    Scripting.AddTimedCallback(2.7, s11VerifySword3)
    Scripting.AddTimedCallback(2.9, s12SwapQuantity)
    Scripting.AddTimedCallback(3.1, s13VerifySwapQuantity)
    Scripting.AddTimedCallback(3.3, s14Merge)
    Scripting.AddTimedCallback(3.5, s15VerifyMerge)
    Scripting.AddTimedCallback(3.7, s16AddSword4)
    Scripting.AddTimedCallback(3.9, s17Consume)
    Scripting.AddTimedCallback(4.1, s18VerifyConsume)
    Scripting.AddTimedCallback(4.3, s19AddShield2)
    Scripting.AddTimedCallback(4.5, s20RemoveItem)
    Scripting.AddTimedCallback(4.7, s21VerifyRemove)
    Scripting.AddTimedCallback(0.9, s22AddTool)
    Scripting.AddTimedCallback(1.1, s23VerifyTool)
    Scripting.AddTimedCallback(1.3, s24AddPlainItem)
    Scripting.AddTimedCallback(1.5, s25UseOutOfRange)
    Scripting.AddTimedCallback(1.7, s26UseWithoutUseRange)
    Scripting.AddTimedCallback(1.9, s27UseInRangeNoKeys)
end

s1CreateItems = function()
    if swordTemplateId == nil or shieldTemplateId == nil then
        Test.Fail("AddSlotsAndSlotCount", "sword/shield item templates missing from database")
        return
    end

    -- sword1 gets a local quantity of 10 so that partial-quantity moves can be tested.
    sword1 = Entities.Create({ Template = swordTemplateId, NonPersistent = true, Quantity = 10 })
    sword2 = Entities.Create({ Template = swordTemplateId, NonPersistent = true })
    sword3 = Entities.Create({ Template = swordTemplateId, NonPersistent = true })
    sword4 = Entities.Create({ Template = swordTemplateId, NonPersistent = true })
    shield1 = Entities.Create({ Template = shieldTemplateId, NonPersistent = true })
    shield2 = Entities.Create({ Template = shieldTemplateId, NonPersistent = true })

    -- The plain item has no UseRange component, so it cannot be used as a tool.
    plainItem = Entities.Create({ Template = swordTemplateId, NonPersistent = true })
end

Scripting.AddTimedCallback(0.5, setup)

Util.LogInfo("[" .. Suite .. "] suite registered")
