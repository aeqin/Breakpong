using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Globals : MonoBehaviour
{}

/// <summary>
/// Global enum of tags, to disallow misspellings and allow gameObjects to have multiple tags
/// </summary>
[System.Flags] public enum Tag
{
    None = 0,
    Ball = 1 << 0,
    Paddle = 1 << 1,
    Brick = 1 << 2,
    Boundary = 1 << 3,
    Deathzone = 1 << 4,
    // = 1 << 5,
    // = 1 << 6,
    // = 1 << 7,
    // = 1 << 8,
    // = 1 << 9,
}

/// <summary>
/// Class to hold a related bundle of integer variables. Current value, min and max. Never go beyond min or max.
/// </summary>
public class LimitInt
{
    public int curr;
    public int max;
    public int min;

    public LimitInt(int _curr, int _min, int _max)
    {
        min = _min;
        max = _max;

        curr = _curr;
    }

    public void Increment()
    {
        if (curr < max)
            curr++;
    }

    public void Decrement()
    {
        if (curr > min)
            curr--;
    }

    public void resetToMin()
    {
        curr = min;
    }

    public void resetToMax()
    {
        curr = max;
    }
}
