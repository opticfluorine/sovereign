-- Tests for the Entities module (create spec variants, remove, template queries).

local Suite = "TestEntities"

local setup, step1VerifyNpc, step2RemoveNpc, step3VerifyRemoved, step4VerifyItemTemplate
local npcEntityId, itemEntityId, swordTemplateId

Test.Async("CreateWithSpec")

step1VerifyNpc = function()
    Test.Step("CreateWithSpec", function()
        Test.AssertTrue(npcEntityId ~= nil and npcEntityId ~= Entities.ToEntityId(0),
            "entity ID should be returned")
        Test.AssertEqual("TestEntitiesFixture", Components.Name.Get(npcEntityId), "name from spec")
        Test.AssertEqual(EntityType.Npc, Components.EntityType.Get(npcEntityId), "entity type from spec")
        local kin = Components.Kinematics.Get(npcEntityId)
        Test.AssertNear(0.5, kin.Position.X, 0.0001, "position X from spec")
        Test.AssertNear(0.5, kin.Position.Y, 0.0001, "position Y from spec")
        Test.AssertNear(1.0, kin.Position.Z, 0.0001, "position Z from spec")
        Test.AssertTrue(Components.ServerOnly.Exists(npcEntityId), "ServerOnly from spec")
        Test.AssertEqual(221, Components.AnimatedSprite.Get(npcEntityId), "animated sprite from spec")
    end)
    Test.Pass("CreateWithSpec")
end

Test.Async("RemoveEntity")

step2RemoveNpc = function()
    Entities.Remove(npcEntityId)
end

step3VerifyRemoved = function()
    Test.Step("RemoveEntity", function()
        Test.AssertTrue(not Components.Name.Exists(npcEntityId), "removed entity should have no name")
    end)
    Test.Pass("RemoveEntity")
end

Test.Async("TemplateQueries")

step4VerifyItemTemplate = function()
    Test.Step("TemplateQueries", function()
        Test.AssertTrue(swordTemplateId ~= nil, "sword item template should exist")
        Test.AssertTrue(Entities.IsTemplate(swordTemplateId), "sword template ID should be a template entity")
        Test.AssertTrue(itemEntityId ~= nil and itemEntityId ~= Entities.ToEntityId(0),
            "item entity should be created")
        Test.AssertEqual(swordTemplateId, Entities.GetTemplate(itemEntityId), "item should reference its template")
        Test.AssertTrue(not Entities.IsTemplate(itemEntityId), "item entity is not a template")
    end)
    Test.Pass("TemplateQueries")
end

Test.Case("AbsoluteTemplateId", function()
    Test.AssertEqual("7FFE000000000004", Entities.FormatEntityId(Entities.ToTemplateEntityId(4)),
        "relative template ID 4 should map to the absolute range")
    Test.AssertEqual(Entities.FirstTemplateEntityId, Entities.ToTemplateEntityId(0),
        "relative template ID 0 should map to the first template entity ID")
end)

Test.Case("IsTemplateForNonTemplate", function()
    Test.AssertTrue(not Entities.IsTemplate(Entities.ToEntityId(0)), "entity ID 0 is not a template")
    Test.AssertTrue(not Entities.IsTemplate(Entities.ToEntityId(0x6FFF000000000000)),
        "block entities are not templates")
end)

-- Suite setup. ------------------------------------------------------------------------

-- Fixture creation is deferred to stagger startup across suites; the engine does not
-- currently serialize concurrent engine mutations from parallel script hosts.
setup = function()
    npcEntityId = Entities.Create({
        Name = "TestEntitiesFixture",
        EntityType = EntityType.Npc,
        NonPersistent = true,
        ServerOnly = true,
        AnimatedSprite = 221,
        Kinematics = {
            Position = { X = 0.5, Y = 0.5, Z = 1.0 },
            Velocity = { X = 0.0, Y = 0.0, Z = 0.0 }
        }
    })

    -- Item entity created from the sword item template (requires the template in the database).
    local swordTemplates = Items.FindByName("Sword")
    swordTemplateId = swordTemplates[1]
    if swordTemplateId ~= nil then
        itemEntityId = Entities.Create({
            Template = swordTemplateId,
            NonPersistent = true
        })
    end

    Scripting.AddTimedCallback(0.3, step1VerifyNpc)
    Scripting.AddTimedCallback(0.5, step2RemoveNpc)
    Scripting.AddTimedCallback(0.7, step3VerifyRemoved)
    Scripting.AddTimedCallback(0.7, step4VerifyItemTemplate)
end

Scripting.AddTimedCallback(0.2, setup)

Util.LogInfo("[" .. Suite .. "] suite registered")
