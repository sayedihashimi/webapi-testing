# Aggregated Analysis: ASP.NET Core Web API Skill Evaluation

**Runs:** 3 | **Configurations:** 5 | **Scenarios:** 3 | **Dimensions:** 24
**Date:** 2026-03-28 15:33 UTC

---

## Executive Summary

| Dimension [Tier] | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| Build & Run Success [CRITICAL] | 4.0 | 3.3 ± 1.2 | 3.7 ± 0.6 | 4.0 | — |
| Security Vulnerability Scan [CRITICAL] | 2.0 | 4.0 ± 1.0 | 3.3 ± 0.6 | 2.3 ± 0.6 | — |
| Minimal API Architecture [CRITICAL] | 1.0 | 5.0 | 4.7 ± 0.6 | 1.7 ± 0.6 | — |
| Input Validation & Guard Clauses [CRITICAL] | 3.0 | 4.0 | 3.7 ± 0.6 | 3.7 ± 0.6 | — |
| NuGet & Package Discipline [CRITICAL] | 1.3 ± 0.6 | 3.7 ± 1.5 | 3.0 ± 1.0 | 2.0 | — |
| EF Migration Usage [CRITICAL] | 1.0 | 3.3 ± 2.1 | 1.7 ± 1.2 | 1.0 | — |
| Business Logic Correctness [HIGH] | 4.3 ± 0.6 | 3.7 ± 1.5 | 4.3 ± 0.6 | 4.3 ± 0.6 | — |
| Prefer Built-in over 3rd Party [HIGH] | 2.0 | 4.3 ± 0.6 | 3.7 ± 0.6 | 2.7 ± 0.6 | — |
| Modern C# Adoption [HIGH] | 2.3 ± 0.6 | 4.7 ± 0.6 | 5.0 | 3.3 ± 0.6 | — |
| Error Handling & Middleware [HIGH] | 2.7 ± 0.6 | 5.0 | 4.7 ± 0.6 | 3.7 ± 1.2 | — |
| Async Patterns & Cancellation [HIGH] | 2.0 | 5.0 | 4.7 ± 0.6 | 3.3 ± 0.6 | — |
| EF Core Best Practices [HIGH] | 2.7 ± 0.6 | 5.0 | 4.7 ± 0.6 | 3.3 ± 0.6 | — |
| Service Abstraction & DI [HIGH] | 4.0 | 5.0 | 5.0 | 4.0 | — |
| Security Configuration [HIGH] | 1.3 ± 0.6 | 1.0 | 1.0 | 1.0 | — |
| DTO Design [MEDIUM] | 2.3 ± 0.6 | 5.0 | 5.0 | 2.7 ± 0.6 | — |
| Sealed Types [MEDIUM] | 1.0 | 5.0 | 5.0 | 2.3 ± 1.2 | — |
| Data Seeder Design [MEDIUM] | 4.0 | 4.0 | 4.0 | 4.0 | — |
| Structured Logging [MEDIUM] | 3.0 | 3.7 ± 0.6 | 3.7 ± 0.6 | 3.3 ± 0.6 | — |
| Nullable Reference Types [MEDIUM] | 4.0 | 4.0 | 4.0 | 4.0 | — |
| API Documentation [MEDIUM] | 2.7 ± 0.6 | 5.0 | 4.3 ± 0.6 | 3.0 | — |
| File Organization [MEDIUM] | 4.0 | 5.0 | 4.3 ± 0.6 | 3.3 ± 0.6 | — |
| HTTP Test File Quality [MEDIUM] | 4.0 | 4.3 ± 0.6 | 3.3 ± 0.6 | 3.3 ± 0.6 | — |
| Type Design & Resource Management [MEDIUM] | 3.3 ± 0.6 | 5.0 | 4.7 ± 0.6 | 3.0 | — |
| Code Standards Compliance [LOW] | 4.0 ± 1.0 | 5.0 | 4.7 ± 0.6 | 4.0 ± 1.0 | — |

---

## Final Rankings

| Rank | Configuration | Mean Weighted Score | Std Dev | Min | Max |
|---|---|---|---|---|---|
| 🥇 | dotnet-webapi | 180.8 | 7.6 | 175.5 | 189.5 |
| 🥈 | dotnet-artisan | 166.7 | 14.8 | 151.0 | 180.5 |
| 🥉 | managedcode-dotnet-skills | 126.3 | 6.8 | 119.5 | 133.0 |
| 4th | no-skills | 110.0 | 4.3 | 106.0 | 114.5 |
| 5th | dotnet-skills | 0.0 | 0.0 | 0.0 | 0.0 |

---

## Weighted Score per Run

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 106.0 | 175.5 | 180.5 | 133.0 | 0.0 |
| 2 | 114.5 | 177.5 | 168.5 | 126.5 | 0.0 |
| 3 | 109.5 | 189.5 | 151.0 | 119.5 | 0.0 |
| **Mean** | **110.0** | **180.8** | **166.7** | **126.3** | **0.0** |

---

## Verification Summary (All Runs)

| Configuration | Build Pass Rate | Run Pass Rate | Avg Warnings |
|---|---|---|---|
| no-skills | 15/15 (100%) | 15/15 (100%) | 122.8 |
| dotnet-webapi | 11/15 (73%) | 11/15 (73%) | 116.9 |
| dotnet-artisan | 3/6 (50%) | 3/6 (50%) | 94.7 |

---

## Consistency Analysis

| Configuration | Score σ | Most Consistent Dim (σ) | Most Variable Dim (σ) |
|---|---|---|---|
| no-skills | 4.3 | Build & Run Success (0.0) | Code Standards Compliance (1.0) |
| dotnet-webapi | 7.6 | Minimal API Architecture (0.0) | EF Migration Usage (2.1) |
| dotnet-artisan | 14.8 | Modern C# Adoption (0.0) | EF Migration Usage (1.2) |
| managedcode-dotnet-skills | 6.8 | Build & Run Success (0.0) | Error Handling & Middleware (1.2) |
| dotnet-skills | 0.0 |  (inf) |  (0.0) |

---

## Per-Dimension Breakdown

### 1. Build & Run Success [CRITICAL × 3]

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 2 | 4 | 4 | — |
| 2 | 4 | 4 | 3 | 4 | — |
| 3 | 4 | 4 | 4 | 4 | — |
| **Mean** | **4.0** | **3.3** | **3.7** | **4.0** | — |

### 2. Security Vulnerability Scan [CRITICAL × 3]

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 2 | 5 | 4 | 3 | — |
| 2 | 2 | 3 | 3 | 2 | — |
| 3 | 2 | 4 | 3 | 2 | — |
| **Mean** | **2.0** | **4.0** | **3.3** | **2.3** | — |

### 3. Minimal API Architecture [CRITICAL × 3]

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 1 | 5 | 5 | 1 | — |
| 2 | 1 | 5 | 5 | 2 | — |
| 3 | 1 | 5 | 4 | 2 | — |
| **Mean** | **1.0** | **5.0** | **4.7** | **1.7** | — |

### 4. Input Validation & Guard Clauses [CRITICAL × 3]

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 4 | 4 | 4 | — |
| 2 | 3 | 4 | 4 | 4 | — |
| 3 | 3 | 4 | 3 | 3 | — |
| **Mean** | **3.0** | **4.0** | **3.7** | **3.7** | — |

### 5. NuGet & Package Discipline [CRITICAL × 3]

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 1 | 5 | 4 | 2 | — |
| 2 | 2 | 2 | 3 | 2 | — |
| 3 | 1 | 4 | 2 | 2 | — |
| **Mean** | **1.3** | **3.7** | **3.0** | **2.0** | — |

### 6. EF Migration Usage [CRITICAL × 3]

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 1 | 1 | 3 | 1 | — |
| 2 | 1 | 4 | 1 | 1 | — |
| 3 | 1 | 5 | 1 | 1 | — |
| **Mean** | **1.0** | **3.3** | **1.7** | **1.0** | — |

### 7. Business Logic Correctness [HIGH × 2]

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 2 | 4 | 4 | — |
| 2 | 5 | 5 | 5 | 5 | — |
| 3 | 4 | 4 | 4 | 4 | — |
| **Mean** | **4.3** | **3.7** | **4.3** | **4.3** | — |

### 8. Prefer Built-in over 3rd Party [HIGH × 2]

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 2 | 5 | 4 | 2 | — |
| 2 | 2 | 4 | 4 | 3 | — |
| 3 | 2 | 4 | 3 | 3 | — |
| **Mean** | **2.0** | **4.3** | **3.7** | **2.7** | — |

### 9. Modern C# Adoption [HIGH × 2]

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 5 | 5 | 4 | — |
| 2 | 2 | 4 | 5 | 3 | — |
| 3 | 2 | 5 | 5 | 3 | — |
| **Mean** | **2.3** | **4.7** | **5.0** | **3.3** | — |

### 10. Error Handling & Middleware [HIGH × 2]

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 2 | 5 | 5 | 5 | — |
| 2 | 3 | 5 | 5 | 3 | — |
| 3 | 3 | 5 | 4 | 3 | — |
| **Mean** | **2.7** | **5.0** | **4.7** | **3.7** | — |

### 11. Async Patterns & Cancellation [HIGH × 2]

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 2 | 5 | 5 | 4 | — |
| 2 | 2 | 5 | 5 | 3 | — |
| 3 | 2 | 5 | 4 | 3 | — |
| **Mean** | **2.0** | **5.0** | **4.7** | **3.3** | — |

### 12. EF Core Best Practices [HIGH × 2]

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 2 | 5 | 5 | 4 | — |
| 2 | 3 | 5 | 5 | 3 | — |
| 3 | 3 | 5 | 4 | 3 | — |
| **Mean** | **2.7** | **5.0** | **4.7** | **3.3** | — |

### 13. Service Abstraction & DI [HIGH × 2]

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 5 | 5 | 4 | — |
| 2 | 4 | 5 | 5 | 4 | — |
| 3 | 4 | 5 | 5 | 4 | — |
| **Mean** | **4.0** | **5.0** | **5.0** | **4.0** | — |

### 14. Security Configuration [HIGH × 2]

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 1 | 1 | 1 | 1 | — |
| 2 | 1 | 1 | 1 | 1 | — |
| 3 | 2 | 1 | 1 | 1 | — |
| **Mean** | **1.3** | **1.0** | **1.0** | **1.0** | — |

### 15. DTO Design [MEDIUM × 1]

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 5 | 5 | 3 | — |
| 2 | 2 | 5 | 5 | 3 | — |
| 3 | 2 | 5 | 5 | 2 | — |
| **Mean** | **2.3** | **5.0** | **5.0** | **2.7** | — |

### 16. Sealed Types [MEDIUM × 1]

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 1 | 5 | 5 | 1 | — |
| 2 | 1 | 5 | 5 | 3 | — |
| 3 | 1 | 5 | 5 | 3 | — |
| **Mean** | **1.0** | **5.0** | **5.0** | **2.3** | — |

### 17. Data Seeder Design [MEDIUM × 1]

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 4 | 4 | 4 | — |
| 2 | 4 | 4 | 4 | 4 | — |
| 3 | 4 | 4 | 4 | 4 | — |
| **Mean** | **4.0** | **4.0** | **4.0** | **4.0** | — |

### 18. Structured Logging [MEDIUM × 1]

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 4 | 4 | 4 | — |
| 2 | 3 | 3 | 3 | 3 | — |
| 3 | 3 | 4 | 4 | 3 | — |
| **Mean** | **3.0** | **3.7** | **3.7** | **3.3** | — |

### 19. Nullable Reference Types [MEDIUM × 1]

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 4 | 4 | 4 | — |
| 2 | 4 | 4 | 4 | 4 | — |
| 3 | 4 | 4 | 4 | 4 | — |
| **Mean** | **4.0** | **4.0** | **4.0** | **4.0** | — |

### 20. API Documentation [MEDIUM × 1]

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 2 | 5 | 4 | 3 | — |
| 2 | 3 | 5 | 5 | 3 | — |
| 3 | 3 | 5 | 4 | 3 | — |
| **Mean** | **2.7** | **5.0** | **4.3** | **3.0** | — |

### 21. File Organization [MEDIUM × 1]

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 5 | 4 | 4 | — |
| 2 | 4 | 5 | 5 | 3 | — |
| 3 | 4 | 5 | 4 | 3 | — |
| **Mean** | **4.0** | **5.0** | **4.3** | **3.3** | — |

### 22. HTTP Test File Quality [MEDIUM × 1]

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 4 | 3 | 4 | — |
| 2 | 4 | 5 | 3 | 3 | — |
| 3 | 4 | 4 | 4 | 3 | — |
| **Mean** | **4.0** | **4.3** | **3.3** | **3.3** | — |

### 23. Type Design & Resource Management [MEDIUM × 1]

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 5 | 5 | 3 | — |
| 2 | 4 | 5 | 5 | 3 | — |
| 3 | 3 | 5 | 4 | 3 | — |
| **Mean** | **3.3** | **5.0** | **4.7** | **3.0** | — |

### 24. Code Standards Compliance [LOW × 0]

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 5 | 5 | 4 | — |
| 2 | 5 | 5 | 5 | 5 | — |
| 3 | 3 | 5 | 4 | 3 | — |
| **Mean** | **4.0** | **5.0** | **4.7** | **4.0** | — |

---

## Raw Data References

- Per-run analysis: `reports/analysis-run-1.md`
- Per-run analysis: `reports/analysis-run-2.md`
- Per-run analysis: `reports/analysis-run-3.md`
- Verification data: `reports/verification-data.json`
- Score data: `reports/scores-data.json`
- Build notes: `reports/build-notes.md`
