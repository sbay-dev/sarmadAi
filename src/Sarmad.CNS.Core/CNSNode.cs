// Copyright (c) 2026 sbay-dev. Licensed under Apache-2.0.
// Sarmad — Cubic Neural Statistics

namespace Sarmad.CNS.Core;

/// <summary>
/// A concept node in CNS space. Each node represents a semantic concept
/// as a point in N-dimensional geometric space with polarity metadata.
/// </summary>
public record CNSNode
{
    /// <summary>SHA-256 based unique identifier: Hash(SpaceId + Concept)[0..16]</summary>
    public required string ConceptHash { get; init; }

    /// <summary>Human-readable concept text (e.g., "رحمة", "إيمان")</summary>
    public required string Concept { get; init; }

    /// <summary>N-dimensional coordinates in the CNS space (e.g., 12 dimensions)</summary>
    public required double[] Coordinates { get; init; }

    /// <summary>Semantic polarity: -1.0 (negative) to +1.0 (positive)</summary>
    public double Polarity { get; init; }

    /// <summary>Cultural significance weight in context</summary>
    public double CulturalWeight { get; init; } = 1.0;

    /// <summary>Node classification type</summary>
    public NodeType Type { get; init; } = NodeType.Core;

    /// <summary>Semantically equivalent concepts</summary>
    public List<string> Synonyms { get; init; } = new();
}

/// <summary>
/// Classification of concept nodes within a CNS space.
/// </summary>
public enum NodeType
{
    /// <summary>Essential, foundational concept</summary>
    Core,
    /// <summary>Observable, measurable attribute</summary>
    Surface,
    /// <summary>Implied or inferred concept (not directly observed)</summary>
    Phantom,
    /// <summary>Concept that evolves across contexts</summary>
    Transition,
    /// <summary>Exception or anomaly in the space</summary>
    Irregular
}
