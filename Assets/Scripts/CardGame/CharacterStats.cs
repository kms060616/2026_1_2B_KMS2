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

    void Start()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;
        UpdateUI();
    }

    public void UseMana(int amount)
    {
        currentMana -= amount;
        if (currentMana < 0)
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

    public void TakeEnemyAction()
    {
        if (gameObject.layer != LayerMask.NameToLayer("Enemy")) return;

        int randomAction = Random.Range(0, 2);

        if (randomAction == 0)
        {
            if (CardManager.Instance != null && CardManager.Instance.playerStats != null)
            {
                int stageBonus = 0;
                if (StageManager.Instance != null)
                {
                    stageBonus = StageManager.Instance.currentStageIndex * 5;
                }

                int finalDamage = 10 + stageBonus;

                Debug.Log($"{characterName}의 반격! 플레이어에게 {finalDamage}의 피해를 줍니다. (스테이지 보너스: +{stageBonus})");
                CardManager.Instance.playerStats.TakeDamage(finalDamage);
            }
        }
        else
        {
            int healBonus = 0;
            if (StageManager.Instance != null)
            {
                healBonus = StageManager.Instance.currentStageIndex * 1;
            }
            int finalHeal = 2 + healBonus;

            Debug.Log($"{characterName}이(가) 숨을 고르며 체력을 {finalHeal} 회복합니다.");
            Heal(finalHeal);
        }
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
                    UpdateUI();
                    return; 
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
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        if (DamageEffectManager.instance != null)
        {
            Vector3 position = transform.position;
            position += new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(1.0f, 1.5f), 0);
            DamageEffectManager.instance.ShowHeal(position, amount, false);
        }
        UpdateUI();
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
