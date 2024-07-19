using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentTag : MonoBehaviour
{
    [SerializeField] private Tag tags; // What Tags are this GameObject a part of

    /// <summary>
    /// Checks if parent GameObect is a part of Tag
    /// </summary>
    /// <param name="tagToCheck">The Tag to check</param>
    /// <returns>Whether parent GameObject is considered this Tag</returns>
    public bool IsOfTag(Tag tagToCheck)
    {
        return tags.HasFlag(tagToCheck);
    }

    /// <summary>
    /// Checks if parent GameObect is NOT a part of Tag
    /// </summary>
    /// <param name="tagToCheck">The Tag to check</param>
    /// <returns>Whether parent GameObject is NOT considered this Tag</returns>
    public bool IsNotTag(Tag tagToCheck)
    {
        return !(tags.HasFlag(tagToCheck));
    }
}
