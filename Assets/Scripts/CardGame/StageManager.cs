using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    private static StageManager instance;
    public static StageManager Instance => instance;

    [Header("게임 종료 UI 연결")]
    public GameObject gameClearPanel;
    public GameObject gameOverPanel;

    private bool isTransitioningStage = false;

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
            Debug.Log("모든 스테이지를 클리어했습니다! 게임 승리!");
            if (gameClearPanel != null) gameClearPanel.SetActive(true);
            return;
        }

        StartStage(currentStageIndex);
    }
    public void StartStage(int index)
    {
        isTransitioningStage = true;

        if (index >= stages.Count)
        {
            Debug.Log("모든 스테이지를 클리어했습니다! 게임 승리!");
            if (gameClearPanel != null)
            {
                gameClearPanel.SetActive(true);
            }
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

            if (CardManager.Instance.handCards.Count > 0)
            {
                CardManager.Instance.discardCards.AddRange(CardManager.Instance.handCards);
                CardManager.Instance.handCards.Clear();
                Debug.Log("이전 스테이지의 손패를 모두 무덤(discardCards)으로 안전하게 보냈습니다.");
            }

            CardManager.Instance.ReturnDiscardsToDeck();

            for (int i = 0; i < 4; i++)
            {
                CardManager.Instance.DrawCard();
            }
        }
        isTransitioningStage = false;
    }
    public void OnEnemyDefeated()
    {
        Debug.Log($"{stages[currentStageIndex].stageName} 클리어!");
        isTransitioningStage = true;

        currentStageIndex++;
        StartCoroutine(NextStageDelayRoutine(currentStageIndex));
    }

    private System.Collections.IEnumerator NextStageDelayRoutine(int nextIndex)
    {
        yield return null;

        if (nextIndex >= stages.Count)
        {
            Debug.Log("마지막 스테이지까지 모두 클리어했습니다! 보상 없이 게임 클리어 패널을 띄웁니다.");

            if (gameClearPanel != null)
            {
                gameClearPanel.SetActive(true);
            }
            yield break;
        }
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
        yield return null;

        if (isTransitioningStage ||
            CardManager.Instance == null ||
            CardManager.Instance.EnemyStats == null ||
            CardManager.Instance.EnemyStats.currentHealth <= 0)
        {
            Debug.Log("적이 사망했거나 스테이지 전환 중이므로 적의 행동을 스킵합니다.");
            yield break;
        }

        Debug.Log("적의 행동 차례입니다.");
        CardManager.Instance.EnemyStats.TakeEnemyAction();
    }

    public void RestartGame()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}
