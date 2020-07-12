using UnityEngine;
using UnityEngine.InputSystem;
using UnityAtoms.BaseAtoms;
using System.Collections.Generic;

public class PlayerCharacter : MonoBehaviour
{
    public FloatReference speed;
    public FloatReference dashDistance;
    public FloatReference dashDuration;
    public FloatReference controlMeter;
    public FloatReference frenzyRadius;
    public IntReference dressedCount;

    Rigidbody2D rb;

    Vector2 moveInput;

    Vector3 dashStartPos;
    Vector3 dashEndPos;
    float dashTime = 1f;

    bool frenzied = false;
    List<Collider2D> frenzyColliders = new List<Collider2D>();

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OutOfControl(float value)
    {
        if (value <= 0f) Frenzy();
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

        controlMeter.Value -= Time.deltaTime;
        if (!frenzied && controlMeter.Value <= 0f) Frenzy();
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void OnDash(InputValue value)
    {
        dashStartPos = transform.position;
        var dashDir = moveInput.normalized;
        dashEndPos = dashStartPos + new Vector3(dashDir.x, dashDir.y, 0f) * dashDistance;
        dashTime = 0f;
    }

    bool isDashing() => dashTime < 1f;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, frenzyRadius.Value);
    }

    void OnFrenzy()
    {
        Frenzy();
    }

    void Frenzy()
    {
        frenzied = true;
        Physics2D.OverlapCircle(transform.position, frenzyRadius.Value, new ContactFilter2D(), frenzyColliders);
        foreach (var col in frenzyColliders)
        {
            if (col.TryGetComponent<NPC>(out var npc))
            {
                if (npc.IsNakey()) dressedCount.Value++;
                npc.ToggleDress();
            }
        }

        int nakeyCount = 0;
        foreach (var npc in FindObjectsOfType<NPC>())
        {
            if (npc.IsNakey()) nakeyCount++;
        }
        Debug.Log("Nakey: " + nakeyCount);
    }
}
