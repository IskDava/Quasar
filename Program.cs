#pragma warning disable

using System;
using System.Text.RegularExpressions;
using Token = System.Collections.Generic.List<string?>;
using TokenList = System.Collections.Generic.List<System.Collections.Generic.List<string?>>;
using Spectre.Console;
using OneOf;

// Here is the starting point
class Quasar
{
    static bool running = true;

    static void RefreshCommands() {
        Type baseType = typeof(Commands.BaseCommand);

        Commands.CommandsList = [.. AppDomain.CurrentDomain // Filling command's list with object that are instances of BaseCommand
            .GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass
                    && !t.IsAbstract
                    && baseType.IsAssignableFrom(t)
                    && t.GetConstructor(Type.EmptyTypes) != null
                )
            .Select(t => (Commands.BaseCommand)Activator.CreateInstance(t)!)];

        foreach (Commands.BaseCommand command in Commands.CommandsList)
        {
            Compiler.Lexer.TOKENS.Insert(0, [command.GetRegex(), command.InternalName]); // Filling token's list with commands
        }
    }

    static void Main() // Starting point
    {
        QTypes.String s = new("Hello!");
        QTypes.String e = new("llo!");
        Console.WriteLine(s);
        Console.WriteLine(s + " World!");
        Console.WriteLine(s * 5);
        Console.WriteLine(s + '!');
        Console.WriteLine(s[0]);
        Console.WriteLine(s.sub(1, 3));
        Console.WriteLine(s.length);
        Console.WriteLine(s.sub(2, 6, 2));
        QTypes.String s1 = new("Hello!");
        QTypes.String s2 = new("Smth");
        Console.WriteLine(s1 == s);
        Console.WriteLine(s > s2);
        Console.WriteLine(s >= s1);
        Console.WriteLine(s - '!');
        Console.WriteLine(s - e);
        /*
        RefreshCommands();

        Console.OutputEncoding = System.Text.Encoding.UTF8; // Changing encoding for UTF8

        Console.WriteLine($"CSQuasar {Variables.globals["Version"]}");
        Console.WriteLine("By David Iskiev (Dava), 2025\n"); // Welcome message

        while (running)
        {
            Console.Write(">> "); // Adding >>

            var input = Console.ReadLine(); // Requiring user input

            if (!string.IsNullOrWhiteSpace(input)) // Ignoring nothing or white spaces
            {
                switch (input)
                {
                    case "debug on": // Turning on debug mode //TODO: make mode a command and debug an argument
                        Variables.globals["DebugMode"] = true; // That will show Lexer's and Parser's output
                        break;
                    case "debug off": // Turning off debug mode //TODO: make mode a command and debug an argument
                        Variables.globals["DebugMode"] = false;
                        break;
                    default:
                        OneOf<string, Errors.BaseError> result = Compiler.JobsLang.Run(input); // Running compiler with user's input
                        if ((bool)Variables.globals["DebugMode"] && result.IsT0) // showing logs if  in debug mode
                        {
                            Console.WriteLine("\n" + result.AsT0);
                        }
                        else if (result.IsT1)
                        {
                            result.AsT1.Write();
                        }
                        break;
                }
            }
        }*/
    }
}