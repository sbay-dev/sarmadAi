// Copyright (c) 2026 sbay-dev. Licensed under Apache-2.0.

namespace Sarmad.DMPS;

/// <summary>
/// Core abstraction for AI model providers in the Dynamic Model Provider System (DMPS).
///
/// DMPS enables seamless switching between model providers:
///   - Local: Ollama, LM Studio
///   - Cloud: Azure OpenAI, OpenAI
///   - Edge: ONNX Runtime, TensorFlow Lite
///   - Custom: User-defined providers
///
/// Each provider implements health checking, model listing, text generation,
/// streaming, embeddings, and vision analysis through a unified interface.
/// </summary>
public interface IModelProvider
{
    /// <summary>Provider name identifier</summary>
    string ProviderName { get; }

    /// <summary>Provider deployment type</summary>
    ModelProviderType ProviderType { get; }

    /// <summary>Check provider health and connectivity</summary>
    Task<HealthCheckResult> CheckHealthAsync(CancellationToken ct = default);

    /// <summary>List all models available through this provider</summary>
    Task<List<ModelInfo>> ListAvailableModelsAsync(CancellationToken ct = default);

    /// <summary>Generate a text completion</summary>
    Task<CompletionResult> GenerateCompletionAsync(CompletionRequest request, CancellationToken ct = default);

    /// <summary>Stream a text completion token-by-token</summary>
    IAsyncEnumerable<string> StreamCompletionAsync(CompletionRequest request, CancellationToken ct = default);

    /// <summary>Generate embedding vectors for one or more texts</summary>
    Task<EmbeddingResult> GenerateEmbeddingsAsync(EmbeddingRequest request, CancellationToken ct = default);

    /// <summary>Analyze an image with a vision model</summary>
    Task<VisionResult> AnalyzeImageAsync(VisionRequest request, CancellationToken ct = default);
}

/// <summary>Provider deployment type classification</summary>
public enum ModelProviderType
{
    /// <summary>Locally hosted models (Ollama, LM Studio)</summary>
    Local,
    /// <summary>Cloud-hosted models (Azure OpenAI, OpenAI API)</summary>
    Cloud,
    /// <summary>Edge-deployed models (ONNX Runtime, TFLite)</summary>
    Edge,
    /// <summary>Custom user-defined provider</summary>
    Custom
}
