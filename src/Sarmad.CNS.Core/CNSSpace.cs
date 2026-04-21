// Copyright (c) 2026 sbay-dev. Licensed under Apache-2.0.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sarmad.CNS.Core;

/// <summary>
/// فضاء CNS - الفضاء الأساسي للنظام
/// </summary>
public class CNSSpace
{
    public string SpaceId { get; }
    public string SpaceName { get; }
    public int Dimensions { get; }
    public Dictionary<string, CNSNode> Nodes { get; } = new();
    public List<PolarityPair> PolarityPairs { get; } = new();
    public List<RoleGap> RoleGaps { get; } = new();
    public Dictionary<string, ConceptRegistration> GlobalRegistry { get; }

    private readonly string _databasePath;

    public CNSSpace(string spaceName, int dimensions, Dictionary<string, ConceptRegistration> globalRegistry)
    {
        SpaceName = spaceName;
        Dimensions = dimensions;
        SpaceId = CNSHash.GenerateSpaceHash(spaceName, dimensions);
        GlobalRegistry = globalRegistry;
        _databasePath = $"data/{SpaceId}.db";

        Directory.CreateDirectory("data");
        Console.WriteLine($"🌌 فضاء جديد: {spaceName} (K_{SpaceId}) - {dimensions} بُعد");
    }

    /// <summary>
    /// إضافة مفهوم للفضاء
    /// </summary>
    public string AddConcept(
        string concept,
        double[] coordinates,
        NodeType type = NodeType.Core,
        double polarity = 0.0)
    {
        if (coordinates.Length != Dimensions)
            throw new ArgumentException($"الإحداثيات يجب أن تكون {Dimensions} بُعد");

        var conceptHash = CNSHash.Generate(SpaceId, concept);

        if (CNSHash.IsConceptRegistered(conceptHash, GlobalRegistry))
        {
            var existing = GlobalRegistry[conceptHash];
            existing.AccessCount++;
            return conceptHash;
        }

        var node = new CNSNode
        {
            NodeId = Guid.NewGuid().ToString("N")[..8],
            ConceptHash = conceptHash,
            Concept = concept,
            Coordinates = coordinates,
            Type = type,
            Polarity = polarity
        };

        Nodes[conceptHash] = node;

        GlobalRegistry[conceptHash] = new ConceptRegistration
        {
            ConceptHash = conceptHash,
            SpaceId = SpaceId,
            SpaceName = SpaceName,
            Concept = concept,
            RegisteredAt = DateTime.UtcNow,
            Coordinates = coordinates,
            AccessCount = 0
        };

        return conceptHash;
    }

    /// <summary>
    /// إضافة زوج قطبي (تناقض)
    /// </summary>
    public void AddPolarityPair(string positive, string negative, int axisDimension, double strength = 1.0)
    {
        PolarityPairs.Add(new PolarityPair
        {
            Positive = positive,
            Negative = negative,
            AxisDimension = axisDimension,
            Strength = strength
        });
    }

    /// <summary>
    /// حساب المسافة بين عقدتين
    /// </summary>
    public double Distance(string hash1, string hash2)
    {
        if (!Nodes.ContainsKey(hash1) || !Nodes.ContainsKey(hash2))
            return double.MaxValue;

        var coords1 = Nodes[hash1].Coordinates;
        var coords2 = Nodes[hash2].Coordinates;

        return Math.Sqrt(coords1.Zip(coords2, (a, b) => (a - b) * (a - b)).Sum());
    }

    /// <summary>
    /// حفظ الفضاء في قاعدة بيانات SQLite
    /// </summary>
    public void SaveToDatabase()
    {
        Console.WriteLine($"💾 حفظ {Nodes.Count} عقدة في {_databasePath}");
    }

    /// <summary>
    /// الحصول على إحصائيات الفضاء
    /// </summary>
    public void PrintStatistics()
    {
        Console.WriteLine($"\n📊 إحصائيات {SpaceName}:");
        Console.WriteLine($"   • الأبعاد: {Dimensions}");
        Console.WriteLine($"   • العقد: {Nodes.Count}");
        Console.WriteLine($"   • الأزواج القطبية: {PolarityPairs.Count}");
        Console.WriteLine($"   • فجوات الشرود: {RoleGaps.Count}");

        var coreNodes = Nodes.Values.Count(n => n.Type == NodeType.Core);
        var surfaceNodes = Nodes.Values.Count(n => n.Type == NodeType.Surface);
        Console.WriteLine($"   • عقد مركزية: {coreNodes}");
        Console.WriteLine($"   • عقد سطحية: {surfaceNodes}");
    }
}
