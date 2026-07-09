# Workspace Rules

## Skill Usage Guidelines

When working in this workspace, ALWAYS utilize the skills located in the `.agents/skill` directory according to the task at hand. The following mapping instructs you on which skill to invoke based on the context of the work:

### 1. Backend & .NET Development
- **`csharp-pro`**: Use this when writing, refactoring, or reviewing C# code to ensure best practices.
- **`dotnet-architect`**: Use this when making structural and architectural decisions for .NET applications.
- **`dotnet-backend-patterns`**: Use this when implementing design patterns, APIs, and business logic in the backend.

### 2. Database & Data Access
- **`database-design`**: Invoke this when creating databases, designing schemas, writing SQL queries, or setting up Entity Framework/ORMs.

### 3. Frontend & UI/UX Design
- **`ui-ux-pro-max`**: Use for high-level user experience design, wireframing, and ensuring premium application aesthetics.
- **`ui-component`**: Use when building modular, reusable front-end components.
- **`minimalist-ui`**: Use when the design language requires a clean, modern, and minimalist aesthetic.

### 4. System & Software Architecture
- **`software-architecture`**: Use when planning the overall structure of the software, microservices, or system components.

### 5. Documentation
- **`documentation`**: Use when generating API docs, code comments, architecture docs, or general technical writing.
- **`documentation-templates`**: Use to fetch standard templates for READMEs and other project documentation.

### 6. AI & Prompt Engineering
- **`prompt-engineering` & `prompt-engineering-patterns`**: Use when crafting complex prompts, interacting with LLMs, or building AI-driven features.
- **`ai-product`**: Use when conceptualizing or refining AI features within the product.

### 7. Other specialized skills
- **`squirrel`**: Use when working with Squirrel scripting, related database tools, or specific workflows defined in this skill.

**Requirement**: Before executing tasks in these domains, use the `view_file` tool to read the `SKILL.md` file of the relevant skill in `c:\Code\github\PRN212_PROJECT\.agents\skill\<skill_name>\SKILL.md` to ensure you follow the defined instructions exactly.

## Coding Conventions

All code written in this workspace MUST follow the project's Coding Conventions defined in [Coding_Convention.md](file:///c:/Code/github/PRN212_PROJECT/Document/Coding_Convention.md). 
Key rules that you must follow:
1. **C# Naming**: PascalCase for Classes/Properties/Methods. `_camelCase` for private fields. `I` prefix for Interfaces.
2. **MVVM Pattern**: Strictly adhere to MVVM. Never write business logic in XAML Code-Behind (`.xaml.cs`). Use `ICommand` and `INotifyPropertyChanged`.
3. **File Structure**: One class per file. Organize files logically into `Views`, `ViewModels`, `Models`, and `Services` directories.
