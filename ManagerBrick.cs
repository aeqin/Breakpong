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

    [SerializeField] Brick pf_brick;
    private List<Brick> list_bricks = new List<Brick>();

    /*********************************************************************************************************************************************************************************
     * Private Methods
     *********************************************************************************************************************************************************************************/
    /// <summary>
    /// Create a Brick at a particular position, and add Brick to list of Brick
    /// </summary>
    private Brick CreateBrickAt(Vector2 _spawnPos)
    {
        Brick _brick = Instantiate(pf_brick, _spawnPos, Quaternion.identity);
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
    }

    /*********************************************************************************************************************************************************************************
     * Public Methods
     *********************************************************************************************************************************************************************************/
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
        ManagerLevel.Instance.UpdateScoreOnBrickDestroyedByBall(_brick, _ball);

        RemoveBrick(_brick);
    }
}
