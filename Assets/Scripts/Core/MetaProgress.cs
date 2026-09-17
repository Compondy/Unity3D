using Zenject;

public class MetaProgress
{
    public const string KEY_BANK = "Meta.Bank";            // накопленный Score
    public const string KEY_LEVEL = "Meta.Level";          // текущий уровень
    public const string KEY_DOUBLE = "Meta.DoubleScore";   // куплен ли DoubleScore

    private readonly ISaveService _save;

    public int Bank { get; private set; }
    public int Level { get; private set; }
    public bool DoubleScorePurchased { get; private set; }

    public MetaProgress(ISaveService save)
    {
        _save = save;
        Load();
    }

    public void Load()
    {
        Bank = _save.GetInt(KEY_BANK, 0);
        Level = _save.GetInt(KEY_LEVEL, 1);
        DoubleScorePurchased = _save.GetBool(KEY_DOUBLE, false);
    }

    public void AddToBank(int amount)
    {
        Bank += amount;
        _save.SetInt(KEY_BANK, Bank);
        _save.Save();
    }

    public bool TrySpend(int amount)
    {
        if (Bank < amount) return false;
        Bank -= amount;
        _save.SetInt(KEY_BANK, Bank);
        _save.Save();
        return true;
    }

    public bool BuyDoubleScore(int price)
    {
        if (DoubleScorePurchased) return false;
        if (!TrySpend(price)) return false;
        DoubleScorePurchased = true;
        _save.SetBool(KEY_DOUBLE, true);
        _save.Save();
        return true;
    }

    public void NextLevel()
    {
        Level++;
        _save.SetInt(KEY_LEVEL, Level);
        _save.Save();
    }

    public void ResetAll()
    {
        Bank = 0;
        Level = 1;
        DoubleScorePurchased = false;
        _save.DeleteAll();
        _save.Save();
    }
}