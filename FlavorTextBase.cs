using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FlavorTextBase : MonoBehaviour
{
    protected TMP_Text c_TMP;

    // Lifetime variables
    protected bool f_hasLifetime = false;
    protected LimitFloat li_lifetime = new LimitFloat(2, 0, 2); // How long should FlavorText live?

    // Fade variables
    protected bool f_doFade = false;
    protected Gradient fadeGradiant;

    // GrowFlash variables
    protected bool f_isGrowing = false;
    [SerializeField] protected AnimationCurve growCurve;
    protected IEnumerator growCoroutine;
    protected float growDuration = 0.3f;
    protected float growOvershoot = 0.25f; // When growing, how much to overshoot by, before going back to scale 1.0

    // Flash variables
    protected bool f_isFlashing = false;
    protected IEnumerator flashCoroutine;
    protected Color baseTextColor = Color.white;
    protected float flashDuration = 0.1f;

    protected virtual void Awake()
    {
        // Is this a TextMesh in World space (TextMeshPro), or in Screen space (TextMeshProUGUI)?
        // Override and assign [c_TMP] in children.

        InitializeGradients();
    }

    protected void Update()
    {
        // If doFade, slightly fade text color as lifetime decreases
        if (f_doFade)
        {
            c_TMP.faceColor = fadeGradiant.Evaluate(1f - li_lifetime.GetPercentage());
        }

        // Decrement lifetime timer
        if (f_hasLifetime)
        {
            li_lifetime.DecrementBy(Time.deltaTime);
            if (li_lifetime.isMin()) DestroyFlavorText(); // Destroy text after lifetime reached
        }

        // Render text mesh
        c_TMP.ForceMeshUpdate(); // Redraw the TextMeshPro's meshes every frame
    }

    /// <summary>
    /// Initialize a FlavorText with a flavor (animation) to display
    /// </summary>
    public virtual void Initialize(string _displayText, Color _textColor, float _textSize = 500f, float _lifetime = -1f, bool _doFade = true)
    {
        c_TMP.fontSize = _textSize;
        c_TMP.color = _textColor;
        c_TMP.SetText(_displayText);

        baseTextColor = _textColor;
        f_hasLifetime = (_lifetime > 0f);
        li_lifetime = new LimitFloat(_lifetime, 0, _lifetime);
        f_doFade = _doFade;
    }

    /*********************************************************************************************************************************************************************************
     * Private Methods
     *********************************************************************************************************************************************************************************/
    #region Private Methods
    #endregion

    /*********************************************************************************************************************************************************************************
     * Protected Methods
     *********************************************************************************************************************************************************************************/
    #region Protected Methods
    /// <summary>
    /// Initialize Gradients to use
    /// </summary>
    protected void InitializeGradients()
    {
        fadeGradiant = new Gradient();
        fadeGradiant.SetKeys
        (
            new GradientColorKey[]
            {
                new GradientColorKey(Color.white, 0.0f), // Stay the
                new GradientColorKey(Color.white, 1.0f), // same color
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f), // Start of full color (no transparency)
                new GradientAlphaKey(1f, 0.6f), // End of full color (no transparency)
                new GradientAlphaKey(0f, 1f), // Fade towards transparency
            }
        );
    }

    /// <summary>
    /// Coroutine that moves the next LifeBall into the level bounds
    /// </summary>
    protected IEnumerator CR_GrowText(Vector3 _offsetToGrowFrom, float _overshootRatio, AnimationCurve _curve, float _duration, bool _growFromPoint)
    {
        // ### BEFORE
        f_isGrowing = true;

        int _curvePeakKeyIndex = Utilities.GetCurvePeakKeyAndValue(_curve).Item1;
        _curve.keys[_curvePeakKeyIndex].value = 1f + _overshootRatio;

        Vector3 _originPoint = Vector3.zero + _offsetToGrowFrom; // Origin point for EVERY vertex to grow from
        Vector3[] _charVertices = new Vector3[c_TMP.mesh.vertexCount]; // Array of vertices to modify
        TMP_TextInfo _textInfo = c_TMP.textInfo;

        // ### DURING
        float _linearProgress = 0f; // Equal step per frame
        float _nonLinearProgress; // Potentially different step per frame (ease-in ease-out)
        while (_linearProgress < 1)
        {
            // Update progress
            _linearProgress += Time.deltaTime / _duration;
            _nonLinearProgress = _curve.Evaluate(_linearProgress);

            // Loop through each character in text
            foreach (TMP_CharacterInfo _charInfo in _textInfo.characterInfo)
            {
                // Loop through each of the 4 vertices of the character
                for (int _vertexNum = 0; _vertexNum < 4; _vertexNum++)
                {
                    int _charVertexIndex = _charInfo.vertexIndex + _vertexNum; // Index of particular vertex in array of all text vertices

                    // Move vertex from origin towards position
                    if (_growFromPoint)
                    {
                        Vector3 _endPos = c_TMP.mesh.vertices[_charVertexIndex];
                        _charVertices[_charVertexIndex] = Vector3.Lerp(_originPoint, _endPos, _nonLinearProgress);
                    }

                    // Scale vertex (for a bit of growth past 1.0 scale)
                    if (_nonLinearProgress > 1.0) _charVertices[_charVertexIndex] *= _nonLinearProgress;
                }
            }

            // Set TextMeshPro's mesh to be the modified vertex array
            c_TMP.textInfo.meshInfo[0].vertices = _charVertices;
            c_TMP.UpdateVertexData();

            yield return null;
        }

        // ### AFTER
        f_isGrowing = false;
    }

    /// <summary>
    /// Coroutine that flashes FlavorText to given color, then back
    /// </summary>
    protected IEnumerator CR_FlashText(Color _baseColor, Color _flashColor, float _duration)
    {
        f_isFlashing = true; // Start flash flag

        Color32[] _charColors = new Color32[c_TMP.mesh.vertexCount]; // Array of color vertices to modify
        TMP_TextInfo _textInfo = c_TMP.textInfo;

        // ### DURING
        float _linearProgress = 0f; // Equal step per frame
        while (_linearProgress < 1)
        {
            // Update progress
            _linearProgress += Time.deltaTime / _duration;

            // Loop through each character in text
            foreach (TMP_CharacterInfo _charInfo in _textInfo.characterInfo)
            {
                // Loop through each of the 4 vertices of the character
                for (int _vertexNum = 0; _vertexNum < 4; _vertexNum++)
                {
                    // Move vertex color towards flash color, then back
                    int _charVertexIndex = _charInfo.vertexIndex + _vertexNum; // Index of particular vertex in array of all text vertices
                    if (_linearProgress > 0.5) // 50% progress
                    {
                        float _normalizedProgress = Utilities.MapToRange(_linearProgress, 0.5f, 1f, 0f, 1f);
                        _charColors[_charVertexIndex] = Color.Lerp(_flashColor, _baseColor, _normalizedProgress);
                    }
                    else
                    {
                        float _normalizedProgress = Utilities.MapToRange(_linearProgress, 0f, 0.5f, 0f, 1f);
                        _charColors[_charVertexIndex] = Color.Lerp(_baseColor, _flashColor, _normalizedProgress);
                    }
                }
            }

            // Set TextMeshPro's mesh to be the modified vertex array
            c_TMP.textInfo.meshInfo[0].colors32 = _charColors;
            c_TMP.UpdateVertexData();

            yield return null;
        }

        f_isFlashing = false; // Finish flash flag
    }
    #endregion

    /*********************************************************************************************************************************************************************************
     * Public Methods
     *********************************************************************************************************************************************************************************/
    #region Public Methods
    /// <summary>
    /// Change text displayed by this FlavorText
    /// </summary>
    public void ChangeText(string _txt)
    {
        c_TMP.SetText(_txt);
    }

    /// <summary>
    /// Change color displayed by this FlavorText
    /// </summary>
    public void ChangeColor(Color _color)
    {
        c_TMP.color = _color;
    }

    /// <summary>
    /// Starts Coroutine that grows text from an offset
    /// </summary>
    public void GrowText(Vector3 _growOffset, float _growDuration = -1f, float _overshootRatio = -1f, bool _growFromPoint = true)
    {
        if (_growDuration < 0f) _growDuration = growDuration;
        if (_overshootRatio < 0f) _overshootRatio = growOvershoot;

        if (f_isGrowing) StopCoroutine(growCoroutine);

        growCoroutine = CR_GrowText(_growOffset, _overshootRatio, growCurve, _growDuration, _growFromPoint);
        StartCoroutine(growCoroutine);
    }

    /// <summary>
    /// Flashes text
    /// </summary>
    public void FlashText(Color _flashColor, float _flashDuration = -1f)
    {
        if (_flashDuration < 0f) _flashDuration = flashDuration;

        if (f_isFlashing) StopCoroutine(flashCoroutine);

        flashCoroutine = CR_FlashText(baseTextColor, _flashColor, _flashDuration);
        StartCoroutine(flashCoroutine);
    }

    /// <summary>
    /// Starts Coroutine that grows AND flashes text
    /// </summary>
    public void GrowAndFlashText(Vector3 _growOffset, Color _flashColor, float _growAndFlashDuration = -1f, float _overshootRatio = -1f, bool _growFromPoint = true)
    {
        if (_growAndFlashDuration < 0f) _growAndFlashDuration = flashDuration;
        if (_overshootRatio < 0f) _overshootRatio = growOvershoot;

        GrowText(_growOffset, _growDuration: _growAndFlashDuration, _overshootRatio: _overshootRatio, _growFromPoint: _growFromPoint);
        FlashText(_flashColor, _growAndFlashDuration);
    }

    /// <summary>
    /// Destroy FlavorText
    /// </summary>
    public void DestroyFlavorText()
    {
        Destroy(gameObject);
    }
    #endregion

    /*********************************************************************************************************************************************************************************
     * On Event Methods
     *********************************************************************************************************************************************************************************/
    #region On Event Methods
    #endregion
}
