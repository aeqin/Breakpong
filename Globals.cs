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

    /// <summary>
    /// Increment curr until max
    /// </summary>
    public void Increment()
    {
        if (curr < max)
            curr++;
    }

    /// <summary>
    /// Decrement curr until min
    /// </summary>
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

    public bool isMin()
    {
        return curr == min;
    }

    public bool isMax()
    {
        return curr == max;
    }

    /// <summary>
    /// Returns the current value percentage of max value
    /// </summary>
    public float GetPercentage()
    {
        return (float)curr / max;
    }
}

/// <summary>
/// Class to hold a related bundle of float variables. Current value, min and max. Never go beyond min or max.
/// </summary>
public class LimitFloat
{
    public float curr;
    public float max;
    public float min;

    public LimitFloat(float _curr, float _min, float _max)
    {
        min = _min;
        max = _max;

        curr = _curr;
    }

    /// <summary>
    /// Increment curr by given value, clamped to min & max
    /// </summary>
    public void IncrementBy(float _plus)
    {
        curr = Mathf.Clamp(curr + _plus, min, max);
    }

    /// <summary>
    /// Decrement curr by given value, clamped to min & max
    /// </summary>
    public void DecrementBy(float _minus)
    {
        curr = Mathf.Clamp(curr - _minus, min, max);
    }

    public void resetToMin()
    {
        curr = min;
    }

    public void resetToMax()
    {
        curr = max;
    }

    public bool isMin()
    {
        return curr == min;
    }

    public bool isMax()
    {
        return curr == max;
    }

    /// <summary>
    /// Returns the current value percentage of max value
    /// </summary>
    public float GetPercentage()
    {
        return curr / max;
    }
}
