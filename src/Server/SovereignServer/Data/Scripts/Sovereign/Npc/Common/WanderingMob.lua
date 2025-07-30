-- WanderingMob.lua
-- Generic behavior script for wandering mobs.

function on_load()
    -- do nothing
end

scripting.AddEntityParameterHint("on_load", "Sovereign.WanderingMob.WanderRadius")
scripting.AddEntityParameterHint("on_load", "Sovereign.WanderingMob.AggroRadius")
scripting.AddEntityParameterHint("on_load", "Sovereign.WanderingMob.WanderRate")
