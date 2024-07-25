using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brick : MonoBehaviour
{
    private SpriteRenderer c_spriteRenderer;

    [SerializeField] private ManagerBrick.BrickType brickType = ManagerBrick.BrickType.Normal;
    [SerializeField] private ManagerPowerup.PowerupType droppedPowerupType = ManagerPowerup.PowerupType.Anything;
    private int brickBaseScore = 10;

    /// <summary>
    /// A bundle of values used to initialize a Brick. Can be used to reset the Brick when the level is over
    /// </summary>
    public struct BrickInitializer
    {
        public Vector2 spawnPos;
        public ManagerBrick.BrickType brickType;
        public ManagerPowerup.PowerupType droppedPowerupType;

        public BrickInitializer(Vector2 _spawnPos, ManagerBrick.BrickType _brickType, ManagerPowerup.PowerupType _droppedPowerupType)
        {
            spawnPos = _spawnPos;
            brickType = _brickType;
            droppedPowerupType = _droppedPowerupType;
        }
    }

    private void Awake()
    {
        c_spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Initialize this Brick after Instantiation using BrickInitializer
    /// </summary>
    public void Initialize(BrickInitializer _initBundle)
    {
        transform.position = _initBundle.spawnPos;
        brickType = _initBundle.brickType;
        droppedPowerupType = _initBundle.droppedPowerupType;

        UpdateSprite();
    }

    /*********************************************************************************************************************************************************************************
     * Private Methods
     *********************************************************************************************************************************************************************************/
    #region Private Methods
    /// <summary>
    /// Depending on type of this Brick, change its display sprite
    /// </summary>
    private void UpdateSprite()
    {
        if (droppedPowerupType == ManagerPowerup.PowerupType.None)
        {
            c_spriteRenderer.color = Color.white;
        }
        else if (droppedPowerupType == ManagerPowerup.PowerupType.Anything)
        {
            c_spriteRenderer.color = Color.yellow;
        }
        else // This Brick drops a specific Powerup
        {
            c_spriteRenderer.color = Color.red;
        }
    }
    #endregion

    /*********************************************************************************************************************************************************************************
     * Public Methods
     *********************************************************************************************************************************************************************************/
    #region Public Methods
    /// <summary>
    /// Return the bundle of variables that describe this Brick
    /// </summary>
    public BrickInitializer GetBrickInitializer()
    {
        BrickInitializer _bundle = new BrickInitializer(transform.position, brickType, droppedPowerupType);
        return _bundle;
    }

    /// <summary>
    /// Return the type of Powerup that this Brick can drop
    /// </summary>
    public ManagerPowerup.PowerupType GetDroppedPowerupType()
    {
        return droppedPowerupType;
    }

    /// <summary>
    /// Return the score that destroying this Brick gives
    /// </summary>
    public int GetScore()
    {
        return brickBaseScore;
    }

    /// <summary>
    /// Destroy this Brick, spawn a ParticleSystem
    /// </summary>
    public void DestroyBrick()
    {
        Destroy(gameObject);
    }
    #endregion

    /*********************************************************************************************************************************************************************************
     * On Event Methods
     *********************************************************************************************************************************************************************************/
    #region On Event Methods
    /// <summary>
    /// When Brick collides with Ball, destroy Brick
    /// </summary>
    protected void OnCollisionEnter2D(Collision2D collision)
    {
        // Did Ball hit Brick?
        if (collision.gameObject.TryGetComponent<Ball>(out Ball _ball))
        {
            // Destroy Brick
            ManagerBrick.Instance.OnBallHitBrick(_ball, this);
        }
    }
    #endregion
}
