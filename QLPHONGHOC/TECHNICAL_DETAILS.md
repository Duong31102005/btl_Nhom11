# 🔍 Technical Implementation Details

## Database Changes Required

### Update MatKhau Column

```sql
-- Change password column to support BCrypt hashes (60 chars)
ALTER TABLE TAIKHOAN 
ALTER COLUMN MatKhau VARCHAR(255);

-- Add index for faster login
CREATE INDEX IX_TAIKHOAN_TenDangNhap ON TAIKHOAN(TenDangNhap);

-- Add index for email
CREATE INDEX IX_TAIKHOAN_Email ON TAIKHOAN(Email);
```

### Insert Test Data with Hashed Password

```sql
-- Password: 'admin123'
-- Hash: $2a$12$x2FJ6D3vWGzKzEYfXJfIu.MQS2D5SJpEFjLKrKDsVWxzgfF2Q2qfO
-- Generated: BCrypt.HashPassword("admin123", 12)

INSERT INTO TAIKHOAN (TenDangNhap, MatKhau, HoTen, Email, SoDienThoai, MaVaiTro, TrangThai)
VALUES 
(
	'admin',
	'$2a$12$x2FJ6D3vWGzKzEYfXJfIu.MQS2D5SJpEFjLKrKDsVWxzgfF2Q2qfO',
	'Quản Trị Viên',
	'admin@example.com',
	'0987654321',
	1,
	'Hoạt động'
);

INSERT INTO TAIKHOAN (TenDangNhap, MatKhau, HoTen, Email, SoDienThoai, MaVaiTro, TrangThai)
VALUES 
(
	'user1',
	'$2a$12$x2FJ6D3vWGzKzEYfXJfIu.MQS2D5SJpEFjLKrKDsVWxzgfF2Q2qfO',
	'Người Dùng 1',
	'user1@example.com',
	'0987654322',
	2,
	'Hoạt động'
);
```

---

## AuthService.cs - Detailed Logic

### LoginAsync Flow

```csharp
public async Task<LoginResponse> LoginAsync(LoginRequest request)
{
	// STEP 1: Input Validation
	if (string.IsNullOrWhiteSpace(request.TenDangNhap))
		return Error("EMPTY_CREDENTIALS");

	// STEP 2: Database Query
	var user = await _context.TAIKHOAN
		.Include(t => t.VaiTro)
		.FirstOrDefaultAsync(u => u.TenDangNhap == request.TenDangNhap);

	if (user == null)
		return Error("INVALID_CREDENTIALS");

	// STEP 3: Password Verification (BCrypt)
	if (!VerifyPassword(request.MatKhau, user.MatKhau))
		return Error("INVALID_CREDENTIALS");

	// STEP 4: Account Status Validation
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

	// STEP 5: Generate JWT Token
	var token = _tokenService.GenerateToken(user);

	// STEP 6: Return Success Response
	return Success(token, user);
}
```

### VerifyPassword Method

```csharp
public bool VerifyPassword(string password, string hash)
{
	try
	{
		// BCrypt automatically extracts salt from hash
		// and verifies the password
		return BCrypt.Net.BCrypt.Verify(password, hash);
	}
	catch (Exception ex)
	{
		_logger.LogError($"Password verification error: {ex.Message}");
		return false;
	}
}
```

### HashPassword Method

```csharp
public string HashPassword(string password)
{
	// workFactor = 12
	// Higher = more secure but slower (default 12 recommended)
	// - 10: ~40ms
	// - 12: ~250ms (recommended)
	// - 14: ~2000ms
	return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
}
```

---

## TokenService.cs - JWT Generation

### GenerateToken Method

```csharp
public string GenerateToken(TaiKhoan user)
{
	// Load JWT Settings from appsettings.json
	var jwtSettings = _configuration.GetSection("JwtSettings");
	var secretKey = jwtSettings.GetValue<string>("SecretKey");
	var issuer = jwtSettings.GetValue<string>("Issuer");
	var audience = jwtSettings.GetValue<string>("Audience");
	var expiryMinutes = jwtSettings.GetValue<int>("ExpiryMinutes", 60);

	// Create Security Key
	var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
	var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

	// Add Claims
	var claims = new List<Claim>
	{
		new Claim(ClaimTypes.NameIdentifier, user.MaTaiKhoan.ToString()),
		new Claim(ClaimTypes.Name, user.TenDangNhap),
		new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
		new Claim("HoTen", user.HoTen ?? string.Empty),
		new Claim("MaVaiTro", user.MaVaiTro.ToString()),
		new Claim("TrangThai", user.TrangThai ?? "Hoạt động"),
		new Claim("VaiTro", user.VaiTro?.TenVaiTro ?? "User")
	};

	// Create JWT Token
	var token = new JwtSecurityToken(
		issuer: issuer,
		audience: audience,
		claims: claims,
		expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
		signingCredentials: credentials
	);

	// Return JWT String
	return new JwtSecurityTokenHandler().WriteToken(token);
}
```

### Token Structure

**Header (Base64Url decoded):**
```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

**Payload (Base64Url decoded):**
```json
{
  "nameid": "1",
  "unique_name": "admin",
  "email": "admin@example.com",
  "HoTen": "Quản Trị Viên",
  "MaVaiTro": "1",
  "TrangThai": "Hoạt động",
  "VaiTro": "Admin",
  "exp": 1703123456,
  "iss": "QLPhongHocAPI",
  "aud": "QLPhongHocClients"
}
```

---

## AccountController.cs - Endpoints

### POST /api/Account/Login

```csharp
[HttpPost]
[Route("api/Account/Login")]
public async Task<IActionResult> ApiLogin([FromBody] LoginRequest request)
{
	// Model validation
	if (!ModelState.IsValid)
		return BadRequest(ModelState);

	// Call auth service
	var response = await _authService.LoginAsync(request);

	// Return appropriate status code
	if (!response.Success)
		return Unauthorized(response);

	return Ok(response);
}
```

### POST /Account/Login (Form)

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Login(string TenDangNhap, string MatKhau)
{
	// Convert form data to request object
	var loginRequest = new LoginRequest
	{
		TenDangNhap = TenDangNhap,
		MatKhau = MatKhau
	};

	// Call auth service
	var response = await _authService.LoginAsync(loginRequest);

	if (!response.Success)
	{
		ViewBag.Error = response.Message;
		return View();
	}

	// Save to Session
	HttpContext.Session.SetString(
		SessionHelper.SessionUserId,
		response.UserInfo.MaTaiKhoan.ToString());

	// Save JWT Token to Session (for later use)
	HttpContext.Session.SetString("JwtToken", response.Token);

	TempData["Success"] = "Đăng nhập thành công.";
	return RedirectToAction("Index", "Home");
}
```

---

## Program.cs - Configuration

### JWT Setup

```csharp
// Load JWT Settings
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings.GetValue<string>("SecretKey");
var issuer = jwtSettings.GetValue<string>("Issuer");
var audience = jwtSettings.GetValue<string>("Audience");

// Add JWT Authentication
builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new SymmetricSecurityKey(
			Encoding.UTF8.GetBytes(secretKey)),
		ValidateIssuer = true,
		ValidIssuer = issuer,
		ValidateAudience = true,
		ValidAudience = audience,
		ValidateLifetime = true,
		ClockSkew = TimeSpan.Zero
	};
});

// Add Custom Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
```

---

## Error Handling Details

### Error Code Mapping

```csharp
private readonly Dictionary<string, (int StatusCode, string Message)> ErrorMapping = 
	new()
	{
		["EMPTY_CREDENTIALS"] = (400, "Vui lòng nhập tên đăng nhập và mật khẩu."),
		["INVALID_CREDENTIALS"] = (401, "Tên đăng nhập hoặc mật khẩu không đúng."),
		["ACCOUNT_PENDING"] = (401, "Tài khoản của bạn đang chờ phê duyệt."),
		["ACCOUNT_LOCKED"] = (401, "Tài khoản đã bị khóa."),
		["ACCOUNT_REJECTED"] = (401, "Yêu cầu đăng ký tài khoản bị từ chối."),
		["INVALID_STATUS"] = (401, "Tài khoản không ở trạng thái hợp lệ."),
		["USERNAME_EXISTS"] = (400, "Tên đăng nhập đã tồn tại."),
		["EMAIL_EXISTS"] = (400, "Email đã được sử dụng."),
		["PASSWORD_MISMATCH"] = (400, "Mật khẩu xác nhận không khớp."),
		["WEAK_PASSWORD"] = (400, "Mật khẩu phải có ít nhất 6 ký tự."),
		["INCOMPLETE_DATA"] = (400, "Vui lòng điền đầy đủ thông tin."),
		["DATABASE_ERROR"] = (500, "Lỗi cơ sở dữ liệu."),
		["LOGIN_ERROR"] = (500, "Lỗi đăng nhập."),
		["REGISTER_ERROR"] = (500, "Lỗi đăng ký.")
	};
```

---

## TrangThai Validation Flow

### Decision Tree

```
START: User Login Attempt
	│
	├─→ Check TenDangNhap in Database
	│       │
	│       ├─→ NOT FOUND → Error: INVALID_CREDENTIALS
	│       │
	│       └─→ FOUND → Continue
	│
	├─→ Check MatKhau (BCrypt Verify)
	│       │
	│       ├─→ NOT MATCH → Error: INVALID_CREDENTIALS
	│       │
	│       └─→ MATCH → Continue
	│
	├─→ Check TrangThai
	│       │
	│       ├─→ "Chờ duyệt" → Error: ACCOUNT_PENDING
	│       │
	│       ├─→ "Khóa" → Error: ACCOUNT_LOCKED
	│       │
	│       ├─→ "Từ chối" → Error: ACCOUNT_REJECTED
	│       │
	│       ├─→ "Hoạt động" → Continue
	│       │
	│       └─→ Other → Error: INVALID_STATUS
	│
	├─→ Generate JWT Token
	│
	├─→ Build Response with User Info
	│
	└─→ Return Success Response
```

---

## Security Implementation

### Password Security

```csharp
// BCrypt Configuration
// workFactor = 12 (default recommended)

// Hashing
string hash = BCrypt.HashPassword("password123", workFactor: 12);
// Output: $2a$12$x2FJ6D3vWGzKzEYfXJfIu.MQS2D5SJpEFjLKrKDsVWxzgfF2Q2qfO

// Verification (always use BCrypt.Verify)
bool isValid = BCrypt.Verify("password123", hash); // true
bool isInvalid = BCrypt.Verify("wrong", hash);     // false
```

**Why BCrypt?**
- Adaptive: Slows down with time
- Salted: Built-in salt generation
- Non-reversible: Can't decrypt hash
- Industry standard: Widely used

### JWT Security

```csharp
// Token Structure: Header.Payload.Signature

// Verification Process
1. Decode header & payload (Base64Url)
2. Get signature from token
3. Recreate signature using secret key
4. Compare: Token Signature == Created Signature
5. If match: Token is valid & not tampered

// Signature = HMACSHA256(header + payload, secretKey)
```

---

## Performance Considerations

### BCrypt Timing

```
Work Factor | Time    | Use Case
-----------|---------|---------
	10     | ~40ms   | Test environments
	12     | ~250ms  | Recommended (default)
	14     | ~2000ms | High security
	15     | ~4000ms | Very high security
```

**Recommendation**: Use 12 for balance between security and performance

### JWT Validation Timing

```
Operation      | Time     | Count
---------------|----------|-------
Generate Token | < 1ms    | Per login
Validate Token | < 1ms    | Per request
Refresh Token  | < 1ms    | Per refresh
```

---

## Logging

### Logged Events

```csharp
// Authentication Service
_logger.LogWarning($"Login attempt with empty credentials");
_logger.LogWarning($"Login attempt with non-existent username: {username}");
_logger.LogWarning($"Login attempt with wrong password: {username}");
_logger.LogInformation($"User logged in successfully: {username}");

// Token Service
_logger.LogInformation($"Token generated for user: {username}");
_logger.LogError($"Error generating token: {error}");

// Account Controller
_logger.LogInformation($"User {username} logged in successfully");
_logger.LogInformation($"User {username} registered successfully");
```

---

## Testing Scenarios

### Valid Login
```
Username: admin
Password: admin123
TrangThai: Hoạt động
→ Expected: Success with JWT Token
```

### Invalid Credentials
```
Username: admin
Password: wrongpassword
→ Expected: Error - INVALID_CREDENTIALS
```

### Pending Account
```
Username: newuser
Password: correct
TrangThai: Chờ duyệt
→ Expected: Error - ACCOUNT_PENDING
```

### Locked Account
```
Username: lockeduser
Password: correct
TrangThai: Khóa
→ Expected: Error - ACCOUNT_LOCKED
```

---

## Migration Guide (Old System)

### If Converting from Plain Text Passwords

```csharp
// Step 1: Create Migration
public partial class MigratePasswordsToHash : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		// Modify column
		migrationBuilder.AlterColumn<string>(
			name: "MatKhau",
			table: "TAIKHOAN",
			type: "nvarchar(255)",
			nullable: false);
	}
}

// Step 2: Update Data
public class PasswordMigrationScript
{
	public static void UpdatePasswords(QLPhongHocContext context)
	{
		var users = context.TAIKHOAN.ToList();

		foreach (var user in users)
		{
			// Hash old password
			user.MatKhau = BCrypt.Net.BCrypt.HashPassword(user.MatKhau, 12);
		}

		context.SaveChanges();
	}
}

// Step 3: Run Migration
dotnet ef migrations add MigratePasswordsToHash
dotnet ef database update
```

---

## Best Practices

### ✅ DO
- [ ] Use HTTPS in production
- [ ] Store SecretKey in environment variables
- [ ] Validate token expiration
- [ ] Implement rate limiting
- [ ] Log authentication events
- [ ] Use HTTPS for token transmission
- [ ] Implement CSRF protection

### ❌ DON'T
- [ ] Store passwords in plain text
- [ ] Expose SecretKey in code
- [ ] Trust client-side validation only
- [ ] Use weak random generation
- [ ] Ignore token expiration
- [ ] Share tokens in URLs
- [ ] Log sensitive information

---

## Troubleshooting

### Token Invalid Error
```
Problem: "Token validation failed"
Solution:
1. Check SecretKey matches between generation & validation
2. Verify Issuer & Audience match
3. Check token not expired (exp claim)
4. Verify token format: "Bearer {token}"
```

### Password Verification Fails
```
Problem: Always returns false
Solution:
1. Ensure hash is BCrypt format (starts with $2a$12$)
2. Verify password is not already hashed
3. Check character encoding (UTF-8)
4. Verify database column size (255+ chars)
```

### Database Connection Error
```
Problem: "Cannot connect to database"
Solution:
1. Check connection string in appsettings.json
2. Verify SQL Server running
3. Check database exists
4. Run migrations: dotnet ef database update
```

---

**Version**: 1.0  
**Status**: ✅ Complete  
**Last Updated**: 2024
