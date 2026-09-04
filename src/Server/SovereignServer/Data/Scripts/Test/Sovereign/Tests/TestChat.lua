-- Tests for the Chat module (registration and send paths; no players are connected,
-- so messages are smoke-tested by verifying that the calls do not raise).

local Suite = "TestChat"

local commaArgs = nil

local function onTestCommand(msg, senderEntityId)
    -- Command callbacks receive the raw remainder and the sender entity ID.
    commaArgs = msg
end

local function onCommaCommand(args, senderEntityId)
    -- CommaSeparatedArgs callbacks receive a 1-indexed table of trimmed strings.
    commaArgs = args
end

Test.Case("AddCommandRegistration", function()
    Chat.AddCommand("testharnesscmd", onTestCommand, ChatCommandFlags.None)
end)

Test.Case("AddCommaSeparatedCommand", function()
    Chat.AddCommand("testharnesscommacmd", onCommaCommand, ChatCommandFlags.CommaSeparatedArgs)
end)

Test.Case("SendSystemMessageSmoke", function()
    Chat.SendSystemMessage(0, "[" .. Suite .. "] smoke message to a nonexistent player")
end)

Test.Case("SendToPlayerSmoke", function()
    Chat.SendToPlayer(0, Color.Rgb(210, 210, 0), "[" .. Suite .. "] smoke message to a nonexistent player")
end)

Test.Case("SendToAllSmoke", function()
    Chat.SendToAll(Color.Rgb(210, 210, 0), "[" .. Suite .. "] smoke message to all players")
end)

Util.LogInfo("[" .. Suite .. "] suite registered")
