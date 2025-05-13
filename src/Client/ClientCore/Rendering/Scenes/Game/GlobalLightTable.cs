using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sovereign.ClientCore.Configuration;
using Sovereign.EngineCore.Configuration;

namespace Sovereign.ClientCore.Rendering.Scenes.Game;

/// <summary>
///     Lookup table of global light values for the day/night cycle.
/// </summary>
public class GlobalLightTable
{
    private readonly DayNightOptions dayNightOptions;
    private readonly List<Vector4> globalLightBySecond;
    private readonly ILogger<GlobalLightTable> logger;
    private readonly int tableSize;

    public GlobalLightTable(IOptions<DayNightOptions> dayNightOptions, IOptions<TimeOptions> timeOptions,
        ILogger<GlobalLightTable> logger)
    {
        this.logger = logger;
        this.dayNightOptions = dayNightOptions.Value;
        tableSize = (int)(timeOptions.Value.SecondsPerHour * timeOptions.Value.HoursPerDay);
        globalLightBySecond = new List<Vector4>(tableSize);

        BuildLookupTable();
    }

    /// <summary>
    /// </summary>
    /// <param name="secondOfDay"></param>
    /// <returns></returns>
    public Vector4 GetGlobalLightColor(uint secondOfDay)
    {
        var index = secondOfDay;
        if (secondOfDay >= tableSize)
        {
            index %= (uint)tableSize;
            logger.LogError("Second of day {secondOfDay} is out of range; wrapping to {wrappedSecond}.",
                secondOfDay, index);
        }

        return globalLightBySecond[(int)index];
    }

    /// <summary>
    ///     Builds the global light lookup table.
    /// </summary>
    private void BuildLookupTable()
    {
        var steps = dayNightOptions.GlobalLightSteps.OrderBy(s => s.SecondOfDay).ToList();
        if (steps.Count == 0)
        {
            logger.LogError("No global light steps defined; defaulting to white light.");
            steps.Add(new GlobalLightStep
            {
                SecondOfDay = 0,
                Red = 1.0f,
                Green = 1.0f,
                Blue = 1.0f
            });
        }

        for (var second = 0; second < tableSize; second++)
        {
            // Find the two steps that bracket the current second.
            FindBracketingSteps(steps, second, out var previousStep, out var nextStep);

            // Interpolate the color between the two steps.
            var t = (float)(second - previousStep.SecondOfDay) /
                    (nextStep.SecondOfDay - previousStep.SecondOfDay);
            var dc = nextStep.Color - previousStep.Color;
            var color = previousStep.Color + t * dc;

            globalLightBySecond.Add(new Vector4(color, 1.0f));
        }
    }

    /// <summary>
    ///     Finds the lighting steps that bracket the given second of the day.
    /// </summary>
    /// <param name="steps">Lighting steps sorted by second of day (ascending).</param>
    /// <param name="second">Second of day.</param>
    /// <param name="previousStep">Previous step.</param>
    /// <param name="nextStep">Next step.</param>
    private static void FindBracketingSteps(List<GlobalLightStep> steps, int second,
        out GlobalLightStep previousStep, out GlobalLightStep nextStep)
    {
        previousStep = steps[0];
        nextStep = steps[1 % steps.Count];

        for (var i = 0; i < steps.Count; i++)
        {
            if (steps[i].SecondOfDay <= second)
            {
                previousStep = steps[i];
                nextStep = steps[(i + 1) % steps.Count]; // Wrap around to the first step.
            }
        }
    }
}