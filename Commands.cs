using System;
using System.Text.RegularExpressions;
using Token = System.Collections.Generic.List<string?>;
using TokenList = System.Collections.Generic.List<System.Collections.Generic.List<string?>>;
using Spectre.Console;
using OneOf;

using ASTNode = Compiler.ASTNode;

class Commands
{
    public static List<BaseCommand> CommandsList = [];
    public static BaseCommand? GetCommand(string? name)
    {
        if (name == null) return null;
        foreach (BaseCommand command in CommandsList)
        {
            if (name == command.InternalName) return command;
        }
        return null;
    }
    abstract public class BaseCommand
    {
        public required string InternalName, FriendlyName, Description, Usage;
        public required List<string> AcceptedTypes;
        // Internal names are used by programm in regex or searching. Should be written in UPPER_SNAKE_CASE
        // Friendly names are made automaticly, written in documentaitions and represent a common way to write the command
        // Description is used in documentation to describe the command's action. It shouldn't have any colors
        // Usage is used in documentation to provide examples of command. I don't recommend to put more than 5 examples, but at least one is essential. Should be collorized in accordance with JHR 
        // RegexContent is created by programm automaticly to work with Lexer. You mustn't change it
        // Implementations should contain ALL possible names for your command in a string array, where first one is common one
        public int required_arguments = 0, unrequired_arguments = 0;
        public abstract OneOf<ASTNode, Errors.BaseError>? Interpret(List<ASTNode> args, Dictionary<string, ASTNode> kwargs);

        public string GetRegex()
        {
            return @"\b(" + FriendlyName + @")\b";
        }

        public string AcceptedTypesToString() {
            string s = "";
            foreach (string type in AcceptedTypes[..^1]) {
                s += type + ", ";
            }
            s = s[..^2];
            s += " or " + AcceptedTypes[AcceptedTypes.Count - 1];
            return s;
        }
    }
    public class Log : BaseCommand
    {
        public Log() : base()
        {
            InternalName = "LOG";
            Description = "Writes your content into console";
            Usage = "[bold yellow]log [[[blue]any content[/]]] [white]|| [/]log [[[blue]any content[/]]] with [[[blue]any content[/]]] with [[[blue]any content[/]]]... [/]";
            FriendlyName = "log";

            AcceptedTypes = ["STRING", "BOOL", "NOTHING"];
        }
        public override OneOf<ASTNode, Errors.BaseError>? Interpret(List<ASTNode> args, Dictionary<string, ASTNode> kwargs)
        {
            string content = "";

            foreach (ASTNode arg in args) {
                ASTNode temp = arg;
            back:
                if (AcceptedTypes.Contains(temp.type)) {
                    content += (string)temp.value;
                } else {
                    BaseCommand? command = GetCommand(temp.type);
                    if (command != null) {
                        OneOf<ASTNode, Errors.BaseError> innernode = Compiler.Interpreter.Interpret(temp);
                        if (innernode.IsT1) return innernode.AsT1;
                        temp = innernode.AsT0;
                        goto back;
                    } else {

                        return new Errors.ValueError($"expected type {this.AcceptedTypesToString()} for {InternalName}, but not {temp.type}");
                    }
                }
            }
            
            Console.WriteLine(content);

            ASTNode result = new("NOTHING", "nothing");
            return result;
        }
    }
}