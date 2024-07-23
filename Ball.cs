using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    private Rigidbody2D c_rb;
    private CircleCollider2D c_circleCol;
    private SpriteRenderer c_spriteRenderer;
    private TrailRenderer c_trailRenderer;

    // Movement variables
    [SerializeField] private float baseMoveSpeed = 8f;
    private float currMoveSpeed;

    // Ball state variables
    public enum BallState { NORMAL, MAGNETIZED, };
    public BallState currState = BallState.NORMAL;

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
    private float particleMultiplier = 1.2f;
    private Color[] scoreMultiplierColors = new Color[5] { new Color32(120, 144, 156, 255), new Color32(43, 175, 43, 255), new Color32(69, 94, 222, 255), new Color32(156, 39, 176, 255), new Color32(229, 28, 35, 255) };

    // On Death variables
    [SerializeField] private ParticleSystem pf_onDeathParticles;


    // Start is called before the first frame update
    void Awake()
    {
        c_rb = GetComponent<Rigidbody2D>();
        c_circleCol = GetComponent<CircleCollider2D>();
        c_spriteRenderer = GetComponent<SpriteRenderer>();
        c_trailRenderer = GetComponent<TrailRenderer>();

        currMoveSpeed = baseMoveSpeed;
        c_rb.velocity = Vector3.left * currMoveSpeed;
        UpdateBallColor();
    }

    private void FixedUpdate()
    {
        switch (currState)
        {
            case BallState.NORMAL:
                // Maintain constant speed
                float _speedModifier = 1 + (li_speedStage.curr * percentSpeedPerStage);
                c_rb.velocity = GetBallCurrDir() * currMoveSpeed * _speedModifier;

                break;

            case BallState.MAGNETIZED:
                break;

            default:
                Debug.LogError("Add case to Ball:FixedUpdate() for BallState." + currState.ToString());
                break;
        }

        UpdateBallColor();

        CheckWithinBounds();
    }

    /*********************************************************************************************************************************************************************************
     * Private Methods
     *********************************************************************************************************************************************************************************/
    private void CheckWithinBounds()
    {
        if (!ManagerLevel.Instance.IsPosInsideLevel(transform.position)) // Is Ball outside of level boundaries? (from physics shenanigans)
        {
            ManagerBall.Instance.OnBallBeyondBounds(this);
        }
    }

    /// <summary>
    /// When Ball collides with Paddle, speed up Ball
    /// </summary>
    private void OnCollisionSpeedMax()
    {
        li_speedStage.resetToMax();
        li_collisionsBeforeSlowStage.resetToMax(); // Reset collisions before slowing down a speed stage
    }

    /// <summary>
    /// When Ball collides with anything, slow down Ball if Ball is faster than base speed
    /// </summary>
    private void OnCollisionDoSlow()
    {
        li_collisionsBeforeSlowStage.Decrement();

        if (li_speedStage.curr > 0 && li_collisionsBeforeSlowStage.curr <= 0)
        {
            li_speedStage.Decrement();
            li_collisionsBeforeSlowStage.resetToMax(); // Reset collisions after speeding up
        }
    }

    /// <summary>
    /// When Ball collides with anything, play particles
    /// </summary>
    private void OnCollisionSpawnParticles(Vector2 _spawnPos)
    {
        switch (currState)
        {
            case BallState.NORMAL:
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
    private void OnDeathSpawnParticles(Vector2 _spawnPos)
    {
        // Instantiate death particles
        ParticleSystem _ps = Instantiate(pf_onDeathParticles, _spawnPos, Quaternion.identity);
    }

    /// <summary>
    /// Reduce Ball speed to least
    /// </summary>
    private void SpeedFloor()
    {
        li_speedStage.resetToMin();
        li_collisionsBeforeSlowStage.resetToMax(); // Reset collisions before slowing down a speed stage
    }

    /// <summary>
    /// Update Ball & trail color
    /// </summary>
    private void UpdateBallColor()
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

            c_trailRenderer.startWidth = transform.localScale.x * 0.3f;
        }
    }

    /*********************************************************************************************************************************************************************************
     * Public Methods
     *********************************************************************************************************************************************************************************/
    /// <returns>The normalized velocity of the Ball (direction)</returns>
    public Vector2 GetBallCurrDir()
    {
        return c_rb.velocity.normalized;
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
    /// When Ball collides with magnetized Paddle, freeze Ball at Paddle
    /// </summary>
    /// <returns>Ideal position of Ball after magnetizing to Paddle</returns>
    public Vector2 FreezeBallOnPaddle(Bounds _paddleBounds)
    {
        Vector3 _squareAroundCollider = Utilities.Vec2FromFloat(c_circleCol.radius * 2f);
        Bounds _ballBounds = new Bounds((Vector2)transform.position, _squareAroundCollider);
        Vector3 _dirToPaddle = (c_rb.velocity * -1).normalized;// (_paddleBounds.center - _ballBounds.center).normalized;
        Vector3 _start = _paddleBounds.center;

        int adjustLimiter = 0; // Limit amount of incremental movements
        _ballBounds.center += _dirToPaddle * Time.fixedDeltaTime;
        while (!_ballBounds.Intersects(_paddleBounds) && adjustLimiter++ < 10)
        {
            _ballBounds.center += _dirToPaddle * Time.fixedDeltaTime; // Slowly increment position until Ball bounds would enter Paddle bounds
        }
        _ballBounds.center -= _dirToPaddle * Time.fixedDeltaTime; // Since while loop tripped when past bounds, decrement once

        c_rb.MovePosition(_ballBounds.center); // Set position at edge of Paddle

        // Freeze Ball
        currState = BallState.MAGNETIZED;
        SpeedFloor();

        return _ballBounds.center;
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
    /// Destroy Ball
    /// </summary>
    public void DestroyBall()
    {
        OnDeathSpawnParticles(transform.position);
        Destroy(gameObject);
    }

    /*********************************************************************************************************************************************************************************
     * On Event Methods
     *********************************************************************************************************************************************************************************/
    private void OnCollisionEnter2D(Collision2D collision)
    {
        OnCollisionDoSlow(); // Potentially slow down Ball on collision with anything

        // Did Ball hit Paddle?
        if (collision.gameObject.TryGetComponent<Paddle>(out Paddle _rbPaddle))
        {
            Vector2 _velInfluence = _rbPaddle.GetPaddleInfluenceVelocityOnBallCollide(); // Get the Paddle velocity that can influence the Ball velocity
            float _y_vel_influence = _velInfluence.y / 4.0f;
            float _x_vel_influence = _velInfluence.x;

            if (Mathf.Abs(_x_vel_influence) > 0) // If Paddle imparts x velocity (such as if in slam movement)
            {
                OnCollisionSpeedMax(); // Speed up ball
            }

            c_rb.velocity = new Vector2(c_rb.velocity.x, c_rb.velocity.y + _y_vel_influence);

            OnPaddleHitResetScoreMultiplier(); // Reset ball score multiplayer
        }

        // Did Ball hit anything?
        float _randX = UnityEngine.Random.Range(-0.1f, 0.1f);
        float _randY = UnityEngine.Random.Range(-0.1f, 0.1f);
        c_rb.velocity += new Vector2(_randX, _randY); // Slightly randomize Ball bounce

        OnCollisionSpawnParticles(collision.GetContact(0).point); // On hit, spawn collision particles
    }
}
