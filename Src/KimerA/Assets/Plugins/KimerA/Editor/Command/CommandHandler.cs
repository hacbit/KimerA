#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace KimerA.Editor.Command
{
    public sealed class CommandHandler
    {
        public static CommandHandler Instance { get; } = new();

        private Dictionary<string, Command> m_registeredCommands = new();

        private void LoadCommands()
        {
            m_registeredCommands.Clear();

            var commandTypes = TypeCache.GetTypesWithAttribute<CustomCommandAttribute>();
            foreach (var type in commandTypes)
            {
                var customCommandAttr = type.GetCustomAttribute<CustomCommandAttribute>();
                var name = customCommandAttr.Name;
                var description = customCommandAttr.Description;
                var command = new Command(name, description);

                var subCommandAttr = type.GetCustomAttribute<SubCommandOfAttribute<ICustomCommand>>();
                if (subCommandAttr != null)
                {
                    var parentCmdType = subCommandAttr.GetType().GetGenericArguments()[0];
                    if (TryFindParentCommand(parentCmdType, out var parentCommand))
                    {
                        parentCommand?.WithSubCommand(command);
                    }
                    else
                    {
                        Debug.LogError($"Parent command '{parentCmdType.Name}' not found for subcommand '{name}'.");
                    }
                }
                else
                {
                    if (m_registeredCommands.TryGetValue(name, out var existingCommand))
                    {
                        Debug.LogError($"Command '{name}' is already defined by '{existingCommand.GetType().Name}' class.");
                    }
                    else
                    {
                        m_registeredCommands[name] = command;
                    }
                }
            }
        }

        private const int MaxDepth = 5; // Prevent infinite recursion

        private bool TryFindParentCommand(Type parentCommandType, out Command? parentCommand, int depth = 0)
        {
            if (depth >= MaxDepth)
            {
                parentCommand = null;
                return false;
            }
            var customCommandAttr = parentCommandType.GetCustomAttribute<CustomCommandAttribute>();
            var parentCommandName = customCommandAttr.Name;
            var subCommandAttr = parentCommandType.GetCustomAttribute<SubCommandOfAttribute<ICustomCommand>>();
            if (subCommandAttr is not null)
            {
                var parentCmdType = subCommandAttr.GetType().GetGenericArguments()[0];
                return TryFindParentCommand(parentCmdType, out parentCommand, depth + 1) &&
                    parentCommand?.SubCommands.TryGetValue(parentCommandName, out parentCommand) is true;
            }
            else
            {
                // found parent command
                return m_registeredCommands.TryGetValue(parentCommandName, out parentCommand);
            }
        }
    }
}

#endif