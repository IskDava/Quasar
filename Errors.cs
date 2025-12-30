using System;
using System.Text.RegularExpressions;
using Token = System.Collections.Generic.List<string?>;
using TokenList = System.Collections.Generic.List<System.Collections.Generic.List<string?>>;
using Spectre.Console;
using OneOf;

class Errors
{
    abstract public class BaseError(string error_name, string msg)
    {
        public string name = error_name, message = msg;

        public void Write()
        {
            AnsiConsole.Markup($"[red]{name}[/]: {message}\n");
        }
    }
    public class SyntaxError(string msg) : BaseError("Incorrect syntax", msg) {}
    public class SecurityError(string msg) : BaseError("Security error", msg) {}
    public class ArgumentsError(string msg) : BaseError("Arguments' error", msg) {}
    public class ValueError(string msg) : BaseError("Invalid value", msg) {}
}