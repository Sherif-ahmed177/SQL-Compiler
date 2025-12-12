using Microsoft.AspNetCore.Mvc;
using SQL_Compiler.Models;
using System.Collections.Generic;
using System.Linq;

namespace SQL_Compiler.Controllers
{
    public class LexerController : Controller
    {
        [HttpGet]
        public IActionResult Index() => View();

        [HttpPost]
        public IActionResult Analyze([FromBody] string inputCode)
        {
            if (string.IsNullOrWhiteSpace(inputCode))
                return BadRequest("Please enter SQL-like code.");

            // 1. Lexical Analysis
            var lexer = new Lexer();
            var tokens = lexer.Analyze(inputCode);

            // 2. Syntax Analysis
            var parser = new Parser(tokens);
            var root = parser.Parse();

            var result = new
            {
                tokens = tokens.Select(t => new
                {
                    type = t.Type.ToString(),   
                    lexeme = t.Lexeme,
                    line = t.Line,
                    column = t.Column
                }).ToList(),
                tree = root, // ParseTreeNode is serializable
                errors = parser.Errors
            };

            return Json(result);
        }
    }
}
