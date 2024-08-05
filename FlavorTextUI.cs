using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FlavorTextUI : FlavorTextBase
{
    protected override void Awake()
    {
        c_TMP = GetComponent<TextMeshProUGUI>(); // Screen space

        base.Awake();
    }

    /// <summary>
    /// Initialize a FlavorText with a flavor (animation) to display
    /// </summary>
    public override void Initialize(string _displayText, Color _textColor, float _textSize = 48f, float _lifetime = -1f, bool _doFade = false)
    {
        base.Initialize(_displayText, _textColor, _textSize, _lifetime, _doFade);
    }
}
