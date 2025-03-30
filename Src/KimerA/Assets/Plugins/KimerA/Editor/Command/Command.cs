#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace KimerA.Editor.Command
{
    /// <summary>
    /// Marks a class as a custom command for the command system.
    /// <para>
    /// This attribute should be used on classes that implement the ICustomCommand interface.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class CustomCommandAttribute : Attribute
    {
        public string Name { get; }
        public string? Description { get; }

        public CustomCommandAttribute(string name, string? description = null)
        {
            Name = name;
            Description = description;
        }
    }

    /// <summary>
    /// Marks a class as a sub-command of another command.
    /// <para>
    /// This attribute should be used on classes that implement the ICustomCommand interface.
    /// It indicates that the class is a sub-command of the command specified by the generic type parameter.
    /// </para>
    /// </summary>
    /// <typeparam name="TCmd"></typeparam>
    public sealed class SubCommandOfAttribute<TCmd> : Attribute where TCmd : ICustomCommand
    {
    }

    public interface ICustomCommand
    {
        /// <summary>
        /// Executes the command with the provided arguments and options.
        /// </summary>
        /// <param name="args">The arguments for the command.</param>
        /// <param name="options">The options for the command.</param>
        void Execute(string[] args, Dictionary<string, string> options);
    }

    public class Command
    {
        public readonly string Name;
        public string? Description { get; set; }
        public Dictionary<string, Command> SubCommands { get; } = new();
        public List<Argument> Arguments { get; } = new();
        public List<Option> Options { get; } = new();

        public Command(string name, string? description = null)
        {
            Name = name;
            Description = description;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Command WithSubCommand(Command subCommand)
        {
            SubCommands[subCommand.Name] = subCommand;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Command WithOption(string name, string? shortName = null, string? description = null, bool isRequired = false)
        {
            Options.Add(new Option(name, shortName, description, isRequired));
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Command WithArgument(string name, string? description, bool isRequired = false)
        {
            Arguments.Add(new Argument(name, description, isRequired));
            return this;
        }
    }

    public class Option
    {
        public readonly string Name;
        public string? ShortName { get; set; }
        public string? Description { get; set; }
        public List<Argument> Arguments { get; } = new();
        public bool IsRequired { get; set; }

        public Option(string name, string? shortName = null, string? description = null, bool isRequired = false)
        {
            Name = name;
            ShortName = shortName;
            Description = description;
            IsRequired = isRequired;
        }
    }

    public class Argument
    {
        public readonly string Name;
        public string? Description { get; set; }
        public bool IsRequired { get; set; }

        public Argument(string name, string? description = null, bool isRequired = false)
        {
            Name = name;
            Description = description;
            IsRequired = isRequired;
        }
    }
}
#endif