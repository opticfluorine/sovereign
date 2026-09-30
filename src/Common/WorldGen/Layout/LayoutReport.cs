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

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sovereign.WorldGen.Layout;

/// <summary>
///     Per-anchor outcome of layout validation: the mass centroid the anchor matched, or a
///     record that it matched nothing.
/// </summary>
public sealed class LayoutAnchorReport
{
    /// <summary>
    ///     Zero-based anchor index.
    /// </summary>
    public required int Index { get; init; }

    /// <summary>
    ///     Anchor center X in normalized footprint coordinates.
    /// </summary>
    public required float X { get; init; }

    /// <summary>
    ///     Anchor center Y in normalized footprint coordinates.
    /// </summary>
    public required float Y { get; init; }

    /// <summary>
    ///     Anchor radius in normalized footprint coordinates.
    /// </summary>
    public required float Radius { get; init; }

    /// <summary>
    ///     Anchor weight.
    /// </summary>
    public required float Weight { get; init; }

    /// <summary>
    ///     Whether the anchor matched a significant mass.
    /// </summary>
    public required bool Matched { get; init; }

    /// <summary>
    ///     Matched mass centroid X in normalized coordinates; NaN when unmatched.
    /// </summary>
    public required float CentroidX { get; init; }

    /// <summary>
    ///     Matched mass centroid Y in normalized coordinates; NaN when unmatched.
    /// </summary>
    public required float CentroidY { get; init; }
}

/// <summary>
///     Best-effort report of how well a generated plan honored its configured layout.
/// </summary>
public sealed class LayoutReport
{
    /// <summary>
    ///     Per-anchor match reports, in anchor order.
    /// </summary>
    public required IReadOnlyList<LayoutAnchorReport> Anchors { get; init; }

    /// <summary>
    ///     Number of significant masses found.
    /// </summary>
    public required int MassCount { get; init; }

    /// <summary>
    ///     Warnings raised during layout resolution and validation.
    /// </summary>
    public required IReadOnlyList<string> Warnings { get; init; }

    /// <summary>
    ///     Whether every significant anchor (weight at least 0.5) matched a mass.
    /// </summary>
    public bool AllSignificantAnchorsMatched => Anchors.All(a => a.Weight < 0.5f || a.Matched);

    /// <summary>
    ///     Number of significant anchors that matched a mass.
    /// </summary>
    public int MatchedAnchorCount => Anchors.Count(a => a.Weight >= 0.5f && a.Matched);

    /// <summary>
    ///     Number of significant anchors.
    /// </summary>
    public int SignificantAnchorCount => Anchors.Count(a => a.Weight >= 0.5f);

    /// <summary>
    ///     Formats the report as a chat-ready block.
    /// </summary>
    /// <returns>Formatted report.</returns>
    public string Format()
    {
        var builder = new StringBuilder();
        builder.AppendLine(
            $"  Layout: {MassCount} masses, {MatchedAnchorCount}/{SignificantAnchorCount} anchors matched");
        foreach (var anchor in Anchors)
        {
            if (anchor.Matched)
            {
                builder.AppendLine(
                    $"  Layout anchor {anchor.Index + 1} ({anchor.X:F2}, {anchor.Y:F2}) -> " +
                    $"mass ({anchor.CentroidX:F2}, {anchor.CentroidY:F2})");
            }
            else
            {
                builder.AppendLine(
                    $"  Layout anchor {anchor.Index + 1} ({anchor.X:F2}, {anchor.Y:F2}) -> unmatched");
            }
        }

        foreach (var warning in Warnings)
        {
            builder.AppendLine($"  Layout warning: {warning}");
        }

        return builder.ToString();
    }
}
