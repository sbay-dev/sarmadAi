// Copyright (c) 2026 sbay-dev. Licensed under Apache-2.0.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Sarmad.CNS.Core;

/// <summary>
/// نظام كشف الشرود (Drift Detection)
/// يكتشف عندما يتكرر مفهوم في فضاءات مختلفة
/// </summary>
public class DriftDetector
{
    private readonly Dictionary<string, ConceptRegistration> _globalRegistry;
    private readonly List<DriftEvent> _driftHistory = new();

    public DriftDetector(Dictionary<string, ConceptRegistration> globalRegistry)
    {
        _globalRegistry = globalRegistry;
    }

    /// <summary>
    /// محاولة تسجيل مفهوم - يُرجع null إذا كان موجوداً (شرود)
    /// </summary>
    public DriftResult AttemptRegister(string spaceId, string spaceName, string concept, double[] coordinates)
    {
        var conceptHash = CNSHash.Generate(spaceId, concept);

        if (_globalRegistry.TryGetValue(conceptHash, out var existing))
        {
            var driftEvent = new DriftEvent
            {
                ConceptHash = conceptHash,
                Concept = concept,
                OriginalSpaceId = existing.SpaceId,
                OriginalSpaceName = existing.SpaceName,
                TargetSpaceId = spaceId,
                TargetSpaceName = spaceName,
                DetectedAt = DateTime.UtcNow,
                OriginalCoordinates = existing.Coordinates,
                AttemptedCoordinates = coordinates
            };

            _driftHistory.Add(driftEvent);
            existing.AccessCount++;

            return new DriftResult
            {
                IsDrift = true,
                ExistingRegistration = existing,
                DriftEvent = driftEvent
            };
        }

        return new DriftResult { IsDrift = false };
    }

    /// <summary>
    /// الحصول على سلسلة الشرود لمفهوم معين
    /// </summary>
    public List<DriftEvent> GetDriftChain(string concept)
    {
        return _driftHistory
            .Where(d => d.Concept == concept)
            .OrderBy(d => d.DetectedAt)
            .ToList();
    }

    /// <summary>
    /// الحصول على الفضاءات المتصلة (عبر الشرود)
    /// </summary>
    public Dictionary<string, List<string>> GetSpaceConnections()
    {
        var connections = new Dictionary<string, List<string>>();

        foreach (var drift in _driftHistory)
        {
            if (!connections.ContainsKey(drift.OriginalSpaceName))
                connections[drift.OriginalSpaceName] = new();

            if (!connections[drift.OriginalSpaceName].Contains(drift.TargetSpaceName))
                connections[drift.OriginalSpaceName].Add(drift.TargetSpaceName);
        }

        return connections;
    }

    /// <summary>
    /// تحليل أنماط الشرود
    /// </summary>
    public DriftAnalysis AnalyzeDrifts()
    {
        var totalDrifts = _driftHistory.Count;
        var uniqueConcepts = _driftHistory.Select(d => d.Concept).Distinct().Count();
        var spaceConnections = GetSpaceConnections();

        var mostDriftedConcept = _driftHistory
            .GroupBy(d => d.Concept)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        return new DriftAnalysis
        {
            TotalDrifts = totalDrifts,
            UniqueDriftedConcepts = uniqueConcepts,
            MostDriftedConcept = mostDriftedConcept?.Key ?? string.Empty,
            MostDriftedCount = mostDriftedConcept?.Count() ?? 0,
            SpaceConnectionCount = spaceConnections.Count,
            DriftHistory = _driftHistory
        };
    }

    public void PrintDriftStatistics()
    {
        var analysis = AnalyzeDrifts();

        Console.WriteLine("\n🌀 إحصائيات الشرود:");
        Console.WriteLine($"   • إجمالي الشرود: {analysis.TotalDrifts}");
        Console.WriteLine($"   • مفاهيم فريدة: {analysis.UniqueDriftedConcepts}");

        if (!string.IsNullOrEmpty(analysis.MostDriftedConcept))
        {
            Console.WriteLine($"   • الأكثر شروداً: '{analysis.MostDriftedConcept}' ({analysis.MostDriftedCount} مرة)");
        }

        Console.WriteLine($"   • الفضاءات المتصلة: {analysis.SpaceConnectionCount}");
    }
}

/// <summary>
/// حدث شرود
/// </summary>
public record DriftEvent
{
    public required string ConceptHash { get; init; }
    public required string Concept { get; init; }
    public required string OriginalSpaceId { get; init; }
    public required string OriginalSpaceName { get; init; }
    public required string TargetSpaceId { get; init; }
    public required string TargetSpaceName { get; init; }
    public required DateTime DetectedAt { get; init; }
    public required double[] OriginalCoordinates { get; init; }
    public required double[] AttemptedCoordinates { get; init; }
}

/// <summary>
/// نتيجة محاولة التسجيل
/// </summary>
public record DriftResult
{
    public required bool IsDrift { get; init; }
    public ConceptRegistration? ExistingRegistration { get; init; }
    public DriftEvent? DriftEvent { get; init; }
}

/// <summary>
/// تحليل أنماط الشرود
/// </summary>
public record DriftAnalysis
{
    public required int TotalDrifts { get; init; }
    public required int UniqueDriftedConcepts { get; init; }
    public required string MostDriftedConcept { get; init; }
    public required int MostDriftedCount { get; init; }
    public required int SpaceConnectionCount { get; init; }
    public required List<DriftEvent> DriftHistory { get; init; }
}
