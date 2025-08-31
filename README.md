# JwtPratik

.NET 8 üzerinde JWT (JSON Web Token) ile kimlik doğrulama yapan örnek bir Web API projesi. Veriler Entity Framework Core ile **SQL Server (LocalDB/Express/Tam Sunucu)** üzerinde tutulur. Swagger üzerinden kolayca test edilebilir.

---

## 🎯 Özellikler

* Kullanıcı kaydı (e‑posta + parola)
* Güvenli parola saklama (PBKDF2 + salt)
* JWT üretimi (issuer, audience, expire, symmetric key)
* JWT doğrulama ve **\[Authorize]** ile korunan endpointler
* EF Core Code‑First migration’ları
* Swagger UI’da Bearer Token ile test

---

## 🧰 Teknolojiler

* .NET 8 (ASP.NET Core Web API)
* Entity Framework Core 8 (SQL Server)
* Microsoft.AspNetCore.Authentication.JwtBearer
* Swashbuckle (Swagger)

---

## 📁 Proje Yapısı

```
JwtPratik/
├─ Controllers/
│  └─ AuthController.cs         # register, login, me
├─ Data/
│  └─ AppDbContext.cs          # EF Core DbContext
├─ DTOs/
│  ├─ LoginRequest.cs
│  └─ RegisterRequest.cs
├─ Models/
│  └─ User.cs                   # Id, Email, PasswordHash, PasswordSalt
├─ Services/
│  ├─ PasswordHasher.cs         # PBKDF2 hash/salt
│  └─ TokenService.cs           # JWT üretimi
├─ Program.cs                   # SQL Server, JWT ve Swagger konfigürasyonu
└─ appsettings.json             # Connection string + JWT ayarları
```

---

## ⚙️ Kurulum

### 1) Ön Koşullar

* .NET SDK **8.x**
* SQL Server LocalDB **veya** SQL Express **veya** tam bir SQL Server örneği
* Visual Studio 2022/VS Code

### 2) Bağımlılıkları Yükleme

Repo klasöründe:

```bash
dotnet restore
```

> Visual Studio kullanıyorsanız ilk açılışta restore otomatik yapılır.

### 3) `appsettings.json` (SQL Server)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=JwtPratikDb;Trusted_Connection=True;TrustServerCertificate=True"
    // Alternatif: "Server=.\\SQLEXPRESS;Database=JwtPratikDb;Trusted_Connection=True;TrustServerCertificate=True"
    // Alternatif: "Server=YOUR_SERVER;Database=JwtPratikDb;User Id=sa;Password=YourStrong(!)Password;TrustServerCertificate=True"
  },
  "Jwt": {
    "Key": "SUPER_SECRET_KEY_MIN_32_CHARS_CHANGE_ME_1234567890",
    "Issuer": "JwtPratik",
    "Audience": "JwtPratikClient",
    "ExpireMinutes": 60
  }
}
```

> **Güvenlik:** `Jwt:Key` değerini production’da **kendi gizli anahtarınızla** değiştirin (User‑Secrets/Azure Key Vault vb. kullanın).

### 4) Veritabanını Oluşturma (EF Core Migrations)

**CLI:**

```bash
dotnet tool install --global dotnet-ef   # (tek seferlik)
dotnet ef migrations add InitialCreate
dotnet ef database update
```

**Visual Studio – Package Manager Console:**

```powershell
Add-Migration InitialCreate
Update-Database
```

### 5) Çalıştırma

```bash
dotnet run
```

Tarayıcı: `https://localhost:xxxxx/swagger`

---

## 🧪 Swagger ile Test

1. **POST** `/api/auth/register`  → kullanıcı oluştur
2. **POST** `/api/auth/login`     → token al (`token` alanını kopyalayın)
3. Swagger sağ üst **Authorize** → `Bearer <token>` girin
4. **GET** `/api/auth/me`         → korumalı endpoint’ten kimlik bilgilerini alın

---

## 🔌 API Uç Noktaları

### POST `/api/auth/register`

**Body**

```json
{ "email": "test@example.com", "password": "Password123!" }
```

**201**

```json
{ "id": 1, "email": "test@example.com" }
```

**400** – e‑posta kullanımda / validasyon hatası

### POST `/api/auth/login`

**Body**

```json
{ "email": "test@example.com", "password": "Password123!" }
```

**200**

```json
{ "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." }
```

**401** – e‑posta/şifre hatalı

### GET `/api/auth/me`  *(Authorize)*

**200**

```json
{ "userId": "1", "email": "test@example.com" }
```

**401** – Token yok/Geçersiz

---

## 🔐 JWT Yapılandırması

`appsettings.json` → `Jwt` bölümünden:

* **Key**: İmzalama anahtarı (SymmetricSecurityKey)
* **Issuer**/**Audience**: Üreten ve hedef kitle bilgisi
* **ExpireMinutes**: Token geçerlilik süresi

`TokenService` claim’leri: `sub` (kullanıcı Id), `email`, `jti`.

---

## 🧱 \[Authorize] ile Yeni Endpoint Koruma (Örnek)

```csharp
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { message = "secret area" });
}
```
