using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private UserInput player { get { return FindObjectOfType<UserInput>(); } set { player = value; } }
    private WeaponHandler wp { get { return player.GetComponent<WeaponHandler>(); } set { wp = value; } }
    private PlayerUI playerUI { get { return FindObjectOfType<PlayerUI>(); } set { playerUI = value; } }
    public static GameController GC;

    void Awake() // Play Before start
    {
        if (GC == null)
        {
            GC = this;
        }
        else
        {
            if (GC != this)
            {
                Destroy(gameObject);
            }
        }
    }

    void Update() // Update is called once per frame
    {
        UpdateUI();
    }

    void UpdateUI() // UpdateUI on the screen live [ammo live bar ect]
    {
        if (player && playerUI)
        {
            if (wp)
            {
                if (playerUI.ammoText)
                {
                    if (wp.currentWeapon == null || (wp.currentWeapon.ammo.clipAmmo == 0 
                        && wp.currentWeapon.ammo.carryingAmmo == 0))
                    {
                        playerUI.ammoText.text = "UNARMED.";
                    }
                    else
                    {
                        playerUI.ammoText.text = wp.currentWeapon.ammo.clipAmmo + "/"
                            + wp.currentWeapon.ammo.carryingAmmo;
                    }
                }            
            }
            if (playerUI.healthBar && playerUI.healthText)
            {
               playerUI.healthText.text = Mathf.Round(playerUI.healthBar.value).ToString();
            }                
        }
    }
}
