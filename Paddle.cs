using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Paddle : MonoBehaviour
{
    protected Rigidbody2D c_rb;
    protected BoxCollider2D c_boxCol;
    protected InputControls inputControls;

    // Paddle Movement Variables
    [SerializeField] protected float paddleMoveSpeed = 10f;
    protected float dirToCenter; // Either -1 or 1 (direction to center of screen)
    protected Vector2 initialPaddlePos;

    // PaddleActionIcon Variables
    [SerializeField] protected PaddleActionIcon pf_ActionOneIcon;
    [SerializeField] protected PaddleActionIcon pf_ActionTwoIcon;
    [SerializeField] protected PaddleActionIcon pf_ActionOneBackground;
    [SerializeField] protected PaddleActionIcon pf_ActionTwoBackground;
    [SerializeField] protected Sprite spr_Empty;
    [SerializeField] protected Sprite spr_Magnet;
    [SerializeField] protected Sprite spr_MagnetOFF;
    [SerializeField] protected Sprite spr_Slam;

    // PaddleAction Variables
    protected List<PaddleAction> list_PA = new List<PaddleAction>();
    protected PaddleAction CurrActionOne { get => GetPaddleActionOne(); set => SetPaddleActionOne(value); }
    protected PaddleAction CurrActionTwo { get => GetPaddleActionTwo(); set => SetPaddleActionTwo(value); }
    protected bool assignNextActionAsOne = true;

    // PaddleAction Empty Variables
    protected PaddleAction PA_Empty;
    protected class PaddleActionEmpty : PaddleAction
    {
        public PaddleActionEmpty(OnStartAction _sM, OnHeldAction _hM, OnCancelAction _cM, DuringPhysics _dPM, OnCollision _oCM, Sprite _sUP, Sprite _sP) : base(_sM, _hM, _cM, _dPM, _oCM, _sUP, _sP) { }
    }

    // PaddleAction Magnet Variables
    protected PaddleAction PA_Magnet;
    protected PaddleAction PA_Magnet_OneUse; // Same magnet action, but one use (used in the beginning of the game)
    protected class PaddleActionMagnet : PaddleAction
    {
        public bool f_magnetized = true;
        public bool f_useOnce = false;
        public Dictionary<Ball, Vector2> dict_ball_offsetPos = new Dictionary<Ball, Vector2>();

        public PaddleActionMagnet(OnStartAction _sM, OnHeldAction _hM, OnCancelAction _cM, DuringPhysics _dPM, OnCollision _oCM, Sprite _sUP, Sprite _sP) : base(_sM, _hM, _cM, _dPM, _oCM, _sUP, _sP) { }
    }

    // PaddleAction Slam Variables
    protected PaddleAction PA_Slam;
    protected class PaddleActionSlam : PaddleAction
    {
        public enum SlamState { IDLE, SLAMMING, RESETTING, };
        public SlamState currState = SlamState.IDLE;
        public Vector2 beforeSlamPos; // Initial Paddle position before Slam movement
        public Vector2 slamAcceleration = new Vector2(17, 0);
        public Vector2 slamDecceleration = new Vector2(21, 0);
        public Vector2 slamInitialVel = new Vector2(16, 0);
        public Vector2 slamReturnVel = new Vector2(-21, 0);
        public float slamDistance = 2;
        public float slamSpeedSwitchProgress = 0.7f;

        public PaddleActionSlam(OnStartAction _sM, OnHeldAction _hM, OnCancelAction _cM, DuringPhysics _dPM, OnCollision _oCM, Sprite _sUP, Sprite _sP) : base(_sM, _hM, _cM, _dPM, _oCM, _sUP, _sP) { }
    }

    /// <summary>
    /// Class used to hold the methods for a PaddleAction depending on if the action button is pressed, held, etc.
    /// </summary>
    protected class PaddleAction
    {
        public delegate void OnStartAction(PaddleAction _PA); // On Input started (first frame of pressed button)
        public delegate void OnHeldAction(PaddleAction _PA); // While button is pressed (second frame +)
        public delegate void OnCancelAction(PaddleAction _PA); // On Input canceled (first frame of released button)
        public delegate void DuringPhysics(PaddleAction _PA); // While each physics frame
        public delegate void OnCollision(PaddleAction _PA, Collision2D _col); // Called when Paddle collides with something

        public OnStartAction onStartActionMethod;
        public OnHeldAction onHeldActionMethod;
        public OnCancelAction onCancelActionMethod;
        public bool f_held = false;
        public DuringPhysics duringPhysicsMethod;
        public OnCollision onCollisionMethod;

        public Sprite spr_unPressed;
        public Sprite spr_pressed;

        public PaddleAction(OnStartAction _startMethod, OnHeldAction _heldMethod, OnCancelAction _cancelMethod, DuringPhysics _duringPhysicsMethod, OnCollision _onCollisionMethod,
                            Sprite _spr_unPressed, Sprite _spr_pressed)
        {
            onStartActionMethod = _startMethod;
            onHeldActionMethod = _heldMethod;
            onCancelActionMethod = _cancelMethod;
            duringPhysicsMethod = _duringPhysicsMethod;
            onCollisionMethod = _onCollisionMethod;
            spr_unPressed = _spr_unPressed;
            spr_pressed = _spr_pressed;

            if (spr_unPressed == null) Debug.LogError("Attemping to create PaddleAction without unPressed sprite");
            if (spr_pressed == null) spr_pressed = spr_unPressed;
        }

        public Sprite GetSpriteByPressed()
        {
            return f_held ? spr_pressed : spr_unPressed;
        }
    }

    protected void Awake()
    {
        inputControls = new InputControls();
        c_rb = GetComponent<Rigidbody2D>();
        c_boxCol = GetComponent<BoxCollider2D>();
        initialPaddlePos = transform.position;

        CreatePaddleActions();
    }

    protected void Update()
    {
        // Potentially call the "held" button method of PaddleActions
        OnPaddleActionHeld(CurrActionOne);
        OnPaddleActionHeld(CurrActionTwo);
    }

    protected void FixedUpdate()
    {
        MovePaddleWithCheck();

        // Potentially call the "during physics" button method of PaddleActions
        OnPaddleActionDuringPhysics(CurrActionOne);
        OnPaddleActionDuringPhysics(CurrActionTwo);
    }

    /*********************************************************************************************************************************************************************************
     * Private Methods
     *********************************************************************************************************************************************************************************/
    /// <summary>
    /// Create all the PaddleAction objects
    /// </summary>
    private void CreatePaddleActions()
    {
        PA_Empty = new PaddleActionEmpty(null, null, null, null, null, spr_Empty, null);
        PA_Magnet = new PaddleActionMagnet(PaddleActionMagnetStart, PaddleActionMagnetHeld, PaddleActionMagnetCancel, PaddleActionMagnetDuring, PaddleActionMagnetCollision, spr_Magnet, spr_MagnetOFF);
        PA_Magnet_OneUse = new PaddleActionMagnet(PaddleActionMagnetStart, PaddleActionMagnetHeld, PaddleActionMagnetCancel, PaddleActionMagnetDuring, PaddleActionMagnetCollision, spr_Magnet, null) { f_useOnce = true }; // Used in the beginning of the game (so Ball starts on Paddle)
        PA_Slam = new PaddleActionSlam(PaddleActionSlamStart, null, null, PaddleActionSlamDuring, null, spr_Slam, null);

        list_PA.Add(PA_Empty);
        list_PA.Add(PA_Magnet);
        list_PA.Add(PA_Magnet_OneUse);
        list_PA.Add(PA_Slam);
    }

    /*********************************************************************************************************************************************************************************
     * Protected Methods
     *********************************************************************************************************************************************************************************/
    /// <summary>
    /// Potentially call PaddleAction button "on press" method
    /// </summary>
    protected void OnPaddleActionStart(PaddleAction _PA)
    {
        if (_PA == null) return; // PaddleAction not assigned

        if (_PA != PA_Empty)
        {
            if (_PA.onStartActionMethod != null) // PaddleAction has a "on press" button method
            {
                _PA.onStartActionMethod(_PA); // Call the "on press" button method
            }

            _PA.f_held = true; // Set flag to consider button as "held" only AFTER calling the method assigned to first frame press
        }

        UpdatePaddleActionIcons(_PA);
    }

    /// <summary>
    /// Potentially call PaddleAction button "held" method
    /// </summary>
    protected void OnPaddleActionHeld(PaddleAction _PA)
    {
        if (_PA == null || _PA == PA_Empty) return; // PaddleAction not assigned


        if (_PA.onHeldActionMethod != null && _PA.f_held) // PaddleAction has a "held" button method, and button is currently pressed
        {
            _PA.onHeldActionMethod(_PA); // Call the "held" button method every frame
        }
    }

    /// <summary>
    /// Potentially call PaddleAction button "on release" method
    /// </summary>
    protected void OnPaddleActionCancel(PaddleAction _PA)
    {
        if (_PA == null) return; // PaddleAction not assigned

        if (_PA != PA_Empty)
        {
            _PA.f_held = false; // UNset flag to consider button as "held" only BEFORE calling the method assigned to button release

            if (_PA.onCancelActionMethod != null) // PaddleAction has a "on release" button method
            {
                _PA.onCancelActionMethod(_PA); // Call the "on release" button method
            }
        }

        UpdatePaddleActionIcons(_PA);
    }

    /// <summary>
    /// Potentially call PaddleAction "during physics" method
    /// </summary>
    protected void OnPaddleActionDuringPhysics(PaddleAction _PA)
    {
        if (_PA == null || _PA == PA_Empty) return; // PaddleAction not assigned


        if (_PA.duringPhysicsMethod != null) // PaddleAction has a "during physics" method
        {
            _PA.duringPhysicsMethod(_PA); // Call the "during physics" button method every physics frame
        }
    }

    /// <summary>
    /// Potentially call PaddleAction "on collision" method
    /// </summary>
    protected void OnPaddleActionCollision(PaddleAction _PA, Collision2D _col)
    {
        if (_PA == null || _PA == PA_Empty) return; // PaddleAction not assigned


        if (_PA.onCollisionMethod != null) // PaddleAction has a "during physics" method
        {
            _PA.onCollisionMethod(_PA, _col); // Call the "during physics" button method every physics frame
        }
    }

    /// <summary>
    /// Assigns the next PaddleAction to either ActionOne or ActionTwo
    /// </summary>
    protected void AssignNextAction(PaddleAction _PA)
    {
        if (assignNextActionAsOne)
        {
            CurrActionOne = _PA;
        }
        else
        {
            CurrActionTwo = _PA;
        }

        UpdatePaddleActionIcons(_PA); // Update PaddleActionIcons of newly assigned PaddleAction

        assignNextActionAsOne = !assignNextActionAsOne; // Flip flag
    }

    /// <summary>
    /// Assigns the next PaddleAction to either ActionOne or ActionTwo
    /// </summary>
    protected void UnassignAction(PaddleAction _PA)
    {
        if (_PA == CurrActionOne)
        {
            CurrActionOne = PA_Empty;
        }
        else// if (_PA == CurrActionTwo)
        {
            CurrActionTwo = PA_Empty;
        }

        _PA.f_held = false; // Unset held flag (since this action is being unassigned and won't trip the OnPaddleActionCancel() method)

        UpdatePaddleActionIcons(_PA); // Update PaddleActionIcons of newly unassigned PaddleAction
    }


/// <summary>
/// Depending on the PaddleAction provided, update the linked Icons
/// </summary>
protected void UpdatePaddleActionIcons(PaddleAction _PA)
    {
        if (CurrActionOne == _PA)
        {
            pf_ActionOneIcon.SetSpriteAs(_PA.GetSpriteByPressed());
            pf_ActionOneBackground.SetPressedSpriteAs(_PA.f_held);
        }
        if (CurrActionTwo == _PA)
        {
            pf_ActionTwoIcon.SetSpriteAs(_PA.GetSpriteByPressed());
            pf_ActionTwoBackground.SetPressedSpriteAs(_PA.f_held);
        }
    }

    /// <summary>
    /// Move Paddle, but make sure that Paddle won't move out of level boundaries
    /// </summary>
    protected void MovePaddleWithCheck()
    {
        Bounds _nextFramePaddleBounds;

        MovePaddle(); // Set velocity first to check movement

        _nextFramePaddleBounds = c_boxCol.bounds;
        _nextFramePaddleBounds.center += (Vector3)c_rb.velocity * Time.fixedDeltaTime; // Check movement with set velocity

        // If set velocity would move Paddle outside of level, set velocity to 0, then move as closely as possible
        if (!ManagerLevel.Instance.IsBoundsInsideLevel(_nextFramePaddleBounds))
        {
            Vector3 _nextDir = c_rb.velocity.normalized; // Store direction of velocity (with no speed)
            c_rb.velocity = Vector2.zero; // Cancel velocity

            _nextFramePaddleBounds = c_boxCol.bounds;
            _nextFramePaddleBounds.center += _nextDir * Time.fixedDeltaTime;
            while (ManagerLevel.Instance.IsBoundsInsideLevel(_nextFramePaddleBounds))
            {
                _nextFramePaddleBounds.center += _nextDir * Time.fixedDeltaTime; // Slowly increment position until nextFrame would leave level bounds
            }
            _nextFramePaddleBounds.center -= _nextDir * Time.fixedDeltaTime; // Since while loop tripped when past bounds, decrement once

            c_rb.MovePosition(_nextFramePaddleBounds.center); // Set position at edge of bounds
        }
    }

    /// <summary>
    /// Move the Paddle in the direction captured by the input vector
    /// </summary>
    protected virtual void MovePaddle()
    {
        Debug.LogError("Override Paddle:MovePaddle() in children");
    }

    /// <summary>
    /// Get the current PaddleActionOne of this particular Paddle
    /// </summary>
    protected virtual PaddleAction GetPaddleActionOne()
    {
        Debug.LogError("Override Paddle:getPaddleActionOne() in children");
        return null;
    }

    /// <summary>
    /// Get the current PaddleActionTwo of this particular Paddle
    /// </summary>
    protected virtual PaddleAction GetPaddleActionTwo()
    {
        Debug.LogError("Override Paddle:getPaddleActionTwo() in children");
        return null;
    }

    /// <summary>
    /// Set the current PaddleActionOne of this particular Paddle
    /// </summary>
    protected virtual void SetPaddleActionOne(PaddleAction _PA)
    {
        Debug.LogError("Override Paddle:setPaddleActionOne() in children");
    }

    /// <summary>
    /// Set the current PaddleActionTwo of this particular Paddle
    /// </summary>
    protected virtual void SetPaddleActionTwo(PaddleAction _PA)
    {
        Debug.LogError("Override Paddle:setPaddleActionTwo() in children");
    }

    /*********************************************************************************************************************************************************************************
     * Public Methods
     *********************************************************************************************************************************************************************************/
    /// <summary>
    /// Get Paddle Ball spawn position. (Middle of Paddle, slightly towards center of the Screen)
    /// </summary>
    public Vector2 GetBallSpawnPos()
    {
        return (Vector2)transform.position + new Vector2(dirToCenter * 0.13f, 0);
    }

    /// <summary>
    /// Called by Ball script on collision, in order to check if Paddle is in a state to influence Ball velocity
    /// </summary>
    /// <param name="ball">The Ball object</param>
    /// <returns>The velocity that Paddle should use to influence the Ball velocity</returns>
    public Vector2 GetPaddleInfluenceVelocityOnBallCollide()
    {
        Vector2 _usableVelocity = Vector2.zero; // How much velocity should actually be used to influence the Ball

        if (CurrActionOne == PA_Slam || CurrActionTwo == PA_Slam)
        {
            PaddleActionSlam _PA_Slam = (PaddleActionSlam)PA_Slam;
            if (_PA_Slam.currState == PaddleActionSlam.SlamState.SLAMMING)
            {
                _usableVelocity.x += c_rb.velocity.x; // During slam movement, use all of Paddle x velocity to influence Ball velocity
            }
        }

        // Use all of Paddle y velocity to influence Ball velocity
        _usableVelocity.y += c_rb.velocity.y;

        return _usableVelocity;
    }

    /// <summary>
    /// When a Paddle picks up a Powerup, handle it
    /// </summary>
    /// <param name="_powerUp">Type of Powerup</param>
    /// <param name="_pickerUpper">The Paddle that picked up the Powerup</param>
    public void AssignActionFromPowerup(ManagerPowerup.PowerupType _powerUp)
    {
        switch (_powerUp)
        {
            case ManagerPowerup.PowerupType.PaddleMagnet:
                AssignNextAction(PA_Magnet);
                break;
            case ManagerPowerup.PowerupType.PaddleMagnetOnce:
                AssignNextAction(PA_Magnet_OneUse);
                break;
            case ManagerPowerup.PowerupType.PaddleSlam:
                AssignNextAction(PA_Slam);
                break;


            default:
                Debug.LogError("Add case to Paddle:AssignActionFromPowerup() for Powerup." + _powerUp.ToString());
                break;
        }
    }

    /// <summary>
    /// When Game begins, reset position and all PaddleActions
    /// </summary>
    public void ResetPaddleAndActions()
    {
        // Reset Paddles
        c_rb.transform.position = initialPaddlePos;

        // Reset PaddleActions
        AssignNextAction(PA_Empty); // Assign one of two
        AssignNextAction(PA_Empty); // Assign two of two
        assignNextActionAsOne = true;

        foreach(PaddleAction _PA in list_PA) // Make sure each PaddleAction is reset to default values
        {
            _PA.f_held = false;
        }

        // Reset sprites
        UpdatePaddleActionIcons(CurrActionOne);
        UpdatePaddleActionIcons(CurrActionTwo);
    }

    /// <summary>
    /// Print current PaddleActions
    /// </summary>
    public void PrintCurrPaddleActions()
    {
        Debug.Log(this + " 1:" + CurrActionOne.ToString() + " 2:" + CurrActionTwo.ToString());
    }
    /*********************************************************************************************************************************************************************************
     * PaddleAction Methods
     *********************************************************************************************************************************************************************************/
    /// <summary>
    /// Start (on press) of PaddleAction Magnet. Shoot Ball forward
    /// </summary>
    protected void PaddleActionMagnetStart(PaddleAction _PA)
    {
        PaddleActionMagnet _PA_Magnet = (PaddleActionMagnet)_PA; // Cast base PaddleAction in PaddleActionMagnet

        foreach (Ball _ball in _PA_Magnet.dict_ball_offsetPos.Keys)
        {
            if (ManagerBall.Instance.IsBallAlive(_ball))
            {
                _ball.LaunchBallInDir(new Vector2(dirToCenter, 0)); // Launch ball away from Paddle
            }
        }

        _PA_Magnet.dict_ball_offsetPos.Clear(); // After all Balls are launched, none remain in "magnetized" dictionary

        // If magnetized Paddle is one use (beginning of the game), then unassign from current action
        if (_PA_Magnet.f_useOnce)
        {
            UnassignAction(_PA_Magnet);
        }
    }

    /// <summary>
    /// Hold (on pressed (second frame +)) of PaddleAction Magnet. Turn off magnet
    /// </summary>
    protected void PaddleActionMagnetHeld(PaddleAction _PA)
    {
        PaddleActionMagnet _PA_Magnet = (PaddleActionMagnet)_PA; // Cast base PaddleAction in PaddleActionMagnet

        _PA_Magnet.f_magnetized = false;
    }

    /// <summary>
    /// Cancel (on release) of PaddleAction Magnet. Turn back on magnet
    /// </summary>
    protected void PaddleActionMagnetCancel(PaddleAction _PA)
    {
        PaddleActionMagnet _PA_Magnet = (PaddleActionMagnet)_PA; // Cast base PaddleAction in PaddleActionMagnet

        _PA_Magnet.f_magnetized = true;
    }

    /// <summary>
    /// During (each physics frame) of PaddleAction Magnet. Move Ball with Paddle if "Captured"
    /// </summary>
    protected void PaddleActionMagnetDuring(PaddleAction _PA)
    {
        PaddleActionMagnet _PA_Magnet = (PaddleActionMagnet)_PA; // Cast base PaddleAction in PaddleActionMagnet

        Vector2 _nextFramePaddlePos = c_rb.position + c_rb.velocity * Time.fixedDeltaTime; // Position of Paddle next physics frame

        foreach ((Ball _ball, Vector2 offsetPos) in _PA_Magnet.dict_ball_offsetPos)
        {
            if (ManagerBall.Instance.IsBallAlive(_ball))
            {
                _ball.MoveBallWithMagnetizedPaddle(_nextFramePaddlePos + offsetPos); // Tell magnetized Ball to move to Paddle position next frame, with offset
            }
        }
    }

    /// <summary>
    /// On collision PaddleAction Magnet. "Capture" Ball on contact
    /// </summary>
    protected void PaddleActionMagnetCollision(PaddleAction _PA, Collision2D _col)
    {
        PaddleActionMagnet _PA_Magnet = (PaddleActionMagnet)_PA; // Cast base PaddleAction in PaddleActionMagnet

        if (!_PA_Magnet.f_magnetized)
        {
            return; // If magnet is currently off, do NOT freeze Ball on Paddle
        }

        // Did Paddle hit a Ball?
        if (_col.gameObject.TryGetComponent<Ball>(out Ball _ball))
        {
            Vector2 _ballPos = _ball.FreezeBallOnPaddle(c_boxCol.bounds); // Tell Ball to freeze itself on Paddle
            Vector2 _paddleToBallOffset = _ballPos - c_rb.position;
            _paddleToBallOffset.x = dirToCenter * c_boxCol.bounds.size.x;
            if (!_PA_Magnet.dict_ball_offsetPos.ContainsKey(_ball))
                _PA_Magnet.dict_ball_offsetPos[_ball] = _paddleToBallOffset;
        }
    }

    /// <summary>
    /// Start (on press) of PaddleAction Slam
    /// </summary>
    protected void PaddleActionSlamStart(PaddleAction _PA)
    {
        PaddleActionSlam _PA_Slam = (PaddleActionSlam)_PA; // Cast base PaddleAction in PaddleActionSlam

        if (_PA_Slam.currState != PaddleActionSlam.SlamState.IDLE)
        {
            return; // Not idle, so do nothing
        }

        _PA_Slam.beforeSlamPos = transform.position;
        _PA_Slam.currState = PaddleActionSlam.SlamState.SLAMMING; // Begin slam
        c_rb.velocity += (_PA_Slam.slamInitialVel * dirToCenter);
    }

    /// <summary>
    /// During (each physics frame) of PaddleAction Slam. Quickly move the Paddle towards the center of the screen, then slowly return
    /// </summary>
    protected void PaddleActionSlamDuring(PaddleAction _PA)
    {
        PaddleActionSlam _PA_Slam = (PaddleActionSlam)_PA; // Cast base PaddleAction in PaddleActionSlam


        if (_PA_Slam.currState == PaddleActionSlam.SlamState.IDLE)
        {
            return; // Do nothing
        }

        else if (_PA_Slam.currState == PaddleActionSlam.SlamState.SLAMMING)
        {
            float _nextXPos = transform.position.x + c_rb.velocity.x * Time.fixedDeltaTime; // The next x position of Paddle during Slam movement
            float _slamEndXPos = initialPaddlePos.x + (_PA_Slam.slamDistance * dirToCenter); // The x position at the end of Slam movement
            float _slamProgress = (_PA_Slam.beforeSlamPos.x - transform.position.x) / (_slamEndXPos - initialPaddlePos.x); // Percentage progress until reaching Slam end x position

            // Stop
            if (Mathf.Abs(_nextXPos) <= Mathf.Abs(_slamEndXPos)) // Paddle will exceed Slam distance next frame
            {
                c_rb.velocity = new Vector2(0, c_rb.velocity.y); // Stop x movement
                c_rb.MovePosition(new Vector2(_slamEndXPos, transform.position.y)); // Set Paddle to end of Slam position
                _PA_Slam.currState = PaddleActionSlam.SlamState.RESETTING; // Begin to reset Slam position
            }

            // Accelerate
            else if (_slamProgress < _PA_Slam.slamSpeedSwitchProgress)
            {
                c_rb.velocity += (_PA_Slam.slamAcceleration * dirToCenter);
            }

            // Deccelerate
            else
            {
                c_rb.velocity += (_PA_Slam.slamDecceleration * dirToCenter * -1);
            }
        }

        else if (_PA_Slam.currState == PaddleActionSlam.SlamState.RESETTING)
        {
            float _nextXPos = transform.position.x + c_rb.velocity.x * Time.fixedDeltaTime; // The next x position of Paddle during Slam return

            if (Mathf.Abs(_nextXPos) >= Mathf.Abs(initialPaddlePos.x)) // Paddle will exceed Slam distance next frame
            {
                c_rb.velocity = new Vector2(0, c_rb.velocity.y); // Stop x movement
                c_rb.MovePosition(new Vector2(initialPaddlePos.x, transform.position.y)); // Set Paddle to initial x position
                _PA_Slam.currState = PaddleActionSlam.SlamState.IDLE; // Finish slam state
            }
            else
            {
                c_rb.velocity += (_PA_Slam.slamReturnVel * dirToCenter);
            }
        }
    }

    /*********************************************************************************************************************************************************************************
     * On Event Methods
     *********************************************************************************************************************************************************************************/
    protected void OnCollisionEnter2D(Collision2D collision)
    {
        OnPaddleActionCollision(CurrActionOne, collision);
        OnPaddleActionCollision(CurrActionTwo, collision);
    }
}
