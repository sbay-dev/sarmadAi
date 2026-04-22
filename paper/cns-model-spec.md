# CNS Embedding Model v2 — Technical Specification

## المواصفات التقنية لنموذج التضمين CNS الإصدار الثاني

**Cubic Neural Statistics: Pre-MTEB Arabic Semantic Embedding**

> **Authors:** sbay-dev  
> **Version:** 2.0  
> **License:** Apache-2.0  
> **Repository:** [sarmadAi-prod](../README.md)

---

## 1. Abstract — ملخص

CNS Embedding Model v2 (CNSEmbeddingModelV2) is a 21,765,123-parameter neural network that encodes Arabic semantic concepts into 768-dimensional L2-normalized embedding vectors. The model accepts 12-dimensional CNS coordinates and a scalar polarity value as input, producing dense vector representations suitable for semantic similarity search, concept clustering, and downstream NLP tasks. Designed and trained prior to the establishment of the Massive Text Embedding Benchmark (MTEB), the model was developed from first principles for Arabic concept representation. The training corpus comprises 148 original Arabic concepts spanning seven semantic categories, augmented to 11,148 training samples through geometric transformations. The architecture features a dual-encoder fusion design with four Pre-Norm Transformer blocks and three specialized output heads for embedding generation, polarity prediction, and node type classification.

---

## 2. Architecture — البنية المعمارية

### 2.1 Overview

The model follows a dual-encoder → fusion → transformer → multi-head architecture:

```
Input
  ├── Coordinates [B, 12]  ──→  Coordinate Encoder  ──→  [B, 768]
  │                                                          │
  └── Polarity [B, 1]     ──→  Polarity Encoder    ──→  [B, 256]
                                                             │
                                              ┌──────────────┘
                                              ▼
                                   Fusion: cat → [B, 1024]
                                              │
                                   Linear(1024, 768) → LayerNorm → GELU
                                              │
                                              ▼
                               4× Pre-Norm Transformer Block
                                    (8 heads, head_dim=96)
                                              │
                               ┌──────────────┼──────────────┐
                               ▼              ▼              ▼
                         Embedding Head   Polarity Head   NodeType Head
                          [B, 768]         [B, 1]          [B, 2]
```

### 2.2 Coordinate Encoder — مُشفّر الإحداثيات

Transforms the 12-dimensional CNS coordinate vector into a 768-dimensional feature representation:

| Layer | Operation | Input Dim | Output Dim | Activation |
|-------|-----------|-----------|------------|------------|
| 1 | Linear + LayerNorm | 12 | 256 | GELU |
| 2 | Linear + LayerNorm | 256 | 512 | GELU |
| 3 | Linear + LayerNorm | 512 | 768 | GELU |

### 2.3 Polarity Encoder — مُشفّر القطبية

Transforms the scalar polarity value into a 256-dimensional feature representation:

| Layer | Operation | Input Dim | Output Dim | Activation |
|-------|-----------|-----------|------------|------------|
| 1 | Linear + LayerNorm | 1 | 64 | GELU |
| 2 | Linear + LayerNorm | 64 | 128 | GELU |
| 3 | Linear + LayerNorm | 128 | 256 | GELU |

### 2.4 Fusion Layer — طبقة الدمج

Concatenates the outputs of both encoders and projects to the transformer dimension:

```
fusion_input = cat(coord_features[B, 768], pol_features[B, 256])  →  [B, 1024]
fusion_output = GELU(LayerNorm(Linear(1024, 768)(fusion_input)))  →  [B, 768]
```

### 2.5 Transformer Blocks — كتل المحول

Four identical Pre-Norm Transformer blocks with residual connections:

```
For each block i ∈ {1, 2, 3, 4}:
    x' = x + MultiHeadAttention(LayerNorm(x))
    x'' = x' + FFN(LayerNorm(x'))

Where:
    MultiHeadAttention: 8 heads, head_dim = 96, total dim = 768
    FFN: Linear(768, 1536) → GELU → Linear(1536, 768)
```

### 2.6 Output Heads — رؤوس الإخراج

**Embedding Head (التضمين):**
```
Linear(768, 768) → Tanh → L2-Normalize
Output: [B, 768] unit-length embedding vectors
```

**Polarity Head (القطبية):**
```
Linear(768, 256) → GELU → Linear(256, 64) → GELU → Linear(64, 1) → Tanh
Output: [B, 1] polarity prediction in [-1, +1]
```

**Node Type Head (تصنيف العقدة):**
```
Linear(768, 128) → GELU → Linear(128, 2)
Output: [B, 2] logits for binary node type classification
```

### 2.7 Parameter Count — عدد المعاملات

| Component | Parameters |
|-----------|-----------|
| Coordinate Encoder | 466,688 |
| Polarity Encoder | 41,344 |
| Fusion Layer | 787,200 |
| Transformer Blocks (×4) | 18,881,536 |
| Embedding Head | 591,360 |
| Polarity Head | 213,057 |
| Node Type Head | 98,562 |
| Layer Norms & Biases | 685,376 |
| **Total** | **21,765,123** |

---

## 3. The 12-Dimensional CNS Coordinate System — نظام الإحداثيات ذو 12 بُعدًا

Each concept in CNS space is located by 12 semantic coordinates. These axes were designed to capture the multifaceted nature of Arabic concepts across cultural, emotional, and social dimensions.

| Dim | Axis Name (English) | الاسم (عربي) | Range | Semantic Meaning |
|-----|---------------------|--------------|-------|------------------|
| 1 | Biological Essence | الجوهر البيولوجي | [0, 1] | Degree of biological or organic nature of the concept |
| 2 | Intellectual Agency | الفاعلية الفكرية | [0, 1] | Capacity for rational thought, reasoning, and decision-making |
| 3 | Emotional Depth | العمق العاطفي | [0, 1] | Intensity and complexity of emotional content |
| 4 | Social Status | المكانة الاجتماعية | [0, 1] | Position within social hierarchies and community structures |
| 5 | Cultural Role | الدور الثقافي | [0, 1] | Significance within cultural traditions and practices |
| 6 | Political Power | السلطة السياسية | [0, 1] | Influence in governance, authority, and political systems |
| 7 | Financial Control | التحكم الاقتصادي | [-1, +1] | Financial influence; negative values denote financial vulnerability |
| 8 | Spiritual Authority | السلطة الروحية | [0, 1] | Connection to spiritual, religious, or metaphysical domains |
| 9 | Physical Strength | القوة الجسدية | [0, 1] | Physical capability, endurance, or material presence |
| 10 | Mental Resilience | المرونة الذهنية | [0, 1] | Psychological fortitude, adaptability, and perseverance |
| 11 | Creative Expression | التعبير الإبداعي | [0, 1] | Capacity for artistic, literary, or innovative output |
| 12 | Temporal Presence | الحضور الزمني | [0, 1] | Persistence across time; relevance in past, present, and future |

> **Note:** Dimension 7 (Financial Control) is the only bipolar axis, using the range [-1, +1] to distinguish between financial empowerment (positive) and financial vulnerability (negative).

---

## 4. Training — التدريب

### 4.1 Dataset Construction — بناء مجموعة البيانات

The training dataset was constructed from 148 original Arabic concepts carefully selected from seven semantic categories:

| Category | الفئة | Example Concepts | Count |
|----------|--------|------------------|-------|
| Emotions | المشاعر | حُبّ (love), حُزن (sadness), فَرَح (joy) | ~21 |
| Nature | الطبيعة | بَحر (sea), جَبَل (mountain), نَهر (river) | ~21 |
| Intellectual | الفكر | عِلم (knowledge), حِكمة (wisdom), فَهم (understanding) | ~21 |
| Spiritual | الروحانية | إيمان (faith), رَحمة (mercy), تَقوى (piety) | ~21 |
| Art | الفنون | شِعر (poetry), موسيقى (music), رَقص (dance) | ~21 |
| Time | الزمن | ماضي (past), حاضر (present), مُستقبل (future) | ~21 |
| Senses | الحواس | بَصَر (sight), سَمع (hearing), لَمس (touch) | ~22 |

### 4.2 Geometric Augmentation — التعزيز الهندسي

Each of the 148 concepts was augmented ×75 through geometric transformations in the 12-dimensional coordinate space, yielding **11,148 training samples**. Augmentation techniques included:

- Coordinate jittering (Gaussian noise addition)
- Polarity perturbation
- Axis-aligned reflections
- Interpolation between semantically related concepts

### 4.3 Training Configuration

| Parameter | Value |
|-----------|-------|
| Epochs | 1,500 |
| Optimizer | AdamW |
| Learning Rate Scheduler | CosineAnnealing |
| Training Duration | 53 minutes |
| Hardware | Single GPU |

### 4.4 Loss Functions — دوال الخسارة

The model is trained with a composite loss comprising five components:

1. **Triplet Loss** — Enforces that anchor-positive distance is smaller than anchor-negative distance by a margin.
2. **Angular Loss** — Penalizes angular deviation between embeddings of similar concepts.
3. **Contrastive Loss** — Pulls similar concepts together and pushes dissimilar concepts apart in embedding space.
4. **Polarity MSE Loss** — Mean squared error between predicted and ground-truth polarity values.
5. **Node Type Cross-Entropy Loss** — Standard cross-entropy for binary node type classification.

---

## 5. Metrics — مقاييس الأداء

### 5.1 Summary Results

| Metric | القياس | Value | Notes |
|--------|--------|-------|-------|
| Best Composite Loss | أفضل خسارة مركبة | 0.1197 | Weighted sum of 5 loss functions |
| Embedding L2 Norms | معايير L2 للتضمين | 1.0000 | Perfect unit-length normalization |
| Self-Similarity | التشابه الذاتي | 1.0000 | cos(v, v) = 1.0 for all embeddings |
| Cross-Similarity μ | متوسط التشابه المتقاطع | 0.8864 | Mean pairwise cosine similarity |
| Cross-Similarity σ | انحراف التشابه المتقاطع | 0.2974 | Indicates healthy semantic diversity |
| Polarity MAE | متوسط الخطأ المطلق للقطبية | 0.0070 | Less than 1% of the [-1, +1] range |
| Node Type Accuracy | دقة تصنيف نوع العقدة | 96.50% | Binary classification accuracy |
| Unique Embeddings | التضمينات الفريدة | 91.73% | Despite ×75 augmentation factor |

### 5.2 Analysis

- **L2 Norm = 1.0000** confirms that all output embeddings lie on the unit hypersphere in ℝ⁷⁶⁸, enabling cosine similarity to be computed as a simple dot product.
- **Self-Similarity = 1.0000** verifies deterministic inference — the same input always produces the same output.
- **Cross-Similarity μ = 0.8864** with **σ = 0.2974** indicates that the model captures both broad semantic relatedness (high mean) and fine-grained distinctions (non-trivial variance).
- **91.73% Unique Embeddings** despite ×75 augmentation demonstrates that the model learned to produce distinct representations even for geometrically perturbed inputs.

---

## 6. Matryoshka Embeddings — تضمينات ماتريوشكا

The CNS Embedding Model v2 supports Matryoshka representation learning, allowing the full 768-dimensional embedding to be truncated to smaller dimensions while retaining semantic utility:

| Dimension | Size (float32) | Use Case |
|-----------|---------------|----------|
| 768 | 3,072 bytes | Full precision — semantic search, clustering |
| 512 | 2,048 bytes | High-quality retrieval with reduced storage |
| 256 | 1,024 bytes | Balanced precision and efficiency |
| 128 | 512 bytes | Fast approximate nearest-neighbor search |
| 64 | 256 bytes | Ultra-compact representations for edge deployment |

The first *d* dimensions of the full 768-d vector can be used directly as a *d*-dimensional embedding. This is achieved through the training objective, which ensures that semantic information is concentrated in the leading dimensions.

```
embedding_768 = model.embed("رحمة")        # Full precision
embedding_256 = embedding_768[:256]         # Truncated — still meaningful
embedding_64  = embedding_768[:64]          # Ultra-compact
```

---

## 7. Model Artifacts — ملفات النموذج

| Artifact | Format | Size |
|----------|--------|------|
| CNSEmbeddingModelV2 | PyTorch (.pt) | 83 MB |
| CNSEmbeddingModelV2 | ONNX (.onnx) | 82.1 MB |

For ONNX parity verification, see [ONNX Parity Proof](./onnx-parity-proof.md).

---

## 8. References — المراجع

### Source Code Interfaces

- [`CNSNode.cs`](../src/Sarmad.CNS.Core/CNSNode.cs) — Concept node data structure
- [`CNSSpace.cs`](../src/Sarmad.CNS.Core/CNSSpace.cs) — N-dimensional semantic space
- [`CNSHash.cs`](../src/Sarmad.CNS.Core/CNSHash.cs) — Deterministic concept hashing
- [`DriftDetector.cs`](../src/Sarmad.CNS.Core/DriftDetector.cs) — Cross-space drift detection
- [`ICNSEmbeddingModel.cs`](../src/Sarmad.CNS.Embedding/ICNSEmbeddingModel.cs) — Embedding model interface

### Related Documents

- [ONNX Parity Proof](./onnx-parity-proof.md) — PyTorch ↔ ONNX numerical equivalence
- [DMPS Architecture](./dmps-architecture.md) — Dynamic Model Provider System
- [Project README](../README.md) — Repository overview

---

*Copyright © 2026 sbay-dev. Licensed under Apache-2.0.*
