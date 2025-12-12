using System;
using System.Collections.Generic;

namespace SQL_Compiler.Models
{
    public class ParseTreeNode
    {
        public string Name { get; set; }
        public string Lexeme { get; set; } // Optional: for leaf nodes
        public List<ParseTreeNode> Children { get; set; } = new List<ParseTreeNode>();

        public ParseTreeNode(string name, string lexeme = "")
        {
            Name = name;
            Lexeme = lexeme;
        }

        public ParseTreeNode AddChild(ParseTreeNode node)
        {
            if (node != null)
                Children.Add(node);
            return node;
        }
    }

    public class Parser
    {
        private readonly List<SqlToken> _tokens;
        private int _current = 0;
        private readonly List<string> _errors = new List<string>();

        public Parser(List<SqlToken> tokens)
        {
            _tokens = tokens;
        }

        public List<string> Errors => _errors;

        private SqlToken Peek()
        {
            if (_current >= _tokens.Count)
            {
                // Return EOF with line/column from last token
                var lastToken = _tokens.Count > 0 ? _tokens[_tokens.Count - 1] : null;
                return new SqlToken
                {
                    Type = "EOF",
                    Lexeme = "",
                    Line = lastToken?.Line ?? 1,
                    Column = lastToken?.Column ?? 0
                };
            }
            return _tokens[_current];
        }

        private SqlToken Previous()
        {
            return _tokens[_current - 1];
        }

        private bool IsAtEnd()
        {
            return Peek().Type == "EOF";
        }

        private SqlToken Advance()
        {
            if (!IsAtEnd()) _current++;
            return Previous();
        }

        private bool Check(string type)
        {
            if (IsAtEnd()) return false;
            return Peek().Type == type;
        }

        private bool Match(params string[] types)
        {
            foreach (var type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        private SqlToken Consume(string type, string message)
        {
            if (Check(type)) return Advance();

            // Error handling
            SyntaxError(message, Peek());
            return new SqlToken { Type = "ERROR", Lexeme = "" }; // Return dummy to avoid crash, or throw
        }

        private SqlToken _lastErrorToken;

        private void SyntaxError(string message, SqlToken token)
        {
            // Suppress redundant errors at the same location (Panic Mode duplicate reduction)
            if (_lastErrorToken != null && 
                _lastErrorToken.Line == token.Line && 
                _lastErrorToken.Column == token.Column)
            {
                return;
            }

            _lastErrorToken = token;
            _errors.Add($"Syntax Error: {message} at line {token.Line}, column {token.Column}, found '{token.Lexeme}'");
        }

        // Panic mode recovery
        private void Synchronize()
        {
            Advance();

            while (!IsAtEnd())
            {
                if (Previous().Type == "SEMICOLON") return;

                switch (Peek().Type)
                {
                    case "SELECT":
                    case "INSERT":
                    case "UPDATE":
                    case "DELETE":
                    case "CREATE":
                        return;
                }

                Advance();
            }
        }

        public ParseTreeNode Parse()
        {
            var root = new ParseTreeNode("Program");
            while (!IsAtEnd())
            {
                try 
                {
                    var stmt = ParseStatement();
                    if (stmt != null) root.AddChild(stmt);
                }
                catch (Exception)
                {
                    Synchronize();
                }
            }
            return root;
        }

        private ParseTreeNode ParseStatement()
        {
            if (Match("SELECT")) return ParseSelectStmt();
            if (Match("INSERT")) return ParseInsertStmt();
            if (Match("UPDATE")) return ParseUpdateStmt();
            if (Match("DELETE")) return ParseDeleteStmt();
            if (Match("CREATE")) return ParseCreateStmt();

            // Only report if it's not EOF, otherwise we get weird errors at end of file logic sometimes
            if (!IsAtEnd())
            {
                SyntaxError("Expected statement (SELECT, INSERT, UPDATE, DELETE, CREATE)", Peek());
                Synchronize();
            }
            return null;
        }

        // Create Statement
        private ParseTreeNode ParseCreateStmt()
        {
            var createNode = new ParseTreeNode("CreateStmt");
            createNode.AddChild(new ParseTreeNode("KEYWORD", "CREATE")); // Already matched

            Consume("TABLE", "Expected 'TABLE' after 'CREATE'");
            createNode.AddChild(new ParseTreeNode("KEYWORD", "TABLE"));

            var tableName = Consume("IDENTIFIER", "Expected table name");
            createNode.AddChild(new ParseTreeNode("IDENTIFIER", tableName.Lexeme));

            Consume("LEFT_PAREN", "Expected '(' after table name");
            createNode.AddChild(new ParseTreeNode("DELIMITER", "("));

            createNode.AddChild(ParseFieldList());

            Consume("RIGHT_PAREN", "Expected ')' after field list");
            createNode.AddChild(new ParseTreeNode("DELIMITER", ")"));

            Consume("SEMICOLON", "Expected ';' at end of statement");
            createNode.AddChild(new ParseTreeNode("DELIMITER", ";"));

            return createNode;
        }

        private ParseTreeNode ParseFieldList()
        {
            var listNode = new ParseTreeNode("FieldList");
            do
            {
                listNode.AddChild(ParseFieldDef());
            } while (Match("COMMA") && (listNode.AddChild(new ParseTreeNode("DELIMITER", ",")) != null)); // Add comma node if matched
            return listNode;
        }

        private ParseTreeNode ParseFieldDef()
        {
            var fieldNode = new ParseTreeNode("FieldDef");
            
            var name = Consume("IDENTIFIER", "Expected column name");
            fieldNode.AddChild(new ParseTreeNode("IDENTIFIER", name.Lexeme));

            var type = Consume("TYPE", "Expected column type");
            fieldNode.AddChild(new ParseTreeNode("TYPE", type.Lexeme));

            if (Match("PRIMARY"))
            {
                fieldNode.AddChild(new ParseTreeNode("KEYWORD", "PRIMARY"));
                Consume("KEY", "Expected 'KEY' after 'PRIMARY'");
                fieldNode.AddChild(new ParseTreeNode("KEYWORD", "KEY"));
            }

            return fieldNode;
        }

        // ==========================================
        // EXPRESSION PARSING (Condition logic)
        // ==========================================

        private ParseTreeNode ParseWhereClause()
        {
            var whereNode = new ParseTreeNode("WhereClause");
            whereNode.AddChild(new ParseTreeNode("KEYWORD", "WHERE")); // Caller matched WHERE
            whereNode.AddChild(ParseCondition());
            return whereNode;
        }

        private ParseTreeNode ParseCondition()
        {
            return ParseOrExpression();
        }

        private ParseTreeNode ParseOrExpression()
        {
            var left = ParseAndExpression();
            
            while (Match("OR"))
            {
                var orNode = new ParseTreeNode("Condition"); // OrExpression
                orNode.AddChild(left);
                orNode.AddChild(new ParseTreeNode("OPERATOR", "OR"));
                orNode.AddChild(ParseAndExpression());
                left = orNode;
            }
            return left;
        }

        private ParseTreeNode ParseAndExpression()
        {
            var left = ParseNotExpression();

            while (Match("AND"))
            {
                var andNode = new ParseTreeNode("Condition"); // AndExpression
                andNode.AddChild(left);
                andNode.AddChild(new ParseTreeNode("OPERATOR", "AND"));
                andNode.AddChild(ParseNotExpression());
                left = andNode;
            }
            return left;
        }

        private ParseTreeNode ParseNotExpression()
        {
            if (Match("NOT"))
            {
                var notNode = new ParseTreeNode("Condition");
                notNode.AddChild(new ParseTreeNode("OPERATOR", "NOT"));
                notNode.AddChild(ParseNotExpression());
                return notNode;
            }
            return ParseRelExpression();
        }

        private ParseTreeNode ParseRelExpression()
        {
            var left = ParseTerm();

            if (Match("EQUAL", "NOT_EQUAL", "LESS_THAN", "GREATER_THAN", "LESS_EQUAL", "GREATER_EQUAL", "LIKE"))
            {
                var opToken = Previous();
                var relNode = new ParseTreeNode("Condition"); // RelExpression
                relNode.AddChild(left);
                relNode.AddChild(new ParseTreeNode("OPERATOR", opToken.Lexeme));
                relNode.AddChild(ParseTerm());
                return relNode;
            }
            
            return left;
        }

        private ParseTreeNode ParseTerm()
        {
            if (Match("IDENTIFIER"))
                return new ParseTreeNode("IDENTIFIER", Previous().Lexeme);
            
            if (Match("NUMBER"))
                return new ParseTreeNode("NUMBER", Previous().Lexeme);
            
            if (Match("STRING"))
                return new ParseTreeNode("STRING", Previous().Lexeme);

            if (Match("NULL"))
                return new ParseTreeNode("KEYWORD", "NULL");

            if (Match("TRUE"))
                return new ParseTreeNode("BOOLEAN_LITERAL", "TRUE");

            if (Match("FALSE"))
                return new ParseTreeNode("BOOLEAN_LITERAL", "FALSE");

            if (Match("LEFT_PAREN"))
            {
                var expr = ParseCondition();
                Consume("RIGHT_PAREN", "Expected ')' after expression");
                return expr; // Or wrap in a ParenthesizedExpr node if desired, but flattening is fine for tree
            }

            // Error
            SyntaxError("Expected expression (identifier, value, or parenthesis)", Peek());
            return new ParseTreeNode("ERROR", "Missing Term");
        }

        // Helper for Select Statement
        private ParseTreeNode ParseSelectStmt()
        {
            var node = new ParseTreeNode("SelectStmt");
            node.AddChild(new ParseTreeNode("KEYWORD", "SELECT")); // Already matched

            node.AddChild(ParseSelectList());

            Consume("FROM", "Expected 'FROM' clause after SelectList");
            node.AddChild(new ParseTreeNode("KEYWORD", "FROM"));

            var table = Consume("IDENTIFIER", "Expected table name");
            node.AddChild(new ParseTreeNode("IDENTIFIER", table.Lexeme));

            if (Match("WHERE"))
            {
                node.AddChild(ParseWhereClause());
            }

            if (Match("ORDER"))
            {
                 var orderNode = new ParseTreeNode("OrderClause");
                 orderNode.AddChild(new ParseTreeNode("KEYWORD", "ORDER"));
                 Consume("BY", "Expected 'BY' after 'ORDER'");
                 orderNode.AddChild(new ParseTreeNode("KEYWORD", "BY"));
                 
                 var col = Consume("IDENTIFIER", "Expected column name in ORDER BY");
                 orderNode.AddChild(new ParseTreeNode("IDENTIFIER", col.Lexeme));

                 if (Match("ASC", "DESC"))
                 {
                     orderNode.AddChild(new ParseTreeNode("KEYWORD", Previous().Lexeme));
                 }
                 node.AddChild(orderNode);
            }

            Consume("SEMICOLON", "Expected ';' at end of query");
            node.AddChild(new ParseTreeNode("DELIMITER", ";"));

            return node;
        }

        private ParseTreeNode ParseSelectList()
        {
            var listNode = new ParseTreeNode("SelectList");
            if (Match("MULTIPLY"))
            {
                listNode.AddChild(new ParseTreeNode("OPERATOR", "*"));
                return listNode;
            }

            do
            {
                var id = Consume("IDENTIFIER", "Expected column name");
                listNode.AddChild(new ParseTreeNode("IDENTIFIER", id.Lexeme));
            } while (Match("COMMA") && (listNode.AddChild(new ParseTreeNode("DELIMITER", ",")) != null));

            return listNode;
        }

        // Helper for Insert Statement
        private ParseTreeNode ParseInsertStmt()
        {
            var node = new ParseTreeNode("InsertStmt");
            node.AddChild(new ParseTreeNode("KEYWORD", "INSERT")); // matched

            Consume("INTO", "Expected 'INTO' after 'INSERT'");
            node.AddChild(new ParseTreeNode("KEYWORD", "INTO"));

            var table = Consume("IDENTIFIER", "Expected table name");
            node.AddChild(new ParseTreeNode("IDENTIFIER", table.Lexeme));

            Consume("VALUES", "Expected 'VALUES' keyword");
            node.AddChild(new ParseTreeNode("KEYWORD", "VALUES"));

            Consume("LEFT_PAREN", "Expected '(' before values");
            node.AddChild(new ParseTreeNode("DELIMITER", "("));

            node.AddChild(ParseValueList());

            Consume("RIGHT_PAREN", "Expected ')' after values");
            node.AddChild(new ParseTreeNode("DELIMITER", ")"));

            Consume("SEMICOLON", "Expected ';' at end");
            node.AddChild(new ParseTreeNode("DELIMITER", ";"));

            return node;
        }

        private ParseTreeNode ParseValueList()
        {
            var listNode = new ParseTreeNode("ValueList");
            do
            {
               listNode.AddChild(ParseTerm()); // Reuse ParseTerm as it handles literals
            } while (Match("COMMA") && (listNode.AddChild(new ParseTreeNode("DELIMITER", ",")) != null));
            return listNode;
        }

        // Helper for Update Statement
        private ParseTreeNode ParseUpdateStmt()
        {
            var node = new ParseTreeNode("UpdateStmt");
            node.AddChild(new ParseTreeNode("KEYWORD", "UPDATE"));

            var table = Consume("IDENTIFIER", "Expected table name");
            node.AddChild(new ParseTreeNode("IDENTIFIER", table.Lexeme));

            Consume("SET", "Expected 'SET' keyword");
            node.AddChild(new ParseTreeNode("KEYWORD", "SET"));

            node.AddChild(ParseAssignList());

            if (Match("WHERE"))
            {
                node.AddChild(ParseWhereClause());
            }

            Consume("SEMICOLON", "Expected ';' at end");
            node.AddChild(new ParseTreeNode("DELIMITER", ";"));
            return node;
        }

        private ParseTreeNode ParseAssignList()
        {
            var listNode = new ParseTreeNode("AssignList");
            do 
            {
                var id = Consume("IDENTIFIER", "Expected column name");
                var assignNode = new ParseTreeNode("Assignment");
                assignNode.AddChild(new ParseTreeNode("IDENTIFIER", id.Lexeme));

                Consume("EQUAL", "Expected '=' in assignment");
                assignNode.AddChild(new ParseTreeNode("OPERATOR", "="));

                assignNode.AddChild(ParseTerm());
                listNode.AddChild(assignNode);

            } while (Match("COMMA") && (listNode.AddChild(new ParseTreeNode("DELIMITER", ",")) != null));

            return listNode;
        }

        // Helper for Delete Statement
        private ParseTreeNode ParseDeleteStmt()
        {
             var node = new ParseTreeNode("DeleteStmt");
             node.AddChild(new ParseTreeNode("KEYWORD", "DELETE"));
             
             Consume("FROM", "Expected 'FROM' after 'DELETE'");
             node.AddChild(new ParseTreeNode("KEYWORD", "FROM"));

             var table = Consume("IDENTIFIER", "Expected table name");
             node.AddChild(new ParseTreeNode("IDENTIFIER", table.Lexeme));

             if (Match("WHERE"))
             {
                 node.AddChild(ParseWhereClause());
             }

             Consume("SEMICOLON", "Expected ';' at end");
             node.AddChild(new ParseTreeNode("DELIMITER", ";"));

             return node;
        }
    }
}
