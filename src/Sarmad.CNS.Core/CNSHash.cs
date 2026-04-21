// Copyright (c) 2026 sbay-dev. Licensed under Apache-2.0.

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Sarmad.CNS.Core;

/// <summary>
/// نظام الهاش الفريد لـ CNS
/// كل مفهوم = Hash(K_Space_Id + Concept)
/// </summary>
public static class CNSHash
{
    /// <summary>
    /// توليد هاش فريد لمفهوم في فضاء معين
    /// </summary>
    public static string Generate(string spaceId, string concept)
    {
        var combined = $"{spaceId}:{concept}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(combined));
        return Convert.ToHexString(hashBytes)[..16];
    }

    /// <summary>
    /// توليد هاش للفضاء نفسه
    /// </summary>
    public static string GenerateSpaceHash(string spaceName, int dimensions)
    {
        var combined = $"SPACE:{spaceName}:D{dimensions}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(combined));
        return Convert.ToHexString(hashBytes)[..12];
    }

    /// <summary>
    /// فحص إذا كان الهاش موجود (للكشف عن الشرود)
    /// </summary>
    public static bool IsConceptRegistered(string conceptHash, Dictionary<string, ConceptRegistration> registry)
    {
        return registry.ContainsKey(conceptHash);
    }
}

/// <summary>
/// تسجيل المفهوم في النظام العام
/// </summary>
public record ConceptRegistration
{
    public required string ConceptHash { get; init; }
    public required string SpaceId { get; init; }
    public required string SpaceName { get; init; }
    public required string Concept { get; init; }
    public required DateTime RegisteredAt { get; init; }
    public required double[] Coordinates { get; init; }
    public int AccessCount { get; set; }
}
