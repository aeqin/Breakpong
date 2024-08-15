using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddlePortal : MonoBehaviour
{
    private SpriteRenderer c_spriteRenderer;
    private BoxCollider2D c_boxCollider;

    private Paddle warpBallToPaddle;

    private void Awake()
    {
        c_spriteRenderer = GetComponent<SpriteRenderer>();
        c_boxCollider = GetComponent<BoxCollider2D>();
    }

    public void Initialize(Paddle _spawner, Collider2D _ignoreSpawnerCollider)
    {
        Physics2D.IgnoreCollision(c_boxCollider, _ignoreSpawnerCollider); // Don't collide with Paddle that spawned PaddlePortal
        ResizePortal(_spawner.GetBoxColliderSize()); // Slightly bigger than normal Paddle, so that Ball hits PaddlePortal before Paddle 

        // Subscribe to Event when spawner Paddle changes size
        _spawner.EventPaddleSizeChanged += ResizePortal;

        // Find and assign the Paddle opposite the spawner Paddle, where Balls that hit PaddlePortal will be warped to
        if (ManagerPaddle.Instance.GetPaddleLeft() == _spawner)
        {
            warpBallToPaddle = ManagerPaddle.Instance.GetPaddleRight();
        }
        else
        {
            warpBallToPaddle = ManagerPaddle.Instance.GetPaddleLeft();
        }
    }

    /*********************************************************************************************************************************************************************************
     * Private Methods
     *********************************************************************************************************************************************************************************/
    #region Private Methods
    /// <summary>
    /// Resize PaddlePortal so that it always remains bigger than Paddle
    /// </summary>
    private void ResizePortal(Vector2 newPaddleSize)
    {
        c_spriteRenderer.size = new Vector2(newPaddleSize.x * 1.5f, newPaddleSize.y * 1.2f);
    }
    #endregion

    /*********************************************************************************************************************************************************************************
     * On Event Methods
     *********************************************************************************************************************************************************************************/
    #region On Event Methods
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        // Did Ball hit PaddlePortal?
        if (collision.gameObject.TryGetComponent<Ball>(out Ball _ball))
        {
            Vector2 _beforeCollisionVel = _ball.GetBallVelocityBeforeCollision();

            // Warp ball to the opposite Paddle
            _ball.WarpBallTo(warpBallToPaddle.GetBallSpawnPos(_ball.GetBallRadius() + 1f)); // +1 to avoid collision with Paddle

            // Give the Ball back its velocity before collision
            _ball.LaunchBallInDir(_beforeCollisionVel);
        }
    }
    #endregion
}
