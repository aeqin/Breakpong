using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerManager : MonoBehaviour
{
    #region Singleton
    public static ManagerManager Instance { get; private set; }

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

    private void Start()
    {
        // Control spawn order of the Managers
        ManagerBall.Instance.Initialize();
        ManagerLevel.Instance.Initialize(); // Spawn last, needs to wait for other Managers to subscribe to BeginGame event
    }
}
