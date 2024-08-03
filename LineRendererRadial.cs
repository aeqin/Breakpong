using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class LineRendererRadial : MonoBehaviour
{
    [SerializeField] private LineRenderer pf_line; // GameObject of a LineRenderer (with use WorldSpace == false) to instantiate for each arc of a RadialLineRenderer

    private List<LineRenderer> list_lines = new List<LineRenderer>();

    /*********************************************************************************************************************************************************************************
     * Private Methods
     *********************************************************************************************************************************************************************************/
    #region Private Methods
    /// <summary>
    /// Returns a color based on given percentage. (Green at full, yellow below 50%, red below 20%)
    /// </summary>
    private Color GetColorFromPercent(float _percent)
    {
        if (_percent >= 0.5) return Color.green;

        if (_percent <= 0.2) return Color.red;

        return Color.yellow;
    }

    /// <summary>
    /// Instantiates and returns a LineRenderer
    /// </summary>
    private LineRenderer CreateLineRenderer(float width, bool doLoop = true)
    {
        LineRenderer _line = Instantiate(pf_line, transform);
        _line.startWidth = width;

        if (doLoop)
        {
            _line.loop = true;
        }

        list_lines.Add(_line);

        return _line;
    }

    /// <summary>
    /// Turn a given LineRenderer into an arc by appending points that follow an arc
    /// </summary>
    private void ArcifyLineRenderer(LineRenderer _arc, float _radius, int smoothness, float _angleStart, float _angleEnd)
    {
        _arc.positionCount = smoothness + 1; // Number of points in the arc (+1 extra for end point)
        for (int _pt = 0; _pt < smoothness; _pt++)
        {
            float _angleAtPoint = _angleStart + _pt * (_angleEnd - _angleStart) / smoothness; // Increase angle each point
            Vector3 _pointPos = new Vector3(Mathf.Cos(_angleAtPoint) * _radius, Mathf.Sin(_angleAtPoint) * _radius, 0f); // Calculate world position from radius and increasing angle
            _arc.SetPosition(_pt, _pointPos);
        }
        _arc.SetPosition(smoothness, new Vector3(Mathf.Cos(_angleEnd) * _radius, Mathf.Sin(_angleEnd) * _radius, 0f)); // Add the end point
    }
    #endregion

    /*********************************************************************************************************************************************************************************
     * Public Methods
     *********************************************************************************************************************************************************************************/
    #region Public Methods
    /// <summary>
    /// Draw a circular progress bar by creating LineRenderer's for each segment arc.
    /// </summary>
    public void DrawRadialBar(float _radius, int _segments = 1, float _gapSizeAngle = Mathf.PI/32, float _width = 8f, float _angleStart = 0f, int _smoothness = 99)
    {
        // Clear, if any, already drawn LineRenderers
        ClearRadialBar();

        // Check gap size
        if (_segments == 1)
        {
            _gapSizeAngle = 0f; // 1 segment, no gap size
        }
        else if (_segments > 1)
        {
            if (_gapSizeAngle * _segments > Utilities.TAU)
            {
                Debug.LogErrorFormat("RadialLineRenderer:DrawRadialBar(), _gapSizeAngle of {0} is too big to support {1} number of segments", _gapSizeAngle, _segments);
            }
        }
        else
        {
            return; // Zero or negative segments cannot be drawn
        }

        // For each segment, draw an arc
        bool _doLoop = (_segments > 1) ? false : true; // Only loop if no gaps
        float _angleOfArcPerSegment = (Utilities.TAU / _segments) - _gapSizeAngle; // Size of each arc segment
        float _currAngle = _angleStart + (_gapSizeAngle / 2f); // Start of circle
        for (int _segment = 0; _segment < _segments; _segment++)
        {
            LineRenderer _arc = CreateLineRenderer(_width, _doLoop);
            ArcifyLineRenderer(_arc, _radius, _smoothness, _currAngle, _currAngle + _angleOfArcPerSegment);

            _currAngle = _currAngle + _angleOfArcPerSegment + _gapSizeAngle; // Skip over a gap, then next start of arc
        }
    }

    /// <summary>
    /// Destroy the drawn circular progress bar by destroying each LineRenderer segment arc.
    /// </summary>
    public void ClearRadialBar()
    {
        foreach (LineRenderer _line in list_lines)
        {
            Destroy(_line.gameObject);
        }
        list_lines.Clear();
    }

    /// <summary>
    /// Update the currently drawn RadialLineRenderer with progress
    /// </summary>
    /// <param name="_percent">Percentage of bar that should be visible</param>
    public void UpdateRadialBarByPercent(float _percent)
    {
        if (list_lines.Count == 0) return; // Nothing to update

        // Single bar (modify LineRenderer's Gradient so that unrepresented progress becomes invisible)
        if (list_lines.Count == 1)
        {
            LineRenderer _line = list_lines[0];
            Gradient _gradient = new Gradient();
            _gradient.SetKeys
            (
                new GradientColorKey[]
                {
                    new GradientColorKey(GetColorFromPercent(_percent), 0.0f), // Stay the
                    new GradientColorKey(GetColorFromPercent(_percent), 1.0f), // same color
                }, 
                new GradientAlphaKey[]
                { 
                    new GradientAlphaKey(1f, 0.0f), // Start of full color (no transparency)
                    new GradientAlphaKey(1f, _percent), // End of full color (no transparency)
                    new GradientAlphaKey(0.0f, _percent), // Start of transparent section
                    new GradientAlphaKey(0.0f, 1.0f), // End of transparent section
                }
            );

            _line.colorGradient = _gradient;
        }

        // Multiple segments (modify each LineRenderer's Gradient so that an entire LineRenderer becomes invisible)
        else
        {
            int _maxSegments = list_lines.Count;
            float _colorUpToSegment = Mathf.RoundToInt(_percent * _maxSegments);
            for (int _segment = 0; _segment < _maxSegments; _segment++)
            {
                LineRenderer _line = list_lines[_segment];
                Gradient _gradient = new Gradient();

                // Make segment visible
                if (_segment < _colorUpToSegment)
                {
                    _gradient.SetKeys
                    (
                        new GradientColorKey[]
                        {
                            new GradientColorKey(GetColorFromPercent((_colorUpToSegment == 1) ? 0.0f : _percent), 0.0f), // Force low color if on last segment
                            new GradientColorKey(GetColorFromPercent((_colorUpToSegment == 1) ? 0.0f : _percent), 1.0f),
                        },
                        new GradientAlphaKey[] // Visible
                        {
                            new GradientAlphaKey(1.0f, 0.0f),
                            new GradientAlphaKey(1.0f, 1.0f),
                        }
                    );
                }

                // Make segment INvisible
                else
                {
                    _gradient.SetKeys
                    (
                        new GradientColorKey[]
                        {
                            new GradientColorKey(_line.startColor, 0.0f),
                            new GradientColorKey(_line.startColor, 1.0f),
                        },
                        new GradientAlphaKey[] // Invisible
                        {
                            new GradientAlphaKey(0.0f, 0.0f),
                            new GradientAlphaKey(0.0f, 1.0f),
                        }
                    );
                }

                _line.colorGradient = _gradient; // Set Gradient for this segment
            }
        }
    }
    #endregion
}
