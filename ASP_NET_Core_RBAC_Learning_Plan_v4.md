# ASP.NET Core RBAC 會員系統學習計畫 (Final)

> **目標**：透過實作 RBAC 會員系統，快速掌握 ASP.NET Core 核心技術
> **前置知識**：有 Spring Boot + Java 17 開發經驗
> **版本**：v4 - 實作進行中
> **最後更新**：2025-12-26

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
10. [學習筆記](#學習筆記)

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
├── Entity Framework Core 9.0  (資料存取) ✅ 已設定
├── LINQ                       (資料查詢)
├── async/await                (非同步程式設計)
├── JWT Authentication         (身份驗證)
├── Authorization              (權限控制)
├── Password Hashing           (密碼加密，只用 PasswordHasher)
├── RESTful API                (API 設計) ✅ 已設定
├── Options Pattern            (強型別設定)
├── FluentValidation           (進階驗證) ✅ 已安裝
└── Serilog                    (結構化日誌) ✅ 已整合

進階技術
├── DTO Pattern                       (資料傳輸物件)
├── Record Types                      (不可變資料類型)
├── Primary Constructors              (簡化 DI) C# 12
├── IEntityTypeConfiguration<T>       (Entity 設定分離) ✅ 已使用
├── Result Pattern                    (統一回應處理)
├── Global Exception Handler          (全域錯誤處理)
├── Swagger/OpenAPI                   (API 文件) ✅ 已設定
└── Docker                            (容器化) ✅ 已設定

❌ 不使用（本專案不用）
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

### 已確定選擇 ✅

| 項目 | 選擇 | 原因 |
|------|------|------|
| 資料庫 | **PostgreSQL 16** | Docker 部署方便 |
| ORM | **EF Core 9.0.0** | 配合 .NET 9 |
| 連線方式 | **Npgsql** | PostgreSQL 官方驅動 |

### 資料表結構（已建立 ✅）

#### User (使用者表)
| 欄位名稱 | 型別 | 說明 | 備註 |
|---------|------|------|------|
| Id | int | 主鍵 | 自動遞增 |
| Username | string(50) | 使用者名稱 | 唯一，必填 |
| Email | string(100) | 電子郵件 | 唯一，必填 |
| PasswordHash | string(256) | 加密後的密碼 | 必填 |
| IsActive | bool | 帳號啟用狀態 | 預設 true |
| CreatedAt | datetime | 建立時間 | 自動產生 |
| UpdatedAt | datetime | 更新時間 | 可 null |

#### Role (角色表)
| 欄位名稱 | 型別 | 說明 | 備註 |
|---------|------|------|------|
| Id | int | 主鍵 | 自動遞增 |
| Name | string(50) | 角色名稱 | 唯一，必填 |
| Description | string(200) | 角色描述 | 可 null |
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
           │ 1
           │
           │ N
┌──────────┴──────────┐
│     UserRole        │
├─────────────────────┤
│ UserId (PK, FK)     │
│ RoleId (PK, FK)     │
└──────────┬──────────┘
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

### 預設資料 (Seed Data) ✅ 已建立

#### 預設角色
| Name | Description |
|------|-------------|
| Admin | 系統管理員，擁有所有權限 |
| Manager | 管理者，可查看使用者資料 |
| User | 一般使用者，僅能管理自己的資料 |

---

## 技術棧選擇

### 開發環境（已確定 ✅）

| 項目 | 選擇 | 備註 |
|------|------|------|
| IDE | VS Code + C# Dev Kit | 另裝 XML (Red Hat) 擴充套件 |
| .NET 版本 | **.NET 9** | 原計劃 .NET 8，實際安裝 .NET 9 |
| 語言 | C# 12 | |
| 資料庫 | PostgreSQL 16 | Docker 部署 |

### 核心套件（已安裝 ✅）

| 套件名稱 | 版本 | 用途 | 對應 Spring Boot |
|---------|------|------|------------------|
| Microsoft.EntityFrameworkCore | 9.0.0 | ORM 框架 | spring-boot-starter-data-jpa |
| Npgsql.EntityFrameworkCore.PostgreSQL | 9.0.0 | PostgreSQL 支援 | postgresql driver |
| Microsoft.EntityFrameworkCore.Tools | 9.0.0 | Migration 工具 | Flyway/Liquibase |
| Microsoft.AspNetCore.Authentication.JwtBearer | 9.0.0 | JWT 驗證 | spring-security-jwt |
| Swashbuckle.AspNetCore | 6.6.2 | Swagger/OpenAPI | springdoc-openapi |
| FluentValidation.AspNetCore | 11.3.1 | 進階驗證 | Bean Validation |
| Serilog.AspNetCore | 10.0.0 | 結構化日誌 | Logback + Logstash |
| Serilog.Sinks.Console | 6.1.1 | Console 輸出 | - |

### 套件安全檢查指令

```bash
# 檢查資安漏洞
dotnet list package --vulnerable

# 檢查過時套件
dotnet list package --outdated

# 檢查所有相依（含間接）
dotnet list package --include-transitive
```

### ❌ 不使用的套件（已確定）

| 套件/模式 | 不用原因 |
|----------|---------|
| Repository Pattern | DbContext 本身就是 Unit of Work，DbSet<T> 就是 Repository |
| AutoMapper | 手動映射更清晰，效能更好 |
| 完整 Identity 系統 | 會產生 7-8 張表，太重。只借用 PasswordHasher<T> |

---

## 設定檔管理

### .NET 使用 JSON（不是 YAML）✅

| 框架 | 設定檔格式 |
|------|-----------|
| Spring Boot | YAML (`application.yml`) |
| ASP.NET Core | JSON (`appsettings.json`) |

### 目前專案設定檔

```
RbacMemberSystem.Api/
├── appsettings.json              ← 主設定檔
├── appsettings.Development.json  ← 開發環境
└── appsettings.Production.json   ← 生產環境（未建立）
```

### appsettings.json（目前內容）

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=RbacMemberDb;Username=postgres;Password=postgres"
  },
  
  "Jwt": {
    "Secret": "this-is-a-super-secret-key-for-jwt-token-generation-at-least-32-characters",
    "Issuer": "RbacMemberSystem",
    "Audience": "RbacMemberSystem",
    "ExpiryMinutes": 60
  }
}
```

---

## 學習階段規劃

### Phase 1：環境建置與專案初始化 ✅ 完成

* ✅ 安裝 .NET 9 SDK
* ✅ 安裝 VS Code + C# Dev Kit
* ✅ 安裝 VS Code 擴充套件（XML by Red Hat）
* ✅ 建立專案（`dotnet new webapi`）
* ✅ 解決 .NET 9 + Swagger 套件衝突（移除 Microsoft.AspNetCore.OpenApi）
* ✅ 安裝必要套件（EF Core 9.0.0, Serilog, FluentValidation, JWT Bearer 9.0.0）
* ✅ 學習套件安全檢查（`--vulnerable`, `--outdated`, `--include-transitive`）
* ✅ 設定 appsettings.json（ConnectionStrings, Jwt）
* ✅ 整合 Serilog
* ✅ 建立專案資料夾結構（Entities, Data, Services, DTOs, etc.）
* ✅ 建立 GlobalUsings.cs
* ✅ 建立 Docker 環境（PostgreSQL 16）
* ✅ 建立 docker-compose.yml, .env, README.md
* ✅ 建立 TestController 並測試 Swagger UI

---

### Phase 2：Entity 與 DbContext 建立 ✅ 完成

* ✅ 建立 Entity 類別（User, Role, UserRole）
* ✅ 理解 `string?`（Nullable Reference Type）與 `ICollection<T>`
* ✅ 理解 Value Type vs Reference Type 預設值差異
* ✅ 建立 DbContext（AppDbContext）
* ✅ 使用 `IEntityTypeConfiguration<T>` 分離設定（避免 DbContext 肥大）
* ✅ 設定多對多關聯（Fluent API：HasOne, WithMany, HasForeignKey）
* ✅ 執行 Migration（`dotnet ef migrations add InitialCreate`）
* ✅ 套用 Migration（`dotnet ef database update`）
* ✅ 建立 Seed Data（DataSeeder，預設 3 個 Role）
* ✅ 更新 dotnet-ef 工具到 9.0.0
* ✅ Git 初始化並推送到 GitHub

---

### Phase 3：註冊功能實作 🔄 進行中

#### 學習目標
- 理解 Controller 與 Action 概念
- 學會建立 DTO（使用 Record）
- 掌握 FluentValidation
- 熟悉密碼加密機制
- 實際使用 async/await

#### 主要任務
- [ ] 建立 RegisterDto（使用 Record）
- [ ] 建立 RegisterDtoValidator（FluentValidation）
- [ ] 建立 AuthService（密碼加密）
- [ ] 建立 AuthController
- [ ] 實作註冊邏輯
- [ ] 測試註冊 API

---

### Phase 4：登入與 JWT 實作

#### 主要任務
- [ ] 建立 LoginDto
- [ ] 建立 JwtSettings（Options Pattern）
- [ ] 建立 JwtService
- [ ] 設定 JWT Authentication
- [ ] 實作登入邏輯
- [ ] 測試登入 API

---

### Phase 5：Authorization 權限控制

#### 主要任務
- [ ] 實作「取得個人資料」API（需登入）
- [ ] 實作「取得所有使用者」API（需 Admin）
- [ ] 實作「設定使用者角色」API（需 Admin）
- [ ] 測試不同角色的存取限制

---

### Phase 6：LINQ 查詢實戰

#### 主要任務
- [ ] 分頁查詢
- [ ] Include 關聯查詢
- [ ] GroupBy 統計
- [ ] 理解 IQueryable vs IEnumerable

---

### Phase 7：錯誤處理與 Result Pattern

#### 主要任務
- [ ] 實作 Result Pattern
- [ ] 全域例外處理
- [ ] 統一回應格式

---

### Phase 8：優化與文件

#### 主要任務
- [ ] Swagger 文件優化
- [ ] 手動映射優化（Extension Method）

---

### Phase 9：Docker 化（可選）

#### 主要任務
- [ ] 撰寫 Dockerfile
- [ ] 整合 docker-compose.yml

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
| @ConfigurationProperties | Options Pattern | 強型別設定 |
| @Entity | Entity class (POCO) | 資料實體 |

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

### JPA vs EF Core 對照

| Spring Boot (JPA) | ASP.NET Core (EF Core) | 說明 |
|-------------------|------------------------|------|
| @Entity | class (POCO) | 實體類別 |
| @Id | [Key] 或 Id 命名慣例 | 主鍵 |
| @Column | Fluent API 或 Data Annotation | 欄位設定 |
| @ManyToMany | HasMany().WithMany() | 多對多關聯 |
| EntityManager | DbContext | 資料庫上下文 |
| JpaRepository | DbSet<T> | 資料存取 |
| findById() | FindAsync() | 依 ID 查詢 |
| save() | AddAsync() + SaveChangesAsync() | 新增/更新 |

### CLI 指令對照

| 用途 | Maven | .NET CLI |
|------|-------|----------|
| 建立專案 | Spring Initializr | `dotnet new webapi` |
| 編譯 | `mvn compile` | `dotnet build` |
| 執行 | `mvn spring-boot:run` | `dotnet run` |
| 加套件 | 編輯 pom.xml | `dotnet add package` |
| 查相依樹 | `mvn dependency:tree` | `dotnet list package --include-transitive` |
| 檢查漏洞 | OWASP plugin | `dotnet list package --vulnerable` |

---

## 關鍵技術說明

### 1. Entity Framework Core

#### 核心概念
- **Entity**：對應資料表的類別，使用 POCO（不需要註解）
- **DbContext**：資料庫會話，負責追蹤 Entity 變更
- **DbSet<T>**：代表一張資料表的集合
- **Migration**：資料庫版本控制機制
- **IEntityTypeConfiguration<T>**：分離 Entity 設定，避免 DbContext 肥大

#### Migration 指令
```bash
# 建立 Migration
dotnet ef migrations add <MigrationName>

# 套用 Migration
dotnet ef database update

# 移除最後一個 Migration
dotnet ef migrations remove
```

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
| FirstOrDefault | LIMIT 1 | findFirst() |

---

### 3. async/await 非同步程式設計

#### 使用時機

| 操作 | 是否使用 async/await | 原因 |
|------|---------------------|------|
| 資料庫查詢 | ✅ 必須 | I/O 操作 |
| 密碼驗證 | ❌ 不用 | CPU 運算 |
| 產生 JWT | ❌ 不用 | 記憶體運算 |
| HTTP 呼叫外部 API | ✅ 必須 | I/O 操作 |

---

### 4. 依賴注入 (Dependency Injection)

#### 生命週期選擇

| 生命週期 | 建立時機 | 何時使用 |
|---------|---------|---------|
| **Singleton** | 應用程式啟動時建立一次 | 無狀態的共用服務 |
| **Scoped** | 每個 HTTP 請求建立一次 | DbContext、Service |
| **Transient** | 每次注入都建立新實例 | 輕量、無狀態的小物件 |

---

## C# 與 .NET 新特性

### Nullable Reference Type（`string?`）

```csharp
public string Name { get; set; }      // 不應該是 null
public string? Description { get; set; }  // 可以是 null（選填欄位）
```

### Value Type vs Reference Type

| 類型 | 分類 | 預設值 | 需要給預設值？ |
|------|------|--------|---------------|
| `int` | Value Type | `0` | ❌ 不用 |
| `bool` | Value Type | `false` | ❌ 不用 |
| `DateTime` | Value Type | `0001-01-01` | ❌ 不用 |
| `string` | Reference Type | `null` | ⚠️ 要，不然有警告 |

### Record Types

```csharp
// 用於 DTO，簡潔 + 不可變 + 自動 Equals/GetHashCode
public record UserResponseDto(int Id, string Username, string Email);
```

### Primary Constructors（C# 12）

```csharp
// 傳統寫法
public class UserService
{
    private readonly AppDbContext _context;
    
    public UserService(AppDbContext context)
    {
        _context = context;
    }
}

// Primary Constructor（簡潔）
public class UserService(AppDbContext context)
{
    // 直接使用 context
}
```

---

## 學習筆記

### 遇到的問題與解法

#### 1. .NET 9 + Swagger 套件衝突
**問題**：`Microsoft.AspNetCore.OpenApi` 與 `Swashbuckle.AspNetCore` 衝突
**解法**：移除 `Microsoft.AspNetCore.OpenApi`，只保留 `Swashbuckle.AspNetCore 6.6.2`

#### 2. EF Core 版本問題
**問題**：`dotnet add package` 預設抓最新版（10.x），與 .NET 9 不相容
**解法**：指定版本 `dotnet add package Microsoft.EntityFrameworkCore --version 9.0.0`

#### 3. dotnet-ef 工具安裝失敗
**問題**：NuGet 快取損壞
**解法**：`dotnet nuget locals all --clear` 後指定版本安裝 `dotnet tool install --global dotnet-ef --version 9.0.0`

#### 4. EF Migration 時 Serilog 誤報錯誤
**問題**：EF 工具「假啟動」應用程式時觸發 `HostAbortedException`
**說明**：這是正常現象，Migration 實際上成功了

---

## 目前專案結構

```
RbacMemberSystem/
├── docker-compose.yml          ← Docker PostgreSQL 設定
├── .env                        ← 環境變數
├── README.md                   ← Docker 使用說明
├── .gitignore
└── RbacMemberSystem.Api/
    ├── Controllers/
    │   └── TestController.cs
    ├── Data/
    │   ├── AppDbContext.cs
    │   ├── Configurations/
    │   │   ├── UserConfiguration.cs
    │   │   ├── RoleConfiguration.cs
    │   │   └── UserRoleConfiguration.cs
    │   └── Seeds/
    │       └── DataSeeder.cs
    ├── Entities/
    │   ├── User.cs
    │   ├── Role.cs
    │   └── UserRole.cs
    ├── Migrations/
    │   ├── 20251226xxxxxx_InitialCreate.cs
    │   └── AppDbContextModelSnapshot.cs
    ├── DTOs/                    ← Phase 3 會用
    ├── Services/                ← Phase 3 會用
    ├── Validators/              ← Phase 3 會用
    ├── Extensions/              ← Phase 3 會用
    ├── Configurations/          ← Phase 4 會用（Options Pattern）
    ├── Properties/
    │   └── launchSettings.json
    ├── appsettings.json
    ├── appsettings.Development.json
    ├── GlobalUsings.cs
    ├── Program.cs
    └── RbacMemberSystem.Api.csproj
```

---

## VS Code 快捷鍵備忘

| 快捷鍵 | 功能 |
|--------|------|
| `Ctrl + P` | 快速開啟檔案 |
| `Ctrl + Shift + F` | 全專案搜尋 |
| `Ctrl + Space` | 觸發自動補全 |
| `Shift + Alt + F` | 格式化檔案 |
| `F12` | 跳到定義 |
| `Ctrl + Shift + P` | 命令面板 |

---

## Git Repository

**GitHub**: https://github.com/orps2678/RbacMemberSystem

---

**最後更新：2025-12-26**