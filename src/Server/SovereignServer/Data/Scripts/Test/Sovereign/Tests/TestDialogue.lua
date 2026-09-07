-- Tests for the Dialogue module (headless smoke tests; no player is connected).

local Suite = "TestDialogue"

Test.Case("ShowSmoke", function()
    Dialogue.Show(0, "TestHarness NPC", "[" .. Suite .. "] smoke dialogue to a nonexistent player")
end)

Test.Case("ShowProfileSmoke", function()
    Dialogue.ShowProfile(0, 101, "TestHarness NPC", "[" .. Suite .. "] smoke profile dialogue")
end)

Util.LogInfo("[" .. Suite .. "] suite registered")
