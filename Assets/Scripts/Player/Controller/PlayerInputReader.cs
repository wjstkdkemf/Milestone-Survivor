using UnityEngine;

public class PlayerInputReader : MonoBehaviour
{
    public Vector2 MoveDirection { get; private set; }
    public bool DashPressed { get; private set; }
    public bool InteractPressed { get; private set; }
    public bool CancelPressed { get; private set; }

    private Vector2 mobileMoveDirection;
    private bool mobileDashPressed;
    private bool mobileInteractPressed;
    private bool mobileCancelPressed;

    private void Update()
    {
        ReadKeyboardInput();
    }

    private void ReadKeyboardInput()
    {
        float moveX = 0f;
        float moveY = 0f;

        if (Input.GetKey(KeyCode.W)) moveY = 1f;
        if (Input.GetKey(KeyCode.S)) moveY = -1f;
        if (Input.GetKey(KeyCode.A)) moveX = -1f;
        if (Input.GetKey(KeyCode.D)) moveX = 1f;

        Vector2 keyboardMoveDirection = new Vector2(moveX, moveY).normalized;

        MoveDirection = mobileMoveDirection.sqrMagnitude > 0.01f
            ? mobileMoveDirection.normalized
            : keyboardMoveDirection;

        DashPressed = Input.GetKeyDown(KeyCode.Space)
            || Input.GetKeyDown(KeyCode.LeftShift)
            || mobileDashPressed;

        InteractPressed = Input.GetKeyDown(KeyCode.E)
            || mobileInteractPressed;

        CancelPressed = Input.GetKeyDown(KeyCode.Escape)
            || mobileCancelPressed;

        mobileDashPressed = false;
        mobileInteractPressed = false;
        mobileCancelPressed = false;
    }
    public void SetMobileMove(Vector2 direction)
    {
        mobileMoveDirection = direction;
    }
    public void ClearMobileMove()
    {
        mobileMoveDirection = Vector2.zero;
    }
    public void PressMobileDash()
    {
        mobileDashPressed = true;
        Debug.Log("체크");
    }
    public void PressMobileInteract()
    {
        mobileInteractPressed = true;
    }
    public void PressMobileCancel()
    {
        mobileCancelPressed = true;
    }
}