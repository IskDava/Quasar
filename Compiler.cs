using System;
using System.Text.RegularExpressions;
using Token = System.Collections.Generic.List<string?>;
using TokenList = System.Collections.Generic.List<System.Collections.Generic.List<string?>>;
using Spectre.Console;
using OneOf;

class Compiler
{
    public class JobsLang
    {
        public static OneOf<string, Errors.BaseError> Run(string input)
        {
            OneOf<TokenList, Errors.BaseError> lexer_result = Lexer.Lex(input); // Recieving lexer's tokens
            
            if (lexer_result.IsT0)
            { // if there are no errors
                TokenList tokens = lexer_result.AsT0;

                string debug_token_res = ""; // Prepairing debug value

                foreach (Token token in tokens)
                {
                    debug_token_res += token[0] + ": " + token[1] + "\n"; // Filling debug
                }

                //? Console.WriteLine(debug_token_res);

                Parser parser = new(tokens); // Recieving Parser's result

                OneOf<List<ASTNode>, Errors.BaseError> ast = parser.Parse(); // Making AST

                string debug_ast = "AST:\n"; // Filling debug with AST

                if (ast.IsT1) return ast.AsT1; // if it is an error return it
                else
                {
                    foreach (ASTNode node in ast.AsT0) debug_ast += node.ToString(); // Filling debug

                    foreach (ASTNode statement in ast.AsT0) // Iterating through statements
                    {
                        OneOf<ASTNode, Errors.BaseError> result = Interpreter.Interpret(statement); // Interpreting (all results are written already in interpreter)
                        if (result.IsT1) {
                            return result.AsT1;
                        }
                    }
                }

                return debug_token_res + "\n" + debug_ast;
            }
            else return lexer_result.AsT1;
        }
    }
    public class Lexer
    {
        public static TokenList TOKENS = [ // All tokens. Commands will be here when programm will initialize
            [@"""[^""]*""", "STRING"],
            [@",", "SEPARATOR"],
            [@"\b[^ ]+\b", "IDENTIFIER"],
            [@"\s+", null],
        ];
        public static Dictionary<string, string> Keywords = new() {
            { "with", "WITH" },
            { "true", "BOOL" },
            { "false", "BOOL" },
            { "nothing", "NOTHING" }
        };
        public static OneOf<TokenList, Errors.BaseError> Lex(string input) // Splitting input to tokens
        {
            TokenList res_tokens = [];

            while (input != "")
            {
                bool found = false;

                foreach (Token token in TOKENS) // iterating through tokens
                {
                    string? pattern = "^" + token[0], token_type = token[1];

                    Match match = Regex.Match(input, pattern); // trying to compare pattern and input
                    if (match.Success) // if found something
                    {
                        if (token_type != null) // skip if null
                        {
                            if (token_type == "STRING") {
                                res_tokens.Add(["STRING", match.Value.Trim('"')]);
                            } else if (token_type == "IDENTIFIER" && Keywords.ContainsKey(match.Value)) {
                                res_tokens.Add([Keywords[match.Value], match.Value]);
                            } else {
                                res_tokens.Add([token_type, match.Value]); // adding token [TYPE, VALUE], e.g. ["LOG", "log"]
                            }
                        }
                        input = input[match.Length..]; // deleting matched part of the string
                        found = true;
                        break;
                    }
                }
                if (!found) // if nothing found
                {
                    return new Errors.SyntaxError($"unexpected symbol '{input[0]}'");
                }
            }
            return res_tokens;
        }
    }
    public class ASTNode(string type, string? value = null)
    {
        public string? type = type, value = value;
        public List<ASTNode> children = [];

        public void Add_child(ASTNode child) {
            children.Add(child);
        }
        public string ToString(int level = 0) {
            string indent = string.Concat(Enumerable.Repeat("  ", level));
            string value_str = "";
            if (value != null) value_str = $": {value}";
            string result = $"{indent}{type}{value_str}";
            foreach (ASTNode child in children) {
                result += "\n" + child.ToString(level + 1);
            }
            return result;
        }
    }
    public class Parser(TokenList tokens)
    {
        public int current_token_index = 0;

        public Token? GetCurrentToken() {
            if (current_token_index < tokens.Count) return tokens[current_token_index];
            return null;
        }
        
        public void Consume() {
            current_token_index += 1;
        }

        public bool CurrentTokenIsContent() {
            List<string> ContentTypes = ["STRING", "BOOL", "IDENTIFIER", "NOTHING"];
            return ContentTypes.Contains(GetCurrentToken()[0]);
        }

        public bool CurrentTokenIsCommand() {
            Commands.BaseCommand? command = Commands.GetCommand(GetCurrentToken()[0]);
            if (command != null) return true;
            return false;
        }

        public OneOf<List<ASTNode>, Errors.BaseError> Parse() {
            OneOf<List<ASTNode>, Errors.BaseError> node = ParseStatements();

            if (node.IsT1) {
                return node.AsT1;
            }

            if (GetCurrentToken() != null) return new Errors.SyntaxError($"unexpected token {GetCurrentToken()[1]}");
            return node;
        }
        public OneOf<List<ASTNode>, Errors.BaseError> ParseStatements() {
            List<ASTNode> statements = [];
            Commands.BaseCommand? command = Commands.GetCommand(GetCurrentToken()[0]);
            if (command != null)
            {
                OneOf<ASTNode, Errors.BaseError> node = ParseCommand();
                if (node.IsT0)
                {
                    statements.Add(node.AsT0);
                }
                else
                {
                    return node.AsT1;
                }
            }
            else
            {
                return new Errors.SyntaxError("unknown command");
            }
            return statements;
        }
        public OneOf<ASTNode, Errors.BaseError> ParseCommand() {
            string command_name = GetCurrentToken()[0];
            Consume();
            if (GetCurrentToken() == null) return new ASTNode(command_name);

            if (CurrentTokenIsContent() || CurrentTokenIsCommand() || GetCurrentToken()[0] == "SEPARATOR")
            {
                ASTNode parent_node = new(command_name);
                List<ASTNode> args = []; // not token list

                bool consumedSeparator = false;
                
                if (GetCurrentToken()[0] == "SEPARATOR") {
                    Consume();
                    consumedSeparator = true;
                }

                if (CurrentTokenIsCommand()) {
                    OneOf<ASTNode, Errors.BaseError> innernode = ParseCommand();
                    if (innernode.IsT1) return innernode.AsT1;
                    args.Add(innernode.AsT0);
                    Consume();
                } else {
                    args.Add(new ASTNode(GetCurrentToken()[0], GetCurrentToken()[1]));
                    Consume();
                    if (GetCurrentToken() != null && GetCurrentToken()[0] == "SEPARATOR" && consumedSeparator) {
                        Consume();
                        consumedSeparator = false;
                    }
                }

                while (GetCurrentToken() != null && GetCurrentToken()[0] != "SEPARATOR") //todo is not the End Of Command
                {
                    if (GetCurrentToken()[0] != "WITH")
                    {
                        return new Errors.SyntaxError($"unexpected {GetCurrentToken()[0]} after command's argument. It should be nothing or \"with\"");
                    }
                    Consume();

                    if (GetCurrentToken() == null) return new Errors.SyntaxError("unexpected end of line after WITH, expected a SEPARATOR or some content");

                    if (GetCurrentToken()[0] == "SEPARATOR") {
                        Consume();
                        consumedSeparator = true;
                    }

                    if (CurrentTokenIsContent())
                    {
                        args.Add(new ASTNode(GetCurrentToken()[0], GetCurrentToken()[1]));
                        Consume();
                        if (GetCurrentToken() != null && GetCurrentToken()[0] == "SEPARATOR" && consumedSeparator) {
                            Consume();
                        }
                    } else if (CurrentTokenIsCommand()) {
                        OneOf<ASTNode, Errors.BaseError> innernode = ParseCommand();
                        if (innernode.IsT1) return innernode.AsT1;
                        args.Add(innernode.AsT0);
                        Consume();
                    } else {
                        return new Errors.SyntaxError($"unexpected {GetCurrentToken()[0]} after WITH. It should be some content");
                    }
                }
                foreach (ASTNode arg in args)
                {
                    parent_node.Add_child(arg);
                }
                return parent_node;
            }
            return new Errors.SyntaxError($"unexpected {GetCurrentToken()[0]} after command. It should be blank, some content or antoher command");
        }
    }
    public class Interpreter
    {
        public static OneOf<ASTNode, Errors.BaseError> Interpret(ASTNode node)
        {
            string? command_name = node.type;
            Commands.BaseCommand? command = Commands.GetCommand(command_name);
            OneOf<ASTNode, Errors.BaseError>? result = null;
            if (command == null) {
                return new Errors.SyntaxError($"Unknown command {command_name}");
            }
            else {
                result = command.Interpret(node.children, []);
                return result.Value;
            }
        }
    }
}