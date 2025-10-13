// wwwroot/js/login.js

document.addEventListener('DOMContentLoaded', () => {
    // ================================
    // 🔧 CẤU HÌNH API
    // ================================
    const API_BASE_URL = "http://localhost:5257";
    const API_LOGIN_ENDPOINT = `${API_BASE_URL}/api/Auth/login`;
    const HOME_PAGE_URL = 'home.html';

    // ================================
    // 📌 LẤY ELEMENT TỪ HTML
    // ================================
    const form = document.getElementById('loginForm');
    const studentIdInput = document.getElementById('studentId');
    const passwordInput = document.getElementById('password');
    const errorMessageDiv = document.getElementById('errorMessage');
    const loginButton = form ? form.querySelector('button[type="submit"]') : null;

    if (!form || !studentIdInput || !passwordInput || !errorMessageDiv || !loginButton) {
        console.error("❌ Không tìm thấy các phần tử cần thiết trong Login form.");
        return;
    }

    // ================================
    // 📌 HÀM HIỂN THỊ THÔNG BÁO
    // ================================
    function displayMessage(message, isError = false) {
        errorMessageDiv.innerHTML = message;
        errorMessageDiv.style.color = isError ? '#e53e3e' : '#38a169';
        errorMessageDiv.style.fontWeight = 'bold';
    }

    // ================================
    // 📌 XỬ LÝ ĐĂNG NHẬP
    // ================================
    form.addEventListener('submit', async function (event) {
        event.preventDefault();
        displayMessage('');

        const studentId = studentIdInput.value.trim();
        const password = passwordInput.value;

        if (!studentId || !password) {
            displayMessage('Vui lòng nhập đầy đủ Mã sinh viên và Mật khẩu.', true);
            return;
        }

        // DTO gửi lên API (PascalCase để match với C#)
        const loginDto = {
            StudentId: studentId,
            Password: password
        };

        // Vô hiệu hoá nút
        loginButton.disabled = true;
        loginButton.textContent = 'Đang đăng nhập...';

        try {
            const response = await fetch(API_LOGIN_ENDPOINT, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify(loginDto),
            });

            let data = {};
            try {
                if (response.headers.get("content-length") !== "0" && response.status !== 500) {
                    data = await response.json();
                }
            } catch (e) {
                console.warn(`⚠️ Cảnh báo: Server trả về non-JSON hoặc phản hồi trống. Status: ${response.status}. Lỗi: ${e.message}`);
            }

            if (response.ok && data.success) {
                // ✅ Đăng nhập thành công
                const token = data.token;
                localStorage.setItem('authToken', token);
                displayMessage('Đăng nhập thành công! Đang chuyển hướng...', false);

                setTimeout(() => {
                    window.location.href = HOME_PAGE_URL;
                }, 500);

            } else if (response.status === 500) {
                displayMessage('Lỗi 500: Server gặp sự cố nội bộ. Kiểm tra Backend C#.', true);
                console.error("❌ Backend Internal Server Error.");

            } else {
                const errorMessage = data.message || "Mã sinh viên hoặc mật khẩu không chính xác.";
                displayMessage(`Đăng nhập thất bại: ${errorMessage}`, true);
                console.error("❌ Chi tiết lỗi Backend:", data);
            }

        } catch (error) {
            console.error('❌ Lỗi khi gọi API đăng nhập:', error);
            displayMessage('LỖI KẾT NỐI: Không thể liên lạc với máy chủ API. Đảm bảo Backend C# đang chạy tại http://localhost:5257', true);

        } finally {
            loginButton.disabled = false;
            loginButton.textContent = 'ĐĂNG NHẬP';
        }
    });

    // ================================
    // 📌 CHUYỂN HƯỚNG QUÊN MẬT KHẨU
    // ================================
    const forgotPasswordLink = document.getElementById('forgotPasswordLink');
    if (forgotPasswordLink) {
        forgotPasswordLink.addEventListener('click', (e) => {
            e.preventDefault();
            window.location.href = 'ForgotPassword.html';
        });
    }
});
