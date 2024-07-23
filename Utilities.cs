using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class Utilities
{
    /// <summary>
    /// Given a value within a range, return that value linearly mapped to a second range
    /// </summary>
    /// <param name="val">Value to map</param>
    /// <param name="val_range_start">Start range of value to map</param>
    /// <param name="val_range_end">End range of value to map</param>
    /// <param name="other_range_start">Start range to map value to</param>
    /// <param name="other_range_end">End range to map value to</param>
    /// <returns>A value linearly mapped to a second range</returns>
    public static float MapToRange(float val, float val_range_start, float val_range_end, float other_range_start, float other_range_end)
	{
		float ratio = (other_range_end - other_range_start) / (val_range_end - val_range_start);
		return ratio * (val - val_range_start) + other_range_start;
    }

    /// <summary>
    /// Given a value, return the sign of the value, or 0 if the value is 0
    /// </summary>
    /// <param name="val">Value to get the sign of</param>
    /// <returns>The sign of value, or 0</returns>
    public static float Sign(float val)
    {
        return val < 0 ? -1 : (val > 0 ? 1 : 0);
    }

    /// <summary>
    /// Given a float, return a Vector2 where x and y components are the float
    /// </summary>
    public static Vector2 Vec2FromFloat(float _f)
    {
        return new Vector2(_f, _f);
    }

    /// <summary>
    /// Given two bounds, return whether the first bounds in COMPLETELY within the second bounds
    /// </summary>
    public static bool IsBoundsInsideBounds(Bounds _candy, Bounds _container)
    {
        return _container.Contains(_candy.min) && _container.Contains(_candy.max); // Consider within if bottom-left and top-right corner are both contained
    }

    /// <summary>
    /// Given a bounds, draw lines to represent its edges
    /// </summary>
    public static void DrawBounds2D(Bounds _bounds, Color _color, float _lifetime = 5f)
    {
        var bot_left = new Vector3(_bounds.min.x, _bounds.min.y, 0);
        var left_top = new Vector3(_bounds.min.x, _bounds.max.y, 0);
        var top_right = new Vector3(_bounds.max.x, _bounds.max.y, 0);
        var right_bot = new Vector3(_bounds.max.x, _bounds.min.y, 0);
        

        Debug.DrawLine(bot_left, left_top, _color, _lifetime);
        Debug.DrawLine(left_top, top_right, _color, _lifetime);
        Debug.DrawLine(top_right, right_bot, _color, _lifetime);
        Debug.DrawLine(right_bot, bot_left, _color, _lifetime);
    }

    /// <summary>
    /// Returns 50% chance to be true, 50% chance to be false
    /// </summary>
    public static bool FlipACoin()
    {
        return (new System.Random().Next(2) == 0);
    }
}
