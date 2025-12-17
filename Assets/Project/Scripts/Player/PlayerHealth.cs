using System;
using UnityEngine;
using Game.Player.Movement;

namespace Game.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 100;

        public int CurrentHealth { get; private set; }
        public bool IsDead { get; private set; }

        public event Action<int, int> OnHealthChanged;
        public event Action OnDeath;

        void Awake()
        {
            CurrentHealth = maxHealth;
            IsDead = false;
        }

        public void TakeDamage(int damageAmount, bool isServerCall = false)
        {
            const int MAX_SAFE_DAMAGE = 150;
            if (damageAmount > MAX_SAFE_DAMAGE || damageAmount < 0)
            {
                Debug.LogWarning($"[Security] Dano inválido ou exagerado ({damageAmount}) ignorado.");
                return;
            }
            if (IsDead) return;
            CurrentHealth -= damageAmount;
            if (CurrentHealth < 0)
            {
                CurrentHealth = 0;
            }
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(int healAmount)
        {
            if (IsDead || healAmount < 0) return;

            CurrentHealth += healAmount;
            if (CurrentHealth > maxHealth)
            {
                CurrentHealth = maxHealth;
            }
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        private void Die()
        {
            if (IsDead) return;
            IsDead = true;
            OnDeath?.Invoke();
            Debug.Log("Player morreu (Ação Server-Authoritative)");

            var movement = GetComponent<PlayerMovement>();
            if (movement != null) movement.enabled = false;
        }
    }
}