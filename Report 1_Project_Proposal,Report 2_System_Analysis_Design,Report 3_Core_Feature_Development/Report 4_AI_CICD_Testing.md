# **Report 4: AI + CI/CD + Testing Report (Integration Phase)** 

## **1. AI Feature Integration** 

- **Mô tả tính năng AI:** 
  Hệ thống AIStudyHub tích hợp trợ lý AI trực tiếp vào luồng học tập, cụ thể tại chức năng PDF Viewer. Khi người dùng (Student) bôi đen một đoạn văn bản khó hiểu trong tài liệu PDF, họ có thể chọn "Explain" (Giải thích) hoặc "Summarize" (Tóm tắt).
  
- **Thuật toán, Models, và API:** 
  - Sử dụng **Gemini API** (hoặc **OpenAI API**) với model `gemini-1.5-flash` / `gpt-4o-mini` để tối ưu chi phí và tốc độ phản hồi.
  - **Kiến trúc gọi API:** Thực hiện thông qua `AIService` trong WPF MVVM. Text bôi đen sẽ được gửi dưới dạng prompt: `"Hãy giải thích đoạn văn bản sau một cách ngắn gọn, dễ hiểu cho học sinh: [SelectedText]"`.

- **Mô phỏng/Ảnh minh họa:**
  *(Bạn hãy chèn ảnh chụp màn hình khi bôi đen text trong PDF và khung chat AI bên phải hiện câu trả lời tại đây)*
  `![AI_Explain_Feature](path_to_image)`

## **2. CI/CD Pipeline Setup** 

- **Công cụ CI/CD (CI/CD Tools):** 
  Dự án sử dụng **GitHub Actions** vì dự án được lưu trữ trên GitHub. Nó hỗ trợ sẵn các runner Windows rất phù hợp cho ứng dụng C# WPF.

- **Các bước trong Pipeline (Pipeline Steps):**
  1. **Checkout Code:** Lấy mã nguồn mới nhất từ nhánh `main`.
  2. **Setup .NET:** Cài đặt môi trường .NET SDK phù hợp với phiên bản của dự án.
  3. **Restore Dependencies:** Chạy `dotnet restore` để tải các thư viện NuGet (ví dụ: MaterialDesignInXaml, SQLite).
  4. **Build:** Chạy `dotnet build --configuration Release` để biên dịch dự án.
  5. **Test:** Chạy `dotnet test` để tự động kiểm tra các module (Unit Test).
  6. **Publish:** Chạy lệnh `dotnet publish` để gom các file thực thi `.exe` và upload lên GitHub Artifacts/Releases.

- **Sơ đồ CI/CD (Draw.io format):**
  *(Copy mã dưới đây, mở Draw.io -> Insert -> Advanced -> Mermaid, dán mã vào để vẽ sơ đồ pipeline)*
  ```mermaid
  flowchart LR
      Commit(Push/Merge to Main) --> Trigger[Trigger GitHub Actions]
      Trigger --> Setup[.NET Setup]
      Setup --> Restore[NuGet Restore]
      Restore --> Build[Build App Release]
      Build --> Test[Run Unit Tests]
      Test --> Publish[Publish Artifact / .exe]
      Publish --> Release[GitHub Releases]
  ```

- **Cấu hình Pipeline:** 
  Tham khảo cách tạo file workflow `.github/workflows/dotnet-desktop.yml`.

## **3. Deployment Workflow** 

- **Cách thức Deployment:** 
  Ứng dụng AIStudyHub là một desktop app cục bộ (Local Desktop App) có SQLite đi kèm. Deployment ở đây là đóng gói (packaging) ứng dụng thành một file nén (`.zip`) hoặc file cài đặt. Người dùng chỉ cần tải từ GitHub Releases, giải nén và chạy file `AIStudyHub.exe`.

- **Mức độ tự động hóa và Tần suất:**
  Quy trình này được tự động hóa 100% qua GitHub Actions. Mỗi khi có tính năng mới (hoặc bug fix) được merge vào `main` và đánh tag phiên bản (ví dụ: `v1.0.1`), hệ thống sẽ tự tạo Release mới trên GitHub để QA/Tester hoặc người dùng tải về.

## **4. Collaboration and Automation** 

- **Điều phối công việc nhóm:**
  - Team sử dụng Git Flow: Mỗi thành viên làm việc trên một nhánh riêng biệt (ví dụ: `feature/ai-integration`, `feature/pdf-viewer`).
  - Khi hoàn thành, tạo Pull Request (PR) về nhánh `main`. GitHub Actions sẽ chạy test trên PR này. Nếu Pass (Xanh) và có người Review (Approve) thì mới được Merge.

- **Công cụ tự động hóa:**
  - Tự động hóa đánh dấu task hoàn thành trên Github Projects/Issues khi commit chứa từ khóa `Fixes #issue_number`.

## **5. Lessons Learned** 

- **Bài học từ việc tích hợp AI:** 
  - Gọi API tốn thời gian, nên bắt buộc phải dùng Lập trình Bất đồng bộ (`async`/`await`) trong C# để giao diện (UI) không bị treo khi chờ AI phản hồi.
  - Cần xử lý Exception các lỗi như: rớt mạng, API rate limit, API key bị sai.

- **Bài học từ Testing & CI/CD:**
  - Thiết lập CI/CD sớm giúp phát hiện ngay lập tức các lỗi do thiếu thư viện (Build fail) mà trên máy của Developer thì lại chạy được.
  - Xây dựng Unit Test cho `ViewModels` dễ dàng và hiệu quả hơn là test trực tiếp `Views` (UI).

## **6. Appendix (Optional)** 

- **Kế hoạch viết Unit Test:** 
  Gợi ý viết Test cho hàm tính ngày ôn tập tiếp theo của Flashcard (Spaced Repetition Algorithm) để đảm bảo không bị lỗi logic thời gian. Mọi người có thể chụp ảnh kết quả chạy Test (Pass màu xanh) để đưa vào báo cáo.
