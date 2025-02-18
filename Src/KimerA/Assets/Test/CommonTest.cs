using System.Runtime.CompilerServices;
using UnityEngine;

[module: SkipLocalsInit]

record Person(string FirstName, string LastName);

struct Point
{
    public int X { get; init; }
    public int Y { get; init; }

    public Point() => (X, Y) = (1, 1);
}

sealed class CommonTest : MonoBehaviour
{
    public required int? Value { get; init; }

    private void Start()
    {
        var numbers = new int[] { 1, 2, 3, 4, 5 };

        int secondToLast = numbers[^2];
        Debug.Log(secondToLast);
        var firstFour = numbers[..4];
        Debug.Log(string.Join(", ", firstFour));

        if (numbers is [_, 2, int third, .. var rest])
        {
            Debug.Log($"Third: {third}, Rest: {string.Join(", ", rest)}");
        }
    }
}