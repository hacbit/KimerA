#if UNITY_EDITOR

using System;

namespace KimerA.Editor.Utils;

public ref struct PathWalker
{
    public readonly ReadOnlySpan<char> Path;

    public PathWalker(string path)
    {
        Path = path;
        right = 0;
    }

    public readonly ReadOnlySpan<char> Current => Path[..right];

    private int right;

    private static readonly char Seperator = '/';

    public bool MoveNext()
    {
        if (right >= Path.Length)
        {
            right = Path.Length;
            return false;
        }
        var next = Path[right..].IndexOf(Seperator);
        right = next > 0 ? right + next : Path.Length;
        return true;
    }
}

#endif