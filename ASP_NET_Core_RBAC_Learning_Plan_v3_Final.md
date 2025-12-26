# ASP.NET Core RBAC 會員系統學習計畫 (Final)

> **目標**：透過實作 RBAC 會員系統，快速掌握 ASP.NET Core 核心技術
> **前置知識**：有 Spring Boot + Java 17 開發經驗
> **版本**：v3 Final - 根據資深架構師建議調整

---

## 📋 目錄

1. [專案概述](#專案概述)
2. [系統功能設計](#系統功能設計)
3. [資料庫設計](#資料庫設計)
4. [技術棧選擇](#技術棧選擇)
5. [設定檔管理](#設定檔管理)
6. [學習階段規劃](#學習階段規劃)
7. [Spring Boot 對照表](#spring-boot-對照表)
8. [關鍵技術說明](#關鍵技術說明)
9. [C# 與 .NET 新特性](#c-與-net-新特性)
10. [待討論與學習重點](#待討論與學習重點)

---

## 專案概述

### 系統名稱
**RBAC Member System** - 基於角色的存取控制會員系統

### 核心功能
- ✅ 使用者註冊（Username + Email + Password）
- ✅ 使用者登入（JWT Token 驗證）
- ✅ 角色管理（Admin, Manager, User）
- ✅ 權限控制（Role-based Authorization）
- ✅ 個人資料查詢
- ✅ 使用者管理（僅 Admin）

### 技術涵蓋範圍
```
核心技術
├── Entity Framework Core    (資料存取)
├── LINQ                      (資料查詢)
├── async/await               (非同步程式設計)
├── JWT Authentication        (身份驗證)
├── Authorization             (權限控制)
├── Password Hashing          (密碼加密，只用 PasswordHasher)
├── RESTful API               (API 設計)
├── Options Pattern           (強型別設定) ⭐ 新增
├── FluentValidation          (進階驗證) ⭐ 新增
└── Serilog                   (結構化日誌) ⭐ 從 Phase 1 開始

進階技術
├── DTO Pattern               (資料傳輸物件)
├── Record Types              (不可變資料類型) ⭐ 必學
├── Primary Constructors      (簡化 DI) ⭐ C# 12
├── Result Pattern            (統一回應處理) ⭐ 新增
├── Global Exception Handler  (全域錯誤處理)
├── Swagger/OpenAPI           (API 文件)
└── Docker                    (容器化)

❌ 不推薦使用（本專案不用）
├── Repository Pattern        (DbContext 本身就是 Repository)
├── AutoMapper                (手動映射更清晰)
└── 完整 Identity 系統        (太重，自己設計更好學)
```

---

## 系統功能設計

### API 端點規劃

#### 認證相關 (Authentication)
| HTTP Method | Endpoint | 功能 | 權限要求 |
|-------------|----------|------|----------|
| POST | `/api/auth/register` | 註冊新使用者 | 無 |
| POST | `/api/auth/login` | 登入取得 Token | 無 |
| GET | `/api/auth/profile` | 取得個人資料 | 需登入 |
| PUT | `/api/auth/profile` | 更新個人資料 | 需登入 |

#### 使用者管理 (Users)
| HTTP Method | Endpoint | 功能 | 權限要求 |
|-------------|----------|------|----------|
| GET | `/api/users` | 取得所有使用者列表 | Admin |
| GET | `/api/users/{id}` | 取得指定使用者資料 | Admin / Manager |
| PUT | `/api/users/{id}` | 更新使用者資料 | Admin |
| DELETE | `/api/users/{id}` | 刪除使用者 | Admin |
| POST | `/api/users/{id}/roles` | 設定使用者角色 | Admin |
| DELETE | `/api/users/{id}/roles/{roleId}` | 移除使用者角色 | Admin |

#### 角色管理 (Roles)
| HTTP Method | Endpoint | 功能 | 權限要求 |
|-------------|----------|------|----------|
| GET | `/api/roles` | 取得所有角色 | Admin |
| GET | `/api/roles/{id}` | 取得指定角色 | Admin |
| POST | `/api/roles` | 新增角色 | Admin |
| PUT | `/api/roles/{id}` | 更新角色 | Admin |
| DELETE | `/api/roles/{id}` | 刪除角色 | Admin |

### 角色權限矩陣

| 功能 | Anonymous | User | Manager | Admin |
|------|-----------|------|---------|-------|
| 註冊 | ✅ | ✅ | ✅ | ✅ |
| 登入 | ✅ | ✅ | ✅ | ✅ |
| 查看自己資料 | ❌ | ✅ | ✅ | ✅ |
| 修改自己資料 | ❌ | ✅ | ✅ | ✅ |
| 查看其他使用者 | ❌ | ❌ | ✅ | ✅ |
| 管理使用者 | ❌ | ❌ | ❌ | ✅ |
| 設定角色 | ❌ | ❌ | ❌ | ✅ |
| 管理角色 | ❌ | ❌ | ❌ | ✅ |

---

## 資料庫設計

### 資料表結構

#### User (使用者表)
| 欄位名稱 | 型別 | 說明 | 備註 |
|---------|------|------|------|
| Id | int | 主鍵 | 自動遞增 |
| Username | string(50) | 使用者名稱 | 唯一，必填 |
| Email | string(100) | 電子郵件 | 唯一，必填 |
| PasswordHash | string(256) | 加密後的密碼 | 必填 |
| IsActive | bool | 帳號啟用狀態 | 預設 true |
| CreatedAt | datetime | 建立時間 | 自動產生 |
| UpdatedAt | datetime | 更新時間 | 自動更新 |

#### Role (角色表)
| 欄位名稱 | 型別 | 說明 | 備註 |
|---------|------|------|------|
| Id | int | 主鍵 | 自動遞增 |
| Name | string(50) | 角色名稱 | 唯一，必填 |
| Description | string(200) | 角色描述 | 可選 |
| CreatedAt | datetime | 建立時間 | 自動產生 |

#### UserRole (使用者角色關聯表)
| 欄位名稱 | 型別 | 說明 | 備註 |
|---------|------|------|------|
| UserId | int | 使用者 ID | 外鍵 → User.Id |
| RoleId | int | 角色 ID | 外鍵 → Role.Id |

**複合主鍵**：(UserId, RoleId)

### 資料表關聯圖

```
┌─────────────────────┐
│       User          │
├─────────────────────┤
│ Id (PK)             │
│ Username (Unique)   │
│ Email (Unique)      │
│ PasswordHash        │
│ IsActive            │
│ CreatedAt           │
│ UpdatedAt           │
└──────────┬──────────┘
           │
           │ 1
           │
           │ N
┌──────────┴──────────┐
│     UserRole        │
├─────────────────────┤
│ UserId (PK, FK)     │
│ RoleId (PK, FK)     │
└──────────┬──────────┘
           │
           │ N
           │
           │ 1
┌──────────┴──────────┐
│       Role          │
├─────────────────────┤
│ Id (PK)             │
│ Name (Unique)       │
│ Description         │
│ CreatedAt           │
└─────────────────────┘
```

### 關聯類型
- **User ↔ Role**：多對多 (Many-to-Many)
- 一個使用者可以有多個角色
- 一個角色可以被多個使用者擁有

### 為什麼只用 3 張表？

你在 Spring Boot 有用過 5 張表的完整 RBAC（User / Role / Permission + 2 個 mapping table）。

本專案簡化設計的原因：
1. **學習目的**：專注於 .NET 核心技術，而非 RBAC 複雜度
2. **Role-based 足夠**：這個專案的權限控制用 Role 就能滿足
3. **之後可擴展**：學完後可以自行加入 Permission 層

### 預設資料 (Seed Data)

#### 預設角色
| Name | Description |
|------|-------------|
| Admin | 系統管理員，擁有所有權限 |
| Manager | 管理者，可查看使用者資料 |
| User | 一般使用者，僅能管理自己的資料 |

#### 測試使用者
| Username | Email | Roles |
|----------|-------|-------|
| admin | admin@example.com | Admin |
| manager1 | manager@example.com | Manager |
| user1 | user@example.com | User |

---

## 技術棧選擇

### 開發環境
- **IDE**：VS Code + C# Dev Kit (或 Rider)
- **.NET 版本**：.NET 8 (LTS)
- **語言**：C# 12

### 核心框架與套件

#### 必要套件
| 套件名稱 | 用途 | 對應 Spring Boot |
|---------|------|------------------|
| Microsoft.AspNetCore.App | Web API 核心 | spring-boot-starter-web |
| Microsoft.EntityFrameworkCore | ORM 框架 | spring-boot-starter-data-jpa |
| Microsoft.EntityFrameworkCore.SqlServer | SQL Server 支援 | - |
| Npgsql.EntityFrameworkCore.PostgreSQL | PostgreSQL 支援 | - |
| Microsoft.EntityFrameworkCore.Tools | Migration 工具 | Flyway/Liquibase |
| Microsoft.AspNetCore.Authentication.JwtBearer | JWT 驗證 | spring-security-jwt |
| Swashbuckle.AspNetCore | Swagger/OpenAPI | springdoc-openapi |
| FluentValidation.AspNetCore | 進階驗證 ⭐ | Bean Validation |
| Serilog.AspNetCore | 結構化日誌 ⭐ | Logback + Logstash |

#### 密碼加密方式

> **不使用完整 Identity 系統的原因**
> 
> ASP.NET Core Identity 會自動產生 7-8 張表，很多你用不到。
> 學習階段自己設計 User/Role/UserRole 更好理解，只借用 `PasswordHasher<T>` 做密碼加密。

```csharp
// 只需要這個
using Microsoft.AspNetCore.Identity;

public class AuthService
{
    private readonly PasswordHasher<User> _passwordHasher = new();
    
    public string HashPassword(User user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }
    
    public bool VerifyPassword(User user, string hashedPassword, string providedPassword)
    {
        var result = _passwordHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
        return result == PasswordVerificationResult.Success;
    }
}
```

### ❌ 不推薦使用的套件

#### Repository Pattern — 不推薦

| 問題 | 詳細說明 |
|------|---------|
| **重複造輪子** | `DbContext` 本身就是 Unit of Work，`DbSet<T>` 本身就是 Repository |
| **遮蔽 EF Core 特性** | 包一層 Repository 後會失去 `Include()`、`AsNoTracking()` 等進階查詢能力 |
| **額外抽象層無價值** | 增加程式碼複雜度但沒帶來實質好處 |

**結論**：直接用 **Service Layer + DbContext**，不要多包一層 Repository。

---

#### AutoMapper — 不推薦

| 問題 | 詳細說明 |
|------|---------|
| **效能較差** | 使用反射機制，比手動映射慢 |
| **隱藏邏輯** | 轉換規則分散在 Profile 類別，debug 困難 |
| **過度工程** | 小專案用手動映射更清晰 |

**建議**：手動映射 + Extension Method，後續可學習 **Mapster**（效能更好）。

---

### 資料庫選擇

| 資料庫 | Docker 指令 |
|--------|------------|
| **PostgreSQL** | `docker run -d --name postgres -e POSTGRES_PASSWORD=password -p 5432:5432 postgres` |
| **SQL Server** | `docker run -d --name sqlserver -e ACCEPT_EULA=Y -e SA_PASSWORD=YourStrong@Passw0rd -p 1433:1433 mcr.microsoft.com/mssql/server` |

**實作時再決定用哪個，都用 Docker 執行。**

---

## 設定檔管理

### .NET 生態使用 JSON（不是 YAML）

| 框架 | 設定檔格式 | 原因 |
|------|-----------|------|
| Spring Boot | YAML (`application.yml`) | Java 生態偏好 |
| ASP.NET Core | JSON (`appsettings.json`) | .NET 生態標準 |

**建議直接用 JSON**，這是 .NET 的標準，所有教學資源都用 JSON。

---

### 專案設定檔結構

```
專案結構
├── Program.cs
├── appsettings.json              ← 主設定檔（≈ application.yml）
├── appsettings.Development.json  ← 開發環境（≈ application-dev.yml）
├── appsettings.Production.json   ← 生產環境（≈ application-prod.yml）
└── ...
```

---

### appsettings.json 範例（設定集中在這裡）

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=RbacDb;Username=postgres;Password=password"
  },
  
  "Jwt": {
    "Secret": "your-super-secret-key-at-least-32-characters-long",
    "Issuer": "RbacMemberSystem",
    "Audience": "RbacMemberSystem",
    "ExpiryMinutes": 60
  }
}
```

---

### 對照 Spring Boot

| Spring Boot (`application.yml`) | ASP.NET Core (`appsettings.json`) |
|--------------------------------|-----------------------------------|
| `jwt.secret` | `"Jwt": { "Secret": "..." }` |
| `jwt.expiry-minutes` | `"Jwt": { "ExpiryMinutes": 60 }` |
| `spring.datasource.url` | `"ConnectionStrings": { "DefaultConnection": "..." }` |

```yaml
# Spring Boot - application.yml
jwt:
  secret: your-super-secret-key
  expiry-minutes: 60
```

```json
// ASP.NET Core - appsettings.json
{
  "Jwt": {
    "Secret": "your-super-secret-key",
    "ExpiryMinutes": 60
  }
}
```

---

### 讀取設定的方式

#### ❌ 弱型別（不推薦）

```csharp
// 容易打錯字，沒有 IDE 提示
var secret = configuration["Jwt:Secret"];
var expiry = configuration["Jwt:ExpiryMinutes"];
```

#### ✅ 強型別 Options Pattern（推薦）

```csharp
// 1. 定義設定類別
public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 60;
}

// 2. Program.cs 註冊
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// 3. Service 中使用
public class JwtService(IOptions<JwtSettings> options)
{
    private readonly JwtSettings _settings = options.Value;
    
    public string GenerateToken()
    {
        var secret = _settings.Secret;        // IDE 自動完成
        var expiry = _settings.ExpiryMinutes; // 打錯字會編譯錯誤
    }
}
```

---

### 環境區分

```json
// appsettings.Development.json（開發環境覆蓋）
{
  "Jwt": {
    "Secret": "dev-secret-key-for-testing-only-32-chars",
    "ExpiryMinutes": 1440
  }
}
```

```json
// appsettings.Production.json（生產環境覆蓋）
{
  "Jwt": {
    "Secret": "production-super-secure-key-from-vault",
    "ExpiryMinutes": 30
  }
}
```

---

## 學習階段規劃

### Phase 1：環境建置與專案初始化

#### 學習目標
- 熟悉 dotnet CLI 指令
- 理解 ASP.NET Core 專案結構
- 設定開發環境
- ⭐ 整合 Serilog（從第一天就有良好的日誌）

#### 主要任務
1. 安裝 .NET 8 SDK
2. 建立 Web API 專案
3. 了解專案檔案結構
4. 安裝必要的 NuGet 套件
5. 設定 appsettings.json
6. ⭐ 整合 Serilog

#### 待討論重點
- [ ] dotnet CLI 常用指令
- [ ] 專案結構說明（Program.cs, Controllers, Models）
- [ ] NuGet 套件管理方式
- [ ] appsettings.json 設定方式（JSON 格式）
- [ ] Global Usings 的運作機制
- [ ] ⭐ Serilog 設定與使用

---

### Phase 2：Entity 與 DbContext 建立

#### 學習目標
- 理解 EF Core 的 Entity 概念
- 學會建立 DbContext
- 掌握多對多關聯設定
- 熟悉 Migration 機制
- ⭐ 學會 Record Types 用於 DTO

#### 主要任務
1. 建立 Entity 類別（User, Role, UserRole）
2. 建立 DbContext 類別
3. 設定 Entity 關聯（Fluent API）
4. 執行 Migration 建立資料表
5. 建立 Seed Data
6. ⭐ 使用 Record 定義 DTO

#### 待討論重點
- [ ] Entity 屬性定義（Property vs Field）
- [ ] 主鍵與外鍵設定
- [ ] 多對多關聯的 Fluent API 寫法
- [ ] Migration 指令（Add-Migration, Update-Database）
- [ ] Seed Data 的實作方式
- [ ] ⭐ Record vs Class 的選擇時機

---

### Phase 3：註冊功能實作

#### 學習目標
- 理解 Controller 與 Action 概念
- 學會建立 DTO（使用 Record）
- 掌握 FluentValidation
- 熟悉密碼加密機制
- 實際使用 async/await

#### 主要任務
1. 建立 RegisterDto（使用 Record）
2. 建立 RegisterDtoValidator（FluentValidation）
3. 建立 AuthController
4. 實作密碼加密（PasswordHasher）
5. 實作註冊邏輯
6. 測試註冊 API

#### ⭐ FluentValidation 範例

```csharp
public record RegisterDto(string Username, string Email, string Password, string ConfirmPassword);

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator(AppDbContext context)
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("使用者名稱為必填")
            .Length(3, 50).WithMessage("使用者名稱長度需在 3-50 字元之間");
        
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email 為必填")
            .EmailAddress().WithMessage("Email 格式不正確");
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密碼為必填")
            .MinimumLength(8).WithMessage("密碼至少需要 8 個字元");
        
        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("確認密碼與密碼不一致");
    }
}
```

---

### Phase 4：登入與 JWT 實作

#### 學習目標
- 理解 JWT 的組成與運作原理
- 學會產生與驗證 JWT Token
- 掌握 Claims-based 驗證
- 設定 Authentication Middleware
- ⭐ 學會 Options Pattern 管理 JWT 設定

#### 主要任務
1. 建立 LoginDto
2. ⭐ 建立 JwtSettings（Options Pattern）
3. 實作登入驗證邏輯
4. 建立 JWT Service
5. 產生 JWT Token（包含 Claims）
6. 設定 JWT Authentication
7. 測試登入流程

---

### Phase 5：Authorization 權限控制

#### 學習目標
- 理解 Authentication vs Authorization 差異
- 掌握 Role-based Authorization
- 學會使用 Authorize Attribute
- 實作不同權限的 API

#### 主要任務
1. 實作「取得個人資料」API（需登入）
2. 實作「取得所有使用者」API（需 Admin）
3. 實作「設定使用者角色」API（需 Admin）
4. 測試不同角色的存取限制

---

### Phase 6：LINQ 查詢實戰

#### 學習目標
- 掌握常用的 LINQ 查詢方法
- 理解 IQueryable vs IEnumerable
- 學會 Include 與 ThenInclude（Eager Loading）
- 熟悉 LINQ 轉 SQL 的機制

#### 主要任務
1. 查詢使用者列表（分頁、排序、篩選）
2. 查詢使用者的角色（Include）
3. 統計各角色的使用者數量（GroupBy + Count）
4. 複雜條件查詢（多條件 Where）

---

### Phase 7：錯誤處理與 Result Pattern

#### 學習目標
- 建立統一的錯誤回應格式
- 實作全域例外處理
- ⭐ 學會 Result Pattern
- 處理業務邏輯錯誤

#### ⭐ Result Pattern 範例

```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string? ErrorMessage { get; }
    public ErrorCode? ErrorCode { get; }
    
    public static Result<T> Success(T data) => new(true, data, null, null);
    public static Result<T> Failure(string message, ErrorCode code) => new(false, default, message, code);
}

// Service 層
public async Task<Result<UserDto>> GetUserAsync(int id)
{
    var user = await _context.Users.FindAsync(id);
    
    if (user == null)
        return Result<UserDto>.Failure("使用者不存在", ErrorCode.NotFound);
    
    return Result<UserDto>.Success(user.ToDto());
}

// Controller 層
public async Task<IActionResult> GetUser(int id)
{
    var result = await _userService.GetUserAsync(id);
    
    if (!result.IsSuccess)
        return NotFound(new { message = result.ErrorMessage });
    
    return Ok(result.Data);
}
```

---

### Phase 8：優化與文件

#### 學習目標
- 設定 Swagger/OpenAPI 文件
- 優化 API 回應格式
- 手動映射優化

#### 主要任務
1. 設定 Swagger UI
2. 設定 JWT Bearer 認證測試
3. 優化 DTO 轉換（Extension Method）

---

### Phase 9：Docker 化（可選）

#### 主要任務
1. 撰寫 Dockerfile
2. 撰寫 docker-compose.yml
3. 測試容器啟動

---

## Spring Boot 對照表

### 專案結構對照

| Spring Boot | ASP.NET Core | 說明 |
|-------------|--------------|------|
| src/main/java | / (專案根目錄) | 原始碼位置 |
| @SpringBootApplication | Program.cs | 程式進入點 |
| application.yml | appsettings.json | 設定檔 |
| pom.xml | .csproj | 專案設定檔 |
| @RestController | Controller + [ApiController] | REST API 控制器 |
| @Service | Service (無特殊註解) | 服務層 |
| @ConfigurationProperties | Options Pattern ⭐ | 強型別設定 |
| @Entity | Entity class | 資料實體 |

### 註解/屬性對照

| Spring Boot | ASP.NET Core | 用途 |
|-------------|--------------|------|
| @RestController | [ApiController] | 標記 REST Controller |
| @RequestMapping | [Route] | 路由設定 |
| @GetMapping | [HttpGet] | GET 請求 |
| @PostMapping | [HttpPost] | POST 請求 |
| @RequestBody | [FromBody] | 請求本體 |
| @PathVariable | [FromRoute] | 路由參數 |
| @RequestParam | [FromQuery] | 查詢參數 |
| @Valid | FluentValidation | 模型驗證 |
| @PreAuthorize | [Authorize] | 權限驗證 |
| @Autowired | 建構子注入 / Primary Constructor | 依賴注入 |
| @AllArgsConstructor | Primary Constructor (C# 12) | 簡化建構子 |

### JPA vs EF Core 對照

| Spring Boot (JPA) | ASP.NET Core (EF Core) | 說明 |
|-------------------|------------------------|------|
| @Entity | class (POCO) | 實體類別 |
| @Id | [Key] 或 Id 命名慣例 | 主鍵 |
| @ManyToMany | HasMany().WithMany() | 多對多關聯 |
| EntityManager | DbContext | 資料庫上下文 |
| findById() | FindAsync() | 依 ID 查詢 |
| save() | AddAsync() + SaveChangesAsync() | 新增/更新 |

---

## 關鍵技術說明

### 1. Entity Framework Core

#### 核心概念
- **Entity**：對應資料表的類別，使用 POCO
- **DbContext**：資料庫會話，負責追蹤 Entity 變更
- **DbSet\<T>**：代表一張資料表的集合
- **Migration**：資料庫版本控制機制

**本專案採用**：Code First

---

### 2. LINQ (Language Integrated Query)

#### 常用查詢方法對照

| LINQ 方法 | SQL 對應 | Java Stream 對應 |
|----------|---------|-----------------|
| Where | WHERE | filter() |
| Select | SELECT | map() |
| OrderBy | ORDER BY | sorted() |
| Skip | OFFSET | skip() |
| Take | LIMIT | limit() |
| Count | COUNT | count() |
| Any | EXISTS | anyMatch() |

#### 最重要的概念
- **IQueryable vs IEnumerable** - 何時轉成 SQL，何時在記憶體執行
- **延遲執行 (Deferred Execution)** - 查詢何時真正執行

---

### 3. async/await 非同步程式設計

#### 使用決策

| 操作 | 是否使用 async/await | 原因 |
|------|---------------------|------|
| 資料庫查詢 | ✅ 必須 | I/O 操作 |
| 密碼驗證 | ❌ 不用 | CPU 運算 |
| 產生 JWT | ❌ 不用 | 記憶體運算 |
| HTTP 呼叫外部 API | ✅ 必須 | I/O 操作 |

---

### 4. JWT (JSON Web Token)

#### JWT 結構
```
Header.Payload.Signature
```

#### 本專案 Claims 規劃
| Claim Type | 值 | 說明 |
|-----------|---|------|
| sub | UserId | 使用者 ID |
| email | Email | 電子郵件 |
| name | Username | 使用者名稱 |
| role | Role Names | 角色列表 |
| exp | Timestamp | 過期時間 |

---

### 5. 依賴注入 (Dependency Injection)

#### 生命週期選擇

| 生命週期 | 建立時機 | 何時使用 |
|---------|---------|---------|
| **Singleton** | 應用程式啟動時建立一次 | 無狀態的共用服務 |
| **Scoped** | 每個 HTTP 請求建立一次 | DbContext、Service |
| **Transient** | 每次注入都建立新實例 | 輕量、無狀態的小物件 |

---

## C# 與 .NET 新特性

### ⭐ Record Types（必學）

```csharp
// 傳統 class（冗長）
public class UserResponseDto 
{
    public int Id { get; set; }
    public string Username { get; set; }
}

// Record（簡潔 + 不可變 + 自動 Equals/GetHashCode）
public record UserResponseDto(int Id, string Username, string Email);
```

#### 何時該用 Record？何時該用 Class？

| 使用場景 | 建議 | 原因 |
|---------|------|------|
| DTO | ✅ Record | 不可變、值相等、簡潔 |
| Entity | ❌ Class | EF Core 需要追蹤變更 |
| Service | ❌ Class | 有狀態、需要 DI |

---

### ⭐ Primary Constructors（C# 12）

> **學習時會提供兩種寫法對照**，方便你理解差異。

```csharp
// ❌ 傳統寫法（冗長，但初學時會先看這個幫助理解）
public class UserService
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserService> _logger;
    
    public UserService(AppDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }
}

// ✅ Primary Constructor（C# 12，實際使用這個）
public class UserService(AppDbContext context, ILogger<UserService> logger)
{
    public async Task<User?> GetByIdAsync(int id)
    {
        return await context.Users.FindAsync(id);
    }
}
```

**如果依賴超過 5-6 個**：這是設計問題的警示，應該考慮拆分 Service。

---

### ⭐ Global Usings

**不會影響效能！** 這是編譯時期的事情，編譯器只會 include 實際有用到的型別。

```csharp
// GlobalUsings.cs
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using Microsoft.EntityFrameworkCore;

// 其他檔案就不用重複寫這些 using
```

---

## 待討論與學習重點

### C# 語法差異

#### 必須搞懂
- [ ] Property vs Field
- [ ] Lambda 表達式
- [ ] Null 安全運算子（`?.`, `??`, `!`）
- [ ] Expression Body（`=>`）
- [ ] 擴充方法
- [ ] ⭐ Record Types
- [ ] ⭐ Primary Constructors

---

### LINQ 查詢

#### 必須搞懂
- [ ] Where, Select, OrderBy
- [ ] FirstOrDefault / SingleOrDefault
- [ ] ToList / ToListAsync
- [ ] Include / ThenInclude
- [ ] Any / All

#### 最重要的概念
- [ ] IQueryable vs IEnumerable
- [ ] 延遲執行 (Deferred Execution)

---

### async/await

#### 使用時機（最重要）
- [ ] ✅ 何時必須用 - 資料庫、HTTP、檔案 I/O
- [ ] ❌ 何時不該用 - 記憶體運算、簡單邏輯

#### 常見陷阱
- [ ] 不要用 async void
- [ ] 不要用 Task.Result（會死鎖）
- [ ] 不要忘記 await

---

## 專案 Checklist

### Phase 1: 環境建置
- ✅ 安裝 .NET SDK（實際安裝 .NET 9）
- ✅ 安裝 VS Code + C# Dev Kit
- ✅ 安裝 VS Code 擴充套件（XML by Red Hat）
- ✅ 建立專案（dotnet new webapi）
- ✅ 解決 .NET 9 + Swagger 套件衝突
- ✅ 安裝必要套件（EF Core, Serilog, FluentValidation, JWT Bearer）
- ✅ 學習套件安全檢查（--vulnerable, --outdated, --include-transitive）
- ✅ 設定 appsettings.json（ConnectionStrings, Jwt）
- ✅ 整合 Serilog
- ✅ 建立專案資料夾結構（Entities, Data, Services, DTOs, etc.）
- ✅ 建立 GlobalUsings.cs
- ✅ 建立 Docker 環境（PostgreSQL）
- ✅ 建立 docker-compose.yml, .env, README.md
- ✅ 建立 TestController 並測試 Swagger UI

### Phase 2: 資料庫設計
- ✅ 建立 Entity 類別（User, Role, UserRole）
- ✅ 理解 `string?` 與 `ICollection<T>`
- ✅ 建立 DbContext
- ✅ 使用 `IEntityTypeConfiguration<T>` 分離設定
- ✅ 設定多對多關聯（Fluent API）
- ✅ 執行 Migration（`dotnet ef migrations add`）
- ✅ 套用 Migration（`dotnet ef database update`）
- ✅ 建立 Seed Data（預設 3 個 Role）
- ✅ 更新 dotnet-ef 工具到 9.0.0

### Phase 3: 註冊功能
- [ ] 建立 RegisterDto（Record）
- [ ] ⭐ 建立 Validator（FluentValidation）
- [ ] 建立 AuthController
- [ ] 實作密碼加密
- [ ] 測試註冊 API

### Phase 4: 登入功能
- [ ] 建立 LoginDto
- [ ] ⭐ 建立 JwtSettings（Options Pattern）
- [ ] 建立 JWT Service
- [ ] 設定 JWT 驗證
- [ ] 測試登入 API

### Phase 5: 權限控制
- [ ] 實作取得個人資料
- [ ] 實作取得使用者列表
- [ ] 實作設定角色
- [ ] 測試權限限制

### Phase 6: LINQ 實戰
- [ ] 分頁查詢
- [ ] Include 關聯查詢
- [ ] GroupBy 統計

### Phase 7: 錯誤處理
- [ ] ⭐ 實作 Result Pattern
- [ ] 全域例外處理
- [ ] 統一回應格式

### Phase 8: 優化
- [ ] Swagger 文件
- [ ] 手動映射優化

### Phase 9: Docker（可選）
- [ ] Dockerfile
- [ ] Docker Compose

---

## 問題紀錄區

### 待確認問題
- [ ] 要用 MS SQL 還是 PostgreSQL？（都用 Docker）
- [ ] 要不要實作 Refresh Token？（進階功能）

### 學習疑問
- [ ] （實作時記錄）

---

**最後更新：2025-12-26**
