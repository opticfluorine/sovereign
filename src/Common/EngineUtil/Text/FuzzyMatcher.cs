// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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

using System;
using System.Buffers;
using System.Collections.Generic;
using Sovereign.EngineUtil.Collections;

namespace Sovereign.EngineUtil.Text;

/// <summary>
///     Allocation-free fuzzy string matcher based on the Damerau-Levenshtein
///     distance (OSA variant with restricted transpositions).
/// </summary>
/// <remarks>
///     This class is NOT thread-safe. It holds reusable scratch buffers
///     (a binary heap and a drain array) intended to be driven from a single
///     thread. Callers may reuse the same instance across calls; internal
///     state is reset on each call to <see cref="AppendBestMatches" />.
///     Steady-state calls allocate no managed heap memory beyond the
///     caller-supplied output list's amortized capacity growth.
/// </remarks>
public sealed class FuzzyMatcher
{
    /// <summary>
    ///     Comparer that orders elements worst-first for top-N eviction:
    ///     ascending by score, and for tied scores descending by name
    ///     (Ordinal) so that the alphabetically-latest tied name is evicted
    ///     first, leaving the alphabetically-earliest name in the output.
    /// </summary>
    private static readonly IComparer<(string Name, float Score)> WorstFirstComparer =
        Comparer<(string Name, float Score)>.Create((a, b) =>
        {
            var c = a.Score.CompareTo(b.Score);
            if (c != 0) return c;
            return string.Compare(b.Name, a.Name, StringComparison.Ordinal);
        });

    private readonly BinaryHeap<(string Name, float Score)> heap;
    private readonly (string Name, float Score)[] drainBuffer;
    private readonly bool caseSensitive;

    /// <summary>
    ///     Creates a new fuzzy matcher.
    /// </summary>
    /// <param name="maxCapacity">
    ///     Maximum number of results that can be returned from a single
    ///     call to <see cref="AppendBestMatches" />. Also sizes the internal
    ///     scratch buffers.
    /// </param>
    /// <param name="caseSensitive">
    ///     If true (default), string comparison is case-sensitive.
    ///     If false, comparison is case-insensitive.
    /// </param>
    public FuzzyMatcher(int maxCapacity = 64, bool caseSensitive = true)
    {
        if (maxCapacity < 0) maxCapacity = 0;
        MaxCapacity = maxCapacity;
        this.caseSensitive = caseSensitive;
        heap = new BinaryHeap<(string Name, float Score)>(Math.Max(maxCapacity, 1), WorstFirstComparer);
        drainBuffer = new (string Name, float Score)[Math.Max(maxCapacity, 1)];
    }

    /// <summary>
    ///     Maximum number of results returned from a single call.
    /// </summary>
    public int MaxCapacity { get; }

    /// <summary>
    ///     Computes the Damerau-Levenshtein distance (OSA variant, restricted
    ///     transpositions) between two strings using byte-wise ordinal
    ///     comparison. <c>null</c> is treated as the empty string.
    /// </summary>
    /// <param name="a">First string.</param>
    /// <param name="b">Second string.</param>
    /// <returns>
    ///     Edit distance (insertions, deletions, substitutions, and
    ///     adjacent-character transpositions).
    /// </returns>
    public static int DamerauLevenshteinDistance(string? a, string? b)
        => ComputeDistance(a ?? "", b ?? "");

    /// <summary>
    ///     Core DP for the Damerau-Levenshtein (OSA) distance between two
    ///     non-null strings. Both arguments must be non-null.
    /// </summary>
    private static int ComputeDistance(string a, string b)
    {
        var la = a.Length;
        var lb = b.Length;

        if (la == 0) return lb;
        if (lb == 0) return la;

        int[] prevRow = ArrayPool<int>.Shared.Rent(lb + 1);
        int[] currRow = ArrayPool<int>.Shared.Rent(lb + 1);
        int[] prevPrevRow = ArrayPool<int>.Shared.Rent(lb + 1);
        try
        {
            // prevRow represents row i-1 (initialized to row 0).
            for (var j = 0; j <= lb; j++) prevRow[j] = j;
            // prevPrevRow represents row i-2 (row -1, conceptually infinite).
            // It is only consulted when i >= 2 alongside the transposition
            // check; by that point it has been overwritten with row 0 (then
            // row 1, etc.) by the rotation below, so its rented garbage is
            // never read.

            for (var i = 1; i <= la; i++)
            {
                currRow[0] = i;
                for (var j = 1; j <= lb; j++)
                {
                    var cost = a[i - 1] == b[j - 1] ? 0 : 1;

                    var del = prevRow[j] + 1;
                    var ins = currRow[j - 1] + 1;
                    var sub = prevRow[j - 1] + cost;
                    var min = del < ins ? del : ins;
                    if (sub < min) min = sub;

                    if (i > 1 && j > 1
                                 && a[i - 1] == b[j - 2]
                                 && a[i - 2] == b[j - 1])
                    {
                        var trans = prevPrevRow[j - 2] + 1;
                        if (trans < min) min = trans;
                    }

                    currRow[j] = min;
                }

                // Rotate rows: prevPrev <- prev, prev <- curr.
                var tmp = prevPrevRow;
                prevPrevRow = prevRow;
                prevRow = currRow;
                currRow = tmp;
            }

            return prevRow[lb];
        }
        finally
        {
            ArrayPool<int>.Shared.Return(prevRow, clearArray: false);
            ArrayPool<int>.Shared.Return(currRow, clearArray: false);
            ArrayPool<int>.Shared.Return(prevPrevRow, clearArray: false);
        }
    }

    /// <summary>
    ///     Computes a similarity score in [0, 1] between two strings, where
    ///     1.0 means identical and 0.0 means completely different (or either
    ///     string is <c>null</c> and the other is non-empty). Two empty (or
    ///     <c>null</c>) strings have similarity 1.0.
    /// </summary>
    /// <param name="a">First string.</param>
    /// <param name="b">Second string.</param>
    /// <returns>Similarity in [0, 1].</returns>
    public static float Similarity(string? a, string? b)
    {
        var la = a?.Length ?? 0;
        var lb = b?.Length ?? 0;
        var maxLen = Math.Max(la, lb);
        if (maxLen == 0) return 1.0f;
        var dist = ComputeDistance(a ?? "", b ?? "");
        var sim = 1.0f - dist / (float)maxLen;
        return sim < 0f ? 0f : sim > 1f ? 1f : sim;
    }

    /// <summary>
    ///     Appends up to <paramref name="maxResults" /> matches (best-first)
    ///     to the caller-supplied <paramref name="results" /> list.
    ///     Pre-existing list contents are preserved.
    /// </summary>
    /// <remarks>
    ///     Results are ordered by descending similarity score; ties are
    ///     broken alphabetically (Ordinal ascending) for determinism.
    ///     Candidates with a similarity score of 0 (no shared characters)
    ///     are excluded. The same list instance is returned (no new list
    ///     is allocated).  String comparison respects the
    ///     <see cref="caseSensitive" /> setting passed at construction.
    /// </remarks>
    /// <param name="query">Query string.</param>
    /// <param name="candidates">Candidate strings.</param>
    /// <param name="maxResults">Maximum number of results to append.</param>
    /// <param name="results">Caller-supplied list to append to.</param>
    public void AppendBestMatches(string query, IEnumerable<string> candidates,
        int maxResults, List<(string Name, float Score)> results)
    {
        if (maxResults < 0) maxResults = 0;
        if (maxResults > MaxCapacity) maxResults = MaxCapacity;
        if (maxResults == 0) return;

        heap.Clear();

        var queryNormalized = caseSensitive ? query : query.ToLowerInvariant();

        foreach (var name in candidates)
        {
            var candidateNormalized = caseSensitive ? name : name.ToLowerInvariant();
            var score = Similarity(queryNormalized, candidateNormalized);
            if (score <= 0f) continue;

            if (heap.Count < maxResults)
            {
                heap.Push((name, score));
            }
            else
            {
                var min = heap.Peek();
                if (score > min.Score
                    || (score == min.Score
                        && string.Compare(name, min.Name, StringComparison.Ordinal) < 0))
                {
                    heap.Pop();
                    heap.Push((name, score));
                }
            }
        }

        // Drain the heap (worst-first at indices 0..count-1).
        var count = heap.Count;
        for (var i = 0; i < count; i++) drainBuffer[i] = heap.Pop();

        // Append best-first (reverse of drain order).
        for (var i = count - 1; i >= 0; i--) results.Add(drainBuffer[i]);
    }
}
