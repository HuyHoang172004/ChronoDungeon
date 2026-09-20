using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float attackRange = 2f;

    public void Attack()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            attackRange
        );

        foreach (Collider2D hit in hits)
        {
            EnemyFollow enemy = hit.GetComponent<EnemyFollow>();

            if (enemy != null)
            {
                Destroy(enemy.gameObject);
                Debug.Log("Enemy defeated!");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}