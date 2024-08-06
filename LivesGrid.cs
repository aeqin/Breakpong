using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LivesGrid : MonoBehaviour
{
    [SerializeField] private GameObject pf_lifeBall; // Represent lives left with sprite of Ball

    private List<GameObject> list_lifeBalls = new List<GameObject>();
    private Canvas c_mainCanvas;
    private GridLayoutGroup c_gridLayoutGroup;
    private RectTransform c_rectTransform;

    // Move GridLayout variables
    private IEnumerator moveGridToOffsetCoroutine;
    private bool f_isMovingGrid = false;
    private float moveDuration = 0.2f;

    // Drop LifeBall variables
    private IEnumerator dropNextLifeBallCoroutine;
    private bool f_isDroppingLifeBall = false;
    private float dropDuration = 0.2f;
    public delegate void DroppedBallReached(Vector2 _ballPos);
    public event DroppedBallReached EventDroppedBallReached;

    private void Awake()
    {
        c_mainCanvas = GetComponentInParent<Canvas>().rootCanvas;
        c_gridLayoutGroup = GetComponent<GridLayoutGroup>();
        c_rectTransform = GetComponent<RectTransform>();
    }

    /*********************************************************************************************************************************************************************************
     * Private Methods
     *********************************************************************************************************************************************************************************/
    #region Private Methods
    /// <summary>
    /// Calculate the offset from the current position of LivesGrid, so that the final Ball in the grid is positioned over the center of the level
    /// </summary>
    private float CalcGridOffset()
    {
        float _cellSizeX = c_gridLayoutGroup.cellSize.x;
        float _spacingX = c_gridLayoutGroup.spacing.x;
        int _livesLeft = list_lifeBalls.Count;

        float _offset = -1 * (_livesLeft - 1) * (_cellSizeX + _spacingX) / 2;

        return Mathf.Min(_offset, 0);
    }

    /// <summary>
    /// Starts Coroutine that moves LivesGrid to an offset, so that the next LifeBall in queue of the GridLayout is positioned over the center of the level
    /// </summary>
    private void MoveGridToOffset()
    {
        if (f_isMovingGrid) StopCoroutine(moveGridToOffsetCoroutine);

        moveGridToOffsetCoroutine = CR_MoveGridToOffset(CalcGridOffset(), moveDuration);
        StartCoroutine(moveGridToOffsetCoroutine);
    }

    /// <summary>
    /// Coroutine that flashes scoreText to given color, then back
    /// </summary>
    private IEnumerator CR_MoveGridToOffset(float _offset, float _timeToMove)
    {
        f_isMovingGrid = true; // Start flash flag

        // Each frame, move towards offset
        float _currX = c_rectTransform.anchoredPosition.x;
        float _currY = c_rectTransform.anchoredPosition.y;
        float _endX = 0 + _offset;
        float _progress = 0f;

        while (_progress < 1)
        {
            Vector2 _nextStepPos = new Vector2(Mathf.Lerp(_currX, _endX, _progress), _currY);
            c_rectTransform.anchoredPosition = _nextStepPos;
            _progress += Time.deltaTime / _timeToMove;
            yield return null;
        }
        c_rectTransform.anchoredPosition = new Vector2(_endX, _currY);

        f_isMovingGrid = false; // Finish move flag
    }

    /// <summary>
    /// Calculate the distance that the next LifeBall in queue needs to fall to reach level bounds. Since level bounds may not necessarily be a rectangle, use a raycast
    /// </summary>
    private float CalcDropDistance()
    {
        Vector3 _topOfLevelScreenPos;
        Vector3 _topOfLevelWorldPos;
        Vector3 _nextLifeBallScreenPos = list_lifeBalls.Last().transform.position;
        Vector3 _nextLifeBallWorldPos = Camera.main.ScreenToWorldPoint(_nextLifeBallScreenPos);

        RaycastHit2D _hit = Physics2D.Raycast(_nextLifeBallWorldPos, Vector2.down, 100f);
        if (_hit)
        {
            _topOfLevelWorldPos = _hit.point;
            _topOfLevelScreenPos = Camera.main.WorldToScreenPoint(_topOfLevelWorldPos);

            return (_topOfLevelScreenPos - _nextLifeBallScreenPos).y - 14f; // Also subtract Ball radius
        }

        Debug.LogError("LivesGrid:CalcDropDistance(), cannot find top of level.");
        return -1;
    }

    /// <summary>
    /// Starts Coroutine that drops the next LifeBall into the level bounds
    /// </summary>
    private void DropNextLifeBall()
    {
        if (f_isDroppingLifeBall) StopCoroutine(dropNextLifeBallCoroutine);

        dropNextLifeBallCoroutine = CR_DropNextLifeBall(CalcDropDistance(), dropDuration);
        StartCoroutine(dropNextLifeBallCoroutine);
    }

    /// <summary>
    /// Coroutine that moves the next LifeBall into the level bounds
    /// </summary>
    private IEnumerator CR_DropNextLifeBall(float _distToDrop, float _timeToDrop)
    {
        f_isDroppingLifeBall = true; // Start drop flag

        // Hide next LifeBall in queue, then create a duplicate one to drop (so GridLayout maintains its shape instead of shrinking)
        GameObject _nextLifeBall = list_lifeBalls.Last();
        _nextLifeBall.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 0); // Make LifeBall invisible
        GameObject _dupeLifeBall = Instantiate(pf_lifeBall, c_mainCanvas.transform); // Instantiate duplicate outside of GridLayout
        _dupeLifeBall.transform.position = _nextLifeBall.transform.position; // Duplicate starts at now invisible LifeBall position

        // Each frame, move towards offset
        float _currX = _dupeLifeBall.transform.position.x;
        float _currY = _dupeLifeBall.transform.position.y;
        float _currZ = _dupeLifeBall.transform.position.z;
        float _endY = _currY + _distToDrop;
        float _progress = 0f;

        while (_progress < 1)
        {
            Vector3 _nextStepPos = new Vector3(_currX, Mathf.Lerp(_currY, _endY, _progress), _currZ);
            _dupeLifeBall.transform.position = _nextStepPos;
            _progress += Time.deltaTime / _timeToDrop;
            yield return null;
        }
        _dupeLifeBall.transform.position = new Vector3(_currX, _endY, _currZ);

        f_isDroppingLifeBall = false; // Finish drop flag

        EventDroppedBallReached.Invoke(Camera.main.ScreenToWorldPoint(_dupeLifeBall.transform.position)); // Send signal that Life stopped dropping
        Destroy(_dupeLifeBall); // Remove duplicate LifeBall
        RemLife(); // Remove invisible LifeBall
    }

    /// <summary>
    /// Remove a Ball sprite from this LivesGrid (a GridLayout)
    /// </summary>
    private void RemLife()
    {
        if (list_lifeBalls.Count == 0) return;

        GameObject _lifeBall = list_lifeBalls.Last();
        list_lifeBalls.RemoveAt(list_lifeBalls.Count - 1);
        Destroy(_lifeBall);

        MoveGridToOffset();
    }
    #endregion

    /*********************************************************************************************************************************************************************************
     * Public Methods
     *********************************************************************************************************************************************************************************/
    #region Public Methods
    /// <summary>
    /// Create a number of Ball sprites to add as children to this LivesGrid (a GridLayout) to represent how many lives the Player has left
    /// </summary>
    /// <param name="_numLives"></param>
    public void InitializeLives(int _numLives)
    {
        // First make sure to clear any old LifeBalls left in GridLayout (from lives represented the previous game)
        foreach (GameObject _oldLifeBall in list_lifeBalls)
        {
            Destroy(_oldLifeBall);
        }
        list_lifeBalls.Clear();

        // Then add new LifeBalls to GridLayout for current number of lives
        for (int life = 0; life < _numLives; life++)
        {
            AddLife();
        }
    }

    /// <summary>
    /// Add a Ball sprite to this LivesGrid (a GridLayout)
    /// </summary>
    public void AddLife()
    {
        GameObject _lifeBall = Instantiate(pf_lifeBall, transform);
        list_lifeBalls.Add(_lifeBall);

        MoveGridToOffset();
    }

    /// <summary>
    /// Drop & remove LivesBall sprite from this LivesGrid (a GridLayout)
    /// </summary>
    public void NextLife()
    {
        if (list_lifeBalls.Count == 0) return;

        DropNextLifeBall();
    }
    #endregion
}
