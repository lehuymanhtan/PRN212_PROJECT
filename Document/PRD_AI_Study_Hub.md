# Tài liệu Đặc tả Yêu cầu Sản phẩm (Product Requirements Document - PRD)

**Tên dự án:** Trợ lý Học tập & Quản lý Tài liệu Số (AI Study Hub)
**Nền tảng:** Windows Desktop Application 
**Ngôn ngữ/Framework:** C# / WPF (Windows Presentation Foundation)
**Mô hình Kiến trúc:** MVVM (Model-View-ViewModel)
**Số lượng thành viên:** 6 người

---

## 1. Tổng quan dự án (Project Overview)
- **Vấn đề:** Sinh viên hiện nay gặp khó khăn trong việc quản lý khối lượng lớn tài liệu học tập (PDF, Slide bài giảng), dễ quên deadline, và mất nhiều thời gian để tra cứu, đọc hiểu hoặc tóm tắt các tài liệu tĩnh dài dòng.
- **Giải pháp:** "AI Study Hub" là một phần mềm Desktop All-in-One. Nó không chỉ giúp quản lý môn học, lịch thi, deadline một cách khoa học mà còn tích hợp trình đọc PDF thông minh. Đặc biệt, ứng dụng sử dụng AI (như ChatGPT hoặc Gemini) để hỗ trợ học tập trực tiếp ngay trên tài liệu (giải thích thuật ngữ, tóm tắt, dịch thuật).

## 2. Đối tượng Người dùng (Target Audience)
- Học sinh, Sinh viên đại học.
- Người tự học, nghiên cứu sinh thường xuyên phải xử lý và ghi chú trên lượng lớn tài liệu số.

---

## 3. Tính năng cốt lõi (Core Features / MVP)

### 3.1. Không gian Quản lý Học tập (Study Dashboard)
- **Quản lý Môn học (Subjects):** Hỗ trợ thêm, sửa, xóa môn học (bao gồm Tên, mã môn, tên giảng viên). Mỗi môn học đóng vai trò như một Workspace riêng biệt.
- **Quản lý Deadline/Task & Lịch thi:** Thêm công việc, bài tập, lịch thi gắn với từng môn học cụ thể.
- **Hiển thị (View):** Hỗ trợ xem dưới dạng Danh sách (List), Bảng Kanban (To-do, Doing, Done) hoặc Lịch (Calendar View). Có bộ đếm ngược (Countdown timer) cho các deadline/bài thi quan trọng.

### 3.2. Quản lý Kho Tài liệu Số (Document Management)
- **Import/Lưu trữ:** Upload file tài liệu (chủ yếu là PDF) vào hệ thống theo từng thư mục môn học.
- **Phân loại & Tìm kiếm:** Cho phép gắn tag (thẻ) cho tài liệu và tìm kiếm nhanh tài liệu theo tên.

### 3.3. Tương tác PDF (Smart PDF Viewer)
- **Trình đọc tích hợp:** Mở, xem và điều hướng file PDF trực tiếp trong phần mềm mà không cần dùng app bên ngoài.
- **Công cụ tương tác:** Hỗ trợ Highlight (tô sáng) đoạn văn bản, thêm Sticky Notes (Ghi chú) dán trực tiếp lên các trang PDF.

### 3.4. Trợ lý AI Thông minh (AI Smart Assistant - Điểm nhấn)
- **Tra cứu nhanh theo ngữ cảnh (Contextual Lookup):** Người dùng bôi đen một đoạn văn bản trong PDF -> Click chuột phải (hoặc phím tắt) -> Chọn các lệnh AI: *Dịch sang tiếng Việt*, *Tóm tắt đoạn này*, hoặc *Giải thích thuật ngữ*.
- **Chat với Tài liệu:** Tích hợp một Sidebar khung chat AI bên cạnh màn hình PDF. Trợ lý có thể trò chuyện và trả lời các câu hỏi dựa trên nội dung tài liệu đang mở.

### 3.5. Hệ thống Ôn tập Thông minh (Flashcards & Spaced Repetition)
- **Tạo Flashcard tự động/thủ công:** Tích hợp AI để tự động sinh các Flashcard (câu hỏi - đáp án) từ nội dung tài liệu/bài học, hoặc cho phép người dùng tự tạo thẻ học tập riêng.
- **Lặp lại ngắt quãng (Spaced Repetition):** Áp dụng thuật toán tối ưu hóa trí nhớ (tương tự Anki) để sắp xếp lịch ôn tập thẻ. Hệ thống sẽ tự động tính toán và nhắc nhở ôn lại thẻ dựa vào mức độ ghi nhớ của người dùng.

---

## 4. Yêu cầu Kỹ thuật & Công nghệ (Tech Stack)
- **Ngôn ngữ lập trình:** C# (Khuyến nghị dùng .NET 8).
- **UI Framework:** WPF kết hợp các thư viện giao diện như `MaterialDesignInXamlToolkit` để có UI hiện đại.
- **Cơ sở dữ liệu (Database):** `SQLite` kết hợp `Entity Framework Core` (Code-first). Phù hợp làm app Desktop cá nhân vì không cần cài cắm server phức tạp.
- **Xử lý PDF:** Các thư viện như `PdfiumViewer`, `Syncfusion.PdfViewer.WPF`, hoặc `MuPDF`.
- **AI Integration:** Sử dụng `HttpClient` gọi REST API đến OpenAI (ChatGPT) hoặc Google (Gemini API).

---

## 5. Đề xuất Phân chia Giai đoạn (Project Roadmap)

- **Giai đoạn 1 (Thiết lập nền tảng):** Phân tích thiết kế, lên sơ đồ Database (SQLite), dựng giao diện WPF cơ bản (Navigation bar, Main Dashboard). Hoàn thành tính năng CRUD cho Môn học & Deadline.
- **Giai đoạn 2 (Kho tài liệu):** Hoàn thiện module tải và quản lý file PDF. Tích hợp thư viện PDF Viewer để mở được tài liệu trên giao diện app.
- **Giai đoạn 3 (Tương tác):** Cài đặt tính năng Highlight và ghi chú (Note) thủ công trên PDF và lưu lại trạng thái.
- **Giai đoạn 4 (Tích hợp AI & Flashcard):** Kết nối API, làm tính năng bôi đen text -> gọi AI trả về kết quả. Xây dựng tính năng tạo và ôn tập Flashcard (Spaced Repetition).
- **Giai đoạn 5 (Hoàn thiện & Đóng gói):** Thực hiện Testing, Fix bug, tinh chỉnh UX/UI và đóng gói phần mềm (.exe).
