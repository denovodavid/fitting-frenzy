using UnityEngine;
using UnityAtoms;
using UnityAtoms.BaseAtoms;

public class NPC : MonoBehaviour
{
    [SerializeField] private bool nakey = false;
    public FloatReference speed;
    public FloatReference startFollowRadius;
    public FloatReference stopFollowRadius;
    public SpriteValueList bodiesList, topsList, pantsList, hairsList, shoesList, hatsList, meleeWeaponsList, rangedWeaponsList, shieldsList;
    public SpriteRenderer body, top, pants, hair, shoes, hat, weapon, shield;

    Rigidbody2D rb;
    GameObject target;
    Vector3 targetOffset;

    bool follow = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        GetComponent<CircleCollider2D>().radius = startFollowRadius.Value;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            target = other.gameObject;
            targetOffset = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f).normalized;
            GetComponent<CircleCollider2D>().radius = stopFollowRadius.Value;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == target)
        {
            target = null;
            GetComponent<CircleCollider2D>().radius = startFollowRadius.Value;
        }
    }

    void MoveTowards(Vector3 position)
    {
        Vector2 dir = (position - transform.position).normalized;
        rb.AddForce(dir * speed.Value);
    }

    private void FixedUpdate()
    {
        if (nakey && target != null && follow)
        {
            MoveTowards(target.transform.position + targetOffset);
        }
    }

    private void OnEnable()
    {
        InitBody();
        if (!nakey) Dress();
    }

    public void StopFollow()
    {
        follow = false;
    }

    void InitBody()
    {
        body.sprite = GetRandomSpriteFromList(bodiesList);
        hair.sprite = Random.value < 0.9f ? GetRandomSpriteFromList(hairsList) : null;
    }

    public void Dress()
    {
        nakey = false;
        top.sprite = GetRandomSpriteFromList(topsList);
        pants.sprite = GetRandomSpriteFromList(pantsList);

        shoes.sprite = Random.value < 0.9f ? GetRandomSpriteFromList(shoesList) : null;
        hat.sprite = Random.value < 0.5f ? GetRandomSpriteFromList(hatsList) : null;

        var hasWeapon = Random.value < 0.5f;
        var isRangedWeapon = Random.value < 0.1f;
        weapon.sprite = hasWeapon ? GetRandomSpriteFromList(isRangedWeapon ? rangedWeaponsList : meleeWeaponsList) : null;
        shield.sprite = hasWeapon && !isRangedWeapon && Random.value < 0.3f ? GetRandomSpriteFromList(shieldsList) : null;

        var leftHanded = Random.value < 0.15f;
        weapon.flipX = leftHanded;
        shield.flipX = leftHanded;
    }

    public void Undress()
    {
        nakey = true;
        top.sprite = pants.sprite = shoes.sprite = hat.sprite = weapon.sprite = shield.sprite = null;
    }

    public void ToggleDress()
    {
        if (!nakey) Undress(); else Dress();
    }

    public static Sprite GetRandomSpriteFromList(SpriteValueList list)
    {
        return list.Get(Random.Range(0, list.Count - 1));
    }

    private void OnValidate()
    {
        InitBody();
        if (nakey) Undress(); else Dress();
    }

    public bool IsNakey() => nakey;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (target != null)
        {
            Gizmos.DrawSphere(target.transform.position + targetOffset, 0.25f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, startFollowRadius.Value);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, stopFollowRadius.Value);
    }
}
