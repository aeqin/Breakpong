using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PaddleLeft : Paddle
{
    private new void Awake()
    {
        base.Awake();
        dirToCenter = 1; // Paddle faces right to center
    }

    private void OnEnable()
    {
        inputControls.Enable();
        inputControls.Paddle.PaddleLeftMovement.performed += OnMovePaddleLeftPerformed;
        inputControls.Paddle.PaddleLeftMovement.canceled += OnMovePaddleLeftReleased;

        inputControls.Paddle.PaddleLeftActionOne.started += OnPaddleLeftActionOnePressed;
        inputControls.Paddle.PaddleLeftActionOne.canceled += OnPaddleLeftActionOneReleased;
        inputControls.Paddle.PaddleLeftActionTwo.started += OnPaddleLeftActionTwoPressed;
        inputControls.Paddle.PaddleLeftActionTwo.canceled += OnPaddleLeftActionTwoReleased;
    }

    private void OnDisable()
    {
        inputControls.Disable();
        inputControls.Paddle.PaddleLeftMovement.performed -= OnMovePaddleLeftPerformed;
        inputControls.Paddle.PaddleLeftMovement.canceled -= OnMovePaddleLeftReleased;

        inputControls.Paddle.PaddleLeftActionOne.started -= OnPaddleLeftActionOnePressed;
        inputControls.Paddle.PaddleLeftActionTwo.started -= OnPaddleLeftActionTwoPressed;
    }

    /*********************************************************************************************************************************************************************************
     * Private Methods
     *********************************************************************************************************************************************************************************/
    #region Private Methods
    private void OnMovePaddleLeftPerformed(InputAction.CallbackContext callbackContext)
    {
        paddleInputVector = callbackContext.ReadValue<Vector2>();
    }
    private void OnMovePaddleLeftReleased(InputAction.CallbackContext callbackContext)
    {
        paddleInputVector = Vector2.zero;
    }

    private void OnPaddleLeftActionOnePressed(InputAction.CallbackContext callbackContext)
    {
        OnPaddleActionPress(currActionOne);
    }
    private void OnPaddleLeftActionOneReleased(InputAction.CallbackContext callbackContext)
    {
        OnPaddleActionRelease(currActionOne);
    }
    private void OnPaddleLeftActionTwoPressed(InputAction.CallbackContext callbackContext)
    {
        OnPaddleActionPress(currActionTwo);
    }
    private void OnPaddleLeftActionTwoReleased(InputAction.CallbackContext callbackContext)
    {
        OnPaddleActionRelease(currActionTwo);
    }
    #endregion
}
