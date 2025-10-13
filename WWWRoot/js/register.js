document.addEventListener('DOMContentLoaded', () => {
    const API_BASE_URL = "http://localhost:5257";
    const API_REGISTER_ENDPOINT = `${API_BASE_URL}/api/Auth/register`;
    const API_CHECK_EMAIL_ENDPOINT = `${API_BASE_URL}/api/Auth/check-email`; // 👈 bạn sẽ tạo ở backend sau
    const LOGIN_PAGE_URL = 'login.html';

    const form = document.getElementById('register-form');
    const messageDiv = document.getElementById('response-message');

    const studentIdInput = document.getElementById('StudentId');
    const passwordInput = document.getElementById('Password');
    const fullNameInput = document.getElementById('FullName');
    const genderInput = document.getElementById('Gender');
    const dateOfBirthInput = document.getElementById('DateOfBirth');
    const courseYearInput = document.getElementById('CourseYear');
    const classNameInput = document.getElementById('ClassName');
    const emailInput = document.getElementById('Email'); // 🆕 Thêm email

    const registerButton = form ? form.querySelector('button[type="submit"]') : null;

    if (!form || !messageDiv || !registerButton) {
        console.error("Lỗi: Không tìm thấy các ID HTML cần thiết (form, messageDiv, hoặc registerButton).");
        return;
    }

    function displayMessage(message, isError = false) {
        messageDiv.innerHTML = message.replace(/\n/g, '<br>');
        messageDiv.style.color = isError ? 'red' : 'green';
        messageDiv.style.fontWeight = 'bold';
    }

    function clearMessages() {
        messageDiv.innerHTML = '';
    }

    // 👇 Hàm kiểm tra định dạng email đơn giản
    function isValidEmail(email) {
        // RFC 5322 pattern (tương đối chuẩn)
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    }

    // 👇 Hàm kiểm tra email có bị trùng không (gọi API)
    async function isEmailDuplicate(email) {
        try {
            const response = await fetch(`${API_CHECK_EMAIL_ENDPOINT}?email=${encodeURIComponent(email)}`);
            if (response.ok) {
                const result = await response.json();
                return result.exists === true;
            } else {
                console.warn("Không thể kiểm tra email, server trả lỗi:", response.status);
                return false; // fallback
            }
        } catch (err) {
            console.error("Lỗi mạng khi kiểm tra email:", err);
            return false;
        }
    }

    form.addEventListener('submit', async function (event) {
        event.preventDefault();
        clearMessages();

        const studentId = studentIdInput.value.trim();
        const password = passwordInput.value;
        const fullName = fullNameInput.value.trim();
        const gender = genderInput.value;
        const dateOfBirthString = dateOfBirthInput.value;
        const courseYearString = courseYearInput.value.trim();
        const className = classNameInput.value.trim();
        const email = emailInput.value.trim(); // 🆕

        // 🛑 Kiểm tra bắt buộc
        if (!studentId || !password || !fullName || !dateOfBirthString || !gender || !courseYearString || !className || !email) {
            displayMessage('Lỗi: Vui lòng điền đầy đủ tất cả thông tin.', true);
            return;
        }

        // 🛑 Kiểm tra định dạng email
        if (!isValidEmail(email)) {
            displayMessage('Lỗi: Địa chỉ email không hợp lệ.', true);
            return;
        }

        // 🛑 Kiểm tra mật khẩu
        if (password.length < 6) {
            displayMessage('Lỗi: Mật khẩu phải có ít nhất 6 ký tự.', true);
            return;
        }

        // 🛑 Kiểm tra email trùng
        const duplicate = await isEmailDuplicate(email);
        if (duplicate) {
            displayMessage('Lỗi: Email này đã được sử dụng cho tài khoản khác.', true);
            return;
        }

        // ✅ Format ngày sinh
        let dateOfBirthFormatted = `${dateOfBirthString}T00:00:00.000Z`;

        // 🧱 DTO gửi về Backend
        const studentRegistrationDto = {
            StudentId: studentId,
            Password: password,
            Fullname: fullName,
            Gender: gender,
            DateOfBirth: dateOfBirthFormatted,
            CourseYear: courseYearString,
            ClassName: className,
            Email: email, // 🆕 gửi kèm
        };

        registerButton.disabled = true;
        registerButton.textContent = 'Đang xử lý...';
        displayMessage('Đang kết nối đến hệ thống...');

        try {
            const response = await fetch(API_REGISTER_ENDPOINT, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify(studentRegistrationDto),
            });

            const data = response.status !== 204 ? await response.json().catch(() => ({})) : {};

            if (response.ok) {
                displayMessage('Đăng ký thành công! Đang chuyển hướng...');
                setTimeout(() => window.location.href = LOGIN_PAGE_URL, 2000);
            } else {
                displayMessage(data.message || `Đăng ký thất bại (Lỗi ${response.status})`, true);
            }
        } catch (err) {
            console.error(err);
            displayMessage('Lỗi kết nối mạng.', true);
        } finally {
            registerButton.disabled = false;
            registerButton.textContent = 'ĐĂNG KÝ';
        }
    });
});
