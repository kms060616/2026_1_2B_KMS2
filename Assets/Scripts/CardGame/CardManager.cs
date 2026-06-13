
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
    public List<CardData> deckCards = new List<CardData>();
    public List<CardData> handCards = new List<CardData>();
    public List<CardData> discardCards = new List<CardData>();

    public GameObject cardPrefabs;
    public Transform deckPosition;
    public Transform handPosition;
    public Transform discardPosition;

    public List<GameObject> cardObjects = new List<GameObject>();

    public CharacterStats playerStats;
    public CharacterStats EnemyStats;

    private static CardManager instance;

    [Header("보상 시스템 설정")]
    public List<CardData> allPossibleCards = new List<CardData>();

    public GameObject cardRewardPanel;
    public List<Button> rewardButtons;
    public List<TextMeshProUGUI> rewardButtonTexts;

    private List<CardData> currentRewardChoices = new List<CardData>();

    public static CardManager Instance
    {
        get { if (instance == null) instance = new CardManager(); return instance; }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ShuffleDeck();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            DrawCard();
        }

        if (Input.GetKeyDown(KeyCode.F))
            {
            ReturnDiscardsToDeck();
        }

        ArrangeHand();
    }

    public void ShuffleDeck()
    {
        List<CardData> tempDeck = new List<CardData>(deckCards);
        deckCards.Clear();

        while (tempDeck.Count > 0)
        {
            int randIndex = Random.Range(0, tempDeck.Count);
            deckCards.Add(tempDeck[randIndex]);
            tempDeck.RemoveAt(randIndex);
        }

        Debug.Log("덱을 섞었습니다 : " + deckCards.Count + "장");
    }

    public void DrawCard()
    {
        if (handCards.Count >= 6)
        {
            Debug.Log("손패가 가득찼습니다 최대6장");
            return;
        }

        if (deckCards.Count == 0)
        {
            Debug.Log("덱이 비었습니다! 버린 카드 더미를 덱으로 되돌리고 새로 섞습니다.");
            ReturnDiscardsToDeck(); 
        }

        if (deckCards.Count == 0)
        {
            Debug.Log("덱과 버린 카드 더미가 모두 비어있어 카드를 드로우할 수 없습니다.");
            return;
        }

        CardData cardData = deckCards[0];
        deckCards.RemoveAt(0);

        handCards.Add(cardData);

        GameObject cardObj = Instantiate(cardPrefabs, deckPosition.position, Quaternion.identity);


        CardDisplay cardDisplay = cardObj.GetComponent<CardDisplay>();
        if (cardDisplay != null)
        {
            cardDisplay.SetupCard(cardData);
            cardDisplay.cardIndex = handCards.Count -1;
            cardObjects.Add(cardObj);
        }

        ArrangeHand();

        Debug.Log("카드를 드로우 했습니다. :" + cardData.cardName + "(손패 :" + handCards.Count + "/6");


    }

    public void ArrangeHand()
    {
        if (handCards.Count == 0) return;

        float cardWidth = 1.2f;
        float spacing = cardWidth + 1.8f;
        float totalWidth = (handCards.Count - 1) * spacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < cardObjects.Count; i++)
        {
            if (cardObjects[i] != null)
            {
                CardDisplay display = cardObjects[i].GetComponent<CardDisplay>();

                if (display != null && display.isDragging)
                    continue;

                Vector3 targetPosition = handPosition.position + new Vector3(startX + (i * spacing), 0, 0);

                cardObjects[i].transform.position = Vector3.Lerp(cardObjects[i].transform.position, targetPosition, Time.deltaTime * 10f);
            }
        }
    }

    public void DiscardCard(int handIndex)
    {
        if (handIndex < 0 || handIndex >= handCards.Count)
        {
            Debug.Log("유효하지 않은 카드 인덱스 입니다");
                return;
        }

        CardData cardData = handCards[handIndex];
        handCards.RemoveAt(handIndex);

        discardCards.Add(cardData);

        if(handIndex < cardObjects.Count)
        {
            Destroy(cardObjects[handIndex]);
            cardObjects.RemoveAt(handIndex);
        }
        for (int i = 0; i < cardObjects.Count;i++)
        {
            CardDisplay display = cardObjects[i].GetComponent<CardDisplay>();
            if (display != null) display.cardIndex = i;

        }

        ArrangeHand();
        Debug.Log("카드를 버렸습니다" + cardData.cardName);


    }

    public void ReturnDiscardsToDeck()
    {
        if (discardCards.Count == 0)
        {
            Debug.Log("버린 카드 더미가 비어 있습니다");
                return;
        }

        deckCards.AddRange(discardCards);
        discardCards.Clear();
        ShuffleDeck();
        Debug.Log("버린 카드" + deckCards.Count + "장을 덱으로 되돌리고 섞었습니다");

    }

    public void ShowCardReward()
    {
        if (allPossibleCards.Count < 3)
        {
            Debug.LogError("전체 카드 목록(allPossibleCards)에 카드가 최소 3장 이상 있어야 합니다!");
            return;
        }

        currentRewardChoices.Clear();
        cardRewardPanel.SetActive(true);
        List<CardData> shuffledTotalCards = new List<CardData>(allPossibleCards);
        for (int i = 0; i < shuffledTotalCards.Count; i++)
        {
            int rnd = Random.Range(0, shuffledTotalCards.Count);
            CardData temp = shuffledTotalCards[i];
            shuffledTotalCards[i] = shuffledTotalCards[rnd];
            shuffledTotalCards[rnd] = temp;
        }
        for (int i = 0; i < 3; i++)
        {
            currentRewardChoices.Add(shuffledTotalCards[i]);
            rewardButtonTexts[i].text = $"{shuffledTotalCards[i].cardName}\n(코스트: {shuffledTotalCards[i].manaCost})\n\n{shuffledTotalCards[i].description}";
            int index = i; 
            rewardButtons[i].onClick.RemoveAllListeners();
            rewardButtons[i].onClick.AddListener(() => SelectRewardCard(index));
        }
    }

    public void SelectRewardCard(int choiceIndex)
    {
        CardData selectedCard = currentRewardChoices[choiceIndex];
        

        if (discardCards != null)
        {
            discardCards.Add(selectedCard);
        }

        Debug.Log($"보상 획득! 덱에 [{selectedCard.cardName}] 카드가 추가되었습니다.");
        cardRewardPanel.SetActive(false);

        if (StageManager.Instance != null)
        {
            StageManager.Instance.ProceedToNextStage();
        }
    }

}
