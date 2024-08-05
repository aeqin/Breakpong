using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FlavorTextWorld : FlavorTextBase
{
    protected override void Awake()
    {
        c_TMP = GetComponent<TextMeshPro>(); // World space

        base.Awake();
    }

    /// <summary>
    /// Initialize a FlavorText with a flavor (animation) to display
    /// </summary>
    public override void Initialize(string _displayText, Color _textColor, float _textSize = 500f, float _lifetime = 2f, bool _doFade = true)
    {
        base.Initialize(_displayText, _textColor, _textSize, _lifetime, _doFade);
    }
}
