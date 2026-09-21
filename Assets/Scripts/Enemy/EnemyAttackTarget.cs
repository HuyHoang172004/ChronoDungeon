using UnityEngine;

// Explicit opt-in for damage from Player and Temporal Ghost attacks.
[DisallowMultipleComponent, RequireComponent(typeof(Health))]
public sealed class EnemyAttackTarget : MonoBehaviour { }
