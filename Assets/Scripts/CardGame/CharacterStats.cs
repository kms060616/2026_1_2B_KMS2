using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class CharacterStats : MonoBehaviour
{
    public string characterName;
    public int maxHealth = 100;
    public int currentHealth;
    public TextMeshProUGUI healthText;

    public int maxMana = 10;
    public int currentMana;
    public TextMeshProUGUI manaText;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;
        UpdateUI();
    }

    public void UseMana(int amount)
    {
        currentMana -= amount;
        if(currentMana < 0)
        {
            currentMana = 0;
        }
        UpdateUI();
    }

    public void GainMana(int amount)
    {
        currentMana += amount;
        if (currentMana > maxMana)
        {
            currentMana = maxMana;

        }
        UpdateUI();
    }

    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;

            if (gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                if (StageManager.Instance != null)
                {
                    StageManager.Instance.OnEnemyDefeated();
                }
            }
            else
            {
                Debug.Log("플레이어가 사망했습니다. 게임 오버!");
            }
        }

        if (DamageEffectManager.instance != null)
        {
            Vector3 position = transform.position;
            position += new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(1.0f, 1.5f), 0);
            DamageEffectManager.instance.ShowDamage(position, damage, false);
        }
        UpdateUI();
    }
    public void Heal(int amount)
    {
        currentHealth += amount;
        if (DamageEffectManager.instance != null)
        {
            Vector3 position = transform.position;
            position += new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(1.0f, 1.5f), 0);
            DamageEffectManager.instance.ShowHeal(position, amount, false);
        }
    }

    private void UpdateUI()
    {
        if (healthText != null)
        {
            healthText.text = $"HP:{currentHealth} / {maxHealth}";
        }
        if (manaText != null)
        {
            manaText.text = $"MP:{currentMana} / {maxMana}";
        }
    }

}
