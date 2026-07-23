# Sơ đồ Tổ chức & Phân chia Vai trò (Team Roles & Responsibilities)

**Dự án:** AI Study Hub
**Nền tảng:** C# WPF (MVVM)
**Số lượng thành viên:** 6 người
**Tiêu chí:** Chia đều khối lượng công việc (mỗi người đều phải code, đều phải báo cáo) để đảm bảo công bằng và mọi người đều có phần để thuyết trình lúc bảo vệ đồ án.

---

## 1. Cấu trúc Đội hình (Team Structure)

Với kiến trúc MVVM, mỗi thành viên sẽ "sở hữu" (own) một tính năng riêng biệt từ đầu đến cuối (từ View, ViewModel xuống Database). Sự chia việc như sau:

### 👤 Thành viên 1: Project Manager (PM) kiêm Database Developer (Phong)
* **Vai trò:** Quản lý dự án & Kiến trúc dữ liệu nền tảng.
* **Nhiệm vụ:**
  * Quản lý tiến độ trên Trello/Jira, nhắc nhở deadline của team. Tổng hợp các file tài liệu (PRD, ERD).
  * **Code:** Khởi tạo dự án C# WPF, setup cấu trúc thư mục MVVM.
  * **Code:** Cấu hình SQLite và Entity Framework Core. Viết tính năng **Quản lý Môn học (Subjects)** (Thêm, Sửa, Xóa).

### 👤 Thành viên 2: UI/UX Lead kiêm Frontend XAML Coder (Vương)
* **Vai trò:** "Bảo kê" toàn bộ về mặt Thẩm mỹ & Trải nghiệm người dùng.
* **Nhiệm vụ:**
  * Thiết kế bản vẽ Wireframe/Mockup (Figma) cho toàn bộ màn hình App.
  * **Code:** Thiết lập thư viện `MaterialDesignInXaml`. Tạo các `ResourceDictionary` chứa màu sắc, font chữ dùng chung.
  * **Code:** Dựng khung App gốc (Navigation bar, thanh Menu, Header) để các thành viên khác "ráp" module của họ vào.

### 👤 Thành viên 3: Module Quản lý Deadline & Lịch thi  (Hòa)
* **Vai trò:** Lập trình viên xử lý Logic Quản lý thời gian.
* **Nhiệm vụ:**
  * **Code:** Xây dựng màn hình danh sách Task/Deadline (Kanban board hoặc DataGrid).
  * **Code:** Viết ViewModel xử lý thêm/sửa/xóa Task.
  * **Code:** Xử lý logic tính toán đếm ngược ngày (Countdown timer) và bộ lọc (Filter) ưu tiên Deadline sắp đến hạn.

### 👤 Thành viên 4: Module Quản lý Tài liệu & PDF Viewer(Minh)
* **Vai trò:** Chuyên gia xử lý File và Trình đọc PDF.
* **Nhiệm vụ:**
  * **Code:** Viết logic hệ thống Upload file PDF, Word vào App (Copy file từ máy vào thư mục App Data, lưu đường dẫn vào Database).
  * **Code:** Nghiên cứu và tích hợp thư viện `PdfiumViewer` (hoặc tương đương) để mở file PDF ngay bên trong App WPF. Xử lý phân trang, Zoom in/Zoom out.

### 👤 Thành viên 5: Module AI Assistant & API Integration(Tân)
* **Vai trò:** Người tích hợp Trí tuệ nhân tạo (Ngòi nổ của App).
* **Nhiệm vụ:**
  * **Code:** Viết các Class HTTP Service để gọi API tới OpenAI (ChatGPT) hoặc Google (Gemini).
  * **Code:** Dựng giao diện Khung Chatbot Sidebar bên cạnh tài liệu.
  * **Code:** Bắt sự kiện người dùng bôi đen văn bản trong PDF -> Click chuột phải -> Gửi văn bản đó cho AI dịch hoặc giải thích thuật ngữ.

### 👤 Thành viên 6: Module Ghi chú (Annotation) & Flashcard (Duy)
* **Vai trò:** Lập trình viên phát triển tính năng Học tập sâu (Active Learning).
* **Nhiệm vụ:**
  * **Code:** Viết module lưu ghi chú. Lưu  xuống Database.
  * **Code:** Xây dựng module Flashcard (Thẻ ghi nhớ). Sinh viên có thể lật thẻ ôn tập.
  * **Code:** Viết thuật toán Spaced Repetition (Lặp lại ngắt quãng) đơn giản để tính toán `NextReviewDate` của các thẻ.

---

## 2. Kế hoạch triển khai chéo (Giao việc theo Sprint)

Để tránh tình trạng "người làm, người chơi", tiến độ được chia đều trong từng giai đoạn:

### 🚀 Giai đoạn 1: Khởi tạo & Dựng móng (Tuần 1 - Tuần 2)
* **TV 1:** Setup DB, code xong CRUD Môn học. Viết tài liệu Requirement.
* **TV 2:** Vẽ xong Figma, code xong Main Window (cái vỏ App).
* **TV 3:** Thiết kế bảng Task trong DB, code xong form thêm Task.
* **TV 4:** Chạy thử thành công thư viện PDF Viewer ở 1 project nháp (Test rủi ro).
* **TV 5:** Lấy API Key AI, code file console gọi API trả về kết quả thành công.
* **TV 6:** Code xong màn hình danh sách Flashcard cơ bản.

### ⚙️ Giai đoạn 2: Ráp nối & Tính năng phức tạp (Tuần 3 - Tuần 5)
* **TV 1:** Đóng vai trò Review Code, ghép code của 5 người kia vào nhánh `main`. Viết báo cáo tiến độ.
* **TV 2:** Chỉnh CSS/XAML cho các màn hình do người khác code bị xấu/lệch layout.
* **TV 3:** Code xong thuật toán đếm ngược và nhắc nhở Deadline.
* **TV 4:** Đưa PDF Viewer lên giao diện chính thức, ráp với màn hình Quản lý tài liệu.
* **TV 5:** Ráp khung Chat AI vào màn hình đọc PDF, chạy luồng hỏi đáp thực tế.
* **TV 6:** Ráp tính năng vẽ Note đè lên View của PDF (Đây là phần khó, cần phối hợp với TV 4).

### 🏁 Giai đoạn 3: Testing & Đóng gói (Tuần 6)
* **Tất cả 6 người:** Cùng nhau Bug bash (bấm nát app để tìm lỗi của nhau).
* **TV 1 & TV 2:** Thiết kế Slide thuyết trình PowerPoint và chuẩn bị kịch bản Demo.
* **Các TV còn lại:** Sửa bug cuối cùng và xuất file cài đặt (.exe).
