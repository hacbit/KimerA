#if UNITY_EDITOR

using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace KimerA.Editor.Command
{   
    public enum CommandTokenType
    {
        /// <summary>
        /// An identifier token. Likes `my_command`, `test`, etc.
        /// </summary>
        Ident,
        /// <summary>
        /// An option token. Likes `-h`, `--help`, etc.
        /// </summary>
        Option,
        /// <summary>
        /// A value token. Likes `"hello world"`, `123`, "true", etc.
        /// </summary>
        Value,
    }

    /// <summary>
    /// Represents a token in a command line string.
    /// </summary>
    public readonly ref struct CommandToken
    {
        public CommandToken(ref string input, CommandTokenType type)
        {
            Value = input.AsSpan();
            Type = type;
        }

        public CommandToken(ReadOnlySpan<char> input, CommandTokenType type)
        {
            Value = input;
            Type = type;
        }

        public readonly CommandTokenType Type;

        private readonly ReadOnlySpan<char> Value;

        public static CommandToken Empty => new(ReadOnlySpan<char>.Empty, CommandTokenType.Ident);

        public bool IsEmpty => Value.IsEmpty;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<char> AsRef() => Value;

        public override string ToString()
        {
            switch (Type)
            {
                case CommandTokenType.Ident:
                case CommandTokenType.Option:
                    return Value.ToString();
                case CommandTokenType.Value:
                    // process the escape characters
                    // allow \" \\ \n \t \r
                    var escape = false;
                    var value = new StringBuilder(Value.Length);
                    foreach (var c in Value)
                    {
                        if (c is '\\' && escape is false)
                        {
                            escape = true;
                            continue;
                        }
                        if (escape)
                        {
                            escape = false;
                            if (c is '\n' or '\r') continue;
                            var ch = c switch
                            {
                                '"' => '"',
                                '\\' => '\\',
                                'n' => '\n',
                                't' => '\t',
                                'r' => '\r',
                                _ => c
                            };
                            value.Append(ch);
                        }
                        else
                        {
                            value.Append(c);
                        }
                    }
                    return value.ToString();
                default:
                    return string.Empty;
            }
        }
    }

    /// <summary>
    /// Represents a stream of command tokens.
    /// <para>
    /// The stream will tokenize the input string.
    /// </para>
    /// <para>
    /// The stream will automatically find the next token when calling <see cref="NextAuto"/>.
    /// </para>
    /// <para>
    /// Or you can specify the token type by calling <see cref="Next"/>.
    /// </para>
    /// </summary>
    public ref struct CommandTokenStream
    {
        public CommandTokenStream(ref string input)
        {
            m_Input = input;
            Current = (-1, -1);
        }

        public CommandTokenStream(ReadOnlySpan<char> input)
        {
            m_Input = input;
            Current = (-1, -1);
        }

        public static CommandTokenStream Empty => new(string.Empty);

        private readonly ReadOnlySpan<char> m_Input;

        /// <summary>
        /// The (start, end) index of current token.
        /// </summary>
        private (int Start, int End) Current;

        public readonly bool IsEnd => Current.End >= m_Input.Length - 1;

        /// <summary>
        /// Find the next token.
        /// </summary>
        /// <returns></returns>
        private (int, int) FindNext()
        {
            var start = Current.End + 1;
            if (start >= m_Input.Length) return (-1, -1);
            var end = -1;
            var escape = false;
            for (var idx = start; idx < m_Input.Length; ++idx)
            {
                var c = m_Input[idx];
                if (c is '\\' && escape is false)
                {
                    escape = true;
                    continue;
                }
                if (escape)
                {
                    if (c is '\n' or '\r') continue;
                    else escape = false;
                }
                if (char.IsWhiteSpace(c) is false)
                {
                    if (end is -1) start = idx;
                    end = idx + 1;
                }
                else if (end is not -1) break;
            }
            // try add the last token
            if (end is -1) end = m_Input.Length;
            return (start, end);
        }

        private bool MoveNext()
        {
            if (IsEnd) return false;
            Current = FindNext();
            if (Current.Start is -1) return false;
            return true;
        }

        public CommandToken NextAuto()
        {
            if (MoveNext() is false) return CommandToken.Empty;
            var (start, end) = Current;
            var token = m_Input[start..end];
            if (token[0] is '-' && token.Length > 1) return ParseOption();
            if (char.IsLetter(token[0]) || token[0] is '_') return ParseIdent();
            return ParseValue();
        }

        /// <summary>
        /// Get the next token by the specified TokenType.
        /// 
        /// <para>
        /// Thrown when the token type is invalid, or specified unknown token type.
        /// </para>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="TokenizeException"></exception>
        public CommandToken Next(CommandTokenType type)
        {
            if (IsEnd) return CommandToken.Empty;
            var token = type switch
            {
                CommandTokenType.Ident => ParseIdent(),
                CommandTokenType.Option => ParseOption(),
                CommandTokenType.Value => ParseValue(),
                _ => throw new TokenizeException($"Invalid token type '{type}'")
            };
            return token;
        }

        /// <summary>
        /// Parse an identifier token, likes `my_command`, `test`, etc.
        /// <para>
        /// The identifier must start with a letter or `_`, and followed by letters, digits or `_`.
        /// </para>
        /// </summary>
        /// <returns>
        /// A token represents the identifier.
        /// </returns>
        /// <exception cref="TokenizeException"></exception>
        private readonly CommandToken ParseIdent()
        {
            var (start, end) = Current;
            var ident = m_Input[start..end];
            // validate the ident
            // the ident must start with a letter or `_`
            if (char.IsLetter(ident[0]) is false && ident[0] is not '_') goto InvalidIdent;
            foreach (var c in ident[1..])
            {
                // the following characters must be letters, digits or `_`
                if (char.IsLetterOrDigit(c) is false && c is not '_') goto InvalidIdent;
            }
            return new CommandToken(ident, CommandTokenType.Ident);

            InvalidIdent:
                throw new TokenizeException($"Invalid identifier '{ident.ToString()}'");
        }

        /// <summary>
        /// Parse an option token, likes `-h`, `--help`, `--set-value`, etc.
        /// <para>
        /// The option token must start with `-` or `--`, and followed by a letter or digit or `-`.
        /// </para>
        /// <para>
        /// The short option must be a single character, likes `-h`, `-v`, etc.
        /// </para>
        /// </summary>
        /// <returns>
        /// A token represents the option. The value of the token will be the option name (ignore the `-` or `--`).
        /// Likes `h`, `help`, `set-value`, etc.
        /// </returns>
        /// <exception cref="TokenizeException"></exception>
        private readonly CommandToken ParseOption()
        {
            var (start, end) = Current;
            var option = m_Input[start..end];
            // validate the option
            if (option[0] is not '-') goto InvalidOption;
            if (option.Length is 1) goto InvalidOption;
            // short option must be a single character, likes `-h`, `-v`, etc.
            if (option.Length > 2 && option[1] is not '-') goto InvalidOption;
            if (option.Length > 2)
            {
                // long option, likes `--help`, `--version`, `--set-value`, etc.
                // the option must be a letter or digit or `-`
                foreach (var c in option[2..])
                {
                    if (char.IsLetterOrDigit(c) is false && c is not '-') goto InvalidOption;
                }
                // return the long option (ignore the `--`)
                return new CommandToken(option[2..], CommandTokenType.Option);
            }
            // return the short option (ignore the `-`)
            return new CommandToken(option[1..], CommandTokenType.Option);

            InvalidOption:
                throw new TokenizeException($"Invalid option '{option.ToString()}'");
        }

        /// <summary>
        /// Parse a value token, likes `"hello world"`, `123`, `true`, etc.
        /// <para>
        /// If the value is starts with `"`, then the value must be enclosed by `"`.
        /// And the value can contains escape characters, likes `\"`, `\\`, etc.
        /// </para>
        /// </summary>
        /// <returns>
        /// A token represents the value.
        /// </returns>
        /// <exception cref="TokenizeException"></exception>
        private CommandToken ParseValue()
        {
            var (start, end) = Current;
            var value = m_Input[start..end];
            // parse the value, likes `"hello world"`, `123`, `true`, etc.
            // but in this implementation, we only parse the string value.
            // type conversion should be responsible by the caller.

            if (value.Length < 2 && value[0] is '"') goto UnclosedString;
            if (value[0] is '"')
            {
                while (value[^1] is not '"')
                {
                    if (MoveNext() is false) goto UnclosedString;
                    var (_, new_end) = Current;
                    // ignore the start and end quote
                    value = m_Input[start..new_end];
                }
                // remove the start and end quote
                value = value[1..^1];
            }
            // check if contains unescaped '"' in the middle of the value
            var escape = false;
            foreach (var c in value)
            {
                if (c is '\\' && escape is false)
                {
                    escape = true;
                    continue;
                }
                if (c is '"' && escape is false) goto InvalidValue;
                escape = false;
            }
            // check if the last character is an escape character
            if (escape) goto InvalidValue;
            return new CommandToken(value, CommandTokenType.Value);

            UnclosedString:
                throw new TokenizeException($"Unclosed string '{value.ToString()}'");

            InvalidValue:
                throw new TokenizeException($"Invalid value '{value.ToString()}'");
        }

        public readonly string ToDebugString()
        {
            var sb = new StringBuilder();
            var stream = new CommandTokenStream(m_Input);
            while (true)
            {
                var token = stream.NextAuto();
                if (token.IsEmpty) break;
                sb.AppendLine($"Token: {token.Type} => {token.ToString()}");
            }
            return sb.ToString();
        }
    }

    public class TokenizeException : Exception
    {
        public TokenizeException(string message) : base(message)
        {
        }
    }
}
#endif