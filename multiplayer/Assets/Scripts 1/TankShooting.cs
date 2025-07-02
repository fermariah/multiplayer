using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Unity.Netcode;
using System;

namespace Tanks.Complete
{
    public class TankShooting : MonoBehaviour
    {
        [Header("Configurações de Tiro")]
        public Transform firePoint; // Ponto de origem do tiro
        public GameObject bulletPrefab; // Prefab da bala
        public float bulletForce = 20f; // Força do disparo
        public float fireRate = 0.5f; // Tempo entre disparos
        private float nextFireTime = 0f;

        [Header("Configurações de Vida")]
        public int maxHealth = 100;
        public int currentHealth;
        public TankHealth healthBar; // Referência para a barra de vida UI

        void Start()
        {
            currentHealth = maxHealth;
            if (healthBar != null)
            {
                healthBar.SetMaxHealth(maxHealth);
            }
        }

        void Update()
        {
            // Disparo com Left Arrow (seta para esquerda)
            if (Input.GetKey(KeyCode.LeftArrow) && Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }

        void Shoot()
        {
            // Instancia a bala no ponto de disparo
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            // Obtém o componente Rigidbody2D da bala
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

            // Aplica força na bala para movê-la
            rb.AddForce(firePoint.right * bulletForce, ForceMode2D.Impulse);

            // Configura o dono da bala para evitar auto-dano
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.owner = gameObject;
            }
        }

        // Método para receber dano
        public void TakeDamage(int damage)
        {
            currentHealth -= damage;

            // Atualiza a barra de vida
            if (healthBar != null)
            {
                healthBar.SetHealth(currentHealth);
            }

            // Verifica se o tanque foi destruído
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        void Die()
        {
            // Lógica para quando o tanque é destruído
            Debug.Log(gameObject.name + " foi destruído!");
            Destroy(gameObject);
        }
    }

    // Script adicional para a bala
    public class Bullet : MonoBehaviour
    {
        public int damage = 10; // Dano causado pela bala
        public float lifetime = 2f; // Tempo de vida da bala
        public GameObject owner; // Referência ao tanque que disparou

        void Start()
        {
            // Destrói a bala após um tempo para evitar que fique voando para sempre
            Destroy(gameObject, lifetime);
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            // Verifica se colidiu com outro objeto (que não seja o dono da bala nem outra bala)
            if (collision.gameObject != owner && !collision.CompareTag("Bullet"))
            {
                TankCombat enemyTank = collision.GetComponent<TankCombat>();

                // Se o objeto atingido for um tanque, causa dano
                if (enemyTank != null)
                {
                    enemyTank.TakeDamage(damage);
                }

                // Destrói a bala após a colisão
                Destroy(gameObject);
            }
        }

        private class TankCombat
        {
            internal void TakeDamage(int damage)
            {
                throw new NotImplementedException();
            }
        }
    }

    //OBS: Métodos como TankCombat, TakeDamage, foram corrigidos pela própria unity!

}
