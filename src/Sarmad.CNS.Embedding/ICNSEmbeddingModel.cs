// Copyright (c) 2026 sbay-dev. Licensed under Apache-2.0.

namespace Sarmad.CNS.Embedding;

/// <summary>
/// CNS Embedding Model interface — CNSEmbeddingModelV2.
///
/// Architecture:
///   Input: 12 CNS coordinates [B, 12] + 1 polarity value [B, 1]
///   → Coordinate Encoder: 12 → 256 → 512 → 768 (LayerNorm + GELU)
///   → Polarity Encoder: 1 → 64 → 128 → 256 (LayerNorm + GELU)
///   → Fusion: cat([B, 768], [B, 256]) → [B, 1024] → Linear(1024, 768) → LayerNorm → GELU
///   → 4× Pre-Norm Transformer (8 heads, head_dim=96, FFN 768→1536→768)
///   → 3 Output Heads:
///       Embedding: 768 → 768 → Tanh → L2-Normalize
///       Polarity:  768 → 256 → 64 → 1 → Tanh
///       NodeType:  768 → 128 → 2
///
/// Total Parameters: 21,765,123
/// Model Size: 83 MB (PyTorch) / 82.1 MB (ONNX)
/// </summary>
public interface ICNSEmbeddingModel : IDisposable
{
    /// <summary>Embedding vector dimensionality (default: 768)</summary>
    int EmbeddingDim { get; }

    /// <summary>
    /// Generate a 768-dimensional L2-normalized embedding from text.
    /// Uses Arabic tokenization with stop-word removal, then mean-pooling.
    /// </summary>
    double[] Embed(string text);

    /// <summary>Cosine similarity between two embedding vectors</summary>
    double CosineSimilarity(double[] vec1, double[] vec2);

    /// <summary>
    /// Semantic search: find the top-K most similar concepts.
    /// Returns (Concept, CosineScore) pairs sorted descending.
    /// </summary>
    List<(string Concept, double Score)> Search(string query, int topK = 5);
}

/// <summary>
/// Configuration for the CNS Embedding Model.
/// </summary>
public record CNSEmbeddingConfig
{
    /// <summary>Output embedding dimension (supports Matryoshka: 64, 128, 256, 512, 768)</summary>
    public int EmbeddingDim { get; init; } = 768;

    /// <summary>Number of Transformer attention layers</summary>
    public int AttentionLayers { get; init; } = 4;

    /// <summary>Number of attention heads per layer</summary>
    public int AttentionHeads { get; init; } = 8;

    /// <summary>Dimension per attention head</summary>
    public int HeadDim { get; init; } = 96;

    /// <summary>Feed-forward network intermediate dimension</summary>
    public int FFNDim { get; init; } = 1536;

    /// <summary>Number of input CNS coordinate dimensions</summary>
    public int InputCoordinates { get; init; } = 12;

    /// <summary>Path to ONNX model file (optional, falls back to manual computation)</summary>
    public string? OnnxModelPath { get; init; }

    /// <summary>Total trainable parameters</summary>
    public long TotalParameters => 21_765_123;
}
