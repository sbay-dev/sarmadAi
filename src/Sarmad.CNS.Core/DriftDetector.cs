// Copyright (c) 2026 sbay-dev. Licensed under Apache-2.0.

namespace Sarmad.CNS.Core;

/// <summary>
/// Detects concept drift and repetition across multiple CNS spaces.
/// When the same concept appears in multiple spaces with different coordinates,
/// the detector flags potential semantic inconsistencies.
/// </summary>
public class DriftDetector
{
    private readonly Dictionary<string, List<(string SpaceId, double[] Coordinates)>> _registry = new();

    /// <summary>Register a concept's coordinates in a given space</summary>
    public void Register(string concept, string spaceId, double[] coordinates)
    {
        var key = concept.ToLowerInvariant().Trim();
        if (!_registry.ContainsKey(key))
            _registry[key] = new();
        _registry[key].Add((spaceId, coordinates));
    }

    /// <summary>
    /// Detect drift: concepts that exist in multiple spaces with
    /// cosine similarity below the threshold.
    /// </summary>
    public IEnumerable<DriftReport> DetectDrift(double similarityThreshold = 0.85)
    {
        foreach (var (concept, entries) in _registry)
        {
            if (entries.Count < 2) continue;
            for (int i = 0; i < entries.Count; i++)
            for (int j = i + 1; j < entries.Count; j++)
            {
                var sim = CNSSpace.CosineSimilarity(entries[i].Coordinates, entries[j].Coordinates);
                if (sim < similarityThreshold)
                    yield return new DriftReport(concept, entries[i].SpaceId,
                        entries[j].SpaceId, sim);
            }
        }
    }
}

/// <summary>A detected semantic drift between two spaces</summary>
public record DriftReport(string Concept, string SpaceA, string SpaceB, double Similarity);
