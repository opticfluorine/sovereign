-- Tests for the Color module (bit packing and constants).

local Suite = "TestColor"

Test.Case("RgbPacking", function()
    Test.AssertEqual(0xFFFFFFFF, Color.Rgb(255, 255, 255), "white should pack as 0xFFFFFFFF")
    Test.AssertEqual(0xFF0000FF, Color.Rgb(255, 0, 0), "red should pack as 0xFF0000FF")
    Test.AssertEqual(0x00FF00FF, Color.Rgb(0, 255, 0), "green should pack as 0x00FF00FF")
    Test.AssertEqual(0x0000FFFF, Color.Rgb(0, 0, 255), "blue should pack as 0x0000FFFF")
    Test.AssertEqual(0x000000FF, Color.Rgb(0, 0, 0), "black should pack as 0x000000FF")
end)

Test.Case("RgbaPacking", function()
    Test.AssertEqual(0xFF0000A0, Color.Rgba(255, 0, 0, 160), "transparent red should pack as 0xFF0000A0")
    Test.AssertEqual(0x10203040, Color.Rgba(0x10, 0x20, 0x30, 0x40), "component packing order")
end)

Test.Case("ColorConstants", function()
    Test.AssertEqual(0xFFFFFFFF, Color.WHITE, "WHITE")
    Test.AssertEqual(0x000000FF, Color.BLACK, "BLACK")
    Test.AssertEqual(0xFF0000FF, Color.RED, "RED")
    Test.AssertEqual(0x00FF00FF, Color.GREEN, "GREEN")
    Test.AssertEqual(0x0000FFFF, Color.BLUE, "BLUE")
end)

Test.Case("PurposeConstants", function()
    Test.AssertEqual(Color.Rgb(160, 160, 240), Color.MOTD, "MOTD")
    Test.AssertEqual(Color.Rgb(210, 80, 80), Color.ALERT, "ALERT")
    Test.AssertEqual(Color.Rgb(179, 179, 179), Color.CHAT_LOCAL, "CHAT_LOCAL")
    Test.AssertEqual(Color.Rgb(255, 255, 255), Color.CHAT_GLOBAL, "CHAT_GLOBAL")
    Test.AssertEqual(Color.Rgb(128, 128, 128), Color.CHAT_SYSTEM, "CHAT_SYSTEM")
end)

Util.LogInfo("[" .. Suite .. "] suite registered")
