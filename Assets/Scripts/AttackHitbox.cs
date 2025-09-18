using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public int damage = 1;

    void OnTriggerEnter2D(Collider2D other)
{
    // Düşman mı?
    var enemy = other.GetComponentInParent<EnemyHealth2D>();
    if (enemy)
    {
        enemy.TakeDamage(damage, transform.position); // transform.position = darbenin geldiği nokta
    }
}

}
