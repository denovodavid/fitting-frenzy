using UnityEngine;
using UnityEngine.InputSystem;
using UnityAtoms.BaseAtoms;
using System.Collections.Generic;
using DG.Tweening;
using System.Linq;
using UnityEngine.Experimental.Rendering.Universal;

public class PlayerCharacter : MonoBehaviour
{
    public FloatReference speed;
    public FloatReference dashDistance;
    public FloatReference dashDuration;
    public FloatReference controlMeter;
    public FloatReference frenzyRadius;
    public IntReference dressedCount;
    public GameObject clothePrefab;
    public GameObject body;
    public PixelPerfectCamera ppc;
    public Cinemachine.CinemachineTargetGroup cinemachineTargetGroup;
    public Cinemachine.CinemachineVirtualCamera vcam;

    Rigidbody2D rb;

    Vector2 moveInput;

    Vector3 dashStartPos;
    Vector3 dashEndPos;
    float dashTime = 1f;

    bool frenzied = false;
    List<Collider2D> frenzyColliders = new List<Collider2D>();

    Sequence jumpSeq;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        jumpSeq = body.transform
            .DOLocalJump(Vector3.zero, 0.1f, 1, 0.2f)
            .SetLoops(-1)
            .OnStepComplete(() =>
            {
                if (moveInput == Vector2.zero) jumpSeq.Pause();
            });
    }

    void OutOfControl(float value)
    {
        if (value <= 0f) Frenzy();
    }

    void FixedUpdate()
    {
        var moveVector = moveInput * speed.Value;
        rb.AddForce(moveVector);

        if (moveInput != Vector2.zero) jumpSeq.Play();

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

        Sequence seq = DOTween.Sequence();
        seq.AppendCallback(() => vcam.GetCinemachineComponent<Cinemachine.CinemachineFramingTransposer>().m_GroupFramingMode = Cinemachine.CinemachineFramingTransposer.FramingMode.HorizontalAndVertical);
        seq.AppendInterval(0.3f);

        var colCount = Physics2D.OverlapCircle(transform.position, frenzyRadius.Value, new ContactFilter2D(), frenzyColliders);
        foreach (var col in frenzyColliders)
        {
            if (col.TryGetComponent<NPC>(out var npc))
            {
                cinemachineTargetGroup.AddMember(npc.transform, 1f, 0f);
                npc.StopFollow();
                var npcRb = npc.GetComponent<Rigidbody2D>();
                npcRb.velocity = Vector2.zero;
                npcRb.isKinematic = true;
                seq.AppendCallback(() =>
                {
                    npc.GetComponent<Rigidbody2D>().isKinematic = false;
                    Time.timeScale = 1f;
                });
                seq.Append(rb.DOMove(npc.transform.position, 0.2f));
                seq.AppendCallback(() =>
                {
                    Time.timeScale = 0.2f;

                    void SpawnClothe(UnityAtoms.SpriteValueList list)
                    {
                        var clothe = Instantiate(clothePrefab, npc.transform.position, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)));
                        clothe.GetComponent<SpriteRenderer>().sprite = NPC.GetRandomSpriteFromList(list);
                        clothe.GetComponent<Rigidbody2D>().AddForce(new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * 40f, ForceMode2D.Impulse);
                        clothe.GetComponent<Rigidbody2D>().AddTorque(Random.Range(-1f, 1f) * 5f, ForceMode2D.Impulse);
                    };

                    SpawnClothe(npc.topsList);
                    SpawnClothe(npc.hatsList);
                    SpawnClothe(npc.meleeWeaponsList);
                    SpawnClothe(npc.pantsList);
                    SpawnClothe(npc.shieldsList);

                    if (npc.IsNakey()) dressedCount.Value++;
                    npc.ToggleDress();
                });
                seq.AppendCallback(() => {
                    rb.DOMove(npc.transform.position /* + (npc.transform.position - transform.position).normalized * 0.01f */, 0.1f);
                }).AppendInterval(0.1f);
                seq.AppendCallback(() => cinemachineTargetGroup.RemoveMember(npc.transform));
            }
        }

        // Sequence camSeq = DOTween.Sequence();
        // camSeq.Append(DOTween.To(() => ppc.assetsPPU, (v) => ppc.assetsPPU = v, 128, 0.3f));
        // camSeq.Append(DOTween.To(() => ppc.assetsPPU, (v) => ppc.assetsPPU = v, 16, seq.Duration()));

        seq.AppendCallback(() => vcam.GetCinemachineComponent<Cinemachine.CinemachineFramingTransposer>().m_GroupFramingMode = Cinemachine.CinemachineFramingTransposer.FramingMode.None);

        seq.AppendCallback(() =>
        {

            Time.timeScale = 1f;
            int nakeyCount = 0;
            foreach (var npc in FindObjectsOfType<NPC>())
            {
                if (npc.IsNakey()) nakeyCount++;
            }
            Debug.Log("Nakey: " + nakeyCount);
        });
    }
}
