# **Report 1: Project Proposal Report (Initiation Phase)** 

## **1. Project Title** 

- **AI Study Hub** 

## **2. Team Members and Roles** 

|No.|Student ID|Student Name|Role|
|---|---|---|---|
|**1.**|[To Be Filled]|Phong|Project Manager (PM) & Database Developer|
|**2.**|[To Be Filled]|Vương|UI/UX Lead & Frontend XAML Coder|
|**3.**|[To Be Filled]|Hòa|Module Quản lý Deadline & Lịch thi|
|**4.**|[To Be Filled]|Minh|Module Quản lý Tài liệu & PDF Viewer|
|**5.**|[To Be Filled]|Tân|Module AI Assistant & API Integration|
|**6.**|[To Be Filled]|Duy|Module Ghi chú (Annotation) & Flashcard|

## **3. Objectives and Expected Outcomes** 

- **Objectives:** 
  - Build a comprehensive desktop application using C# WPF and MVVM architecture to assist students in their learning process.
  - Provide a unified platform for managing deadlines, organizing study materials, reading documents (PDFs), and reviewing knowledge via flashcards.
  - Integrate Artificial Intelligence (ChatGPT/Gemini) directly into the study workflow to help explain terms, summarize content, and answer questions.

- **Expected Outcomes:** 
  - A fully functional Windows desktop application executable (.exe).
  - A robust document management and PDF reading experience built-in.
  - An intelligent AI assistant sidebar that interacts with the user's selected text in documents.
  - A task tracking system with countdown timers for exams and assignments.
  - An active learning module featuring flashcards with a spaced repetition algorithm.

## **4. Scope and Technical Requirements** 

- **Scope:** 
  - **Included:** Local file management, internal PDF viewer with annotation capabilities (highlight, sticky notes), AI chatbot integration, Kanban-style task management, flashcard creation and spaced repetition review.
  - **Excluded:** Mobile/Web versions, real-time multi-user collaboration (this is designed as a personal, local desktop application).

- **Technical Requirements:** 
  - **Platform:** Windows Desktop
  - **Framework/Language:** C# WPF, MVVM Architecture
  - **Database:** SQLite with Entity Framework Core
  - **UI/UX:** MaterialDesignInXaml toolkit
  - **External Integrations:** PdfiumViewer (or equivalent) for PDF rendering, OpenAI API / Google Gemini API for AI features.

## **5. Initial Implementation Plan** 

- **Phase 1: Khởi tạo & Dựng móng (Tuần 1 - Tuần 2)**
  - Setup database, complete CRUD for Subjects.
  - Design Figma wireframes and build the Main Window UI shell.
  - Design Task database tables and create the add task form.
  - Test PDF Viewer library in an isolated environment.
  - Secure AI API keys and test API connectivity.
  - Build basic UI for Flashcard list.

- **Phase 2: Ráp nối & Tính năng phức tạp (Tuần 3 - Tuần 5)**
  - Code review and integrate all modules into the `main` branch.
  - Polish UI/UX and fix layout issues across screens.
  - Implement countdown logic and deadline reminders.
  - Integrate PDF Viewer into the main application.
  - Connect AI Chat UI with the document viewer.
  - Implement complex PDF annotation logic (drawing notes over PDF view).

- **Phase 3: Testing & Đóng gói (Tuần 6)**
  - Team-wide bug bash and final bug fixing.
  - Prepare presentation slides and demo scripts.
  - Export the final installer (.exe).

## **6. Resources and Tools** 

- **Development:** Visual Studio, C# (.NET Core/.NET 6+)
- **Database:** SQLite, Entity Framework Core
- **Libraries:** MaterialDesignInXaml, PdfiumViewer
- **APIs:** OpenAI API / Google Gemini API
- **Design & Management:** Figma, Trello/Jira
- **Version Control:** Git, GitHub

## **7. Risk Assessment** 

- **Risk:** Difficulty rendering and interacting with PDFs inside WPF. 
  **Mitigation:** Allocate time in Phase 1 for pure R&D and standalone testing of the `PdfiumViewer` library before integration.
- **Risk:** High AI API usage costs or rate limits. 
  **Mitigation:** Utilize free tier APIs or mock API responses during the initial development and testing phases.
- **Risk:** Synchronization issues between UI overlays and PDF coordinates for annotations. 
  **Mitigation:** Close collaboration between the UI Lead and the Annotation module developer; start with basic highlights before moving to freehand notes.

## **8. References (Optional)** 

- Microsoft WPF Documentation
- MaterialDesignInXaml GitHub Repository
- Entity Framework Core Documentation
