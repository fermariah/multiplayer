using System;
using Tanks.Complete;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.GridLayoutGroup;

namespace Tanks.Complete
{
    [RequireComponent(typeof(Collider2D))]
    public class TankHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 100;
        private int currentHealth;

        // Componentes da UI
        private Canvas healthCanvas;
        private Slider healthSlider;

    
        private void Awake()
        {
            InitializeHealthSystem();
        }

        private void InitializeHealthSystem()
        {
            // Configura saúde inicial
            currentHealth = maxHealth;

            // Garante que existe um Collider2D
            if (GetComponent<Collider2D>() == null)
            {
                gameObject.AddComponent<BoxCollider2D>();
            }

            CreateHealthUI();
        }

        private void CreateHealthUI()
        {
            // Cria o Canvas
            healthCanvas = new GameObject("HealthCanvas").AddComponent<Canvas>();
            healthCanvas.renderMode = RenderMode.WorldSpace;
            healthCanvas.transform.SetParent(transform);
            healthCanvas.transform.localPosition = new Vector3(0, 1.5f, 0);

            var canvasRT = healthCanvas.GetComponent<RectTransform>();
            canvasRT.sizeDelta = new Vector2(3, 0.5f);

            // Cria o Slider
            healthSlider = new GameObject("HealthSlider").AddComponent<Slider>();
            healthSlider.transform.SetParent(healthCanvas.transform);
            healthSlider.transform.localPosition = Vector3.zero;
            healthSlider.transform.localScale = Vector3.one;

            // Configuração básica do Slider
            healthSlider.minValue = 0;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;

            // Configuração do RectTransform
            var sliderRT = healthSlider.GetComponent<RectTransform>();
            sliderRT.anchorMin = Vector2.zero;
            sliderRT.anchorMax = Vector2.one;
            sliderRT.sizeDelta = Vector2.zero;

            // Adiciona background
            var bg = new GameObject("Background").AddComponent<Image>();
            bg.transform.SetParent(healthSlider.transform);
            bg.color = new Color(0.2f, 0.2f, 0.2f);

            var bgRT = bg.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;

            // Adiciona fill area
            var fillArea = new GameObject("Fill Area").AddComponent<RectTransform>();
            fillArea.SetParent(healthSlider.transform);
            fillArea.anchorMin = new Vector2(0, 0.25f);
            fillArea.anchorMax = new Vector2(1, 0.75f);
            fillArea.sizeDelta = Vector2.zero;

            // Adiciona fill
            var fill = new GameObject("Fill").AddComponent<Image>();
            fill.transform.SetParent(fillArea);
            fill.color = Color.green;

            var fillRT = fill.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.sizeDelta = Vector2.zero;

            healthSlider.fillRect = fillRT;
        }

        public void TakeDamage(int damage)
        {
            currentHealth = Mathf.Max(0, currentHealth - damage);
            healthSlider.value = currentHealth;

            // Atualiza cor baseada na saúde
            float healthPercent = (float)currentHealth / maxHealth;
            healthSlider.fillRect.GetComponent<Image>().color =
                Color.Lerp(Color.red, Color.green, healthPercent);

            if (currentHealth <= 0)
            {
                Destroy(gameObject);
            }
        }

        internal void SetHealth(int currentHealth)
        {
            throw new NotImplementedException();
        }

        internal void SetMaxHealth(int maxHealth)
        {
            throw new NotImplementedException();
        }
    }



}

public class Bullet : MonoBehaviour
{
    public int damage = 10;
    public float lifetime = 3f;
    public GameObject owner;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == owner) return;

        TankHealth health = other.GetComponent<TankHealth>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}


