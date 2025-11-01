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

            var lexer = new Lexer();
            var tokens = lexer.Analyze(inputCode);

            var result = tokens.Select(t => new
            {
                type = t.Type.ToString(),   
                lexeme = t.Lexeme,
                line = t.Line,
                column = t.Column
            }).ToList();

            return Json(result);
        }
    }
}
