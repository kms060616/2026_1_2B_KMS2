using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    private static StageManager instance;
    public static StageManager Instance => instance;

    [System.Serializable]
    public class StageData
    {
        public string stageName; 
        public GameObject enemyPrefab;
    }

    [Header("스테이지 설정")]
    public List<StageData> stages = new List<StageData>();
    public int currentStageIndex = 0; 

    [Header("UI 연결 (선택)")]
    public TextMeshProUGUI stageText;

    [Header("적 생성 위치")]
    public Transform enemySpawnPoint;
    private GameObject currentEnemyObject;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (stages.Count == 0)
        {
            Debug.LogError("StageManager: 인스펙터에서 스테이지 데이터를 설정해주세요!");
            return;
        }

        StartStage(currentStageIndex);
    }
    public void StartStage(int index)
    {
        if (index >= stages.Count)
        {
            Debug.Log("모든 스테이지를 클리어했습니다! 게임 승리!");
            return;
        }
        currentStageIndex = index;
        StageData currentStage = stages[currentStageIndex];

        if (stageText != null)
        {
            stageText.text = currentStage.stageName;
        }
        else
        {
            Debug.LogWarning("StageManager: stageText 변수가 인스펙터에서 비어있습니다!");
        }

        if (currentEnemyObject != null)
        {
            Destroy(currentEnemyObject);
        }

        currentEnemyObject = Instantiate(currentStage.enemyPrefab, enemySpawnPoint.position, Quaternion.identity);

        if (CardManager.Instance != null && currentEnemyObject != null)
        {
            CardManager.Instance.EnemyStats = currentEnemyObject.GetComponent<CharacterStats>();
        }
        if (CardManager.Instance != null && CardManager.Instance.playerStats != null)
        {
            CharacterStats player = CardManager.Instance.playerStats;
            player.GainMana(player.maxMana);
            player.Heal(player.maxHealth);

            Debug.Log("다음 스테이지 시작: 플레이어의 HP와 MP가 완전히 회복되었습니다!");
        }

        if (CardManager.Instance != null)
        {
            foreach (var cardObj in new List<GameObject>(CardManager.Instance.cardObjects))
            {
                Destroy(cardObj);
            }
            CardManager.Instance.cardObjects.Clear();
            CardManager.Instance.handCards.Clear();
            CardManager.Instance.ReturnDiscardsToDeck();
            for (int i = 0; i < 4; i++)
            {
                CardManager.Instance.DrawCard();
            }
        }
    }
    public void OnEnemyDefeated()
    {
        Debug.Log($"{stages[currentStageIndex].stageName} 클리어!");
        currentStageIndex++;
        StartCoroutine(NextStageDelayRoutine(currentStageIndex));
    }

    private System.Collections.IEnumerator NextStageDelayRoutine(int nextIndex)
    {
        yield return null;

        if (CardManager.Instance != null)
        {
            CardManager.Instance.ShowCardReward();
        }
    }
    public void ProceedToNextStage()
    {
        StartStage(currentStageIndex);
    }

    public System.Collections.IEnumerator OnCardUsedRoutine()
    {
        if (CardManager.Instance == null ||
            CardManager.Instance.EnemyStats == null ||
            CardManager.Instance.EnemyStats.currentHealth <= 0)
        {
            yield break;
        }

        if (CardManager.Instance.EnemyStats.gameObject == null)
        {
            yield break;
        }

        Debug.Log("적의 행동 차례입니다.");
        CardManager.Instance.EnemyStats.TakeEnemyAction();
    }
}
