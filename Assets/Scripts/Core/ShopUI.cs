using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ShopUI : MonoBehaviour
{
    [Inject] private MetaProgress _meta;

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

    private void Start()
    {
        buyDoubleScoreButton.onClick.AddListener(OnBuyDoubleScore);
        nextLevelButton.onClick.AddListener(OnNextLevel);
        resetProgressButton.onClick.AddListener(OnResetProgress);
        closeButton.onClick.AddListener(Close);

        doubleScorePriceText.text = doubleScorePrice.ToString();
        nextLevelPriceText.text = nextLevelPrice.ToString();
    }

    public void Open()
    {
        shopPanel.SetActive(true);
        Refresh();
    }

    public void Close() => shopPanel.SetActive(false);

    private void Refresh()
    {
        bankText.text = $"Bank: {_meta.Bank}";
        levelText.text = $"Level: {_meta.Level}";

        buyDoubleScoreButton.interactable = !_meta.DoubleScorePurchased && _meta.Bank >= doubleScorePrice;
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
            statusText.text = $"Level up! Now level {_meta.Level}";
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