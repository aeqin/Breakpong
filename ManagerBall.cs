using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerBall : MonoBehaviour
{
    #region Singleton
    public static ManagerBall Instance { get; private set; }

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

    [SerializeField] private Ball pf_Ball; // Ball prefab
    [SerializeField] private float splitDegree = 5f; // Angle degree at which to split the Ball (on split Powerup)

    private List<Ball> list_Balls = new List<Ball>();

    public void Initialize()
    {
        // Subscribe to Events
        //ManagerLevel.Instance.EventBeginGame += OnBeginGame;
    }

    /*********************************************************************************************************************************************************************************
     * Private Methods
     *********************************************************************************************************************************************************************************/
    #region Private Methods
    /// <summary>
    /// Create a Ball in the middle of the screen, and add Ball to list of Balls
    /// </summary>
    private Ball CreateBall()
    {
        Ball _ball = Instantiate(pf_Ball, Vector2.zero, Quaternion.identity);
        list_Balls.Add(_ball);

        return _ball;
    }

    /// <summary>
    /// Create a Ball at a particular position, and add Ball to list of Balls
    /// </summary>
    private Ball CreateBallAt(Vector2 pos)
    {
        Ball _ball = Instantiate(pf_Ball, pos, Quaternion.identity);
        list_Balls.Add(_ball);

        return _ball;
    }

    /// <summary>
    /// Remove Ball, and ask itself to Destroy itself
    /// </summary>
    private void RemoveBall(Ball _b)
    {
        list_Balls.RemoveAll(_ball => _ball == _b);

        _b.DestroyBall();

        if (list_Balls.Count == 0) // No more Balls left, need to either GameOver or spawn another Ball
        {
            ManagerLevel.Instance.OnNoBallsLeft();
        }
    }

    /// <summary>
    /// Split all Balls into two
    /// </summary>
    private void SplitBalls()
    {
        List<Ball> _b4_split_ball_list = new List<Ball>(list_Balls);

        foreach (Ball _ball in _b4_split_ball_list)
        {
            Vector2 _dir = _ball.GetBallCurrDir(); // Current direction of Ball
            Ball _ballClone = CreateBallAt(_ball.transform.position);
            _ballClone.SynchronizeBallFlags(_ball); // Clone should have same flags as base Ball

            // Send both Ball, and clone of Ball in branching directions slightly off from original direction
            Quaternion _rotByDegUp = Quaternion.Euler(0, 0, splitDegree);
            Quaternion _rotByDegDown = Quaternion.Euler(0, 0, -1 * splitDegree);
            _ball.LaunchBallInDir(_rotByDegUp * _dir);
            _ballClone.LaunchBallInDir(_rotByDegDown * _dir);
        }
    }

    /// <summary>
    /// Designate all Balls to spawn BallSpores
    /// </summary>
    private void SporeBalls()
    { 
        foreach (Ball _ball in list_Balls)
        {
            _ball.SetBallSporeSpawner(true); // Allow active Ball to spawn BallSpores on collision
        }
    }

    /// <summary>
    /// Designate all Balls to do pull gravity
    /// </summary>
    private void GravityBalls()
    {
        foreach (Ball _ball in list_Balls)
        {
            _ball.SetBallDoesGravity(true); // Allow active Ball to influence velocity of other Balls in range
        }
    }
    #endregion

    /*********************************************************************************************************************************************************************************
     * Public Methods
     *********************************************************************************************************************************************************************************/
    #region Public Methods
    /// <summary>
    /// Ask BallManager to check whether Ball is alive
    /// </summary>
    public bool IsBallAlive(Ball _testBall)
    {
        return Instance.list_Balls.Contains(_testBall);
    }

    /// <summary>
    /// Ask BallManager to handle event when Ball enters Deathzone
    /// </summary>
    public void OnBallEnteredDeathzone(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<Ball>(out Ball _ball))
        {
            Instance.RemoveBall(_ball);
        }
    }

    /// <summary>
    /// Ask BallManager to handle event when Ball exits level boundaries
    /// </summary>
    public void OnBallBeyondBounds(Ball _ball)
    {
        Instance.RemoveBall(_ball);
    }

    /// <summary>
    /// When a Paddle picks up a Powerup that affects Ball, handle it
    /// </summary>
    /// <param name="_powerUp">Type of Powerup</param>
    public void OnPowerupPickup(ManagerPowerup.PowerupType _powerUp)
    {
        switch (_powerUp)
        {
            case ManagerPowerup.PowerupType.BallSplit:
                SplitBalls();
                break;

            case ManagerPowerup.PowerupType.BallSpore:
                SporeBalls();
                break;

            case ManagerPowerup.PowerupType.BallGravity:
                GravityBalls();
                break;

            default:
                Debug.LogError("Add case to ManagerBall:OnPowerupPickup() for Powerup." + _powerUp.ToString());
                break;
        }
    }

    /// <summary>
    /// When Game begins, spawn 1 Ball at position
    /// </summary>
    public void OnBeginGameSpawnBallAt(Vector2 _spawnPos)
    {
        CreateBallAt(_spawnPos);
    }

    /// <summary>
    /// When Player loses a life, spawn 1 Ball at center top of screen, then randomly launch it towards one of the Paddles
    /// </summary>
    public void OnLifeLostSpawnBallAt(Vector2 _spawnPos)
    {
        Ball _ball = CreateBallAt(_spawnPos);

        Vector2 _dirToPaddleLeft = (ManagerPaddle.Instance.GetPaddleLeft().transform.position - _ball.transform.position).normalized;
        Vector2 _dirToPaddleRight = (ManagerPaddle.Instance.GetPaddleRight().transform.position - _ball.transform.position).normalized;
        Vector2 _randDir = Utilities.FlipACoin() ? _dirToPaddleLeft : _dirToPaddleRight; // 50% each direction

        _ball.LaunchBallInDir(_randDir);
    }

    /// <summary>
    /// Destroy all Balls
    /// </summary>
    public void DestroyBalls()
    {
        // Destroy normal Balls
        foreach (Ball _ball in list_Balls)
        {
            _ball.DestroyBall();
        }
        list_Balls.Clear();

        // Destroy secondary Balls (like BallSpore)
        foreach (Transform _childTransform in transform) // Iterate through all children of ManagerBall
        {
            Destroy(_childTransform.gameObject);
        }
    }
    #endregion

    /*********************************************************************************************************************************************************************************
     * On Event Methods
     *********************************************************************************************************************************************************************************/
    #region On Event Methods
    /// <summary>
    /// When Game begins, spawn 1 Ball in the middle of the screen
    /// </summary>
    private void OnBeginGame()
    {
        CreateBallAt(ManagerPaddle.Instance.GetPaddleLeft().transform.position);
    }
    #endregion
}