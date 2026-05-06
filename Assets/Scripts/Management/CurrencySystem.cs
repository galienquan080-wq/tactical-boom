using UnityEngine;
using System;

/// <summary>
/// Quản lý tiền tệ, mua vũ khí, phụ kiện
/// </summary>
public class CurrencySystem : MonoBehaviour
{
    private int currentCurrency = 5000; // Starting money
    public event Action<int> OnCurrencyChanged;

    void Start()
    {
        OnCurrencyChanged?.Invoke(currentCurrency);
    }

    public void AddCurrency(int amount)
    {
        currentCurrency += amount;
        OnCurrencyChanged?.Invoke(currentCurrency);
        Debug.Log($"[CURRENCY] Added ${amount}. Total: ${currentCurrency}");
    }

    public bool SpendCurrency(int amount)
    {
        if (currentCurrency >= amount)
        {
            currentCurrency -= amount;
            OnCurrencyChanged?.Invoke(currentCurrency);
            Debug.Log($"[CURRENCY] Spent ${amount}. Remaining: ${currentCurrency}");
            return true;
        }
        return false;
    }

    public int GetCurrentCurrency() => currentCurrency;

    public void PurchaseWeapon(WeaponSystem.WeaponType weaponType, int price)
    {
        if (SpendCurrency(price))
        {
            // TODO: Add weapon to inventory
            Debug.Log($"[SHOP] Purchased weapon!");
        }
    }

    public void PurchaseAttachment(WeaponSystem.Attachment attachment)
    {
        if (SpendCurrency(attachment.price))
        {
            // TODO: Add attachment
            Debug.Log($"[SHOP] Purchased attachment: {attachment.attachmentName}");
        }
    }
}
