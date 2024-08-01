using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerBrick : MonoBehaviour
{
    #region Singleton
    public static ManagerBrick Instance { get; private set; }

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

    /// <summary>
    /// Enum of every type of Brick in the game
    /// </summary>
    public enum BrickType
    {
        None,
        Normal,
    }

    [SerializeField] GameObject FLDR_Bricks; // Reference to folder of Bricks
    [SerializeField] Brick pf_brick;
    private List<Brick> list_bricks = new List<Brick>();
    private List<Brick.BrickInitializer> list_brickInitializers = new List<Brick.BrickInitializer>(); // Only hold Bricks created on start of level, not afterwards during gameplay

    public void Initialize()
    {
        // Subscribe to Events
        ManagerLevel.Instance.EventBeginGame += OnBeginGame;
    }

    /*********************************************************************************************************************************************************************************
     * Private Methods
     *********************************************************************************************************************************************************************************/
    #region Private Methods
    /// <summary>
    /// Create a Brick at a particular position, and add Brick to list of Brick
    /// </summary>
    private Brick CreateBrickAt(Vector2 _spawnPos)
    {
        Brick _brick = Instantiate(pf_brick, FLDR_Bricks.transform);
        _brick.transform.position = _spawnPos;
        list_bricks.Add(_brick);

        return _brick;
    }

    /// <summary>
    /// Remove Ball from list, and ask itself to Destroy itself
    /// </summary>
    private void RemoveBrick(Brick _brickToDestroy)
    {
        list_bricks.RemoveAll(_brick => _brick == _brickToDestroy);

        // If Brick can spawn Powerup, then spawn Powerup of type
        ManagerPowerup.PowerupType _droppedPowerup = _brickToDestroy.GetDroppedPowerupType();
        if (_droppedPowerup != ManagerPowerup.PowerupType.None)
        {
            ManagerPowerup.Instance.SpawnSpecificPowerupAt(_droppedPowerup, _brickToDestroy.transform.position);
        }

        _brickToDestroy.DestroyBrick();

        if (list_bricks.Count == 0) // No more Bricks left, level complete
        {
            ManagerLevel.Instance.OnNoBricksLeft();
        }
    }
    #endregion

    /*********************************************************************************************************************************************************************************
     * Public Methods
     *********************************************************************************************************************************************************************************/
    #region Private Methods
    /// <summary>
    /// Spawn a Brick at given position
    /// </summary>
    /// <param name="_spawnPos">World position of Brick</param>
    public void SpawnPowerupAt(Vector2 _spawnPos)
    {
        CreateBrickAt(_spawnPos);
    }

    /// <summary>
    /// Spawn a Brick at given position
    /// </summary>
    /// <param name="_spawnPos">World position of Brick</param>
    public void OnBallHitBrick(Ball _ball, Brick _brick)
    {
        Brick.BrickDamage _result = _brick.DamageBrick();

        switch (_result)
        {
            case Brick.BrickDamage.SUCCESS:
                ManagerLevel.Instance.UpdateScoreOnBrickHitByBall(_brick, _ball);
                break;
            case Brick.BrickDamage.INVINCIBLE:
                break;
            case Brick.BrickDamage.DEATH:
                ManagerLevel.Instance.UpdateScoreOnBrickHitByBall(_brick, _ball);
                RemoveBrick(_brick);
                break;
        }
    }

    /// <summary>
    /// Reset Bricks for current level
    /// </summary>
    public void ResetBricks()
    {
        // Delete each remaining Brick
        foreach (Brick _brickToDestroy in list_bricks)
        {
            _brickToDestroy.DestroyBrick();
        }
        list_bricks.Clear();

        // Respawn each Brick as in the beginning of the level
        foreach (Brick.BrickInitializer _initBundle in list_brickInitializers)
        {
            Brick _respawnedBrick =  CreateBrickAt(_initBundle.spawnPos);
            _respawnedBrick.Initialize(_initBundle);
        }
    }
    #endregion

    /*********************************************************************************************************************************************************************************
     * On Event Methods
     *********************************************************************************************************************************************************************************/
    #region On Event Methods
    /// <summary>
    /// When Game begins, find a list of already spawned Bricks, and save their BrickInitializer values, in order to re-create them on level reset
    /// </summary>
    private void OnBeginGame()
    {
        list_brickInitializers.Clear();
        ResetBricks();

        foreach (Transform _childTransform in FLDR_Bricks.transform) // Iterate through Bricks folder GameObject
        {
            if (_childTransform.gameObject.activeInHierarchy && _childTransform.gameObject.TryGetComponent<Brick>(out Brick _brick))
            {
                Brick.BrickInitializer _brickInit = _brick.GetBrickInitializer();

                list_bricks.Add(_brick);
                list_brickInitializers.Add(_brickInit);
                _brick.Initialize(_brickInit);
            }
        }
    }
    #endregion
}
