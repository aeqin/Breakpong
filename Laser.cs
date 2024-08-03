using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    private Rigidbody2D c_rb;
    private BoxCollider2D c_boxCol;

    private float laserMoveSpeed = 2500f;
    private int laserDirection = 0; // -1 or 1

    private void Awake()
    {
        c_rb = GetComponent<Rigidbody2D>();
        c_boxCol = GetComponent<BoxCollider2D>();
    }

    private void FixedUpdate()
    {
        // Move Laser
        MoveLaser();

        // Destroy Laser if offscreen
        if (Utilities.IsPosOffScreen(transform.position))
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Initialize a Laser with a collider to ignore, and a direction to move in
    /// </summary>
    public void Initialize(Collider2D _colToIgnore, int _dir)
    {
        Physics2D.IgnoreCollision(c_boxCol, _colToIgnore); // Prevent Laser destroying itself on spawn
        laserDirection = _dir;
    }

    /*********************************************************************************************************************************************************************************
     * Private Methods
     *********************************************************************************************************************************************************************************/
    #region Private Methods
    /// <summary>
    /// Move Laser horizontally
    /// </summary>
    private void MoveLaser()
    {
        c_rb.velocity = new Vector2(laserMoveSpeed * laserDirection, 0);
    }
    #endregion

    /*********************************************************************************************************************************************************************************
     * Public Methods
     *********************************************************************************************************************************************************************************/
    #region Public Methods
    /// <summary>
    /// Return this Laser's velocity
    /// </summary>
    public Vector2 GetCurrVelocity()
    {
        return c_rb.velocity;
    }

    /// <summary>
    /// Change this Laser's direction
    /// </summary>
    public void ReflectLaser()
    {
        laserDirection *= -1;
    }

    /// <summary>
    /// Destroy this Laser
    /// </summary>
    public void DestroyLaser()
    {
        Destroy(gameObject);
    }
    #endregion

    /*********************************************************************************************************************************************************************************
     * On Event Methods
     *********************************************************************************************************************************************************************************/
    #region On Event Methods
    /// <summary>
    /// When Laser collides
    /// </summary>
    protected void OnTriggerEnter2D(Collider2D collision)
    {
        // Did Laser hit Brick?
        if (collision.gameObject.TryGetComponent<Brick>(out Brick _brick))
        {
            // Damage & potentially destroy Brick
            ManagerBrick.Instance.OnLaserHitBrick(this, _brick);

            return; // Return, as ManagerBrick will decide to either Destroy/Reflect laser
        }

        // Did Laser hit Powerup?
        if (collision.gameObject.TryGetComponent<Powerup>(out Powerup _powerup))
        {
            // Destroy Powerup
            ManagerPowerup.Instance.OnLaserHitPowerup(this, _powerup);
        }

        // Did Laser hit Ball?
        if (collision.gameObject.TryGetComponent<Ball>(out Ball _ball))
        {
            // Slightly influence Ball velocity
            _ball.OnLaserHitBall(this);
        }

        DestroyLaser(); // Destroy Laser on hit
    }
    #endregion
}
