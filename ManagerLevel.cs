using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ManagerLevel : MonoBehaviour
{
    #region Singleton
    public static ManagerLevel Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }
    #endregion

    public delegate void BeginGame();
    public event BeginGame EventBeginGame;

    [SerializeField] private EdgeCollider2D c_boundary;

    // Score variables
    public TextMeshProUGUI scoreText;
    private int score = 0;
    private Color baseScoreColor = Color.white;

    private IEnumerator flashScoreCoroutine;
    private bool f_isFlashingScore = false;

    // Lives variables
    [SerializeField] private LivesGrid livesGrid;
    [SerializeField] private GameObject panelGameOver;
    [SerializeField] private GameObject panelWin;
    private LimitInt li_lives = new LimitInt(3, 0, 3);
    
    // Current level variables
    private int currLevel = 0;

    void Update()
    {
        if (Input.GetKeyDown("k"))
        {
            Time.timeScale = 1;
        }
        if (Input.GetKeyDown("l"))
        {
            Time.timeScale = 0.0f;
            ManagerPaddle.Instance.GetPaddleLeft().PrintCurrPaddleActions();
            ManagerPaddle.Instance.GetPaddleRight().PrintCurrPaddleActions();
        }
    }

    public void Initialize()
    {
        // Subscribe to signals
        livesGrid.EventDroppedBallReached += OnDroppedBallReached;

        OnGameBegin(currLevel);

        // Send Signals
        EventBeginGame?.Invoke();
    }

    /*********************************************************************************************************************************************************************************
     * Private Methods
     *********************************************************************************************************************************************************************************/
    #region Private Methods
    /// <summary>
    /// Once the game begins
    /// </summary>
    /// <param name="_level">Level to begin</param>
    private void OnGameBegin(int _level)
    {
        // Set current level
        currLevel = _level;

        // Disable UI elements
        panelWin.SetActive(false);
        panelGameOver.SetActive(false);
        Time.timeScale = 1.0f;

        // Reset lives
        li_lives.resetToMax();
        livesGrid.InitializeLives(li_lives.curr);

        // Zero score
        scoreText.color = baseScoreColor;
        score = 0;
        UpdateScore(0);

        // Reset PowerupDropEngine for current level
        ManagerPowerup.Instance.ResetPowerupDropEngine(GetPowerupWeightDictForLevel(currLevel));

        // Reset Bricks
        ManagerBrick.Instance.ResetBricks();

        // Reset Paddles
        ManagerPaddle.Instance.ResetPaddles();

        // Magnetize (one use) the Paddle and Spawn ball
        Paddle _p = ManagerPaddle.Instance.GetPaddleLeft();
        ManagerPowerup.Instance.HandlePowerupPickup(ManagerPowerup.PowerupType.PaddleMagnetOnce, _p);
        ManagerBall.Instance.OnBeginGameSpawnBallAt(_p.GetBallSpawnPos());
    }
    
    /// <summary>
    /// Updates Score
    /// </summary>
    private void UpdateScore(int _scoreToAdd)
    {
        score += _scoreToAdd;
        scoreText.text = score.ToString("00000000");
    }

    /// <summary>
    /// Flashes Score text
    /// </summary>
    private void FlashScore(Color _flashColor)
    {
        if (f_isFlashingScore)
        {
            StopCoroutine(flashScoreCoroutine);

            flashScoreCoroutine = CR_FlashScore(baseScoreColor, _flashColor, 0.1f);
            StartCoroutine(flashScoreCoroutine);
        }
        else
        {
            flashScoreCoroutine = CR_FlashScore(baseScoreColor, _flashColor, 0.1f);
            StartCoroutine(flashScoreCoroutine);
        }
    }

    /// <summary>
    /// Coroutine that flashes scoreText to given color, then back
    /// </summary>
    private  IEnumerator CR_FlashScore(Color _baseColor, Color _flashColor, float _timeToPeak)
    {
        f_isFlashingScore = true; // Start flash flag

        // Each frame, move towards  _flashColor
        float _elapsedStep = 0f;
        Color _currColor = scoreText.color;
        while (_elapsedStep < 1)
        {
            scoreText.color = Color.Lerp(_currColor, _flashColor, _elapsedStep);
            _elapsedStep = _elapsedStep + Time.deltaTime / _timeToPeak;
            yield return null;
        }

        scoreText.color = _flashColor;
        _currColor = scoreText.color;

        // Each frame, return back to _baseColor
        _elapsedStep = 0f;
        while (_elapsedStep < 1)
        {
            scoreText.color = Color.Lerp(_currColor, _baseColor, _elapsedStep);
            _elapsedStep = _elapsedStep + Time.deltaTime / _timeToPeak;
            yield return null;
        }

        scoreText.color = _baseColor;

        f_isFlashingScore = false; // Finish flash flag
    }

    private Dictionary<ManagerPowerup.PowerupType, int> GetPowerupWeightDictForLevel(int _level)
    {
        Dictionary<ManagerPowerup.PowerupType, int> _dict_pwrType_weight;

        switch (_level)
        {
            default:
                _dict_pwrType_weight = new Dictionary<ManagerPowerup.PowerupType, int>()
                {
                    {ManagerPowerup.PowerupType.None, 100},
                    {ManagerPowerup.PowerupType.PaddleMagnet, 8},
                    {ManagerPowerup.PowerupType.PaddleSlam, 16},
                    {ManagerPowerup.PowerupType.PaddleGhostPaddle, 20},
                    {ManagerPowerup.PowerupType.PaddleGrowPaddle, 25},
                    {ManagerPowerup.PowerupType.PaddleShrinkPaddle, 8},
                    {ManagerPowerup.PowerupType.BallSplit, 50},
                    {ManagerPowerup.PowerupType.BallSpore, 10},
                };
                break;
        }

        return _dict_pwrType_weight;
    }

    /// <summary>
    /// Lose a life
    /// </summary>
    private void LoseLife()
    {
        li_lives.Decrement();

        livesGrid.NextLife();
    }

    /// <summary>
    /// Gain a life
    /// </summary>
    private void GainLife()
    {
        li_lives.Increment();

        livesGrid.AddLife();
    }

    /// <summary>
    /// Player ran out of lives, Game Over
    /// </summary>
    private void GameOver()
    {
        panelGameOver.SetActive(true);

        ManagerPaddle.Instance.DisablePaddles();
        ManagerBall.Instance.DestroyBalls();
    }

    /// <summary>
    /// Player destroyed all the Bricks, Game Win
    /// </summary>
    private void GameWin()
    {
        panelWin.SetActive(true);

        ManagerPaddle.Instance.DisablePaddles();
        ManagerBall.Instance.DestroyBalls();
    }
    #endregion

    /*********************************************************************************************************************************************************************************
     * Public Methods
     *********************************************************************************************************************************************************************************/
    #region Public Methods
    /// <summary>
    /// Returns the level boundaries in Bounds
    /// </summary>
    public Bounds GetLevelBounds()
    {
        return c_boundary.bounds;
    }

    /// <summary>
    /// Checks if given Bounds is within the boundaries of the level
    /// </summary>
    public bool IsBoundsInsideLevel(Bounds _checkBounds)
    {
        return Utilities.IsBoundsInsideBounds(_checkBounds, c_boundary.bounds);
    }

    /// <summary>
    /// Checks if given position is within the boundaries of the level
    /// </summary>
    public bool IsPosInsideLevel(Vector2 _pos)
    {
        return c_boundary.bounds.Contains(_pos);
    }

    /// <summary>
    /// When Brick is hit by a Ball, add to score. (the more Bricks that Balls hit without hitting Paddle, the larger the score gain)
    /// </summary>
    public void UpdateScoreOnBrickHitByBall(Brick _brick, Ball _ball)
    {
        // Update Ball score multiplier on hit with Brick
        _ball.UpdateBallScoreMultiplierOnBrickHit();

        int _brickScore = (int)(_brick.GetScore() * _ball.GetBallScoreMultiplier());
        UpdateScore(_brickScore);

        FlashScore(_ball.GetBallScoreMultiplierColor()); // Flash score by Ball score multiplier color
    }

    /// <summary>
    /// No more Balls left on screen, Player loses one life
    /// </summary>
    public void OnNoBallsLeft()
    {
        if (li_lives.curr == 0) // No lives left
        {
            GameOver();
        }
        else
        {
            LoseLife();
        }
    }

    /// <summary>
    /// No more Bricks left on screen, Player wins
    /// </summary>
    public void OnNoBricksLeft()
    {
        GameWin();
    }

    /// <summary>
    /// Restart the current level
    /// </summary>
    public void OnRestart()
    {
        OnGameBegin(currLevel);
    }
    #endregion

    /*********************************************************************************************************************************************************************************
     * On Event Methods
     *********************************************************************************************************************************************************************************/
    #region On Event Methods
    /// <summary>
    /// When Game begins, spawn 1 Ball in the middle of the screen
    /// </summary>
    private void OnDroppedBallReached(Vector2 _ballPos)
    {
        ManagerBall.Instance.OnLifeLostSpawnBallAt(_ballPos);
    }
    #endregion
}
