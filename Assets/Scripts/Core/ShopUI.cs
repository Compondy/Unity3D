using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ShopUI : MonoBehaviour
{
    [Inject] private MetaProgress _meta;
    [Inject] private GameManager _gameManager;

    [Header("Panel")]
    [SerializeField] private GameObject shopPanel;

    [Header("UI")]
    [SerializeField] private TMP_Text bankText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text statusText;

    [SerializeField] private Button buyDoubleScoreButton;
    [SerializeField] private TMP_Text doubleScorePriceText;

    [SerializeField] private Button nextLevelButton;
    [SerializeField] private TMP_Text nextLevelPriceText;

    [SerializeField] private Button resetProgressButton;
    [SerializeField] private Button closeButton;

    [Header("Prices")]
    [SerializeField] private int doubleScorePrice = 500;
    [SerializeField] private int nextLevelPrice = 1000;
    
    private bool _pausedByShop;
    private bool _isInitialized = false;
    public bool IsOpen => shopPanel != null && shopPanel.activeSelf;

    private void Start()
    {
        if (shopPanel == null)
        {
            Debug.LogError("[ShopUI] shopPanel is not assigned");
            return;
        }

        if (buyDoubleScoreButton == null)
        {
            Debug.LogError("[ShopUI] buyDoubleScoreButton is not assigned");
            return;
        }

        if (nextLevelButton == null)
        {
            Debug.LogError("[ShopUI] nextLevelButton is not assigned");
            return;
        }

        if (resetProgressButton == null)
        {
            Debug.LogError("[ShopUI] resetProgressButton is not assigned");
            return;
        }

        if (closeButton == null)
        {
            Debug.LogError("[ShopUI] closeButton is not assigned");
            return;
        }

        buyDoubleScoreButton.onClick.AddListener(OnBuyDoubleScore);
        nextLevelButton.onClick.AddListener(OnNextLevel);
        resetProgressButton.onClick.AddListener(OnResetProgress);
        closeButton.onClick.AddListener(Close);

        if (doubleScorePriceText != null)
            doubleScorePriceText.text = doubleScorePrice.ToString();

        if (nextLevelPriceText != null)
            nextLevelPriceText.text = nextLevelPrice.ToString();

        _isInitialized = true;
    }

    public void Open()
    {
        if (!_isInitialized)
        {
            Debug.LogError("[ShopUI] Shop is not properly initialized. Check inspector assignments.");
            return;
        }

        if (shopPanel == null) { Debug.LogError("[ShopUI] shopPanel is null in Open()"); return; }

        if (_gameManager.CurrentState == GameManager.State.Playing)
        {
            _gameManager.Pause(showPausePanel: false);
            _pausedByShop = true;
        }
        else
        {
            _pausedByShop = false;
        }

        shopPanel.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        if (shopPanel == null) { Debug.LogWarning("[ShopUI] shopPanel is null in Close()"); return; }

        shopPanel.SetActive(false);

        if (_pausedByShop)
        {
            _gameManager.Resume(hidePausePanel: false);
            _pausedByShop = false;
        }
    }

    private void Refresh()
    {
        if (!bankText) return;
        bankText.text = $"Bank: {_meta.Bank}";

        if (!levelText) return;
        int bonusPercent = Mathf.RoundToInt((_meta.BonusDurationMultiplier - 1f) * 100f);
        int nextBonusPercent = Mathf.RoundToInt(((_meta.BonusDurationMultiplier + 0.2f) - 1f) * 100f);

        string cityUnlockInfo = _meta.Level >= 2
            ? " | City Zone: Unlocked!"
            : " | City Zone: Locked (Level 2 required)";

        levelText.text = $"Level: {_meta.Level} (+{bonusPercent}% bonus duration{cityUnlockInfo})\nNext: +{nextBonusPercent}% bonus duration (Unlocks City Zone if Level 2)";

        if (buyDoubleScoreButton != null)
            buyDoubleScoreButton.interactable = !_meta.DoubleScorePurchased && _meta.Bank >= doubleScorePrice;
        
        if (nextLevelButton != null)
            nextLevelButton.interactable = _meta.Bank >= nextLevelPrice;

        statusText.text = _meta.DoubleScorePurchased
            ? "DoubleScore: purchased"
            : "DoubleScore: not purchased";
    }

    private void OnBuyDoubleScore()
    {
        if (_meta.BuyDoubleScore(doubleScorePrice))
            statusText.text = "DoubleScore purchased!";
        else
            statusText.text = "Not enough score";

        Refresh();
    }

    private void OnNextLevel()
    {
        if (_meta.TrySpend(nextLevelPrice))
        {
            _meta.NextLevel();
            string extraMsg = _meta.Level >= 2 ? " City Zone unlocked!" : "";
            statusText.text = $"Level up! Now level {_meta.Level}.{extraMsg}";
        }
        else
            statusText.text = "Not enough score";

        Refresh();
    }

    private void OnResetProgress()
    {
        _meta.ResetAll();
        statusText.text = "Progress reset";
        Refresh();
    }
}