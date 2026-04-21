// Copyright (c) 2026 sbay-dev. Licensed under Apache-2.0.
// Sarmad — Cubic Neural Statistics

using System;
using System.Collections.Generic;

namespace Sarmad.CNS.Core;

/// <summary>
/// عقدة في فضاء CNS
/// </summary>
public class CNSNode
{
    public required string NodeId { get; init; }
    public required string ConceptHash { get; init; }
    public required string Concept { get; init; }
    public required double[] Coordinates { get; init; }
    public required NodeType Type { get; init; }
    public Dictionary<string, double> Neighbors { get; init; } = new();
    public double Polarity { get; set; }
    public List<string> Antonyms { get; init; } = new();
    public List<string> Synonyms { get; init; } = new();
    public double CulturalWeight { get; set; } = 1.0;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}

public enum NodeType
{
    Core,
    Surface,
    Phantom,
    Transition,
    Irregular
}

/// <summary>
/// زوج قطبي (تناقض)
/// </summary>
public record PolarityPair
{
    public required string Positive { get; init; }
    public required string Negative { get; init; }
    public required int AxisDimension { get; init; }
    public double Strength { get; init; } = 1.0;
}

/// <summary>
/// فجوة بين الأدوار (للشرود)
/// </summary>
public record RoleGap
{
    public required string GapId { get; init; }
    public required string SourceRole { get; init; }
    public required string TargetRole { get; init; }
    public required string[] TransitionConcepts { get; init; }
    public required double TransitionProbability { get; init; }
    public string? TargetSpaceId { get; init; }
}
