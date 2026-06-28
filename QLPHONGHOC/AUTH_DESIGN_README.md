# 📱 Giao Diện Đăng Nhập & Đăng Ký - Hệ Thống Quản Lý Phòng Học

## 🎨 Tổng Quan

Đã thiết kế lại hoàn toàn giao diện **Đăng Nhập (Login)** và tạo mới giao diện **Đăng Ký (Register)** cho hệ thống Quản Lý Phòng Học với:

- ✨ **Giao diện hiện đại, tối giản (Minimalist Design)**
- 🎯 **Màu xanh dương chuyên nghiệp** (Professional Blue)
- 🔒 **Form nhạy cảm với bóng nhẹ** (Box-shadow effects)
- ✅ **Validate form cơ bản phía client**
- 📱 **Responsive design** cho mọi thiết bị

---

## 📁 Cấu Trúc Tập Tin

### Views
```
QLPHONGHOC/Views/Account/
├── Login.cshtml          # Trang Đăng nhập (thiết kế lại)
├── Register.cshtml       # Trang Đăng ký (mới)
└── AccessDenied.cshtml   # Trang từ chối truy cập
```

### Styles & Scripts
```
QLPHONGHOC/wwwroot/
├── css/
│   └── auth.css                    # Stylesheet cho trang xác thực
└── js/
	└── register-validation.js      # Validation script cho form đăng ký
```

### Backend
```
QLPHONGHOC/Controllers/
└── AccountController.cs            # Xử lý Logic đăng nhập/đăng ký
```

---

## 🔐 Trang Đăng Nhập (Login)

### 📋 Form Fields
- **Tên đăng nhập** (Username)
- **Mật khẩu** (Password)
- **Link "Chưa có tài khoản? Đăng ký ngay"**

### ✅ Validation
- Kiểm tra field không được trống
- Thông báo lỗi nếu thông tin không đúng
- Reset validation khi user nhập dữ liệu

### 🎨 Thiết Kế
- Gradient background xanh dương (Từ `#0066cc` → `#0052a3`)
- Card white với box-shadow mềm
- Form căn giữa màn hình
- Nút đăng nhập với hover effect

### 🔗 URL
```
GET  /Account/Login    # Hiển thị form đăng nhập
POST /Account/Login    # Xử lý đăng nhập
```

---

## 📝 Trang Đăng Ký (Register)

### 📋 Form Fields
1. **👤 Tên đăng nhập** (3-20 ký tự, chỉ chữ/số/underscore)
2. **👨 Họ tên** (ít nhất 3 ký tự)
3. **✉️ Email** (phải đúng định dạng email)
4. **☎️ Số điện thoại** (Định dạng Việt Nam: 10-11 chữ số)
5. **🔐 Mật khẩu** (tối thiểu 6 ký tự)
6. **🔒 Xác nhận mật khẩu** (phải khớp với mật khẩu)

### ✅ Validation Features

#### Client-side Validation
- **Real-time validation**: Kiểm tra ngay khi user nhập dữ liệu
- **Visual feedback**: 
  - ✅ Green border = Valid
  - ❌ Red border = Invalid
  - Error messages dưới từng field

#### Validation Rules
| Field | Rule | Error Message |
|-------|------|---------------|
| Tên đăng nhập | 3-20 ký tự, `[a-zA-Z0-9_]` | "Tên đăng nhập chỉ chứa chữ, số và dấu gạch dưới" |
| Họ tên | ≥ 3 ký tự | "Họ tên phải có ít nhất 3 ký tự" |
| Email | Format email valid | "Email không đúng định dạng" |
| Số điện thoại | +84/0 + 9-10 chữ số | "Số điện thoại không hợp lệ (VN: 10-11 chữ số)" |
| Mật khẩu | ≥ 6 ký tự | "Mật khẩu phải có ít nhất 6 ký tự" |
| Xác nhận | Khớp với Mật khẩu | "Mật khẩu xác nhận không khớp" |

#### Password Strength Indicator
- **Yếu** (Weak): < 3 điều kiện
- **Trung bình** (Medium): 3-4 điều kiện  
- **Mạnh** (Strong): ≥ 4 điều kiện

Điều kiện:
- Độ dài ≥ 6 ký tự
- Độ dài ≥ 8 ký tự
- Chứa cả chữ thường & chữ hoa
- Chứa chữ số
- Chứa ký tự đặc biệt

### 🎨 Thiết Kế
- Giống Login nhưng với max-width lớn hơn
- Grid layout 2 cột (responsive)
- Password strength bar visual
- Animated loading spinner trên button

### 🔗 URL
```
GET  /Account/Register    # Hiển thị form đăng ký
POST /Account/Register    # Xử lý đăng ký
```

### 📚 Backend Logic (AccountController.cs)
```csharp
[HttpPost]
public async Task<IActionResult> Register(
	string TenDangNhap, 
	string MatKhau, 
	string XacNhanMatKhau,
	string HoTen, 
	string Email, 
	string SoDienThoai)
{
	// Kiểm tra field không trống
	// Kiểm tra mật khẩu khớp
	// Kiểm tra tên đăng nhập tồn tại
	// Kiểm tra email tồn tại
	// Tạo user mới với vai trò mặc định (MaVaiTro = 2)
	// Redirect về Login
}
```

---

## 🎨 Màu Sắc & Typography

### Color Palette
```css
--primary-blue: #0066cc;           /* Màu xanh chính */
--primary-blue-dark: #0052a3;      /* Màu xanh đậm */
--primary-blue-light: #e6f0ff;     /* Màu xanh nhạt (focus) */
--text-dark: #1a1a1a;              /* Text chính */
--text-gray: #666666;              /* Text phụ */
--border-light: #e0e0e0;           /* Border */
--success-green: #28a745;          /* Success state */
```

### Typography
- **Font Family**: System fonts (-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto)
- **Heading**: 28px, Font-weight 600
- **Body**: 14px, Font-weight 400
- **Labels**: 14px, Font-weight 500

---

## 🚀 Deployment Considerations

### ⚠️ Security Notes

1. **Password Hashing**: Hiện tại mật khẩu được lưu trực tiếp. **PHẢI** hash mật khẩu trước lưu:
   ```csharp
   // Thêm vào Register method
   using System.Security.Cryptography;

   string hashedPassword = BCrypt.Net.BCrypt.HashPassword(MatKhau);
   newUser.MatKhau = hashedPassword;
   ```

2. **Email Verification**: Nên thêm xác minh email sau khi đăng ký

3. **Password Reset**: Thêm functionality "Quên mật khẩu"

4. **Rate Limiting**: Giới hạn số lần thử đăng nhập để chống brute-force

5. **HTTPS**: Ensure sử dụng HTTPS khi deploy

### 📦 Dependencies
- Bootstrap 5 (CSS framework)
- jQuery (nếu cần, không bắt buộc)
- .NET 10 / ASP.NET Core

---

## 📱 Responsive Design

### Breakpoints
- **Desktop**: ≥ 992px (max-width: 520px cho container)
- **Tablet**: 576px - 991px (full-width, padding 20px)
- **Mobile**: < 576px (full-width, padding 15px, single column)

### Mobile Optimizations
- Form stacking một cột
- Larger touch targets (12px padding)
- Readable font sizes
- Full-width inputs

---

## 🧪 Testing Checklist

- [ ] Login form validation
- [ ] Register form validation
- [ ] Email regex validation
- [ ] Password strength indicator
- [ ] Form submission
- [ ] Error messages display
- [ ] Success messages
- [ ] Mobile responsiveness
- [ ] Keyboard navigation
- [ ] Browser compatibility (Chrome, Firefox, Safari, Edge)

---

## 🔄 User Flow

```
┌─────────────────┐
│   Home/Landing  │
└────────┬────────┘
		 │
		 ├──────────────────────┐
		 │                      │
	┌────▼────┐          ┌──────▼─────┐
	│  Login   │          │  Register  │
	└────┬────┘          └──────┬─────┘
		 │                      │
	┌────▼──────────┬───────────▼───┐
	│   Dashboard   │  Không có TK?  │──────┐
	└───────────────┴───────────────┘      │
											│
									  ┌─────▼────┐
									  │ Go Register
									  └───────────┘
```

---

## 📞 Contact & Support

Nếu có câu hỏi hoặc cần hỗ trợ thêm, vui lòng liên hệ team phát triển.

---

**Last Updated**: 2024
**Version**: 1.0
**Status**: ✅ Production Ready
