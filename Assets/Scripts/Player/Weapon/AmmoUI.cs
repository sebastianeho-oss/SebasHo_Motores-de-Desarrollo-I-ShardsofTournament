using UnityEngine;
using TMPro;

public class AmmoUI : MonoBehaviour
{
    [Header("Componentes de Texto UI")]
    public TextMeshProUGUI weaponNameText;
    public TextMeshProUGUI ammoText;

    [Header("Referencias del Jugador")]
    public WeaponSwitcher weaponSwitcher;
    public PlayerAmmoInventory playerInventory;

    [Header("Colores Visuales")]
    public Color normalColor = Color.white;
    public Color reloadingColor = Color.yellow;
    public Color emptyColor = Color.red;

    void Update()
    {
        if (weaponSwitcher == null || playerInventory == null) return;

        // Si el jugador está desarmado (-1) o el índice está fuera de rango, limpia la UI
        if (weaponSwitcher.selectedWeapon < 0 || weaponSwitcher.selectedWeapon >= weaponSwitcher.transform.childCount)
        {
            ClearUI();
            return;
        }

        Transform activeWeaponTransform = weaponSwitcher.transform.GetChild(weaponSwitcher.selectedWeapon);

        if (activeWeaponTransform == null || !activeWeaponTransform.gameObject.activeSelf)
        {
            ClearUI();
            return;
        }

        WeaponAmmo activeAmmo = activeWeaponTransform.GetComponent<WeaponAmmo>();

        if (activeAmmo != null)
        {
            if (weaponNameText != null)
            {
                weaponNameText.text = activeWeaponTransform.gameObject.name.ToUpper();
            }

            if (ammoText != null)
            {
                int currentInMag = activeAmmo.currentAmmo;
                int reserve = playerInventory.GetReserveAmmo(activeAmmo.ammoType);

                if (activeAmmo.IsReloading)
                {
                    ammoText.text = "RECARGANDO";
                    ammoText.color = reloadingColor;
                }
                else if (currentInMag == 0 && reserve == 0)
                {
                    ammoText.text = "SIN MUNICION";
                    ammoText.color = emptyColor;
                }
                else
                {
                    ammoText.text = $"{currentInMag} <size=70%><color=#A0A0A0>/ {reserve}</color></size>";
                    ammoText.color = (currentInMag == 0) ? emptyColor : normalColor;
                }
            }
        }
        else
        {
            ClearUI();
        }
    }

    private void ClearUI()
    {
        if (weaponNameText != null)
        {
            weaponNameText.text = "";
        }

        if (ammoText != null)
        {
            ammoText.text = "";
        }
    }
}