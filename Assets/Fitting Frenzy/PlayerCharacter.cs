using UnityEngine;
using UnityEngine.InputSystem;
using UnityAtoms.BaseAtoms;

public class PlayerCharacter : MonoBehaviour
{
    public FloatReference speed;
    public FloatReference dashDistance;
    public FloatReference dashDuration;

    Rigidbody2D rb;

    Vector2 moveInput;

    Vector3 dashStartPos;
    Vector3 dashEndPos;
    float dashTime = 1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        var moveVector = moveInput * speed.Value;
        rb.AddForce(moveVector);

        if (dashTime < 1f)
        {
            // TODO: adjust moveInput power by how perpendicular the input and dash direction is.
            // e.g. (-)90deg = 1, 0deg = 0, 180deg = 0
            dashEndPos += new Vector3(moveInput.x, moveInput.y, 0f) * Time.deltaTime;
            var targetPos = Vector3.Lerp(dashStartPos, dashEndPos, Mathf.SmoothStep(0f, 1f, dashTime));
            rb.MovePosition(targetPos);
            dashTime += (1f / dashDuration) * Time.deltaTime;
        }
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnDash(InputValue value)
    {
        dashStartPos = transform.position;
        var dashDir = moveInput.normalized;
        dashEndPos = dashStartPos + new Vector3(dashDir.x, dashDir.y, 0f) * dashDistance;
        dashTime = 0f;
    }

    bool isDashing() => dashTime < 1f;
}
