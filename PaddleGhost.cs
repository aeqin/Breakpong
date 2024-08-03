using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleGhost : Paddle
{
    private Paddle ghostOfPaddle; // What main Paddle is this PaddleGhost a copy of
    private float ghostXMoveSpeed = 200f;


    protected override void FixedUpdate()
    {
        // Move PaddleGhost
        MovePaddleGhost();

        // Destroy Powerup if offscreen
        if (Utilities.IsPosOffScreen(transform.position))
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Initialize a PaddleGhost with a reference to a PaddleLeft or PaddleRight (what ghost should copy & spawn from)
    /// </summary>
    /// <param name="_ghostOfPaddle"></param>
    public void Initialize(Paddle _ghostOfPaddle)
    {
        ghostOfPaddle = _ghostOfPaddle;
        dirToCenter = ghostOfPaddle.GetDirToCenter();
        SetPaddleSize(ghostOfPaddle.GetSpriteRendererSize()); // Make sure PaddleGhost is the same size as its spawner Paddle
        c_spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f); // Make sprite more transparent than normal Paddle
    }

    /*********************************************************************************************************************************************************************************
     * Private Methods
     *********************************************************************************************************************************************************************************/
    #region Private Methods
    /// <summary>
    /// Move PaddleGhost. Always moves horizontally, moves vertically if under control by main Paddle
    /// </summary>
    private void MovePaddleGhost()
    {
        Vector2 _ghostPaddleDir = new Vector2(ghostXMoveSpeed * dirToCenter, (paddleInputVector * paddleMoveSpeed).y);
        c_rb.velocity = _ghostPaddleDir;
    }
    #endregion

    /*********************************************************************************************************************************************************************************
     * Protected Methods
     *********************************************************************************************************************************************************************************/
    #region Protected Methods
    /// <summary>
    /// Create all the PaddleAction objects
    /// </summary>
    protected override void CreatePaddleActions() { } // Do not attempt to create any PaddleActions

    /// <summary>
    /// Depending on the PaddleAction provided, update the linked Icons
    /// </summary>
    protected override void UpdatePaddleActionIcons(PaddleAction _PA) { } // Do not attempt to update any PaddleActionIcons
    #endregion

    /*********************************************************************************************************************************************************************************
    * Public Methods
    *********************************************************************************************************************************************************************************/
    #region Public Methods
    /// <summary>
    /// Control PaddleGhost by setting its movement vector equal to given input vector
    /// </summary>
    /// <param name="_inputVector"></param>
    public void ControlGhostPaddleBy(Vector2 _inputVector)
    {
        paddleInputVector = _inputVector;
    }

    /// <summary>
    /// Returns whether or not Paddle will reset Ball score multiplier on hit
    /// </summary>
    public override bool IsBallScoreMultiplierResetter()
    {
        return false; // Do not consider PaddleGhost a "normal" Paddle, and so do NOT reset Ball score multiplier on hit
    }

    /// <summary>
    /// Called by Ball script on collision, in order to check if Paddle is in a state to influence Ball velocity
    /// </summary>
    /// <returns>The velocity that Paddle should use to influence the Ball velocity</returns>
    public override Vector2 GetPaddleInfluenceVelocityOnBallCollide()
    {
        Vector2 _usableVelocity = Vector2.zero; // How much velocity should actually be used to influence the Ball

        _usableVelocity.x = 0.0f; // Use none of Paddle x velocity to influence Ball velocity
        _usableVelocity.y += c_rb.velocity.y; // Use all of Paddle y velocity to influence Ball velocity

        return _usableVelocity;
    }
    #endregion

    /*********************************************************************************************************************************************************************************
    * On Event Methods
    *********************************************************************************************************************************************************************************/
    #region On Event Methods
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        // Did PaddleGhost hit Ball?
        if (collision.gameObject.TryGetComponent<Ball>(out Ball _ball))
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        ghostOfPaddle.RegainInputControl(); // Once PaddleGhost is destroyed, allow main Paddle to respond to movement input again
    }
    #endregion
}
