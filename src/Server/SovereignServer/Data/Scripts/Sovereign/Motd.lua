g_ticks = 0

function on_tick()
    if g_ticks % 50 == 0 then
        util.log_info("tick " .. g_ticks)
    end
    g_ticks = g_ticks + 1
end

util.log_info("This is a test of the scripting engine.")
scripting.add_event_callback(events.Core_Tick, on_tick)
