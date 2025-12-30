using System;
using System.Text.RegularExpressions;
using Token = System.Collections.Generic.List<string?>;
using TokenList = System.Collections.Generic.List<System.Collections.Generic.List<string?>>;
using Spectre.Console;
using OneOf;

class Variables 
{
    public static Dictionary<string, object?> globals = new()
    {
        {"Running", true },
        {"DebugMode", false },
        {"Version", "Alpha-1 0.0.1"}
    };
}