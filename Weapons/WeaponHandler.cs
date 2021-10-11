using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    Animator animator;
    SoundController sc;

    [System.Serializable]
    public class UserSettings
    {
        public Transform rightHand;
        public Transform pistolUnequipSpot;
        public Transform rifleUnequipSpot;
    }
    [SerializeField]
    public UserSettings userSettings;

    [System.Serializable]
    public class Animations
    {
        public string weaponTypeInt = "WeaponType";
        public string reloadingBool = "isReloading";
        public string aimingBool = "Aiming";
    }
    [SerializeField]
    public Animations animations;

    public Weapon currentWeapon;
    public List<Weapon> weaponsList = new List<Weapon>();
    public int maxWeapons = 2;
    public bool aim { get; protected set; }
    bool reload;
    int weaponType;
    bool settingWeapon;

    void Start() // Start is called before the first frame update
    {
        GameObject check = GameObject.FindGameObjectWithTag("Sound Controller");
        if (check != null)
            sc = check.GetComponent<SoundController>();
        animator = GetComponent<Animator>();
    }
    
    void Update() // Update is called once per frame
    {
        if (currentWeapon)
        {
            currentWeapon.SetEquipped(true);
            currentWeapon.SetOwner(this);
            AddWeaponToList(currentWeapon);
            if (currentWeapon.ammo.clipAmmo <= 0)
                Reload();
            if (reload)
                if (settingWeapon)
                    reload = false;
        }
        if (weaponsList.Count > 0)
        {
            for (int i = 0; i < weaponsList.Count; i++)
            {
                if (weaponsList[i] != currentWeapon)
                {
                    weaponsList[i].SetEquipped(false);
                    weaponsList[i].SetOwner(this);
                }
            }
        }
        Animate();
    }

    void Animate() // Animates the character
    {
        if (!animator)
            return;
        animator.SetBool(animations.aimingBool, aim);
        animator.SetBool(animations.reloadingBool, reload);
        animator.SetInteger(animations.weaponTypeInt, weaponType);
        if (!currentWeapon)
        {
            weaponType = 0;
            return;
        }
        switch (currentWeapon.weaponType)
        {
            case Weapon.WeaponType.Pistol : weaponType = 1;
                break;
            case Weapon.WeaponType.Rifle : weaponType = 2;
                break;
        }
    }
    
    void AddWeaponToList(Weapon weapon) // Adds a weapon to the weaponsList
    {
        if (weaponsList.Contains(weapon))
            return;
        weaponsList.Add(weapon);
    }
    
    public void FireCurrentWeapon(Ray aimRay) // Puts the finger on the trigger and asks if we pulled
    {
        if (currentWeapon.ammo.clipAmmo == 0)
        {
            Reload();
            return;
        }
        currentWeapon.Fire(aimRay);
    }

    public void Reload() // Reloads the current weapon
    {
        if (reload || !currentWeapon)
            return;
        if (currentWeapon.ammo.carryingAmmo <= 0 || currentWeapon.ammo.clipAmmo == currentWeapon.ammo.maxClipAmmo)
            return;
        if (sc != null)
        {
            if (currentWeapon.sounds.reloadSound != null)
            {
                if (currentWeapon.sounds.audioS != null)
                {
                    sc.PlaySound(currentWeapon.sounds.audioS, currentWeapon.sounds.reloadSound
                        , true, currentWeapon.sounds.pitchMin, currentWeapon.sounds.pitchMax);
                }
            }
        }
        reload = true;
        StartCoroutine(StopReload());
    }

    IEnumerator StopReload() // Stops the reloading of the weapon
    {
        yield return new WaitForSeconds(currentWeapon.weaponSettings.reloadDuration);
        currentWeapon.LoadClip();
        reload = false;
    }

    public void Aim(bool aiming) // Sets out aim bool to be what we pass it
    {
        aim = aiming;
    }

    public void DropCurWeapon() // Drops the current weapon
    {
        if (!currentWeapon)
            return;
        currentWeapon.SetEquipped(false);
        currentWeapon.SetOwner(null);
        weaponsList.Remove(currentWeapon);
        currentWeapon = null;
    }
  
    public void SwitchWeapons() // Switches to the next weapon
    {
        if (settingWeapon || weaponsList.Count == 0)
            return;
        if (currentWeapon)
        {
            int currentWeaponIndex = weaponsList.IndexOf(currentWeapon);
            int nextWeaponIndex = (currentWeaponIndex + 1) % weaponsList.Count;
            currentWeapon = weaponsList[nextWeaponIndex];
        }
        else
        {
            currentWeapon = weaponsList[0];
        }
        settingWeapon = true;
        StartCoroutine(StopSettingWeapon());
        //SetupWeapons();
    }

    IEnumerator StopSettingWeapon() // Stops swapping weapons
    {
        yield return new WaitForSeconds(0.7f);
        settingWeapon = false;
    }

    void OnAnimatorIK()
    {
        if (!animator)
            return;
        if (currentWeapon && currentWeapon.userSettings.leftHandIKTarget && weaponType == 2
            && !reload && !settingWeapon)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
            Transform target = currentWeapon.userSettings.leftHandIKTarget;
            Vector3 targetPos = target.position;
            Quaternion targetRot = target.rotation;
            animator.SetIKPosition(AvatarIKGoal.LeftHand, targetPos);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, targetRot);
        }
        else
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
        }
    }

    void SetupWeapons()
    {
        if (currentWeapon)
        {
            currentWeapon.SetEquipped(true);
            currentWeapon.SetOwner(this);
            AddWeaponToList(currentWeapon);
            if (currentWeapon.ammo.clipAmmo <= 0)
                Reload();

            if (reload)
                if (settingWeapon)
                    reload = false;
        }
        if (weaponsList.Count > 0)
        {
            for (int i = 0; i < weaponsList.Count; i++)
            {
                if (weaponsList[i] != currentWeapon)
                {
                    weaponsList[i].SetEquipped(false);
                    weaponsList[i].SetOwner(this);
                }
            }
        }
    }
}
