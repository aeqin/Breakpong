using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

public class Paddle : MonoBehaviour
{
    protected Rigidbody2D c_rb;
    protected SpriteRenderer c_spriteRenderer;
    protected BoxCollider2D c_boxCol;
    protected InputControls inputControls;

    // Paddle Movement Variables
    [SerializeField] protected float paddleMoveSpeed = 10f;
    protected int dirToCenter; // Either -1 or 1 (direction from Paddle to center of screen)
    protected Vector2 initialPaddlePos;
    protected Vector2 paddleInputVector;
    protected enum PaddleMoveState { DEFAULT, IGNORE, };
    protected PaddleMoveState currPaddleMoveState = PaddleMoveState.DEFAULT;

    // PaddleActionIcon Variables
    [SerializeField] protected PaddleActionIcon pf_ActionOneIcon;
    [SerializeField] protected PaddleActionIcon pf_ActionTwoIcon;
    [SerializeField] protected PaddleActionIcon pf_ActionOneBackground;
    [SerializeField] protected PaddleActionIcon pf_ActionTwoBackground;
    [SerializeField] protected PaddleActionIconSpriteLib pf_PAIconSprLib;

    // PaddleAction Variables
    #region PaddleAction Definitions
    protected List<PaddleAction> list_PA = new List<PaddleAction>();
    protected PaddleAction currActionOne;
    protected PaddleAction currActionTwo;
    protected bool assignNextActionAsOne = true;

    // PaddleAction Empty Variables
    protected PaddleAction PA_Empty;
    protected class PaddleActionEmpty : PaddleAction
    {
        public PaddleActionEmpty(Sprite _sUP, Sprite _sP, int _nP = -1, float _aD = -1f) : base(_sUP, _sP, _nP, _aD) { }
    }

    // PaddleAction Magnet Variables
    protected PaddleAction PA_Magnet;
    protected PaddleAction PA_Magnet_OneUse; // Same magnet action, but one use (used in the beginning of the game)
    protected class PaddleActionMagnet : PaddleAction
    {
        public bool f_magnetized = true;
        public bool f_useOnce = false;
        public Dictionary<Ball, Vector2> dict_ball_offsetPos = new Dictionary<Ball, Vector2>();

        public PaddleActionMagnet(Sprite _sUP, Sprite _sP, int _nP = -1, float _aD = -1f) : base(_sUP, _sP, _nP, _aD) { }

        /// <summary>
        /// Resets this PaddleAction attributes
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            f_magnetized = true;
        }
    }

    // PaddleAction Slam Variables
    protected PaddleAction PA_Slam;
    protected class PaddleActionSlam : PaddleAction
    {
        public enum SlamState { IDLE, SLAMMING, RESETTING, };
        private  SlamState _currState;
        public SlamState CurrState
        {  
            get
            {
                return _currState;
            }
            set
            {
                _currState = value;
                if (f_delayedUnassignment && value == SlamState.IDLE) // Unassignment was delayed until Paddle stopped moving
                {
                    f_forceUnassignment = true; // Allow PaddleActionSlam to be unassigned
                }
            }
        }
        public Vector2 beforeSlamPos; // Initial Paddle position before Slam movement
        public Vector2 slamAcceleration = new Vector2(17, 0);
        public Vector2 slamDecceleration = new Vector2(21, 0);
        public Vector2 slamInitialVel = new Vector2(16, 0);
        public Vector2 slamReturnVel = new Vector2(-21, 0);
        public float slamDistance = 2;
        public float slamSpeedSwitchProgress = 0.7f;

        public PaddleActionSlam(Sprite _sUP, Sprite _sP, int _nP = -1, float _aD = -1f) : base(_sUP, _sP, _nP, _aD) { }

        /// <summary>
        /// Check to see if PaddleActionSlam should delay unassignment (not yet idle, means Paddle is out of position)
        /// </summary>
        public override bool CheckDelayUnassign()
        {
            bool _baseCheck = base.CheckDelayUnassign();

            // If base method decides that PaddleAction should NOT delay unassignment, check for PaddleActionSlam specific criteria
            if (!_baseCheck)
            {
                if (CurrState != SlamState.IDLE) // Paddle is currently still moving, so delay unassignment until IDLE state reached again
                {
                    f_delayedUnassignment = true;
                    return true; // Do delay
                }
            }

            return _baseCheck;
        }

        /// <summary>
        /// Resets this PaddleAction attributes
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            CurrState = SlamState.IDLE;
        }
    }

    // PaddleAction GhostPaddle Variables
    [SerializeField] protected PaddleGhost pf_PaddleGhost;
    protected PaddleAction PA_GhostPaddle;
    protected class PaddleActionGhostPaddle : PaddleAction
    {
        public PaddleGhost currGhostPaddle = null;
        public bool f_controlCurrGhost = false;

        public PaddleActionGhostPaddle(Sprite _sUP, Sprite _sP, int _nP = -1, float _aD = -1f) : base(_sUP, _sP, _nP, _aD) { }

        /// <summary>
        /// Check to see if PaddleActionSlam should delay unassignment (not yet idle, means Paddle is out of position)
        /// </summary>
        public override bool CheckDelayUnassign()
        {
            bool _baseCheck = base.CheckDelayUnassign();

            // If base method decides that PaddleAction should NOT delay unassignment, check for PaddleActionGhostPaddle specific criteria
            if (!_baseCheck)
            {
                if (f_controlCurrGhost && currGhostPaddle != null) // Still currently controlling a PaddleGhost, do not unassign until Paddle regains control
                {
                    f_delayedUnassignment = true;
                    return true; // Do delay
                }
            }

            return _baseCheck;
        }

        /// <summary>
        /// Resets this PaddleAction attributes
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            f_controlCurrGhost = false;
        }
    }

    /// <summary>
    /// Class used to hold the methods for a PaddleAction depending on event if the action button is pressed, held, release, or collision, etc.
    /// </summary>
    protected class PaddleAction
    {
        public delegate bool OnPressAction(PaddleAction _PA); // On Input started (first frame of pressed button)
        public delegate void OnHeldAction(PaddleAction _PA); // While button is pressed (second frame +)
        public delegate void OnReleaseAction(PaddleAction _PA); // On Input released (first frame of released button)
        public delegate void DuringPhysics(PaddleAction _PA); // While each physics frame
        public delegate void OnCollision(PaddleAction _PA, Collision2D _col); // Called when Paddle collides with something
        public delegate void OnAssignAction(PaddleAction _PA); // Called when PaddleAction is assigned to be active
        public delegate void OnUnassignAction(PaddleAction _PA); // Called when PaddleAction is unassigned to be inactive

        public OnPressAction onPressActionMethod;
        public OnHeldAction onHeldActionMethod;
        public OnReleaseAction onReleaseActionMethod;
        public DuringPhysics duringPhysicsMethod;
        public OnCollision onCollisionMethod;
        public OnAssignAction onAssignMethod;
        public OnUnassignAction onUnassignMethod;

        // Flags
        public bool f_held = false;
        public bool f_delayedUnassignment = false; // Has this PaddleAction delayed an unassignment?
        public bool f_forceUnassignment = false; // Allow this PaddleAction to be forcibly unassigned

        // Sprites of PaddleAction
        public Sprite spr_unPressed;
        public Sprite spr_pressed;

        // Uses of PaddleAction
        public bool f_pressLimited = false; // Is PaddleAction limited by number of presses?
        public LimitInt li_presses;

        // Duration of PaddleAction
        public bool f_durationLimited = false; // Is PaddleAction limited by duration?
        public LimitFloat li_duration;

        public PaddleAction(Sprite _spr_unPressed, Sprite _spr_pressed, int _numPresses = -1, float _activeDuration = -1f)
        {
            spr_unPressed = _spr_unPressed;
            spr_pressed = _spr_pressed;
            if (_numPresses > 0)
            {
                li_presses = new LimitInt(_numPresses, 0, _numPresses);
                f_pressLimited = true;
            }
            if (_activeDuration > 0f)
            {
                li_duration = new LimitFloat(_activeDuration, 0, _activeDuration);
                f_durationLimited = true;
            }

            if (spr_unPressed == null) Debug.LogError("Attemping to create PaddleAction without unPressed sprite");
            if (spr_pressed == null) spr_pressed = spr_unPressed;
        }

        /// <summary>
        /// Return either the sprite of this PaddleAction when action button pressed, or unpressed
        /// </summary>
        public Sprite GetSpriteByPressed()
        {
            return f_held ? spr_pressed : spr_unPressed;
        }

        /// <summary>
        /// Check to see if PaddleAction should delay unassignment (perhaps in the middle of some action)
        /// </summary>
        public virtual bool CheckDelayUnassign()
        {
            bool _doDelay = true; 

            if (!f_pressLimited && !f_durationLimited) return false; // Don't delay if PaddleAction isn't limited

            if (f_pressLimited && li_presses.isMin()) return false; // Don't delay if PaddleAction is press-limited and out of presses

            if (f_durationLimited && li_duration.isMin()) return false; // Don't delay if PaddleAction is duration-limited and out of time

            // Made it here, means _doDelay is true
            f_delayedUnassignment = true;

            return _doDelay;
        }

        /// <summary>
        /// Restores this PaddleAction's presses / durations
        /// </summary>
        public virtual void Restore()
        {
            f_delayedUnassignment = false;
            f_forceUnassignment = false;
            if (f_pressLimited) li_presses.resetToMax();
            if (f_durationLimited) li_duration.resetToMax();
        }

        /// <summary>
        /// Resets this PaddleAction attributes
        /// </summary>
        public virtual void Reset()
        {
            f_held = false;
            Restore();
        }
    }
    #endregion

    protected void Awake()
    {
        inputControls = new InputControls();
        c_rb = GetComponent<Rigidbody2D>();
        c_spriteRenderer = GetComponent<SpriteRenderer>();
        c_boxCol = GetComponent<BoxCollider2D>();
        initialPaddlePos = transform.position;

        CreatePaddleActions();
    }

    protected void Update()
    {
        // Potentially call the "held" button method of PaddleActions
        OnPaddleActionHeld(currActionOne);
        OnPaddleActionHeld(currActionTwo);
    }

    protected virtual void FixedUpdate()
    {
        MovePaddleWithCheck();

        // Potentially call the "during physics" button method of PaddleActions
        OnPaddleActionDuringPhysics(currActionOne);
        OnPaddleActionDuringPhysics(currActionTwo);
    }

    /*********************************************************************************************************************************************************************************
     * Private Methods
     *********************************************************************************************************************************************************************************/
    #region Private Methods
    #endregion

    /*********************************************************************************************************************************************************************************
     * Protected Methods
     *********************************************************************************************************************************************************************************/
    #region Protected Methods
    /// <summary>
    /// Create all the PaddleAction objects
    /// </summary>
    protected virtual void CreatePaddleActions()
    {
        PA_Empty = new PaddleActionEmpty(pf_PAIconSprLib.spr_Empty, null, -1, -1f);
        PA_Magnet = new PaddleActionMagnet(pf_PAIconSprLib.spr_Magnet, pf_PAIconSprLib.spr_MagnetOFF, -1, 20f)
        {
            onPressActionMethod = PaddleActionMagnetPress,
            onHeldActionMethod = PaddleActionMagnetHeld,
            onReleaseActionMethod = PaddleActionMagnetRelease,
            duringPhysicsMethod = PaddleActionMagnetDuring,
            onCollisionMethod = PaddleActionMagnetCollision,
            onUnassignMethod = PaddleActionMagnetUnassign,
        };
        PA_Magnet_OneUse = new PaddleActionMagnet(pf_PAIconSprLib.spr_Magnet, null, -1, -1f) // Used in the beginning of the game (so Ball starts on Paddle)
        {
            f_useOnce = true, // Allow Magnet to work only ONCE
            onPressActionMethod = PaddleActionMagnetPress,
            onHeldActionMethod = PaddleActionMagnetHeld,
            onReleaseActionMethod = PaddleActionMagnetRelease,
            duringPhysicsMethod = PaddleActionMagnetDuring,
            onCollisionMethod = PaddleActionMagnetCollision,
        };
        PA_Slam = new PaddleActionSlam(pf_PAIconSprLib.spr_Slam, null, 3, -1f)
        {
            onPressActionMethod = PaddleActionSlamPress,
            duringPhysicsMethod = PaddleActionSlamDuring,
        };
        PA_GhostPaddle = new PaddleActionGhostPaddle(pf_PAIconSprLib.spr_GhostPaddle, null, 6, -1f)
        {
            onPressActionMethod = PaddleActionGhostPaddlePress,
            onHeldActionMethod = PaddleActionGhostPaddleHeld,
            onReleaseActionMethod = PaddleActionGhostPaddleRelease,
            duringPhysicsMethod = PaddleActionGhostPaddleDuring,
            onUnassignMethod = PaddleActionGhostPaddleUnassign,
        };

        list_PA.Add(PA_Empty);
        list_PA.Add(PA_Magnet);
        list_PA.Add(PA_Magnet_OneUse);
        list_PA.Add(PA_Slam);
        list_PA.Add(PA_GhostPaddle);
    }

    /// <summary>
    /// Potentially call PaddleAction button "on press" method
    /// </summary>
    protected void OnPaddleActionPress(PaddleAction _PA)
    {
        if (_PA == null) return; // PaddleAction not assigned

        if (_PA != PA_Empty)
        {
            bool _f_hasPressesLeft = (_PA.f_pressLimited && !_PA.li_presses.isMin()); 
            bool _f_allowPressMethod = !_PA.f_pressLimited || _f_hasPressesLeft; // PaddleAction is NOT press-limited, OR, is press-limited AND has presses left

            if (_PA.onPressActionMethod != null && _f_allowPressMethod) // PaddleAction has a "on press" button method
            {
                bool _pressSucceeded = _PA.onPressActionMethod(_PA); // Call the "on press" button method

                if (_PA.f_pressLimited && _pressSucceeded) // Decrement a press, ONLY if action succeeded
                {
                    _PA.li_presses.Decrement();
                }
            }

            // If PaddleAction is press-limited, and out of presses
            if (_PA.f_pressLimited && _PA.li_presses.isMin())
            {
                UnassignAction(_PA); // Unassign PaddleAction which ran out of presses
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
    protected void OnPaddleActionRelease(PaddleAction _PA)
    {
        if (_PA == null) return; // PaddleAction not assigned

        if (_PA != PA_Empty)
        {
            _PA.f_held = false; // UNset flag to consider button as "held" only BEFORE calling the method assigned to button release

            if (_PA.onReleaseActionMethod != null) // PaddleAction has a "on release" button method
            {
                _PA.onReleaseActionMethod(_PA); // Call the "on release" button method
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
            _PA.duringPhysicsMethod(_PA); // Call the "during physics" method every physics frame
        }

        // Count down a timer, if PaddleAction is duration-limited
        if (_PA.f_durationLimited)
        {
            _PA.li_duration.DecrementBy(Time.fixedDeltaTime);
            if (_PA.li_duration.isMin()) // Duration finished
            {
                UnassignAction(_PA); // Unassign PaddleAction which ran out of time
            }
        }

        // Unassign PaddleAction, which previously delayed unassignment
        if (_PA.f_forceUnassignment)
        {
            UnassignAction(_PA);
        }
    }

    /// <summary>
    /// Potentially call PaddleAction "on collision" method
    /// </summary>
    protected void OnPaddleActionCollision(PaddleAction _PA, Collision2D _col)
    {
        if (_PA == null || _PA == PA_Empty) return; // PaddleAction not assigned


        if (_PA.onCollisionMethod != null) // PaddleAction has a "on collision" method
        {
            _PA.onCollisionMethod(_PA, _col); // Call the "on collision" method
        }
    }

    /// <summary>
    /// Potentially call PaddleAction "on assign" method
    /// </summary>
    protected void OnPaddleActionAssigned(PaddleAction _PA)
    {
        if (_PA == null || _PA == PA_Empty) return; // PaddleAction not assigned


        if (_PA.onAssignMethod != null) // PaddleAction has a "on assign" method
        {
            _PA.onAssignMethod(_PA); // Call the "on assign" method
        }
    }

    /// <summary>
    /// Potentially call PaddleAction "on assign" method
    /// </summary>
    protected void OnPaddleActionUnassigned(PaddleAction _PA)
    {
        if (_PA == null || _PA == PA_Empty) return; // PaddleAction not assigned


        if (_PA.onUnassignMethod != null) // PaddleAction has a "on unassign" method
        {
            _PA.onUnassignMethod(_PA); // Call the "on unassign" method
        }
    }

    /// <summary>
    /// Assigns the next PaddleAction to either ActionOne or ActionTwo
    /// </summary>
    /// <returns>Whether or not PaddleAction was successfully assigned</returns>
    protected bool AssignNextAction(PaddleAction _PA)
    {
        // Next PaddleAction to assign is ALREADY assigned, so simply restore the current one
        if (_PA != PA_Empty && (currActionOne == _PA || currActionTwo == _PA))
        {
            _PA.Restore(); // Restore (presses/duration)
            return true;
        }

        // Next PaddleAction is NOT already assigned, so assign to the next ready action slot
        if (assignNextActionAsOne)
        {
            if (!UnassignAction(currActionOne)) // Reset old
            {
                return false; // Could not unassign old PaddleAction, so return false
            }
            currActionOne = _PA; // Assign new
        }
        else
        {
            if (!UnassignAction(currActionTwo)) // Reset old
            {
                return false; // Could not unassign old PaddleAction, so return false
            }
            currActionTwo = _PA; // Assign new
        }
        assignNextActionAsOne = !assignNextActionAsOne; // Flip flag

        OnPaddleActionAssigned(_PA); // Call PaddleAction "on assign" method

        UpdatePaddleActionIcons(_PA); // Update PaddleActionIcons of newly assigned PaddleAction

        return true;
    }

    /// <summary>
    /// Assigns the next PaddleAction to either ActionOne or ActionTwo
    /// </summary>
    /// <returns>Whether or not PaddleAction was successfully unassigned</returns>
    protected bool UnassignAction(PaddleAction _PA)
    {
        if (_PA == null) return true; // Return true if already unassigned

        if (!_PA.f_forceUnassignment && _PA.CheckDelayUnassign())
        {
            return false; // Return false if PaddleAction is in the middle of some action and can't yet be unassigned
        }

        _PA.Reset(); // Reset current PaddleAction

        OnPaddleActionUnassigned(_PA); // Call PaddleAction "on unassign" method

        if (_PA == currActionOne)
        {
            currActionOne = PA_Empty;
            UpdatePaddleActionIcons(currActionOne);
        }
        else// if (_PA == CurrActionTwo)
        {
            currActionTwo = PA_Empty;
            UpdatePaddleActionIcons(currActionTwo);
        }

        return true;
    }

    /// <summary>
    /// Depending on the PaddleAction provided, update the linked Icons
    /// </summary>
    protected virtual void UpdatePaddleActionIcons(PaddleAction _PA)
    {
        if (currActionOne == _PA)
        {
            pf_ActionOneIcon.SetSpriteAs(_PA.GetSpriteByPressed());
            pf_ActionOneBackground.SetPressedSpriteAs(_PA.f_held);
        }
        if (currActionTwo == _PA)
        {
            pf_ActionTwoIcon.SetSpriteAs(_PA.GetSpriteByPressed());
            pf_ActionTwoBackground.SetPressedSpriteAs(_PA.f_held);
        }
    }

    /// <summary>
    /// Move the Paddle in the direction captured by the input vector
    /// </summary>
    protected virtual void MovePaddle()
    {
        c_rb.velocity = paddleInputVector * paddleMoveSpeed;
    }

    /// <summary>
    /// Move Paddle, but make sure that Paddle won't move out of level boundaries
    /// </summary>
    protected void MovePaddleWithCheck()
    {
        if (currPaddleMoveState == PaddleMoveState.IGNORE) return; // If Paddle movement is set to IGNORE, then ignore move Paddle by Player input

        MovePaddle(); // Set velocity first with MovePaddle() before checking movement

        Bounds _nextFramePaddleBounds;
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
    #endregion

    /*********************************************************************************************************************************************************************************
     * Public Methods
     *********************************************************************************************************************************************************************************/
    #region Public Methods
    /// <summary>
    /// Get Paddle Ball spawn position. (Middle of Paddle, slightly towards center of the Screen)
    /// </summary>
    public Vector2 GetBallSpawnPos()
    {
        return (Vector2)transform.position + new Vector2(dirToCenter * 0.13f, 0);
    }

    /// <summary>
    /// Get the box collider size of this Paddle
    /// </summary>
    public Vector2 GetBoxColliderSize()
    {
        return c_boxCol.size;
    }

    /// <summary>
    /// Get the input Vector2 of this Paddle
    /// </summary>
    public Vector2 GetInputVector()
    {
        return paddleInputVector;
    }

    /// <summary>
    /// Get the direction towards the center of the screen for this Paddle. Always -1 or 1
    /// </summary>
    public int GetDirToCenter()
    {
        return dirToCenter;
    }

    /// <summary>
    /// Returns whether or not Paddle will reset Ball score multiplier on hit
    /// </summary>
    public virtual bool IsBallScoreMultiplierResetter()
    {
        return true;
    }

    /// <summary>
    /// Called by Ball script on collision, in order to check if Paddle is in a state to influence Ball velocity
    /// </summary>
    /// <returns>The velocity that Paddle should use to influence the Ball velocity</returns>
    public virtual Vector2 GetPaddleInfluenceVelocityOnBallCollide()
    {
        Vector2 _usableVelocity = Vector2.zero; // How much velocity should actually be used to influence the Ball

        if (currActionOne == PA_Slam || currActionTwo == PA_Slam)
        {
            PaddleActionSlam _PA_Slam = (PaddleActionSlam)PA_Slam;
            if (_PA_Slam.CurrState == PaddleActionSlam.SlamState.SLAMMING)
            {
                _usableVelocity.x += c_rb.velocity.x; // During slam movement, use all of Paddle x velocity to influence Ball velocity
            }
        }

        // Use all of Paddle y velocity to influence Ball velocity
        _usableVelocity.y += c_rb.velocity.y;

        return _usableVelocity;
    }

    /// <summary>
    /// Have this Paddle regain input control
    /// </summary>
    public void RegainInputControl()
    {
        currPaddleMoveState = PaddleMoveState.DEFAULT;
    }

    /// <summary>
    /// Have this Paddle lose input control
    /// </summary>
    public void LoseInputControl()
    {
        currPaddleMoveState = PaddleMoveState.IGNORE;
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
            case ManagerPowerup.PowerupType.PaddleGhostPaddle:
                AssignNextAction(PA_GhostPaddle);
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
        // Reset PaddleActions
        AssignNextAction(PA_Empty); // Assign one of two
        AssignNextAction(PA_Empty); // Assign two of two
        assignNextActionAsOne = true;

        foreach(PaddleAction _PA in list_PA) // Make sure each PaddleAction is reset to default values
        {
            _PA.Reset();
        }

        // Reset Paddle position
        c_rb.transform.position = initialPaddlePos;

        // Reset sprites
        UpdatePaddleActionIcons(currActionOne);
        UpdatePaddleActionIcons(currActionTwo);
    }

    /// <summary>
    /// Print current PaddleActions
    /// </summary>
    public void PrintCurrPaddleActions()
    {
        Debug.Log(this + " 1:" + currActionOne.ToString() + " 2:" + currActionTwo.ToString());
    }
    #endregion

    /*********************************************************************************************************************************************************************************
     * PaddleAction Methods
     *********************************************************************************************************************************************************************************/
    #region PaddleAction Methods
    /// <summary>
    /// On press (of button) of PaddleAction Magnet. Shoot Ball forward.
    /// </summary>
    /// <returns>Returns true</returns>
    protected bool PaddleActionMagnetPress(PaddleAction _PA)
    {
        PaddleActionMagnet _PA_Magnet = (PaddleActionMagnet)_PA; // Cast base PaddleAction into PaddleActionMagnet

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

        return true;
    }

    /// <summary>
    /// Hold (on pressed (second frame +)) of PaddleAction Magnet. Turn off magnet
    /// </summary>
    protected void PaddleActionMagnetHeld(PaddleAction _PA)
    {
        PaddleActionMagnet _PA_Magnet = (PaddleActionMagnet)_PA; // Cast base PaddleAction into PaddleActionMagnet

        _PA_Magnet.f_magnetized = false;
    }

    /// <summary>
    /// On release (of button) of PaddleAction Magnet. Turn back on magnet
    /// </summary>
    protected void PaddleActionMagnetRelease(PaddleAction _PA)
    {
        PaddleActionMagnet _PA_Magnet = (PaddleActionMagnet)_PA; // Cast base PaddleAction into PaddleActionMagnet

        _PA_Magnet.f_magnetized = true;
    }

    /// <summary>
    /// During (each physics frame) of PaddleAction Magnet. Move Ball with Paddle if "Captured"
    /// </summary>
    protected void PaddleActionMagnetDuring(PaddleAction _PA)
    {
        PaddleActionMagnet _PA_Magnet = (PaddleActionMagnet)_PA; // Cast base PaddleAction into PaddleActionMagnet

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
        PaddleActionMagnet _PA_Magnet = (PaddleActionMagnet)_PA; // Cast base PaddleAction into PaddleActionMagnet

        if (!_PA_Magnet.f_magnetized)
        {
            return; // If magnet is currently off, do NOT freeze Ball on Paddle
        }

        // Did Paddle hit a Ball?
        if (_col.gameObject.TryGetComponent<Ball>(out Ball _ball))
        {
            Vector2 _ballPos = _ball.FreezeBallOnPaddle(c_boxCol.bounds); // Tell Ball to freeze itself on Paddle
            Vector2 _paddleToBallOffset = _ballPos - c_rb.position;
            if (!_PA_Magnet.dict_ball_offsetPos.ContainsKey(_ball))
                _PA_Magnet.dict_ball_offsetPos[_ball] = _paddleToBallOffset;
        }
    }

    /// <summary>
    /// On unassign PaddleAction Magnet. Release any still magnetized Balls
    /// </summary>
    protected void PaddleActionMagnetUnassign(PaddleAction _PA)
    {
        PaddleActionMagnet _PA_Magnet = (PaddleActionMagnet)_PA; // Cast base PaddleAction into PaddleActionMagnet

        if (_PA_Magnet.dict_ball_offsetPos.Count > 0) // Balls still magnetized to Paddle
        {
            _PA_Magnet.onPressActionMethod(_PA_Magnet); // Release the Balls & clear dictionary
        }
    }

    /// <summary>
    /// On press (of button) of PaddleAction Slam. Initiate a "Slam" movement, moving Paddle forward
    /// </summary>
    /// <returns>Returns whether or not Slam was actually initiated</returns>
    protected bool PaddleActionSlamPress(PaddleAction _PA)
    {
        PaddleActionSlam _PA_Slam = (PaddleActionSlam)_PA; // Cast base PaddleAction into PaddleActionSlam

        if (_PA_Slam.CurrState != PaddleActionSlam.SlamState.IDLE)
        {
            return false; // Not IDLE, so do not initiate a Slam
        }

        _PA_Slam.beforeSlamPos = transform.position;
        _PA_Slam.CurrState = PaddleActionSlam.SlamState.SLAMMING; // Begin slam
        c_rb.velocity += (_PA_Slam.slamInitialVel * dirToCenter);

        return true;
    }

    /// <summary>
    /// During (each physics frame) of PaddleAction Slam. Quickly move the Paddle towards the center of the screen, then slowly return
    /// </summary>
    protected void PaddleActionSlamDuring(PaddleAction _PA)
    {
        PaddleActionSlam _PA_Slam = (PaddleActionSlam)_PA; // Cast base PaddleAction into PaddleActionSlam


        if (_PA_Slam.CurrState == PaddleActionSlam.SlamState.IDLE)
        {
            return; // Do nothing
        }

        else if (_PA_Slam.CurrState == PaddleActionSlam.SlamState.SLAMMING)
        {
            float _nextXPos = transform.position.x + c_rb.velocity.x * Time.fixedDeltaTime; // The next x position of Paddle during Slam movement
            float _slamEndXPos = initialPaddlePos.x + (_PA_Slam.slamDistance * dirToCenter); // The x position at the end of Slam movement
            float _slamProgress = (_PA_Slam.beforeSlamPos.x - transform.position.x) / (_slamEndXPos - initialPaddlePos.x); // Percentage progress until reaching Slam end x position

            // Stop
            if (Mathf.Abs(_nextXPos) <= Mathf.Abs(_slamEndXPos)) // Paddle will exceed Slam distance next frame
            {
                c_rb.velocity = new Vector2(0, c_rb.velocity.y); // Stop x movement
                c_rb.MovePosition(new Vector2(_slamEndXPos, transform.position.y)); // Set Paddle to end of Slam position
                _PA_Slam.CurrState = PaddleActionSlam.SlamState.RESETTING; // Begin to reset Slam position
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

        else if (_PA_Slam.CurrState == PaddleActionSlam.SlamState.RESETTING)
        {
            float _nextXPos = transform.position.x + c_rb.velocity.x * Time.fixedDeltaTime; // The next x position of Paddle during Slam return

            if (Mathf.Abs(_nextXPos) >= Mathf.Abs(initialPaddlePos.x)) // Paddle will exceed Slam distance next frame
            {
                c_rb.velocity = new Vector2(0, c_rb.velocity.y); // Stop x movement
                c_rb.MovePosition(new Vector2(initialPaddlePos.x, transform.position.y)); // Set Paddle to initial x position
                _PA_Slam.CurrState = PaddleActionSlam.SlamState.IDLE; // Finish slam state
            }
            else
            {
                c_rb.velocity += (_PA_Slam.slamReturnVel * dirToCenter);
            }
        }
    }

    /// <summary>
    /// On press (of button) of PaddleAction GhostPaddle. Spawns a PaddleGhost, which moves according to Player input INSTEAD of this Paddle
    /// </summary>
    /// <returns>Returns true</returns>
    protected bool PaddleActionGhostPaddlePress(PaddleAction _PA)
    {
        PaddleActionGhostPaddle _PA_GhostPaddle = (PaddleActionGhostPaddle)_PA; // Cast base PaddleAction into PaddleActionGhostPaddle

        _PA_GhostPaddle.currGhostPaddle = Instantiate(pf_PaddleGhost, transform.position, transform.rotation); // Spawn PaddleGhost on top of this Paddle
        _PA_GhostPaddle.currGhostPaddle.Initialize(this);

        c_rb.velocity = Vector2.zero; // Stop any current velocity for one frame
        LoseInputControl(); // Have this Paddle stop responding to movement input (input now controls currGhostPaddle)
        _PA_GhostPaddle.f_controlCurrGhost = true;

        return true;
    }

    /// <summary>
    /// Hold (on pressed (second frame +)) of PaddleAction GhostPaddle. Control spawned PaddleGhost
    /// </summary>
    protected void PaddleActionGhostPaddleHeld(PaddleAction _PA)
    {
        PaddleActionGhostPaddle _PA_GhostPaddle = (PaddleActionGhostPaddle)_PA; // Cast base PaddleAction into PaddleActionGhostPaddle

        if (_PA_GhostPaddle.currGhostPaddle == null)
        {
            RegainInputControl();
            _PA_GhostPaddle.f_controlCurrGhost = false; // If there are no PaddleGhost on the screen, then regain control of this Paddle
        }
    }

    /// <summary>
    /// On release (of button) of PaddleAction GhostPaddle. Resume control of this Paddle
    /// </summary>
    protected void PaddleActionGhostPaddleRelease(PaddleAction _PA)
    {
        PaddleActionGhostPaddle _PA_GhostPaddle = (PaddleActionGhostPaddle)_PA; // Cast base PaddleAction into PaddleActionGhostPaddle

        RegainInputControl();
        _PA_GhostPaddle.f_controlCurrGhost = false;
    }

    /// <summary>
    /// During (each physics frame) of PaddleAction GhostPaddle. Using movement input of this Paddle, instead control current PaddleGhost
    /// </summary>
    protected void PaddleActionGhostPaddleDuring(PaddleAction _PA)
    {
        PaddleActionGhostPaddle _PA_GhostPaddle = (PaddleActionGhostPaddle)_PA; // Cast base PaddleAction into PaddleActionGhostPaddle

        if (_PA_GhostPaddle.currGhostPaddle != null)
        {
            if (_PA_GhostPaddle.f_controlCurrGhost)
            {
                _PA_GhostPaddle.currGhostPaddle.ControlGhostPaddleBy(paddleInputVector); // Control PaddleGhost (y movement)
            }
            else
            {
                _PA_GhostPaddle.currGhostPaddle.ControlGhostPaddleBy(Vector2.zero); // Do not control PaddleGhost
            }
        }

        if (_PA_GhostPaddle.f_delayedUnassignment && !_PA_GhostPaddle.f_controlCurrGhost) // Unassignment was delayed until Paddle regains control from current PaddleGhost
        {
            _PA_GhostPaddle.f_forceUnassignment = true; // Allow PaddleActionGhostPaddle to be unassigned
        }
    }

    /// <summary>
    /// On unassign PaddleAction GhostPaddle. Regain movement input controls for Paddle
    /// </summary>
    protected void PaddleActionGhostPaddleUnassign(PaddleAction _PA)
    {
        PaddleActionGhostPaddle _PA_GhostPaddle = (PaddleActionGhostPaddle)_PA; // Cast base PaddleAction into PaddleActionGhostPaddle

        if (_PA_GhostPaddle.currGhostPaddle != null) // If a PaddleGhost already exists, disable respond to movement input for that particular one
        {
            _PA_GhostPaddle.currGhostPaddle.LoseInputControl();
        }

        RegainInputControl(); // Have this Paddle respond to movement inputs again
    }
    #endregion

    /*********************************************************************************************************************************************************************************
     * On Event Methods
     *********************************************************************************************************************************************************************************/
    #region On Event Methods
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        OnPaddleActionCollision(currActionOne, collision);
        OnPaddleActionCollision(currActionTwo, collision);
    }
    #endregion
}
