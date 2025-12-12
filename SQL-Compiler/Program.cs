using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SQL_Compiler.Models;
using System;
using System.IO;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);

// Register MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ==========================================
// PHASE 2 TEST RUNNER
// ==========================================
string inputPath = Path.Combine(Directory.GetCurrentDirectory(), "input.txt");
if (File.Exists(inputPath))
{
    // Redirect output to file
    string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "parser_output.txt");
    using (var writer = new StreamWriter(outputPath))
    {
        Console.SetOut(writer);
        
        Console.WriteLine("========================================");
        Console.WriteLine("    PHASE 2 PARSER TEST RUNNER");
        Console.WriteLine("========================================");
        
        string input = File.ReadAllText(inputPath);
        Console.WriteLine($"Input Code:\n{input}\n");
        Console.WriteLine("----------------------------------------");

        // 1. Lexical Analysis
        var lexer = new Lexer();
        List<SqlToken> tokens = lexer.Analyze(input);
        
        // 2. Syntax Analysis
        var parser = new Parser(tokens);
        var root = parser.Parse();

        Console.WriteLine("\n[Parse Tree]");
        PrintTree(root, "", true);

        Console.WriteLine("\n[Errors]");
        if (parser.Errors.Count == 0)
        {
            Console.WriteLine("No syntax errors detected.");
        }
        else
        {
            foreach (var error in parser.Errors)
            {
                Console.WriteLine(error); // No color in file
            }
        }
        Console.WriteLine("========================================\n");
        
        // Restore Console
        var standardOutput = new StreamWriter(Console.OpenStandardOutput());
        standardOutput.AutoFlush = true;
        Console.SetOut(standardOutput);
    }
}

void PrintTree(ParseTreeNode node, string indent, bool last)
{
    Console.Write(indent);
    if (last)
    {
        Console.Write("└── ");
        indent += "    ";
    }
    else
    {
        Console.Write("├── ");
        indent += "│   ";
    }

    string displayLexeme = node.Lexeme;
    if (node.Name == "DELIMITER" || node.Name == "OPERATOR" || node.Name == "STRING")
    {
        // Add quotes for clarity if it's a symbol or string literal
        // Check if already quoted (STRINGs might be)
        if (!displayLexeme.StartsWith("'") && !displayLexeme.StartsWith("\""))
        {
             displayLexeme = $"'{displayLexeme}'";
        }
    }
    
    Console.WriteLine($"{node.Name} ({displayLexeme})");

    for (int i = 0; i < node.Children.Count; i++)
    {
        PrintTree(node.Children[i], indent, i == node.Children.Count - 1);
    }
}

// Check for errors before running web app to see console output easily
if (app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Lexer}/{action=Index}/{id?}");

app.Run();
