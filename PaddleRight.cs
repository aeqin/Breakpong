using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PaddleRight : Paddle
{
    private new void Awake()
    {
        base.Awake();
        dirToCenter = -1; // Paddle faces left to center
    }

    private void OnEnable()
    {
        inputControls.Enable();
        inputControls.Paddle.PaddleRightMovement.performed += OnMovePaddleRightPerformed;
        inputControls.Paddle.PaddleRightMovement.canceled += OnMovePaddleRightReleased;

        inputControls.Paddle.PaddleRightActionOne.started += OnPaddleRightActionOnePressed;
        inputControls.Paddle.PaddleRightActionOne.canceled += OnPaddleRightActionOneReleased;
        inputControls.Paddle.PaddleRightActionTwo.started += OnPaddleRightActionTwoPressed;
        inputControls.Paddle.PaddleRightActionTwo.canceled += OnPaddleRightActionTwoReleased;
    }

    private void OnDisable()
    {
        inputControls.Disable();
        inputControls.Paddle.PaddleRightMovement.performed -= OnMovePaddleRightPerformed;
        inputControls.Paddle.PaddleRightMovement.canceled -= OnMovePaddleRightReleased;

        inputControls.Paddle.PaddleRightActionOne.started -= OnPaddleRightActionOnePressed;
        inputControls.Paddle.PaddleRightActionTwo.started -= OnPaddleRightActionTwoPressed;
    }

    /*********************************************************************************************************************************************************************************
     * Private Methods
     *********************************************************************************************************************************************************************************/
    #region Private Methods
    private void OnMovePaddleRightPerformed(InputAction.CallbackContext callbackContext)
    {
        paddleInputVector = callbackContext.ReadValue<Vector2>();
    }
    private void OnMovePaddleRightReleased(InputAction.CallbackContext callbackContext)
    {
        paddleInputVector = Vector2.zero;
    }

    private void OnPaddleRightActionOnePressed(InputAction.CallbackContext callbackContext)
    {
        OnPaddleActionPress(currActionOne);
    }
    private void OnPaddleRightActionOneReleased(InputAction.CallbackContext callbackContext)
    {
        OnPaddleActionRelease(currActionOne);
    }
    private void OnPaddleRightActionTwoPressed(InputAction.CallbackContext callbackContext)
    {
        OnPaddleActionPress(currActionTwo);
    }
    private void OnPaddleRightActionTwoReleased(InputAction.CallbackContext callbackContext)
    {
        OnPaddleActionRelease(currActionTwo);
    }
    #endregion
}
