# 🔐 Hướng Dẫn Logic Xác Thực (Authentication) - Hệ Thống Quản Lý Phòng Học

## 📋 Tổng Quan

Đã triển khai hệ thống xác thực hoàn chỉnh cho hệ thống Quản Lý Phòng Học với:

- ✅ **Password Hashing** sử dụng BCrypt (workfactor 12)
- 🔑 **JWT Token** cho API authentication
- 🔒 **Validation TrangThai** (Chờ duyệt, Khóa, Từ chối, Hoạt động)
- 📝 **Logging** toàn bộ hoạt động xác thực
- 🛡️ **Session & Token** hỗ trợ
- ⚡ **Error Handling** chi tiết

---

## 🏗️ Cấu Trúc Thư Mục

```
QLPHONGHOC/
├── Models/Auth/
│   └── AuthModels.cs                # DTO models (Request/Response)
├── Services/
│   ├── AuthService.cs               # Logic xác thực chính
│   └── TokenService.cs              # Quản lý JWT Token
├── Controllers/
│   └── AccountController.cs         # API endpoints
├── Program.cs                        # Cấu hình DI & Authentication
├── appsettings.json                 # JWT Settings
└── Views/Account/
	├── Login.cshtml                 # Giao diện đăng nhập
	└── Register.cshtml              # Giao diện đăng ký
```

---

## 🔒 Password Security

### Hash Password (BCrypt)

```csharp
// Hashing mật khẩu
string hashedPassword = authService.HashPassword("MyPassword123");

// Verify mật khẩu
bool isValid = authService.VerifyPassword("MyPassword123", hashedPassword);
```

**Cấu hình BCrypt:**
- Work Factor: 12 (10-12 được khuyến nghị)
- Algorithm: PBKDF2 + Blowfish
- Salt: Tự động sinh

### Database Schema

Mật khẩu cần lưu dưới dạng hash BCrypt (độ dài ~60 ký tự):

```sql
ALTER TABLE TAIKHOAN ALTER COLUMN MatKhau VARCHAR(255);
```

---

## 🔑 JWT Token Configuration

### appsettings.json

```json
{
  "JwtSettings": {
	"SecretKey": "your-secret-key-must-be-at-least-32-characters-long-for-security",
	"Issuer": "QLPhongHocAPI",
	"Audience": "QLPhongHocClients",
	"ExpiryMinutes": 60
  }
}
```

**⚠️ BẢNG: Thay đổi SecretKey trong Production!**

### Token Structure

```
Header:
{
  "alg": "HS256",
  "typ": "JWT"
}

Payload:
{
  "nameid": "1",
  "unique_name": "admin",
  "email": "admin@example.com",
  "HoTen": "Quản Trị Viên",
  "MaVaiTro": "1",
  "TrangThai": "Hoạt động",
  "VaiTro": "Admin",
  "exp": 1234567890,
  "iss": "QLPhongHocAPI",
  "aud": "QLPhongHocClients"
}
```

---

## 📡 API Endpoints

### 1. POST: /Account/Login (Form)
**Đăng nhập qua Form (Session-based)**

**Request:**
```html
<form method="post" action="/Account/Login">
  <input type="text" name="TenDangNhap" />
  <input type="password" name="MatKhau" />
  <input type="checkbox" name="GhiNho" />
  <button type="submit">Đăng nhập</button>
</form>
```

**Response:**
- ✅ Success: Redirect to Home, Session saved
- ❌ Error: Redirect to Login with error message

---

### 2. POST: /api/Account/Login (JSON API)
**Đăng nhập qua API (JWT Token)**

**Request:**
```bash
POST /api/Account/Login
Content-Type: application/json

{
  "tenDangNhap": "admin",
  "matKhau": "password123",
  "ghiNho": false
}
```

**Response (Success):**
```json
HTTP/1.1 200 OK
Content-Type: application/json

{
  "success": true,
  "message": "Đăng nhập thành công.",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600,
  "userInfo": {
	"maTaiKhoan": 1,
	"tenDangNhap": "admin",
	"hoTen": "Quản Trị Viên",
	"email": "admin@example.com",
	"soDienThoai": "0987654321",
	"maVaiTro": 1,
	"tenVaiTro": "Admin",
	"trangThai": "Hoạt động"
  }
}
```

**Response (Error - Invalid Credentials):**
```json
HTTP/1.1 401 Unauthorized
Content-Type: application/json

{
  "success": false,
  "message": "Tên đăng nhập hoặc mật khẩu không đúng.",
  "errorCode": "INVALID_CREDENTIALS"
}
```

**Response (Error - Account Pending):**
```json
HTTP/1.1 401 Unauthorized
Content-Type: application/json

{
  "success": false,
  "message": "Tài khoản của bạn đang chờ phê duyệt.",
  "errorCode": "ACCOUNT_PENDING"
}
```

---

### 3. POST: /Account/Register (Form)
**Đăng ký tài khoản mới**

**Request:**
```html
<form method="post" action="/Account/Register">
  <input type="text" name="TenDangNhap" />
  <input type="text" name="HoTen" />
  <input type="email" name="Email" />
  <input type="tel" name="SoDienThoai" />
  <input type="password" name="MatKhau" />
  <input type="password" name="XacNhanMatKhau" />
  <button type="submit">Đăng ký</button>
</form>
```

**Response:**
- ✅ Success: Redirect to Login, new account in "Chờ duyệt" status
- ❌ Error: Redirect to Register with error message

---

### 4. POST: /api/Account/Register (JSON API)
**Đăng ký qua API**

**Request:**
```bash
POST /api/Account/Register
Content-Type: application/json

{
  "tenDangNhap": "newuser",
  "hoTen": "Người Dùng Mới",
  "email": "newuser@example.com",
  "soDienThoai": "0987654321",
  "matKhau": "password123",
  "xacNhanMatKhau": "password123"
}
```

**Response (Success):**
```json
HTTP/1.1 200 OK
Content-Type: application/json

{
  "success": true,
  "message": "Đăng ký thành công! Vui lòng chờ phê duyệt từ quản trị viên.",
  "maTaiKhoan": 5
}
```

**Response (Error - Username Exists):**
```json
HTTP/1.1 400 Bad Request
Content-Type: application/json

{
  "success": false,
  "message": "Tên đăng nhập đã tồn tại.",
  "errorCode": "USERNAME_EXISTS"
}
```

---

## 🔐 TrangThai Validation

### Các trạng thái hợp lệ

| TrangThai | Mô Tả | Cho phép Đăng nhập | Thông báo |
|-----------|-------|-------------------|-----------|
| `Hoạt động` | Tài khoản bình thường | ✅ Yes | - |
| `Chờ duyệt` | Chờ admin phê duyệt | ❌ No | "Tài khoản của bạn đang chờ phê duyệt." |
| `Khóa` | Bị admin khóa | ❌ No | "Tài khoản đã bị khóa." |
| `Từ chối` | Đăng ký bị từ chối | ❌ No | "Yêu cầu đăng ký tài khoản bị từ chối." |

### Quy tắc

1. **Sau khi đăng ký:** Trạng thái = "Chờ duyệt"
2. **Admin phê duyệt:** Trạng thái = "Hoạt động"
3. **Admin từ chối:** Trạng thái = "Từ chối"
4. **Admin khóa tài khoản:** Trạng thái = "Khóa"

---

## 🛡️ Authorization (API)

### Sử dụng JWT Token

**Request với Bearer Token:**
```bash
GET /api/PhongHoc
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Protected Controller Action:**
```csharp
[Authorize]
[HttpGet("api/PhongHoc")]
public IActionResult GetPhongHoc()
{
	var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
	var userRole = User.FindFirst("VaiTro")?.Value;

	return Ok("Data for authorized user");
}
```

### Role-based Authorization

```csharp
[Authorize(Roles = "Admin")]
[HttpDelete("api/PhongHoc/{id}")]
public IActionResult DeletePhongHoc(int id)
{
	// Chỉ Admin có thể xóa
	return Ok("Deleted");
}
```

---

## 📝 AuthService Logic

### LoginAsync Method

```csharp
public async Task<LoginResponse> LoginAsync(LoginRequest request)
{
	// 1. Validate input
	if (string.IsNullOrWhiteSpace(request.TenDangNhap))
		return Error("EMPTY_CREDENTIALS", "Vui lòng nhập tên đăng nhập");

	// 2. Tìm tài khoản
	var user = await _context.TAIKHOAN
		.Include(t => t.VaiTro)
		.FirstOrDefaultAsync(u => u.TenDangNhap == request.TenDangNhap);

	if (user == null)
		return Error("INVALID_CREDENTIALS", "Tên đăng nhập hoặc mật khẩu không đúng");

	// 3. Verify password (BCrypt)
	if (!VerifyPassword(request.MatKhau, user.MatKhau))
		return Error("INVALID_CREDENTIALS", "Tên đăng nhập hoặc mật khẩu không đúng");

	// 4. Validate TrangThai
	switch (user.TrangThai?.ToLower())
	{
		case "chờ duyệt":
			return Error("ACCOUNT_PENDING", "Tài khoản của bạn đang chờ phê duyệt.");
		case "khóa":
			return Error("ACCOUNT_LOCKED", "Tài khoản đã bị khóa.");
		case "từ chối":
			return Error("ACCOUNT_REJECTED", "Yêu cầu đăng ký tài khoản bị từ chối.");
		case "hoạt động":
			break;
		default:
			return Error("INVALID_STATUS", "Tài khoản không ở trạng thái hợp lệ.");
	}

	// 5. Tạo JWT Token
	var token = _tokenService.GenerateToken(user);

	// 6. Trả về response
	return Success(token, user);
}
```

---

## 📊 Error Codes

| Error Code | HTTP | Mô Tả |
|-----------|------|-------|
| `EMPTY_CREDENTIALS` | 400 | Tên đăng nhập hoặc mật khẩu trống |
| `INVALID_CREDENTIALS` | 401 | Tên đăng nhập/mật khẩu sai |
| `ACCOUNT_PENDING` | 401 | Tài khoản chờ phê duyệt |
| `ACCOUNT_LOCKED` | 401 | Tài khoản bị khóa |
| `ACCOUNT_REJECTED` | 401 | Đăng ký bị từ chối |
| `INVALID_STATUS` | 401 | Trạng thái không hợp lệ |
| `USERNAME_EXISTS` | 400 | Tên đăng nhập đã tồn tại |
| `EMAIL_EXISTS` | 400 | Email đã được sử dụng |
| `PASSWORD_MISMATCH` | 400 | Mật khẩu không khớp |
| `WEAK_PASSWORD` | 400 | Mật khẩu quá yếu |
| `INCOMPLETE_DATA` | 400 | Dữ liệu không đầy đủ |
| `DATABASE_ERROR` | 500 | Lỗi cơ sở dữ liệu |
| `LOGIN_ERROR` | 500 | Lỗi đăng nhập |
| `REGISTER_ERROR` | 500 | Lỗi đăng ký |

---

## 🧪 Testing APIs

### Sử dụng curl

**Login:**
```bash
curl -X POST http://localhost:5266/api/Account/Login \
  -H "Content-Type: application/json" \
  -d '{
	"tenDangNhap": "admin",
	"matKhau": "password123"
  }'
```

**Protected endpoint:**
```bash
curl -X GET http://localhost:5266/api/PhongHoc \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE"
```

### Sử dụng Postman

1. **Request Type:** POST
2. **URL:** `http://localhost:5266/api/Account/Login`
3. **Headers:** `Content-Type: application/json`
4. **Body:**
```json
{
  "tenDangNhap": "admin",
  "matKhau": "password123"
}
```

---

## 🔄 Migration & Database Setup

### Tạo Migration

```bash
dotnet ef migrations add AddPasswordHashingAndStatus
dotnet ef database update
```

### SQL Script (Nếu cần)

```sql
-- Cập nhật cột MatKhau để hỗ trợ hash BCrypt
ALTER TABLE TAIKHOAN 
ALTER COLUMN MatKhau VARCHAR(255);

-- Cập nhật mật khẩu cũ thành hash (example)
UPDATE TAIKHOAN 
SET MatKhau = '$2a$12$...' 
WHERE TenDangNhap = 'admin';
```

---

## ⚙️ Configuration (Program.cs)

```csharp
// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(
				Encoding.UTF8.GetBytes(jwtSettings["SecretKey"])),
			ValidateIssuer = true,
			ValidIssuer = jwtSettings["Issuer"],
			ValidateAudience = true,
			ValidAudience = jwtSettings["Audience"],
			ValidateLifetime = true,
			ClockSkew = TimeSpan.Zero
		};
	});

// Đăng ký Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
```

---

## 📋 Checklist & Security Best Practices

- [ ] Change JWT SecretKey trong Production
- [ ] Enable HTTPS
- [ ] Thêm Rate Limiting để chống brute-force
- [ ] Implement Email Verification
- [ ] Thêm 2FA (Two-Factor Authentication)
- [ ] Implement Password Reset
- [ ] Audit logging
- [ ] IP Whitelist (nếu cần)
- [ ] API Throttling
- [ ] Update libraries regularly

---

## 📞 Troubleshooting

### Token không hợp lệ
- Kiểm tra SecretKey trong appsettings.json
- Kiểm tra thời gian expiry
- Kiểm tra format: `Bearer {token}`

### Password verification thất bại
- Kiểm tra mật khẩu cũ không phải hash BCrypt
- Cần update tất cả mật khẩu cũ

### TrangThai validation không hoạt động
- Kiểm tra giá trị TrangThai trong database
- Kiểm tra case-sensitivity

---

## 📚 Tài liệu Tham Khảo

- [JWT.io](https://jwt.io)
- [BCrypt.Net](https://github.com/BcryptNet/bcrypt.net)
- [Microsoft JWT Bearer](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.jwtbearer)
- [ASP.NET Core Security](https://learn.microsoft.com/en-us/aspnet/core/security)

---

**Version**: 1.0  
**Last Updated**: 2024  
**Status**: ✅ Production Ready
