# SQL-Compiler

A web-based SQL lexical analyzer built with ASP.NET Core MVC that performs tokenization of SQL queries. The compiler currently implements the lexical analysis (scanning) phase of compilation, breaking down SQL code into meaningful tokens.

## Features

- **Live Analysis**: Instantly analyze SQL code and view token breakdowns
- **Token Visualization**: Clear presentation of tokens with color-coded categories
- **File Support**: Upload and analyze SQL code from .txt files
- **Export Capability**: Download token analysis results as CSV
- **Interactive UI**: Filter tokens, view summaries, and get instant feedback
- **Example Code**: Built-in SQL examples to demonstrate functionality

## Token Types

The lexer recognizes the following token types:

- **Keywords**: SQL reserved words (CREATE, TABLE, INSERT, etc.)
- **Types**: Data types (INT, FLOAT, TEXT)
- **Identifiers**: Table and column names
- **Numbers**: Numeric literals
- **Strings**: Text enclosed in single quotes
- **Operators**: =, >, <, +, -, *, /
- **Delimiters**: (, ), ,, ;

## Comment Support

The lexer supports two types of comments:
- Single-line comments starting with `--`
- Block comments enclosed in `#...#`

## Getting Started

1. Clone the repository
2. Open the solution in Visual Studio
3. Run the project
4. Navigate to the Lexer page
5. Enter SQL code or use the example button
6. Click Analyze to see the token breakdown

## Technologies

- ASP.NET Core MVC
- C# (.NET 9.0)
- Bootstrap 5
- JavaScript

## Example Usage

```sql
CREATE TABLE students (id INT, name TEXT);
INSERT INTO students VALUES (1, 'Sherif');
SELECT name FROM students WHERE id = 1;
```

This code will be tokenized into its components, identifying keywords, identifiers, types, literals, and delimiters.