function on_player_entered(playerEntityId)
    util.log_info("Player entered world: " .. playerEntityId)
end

util.log_info("This is a test of the scripting engine.")
scripting.add_event_callback(events.Server_Persistence_PlayerEnteredWorld, on_player_entered)
