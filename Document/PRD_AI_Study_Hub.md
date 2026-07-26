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
- **Tra cứu nhanh theo ngữ cảnh (Contextual Lookup):** Người dùng bôi đen một đoạn văn bản trong PDF -> Click chuột phải (hoặc phím tắt) -> Chọn các lệnh AI: *Dịch sang tiếng Việt*, *Tóm tắt đoạn này*, hoặc *Giải thích thuật ngữ*. Kết quả tra cứu sẽ được đẩy thẳng vào giao diện Chat Sidebar để người dùng tiện theo dõi và hỏi tiếp.
- **Chat với Tài liệu:** Tích hợp một Sidebar khung chat AI bên cạnh màn hình PDF. 
  - Lịch sử chat sẽ được lưu lại theo từng tài liệu (Mỗi file PDF có một luồng chat riêng biệt).
  - Áp dụng cơ chế **RAG (Retrieval-Augmented Generation)**: Khi người dùng thêm tài liệu vào phần mềm, hệ thống sẽ chạy ngầm xử lý trích xuất văn bản và tạo Index (chỉ mục). Khi người dùng đặt câu hỏi, AI sẽ tìm kiếm các đoạn văn bản liên quan nhất trong Index để trả lời, giải quyết vấn đề giới hạn Token của các file PDF dài.
- **AI Agent (Thực thi tự động - Tính năng Thử nghiệm):** 
  - Nâng cấp AI từ việc chỉ "trả lời tin nhắn" (Chatbot) thành một Trợ lý thực thụ (Agent) có khả năng tự động gọi các hàm trong ứng dụng (Function Calling) dựa trên yêu cầu của người dùng.
  - *Ví dụ:* Người dùng nhắn "Tạo cho mình 10 Flashcard từ chương này", AI sẽ tự phân tích và lưu thẳng thẻ vào Database; hoặc "Thêm lịch thi ngày mai", AI tự gọi hàm AddDeadline.
  - *(Lưu ý - Experimental): Đây là tính năng mở rộng (Nice-to-have). Nhóm sẽ ưu tiên hoàn thiện bản Chatbot cốt lõi trước, nếu còn thời gian sẽ tích hợp Agentic AI để làm điểm nhấn tuyệt đối, tránh rủi ro cháy tiến độ dự án.*
- **Xử lý ngoại tuyến (Offline State):** Tính năng AI yêu cầu kết nối mạng liên tục. Nếu mất mạng, Sidebar AI sẽ hiển thị thông báo "Offline" và tạm khóa các chức năng AI (chỉ cho phép đọc/quản lý PDF nội bộ).

### 3.5. Hệ thống Ôn tập Thông minh (Flashcards & Spaced Repetition)
- **Tạo Flashcard tự động/thủ công:** Tích hợp AI để tự động sinh các Flashcard (câu hỏi - đáp án) từ nội dung tài liệu/bài học, hoặc cho phép người dùng tự tạo thẻ học tập riêng.
- **Lặp lại ngắt quãng (Spaced Repetition):** Áp dụng thuật toán tối ưu hóa trí nhớ (tương tự Anki) để sắp xếp lịch ôn tập thẻ. Hệ thống sẽ tự động tính toán và nhắc nhở ôn lại thẻ dựa vào mức độ ghi nhớ của người dùng.

---

## 4. Yêu cầu Kỹ thuật & Công nghệ (Tech Stack)
- **Ngôn ngữ lập trình:** C# (Khuyến nghị dùng .NET 8).
- **UI Framework:** WPF kết hợp các thư viện giao diện như `MaterialDesignInXamlToolkit` để có UI hiện đại.
- **Cơ sở dữ liệu (Database):** `SQLite` kết hợp `Entity Framework Core` (Code-first). Phù hợp làm app Desktop cá nhân vì không cần cài cắm server phức tạp.
- **Xử lý PDF:** Các thư viện như `PdfiumViewer`, `Syncfusion.PdfViewer.WPF`, hoặc `MuPDF` (Hiện tại đang sử dụng tích hợp mặc định của WebView2).
- **AI Integration:** Sử dụng `HttpClient` gọi REST API đến OpenAI (ChatGPT) hoặc Google (Gemini API). Cần thiết kế một trang **Settings (Cài đặt)** để người dùng tự nhập và lưu API Key cá nhân của họ.

---

## 5. Đề xuất Phân chia Giai đoạn (Project Roadmap)

- **Giai đoạn 1 (Thiết lập nền tảng):** Phân tích thiết kế, lên sơ đồ Database (SQLite), dựng giao diện WPF cơ bản (Navigation bar, Main Dashboard). Hoàn thành tính năng CRUD cho Môn học & Deadline.
- **Giai đoạn 2 (Kho tài liệu):** Hoàn thiện module tải và quản lý file PDF. Tích hợp thư viện PDF Viewer để mở được tài liệu trên giao diện app.
- **Giai đoạn 3 (Tương tác):** Cài đặt tính năng Highlight và ghi chú (Note) thủ công trên PDF và lưu lại trạng thái.
- **Giai đoạn 4 (Tích hợp AI & Flashcard):** Kết nối API, làm tính năng bôi đen text -> gọi AI trả về kết quả. Xây dựng tính năng tạo và ôn tập Flashcard (Spaced Repetition).
- **Giai đoạn 5 (Hoàn thiện & Đóng gói):** Thực hiện Testing, Fix bug, tinh chỉnh UX/UI và đóng gói phần mềm (.exe).
