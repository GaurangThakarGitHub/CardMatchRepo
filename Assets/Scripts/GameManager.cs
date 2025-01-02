using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using static Unity.Burst.Intrinsics.X86.Sse4_2;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;
    public int gameSize = 2;

    [SerializeField]
    private GameObject prefab;
    [SerializeField]
    private GameObject cardList;

    [SerializeField]
    private Sprite cardBack;

    [SerializeField]
    private Sprite[] sprites;
    private Card[] cards;

    [SerializeField]
    private GameObject panel;
    [SerializeField]
    private GameObject info;
    [SerializeField]
    private GameObject panel_Score;
    [SerializeField]
    private GameObject panel_Timer;
    [SerializeField]
    private Card spritePreload;
    [SerializeField]
    private TMP_Text txt_Score;
    [SerializeField]
    private TMP_Text sizeLabel;
    [SerializeField]
    private Slider sizeSlider;
    [SerializeField]
    private TMP_Text timeLabel;
    [SerializeField]
    private Canvas canvasGamePlay;
    private float time;
    private int score = 0;
    private int spriteSelected;
    private int cardSelected;
    private int cardLeft;
    private bool gameStart;

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        gameStart = false;
       
    }
    private void PreloadCardImage()
    {
        for (int i = 0; i < sprites.Length; i++)
            spritePreload.SpriteID = i;
        spritePreload.gameObject.SetActive(false);
    }
    public void StartCardGame()
    {
        if (gameStart)
            return;
        gameStart = true;
        canvasGamePlay.enabled = true;
        info.SetActive(false);
        panel_Timer.SetActive(true);
        panel_Score.SetActive(true);
        SetGamePanel();
        cardSelected = spriteSelected = -1;
        cardLeft = cards.Length;
        SpriteCardAllocation();
        StartCoroutine(HideFace());
        time = 0;
        score = 0;
    }
    private void SetGamePanel()
    {
        int isOdd = gameSize % 2;
        cards = new Card[gameSize * gameSize - isOdd];
        foreach (Transform child in cardList.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        RectTransform panelsize = panel.transform.GetComponent(typeof(RectTransform)) as RectTransform;
        float row_size = panelsize.sizeDelta.x;
        float col_size = panelsize.sizeDelta.y;
        float scale = 1.0f / gameSize;
        float xInc = row_size / gameSize;
        float yInc = col_size / gameSize;
        float curX = -xInc * (float)(gameSize / 2);
        float curY = -yInc * (float)(gameSize / 2);

        if (isOdd == 0)
        {
            curX += xInc / 2;
            curY += yInc / 2;
        }
        float initialX = curX;
        for (int i = 0; i < gameSize; i++)
        {
            curX = initialX;
            for (int j = 0; j < gameSize; j++)
            {
                GameObject cardObj;

                if (isOdd == 1 && i == (gameSize - 1) && j == (gameSize - 1))
                {
                    int index = gameSize / 2 * gameSize + gameSize / 2;
                    cardObj = cards[index].gameObject;
                }
                else
                {

                    cardObj = Instantiate(prefab);
                    cardObj.transform.parent = cardList.transform;
                    int index = i * gameSize + j;
                    cards[index] = cardObj.GetComponent<Card>();
                    cards[index].ID = index;
                    cardObj.transform.localScale = new Vector3(scale, scale);
                }
                cardObj.transform.localPosition = new Vector3(curX, curY, 0);
                Debug.Log("X pos " + curX);
                Debug.Log("Y pos " + curY);
                curX += xInc;

            }
            curY += yInc;
        }

    }
    void ResetFace()
    {
        for (int i = 0; i < gameSize; i++)
            cards[i].ResetRotation();
    }
    private void OnApplicationQuit()
    {
        SaveGame();
    }

    IEnumerator HideFace()
    {
        yield return new WaitForSeconds(0.7f);
        for (int i = 0; i < cards.Length; i++)
            cards[i].Flip();
        yield return new WaitForSeconds(0.5f);
    }

    private void SpriteCardAllocation()
    {
        int i, j;
        int[] selectedID = new int[cards.Length / 2];
        for (i = 0; i < cards.Length / 2; i++)
        {

            int value = Random.Range(0, sprites.Length - 1);
            for (j = i; j > 0; j--)
            {
                if (selectedID[j - 1] == value)
                    value = (value + 1) % sprites.Length;
            }
            selectedID[i] = value;
        }


        for (i = 0; i < cards.Length; i++)
        {
            cards[i].Active();
            cards[i].SpriteID = -1;
            cards[i].ResetRotation();
        }

        for (i = 0; i < cards.Length / 2; i++)
            for (j = 0; j < 2; j++)
            {
                int value = Random.Range(0, cards.Length - 1);
                while (cards[value].SpriteID != -1)
                    value = (value + 1) % cards.Length;

                cards[value].SpriteID = selectedID[i];
            }

    }

    public void SetGameSize()
    {
        gameSize = (int)sizeSlider.value;
        sizeLabel.text = gameSize + " X " + gameSize;
    }

    public Sprite GetSprite(int spriteId)
    {
        return sprites[spriteId];
    }

    public Sprite CardBack()
    {
        return cardBack;
    }

    public bool canClick()
    {
        if (!gameStart)
            return false;
        return true;
    }

    public void cardClicked(int spriteId, int cardId)
    {

        if (spriteSelected == -1)
        {
            spriteSelected = spriteId;
            cardSelected = cardId;
        }
        else
        {
            if (spriteSelected == spriteId)
            {
                cards[cardSelected].Inactive();
                cards[cardId].Inactive();
                score++;
                txt_Score.text = score.ToString();
                cardLeft -= 2;
                CheckGameWin();
            }
            else
            {

                cards[cardSelected].Flip();
                cards[cardId].Flip();
            }
            cardSelected = spriteSelected = -1;
        }
    }

    private void CheckGameWin()
    {

        if (cardLeft == 0)
        {
            EndGame();
        }
    }

    private void EndGame()
    {
        gameStart = false;
        panel.SetActive(false);
    }
    public void GiveUp()
    {
        EndGame();
    }
    public void DisplayInfo(bool i)
    {
        info.SetActive(i);
    }

    private void Update()
    {
        if (gameStart)
        {
            time += Time.deltaTime;
            timeLabel.text = "Time: " + (int)time + "s";
        }
    }
    public void SaveGame()
    {
        if (!gameStart)
            return;
        GameState gameState = new GameState
        {
            score = score,
            time = time,
            cardIDs = cards.Select(c => c.ID).ToArray(),
            cardSpriteIDs = cards.Select(c => c.SpriteID).ToArray(),
            cardFlippedStates = cards.Select(c => c.IsFlipped).ToArray(),
            cardPositions = cards.Select(c => c.transform.localPosition).ToArray(),
            cardColors = cards.Select(c => c.GetComponent<Image>().color).ToArray(),
            cardScales = cards.Select(c => c.transform.localScale).ToArray()
        };
        GameStateManager.SaveGame(gameState);
    }
    public void LoadGame()
    {
        /*int isOdd = gameSize % 2;
        cards = new Card[gameSize * gameSize - isOdd];*/
        GameState gameState = GameStateManager.LoadGame();
        if (gameState == null)
            return;
        score = gameState.score;
        time = gameState.time;
        txt_Score.text = score.ToString();
        cardSelected = spriteSelected = -1;
        cardLeft = gameState.cardIDs.Length;
        cards = new Card[cardLeft];
        foreach (Transform child in cardList.transform)
        {
            Destroy(child.gameObject);
        }
        canvasGamePlay.enabled = true;
        info.SetActive(false);

        for (int i = 0; i < gameState.cardIDs.Length; i++)
        {
            GameObject cardObj = Instantiate(prefab, cardList.transform);
            Card card = cardObj.GetComponent<Card>();
            card.ID = gameState.cardIDs[i];
            card.SpriteID = gameState.cardSpriteIDs[i];
            cardObj.transform.localPosition = gameState.cardPositions[i];
            cardObj.transform.localScale = gameState.cardScales[i];
            card.GetComponent<Image>().color = gameState.cardColors[i];
            card.gameObject.SetActive(true);

            if (gameState.cardColors[i] == Color.clear)
            {
                cardObj.GetComponent<Image>().color = Color.clear;
            }
            else
            {
                card.Flip();
            }
            cards[i] = card;
        }

        gameStart = true;
    }
}
