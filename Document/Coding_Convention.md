# Coding Convention - AI Study Hub

Tài liệu này quy định các chuẩn mực viết code và quản lý mã nguồn (source code) cho dự án AI Study Hub, giúp team 6 người làm việc đồng nhất, tránh xung đột và dễ dàng bảo trì.

## 1. Quy tắc Đặt tên (Naming Conventions) trong C#

*   **Class, Record, Struct, Method, Property, Event:** Sử dụng `PascalCase`.
    *   *Ví dụ:* `UserService`, `CalculateScore()`, `TotalCredits`.
*   **Interface:** Bắt đầu bằng chữ `I` và theo sau là `PascalCase`.
    *   *Ví dụ:* `IAuthService`, `IDocumentParser`.
*   **Tham số (Parameters) & Biến cục bộ (Local variables):** Sử dụng `camelCase`.
    *   *Ví dụ:* `userId`, `documentName`.
*   **Private fields (Biến toàn cục private trong class):** Bắt đầu bằng dấu gạch dưới `_` và theo sau là `camelCase`.
    *   *Ví dụ:* `_databaseContext`, `_httpClient`.
*   **Hằng số (Constants):** Sử dụng `PascalCase` (Không dùng UPPER_SNAKE_CASE trong C# theo chuẩn Microsoft).
    *   *Ví dụ:* `public const int MaxRetries = 3;`

## 2. Quy tắc XAML & WPF

*   **Tên Control (`x:Name`):** Chỉ đặt tên cho những control thực sự cần tương tác trong file Code-Behind. Sử dụng hậu tố hoặc tiền tố chỉ định loại control (tùy team thống nhất, ví dụ: `txtUsername`, `btnSubmit`).
*   **Data Binding:** Luôn ưu tiên sử dụng cơ chế Data Binding của WPF để liên kết giao diện với ViewModel. Hạn chế tối đa việc dùng `x:Name` để `TênControl.Text = "..."` cập nhật UI thủ công trong code-behind.
*   **Resource Dictionary:** Gom nhóm màu sắc (Colors), font chữ, style (Button, TextBox) ra một thư mục `Themes` hoặc `Styles` riêng để tái sử dụng toàn cục, không code style cứng vào từng thành phần một.

## 3. Kiến trúc MVVM (Cực kỳ quan trọng)

*   **Cấm tuyệt đối:** KHÔNG viết logic nghiệp vụ (gọi Database, gọi API, tính toán thuật toán) vào file Code-Behind (`.xaml.cs`). Code-behind chỉ dùng cho UI Logic (như hiệu ứng animation, drag window, tính toán chiều cao giao diện).
*   **Views:** Thư mục chứa các file giao diện `.xaml`. Views chỉ chịu trách nhiệm hiển thị và Binding data.
*   **ViewModels:** Nơi chứa toàn bộ logic xử lý sự kiện và dữ liệu cho màn hình đó. ViewModels tuyệt đối không được tham chiếu trực tiếp đến Views (Trong file ViewModel không được `using System.Windows.Controls`). Giao tiếp với View thông qua `ICommand` (nút bấm) và `INotifyPropertyChanged` (cập nhật UI).
*   **Models:** Các class đại diện cho cấu trúc dữ liệu (thực thể Entity Framework, DTO).

## 4. Git & Workflow cho Team 6 người

*   **Branching (Luồng chia nhánh):**
    *   `main`: Code hoàn chỉnh, ít bug nhất, dùng để nộp bài/release. Không ai push code thẳng lên `main`.
    *   `develop`: Nhánh gom code của cả team. Mọi người pull từ đây và tạo Pull Request (PR) merge vào đây.
    *   `feature/[tên-tính-năng]`: Nhánh dùng để làm chức năng mới. Bắt nguồn từ `develop`. (VD: `feature/login`, `feature/pdf-viewer`).
    *   `bugfix/[tên-bug]`: Dùng để sửa lỗi.
*   **Commit Message Format:** Rõ ràng, cho biết commit này làm gì.
    *   `[Feature] Thêm màn hình Quản lý môn học`
    *   `[Bugfix] Sửa lỗi crash khi upload PDF quá 5MB`
    *   `[Refactor] Tối ưu hóa truy vấn SQLite lấy danh sách Flashcard`

## 5. Comment & Documentation

*   Thêm XML Comments `/// <summary>` cho các Interface, Class quan trọng ở mức Service/Repository và các hàm có logic tính toán phức tạp.
*   Không comment những thứ hiển nhiên. (VD: Hàm `GetUsers()` không cần comment "Hàm này dùng để lấy danh sách users").
*   Thay vào đó, hãy comment "TẠI SAO lại code như vậy" (Why) ở những đoạn logic lạ để các thành viên khác đọc hiểu lý do (VD: `// Phải trừ đi 1 vì mảng PDF bắt đầu từ index 0`).
