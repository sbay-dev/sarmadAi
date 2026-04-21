# DMPS — Dynamic Model Provider System Architecture

## نظام مزوّد النماذج الديناميكي — البنية المعمارية

> **Authors:** sbay-dev  
> **Version:** 1.0  
> **License:** Apache-2.0  
> **Repository:** [sarmadAi-prod](../README.md)

---

## 1. Overview — نظرة عامة

The Dynamic Model Provider System (DMPS) is a .NET library that abstracts AI model providers behind a unified interface (`IModelProvider`). DMPS enables applications to seamlessly switch between local, cloud, edge, and custom AI model providers without modifying application logic. The system follows Model-View-Controller (MVC) architectural principles, organizing agent capabilities into controller endpoints that expose a consistent API surface regardless of the underlying model provider.

DMPS addresses a fundamental challenge in modern AI application development: the proliferation of model providers with incompatible APIs. By defining a single interface contract, DMPS allows applications to be written once and deployed against any supported backend — from a locally hosted Ollama instance during development to Azure OpenAI in production, with transparent fallback to edge-deployed ONNX models for offline scenarios.

---

## 2. Architecture — البنية المعمارية

### 2.1 System Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                       Application Layer                     │
│            (ASP.NET Core / Console / Desktop)                │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│                    IModelProvider                            │
│              (Unified Provider Interface)                    │
│                                                             │
│   ┌──────────┬──────────┬──────────┬──────────────────┐     │
│   │  Health   │  Model   │  Text    │    Embedding     │     │
│   │  Check    │  List    │  Gen     │    & Vision      │     │
│   └──────────┴──────────┴──────────┴──────────────────┘     │
│                                                             │
├──────────┬──────────┬──────────┬────────────────────────────┤
│  Ollama  │  Azure   │ Foundry  │     Custom Provider        │
│  Local   │  OpenAI  │  1700+   │     (User-defined)         │
│          │  Cloud   │  Models  │                            │
├──────────┴──────────┴──────────┴────────────────────────────┤
│                    Provider Registry                        │
│          (Priority-based Selection & Fallback)              │
├─────────────────────────────────────────────────────────────┤
│                    Health Monitor                           │
│          (Continuous Provider Health Checking)               │
└─────────────────────────────────────────────────────────────┘
```

### 2.2 Provider Types — أنواع المزوّدين

| Type | النوع | Description | Examples |
|------|--------|-------------|----------|
| **Local** | محلي | Models hosted on the same machine or local network | Ollama, LM Studio |
| **Cloud** | سحابي | Models accessed via cloud APIs | Azure OpenAI, OpenAI API |
| **Edge** | طرفي | Models deployed for on-device inference | ONNX Runtime, TensorFlow Lite |
| **Custom** | مخصص | User-defined providers implementing IModelProvider | Domain-specific models |

### 2.3 Interface Contract

The `IModelProvider` interface defines the contract that all providers must implement:

```
IModelProvider
├── ProviderName: string          — Provider identifier
├── ProviderType: ModelProviderType — Deployment classification
├── CheckHealthAsync()            — Connectivity and health verification
├── ListAvailableModelsAsync()    — Enumerate available models
├── GenerateCompletionAsync()     — Synchronous text generation
├── StreamCompletionAsync()       — Token-by-token streaming
├── GenerateEmbeddingsAsync()     — Vector embedding generation
└── AnalyzeImageAsync()           — Vision model analysis
```

See [`IModelProvider.cs`](../src/Sarmad.DMPS/IModelProvider.cs) for the full interface definition.

---

## 3. Capabilities — القدرات

### 3.1 Supported Operations

DMPS provides a comprehensive set of AI operations through its unified interface:

| Operation | الوظيفة | Description |
|-----------|---------|-------------|
| **Health Check** | فحص الصحة | Verify provider connectivity, model availability, and service status |
| **Model Listing** | قائمة النماذج | Enumerate all models available through a provider with metadata |
| **Text Completion** | إكمال النص | Generate text completions with configurable temperature, top-p, and penalties |
| **Streaming** | البث المباشر | Token-by-token streaming via `IAsyncEnumerable<string>` |
| **Text Embedding** | تضمين النص | Generate dense vector representations for semantic similarity |
| **Image Embedding** | تضمين الصور | Generate embeddings from image data |
| **Multimodal Embedding** | تضمين متعدد الوسائط | Combined text-image embedding generation |
| **Matryoshka Reduction** | تقليل ماتريوشكا | Variable-dimensionality embeddings (768, 512, 256, 128, 64) |
| **Vision Analysis** | تحليل الرؤية | Image description, tagging, and classification |

### 3.2 Request/Response Models

All operations use strongly-typed request and response models defined in [`Models.cs`](../src/Sarmad.DMPS/Models.cs):

- `CompletionRequest` / `CompletionResult` — Text generation with token usage metrics
- `EmbeddingRequest` / `EmbeddingResult` — Batch embedding generation
- `VisionRequest` / `VisionResult` — Image analysis with tags and scores
- `HealthCheckResult` — Provider health status with diagnostic details
- `ModelInfo` — Model metadata including parameter count and capabilities

---

## 4. Agent Orchestration — تنسيق الوكلاء

DMPS organizes AI agent capabilities into 12 categories, exposed through 28 MVC controller endpoints:

### 4.1 Capability Categories

| # | Category | الفئة | Endpoints | Description |
|---|----------|--------|-----------|-------------|
| 1 | Text Generation | توليد النص | 3 | Completion, streaming, chat |
| 2 | Embedding | التضمين | 3 | Text, image, and multimodal embeddings |
| 3 | Vision | الرؤية | 2 | Image analysis and description |
| 4 | Search | البحث | 3 | Semantic search, retrieval, ranking |
| 5 | Classification | التصنيف | 2 | Text and concept classification |
| 6 | Summarization | التلخيص | 2 | Document and conversation summarization |
| 7 | Translation | الترجمة | 2 | Multi-language translation |
| 8 | Code Generation | توليد الكود | 2 | Code completion and explanation |
| 9 | Reasoning | الاستدلال | 2 | Chain-of-thought and logical reasoning |
| 10 | Memory | الذاكرة | 3 | Session context and conversation history |
| 11 | Health & Monitoring | الصحة والمراقبة | 2 | Provider health and performance metrics |
| 12 | Configuration | الإعدادات | 2 | Runtime provider and model configuration |

### 4.2 Multi-Agent Workflow

DMPS supports orchestration of multiple agents, each backed by different providers:

```
┌─────────────────────────────────────────┐
│            Orchestrator                  │
│     (Routes tasks to best agent)        │
├──────────┬──────────┬───────────────────┤
│ Agent A  │ Agent B  │    Agent C        │
│ (Ollama) │ (Azure)  │    (ONNX)         │
│ Creative │ Analytic │    Embedding      │
│ Tasks    │ Tasks    │    Tasks          │
└──────────┴──────────┴───────────────────┘
```

The orchestrator selects the optimal provider for each task based on:
- Provider health status
- Model capability matching
- Latency and cost requirements
- Priority-based fallback configuration

---

## 5. Configuration — الإعدادات

### 5.1 Provider Configuration

DMPS uses JSON-based configuration for provider registration and priority management:

```json
{
  "DMPS": {
    "Providers": [
      {
        "Name": "ollama-local",
        "Type": "Local",
        "Endpoint": "http://localhost:11434",
        "Priority": 1,
        "Models": ["llama3", "mistral", "codellama"],
        "Enabled": true
      },
      {
        "Name": "azure-openai",
        "Type": "Cloud",
        "Endpoint": "https://{instance}.openai.azure.com",
        "Priority": 2,
        "ApiKeyEnvVar": "AZURE_OPENAI_API_KEY",
        "Models": ["gpt-4", "gpt-4o", "text-embedding-ada-002"],
        "Enabled": true
      },
      {
        "Name": "onnx-edge",
        "Type": "Edge",
        "ModelPath": "./models/cns-embedding-v2.onnx",
        "Priority": 3,
        "Models": ["cns-embedding-v2"],
        "Enabled": true
      }
    ],
    "Fallback": {
      "Enabled": true,
      "Strategy": "PriorityDescending",
      "MaxRetries": 3,
      "RetryDelayMs": 1000
    },
    "HealthCheck": {
      "IntervalSeconds": 30,
      "TimeoutSeconds": 5
    }
  }
}
```

### 5.2 Priority-Based Fallback

When a provider is unavailable, DMPS automatically falls back to the next healthy provider in priority order:

```
Request → Provider 1 (Priority 1)
              │
              ├── Healthy? → Execute request
              │
              └── Unhealthy → Provider 2 (Priority 2)
                                  │
                                  ├── Healthy? → Execute request
                                  │
                                  └── Unhealthy → Provider 3 (Priority 3)
                                                      │
                                                      └── ... → Error (all providers unavailable)
```

---

## 6. Integration with CNS — التكامل مع CNS

DMPS integrates with the CNS (Cubic Neural Statistics) system through the embedding pipeline:

1. Arabic concepts are encoded as 12-dimensional CNS coordinates (see [CNS Model Spec](./cns-model-spec.md))
2. CNSEmbeddingModelV2 (via ONNX Runtime edge provider) generates 768-d embeddings
3. Embeddings are served through the DMPS embedding endpoint
4. Applications consume embeddings through the unified `IModelProvider.GenerateEmbeddingsAsync()` interface

This architecture allows the CNS embedding model to be deployed alongside general-purpose models, with DMPS handling routing, fallback, and health monitoring transparently.

---

## 7. References — المراجع

### Source Code

- [`IModelProvider.cs`](../src/Sarmad.DMPS/IModelProvider.cs) — Core provider interface
- [`Models.cs`](../src/Sarmad.DMPS/Models.cs) — Request and response models

### Related Documents

- [CNS Model Specification](./cns-model-spec.md) — Embedding model architecture and training
- [ONNX Parity Proof](./onnx-parity-proof.md) — PyTorch ↔ ONNX equivalence verification
- [Project README](../README.md) — Repository overview

---

*Copyright © 2026 sbay-dev. Licensed under Apache-2.0.*
