// Register Form Validation Script

const registerForm = document.getElementById('registerForm');
const registerBtn = document.getElementById('registerBtn');
const tenDangNhapInput = document.getElementById('tenDangNhap');
const hoTenInput = document.getElementById('hoTen');
const emailInput = document.getElementById('email');
const soDienThoaiInput = document.getElementById('soDienThoai');
const matKhauInput = document.getElementById('matKhau');
const xacNhanMatKhauInput = document.getElementById('xacNhanMatKhau');

// Email regex pattern
const emailPattern = new RegExp("^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$");
// Phone regex for Vietnam: starts with +84 or 0, followed by 9-10 digits
const phonePattern = new RegExp("^(\\+84|0)[0-9]{9,10}$");
// Username pattern: letters, numbers, underscore
const usernamePattern = new RegExp("^[a-zA-Z0-9_]+$");

// Helper: Show error
function showError(input, message) {
    input.classList.add('is-invalid');
    input.classList.remove('is-valid');
    const errorDiv = input.closest('.form-group').querySelector('.error-message');
    if (errorDiv) {
        errorDiv.textContent = message;
        errorDiv.classList.add('show');
    }
}

// Helper: Show success
function showSuccess(input) {
    input.classList.remove('is-invalid');
    input.classList.add('is-valid');
    const errorDiv = input.closest('.form-group').querySelector('.error-message');
    if (errorDiv) {
        errorDiv.classList.remove('show');
    }
}

// Validate Tên đăng nhập
function validateTenDangNhap() {
    const value = tenDangNhapInput.value.trim();
    if (!value) {
        showError(tenDangNhapInput, 'Vui lòng nhập tên đăng nhập');
        return false;
    }
    if (value.length < 3) {
        showError(tenDangNhapInput, 'Tên đăng nhập phải từ 3-20 ký tự');
        return false;
    }
    if (!usernamePattern.test(value)) {
        showError(tenDangNhapInput, 'Tên đăng nhập chỉ chứa chữ, số và dấu gạch dưới');
        return false;
    }
    showSuccess(tenDangNhapInput);
    return true;
}

// Validate Họ tên
function validateHoTen() {
    const value = hoTenInput.value.trim();
    if (!value) {
        showError(hoTenInput, 'Vui lòng nhập họ và tên');
        return false;
    }
    if (value.length < 3) {
        showError(hoTenInput, 'Họ tên phải có ít nhất 3 ký tự');
        return false;
    }
    showSuccess(hoTenInput);
    return true;
}

// Validate Email
function validateEmail() {
    const value = emailInput.value.trim();
    if (!value) {
        showError(emailInput, 'Vui lòng nhập email');
        return false;
    }
    if (!emailPattern.test(value)) {
        showError(emailInput, 'Email không đúng định dạng');
        return false;
    }
    showSuccess(emailInput);
    return true;
}

// Validate Số điện thoại
function validateSoDienThoai() {
    const value = soDienThoaiInput.value.trim();
    if (!value) {
        showError(soDienThoaiInput, 'Vui lòng nhập số điện thoại');
        return false;
    }
    if (!phonePattern.test(value)) {
        showError(soDienThoaiInput, 'Số điện thoại không hợp lệ (VN: 10-11 chữ số)');
        return false;
    }
    showSuccess(soDienThoaiInput);
    return true;
}

// Check password strength
function checkPasswordStrength(password) {
    let strength = 0;
    if (password.length >= 6) strength++;
    if (password.length >= 8) strength++;
    if (/[a-z]/.test(password) && /[A-Z]/.test(password)) strength++;
    if (/[0-9]/.test(password)) strength++;
    if (/[^a-zA-Z0-9]/.test(password)) strength++;

    const strengthBar = matKhauInput.closest('.form-group').querySelector('.password-strength-fill');
    const strengthLabel = matKhauInput.closest('.form-group').querySelector('.strength-label');
    const strengthDiv = matKhauInput.closest('.form-group').querySelector('.password-strength');

    strengthBar.classList.remove('weak', 'medium', 'strong');

    if (password.length === 0) {
        strengthDiv.style.display = 'none';
    } else {
        strengthDiv.style.display = 'block';
        if (strength < 3) {
            strengthBar.classList.add('weak');
            strengthLabel.textContent = 'Yếu';
        } else if (strength < 4) {
            strengthBar.classList.add('medium');
            strengthLabel.textContent = 'Trung bình';
        } else {
            strengthBar.classList.add('strong');
            strengthLabel.textContent = 'Mạnh';
        }
    }
}

// Validate Mật khẩu
function validateMatKhau() {
    const value = matKhauInput.value.trim();
    if (!value) {
        showError(matKhauInput, 'Vui lòng nhập mật khẩu');
        return false;
    }
    if (value.length < 6) {
        showError(matKhauInput, 'Mật khẩu phải có ít nhất 6 ký tự');
        return false;
    }
    showSuccess(matKhauInput);
    return true;
}

// Validate Xác nhận mật khẩu
function validateXacNhanMatKhau() {
    const value = xacNhanMatKhauInput.value.trim();
    if (!value) {
        showError(xacNhanMatKhauInput, 'Vui lòng xác nhận mật khẩu');
        return false;
    }
    if (value !== matKhauInput.value) {
        showError(xacNhanMatKhauInput, 'Mật khẩu xác nhận không khớp');
        return false;
    }
    showSuccess(xacNhanMatKhauInput);
    return true;
}

// Real-time validation
tenDangNhapInput.addEventListener('input', validateTenDangNhap);
hoTenInput.addEventListener('input', validateHoTen);
emailInput.addEventListener('input', validateEmail);
soDienThoaiInput.addEventListener('input', validateSoDienThoai);

matKhauInput.addEventListener('input', function() {
    checkPasswordStrength(this.value);
    validateMatKhau();
    // Revalidate xác nhận mật khẩu nếu đã nhập
    if (xacNhanMatKhauInput.value) {
        validateXacNhanMatKhau();
    }
});

xacNhanMatKhauInput.addEventListener('input', validateXacNhanMatKhau);

// Form submit
registerForm.addEventListener('submit', function(e) {
    e.preventDefault();

    const validations = [
        validateTenDangNhap(),
        validateHoTen(),
        validateEmail(),
        validateSoDienThoai(),
        validateMatKhau(),
        validateXacNhanMatKhau()
    ];

    if (validations.every(v => v)) {
        registerBtn.disabled = true;
        this.submit();
    }
});
