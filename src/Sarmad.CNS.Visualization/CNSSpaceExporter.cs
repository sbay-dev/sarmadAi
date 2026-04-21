// Copyright (c) 2026 sbay-dev. Licensed under Apache-2.0.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Sarmad.CNS.Core;

namespace Sarmad.CNS.Visualization;

/// <summary>
/// يُصدِّر بيانات فضاء CNS إلى JSON لعرضها ثلاثي الأبعاد في المتصفح.
/// يستخدم PCA يدوي بسيط لإسقاط الأبعاد N إلى 3 أبعاد قابلة للرسم.
/// </summary>
public static class CNSSpaceExporter
{
    public static void Export(CNSSpace space, string outputPath = "wwwroot/cns_space.json")
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        var nodes3D = Project3D(space);
        var links = BuildLinks(space, nodes3D);
        var axes = BuildAxes(space);

        var payload = new SpacePayload
        {
            SpaceId = space.SpaceId,
            SpaceName = space.SpaceName,
            Dimensions = space.Dimensions,
            ExportedAt = DateTime.UtcNow.ToString("O"),
            Stats = new SpaceStats
            {
                TotalNodes = space.Nodes.Count,
                CoreNodes = space.Nodes.Values.Count(n => n.Type == NodeType.Core),
                SurfaceNodes = space.Nodes.Values.Count(n => n.Type == NodeType.Surface),
                PolarityPairs = space.PolarityPairs.Count,
                RoleGaps = space.RoleGaps.Count,
                PositiveNodes = space.Nodes.Values.Count(n => n.Polarity > 0.1),
                NegativeNodes = space.Nodes.Values.Count(n => n.Polarity < -0.1),
                NeutralNodes = space.Nodes.Values.Count(n => n.Polarity is >= -0.1 and <= 0.1)
            },
            Nodes = nodes3D,
            Links = links,
            Axes = axes
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        File.WriteAllText(outputPath, JsonSerializer.Serialize(payload, options));
    }

    private static List<Node3D> Project3D(CNSSpace space)
    {
        var nodes = space.Nodes.Values.ToList();
        if (nodes.Count == 0) return [];

        int dims = space.Dimensions;
        var mean = new double[dims];

        foreach (var n in nodes)
            for (int d = 0; d < dims; d++)
                mean[d] += n.Coordinates[d];

        for (int d = 0; d < dims; d++)
            mean[d] /= nodes.Count;

        var variances = new double[dims];
        foreach (var n in nodes)
            for (int d = 0; d < dims; d++)
            {
                var diff = n.Coordinates[d] - mean[d];
                variances[d] += diff * diff;
            }

        var topAxes = variances
            .Select((v, i) => (v, i))
            .OrderByDescending(x => x.v)
            .Take(3)
            .Select(x => x.i)
            .ToArray();

        while (topAxes.Length < 3)
            topAxes = [.. topAxes, 0];

        static double Scale(double v) => Math.Clamp(v * 2.0, -1.0, 1.0);

        return nodes.Select(n => new Node3D
        {
            Id = n.ConceptHash,
            Label = n.Concept,
            X = Scale(n.Coordinates[topAxes[0]] - mean[topAxes[0]]),
            Y = Scale(n.Coordinates[topAxes[1]] - mean[topAxes[1]]),
            Z = Scale(n.Coordinates[topAxes[2]] - mean[topAxes[2]]),
            Polarity = n.Polarity,
            Type = n.Type.ToString(),
            DimWeights = n.Coordinates,
            HasAntonyms = n.Antonyms.Count > 0,
            AntonymCount = n.Antonyms.Count,
            CulturalWeight = n.CulturalWeight,
            Color = n.Polarity > 0.1 ? "#4ade80"
                  : n.Polarity < -0.1 ? "#f87171"
                  : "#e2e8f0",
            Size = n.Type switch
            {
                NodeType.Core => 1.4,
                NodeType.Surface => 0.9,
                _ => 0.7
            }
        }).ToList();
    }

    private static List<Link3D> BuildLinks(CNSSpace space, List<Node3D> nodes3D)
    {
        var idMap = nodes3D.ToDictionary(n => n.Label, n => n.Id);
        var links = new List<Link3D>();

        foreach (var pair in space.PolarityPairs)
        {
            if (idMap.TryGetValue(pair.Positive, out var srcId) &&
                idMap.TryGetValue(pair.Negative, out var tgtId))
            {
                links.Add(new Link3D
                {
                    Source = srcId,
                    Target = tgtId,
                    Type = "polarity",
                    Strength = pair.Strength,
                    Color = "#a78bfa",
                    Label = $"{pair.Positive} ↔ {pair.Negative}"
                });
            }
        }

        foreach (var gap in space.RoleGaps)
        {
            if (idMap.TryGetValue(gap.SourceRole, out var srcId) &&
                idMap.TryGetValue(gap.TargetRole, out var tgtId))
            {
                links.Add(new Link3D
                {
                    Source = srcId,
                    Target = tgtId,
                    Type = "roleGap",
                    Strength = gap.TransitionProbability,
                    Color = "#fbbf24",
                    Label = $"{gap.SourceRole} → {gap.TargetRole}"
                });
            }
        }

        return links;
    }

    private static List<AxisInfo> BuildAxes(CNSSpace space)
    {
        return Enumerable.Range(0, space.Dimensions).Select(i => new AxisInfo
        {
            Index = i,
            ConceptCount = space.Nodes.Values.Count(n => n.Coordinates.Length > i && n.Coordinates[i] >= 0.4)
        }).ToList();
    }
}

public record SpacePayload
{
    [JsonPropertyName("spaceId")] public required string SpaceId { get; init; }
    [JsonPropertyName("spaceName")] public required string SpaceName { get; init; }
    [JsonPropertyName("dimensions")] public required int Dimensions { get; init; }
    [JsonPropertyName("exportedAt")] public required string ExportedAt { get; init; }
    [JsonPropertyName("stats")] public required SpaceStats Stats { get; init; }
    [JsonPropertyName("nodes")] public required List<Node3D> Nodes { get; init; }
    [JsonPropertyName("links")] public required List<Link3D> Links { get; init; }
    [JsonPropertyName("axes")] public required List<AxisInfo> Axes { get; init; }
}

public record SpaceStats
{
    [JsonPropertyName("totalNodes")] public int TotalNodes { get; init; }
    [JsonPropertyName("coreNodes")] public int CoreNodes { get; init; }
    [JsonPropertyName("surfaceNodes")] public int SurfaceNodes { get; init; }
    [JsonPropertyName("polarityPairs")] public int PolarityPairs { get; init; }
    [JsonPropertyName("roleGaps")] public int RoleGaps { get; init; }
    [JsonPropertyName("positiveNodes")] public int PositiveNodes { get; init; }
    [JsonPropertyName("negativeNodes")] public int NegativeNodes { get; init; }
    [JsonPropertyName("neutralNodes")] public int NeutralNodes { get; init; }
}

public record Node3D
{
    [JsonPropertyName("id")] public required string Id { get; init; }
    [JsonPropertyName("label")] public required string Label { get; init; }
    [JsonPropertyName("x")] public required double X { get; init; }
    [JsonPropertyName("y")] public required double Y { get; init; }
    [JsonPropertyName("z")] public required double Z { get; init; }
    [JsonPropertyName("polarity")] public required double Polarity { get; init; }
    [JsonPropertyName("type")] public required string Type { get; init; }
    [JsonPropertyName("color")] public required string Color { get; init; }
    [JsonPropertyName("size")] public required double Size { get; init; }
    [JsonPropertyName("dimWeights")] public required double[] DimWeights { get; init; }
    [JsonPropertyName("hasAntonyms")] public required bool HasAntonyms { get; init; }
    [JsonPropertyName("antonymCount")] public required int AntonymCount { get; init; }
    [JsonPropertyName("culturalWeight")] public required double CulturalWeight { get; init; }
}

public record Link3D
{
    [JsonPropertyName("source")] public required string Source { get; init; }
    [JsonPropertyName("target")] public required string Target { get; init; }
    [JsonPropertyName("type")] public required string Type { get; init; }
    [JsonPropertyName("strength")] public required double Strength { get; init; }
    [JsonPropertyName("color")] public required string Color { get; init; }
    [JsonPropertyName("label")] public required string Label { get; init; }
}

public record AxisInfo
{
    [JsonPropertyName("index")] public required int Index { get; init; }
    [JsonPropertyName("conceptCount")] public required int ConceptCount { get; init; }
}
