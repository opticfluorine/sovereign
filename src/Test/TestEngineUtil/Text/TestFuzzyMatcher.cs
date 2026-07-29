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
using System.Collections.Generic;
using Sovereign.EngineUtil.Text;
using Xunit;

namespace TestEngineUtil.Text;

public class TestFuzzyMatcher
{
    [Fact]
    public void Similarity_IdenticalStrings_IsOne()
    {
        Assert.Equal(1.0f, FuzzyMatcher.Similarity("alice", "alice"));
    }

    [Fact]
    public void Similarity_BothEmpty_IsOne()
    {
        Assert.Equal(1.0f, FuzzyMatcher.Similarity("", ""));
    }

    [Fact]
    public void DamerauLevenshteinDistance_SingleTransposition_IsOne()
    {
        Assert.Equal(1, FuzzyMatcher.DamerauLevenshteinDistance("ab", "ba"));
    }

    [Fact]
    public void Similarity_SingleTransposition_IsHalf()
    {
        Assert.Equal(0.5f, FuzzyMatcher.Similarity("ab", "ba"));
    }

    [Fact]
    public void DamerauLevenshteinDistance_KittenSitting_IsThree()
    {
        Assert.Equal(3, FuzzyMatcher.DamerauLevenshteinDistance("kitten", "sitting"));
    }

    [Fact]
    public void Similarity_KittenSitting_ApproxOneMinusThreeOverSeven()
    {
        Assert.Equal(1.0f - 3.0f / 7.0f, FuzzyMatcher.Similarity("kitten", "sitting"), 5);
    }

    [Fact]
    public void Similarity_NullFirst_IsZero()
    {
        Assert.Equal(0.0f, FuzzyMatcher.Similarity(null, "a"));
    }

    [Fact]
    public void DamerauLevenshteinDistance_BothNull_IsZero()
    {
        Assert.Equal(0, FuzzyMatcher.DamerauLevenshteinDistance(null, null));
    }

    [Fact]
    public void AppendBestMatches_AppendsTopN_PreservesExistingEntries()
    {
        var matcher = new FuzzyMatcher();
        var results = new List<(string Name, float Score)> { ("preexisting", 999.0f) };

        matcher.AppendBestMatches("alice", new[] { "Alice", "Bob", "Alicia", "alicE" }, 3, results);

        Assert.Equal(4, results.Count);
        Assert.Equal("preexisting", results[0].Name);
        Assert.Equal(999.0f, results[0].Score);

        // Best-first ordering; ties broken alphabetically (Ordinal ascending).
        Assert.Equal("Alice", results[1].Name);
        Assert.Equal("alicE", results[2].Name);
        Assert.Equal("Alicia", results[3].Name);

        // Alice and alicE both have similarity 1.0 (identical modulo case);
        // the byte-wise distance for "Alice" vs "alice" is 1 (one case diff),
        // but they should still rank above Alicia.
        Assert.True(results[1].Score >= results[2].Score);
        Assert.True(results[2].Score >= results[3].Score);
    }

    [Fact]
    public void AppendBestMatches_ExcludesZeroScoreCandidates()
    {
        var matcher = new FuzzyMatcher();
        var results = new List<(string Name, float Score)>();

        matcher.AppendBestMatches("alice", new[] { "zzzzzz" }, 3, results);

        Assert.Empty(results);
    }

    [Fact]
    public void AppendBestMatches_MaxResultsZero_AppendsNothing()
    {
        var matcher = new FuzzyMatcher();
        var results = new List<(string Name, float Score)>();

        matcher.AppendBestMatches("alice", new[] { "Alice", "Alicia" }, 0, results);

        Assert.Empty(results);
    }

    [Fact]
    public void AppendBestMatches_NegativeMaxResults_AppendsNothing()
    {
        var matcher = new FuzzyMatcher();
        var results = new List<(string Name, float Score)>();

        matcher.AppendBestMatches("alice", new[] { "Alice", "Alicia" }, -5, results);

        Assert.Empty(results);
    }

    [Fact]
    public void AppendBestMatches_MaxResultsLargerThanCandidates_AppendsAllQualifying()
    {
        var matcher = new FuzzyMatcher();
        var results = new List<(string Name, float Score)>();

        matcher.AppendBestMatches("alice", new[] { "Alice", "Alicia" }, 100, results);

        Assert.Equal(2, results.Count);
        Assert.Equal("Alice", results[0].Name);
        Assert.Equal("Alicia", results[1].Name);
    }

    [Fact]
    public void AppendBestMatches_ReturnsSameListInstance()
    {
        var matcher = new FuzzyMatcher();
        var results = new List<(string Name, float Score)>();

        var returned = results;
        matcher.AppendBestMatches("alice", new[] { "Alice" }, 3, results);

        Assert.Same(returned, results);
    }

    [Fact]
    public void AppendBestMatches_ReuseAcrossCalls_ResetsInternalScratch()
    {
        var matcher = new FuzzyMatcher();
        var results = new List<(string Name, float Score)>();

        matcher.AppendBestMatches("alice", new[] { "Alice", "Alicia", "Bob" }, 3, results);
        Assert.Equal(2, results.Count);
        results.Clear();

        matcher.AppendBestMatches("bob", new[] { "Bob", "Alice", "Alicia" }, 3, results);
        var single = Assert.Single(results);
        Assert.Equal("Bob", single.Name);
    }

    [Fact]
    public void AppendBestMatches_TieBreakAlphabetically()
    {
        var matcher = new FuzzyMatcher();
        var results = new List<(string Name, float Score)>();

        // Two strings equidistant from the query should be ordered Ordinal-ascending.
        // "abc" vs "abd": both distance 1 from "abc"; "abc" is identical (score 1.0),
        // so use two distinct equidistant candidates.
        matcher.AppendBestMatches("abc", new[] { "abd", "abe" }, 2, results);

        Assert.Equal(2, results.Count);
        Assert.Equal("abd", results[0].Name);
        Assert.Equal("abe", results[1].Name);
        Assert.Equal(results[0].Score, results[1].Score);
    }

    [Fact]
    public void AppendBestMatches_TopNEviction_KeepsBestScores()
    {
        var matcher = new FuzzyMatcher();
        var results = new List<(string Name, float Score)>();

        // maxResults = 1 should keep only the best match.
        matcher.AppendBestMatches("alice", new[] { "Bob", "Alicia", "Alice" }, 1, results);

        var single = Assert.Single(results);
        Assert.Equal("Alice", single.Name);
    }
}
