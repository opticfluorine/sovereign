-- Tests for the Radiant scripting library and RadiantData component indexing.
-- Exercises Radiant.GetValue linear field evaluation, category filtering,
-- unpositioned child entities anchored to a positioned parent, and
-- Components.RadiantData.Set/Remove re-indexing. Kinematics-driven movement is
-- intentionally not exercised: the indexer snapshots positions at RadiantData
-- add/modify time and never re-indexes on movement.
--
-- A raw category integer (7) is used to simulate a second category because the
-- RadiantCategory enum currently has a single member.

local Suite = "TestRadiant"

local setup, s1VerifyFields, s2SetComponent, s3VerifyReindex
local s4RemoveChild, s5VerifyRemoved
local fixtureAId, fixtureBId, childId

-- Query point: distance 4 from fixture A's position (100, 100, 0) and
-- distance 12 from fixture B's position (92, 100, 0).
local QueryPos = {X = 104.0, Y = 100.0, Z = 0.0}

Test.Async("RadiantFields")

s1VerifyFields = function()
    Test.Step("RadiantFields", function()
        -- Fixture A: Param0 * 4 + Param1 = -2 * 4 + 10 = 2.
        -- Child C: unpositioned, evaluated at the parent's position: -1 * 4 + 6 = 2.
        Test.AssertNear(4.0, Radiant.GetValue(RadiantCategory.MineralDrop, QueryPos), 1e-3,
            "mineral drop field should sum fixture A and child contributions")
        -- Fixture B (category 7): -1 * 12 + 50 = 38.
        Test.AssertNear(38.0, Radiant.GetValue(7, QueryPos), 1e-3,
            "category 7 field should only include fixture B")
        Test.AssertTrue(Components.RadiantData.Exists(fixtureAId),
            "fixture A should have a RadiantData component")
    end)
    Test.Pass("RadiantFields")
end

Test.Async("SetReindexes")

s2SetComponent = function()
    -- Modify fixture A's field: -3 * 4 + 10 = -2 at the query point.
    Components.RadiantData.Set(fixtureAId,
        {Category = RadiantCategory.MineralDrop, Function = RadiantFunction.Linear,
         Param0 = -3.0, Param1 = 10.0})
end

s3VerifyReindex = function()
    Test.Step("SetReindexes", function()
        -- Fixture A: -2; child C: 2.
        Test.AssertNear(0.0, Radiant.GetValue(RadiantCategory.MineralDrop, QueryPos), 1e-3,
            "mineral drop field should reflect the modified fixture A parameters")
        Test.AssertEqual(-3.0, Components.RadiantData.Get(fixtureAId).Param0,
            "fixture A Param0 after Set")
    end)
    Test.Pass("SetReindexes")
end

Test.Async("RemoveDropsContribution")

s4RemoveChild = function()
    Components.RadiantData.Remove(childId)
end

s5VerifyRemoved = function()
    Test.Step("RemoveDropsContribution", function()
        -- Only fixture A contributes now.
        Test.AssertNear(-2.0, Radiant.GetValue(RadiantCategory.MineralDrop, QueryPos), 1e-3,
            "mineral drop field should exclude the removed child")
        Test.AssertTrue(not Components.RadiantData.Exists(childId),
            "child should no longer have a RadiantData component")
    end)
    Test.Pass("RemoveDropsContribution")
end

-- Suite setup. ------------------------------------------------------------------------

setup = function()
    fixtureAId = Entities.Create({
        Name = "TestRadiantFixtureA",
        NonPersistent = true,
        Kinematics = {Position = {X = 100.0, Y = 100.0, Z = 0.0}, Velocity = {X = 0.0, Y = 0.0, Z = 0.0}},
        RadiantData = {Category = RadiantCategory.MineralDrop, Function = RadiantFunction.Linear,
                       Param0 = -2.0, Param1 = 10.0}
    })

    -- Unpositioned child anchored to fixture A; its contribution is evaluated
    -- against the parent's position snapshot.
    childId = Entities.Create({
        Name = "TestRadiantChild",
        NonPersistent = true,
        Parent = fixtureAId,
        RadiantData = {Category = RadiantCategory.MineralDrop, Function = RadiantFunction.Linear,
                       Param0 = -1.0, Param1 = 6.0}
    })

    -- Fixture B emits in a different (raw) category and must be filtered out
    -- of mineral drop queries.
    fixtureBId = Entities.Create({
        Name = "TestRadiantFixtureB",
        NonPersistent = true,
        Kinematics = {Position = {X = 92.0, Y = 100.0, Z = 0.0}, Velocity = {X = 0.0, Y = 0.0, Z = 0.0}},
        RadiantData = {Category = 7, Function = RadiantFunction.Linear,
                       Param0 = -1.0, Param1 = 50.0}
    })

    Scripting.AddTimedCallback(1.0, s1VerifyFields)
    Scripting.AddTimedCallback(1.3, s2SetComponent)
    Scripting.AddTimedCallback(1.6, s3VerifyReindex)
    Scripting.AddTimedCallback(1.9, s4RemoveChild)
    Scripting.AddTimedCallback(2.2, s5VerifyRemoved)
end

Scripting.AddTimedCallback(0.5, setup)

Util.LogInfo("[" .. Suite .. "] suite registered")
