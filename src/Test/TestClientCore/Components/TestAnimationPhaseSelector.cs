// Sovereign Engine
// Copyright (c) 2026 opticfluorine
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using Sovereign.ClientCore.Components;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Entities;
using Xunit;

namespace TestClientCore.Components;

/// <summary>
///     Unit tests for <see cref="AnimationPhaseSelector" /> precedence and time shifting.
/// </summary>
public class TestAnimationPhaseSelector
{
    private const ulong EntityId = 0x7fff000000000001;

    private readonly ActiveAnimationPhaseComponentCollection activePhases;
    private readonly AnimationPhaseComponentCollection basePhases;
    private readonly AnimationPhaseSelector selector;

    public TestAnimationPhaseSelector()
    {
        var componentManager = new ComponentManager(new EntityNotifier());
        var entityTable = new EntityTable();
        basePhases = new AnimationPhaseComponentCollection(entityTable, componentManager);
        activePhases = new ActiveAnimationPhaseComponentCollection(entityTable, componentManager);
        selector = new AnimationPhaseSelector(basePhases, activePhases);
    }

    [Fact]
    public void Select_NoComponents_DefaultsToDefaultWithAbsoluteTime()
    {
        selector.Select(EntityId, 1234, out var phase, out var effectiveTime);

        Assert.Equal(AnimationPhase.Default, phase);
        Assert.Equal(1234UL, effectiveTime);
    }

    [Fact]
    public void Select_BaseOnly_UsesBaseWithAbsoluteTime()
    {
        basePhases.AddOrUpdateComponent(EntityId, AnimationPhase.Moving);

        selector.Select(EntityId, 1234, out var phase, out var effectiveTime);

        Assert.Equal(AnimationPhase.Moving, phase);
        Assert.Equal(1234UL, effectiveTime);
    }

    [Fact]
    public void Select_ActiveOnly_UsesActiveWithShiftedTime()
    {
        activePhases.AddOrUpdateComponent(EntityId, new ActiveAnimationPhase
        {
            Phase = AnimationPhase.Moving,
            Loops = 1,
            StartTimeTicks = 1000,
            LoopDurationTicks = 0
        });

        selector.Select(EntityId, 1500, out var phase, out var effectiveTime);

        Assert.Equal(AnimationPhase.Moving, phase);
        Assert.Equal(500UL, effectiveTime);
    }

    [Fact]
    public void Select_ActiveOverridesBase()
    {
        basePhases.AddOrUpdateComponent(EntityId, AnimationPhase.Default);
        activePhases.AddOrUpdateComponent(EntityId, new ActiveAnimationPhase
        {
            Phase = AnimationPhase.Moving,
            Loops = 1,
            StartTimeTicks = 1000,
            LoopDurationTicks = 0
        });

        selector.Select(EntityId, 1000, out var phase, out var effectiveTime);

        Assert.Equal(AnimationPhase.Moving, phase);
        Assert.Equal(0UL, effectiveTime);
    }
}
