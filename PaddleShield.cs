using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class PaddleShield : MonoBehaviour
{
    private SpriteRenderer c_spriteRenderer;
    private BoxCollider2D c_boxCollider;

    [SerializeField] AnimationCurve easeOutCurve;
    private Vector3 posToGo; // World position to move Shield to once spawned
    private float timeToTravel;
    private float travelProgress = 0f;
    private LimitFloat li_lifeTime = new LimitFloat(-1f, -1f, -1f); 

    private void Awake()
    {
        c_spriteRenderer = GetComponent<SpriteRenderer>();
        c_boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        if (li_lifeTime.curr < 0f) return; // PaddleShield hasn't been initialized yet

        // Timer of lifetime
        li_lifeTime.DecrementBy(Time.deltaTime);
        if (li_lifeTime.isMin()) DestroyShield(); // Ran out of lifetime
        else
        {
            c_spriteRenderer.color = new Color(1f, 1f, 1f, li_lifeTime.GetPercentage()); // Fade sprite as death approaches
        }
    }

    private void FixedUpdate()
    {
        if (li_lifeTime.curr < 0f) return; // PaddleShield hasn't been initialized yet

        // Move PaddleShield towards target position
        travelProgress += Time.fixedDeltaTime / timeToTravel;
        transform.position = Vector3.Lerp(transform.position, posToGo, easeOutCurve.Evaluate(travelProgress));
    }

    public void Initialize(Collider2D _ignoreSpawner, float _ySize, float _xDist, float _timeToTravel, float _lifeTime)
    {
        Physics2D.IgnoreCollision(c_boxCollider, _ignoreSpawner); // Don't collide with Paddle that spawned PaddleShield

        c_spriteRenderer.size = new Vector2(c_spriteRenderer.size.x, _ySize); // Set the height of the PaddleShield
        c_spriteRenderer.flipX = (_xDist < 0f);
        posToGo = transform.position + Utilities.Vec3OnlyX(_xDist);
        timeToTravel = _timeToTravel;
        li_lifeTime = new LimitFloat(_lifeTime, 0f, _lifeTime);
    }

    /*********************************************************************************************************************************************************************************
     * Private Methods
     *********************************************************************************************************************************************************************************/
    #region Private Methods
    /// <summary>
    /// Destroy PaddleShield
    /// </summary>
    private void DestroyShield()
    {
        Destroy(gameObject);
    }
    #endregion

    /*********************************************************************************************************************************************************************************
     * On Event Methods
     *********************************************************************************************************************************************************************************/
    #region On Event Methods
    #endregion

}
