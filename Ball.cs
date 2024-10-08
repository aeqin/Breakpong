using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    protected Rigidbody2D c_rb;
    protected CircleCollider2D c_circleCol;
    protected SpriteRenderer c_spriteRenderer;
    protected TrailRenderer c_trailRenderer;
    [SerializeField] protected ColliderCircleTriggerChild c_circleTrigger;

    // Movement variables
    [SerializeField] private float baseMoveSpeed = 330f;
    private float currMoveSpeed;
    private float extraHorizontalSpeedOnCollision = 200f;
    private Vector3 velocityBeforeCollision;

    // Ball state variables
    public enum BallState { NORMAL, MAGNETIZED, }; // Mutually exclusive Ball states
    public BallState currState = BallState.NORMAL;
    protected struct BallFlags // Ball "states" that are NOT mutually exclusive
    {
        public bool f_spawnSpores;
        public bool f_hasGravity;
    }
    protected BallFlags currFlags;

    // BallSpore variables
    [SerializeField] private BallSpore pf_BallSpore;
    private int numSporesOnSpawn = 3; // How many BallSpores should be spawned
    private float sporeSpawnCooldown = 0.2f;
    private float sporeSpawnOKTime = 0f;

    // BallGravity variables
    [SerializeField] private ParticleSystem pf_gravityParticles;
    private List<Ball> list_gravityInfluenced = new List<Ball>(); // What objects are influenced by the gravity of this Ball
    private float baseGravityPull = 0.2f;
    private float randomGravityModifier; // Slightly randomize gravity pull for each Ball, so split Balls don't circle eachother for eternity

    // SpeedStage variables
    private LimitInt li_collisionsBeforeSlowStage = new LimitInt(2, 0, 2);
    private LimitInt li_speedStage = new LimitInt(0, 0, 2);
    private float percentSpeedPerStage = 0.5f;
    private Color[] speedColors = new Color[3] { Color.grey, Color.yellow, Color.red };

    // Score variables
    private LimitInt li_noPaddleHitScoreMultiplierStage = new LimitInt(0, 0, 4);
    private float currScoreMultiplier = 1.0f;
    private float scoreMultiplierIncreaseOnHit = 2.0f;

    // Collision variables
    [SerializeField] private ParticleSystem pf_onHitParticles;
    private int baseParticleNum = 5;
    private float lastHitParticlesSpawnTime = 0f;
    private float particleMultiplier = 1.2f;
    private Color[] scoreMultiplierColors = new Color[5] { new Color32(120, 144, 156, 255), new Color32(43, 175, 43, 255), new Color32(69, 94, 222, 255), new Color32(156, 39, 176, 255), new Color32(229, 28, 35, 255) };

    // On Death variables
    [SerializeField] private ParticleSystem pf_onDeathParticles;


    // Start is called before the first frame update
    protected virtual void Awake()
    {
        c_rb = GetComponent<Rigidbody2D>();
        c_circleCol = GetComponent<CircleCollider2D>();
        c_spriteRenderer = GetComponent<SpriteRenderer>();
        c_trailRenderer = GetComponent<TrailRenderer>();

        // Initialize movement
        currMoveSpeed = baseMoveSpeed;
        c_rb.velocity = Vector3.right * currMoveSpeed;

        // Initialize random
        randomGravityModifier = UnityEngine.Random.Range(0.75f, 1.25f);

        // Initialize display
        c_trailRenderer.startWidth = c_circleCol.radius * 2.0f;
        UpdateBallColor();

        // Subscribe to signals
        c_circleTrigger.EventOnTriggerEnter += OnTriggerEntered;
        c_circleTrigger.EventOnTriggerExit += OnTriggerExited;
    }

    protected void FixedUpdate()
    {
        // Move Ball
        MoveByState();
        CheckWithinBounds();
        velocityBeforeCollision = c_rb.velocity;

        // Do Ball
        DoByFlag();

        // Display Ball
        UpdateBallColor();
    }

    /*********************************************************************************************************************************************************************************
     * Protected Methods
     *********************************************************************************************************************************************************************************/
    #region Protected Methods
    /// <summary>
    /// Returns Ball's current flags
    /// </summary>
    protected BallFlags GetBallFlags()
    {
        return currFlags;
    }

    /// <summary>
    /// Returns the collider of the Ball
    /// </summary>
    protected CircleCollider2D GetCircleCollider()
    {
        return c_circleCol;
    }

    /// <summary>
    /// Returns the base speed of this Ball
    /// </summary>
    protected float GetBallBaseSpeed()
    {
        return baseMoveSpeed;
    }

    /// <summary>
    /// Potentially destroys Ball if this Ball is beyond level boundaries
    /// </summary>
    protected void CheckWithinBounds()
    {
        if (!ManagerLevel.Instance.IsPosInsideLevel(transform.position)) // Is Ball outside of level boundaries? (from physics shenanigans)
        {
            ManagerBall.Instance.OnBallBeyondBounds(this);
        }
    }
    
    /// <summary>
    /// Based on this Ball's state, move each physics frame
    /// </summary>
    protected void MoveByState()
    {
        switch (currState)
        {
            case BallState.NORMAL:
                // Maintain constant speed
                float _speedModifier = 1 + (li_speedStage.curr * percentSpeedPerStage);
                c_rb.velocity = GetBallCurrDir() * currMoveSpeed * _speedModifier;

                break;

            case BallState.MAGNETIZED:
                break; // Do not move

            default:
                Debug.LogError("Add case to Ball:MoveByState() for BallState." + currState.ToString());
                break;
        }
    }

    /// <summary>
    /// Based on this Ball's flags, do stuff per physics frame
    /// </summary>
    protected void DoByFlag()
    {
        // Ball currently draws other Balls towards it
        if (currFlags.f_hasGravity)
        {
            DoGravityPull();
        }
    }

    /// <summary>
    /// Draw in other Balls within range of influence
    /// </summary>
    protected void DoGravityPull()
    {
        for (int i = list_gravityInfluenced.Count - 1; i >= 0; i--)
        {
            Ball _satellite = list_gravityInfluenced[i];
            if (_satellite != null)
            {
                Vector2 _dirFromSatelliteToMe = (transform.position - _satellite.transform.position).normalized;
                Vector2 _pullForce = _dirFromSatelliteToMe * c_circleCol.radius * baseGravityPull; // Scale pull force by size of Ball
                _satellite.InfluenceVelocity(randomGravityModifier * _pullForce); // Each Ball, even same size, has slightly different pull force 
            }
            else
            {
                list_gravityInfluenced.RemoveAt(i); // Remove null Ball
            }
        }
    }

    /// <summary>
    /// When Ball collides with anything, do things (play particles, slow down Ball, spawn spores, etc.)
    /// </summary>
    protected virtual void OnCollisionDo(Collision2D collision)
    {
        // Did Ball hit Paddle?
        if (collision.gameObject.TryGetComponent<Paddle>(out Paddle _paddle))
        {
            Vector2 _velInfluence = _paddle.GetPaddleInfluenceVelocityOnBallCollide(); // Get the Paddle velocity that can influence the Ball velocity
            float _y_vel_influence = _velInfluence.y / 4.0f;
            float _x_vel_influence = _velInfluence.x;

            if (Mathf.Abs(_x_vel_influence) > 1f) // If Paddle imparts great x velocity (such as if in PaddleActionSlam)
            {
                OnCollisionSpeedMax(); // Speed up ball
            }
            else
            {
                // On collision with Paddle, slightly give Ball more horizontal speed than physically possible, to reduce scenario where Ball ends
                // up moving almost vertically across the level
                _x_vel_influence = _paddle.GetDirToCenter() * extraHorizontalSpeedOnCollision;
            }

            c_rb.velocity = new Vector2(c_rb.velocity.x + _x_vel_influence, c_rb.velocity.y + _y_vel_influence); // Set Ball velocity

            if (_paddle.IsBallScoreMultiplierResetter())
            {
                OnPaddleHitResetScoreMultiplier(); // Reset Ball score multiplier on hit with "normal" Paddle
            }
        }

        OnCollisionSlow(); // Potentially slow down Ball
        OnCollisionSpawnSpores(); // Potentially spawn BallSpores

        OnCollisionSpawnParticles(collision.GetContact(0).point); // Spawn hit particles
    }

    /// <summary>
    /// When Ball collides with Paddle, speed up Ball
    /// </summary>
    protected void OnCollisionSpeedMax()
    {
        li_speedStage.resetToMax();
        li_collisionsBeforeSlowStage.resetToMax(); // Reset collisions before slowing down a speed stage
    }

    /// <summary>
    /// When Ball collides with anything, slow down Ball if Ball is faster than base speed
    /// </summary>
    protected void OnCollisionSlow()
    {
        li_collisionsBeforeSlowStage.Decrement();

        if (li_speedStage.curr > 0 && li_collisionsBeforeSlowStage.curr <= 0)
        {
            li_speedStage.Decrement();
            li_collisionsBeforeSlowStage.resetToMax(); // Reset collisions after speeding up
        }
    }

    /// <summary>
    /// When Ball collides with anything, slow down Ball if Ball is faster than base speed
    /// </summary>
    protected void OnCollisionSpawnSpores()
    {
        if (!currFlags.f_spawnSpores) return; // Cannot spawn BallSpores
        if (currState == BallState.MAGNETIZED) return; // Cannot spawn BallSpores when magnetized

        // Spore spawn cooldown
        if (!(Time.time > sporeSpawnOKTime)) return; // Cannot spawn BallSpores when on cooldown
        sporeSpawnOKTime = Time.time + sporeSpawnCooldown;

        // Spawn a number of BallSpores
        float _randAngleOffset = UnityEngine.Random.Range(0f, 360f); // Start with a random angle offset
        for (int _sporeNum = 0; _sporeNum < numSporesOnSpawn; _sporeNum++)
        {
            float _angleDegree = (360 / numSporesOnSpawn) * _sporeNum + _randAngleOffset;
            float _angleRadian = _angleDegree * Mathf.Deg2Rad;
            Vector3 _pointOnCirc = new Vector3(Mathf.Cos(_angleRadian), Mathf.Sin(_angleRadian), 0) * c_circleCol.radius; // Spawn spores along circumference of Ball (otherwise BallSpores would collide with eachother)
            Quaternion _rot = Quaternion.Euler(0, 0, _angleDegree);

            BallSpore _bs = Instantiate(pf_BallSpore, transform.position + _pointOnCirc, Quaternion.identity, ManagerBall.Instance.transform); // Instantiate BallSpores under ManagerBall (for easy cleanup)
            Physics2D.IgnoreCollision(_bs.GetCircleCollider(), c_circleCol); // Make spawned spores ignore collision with spawner Ball (otherwise they get stuck)
            _bs.LaunchBallInDir(_rot * Vector2.right);
        }
    }

    /// <summary>
    /// When Ball collides with anything, play particles
    /// </summary>
    protected void OnCollisionSpawnParticles(Vector2 _spawnPos)
    {
        switch (currState)
        {
            case BallState.NORMAL:
                // Make sure to limit particles
                if (Time.time < lastHitParticlesSpawnTime + 0.01f)
                {
                    return;
                }
                lastHitParticlesSpawnTime = Time.time;

                // Instantiate hit particles
                ParticleSystem _ps = Instantiate(pf_onHitParticles, _spawnPos, Quaternion.identity);

                // Set particle color over time to Ball color, fading into transparency
                ParticleSystem.ColorOverLifetimeModule _colLife = _ps.colorOverLifetime;
                Gradient _grad = new Gradient();
                _grad.SetKeys(new GradientColorKey[] { new GradientColorKey(c_spriteRenderer.color, 0.0f), new GradientColorKey(c_spriteRenderer.color, 1.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) });
                _colLife.color = _grad;

                // Modify number of particles based on score multiplier
                ParticleSystem.Burst _burst = _ps.emission.GetBurst(0);
                _burst.count = Math.Clamp((int)baseParticleNum * particleMultiplier * li_noPaddleHitScoreMultiplierStage.curr, baseParticleNum, (int)baseParticleNum * particleMultiplier * li_noPaddleHitScoreMultiplierStage.max);
                _ps.emission.SetBurst(0, _burst);

                break;

            case BallState.MAGNETIZED:
                break;

            default:
                Debug.LogError("Add case to Ball:OnCollisionSpawnParticles() for BallState." + currState.ToString());
                break;
        }
    }

    /// <summary>
    /// When Ball is destroyed, play particles
    /// </summary>
    protected void OnDeathSpawnParticles(Vector2 _spawnPos)
    {
        if (!pf_onDeathParticles) return; // If no death particles, (like if Ball is BallSpore), then return

        // Instantiate death particles
        ParticleSystem _ps = Instantiate(pf_onDeathParticles, _spawnPos, Quaternion.identity);
    }

    /// <summary>
    /// Reduce Ball speed to least
    /// </summary>
    protected void SpeedFloor()
    {
        li_speedStage.resetToMin();
        li_collisionsBeforeSlowStage.resetToMax(); // Reset collisions before slowing down a speed stage
    }

    /// <summary>
    /// Update Ball & trail color
    /// </summary>
    protected virtual void UpdateBallColor()
    {
        switch (currState)
        {
            case BallState.NORMAL:
                c_trailRenderer.enabled = true; // Disable trail if magnetized
                break;

            case BallState.MAGNETIZED:
                c_trailRenderer.enabled = false; // Disable trail if magnetized
                break;

            default:
                Debug.LogError("Add case to Ball:UpdateBallColor() for BallState." + currState.ToString());
                break;
        }

        c_spriteRenderer.color = scoreMultiplierColors[li_noPaddleHitScoreMultiplierStage.curr];

        if (c_trailRenderer.enabled)
        {
            Gradient _grad = new Gradient();
            _grad.SetKeys(new GradientColorKey[] { new GradientColorKey(c_spriteRenderer.color, 0.0f), new GradientColorKey(speedColors[li_speedStage.curr], 0.5f) }, new GradientAlphaKey[] { new GradientAlphaKey(0.7f, 0.0f), new GradientAlphaKey(0.2f, 1.0f) });
            c_trailRenderer.colorGradient = _grad;
        }

        // Play or stop gravity particles
        if (currFlags.f_hasGravity) pf_gravityParticles.Play();
        else pf_gravityParticles.Stop();
    }
    #endregion

    /*********************************************************************************************************************************************************************************
     * Public Methods
     *********************************************************************************************************************************************************************************/
    #region Public Methods
    /// <summary>
    /// Returns the current radius of this Ball
    /// </summary>
    public float GetBallRadius()
    {
        return c_circleCol.radius;
    }

    /// <returns>The normalized velocity of the Ball (direction)</returns>
    public Vector2 GetBallCurrDir()
    {
        return c_rb.velocity.normalized;
    }

    /// <summary>
    /// Returns the current velocity of this Ball, before OnTrigger/OnCollision triggers
    /// </summary>
    public Vector2 GetBallVelocityBeforeCollision()
    {
        return velocityBeforeCollision;
    }

    /// <summary>
    /// Returns the current score multiplier of this Ball
    /// </summary>
    public float GetBallScoreMultiplier()
    {
        return currScoreMultiplier;
    }
    
    /// <summary>
    /// Returns the current score multiplier Color of this Ball
    /// </summary>
    public Color GetBallScoreMultiplierColor()
    {
        return scoreMultiplierColors[li_noPaddleHitScoreMultiplierStage.curr];
    }

    /// <summary>
    /// Sets this Ball's flags to the same as the given Ball's flags
    /// </summary>
    public void SynchronizeBallFlags(Ball _ball)
    {
        currFlags = _ball.GetBallFlags();
    }

    /// <summary>
    /// When Ball collides with magnetized Paddle, freeze Ball at Paddle
    /// </summary>
    /// <returns>Ideal position of Ball after magnetizing to Paddle</returns>
    public Vector2 FreezeBallOnPaddle(Bounds _paddleBounds)
    {
        // Edge of Paddle + radius of Ball = position of Ball without any overlapping colliders
        float _left_or_right = Mathf.Sign(transform.position.x - _paddleBounds.center.x);
        Vector3 _paddleEdgePlusBall = _paddleBounds.center + new Vector3((_paddleBounds.extents.x + c_circleCol.radius) * _left_or_right, transform.position.y, 0);
        transform.position = _paddleEdgePlusBall;

        // Freeze Ball
        currState = BallState.MAGNETIZED;
        SpeedFloor();

        return transform.position;
    }

    /// <summary>
    /// When Ball is magnetized to Paddle, influence Ball position with Paddle position
    /// </summary>
    public void MoveBallWithMagnetizedPaddle(Vector2 _ballPosNextPhysicsFrame)
    {
        c_rb.MovePosition(_ballPosNextPhysicsFrame);
    }

    /// <summary>
    /// Launch Ball in direction
    /// </summary>
    public void LaunchBallInDir(Vector2 _dir)
    {
        c_rb.velocity = _dir;

        currState = BallState.NORMAL;
    }

    /// <summary>
    /// Warp Ball to global position
    /// </summary>
    public void WarpBallTo(Vector2 _pos)
    {
        transform.position = _pos;

        c_trailRenderer.Clear(); // Clear current trail, otherwise get a huge trail between before pos and warp pos
    }

    /// <summary>
    /// Influence Ball velocity
    /// </summary>
    public void InfluenceVelocity(Vector2 _influence)
    {
        c_rb.velocity += _influence;
    }

    /// <summary>
    /// On Brick hit, increase Ball score multiplier (up to max), and tell ManagerLevel to increase the score
    /// </summary>
    public void UpdateBallScoreMultiplierOnBrickHit()
    {
        if (li_noPaddleHitScoreMultiplierStage.curr < li_noPaddleHitScoreMultiplierStage.max)
        {
            currScoreMultiplier *= scoreMultiplierIncreaseOnHit; // Increase ball score multiplier
            li_noPaddleHitScoreMultiplierStage.Increment();
        }
    }

    /// <summary>
    /// On Paddle hit, reset Ball score multiplier
    /// </summary>
    public void OnPaddleHitResetScoreMultiplier()
    {
        currScoreMultiplier = 1.0f;
        li_noPaddleHitScoreMultiplierStage.resetToMin();
    }

    /// <summary>
    /// Sets whether or not this Ball will spawn BallSpores on collision
    /// </summary>
    public void SetBallSporeSpawner(bool _doSpawnSpores)
    {
        currFlags.f_spawnSpores = _doSpawnSpores;
    }

    /// <summary>
    /// Sets whether or not this Ball will influece velocity of other Balls in range
    /// </summary>
    public void SetBallDoesGravity(bool _hasGravity)
    {
        currFlags.f_hasGravity = _hasGravity;
        UpdateBallColor(); // Play gravity particles
    }

    /// <summary>
    /// When Laser hits Ball, slightly influence Ball velocity
    /// </summary>
    public virtual void OnLaserHitBall(Laser _laser)
    {
        c_rb.velocity = new Vector2(c_rb.velocity.x + _laser.GetCurrVelocity().x / 3f, c_rb.velocity.y);
    }

    /// <summary>
    /// Destroy Ball
    /// </summary>
    public void DestroyBall()
    {
        OnDeathSpawnParticles(transform.position);
        Destroy(gameObject);
    }
    #endregion

    /*********************************************************************************************************************************************************************************
     * On Event Methods
     *********************************************************************************************************************************************************************************/
    #region On Event Methods
    protected void OnCollisionEnter2D(Collision2D collision)
    {
        // Did Ball hit anything?
        float _randX = UnityEngine.Random.Range(-5f, 5f);
        float _randY = UnityEngine.Random.Range(-5f, 5f);
        c_rb.velocity += new Vector2(_randX, _randY); // Slightly randomize Ball bounce

        // Do additional work on collision
        OnCollisionDo(collision);
    }

    /// <summary>
    /// Event called by signal sent from ColliderCircleTriggerChild
    /// </summary>
    protected void OnTriggerEntered(Collider2D collision)
    {
        if (!currFlags.f_hasGravity) return;

        // Did Ball enter gravity influence?
        if (collision.gameObject.TryGetComponent<Ball>(out Ball _satellite))
        {
            list_gravityInfluenced.Add(_satellite);
        }
    }

    /// <summary>
    /// Event called by signal sent from ColliderCircleTriggerChild
    /// </summary>
    protected void OnTriggerExited(Collider2D collision)
    {
        // Did Ball exit gravity influence?
        if (collision.gameObject.TryGetComponent<Ball>(out Ball _satellite))
        {
            list_gravityInfluenced.RemoveAll(_ball => _ball == _satellite);
        }
    }

    #endregion
}
