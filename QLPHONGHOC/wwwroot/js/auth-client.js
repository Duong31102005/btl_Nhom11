/**
 * Authentication API Client Example
 * Ví dụ sử dụng API xác thực từ Client-side (JavaScript)
 */

// ============================================
// 1. CONFIGURATION
// ============================================

const API_BASE_URL = 'http://localhost:5266';
const API_ENDPOINTS = {
    LOGIN: '/api/Account/Login',
    REGISTER: '/api/Account/Register',
    LOGOUT: '/Account/Logout'
};

let tokenStorage = {
    token: localStorage.getItem('jwtToken'),
    expiresAt: localStorage.getItem('tokenExpiresAt')
};

// ============================================
// 2. LOGIN FUNCTION
// ============================================

/**
 * Đăng nhập và lưu JWT token
 */
async function login(tenDangNhap, matKhau) {
    try {
        const response = await fetch(`${API_BASE_URL}${API_ENDPOINTS.LOGIN}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                tenDangNhap: tenDangNhap,
                matKhau: matKhau,
                ghiNho: false
            })
        });

        const data = await response.json();

        if (!response.ok) {
            throw new Error(data.message || 'Đăng nhập thất bại');
        }

        // Lưu token
        saveToken(data.token, data.expiresIn);

        console.log('✅ Đăng nhập thành công');
        console.log('User Info:', data.userInfo);

        return {
            success: true,
            userInfo: data.userInfo,
            token: data.token
        };

    } catch (error) {
        console.error('❌ Login error:', error.message);
        return {
            success: false,
            error: error.message
        };
    }
}

// ============================================
// 3. REGISTER FUNCTION
// ============================================

/**
 * Đăng ký tài khoản mới
 */
async function register(userData) {
    try {
        const response = await fetch(`${API_BASE_URL}${API_ENDPOINTS.REGISTER}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                tenDangNhap: userData.tenDangNhap,
                hoTen: userData.hoTen,
                email: userData.email,
                soDienThoai: userData.soDienThoai,
                matKhau: userData.matKhau,
                xacNhanMatKhau: userData.xacNhanMatKhau
            })
        });

        const data = await response.json();

        if (!response.ok) {
            throw new Error(data.message || 'Đăng ký thất bại');
        }

        console.log('✅ Đăng ký thành công. Mã tài khoản:', data.maTaiKhoan);

        return {
            success: true,
            message: data.message,
            maTaiKhoan: data.maTaiKhoan
        };

    } catch (error) {
        console.error('❌ Register error:', error.message);
        return {
            success: false,
            error: error.message
        };
    }
}

// ============================================
// 4. TOKEN MANAGEMENT
// ============================================

/**
 * Lưu JWT token vào localStorage
 */
function saveToken(token, expiresInSeconds) {
    localStorage.setItem('jwtToken', token);

    const expiresAt = new Date().getTime() + (expiresInSeconds * 1000);
    localStorage.setItem('tokenExpiresAt', expiresAt);

    tokenStorage.token = token;
    tokenStorage.expiresAt = expiresAt;
}

/**
 * Lấy JWT token từ localStorage
 */
function getToken() {
    const token = localStorage.getItem('jwtToken');
    const expiresAt = localStorage.getItem('tokenExpiresAt');

    // Kiểm tra token hết hạn
    if (expiresAt && new Date().getTime() > parseInt(expiresAt)) {
        clearToken();
        return null;
    }

    return token;
}

/**
 * Xóa JWT token
 */
function clearToken() {
    localStorage.removeItem('jwtToken');
    localStorage.removeItem('tokenExpiresAt');
    tokenStorage.token = null;
    tokenStorage.expiresAt = null;
}

/**
 * Kiểm tra token còn hạn không
 */
function isTokenValid() {
    const token = getToken();
    return token !== null;
}

/**
 * Lấy thời gian còn lại của token (tính bằng giây)
 */
function getTokenTimeRemaining() {
    const expiresAt = localStorage.getItem('tokenExpiresAt');
    if (!expiresAt) return 0;

    const remaining = parseInt(expiresAt) - new Date().getTime();
    return Math.max(0, Math.floor(remaining / 1000));
}

// ============================================
// 5. AUTHENTICATED API REQUESTS
// ============================================

/**
 * Gửi request với JWT token
 */
async function fetchWithAuth(url, options = {}) {
    const token = getToken();

    if (!token) {
        throw new Error('Token không tồn tại. Vui lòng đăng nhập.');
    }

    const headers = {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`,
        ...options.headers
    };

    const response = await fetch(url, {
        ...options,
        headers: headers
    });

    // Nếu token hết hạn (401)
    if (response.status === 401) {
        clearToken();
        window.location.href = '/Account/Login';
        throw new Error('Token hết hạn. Vui lòng đăng nhập lại.');
    }

    return response;
}

/**
 * GET request với authentication
 */
async function getWithAuth(endpoint) {
    try {
        const response = await fetchWithAuth(`${API_BASE_URL}${endpoint}`, {
            method: 'GET'
        });

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }

        return await response.json();
    } catch (error) {
        console.error('GET error:', error.message);
        throw error;
    }
}

/**
 * POST request với authentication
 */
async function postWithAuth(endpoint, data) {
    try {
        const response = await fetchWithAuth(`${API_BASE_URL}${endpoint}`, {
            method: 'POST',
            body: JSON.stringify(data)
        });

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }

        return await response.json();
    } catch (error) {
        console.error('POST error:', error.message);
        throw error;
    }
}

// ============================================
// 6. USAGE EXAMPLES
// ============================================

// Example 1: Đăng nhập
/*
(async () => {
    const result = await login('admin', 'password123');
    if (result.success) {
        console.log('Logged in:', result.userInfo);
    } else {
        console.log('Error:', result.error);
    }
})();
*/

// Example 2: Đăng ký
/*
(async () => {
    const result = await register({
        tenDangNhap: 'newuser',
        hoTen: 'Người Dùng Mới',
        email: 'newuser@example.com',
        soDienThoai: '0987654321',
        matKhau: 'password123',
        xacNhanMatKhau: 'password123'
    });
    if (result.success) {
        console.log('Registered:', result.message);
    } else {
        console.log('Error:', result.error);
    }
})();
*/

// Example 3: Gọi API được bảo vệ
/*
(async () => {
    try {
        const phongHoc = await getWithAuth('/api/PhongHoc');
        console.log('Phòng học:', phongHoc);
    } catch (error) {
        console.error('Error:', error.message);
    }
})();
*/

// ============================================
// 7. AUTO-LOGOUT BEFORE TOKEN EXPIRES
// ============================================

/**
 * Auto-logout 1 phút trước khi token hết hạn
 */
function setupAutoLogout() {
    setInterval(() => {
        const remaining = getTokenTimeRemaining();

        // Logout nếu còn 1 phút
        if (remaining > 0 && remaining <= 60) {
            console.warn('Token sẽ hết hạn trong 1 phút. Đang đăng xuất...');
            logout();
        }

        // Logout nếu hết hạn
        if (remaining === 0 && isTokenValid()) {
            console.warn('Token đã hết hạn. Đang đăng xuất...');
            logout();
        }
    }, 30000); // Check mỗi 30 giây
}

function logout() {
    clearToken();
    window.location.href = '/Account/Login';
}

// Khởi động auto-logout khi trang tải
document.addEventListener('DOMContentLoaded', setupAutoLogout);

// ============================================
// 8. DISPLAY USER INFO
// ============================================

/**
 * Hiển thị thông tin người dùng đã đăng nhập
 */
function displayUserInfo(userInfo) {
    const userElement = document.getElementById('user-info');
    if (userElement) {
        userElement.innerHTML = `
            <div class="user-profile">
                <p><strong>Tên:</strong> ${userInfo.hoTen}</p>
                <p><strong>Email:</strong> ${userInfo.email}</p>
                <p><strong>Vai trò:</strong> ${userInfo.tenVaiTro}</p>
                <p><strong>Trạng thái:</strong> ${userInfo.trangThai}</p>
            </div>
        `;
    }
}

// ============================================
// 9. ERROR HANDLING
// ============================================

/**
 * Xử lý lỗi API
 */
function handleApiError(errorCode, message) {
    const errorMessages = {
        'EMPTY_CREDENTIALS': 'Vui lòng nhập đầy đủ thông tin đăng nhập.',
        'INVALID_CREDENTIALS': 'Tên đăng nhập hoặc mật khẩu không đúng.',
        'ACCOUNT_PENDING': 'Tài khoản của bạn đang chờ phê duyệt.',
        'ACCOUNT_LOCKED': 'Tài khoản đã bị khóa.',
        'ACCOUNT_REJECTED': 'Yêu cầu đăng ký tài khoản bị từ chối.',
        'USERNAME_EXISTS': 'Tên đăng nhập đã tồn tại.',
        'EMAIL_EXISTS': 'Email đã được sử dụng.',
        'PASSWORD_MISMATCH': 'Mật khẩu không khớp.',
        'WEAK_PASSWORD': 'Mật khẩu quá yếu. Cần ít nhất 6 ký tự.',
        'INCOMPLETE_DATA': 'Vui lòng điền đầy đủ thông tin.',
        'DATABASE_ERROR': 'Lỗi cơ sở dữ liệu. Vui lòng thử lại.',
        'LOGIN_ERROR': 'Lỗi đăng nhập. Vui lòng thử lại.',
        'REGISTER_ERROR': 'Lỗi đăng ký. Vui lòng thử lại.'
    };

    return errorMessages[errorCode] || message;
}

// ============================================
// Export cho Module (nếu dùng bundler)
// ============================================

/*
export {
    login,
    register,
    logout,
    getToken,
    isTokenValid,
    getTokenTimeRemaining,
    getWithAuth,
    postWithAuth,
    displayUserInfo,
    handleApiError
};
*/
