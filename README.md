# SQL Compiler

A comprehensive web-based SQL compiler built with ASP.NET Core MVC that performs **lexical analysis** (Phase 1) and **syntax analysis** (Phase 2) on SQL queries. The compiler features a professional web interface with interactive parse tree visualization and real-time error detection.

## 🎯 Project Status

- ✅ **Phase 1**: Lexical Analyzer (Complete)
- ✅ **Phase 2**: Syntax Analyzer (Complete)
- ⏳ **Phase 3**: Semantic Analyzer (Upcoming)

## ✨ Features

### Lexical Analysis (Phase 1)
- **Token Recognition**: Identifies keywords, identifiers, operators, literals, and delimiters
- **Comment Support**: Single-line (`--`) and multi-line (`##...##`) comments
- **Error Detection**: Reports invalid characters with line/column information

### Syntax Analysis (Phase 2)
- **Recursive Descent Parser**: Hand-built parser following formal grammar rules
- **Parse Tree Generation**: Visual hierarchical representation of SQL structure
- **Error Recovery**: Panic mode synchronization for multiple error detection
- **Complex Expressions**: Full support for compound boolean queries (AND, OR, NOT)

### Web Interface
- **Status Bar**: Real-time validation feedback (green for valid, red for errors)
- **Interactive Parse Tree**: Zoom and pan controls for large trees
- **Error Highlighting**: Visual indicators for syntax errors in code editor
- **File Upload**: Analyze SQL files (.txt, .sql)
- **Example Queries**: Built-in examples to demonstrate functionality

## 📋 Supported SQL Statements

- **SELECT**: `SELECT columns FROM table WHERE condition ORDER BY column;`
- **INSERT**: `INSERT INTO table VALUES (value1, value2);`
- **UPDATE**: `UPDATE table SET column = value WHERE condition;`
- **DELETE**: `DELETE FROM table WHERE condition;`
- **CREATE TABLE**: `CREATE TABLE name (column TYPE, ...);`

## 🔤 Token Types

- **Keywords**: SELECT, FROM, WHERE, INSERT, UPDATE, DELETE, CREATE, TABLE, etc.
- **Types**: INT, FLOAT, TEXT, VARCHAR, CHAR, DATE, DATETIME
- **Identifiers**: Table and column names
- **Literals**: Numbers, strings, NULL, TRUE, FALSE
- **Operators**: =, !=, <, >, <=, >=, +, -, *, /, LIKE
- **Delimiters**: (, ), ,, ;, .

## 🌳 Grammar (EBNF)

```ebnf
Program      ::= Statement*
Statement    ::= SelectStmt | InsertStmt | UpdateStmt | DeleteStmt | CreateStmt

SelectStmt   ::= "SELECT" SelectList "FROM" Identifier WhereClause? OrderClause? ";"
InsertStmt   ::= "INSERT" "INTO" Identifier "VALUES" "(" ValueList ")" ";"
UpdateStmt   ::= "UPDATE" Identifier "SET" AssignList WhereClause? ";"
DeleteStmt   ::= "DELETE" "FROM" Identifier WhereClause? ";"
CreateStmt   ::= "CREATE" "TABLE" Identifier "(" FieldList ")" ";"

Condition    ::= OrExpression
OrExpression ::= AndExpression ("OR" AndExpression)*
AndExpression::= NotExpression ("AND" NotExpression)*
NotExpression::= "NOT" NotExpression | RelExpression
RelExpression::= Term (REL_OP Term)*
Term         ::= Identifier | Number | String | "NULL" | "TRUE" | "FALSE" | "(" Condition ")"
```

## 🚀 Getting Started

### Prerequisites
- .NET 9.0 SDK or later
- Visual Studio 2022 or VS Code

### Running the Project

1. **Clone the repository**
   ```bash
   git clone https://github.com/Sherif-ahmed177/SQL-Compiler.git
   cd SQL-Compiler
   ```

2. **Run the application**
   ```bash
   cd SQL-Compiler
   dotnet run
   ```

3. **Open in browser**
   - Navigate to `http://localhost:5041`
   - The web interface will load automatically

### Using the Compiler

1. **Enter SQL Code**: Type or paste SQL queries in the code editor
2. **Upload File**: Click "Upload" to load SQL from a file
3. **Analyze**: Click "Analyze Code" to process the input
4. **View Results**:
   - **Parse Tree Tab**: See the hierarchical structure
   - **Tokens Tab**: View all identified tokens
   - **Status Bar**: Check validation status (green/red)

## 📝 Example Queries

### Valid Query
```sql
SELECT name, age FROM students WHERE age >= 18 AND status = 'active';
INSERT INTO users VALUES (1, 'Alice', TRUE);
UPDATE products SET price = 100 WHERE id = 5;
DELETE FROM logs WHERE date < '2024-01-01';
CREATE TABLE employees (id INT PRIMARY KEY, name TEXT);
```

### Complex Boolean Expression
```sql
SELECT * FROM users 
WHERE age > 18 AND (status = 'active' OR role = 'admin') AND NOT deleted;
```

## 🛠️ Technologies

- **Backend**: ASP.NET Core MVC (.NET 9.0)
- **Frontend**: HTML5, CSS3, JavaScript
- **UI Framework**: Bootstrap 5.3
- **Icons**: Bootstrap Icons
- **Language**: C#

## 📂 Project Structure

```
SQL-Compiler/
├── SQL-Compiler.sln           # Solution file
├── SQL-Compiler/              # Main project
│   ├── Controllers/           # MVC Controllers
│   │   ├── LexerController.cs # API for analysis
│   │   └── HomeController.cs
│   ├── Models/                # Core logic
│   │   ├── Lexer.cs          # Lexical analyzer
│   │   ├── Parser.cs         # Syntax analyzer
│   │   └── Token.cs          # Token definition
│   ├── Views/                 # Razor views
│   │   ├── Lexer/
│   │   │   └── Index.cshtml  # Main UI
│   │   └── Shared/
│   │       └── _Layout.cshtml
│   ├── wwwroot/              # Static files
│   │   └── css/
│   │       └── site.css      # Custom styles
│   ├── Program.cs            # Application entry
│   └── input.txt             # Test queries
└── README.md
```

## 🎓 Academic Project

This compiler is developed as part of **CSCI415 - Compiler Design** course project (Fall 2025). The project is divided into three phases:

- **Phase 1** (Nov 1): Lexical Analyzer ✅
- **Phase 2** (Dec 19): Syntax Analyzer ✅
- **Phase 3** (Dec 26): Semantic Analyzer ⏳

## 📄 Documentation

- **Phase 2 Report**: See `Phase2_Report.md` for detailed grammar, parsing technique, and implementation details
- **Implementation Plan**: See `implementation_plan.md` for development roadmap

## 🤝 Contributing

This is an academic project. Contributions are not accepted as per course requirements.

## 📜 License

MIT License - See LICENSE file for details

