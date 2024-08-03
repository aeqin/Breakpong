using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LineRendererDotted : MonoBehaviour
{
    private LineRenderer c_lineRenderer;

    private Color lineColor;
    private float lineWidth = 10f;
    private Vector2 startPos;
    private Vector2 endPos;
    private bool doDraw = false;

    void Awake()
    {
        c_lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (!doDraw) return;

        // Scale material texture by line width
        float _width = c_lineRenderer.startWidth;
        c_lineRenderer.material.mainTextureScale = new Vector2(1f / _width, 1f);
    }

    /*********************************************************************************************************************************************************************************
     * Public Methods
     *********************************************************************************************************************************************************************************/
    #region Public Methods
    /// <summary>
    /// Create a dotted line stretching between two given points
    /// </summary>
    public void Initialize(Vector2 _startPos, Vector2 _endPos, Color _lineColor, float _lineWidth = 10f, bool _fade = false)
    {
        lineColor = _lineColor;
        lineWidth = _lineWidth;
        startPos = _startPos;
        endPos = _endPos;

        c_lineRenderer.startColor = _lineColor;
        c_lineRenderer.endColor = _lineColor;
        c_lineRenderer.startWidth = lineWidth;
        c_lineRenderer.positionCount = 2;
        c_lineRenderer.SetPosition(0, startPos);
        c_lineRenderer.SetPosition(1, endPos);

        if (_fade) // Fade dotted line towards its end
        {
            c_lineRenderer.endColor = new Color(_lineColor.r, _lineColor.g, _lineColor.b, 0.0f);
        }

        doDraw = true;
    }

    /// <summary>
    /// Update start position of dotted line
    /// </summary>
    public void UpdateStartPos(Vector2 _startPos)
    {
        c_lineRenderer.SetPosition(0, _startPos);
    }

    /// <summary>
    /// Update end position of dotted line
    /// </summary>
    public void UpdateEndPos(Vector2 _endPos)
    {
        c_lineRenderer.SetPosition(1, _endPos);
    }
    #endregion
}
