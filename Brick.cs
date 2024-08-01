using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Brick : MonoBehaviour
{
    private SpriteRenderer c_spriteRenderer;

    [SerializeField] private ManagerBrick.BrickType brickType = ManagerBrick.BrickType.Normal;
    [SerializeField] private ManagerPowerup.PowerupType droppedPowerupType = ManagerPowerup.PowerupType.Anything;
    private int brickBaseScore = 10;

    // Brick Damage variables
    /// <summary>
    /// Type of damage that Brick took
    /// </summary>
    public enum BrickDamage
    {
        SUCCESS, // Did successfully damage brick
        INVINCIBLE, // Brick was invincible
        DEATH, // Brick died from damage
    }
    [SerializeField] private int maxHP = 1;
    [SerializeField] private TextMeshPro c_HPText;
    private LimitInt li_brickHP;
    private bool f_isInvincible = false;

    [SerializeField] private ParticleSystem pf_onBreakParticles;

    /// <summary>
    /// A bundle of values used to initialize a Brick. Can be used to reset the Brick when the level is over
    /// </summary>
    public struct BrickInitializer
    {
        public Vector2 spawnPos;
        public ManagerBrick.BrickType brickType;
        public ManagerPowerup.PowerupType droppedPowerupType;
        public int maxHP;

        public BrickInitializer(Vector2 _spawnPos, ManagerBrick.BrickType _brickType, ManagerPowerup.PowerupType _droppedPowerupType, int _maxHP)
        {
            spawnPos = _spawnPos;
            brickType = _brickType;
            droppedPowerupType = _droppedPowerupType;
            maxHP = _maxHP;
        }
    }

    private void Awake()
    {
        c_spriteRenderer = GetComponent<SpriteRenderer>();
        li_brickHP = new LimitInt(maxHP, 0, maxHP);
    }

    /// <summary>
    /// Initialize this Brick after Instantiation using BrickInitializer
    /// </summary>
    public void Initialize(BrickInitializer _initBundle)
    {
        transform.position = _initBundle.spawnPos;
        brickType = _initBundle.brickType;
        droppedPowerupType = _initBundle.droppedPowerupType;
        li_brickHP = new LimitInt(_initBundle.maxHP, 0, _initBundle.maxHP);

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

        c_HPText.text = GetHP().ToString(); // Display remaining HP
    }

    /// <summary>
    /// When Brick is destroyed, play particles
    /// </summary>
    private void OnDeathSpawnParticles(Vector2 _spawnPos)
    {
        ParticleSystem _ps = Instantiate(pf_onBreakParticles, _spawnPos, Quaternion.identity);
        ParticleSystem.MainModule _psMain = _ps.main;
        _psMain.startColor = c_spriteRenderer.color;
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
        BrickInitializer _bundle = new BrickInitializer(transform.position, brickType, droppedPowerupType, li_brickHP.max);
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
    /// Return the health remaining of this Brick
    /// </summary>
    public int GetHP()
    {
        return li_brickHP.curr;
    }

    /// <summary>
    /// Damage this Brick
    /// </summary>
    public BrickDamage DamageBrick()
    {
        if (f_isInvincible) return BrickDamage.INVINCIBLE;

        li_brickHP.Decrement();

        if (li_brickHP.isMin()) return BrickDamage.DEATH;
        else
        {
            UpdateSprite();
            return BrickDamage.SUCCESS;
        }
    }

    /// <summary>
    /// Destroy this Brick, spawn a ParticleSystem
    /// </summary>
    public void DestroyBrick()
    {
        OnDeathSpawnParticles(transform.position);
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
            // Damage & potentially destroy Brick
            ManagerBrick.Instance.OnBallHitBrick(_ball, this);
        }
    }
    #endregion
}
