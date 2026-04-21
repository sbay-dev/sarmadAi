// Copyright (c) 2026 sbay-dev. Licensed under Apache-2.0.

namespace Sarmad.DMPS;

// ──── Request Models ────

/// <summary>Text completion request parameters</summary>
public record CompletionRequest
{
    public required string Prompt { get; init; }
    public string? SystemPrompt { get; init; }
    public string? ModelName { get; init; }
    public double Temperature { get; init; } = 0.7;
    public int MaxTokens { get; init; } = 2048;
    public double TopP { get; init; } = 0.9;
    public double FrequencyPenalty { get; init; } = 0.0;
    public double PresencePenalty { get; init; } = 0.0;
    public string? SessionId { get; init; }
}

/// <summary>Embedding generation request</summary>
public record EmbeddingRequest
{
    /// <summary>One or more texts to embed</summary>
    public required List<string> Texts { get; init; }
    public string? ModelName { get; init; }
    public Dictionary<string, object>? AdditionalParameters { get; init; }
}

/// <summary>Vision analysis request</summary>
public record VisionRequest
{
    public required byte[] ImageData { get; init; }
    public string? Prompt { get; init; }
    public string? ModelName { get; init; }
}

// ──── Response Models ────

/// <summary>Text completion result with token usage metrics</summary>
public record CompletionResult
{
    public required string Text { get; init; }
    public required string ModelUsed { get; init; }
    public int PromptTokens { get; init; }
    public int CompletionTokens { get; init; }
    public int TotalTokens { get; init; }
    public TimeSpan Duration { get; init; }
    public string? FinishReason { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>Embedding generation result</summary>
public record EmbeddingResult
{
    /// <summary>Embedding vectors — one per input text</summary>
    public required List<float[]> Embeddings { get; init; }
    public required string ModelUsed { get; init; }
    public int TotalTokens { get; init; }
    public TimeSpan Duration { get; init; }
}

/// <summary>Vision analysis result</summary>
public record VisionResult
{
    public required string Description { get; init; }
    public List<string> Tags { get; init; } = new();
    public Dictionary<string, float> Scores { get; init; } = new();
    public required string ModelUsed { get; init; }
    public TimeSpan Duration { get; init; }
}

/// <summary>Provider health check result</summary>
public record HealthCheckResult
{
    public required bool IsHealthy { get; init; }
    public string? Message { get; init; }
    public Dictionary<string, string>? Details { get; init; }
    public DateTime CheckedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>Available model information</summary>
public record ModelInfo
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public long? ParameterCount { get; init; }
    public string? Family { get; init; }
    public List<string> Capabilities { get; init; } = new();
}
