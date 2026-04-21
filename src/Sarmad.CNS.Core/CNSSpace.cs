// Copyright (c) 2026 sbay-dev. Licensed under Apache-2.0.

namespace Sarmad.CNS.Core;

/// <summary>
/// A CNS Space is an N-dimensional semantic space containing concept nodes.
/// Each space defines a geometric structure for encoding domain-specific concepts.
///
/// Example: A 12-dimensional space with axes for emotional depth, intellectual
/// agency, spiritual authority, temporal presence, etc.
/// </summary>
public class CNSSpace
{
    /// <summary>Unique identifier for this space</summary>
    public required string SpaceId { get; init; }

    /// <summary>Human-readable name (e.g., "Arabic Concepts Space")</summary>
    public required string SpaceName { get; init; }

    /// <summary>Number of dimensions (e.g., 12 for the standard CNS coordinate system)</summary>
    public required int Dimensions { get; init; }

    /// <summary>All concept nodes indexed by ConceptHash</summary>
    public Dictionary<string, CNSNode> Nodes { get; } = new();

    /// <summary>Pairs of semantically opposite concepts (e.g., حب/كراهية)</summary>
    public List<PolarityPair> PolarityPairs { get; } = new();

    /// <summary>Detected gaps in the concept coverage</summary>
    public List<RoleGap> RoleGaps { get; } = new();

    /// <summary>Cross-space concept tracking for drift detection</summary>
    public Dictionary<string, ConceptRegistration> GlobalRegistry { get; } = new();

    /// <summary>Add a concept node to this space</summary>
    public void AddNode(CNSNode node) => Nodes[node.ConceptHash] = node;

    /// <summary>Find nearest concepts by cosine similarity in coordinate space</summary>
    public IEnumerable<(CNSNode Node, double Similarity)> FindNearest(
        double[] coordinates, int topK = 5)
    {
        return Nodes.Values
            .Select(n => (Node: n, Similarity: CosineSimilarity(coordinates, n.Coordinates)))
            .OrderByDescending(x => x.Similarity)
            .Take(topK);
    }

    /// <summary>Cosine similarity between two vectors</summary>
    public static double CosineSimilarity(double[] a, double[] b)
    {
        double dot = 0, normA = 0, normB = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }
        return dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }
}

/// <summary>A pair of semantically opposite concepts</summary>
public record PolarityPair(string PositiveHash, string NegativeHash, double Strength);

/// <summary>A detected gap in concept coverage</summary>
public record RoleGap(string Description, double[] ExpectedCoordinates, double Confidence);

/// <summary>Cross-space concept registration for drift detection</summary>
public record ConceptRegistration(string SpaceId, string ConceptHash, DateTime RegisteredAt);
