-- Tests for tool-side item use callbacks and the OnInteract face argument.
-- Requires the item templates in the database (a stackable "Sword").

local Suite = "TestItemUseCallbacks"
local ScriptName = "Test/Sovereign/Tests/TestItemUseCallbacks"

local setup, addToolWithUse, addToolWithoutUse, addToolBoth
local runToolOnly, verifyToolOnly, runTargetOnly, verifyTargetOnly, runBoth, verifyBoth
local actorId, toolWithUse, toolWithoutUse, toolBoth
local targetToolOnly, targetTargetOnly, targetBoth

local toolOnlyCalls = 0
local targetOnlyCalls = 0
local bothToolCalls = 0
local bothInteractCalls = 0
local toolOnlyFace = nil
local targetOnlyFace = nil
local bothToolFace = nil
local bothInteractFace = nil
local toolOnlyArgs = nil
local targetOnlyArgs = nil

-- Synchronous cases. -----------------------------------------------------------------

Test.Case("ItemUseFaceConstants", function()
    Test.AssertEqual(0, ItemUseFace.None, "ItemUseFace.None should be 0")
    Test.AssertEqual(1, ItemUseFace.BlockFrontFace, "ItemUseFace.BlockFrontFace should be 1")
    Test.AssertEqual(2, ItemUseFace.BlockTopFace, "ItemUseFace.BlockTopFace should be 2")
end)

-- Callbacks. -------------------------------------------------------------------------

function OnToolOnlyUseItem(actor, tool, target, face)
    toolOnlyCalls = toolOnlyCalls + 1
    toolOnlyFace = face
    toolOnlyArgs = { Actor = actor, Tool = tool, Target = target, Count = select("#", actor, tool, target, face) }
end

function OnTargetOnlyInteract(actor, tool, target, face)
    targetOnlyCalls = targetOnlyCalls + 1
    targetOnlyFace = face
    targetOnlyArgs = { Actor = actor, Tool = tool, Target = target, Count = select("#", actor, tool, target, face) }
end

function OnBothUseItem(actor, tool, target, face)
    bothToolCalls = bothToolCalls + 1
    bothToolFace = face
end

function OnBothInteract(actor, tool, target, face)
    bothInteractCalls = bothInteractCalls + 1
    bothInteractFace = face
end

-- Asynchronous cases. ----------------------------------------------------------------

Test.Async("UseItemToolCallback")

runToolOnly = function()
    Inventory.UseItem(actorId, toolWithUse, targetToolOnly)
end

verifyToolOnly = function()
    Test.Step("UseItemToolCallback", function()
        Test.AssertEqual(1, toolOnlyCalls, "tool OnUseItem callback should fire once")
        Test.AssertEqual(4, toolOnlyArgs.Count, "tool callback should receive 4 arguments")
        Test.AssertEqual(actorId, toolOnlyArgs.Actor, "tool callback actor argument")
        Test.AssertEqual(toolWithUse, toolOnlyArgs.Tool, "tool callback tool argument")
        Test.AssertEqual(targetToolOnly, toolOnlyArgs.Target, "tool callback target argument")
        Test.AssertEqual(0, toolOnlyFace, "tool callback face should be ItemUseFace.None")
    end)
    Test.Pass("UseItemToolCallback")
end

Test.Async("UseItemTargetCallback")

runTargetOnly = function()
    Inventory.UseItem(actorId, toolWithoutUse, targetTargetOnly)
end

verifyTargetOnly = function()
    Test.Step("UseItemTargetCallback", function()
        Test.AssertEqual(1, targetOnlyCalls, "target OnInteract callback should fire once")
        Test.AssertEqual(4, targetOnlyArgs.Count, "target callback should receive 4 arguments")
        Test.AssertEqual(actorId, targetOnlyArgs.Actor, "target callback actor argument")
        Test.AssertEqual(toolWithoutUse, targetOnlyArgs.Tool, "target callback tool argument")
        Test.AssertEqual(targetTargetOnly, targetOnlyArgs.Target, "target callback target argument")
        Test.AssertEqual(0, targetOnlyFace, "target callback face should be ItemUseFace.None")
    end)
    Test.Pass("UseItemTargetCallback")
end

Test.Async("UseItemBothCallbacks")

runBoth = function()
    Inventory.UseItem(actorId, toolBoth, targetBoth)
end

verifyBoth = function()
    Test.Step("UseItemBothCallbacks", function()
        -- Both callbacks run on independent per-entity FIFO chains; only the enqueue order is defined,
        -- so this test asserts that both fired without relying on which one completed first.
        Test.AssertEqual(1, bothToolCalls, "tool OnUseItem callback should fire once")
        Test.AssertEqual(1, bothInteractCalls, "target OnInteract callback should fire once")
        Test.AssertEqual(0, bothToolFace, "tool callback face should be ItemUseFace.None")
        Test.AssertEqual(0, bothInteractFace, "target callback face should be ItemUseFace.None")
    end)
    Test.Pass("UseItemBothCallbacks")
end

-- Suite setup. ------------------------------------------------------------------------

-- Fixture creation is deferred to stagger startup across suites; the engine does not
-- currently serialize concurrent engine mutations from parallel script hosts.
setup = function()
    local swords = Items.FindByName("Sword")
    local swordTemplateId = swords[1]

    actorId = Entities.Create({
        Name = "TestItemUseCallbacksActor",
        EntityType = EntityType.Npc,
        NonPersistent = true,
        Kinematics = {
            Position = { X = 0.5, Y = 0.5, Z = 1.0 },
            Velocity = { X = 0.0, Y = 0.0, Z = 0.0 }
        }
    })

    targetToolOnly = Entities.Create({
        Name = "TestItemUseCallbacksToolTarget",
        EntityType = EntityType.Npc,
        NonPersistent = true,
        Kinematics = {
            Position = { X = 1.5, Y = 0.5, Z = 1.0 },
            Velocity = { X = 0.0, Y = 0.0, Z = 0.0 }
        }
    })

    targetTargetOnly = Entities.Create({
        Name = "TestItemUseCallbacksInteractTarget",
        EntityType = EntityType.Npc,
        NonPersistent = true,
        Kinematics = {
            Position = { X = 2.5, Y = 0.5, Z = 1.0 },
            Velocity = { X = 0.0, Y = 0.0, Z = 0.0 }
        }
    })

    targetBoth = Entities.Create({
        Name = "TestItemUseCallbacksBothTarget",
        EntityType = EntityType.Npc,
        NonPersistent = true,
        Kinematics = {
            Position = { X = 3.5, Y = 0.5, Z = 1.0 },
            Velocity = { X = 0.0, Y = 0.0, Z = 0.0 }
        }
    })

    toolWithUse = Entities.Create({
        Template = swordTemplateId,
        NonPersistent = true,
        UseRange = 5.0,
        ItemUse = ItemUse.Npc
    })

    toolWithoutUse = Entities.Create({
        Template = swordTemplateId,
        NonPersistent = true,
        UseRange = 5.0,
        ItemUse = ItemUse.Npc
    })

    toolBoth = Entities.Create({
        Template = swordTemplateId,
        NonPersistent = true,
        UseRange = 5.0,
        ItemUse = ItemUse.Npc
    })

    Test.SetEntityKeyValue(toolWithUse, "__OnUseItem_Script", ScriptName)
    Test.SetEntityKeyValue(toolWithUse, "__OnUseItem_Function", "OnToolOnlyUseItem")
    Test.SetEntityKeyValue(toolBoth, "__OnUseItem_Script", ScriptName)
    Test.SetEntityKeyValue(toolBoth, "__OnUseItem_Function", "OnBothUseItem")

    Test.SetEntityKeyValue(targetTargetOnly, "__OnInteract_Script", ScriptName)
    Test.SetEntityKeyValue(targetTargetOnly, "__OnInteract_Function", "OnTargetOnlyInteract")
    Test.SetEntityKeyValue(targetBoth, "__OnInteract_Script", ScriptName)
    Test.SetEntityKeyValue(targetBoth, "__OnInteract_Function", "OnBothInteract")

    Inventory.AddSlots(actorId, 4)

    Scripting.AddTimedCallback(0.4, addToolWithUse)
    Scripting.AddTimedCallback(0.6, addToolWithoutUse)
    Scripting.AddTimedCallback(0.8, addToolBoth)
    Scripting.AddTimedCallback(1.2, runToolOnly)
    Scripting.AddTimedCallback(1.8, verifyToolOnly)
    Scripting.AddTimedCallback(1.9, runTargetOnly)
    Scripting.AddTimedCallback(2.5, verifyTargetOnly)
    Scripting.AddTimedCallback(2.6, runBoth)
    Scripting.AddTimedCallback(3.2, verifyBoth)
end

addToolWithUse = function()
    Inventory.AddItem(actorId, toolWithUse)
end

addToolWithoutUse = function()
    Inventory.AddItem(actorId, toolWithoutUse)
end

addToolBoth = function()
    Inventory.AddItem(actorId, toolBoth)
end

Scripting.AddTimedCallback(0.5, setup)

Util.LogInfo("[" .. Suite .. "] suite registered")
