# Sơ đồ Thực thể Liên kết (ERD) - AI Study Hub

Để đảm bảo hệ thống không bị "phá vỡ" (breaking changes) khi mở rộng trong tương lai (đặc biệt là khi nâng cấp các tính năng AI, Flashcard và Spaced Repetition), cơ sở dữ liệu cần được thiết kế với tính chuẩn hóa cao và hỗ trợ các trường linh hoạt.

Dưới đây là sơ đồ ERD được thiết kế cực kỳ chi tiết, đáp ứng cả MVP hiện tại và Vision tương lai.

## 1. Sơ đồ ERD (Mermaid Code)

Bạn có thể xem trực tiếp sơ đồ này bằng cách dùng tính năng **Markdown Preview** của Visual Studio Code (phím tắt `Ctrl + Shift + V`).

```mermaid
erDiagram
    USER ||--o{ SUBJECT : "creates"
    SUBJECT ||--o{ DOCUMENT : "owns"
    SUBJECT ||--o{ TASK : "has deadlines"
    SUBJECT ||--o{ FLASHCARD_DECK : "owns"
    
    DOCUMENT ||--o{ ANNOTATION : "has highlights/notes"
    DOCUMENT ||--o{ CHAT_SESSION : "context for"
    DOCUMENT ||--o{ DOCUMENT_TAG : "tagged with"
    
    TAG ||--o{ DOCUMENT_TAG : "tags"
    
    FLASHCARD_DECK ||--o{ FLASHCARD : "contains"
    CHAT_SESSION ||--o{ CHAT_MESSAGE : "contains"

    USER {
        string Id PK "Khóa chính (GUID)"
        string Username
        string PasswordHash
        datetime CreatedAt
    }

    SUBJECT {
        string Id PK "GUID"
        string UserId FK
        string Name "Tên môn (VD: Toán rời rạc)"
        string CourseCode "Mã môn (VD: MAD101)"
        int Credits "Số tín chỉ"
        string ColorHex "Màu hiển thị UI (VD: #FF0000)"
    }

    DOCUMENT {
        string Id PK "GUID"
        string SubjectId FK
        string Title "Tên hiển thị"
        string FilePath "Đường dẫn file (Local path)"
        string FileType "PDF, DOCX"
        datetime UploadedAt
    }

    TASK {
        string Id PK "GUID"
        string SubjectId FK
        string Title "Tên công việc/kỳ thi"
        datetime DueDate "Hạn chót"
        string Status "Todo, InProgress, Done"
        string Type "Assignment, Exam, Review"
    }

    ANNOTATION {
        string Id PK "GUID"
        string DocumentId FK
        int PageNumber "Trang số mấy"
        float PosX "Tọa độ X"
        float PosY "Tọa độ Y"
        string Content "Text đã highlight hoặc Note"
        string Type "Highlight, Note"
    }

    FLASHCARD_DECK {
        string Id PK "GUID"
        string SubjectId FK
        string DocumentId FK "Nullable (Nếu AI tạo từ 1 doc)"
        string Title
    }

    FLASHCARD {
        string Id PK "GUID"
        string DeckId FK
        string FrontText "Mặt trước (Câu hỏi)"
        string BackText "Mặt sau (Đáp án)"
        datetime NextReviewDate "Dùng cho Spaced Repetition"
        int RepetitionCount "Số lần đã ôn"
    }

    CHAT_SESSION {
        string Id PK "GUID"
        string DocumentId FK "Nullable (Context của đoạn chat)"
        string Title
        datetime CreatedAt
    }

    CHAT_MESSAGE {
        string Id PK "GUID"
        string SessionId FK
        string Sender "User hoặc AI"
        string Content "Nội dung chat"
        datetime Timestamp
    }

    TAG {
        string Id PK "GUID"
        string Name
    }

    DOCUMENT_TAG {
        string DocumentId PK, FK
        string TagId PK, FK
    }
```

## 2. Giải thích thiết kế (Design Rationale) để chống "Gãy" trong tương lai

1. **Khóa chính là chuỗi GUID (uniqueidentifier):** 
   - Tất cả khóa chính đều dùng kiểu `string` (chứa GUID) thay vì số nguyên `int` tự tăng (Auto Increment). 
   - **Lý do:** Lợi ích lớn nhất là nếu sau này bạn làm thêm app Mobile và muốn đồng bộ data (Sync) giữa PC, Mobile và Cloud, GUID sẽ không bao giờ bị trùng lặp khóa chính ở các thiết bị khác nhau.
2. **Bảng `USER`:** 
   - Dù ứng dụng hiện tại là Desktop App (lưu Local bằng SQLite), nhưng thiết kế sẵn bảng User giúp bạn dễ dàng nâng cấp lên tính năng hỗ trợ nhiều profile trên cùng 1 máy tính hoặc **Cloud Sync** (Đồng bộ đám mây) sau này.
3. **Bảng `ANNOTATION` (Cực kỳ quan trọng cho PDF Viewer):** 
   - Lưu trữ chi tiết `PageNumber`, `PosX`, `PosY`. 
   - **Lý do:** Việc này cho phép app vẽ lại (render) chính xác vị trí người dùng đã highlight hoặc gắn sticky note mỗi lần mở file, mà không làm hỏng cấu trúc file PDF gốc (Non-destructive editing).
4. **Thuật toán Spaced Repetition cho `FLASHCARD`:** 
   - Thay vì chỉ lưu Câu hỏi - Trả lời, bảng Flashcard có trường `NextReviewDate` (Ngày ôn tập tiếp theo) và `RepetitionCount`. 
   - **Lý do:** Điều này dọn đường sẵn cho tính năng "Lặp lại ngắt quãng" (tương tự thuật toán của app Anki) - một tính năng must-have cho app hỗ trợ học tập, giúp AI tự tính toán và nhắc sinh viên ôn bài đúng thời điểm sắp quên.
5. **Bảng `CHAT_SESSION` & `CHAT_MESSAGE`:** 
   - Thiết kế chuẩn theo cấu trúc API của OpenAI (ChatGPT). Chia theo Session (phiên chat) giúp người dùng lưu lại lịch sử hội thoại thay vì hỏi xong là mất. 
   - `DocumentId` (Nullable) cho phép AI biết phiên chat này có đang bám sát vào nội dung của một file PDF cụ thể nào không (Đây chính là kiến trúc RAG - Retrieval-Augmented Generation).
