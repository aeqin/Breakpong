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
    [SerializeField] private FlavorTextUI pf_scoreText;
    private int score = 0;
    private Color baseScoreColor = Color.white;

    // Lives variables
    [SerializeField] private LivesGrid livesGrid;
    [SerializeField] private GameObject panelGameOver;
    [SerializeField] private GameObject panelWin;
    private LimitInt li_lives = new LimitInt(3, 0, 3);
    
    // Current level variables
    private int currLevel = 0;

    // Combo
    [SerializeField] private FlavorTextWorld pf_comboPopupText;
    [SerializeField] private FlavorTextUI pf_comboText;
    [SerializeField] private FlavorTextUI pf_comboScoreMultiplierText;
    private Vector3 lastBrickHitPos = Vector3.zero;
    private ComboEngine currComboEngine = new ComboEngine();
    /// <summary>
    /// Class used to store & calculate current combo (which increases score gain)
    /// </summary>
    private class ComboEngine
    {
        private int currentWholeCombo = 0;
        private int currentComboLeftToCount = 0;
        private LimitInt nextComboToIncreaseMultiplier = new LimitInt(3, 3, 9999); // Takes 3 combo to increase score multiplier to 2x, then +1 combo for subsequent multiplier increment
        private LimitInt currentComboScoreMultiplier = new LimitInt(1, 1, 9999); // Score multiplier starts at 1x
        private float resetComboTime = 0f;
        private float comboExtensionDuration = 1f; // Every time combo is incremented, add this time before combo is reset

        public enum RunResult {COMBO_RESET, SCORE_MULTIPLIER_INCREASED, SCORE_MULTIPLIER_SAME}

        public ComboEngine() { }

        public int GetCombo()
        {
            return currentWholeCombo;
        }

        public int GetComboScoreMultiplier()
        {
            return currentComboScoreMultiplier.curr;
        }

        /// <summary>
        /// Run engine per physics frame. Checks to see if combo should be reset, or whether combo should increase score multiplier.
        /// </summary>
        /// <returns>Returns an enum RunResult. Combo was reset, combo score multiplier increased, or combo score multiplier remained the same.</returns>
        public RunResult RunComboEngine()
        {
            if (Time.time > resetComboTime)
            {
                ResetCombo();
                return RunResult.COMBO_RESET;
            }
            else
            {
                bool _f_multiplierIncreased = false;
                while (currentComboLeftToCount > 0)
                {
                    // Every time score multiplier is increased, amount of combo required increases (so 3 combo for 2x, then 4 combo for 3x, 5 combo for 4x, and so on...)
                    if (currentComboLeftToCount >= nextComboToIncreaseMultiplier.curr)
                    {
                        currentComboLeftToCount -= nextComboToIncreaseMultiplier.curr;
                        nextComboToIncreaseMultiplier.Increment(); // +1 combo needed for next score multiplier increase
                        currentComboScoreMultiplier.Increment(); // Add 1x to score multiplier
                        _f_multiplierIncreased = true;
                    }
                    else
                    {
                        break; // Not enough combo to increase score multiplier, so break out of loop
                    }
                }

                return _f_multiplierIncreased ? RunResult.SCORE_MULTIPLIER_INCREASED : RunResult.SCORE_MULTIPLIER_SAME;
            }
        }

        public void IncrementComboBy(int _combo = 1)
        {
            currentWholeCombo += _combo;
            currentComboLeftToCount += _combo;
            resetComboTime = Time.time + comboExtensionDuration; // Extend time before combo is reset
        }

        public void ResetCombo()
        {
            currentWholeCombo = 0;
            currentComboLeftToCount = 0;
            currentComboScoreMultiplier.resetToMin();
            nextComboToIncreaseMultiplier.resetToMin();
        }
    }

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

    private void FixedUpdate()
    {
        // Calculate combo & potentially update combo score multiplier
        ComboEngine.RunResult _comboResult = currComboEngine.RunComboEngine();
        if (_comboResult == ComboEngine.RunResult.SCORE_MULTIPLIER_INCREASED)
        {
            // Combo increased the score multiplier, so display a Flavor text at last location of combo increase
            SpawnComboFlavorText();
        }
        else if (_comboResult == ComboEngine.RunResult.COMBO_RESET)
        {
            // Too much time passed before gaining new combo
            ResetComboDisplayText();
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
        pf_scoreText.ChangeColor(baseScoreColor);
        score = 0;
        UpdateScore(0);

        // Reset combo
        currComboEngine.ResetCombo();
        ResetComboDisplayText();

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
        score += _scoreToAdd * currComboEngine.GetComboScoreMultiplier();
        pf_scoreText.ChangeText(score.ToString("00000000"));
    }

    /// <summary>
    /// Updates Score, by Brick hit
    /// </summary>
    private void UpdateScoreByBrickHit(Brick _brick, int _scoreToAdd)
    {
        UpdateScore(_scoreToAdd);
        currComboEngine.IncrementComboBy(1); // Add 1 combo per brick hit
        lastBrickHitPos = _brick.transform.position;

        // Update UI display
        pf_comboText.ChangeText(currComboEngine.GetCombo().ToString());
        pf_comboText.GrowText(Vector3.zero);
    }

    /// <summary>
    /// Spawn a FlavorTextWorld that displays new combo score multiplier
    /// </summary>
    private void SpawnComboFlavorText()
    {
        float _jumpOffset = 80f;
        string _multiplierText = currComboEngine.GetComboScoreMultiplier().ToString();
        string _popupText = _multiplierText + "X";

        FlavorTextWorld _comboFlavor = Instantiate(pf_comboPopupText, lastBrickHitPos + Utilities.Vec3OnlyY(_jumpOffset), Quaternion.identity);
        _comboFlavor.Initialize(_popupText, Color.white, _textSize: 1000, _lifetime: 1.0f, _doFade: true);
        _comboFlavor.GrowAndFlashText(Utilities.Vec3OnlyY(-1 * _jumpOffset), Color.magenta, _growAndFlashDuration: 0.5f, _overshootRatio: 0.6f);

        // Update UI display
        pf_comboScoreMultiplierText.ChangeText(_popupText);
        pf_comboScoreMultiplierText.GrowAndFlashText(Vector3.zero, Color.yellow, _growAndFlashDuration: 0.4f, _overshootRatio: 3.5f);
    }

    /// <summary>
    /// Reset the UI that displays current combo, as well as current combo score multiplier
    /// </summary>
    private void ResetComboDisplayText()
    {
        pf_comboText.ChangeText("0");
        pf_comboScoreMultiplierText.ChangeText("1x");
    }

    private Dictionary<ManagerPowerup.PowerupType, int> GetPowerupWeightDictForLevel(int _level)
    {
        Dictionary<ManagerPowerup.PowerupType, int> _dict_pwrType_weight;

        switch (_level)
        {
            default:
                _dict_pwrType_weight = new Dictionary<ManagerPowerup.PowerupType, int>()
                {
                    {ManagerPowerup.PowerupType.None, 200},
                    {ManagerPowerup.PowerupType.PaddleMagnet, 8},
                    {ManagerPowerup.PowerupType.PaddleSlam, 16},
                    {ManagerPowerup.PowerupType.PaddleGhostPaddle, 20},
                    {ManagerPowerup.PowerupType.PaddleGrowPaddle, 25},
                    {ManagerPowerup.PowerupType.PaddleShrinkPaddle, 8},
                    {ManagerPowerup.PowerupType.PaddleLaser, 10},
                    {ManagerPowerup.PowerupType.PaddleShield, 25},
                    {ManagerPowerup.PowerupType.BallSplit, 25},
                    {ManagerPowerup.PowerupType.BallSpore, 10},
                    {ManagerPowerup.PowerupType.BallGravity, 10},
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
        UpdateScoreByBrickHit(_brick, _brickScore);

        pf_scoreText.GrowAndFlashText(Vector3.zero, _ball.GetBallScoreMultiplierColor(), _growAndFlashDuration: 0.3f, _overshootRatio: 0.18f, _growFromPoint: false);
    }

    /// <summary>
    /// When Brick is hit by a Ball, add to score.
    /// </summary>
    public void UpdateScoreOnBrickHitByLaser(Brick _brick, Laser _laser)
    {
        int _brickScore = _brick.GetScore();
        UpdateScoreByBrickHit(_brick, _brickScore);
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
