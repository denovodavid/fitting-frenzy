using UnityEngine;
using UnityAtoms;

public class NPC : MonoBehaviour
{
    [SerializeField] private bool nakey = false;
    public float speed = 90;
    public SpriteValueList bodiesList, topsList, pantsList, hairsList, shoesList, hatsList, meleeWeaponsList, rangedWeaponsList, shieldsList;
    public SpriteRenderer body, top, pants, hair, shoes, hat, weapon, shield;

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (nakey && other.CompareTag("Player"))
        {
            Vector2 dir = (other.transform.position - transform.position).normalized;
            rb.AddForce(dir * speed);
        }
    }

    private void OnEnable()
    {
        body.sprite = GetRandomSpriteFromList(bodiesList);
        hair.sprite = Random.value < 0.9f ? GetRandomSpriteFromList(hairsList) : null;
        if (!nakey) Dress();
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

    Sprite GetRandomSpriteFromList(SpriteValueList list)
    {
        return list.Get(Random.Range(0, list.Count - 1));
    }
}
