using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponSwitcher : MonoBehaviour
{
    [Header("Permisos Locales")]
    public bool canSwitchWeapon = true;

    [Header("Referencias")]
    public PlayerHealth playerHealth;
    [SerializeField] private Animator animator; // Referencia al Animator para las animaciones del torso

    [Header("Arma Seleccionada")]
    [Tooltip("-1 significa que no tiene ningún arma seleccionada.")]
    public int selectedWeapon = -1;

    [Header("Armas Desbloqueadas")]
    [Tooltip("Indica si cada arma del WeaponHolder ha sido recogida.")]
    public bool[] unlockedWeapons;

    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();

        inputActions.Player.SelectWeapon.performed += ctx => OnScroll(ctx.ReadValue<float>());

        inputActions.Player.Weapon1.performed += _ => SelectWeaponIndex(0);
        inputActions.Player.Weapon2.performed += _ => SelectWeaponIndex(1);
        inputActions.Player.Weapon3.performed += _ => SelectWeaponIndex(2);
        inputActions.Player.Weapon4.performed += _ => SelectWeaponIndex(3);
        inputActions.Player.Weapon5.performed += _ => SelectWeaponIndex(4);
        inputActions.Player.Weapon6.performed += _ => SelectWeaponIndex(5);
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    void Start()
    {
        selectedWeapon = -1; // Forzamos el inicio sin armas

        if (playerHealth == null)
        {
            playerHealth = GetComponentInParent<PlayerHealth>();
        }

        // Busca el Animator automáticamente si no está asignado en el Inspector
        if (animator == null)
        {
            animator = GetComponentInParent<Animator>();
        }

        // Inicializar el arreglo de desbloqueo según la cantidad de armas hijas
        int childCount = transform.childCount;
        if (unlockedWeapons == null || unlockedWeapons.Length != childCount)
        {
            unlockedWeapons = new bool[childCount]; // Todas inician en false
        }

        SelectWeapon();
    }

    private void OnScroll(float scrollValue)
    {
        if (!CanSwitch() || !HasAnyWeaponUnlocked()) return;

        int previousSelected = selectedWeapon;
        int childCount = transform.childCount;

        if (scrollValue > 0f)
        {
            // Busca el siguiente índice desbloqueado
            int next = selectedWeapon;
            do
            {
                next = (next >= childCount - 1) ? 0 : next + 1;
            } while (!unlockedWeapons[next] && next != selectedWeapon);

            if (unlockedWeapons[next]) selectedWeapon = next;
        }
        else if (scrollValue < 0f)
        {
            // Busca el índice anterior desbloqueado
            int prev = selectedWeapon;
            do
            {
                prev = (prev <= 0) ? childCount - 1 : prev - 1;
            } while (!unlockedWeapons[prev] && prev != selectedWeapon);

            if (unlockedWeapons[prev]) selectedWeapon = prev;
        }

        if (previousSelected != selectedWeapon)
        {
            SelectWeapon();
        }
    }

    private void SelectWeaponIndex(int index)
    {
        if (!CanSwitch()) return;

        // Solo equipa si el índice existe, está desbloqueado y es diferente al actual
        if (index >= 0 && index < transform.childCount && unlockedWeapons[index] && selectedWeapon != index)
        {
            selectedWeapon = index;
            SelectWeapon();
        }
    }

    private bool CanSwitch()
    {
        return canSwitchWeapon && (playerHealth == null || !playerHealth.isDead) && !GameManager.IsPaused;
    }

    private bool HasAnyWeaponUnlocked()
    {
        if (unlockedWeapons == null) return false;
        foreach (bool unlocked in unlockedWeapons)
        {
            if (unlocked) return true;
        }
        return false;
    }

    public void UnlockWeapon(int index, bool autoEquip = true)
    {
        if (index >= 0 && index < transform.childCount)
        {
            unlockedWeapons[index] = true;

            if (autoEquip)
            {
                selectedWeapon = index;
                SelectWeapon();
            }
        }
    }

    public void SelectWeapon()
    {
        int i = 0;
        foreach (Transform weapon in transform)
        {
            bool shouldBeActive = (selectedWeapon != -1) && (i == selectedWeapon) && unlockedWeapons[i];
            weapon.gameObject.SetActive(shouldBeActive);
            i++;
        }

        if (animator != null)
        {
            int animatorValue = 0; // Por defecto 0 (Sin arma)

            if (selectedWeapon == -1)
            {
                animatorValue = 0; // Sin arma -> WeaponType = 0
            }
            else if (selectedWeapon == 0)
            {
                animatorValue = 1; // Pistola -> WeaponType = 1
            }
            else
            {
                animatorValue = 2; // Resto de armas (Rifles) -> WeaponType = 2
            }

            animator.SetInteger("WeaponType", animatorValue);
        }
    }
}