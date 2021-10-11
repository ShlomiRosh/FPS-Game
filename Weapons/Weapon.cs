using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Weapon : MonoBehaviour
{
    Collider col;
    Rigidbody rigidBody;
    Animator animator;
    SoundController sc;
    public enum WeaponType
    {
        Pistol, Rifle
    }
    public WeaponType weaponType;

    [System.Serializable]
    public class UserSettings
    {
        public Transform leftHandIKTarget;
        public Vector3 spineRotation;
    }
    [SerializeField]
    public UserSettings userSettings;

    [System.Serializable]
    public class WeaponSettings
    {
        [Header("-Bullet Options-")]
        public Transform bulletSpawn;
        public float damage = 5.0f;
        public float bulletSpread = 5.0f;
        public float fireRate = 0.2f;
        public LayerMask bulletLayers;
        public float range = 200.0f; // range for sniper rifle, rifle, pistol Determined from the inspector

        [Header("-Effects-")]
        public GameObject muzzleFlash;
        public GameObject decal;
        public GameObject shell;
        public GameObject clip;

        [Header("-Other-")]
        public GameObject crosshairPrefab;
        public float reloadDuration = 2.0f;
        public Transform shellEjectSpot;
        public float shellEjectSpeed = 7.5f;
        public Transform clipEjectPos;
        public GameObject clipGO;

        [Header("-Positioning-")]
        public Vector3 equipPosition;
        public Vector3 equipRotation;
        public Vector3 unequipPosition;
        public Vector3 unequipRotation;

        [Header("-Animation-")]
        public bool useAnimation;
        public int fireAnimationLayer = 0;
        public string fireAnimationName = "Fire";
    }
    [SerializeField]
    public WeaponSettings weaponSettings;

    [System.Serializable]
    public class Ammunition
    {
        public int carryingAmmo;
        public int clipAmmo;
        public int maxClipAmmo;
    }
    [SerializeField]
    public Ammunition ammo;

    [System.Serializable]
    public class SoundSettings
    {
        public AudioClip[] gunshotSounds;
        public AudioClip reloadSound;
        [Range(0, 3)] public float pitchMin = 1;
        [Range(0, 3)] public float pitchMax = 1.2f;
        public AudioSource audioS;
    }
    [SerializeField]
    public SoundSettings sounds;
    WeaponHandler owner;
    bool equipped;
    bool resettingCartridge;
 
    void Start() // Start is called before the first frame update
    {
        GameObject check = GameObject.FindGameObjectWithTag("Sound Controller");
        if (check != null)
        {
            sc = check.GetComponent<SoundController>();
        }
        col = GetComponent<Collider>();
        rigidBody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }
    
    void Update() // Update is called once per frame
    {
        if (owner)
        {
            DisableEnableComponents(false);
            if (equipped)
            {
                if (owner.userSettings.rightHand)
                {
                    Equip();
                }
            }
            else
            {
                if (weaponSettings.bulletSpawn.childCount > 0)
                {
                    foreach (Transform t in weaponSettings.bulletSpawn.GetComponentsInChildren<Transform>())
                    {
                        if (t != weaponSettings.bulletSpawn)
                        {
                            Destroy(t.gameObject);
                        }
                    }
                }
                Unequip(weaponType);
            }
        }
        else // If owner is null
        { 
            DisableEnableComponents(true);
            transform.SetParent(null);
        }
    }
    
    public void Fire(Ray ray) // This fires the weapon
    {
        if (ammo.clipAmmo <= 0 || resettingCartridge || !weaponSettings.bulletSpawn || !equipped)
            return;
        RaycastHit hit;
        Transform bSpawn = weaponSettings.bulletSpawn;
        Vector3 bSpawnPoint = bSpawn.position;
        Vector3 dir = ray.GetPoint(weaponSettings.range) - bSpawnPoint;
        dir += (Vector3)Random.insideUnitCircle * weaponSettings.bulletSpread;
        if (Physics.Raycast(bSpawnPoint, dir, out hit, weaponSettings.range, weaponSettings.bulletLayers))
        {
            HitEffects(hit);
        }
        GunEffects();
        if (weaponSettings.useAnimation)
            animator.Play(weaponSettings.fireAnimationName, weaponSettings.fireAnimationLayer);
        ammo.clipAmmo--;
        resettingCartridge = true;
        StartCoroutine(LoadNextBullet());
    }

    IEnumerator LoadNextBullet() // Loads the next bullet into the chamber
    {
        yield return new WaitForSeconds(weaponSettings.fireRate);
        resettingCartridge = false;
    }

    void HitEffects(RaycastHit hit) // Effect on objects we hit
    {
        if (hit.collider.gameObject.isStatic)
        {
            if (weaponSettings.decal)
            {
                Vector3 hitPoint = hit.point;
                Quaternion lookRotation = Quaternion.LookRotation(hit.normal);
                GameObject decal = Instantiate(weaponSettings.decal, hitPoint, lookRotation) as GameObject;
                Transform decalT = decal.transform;
                Transform hitT = hit.transform;
                decalT.SetParent(hitT);
                Destroy(decal, Random.Range(15.0f, 20.0f));
            }
        }
    }

    void GunEffects() // Effect whan we fire
    {
        if (weaponSettings.muzzleFlash)
        {
            Vector3 bulletSpawnPos = weaponSettings.bulletSpawn.position;
            GameObject muzzleFlash = Instantiate(weaponSettings.muzzleFlash
                , bulletSpawnPos, Quaternion.identity) as GameObject;
            Transform muzzleT = muzzleFlash.transform;
            muzzleT.SetParent(weaponSettings.bulletSpawn);
            Destroy(muzzleFlash, 2.0f);
        }
        if (weaponSettings.shell)
        {
            if (weaponSettings.shellEjectSpot)
            {
                Vector3 shellEjectPos = weaponSettings.shellEjectSpot.position;
                Quaternion shellEjectRot = weaponSettings.shellEjectSpot.rotation;
                GameObject shell = Instantiate(weaponSettings.shell, shellEjectPos, shellEjectRot) as GameObject;
                if (shell.GetComponent<Rigidbody>())
                {
                    Rigidbody rigidB = shell.GetComponent<Rigidbody>();
                    rigidB.AddForce(weaponSettings.shellEjectSpot.forward * weaponSettings.shellEjectSpeed
                        , ForceMode.Impulse);
                }
                Destroy(shell, Random.Range(15.0f, 20.0f));
            }
        }
        PlayGunshotSound();
    }

    void PlayGunshotSound()
    {
        if (sc == null)
        {
            return;
        }
        if (sounds.audioS != null)
        {
            if (sounds.gunshotSounds.Length > 0)
            {
                sc.InstantiateClip(
                    weaponSettings.bulletSpawn.position, // Where we want to play the sound from
                    sounds.gunshotSounds[Random.Range(0, sounds.gunshotSounds.Length)],  // What audio clip we will use for this sound
                    2, // How long before we destroy the audio
                    true, // Do we want to randomize the sound?
                    sounds.pitchMin, // The minimum pitch that the sound will use.
                    sounds.pitchMax); // The maximum pitch that the sound will use.
            }
        }
    }

    void DisableEnableComponents(bool enabled) // Disables or enables collider and rigidbody
    {
        if (!enabled)
        {
            rigidBody.isKinematic = true;
            col.enabled = false;
        }
        else
        {
            rigidBody.isKinematic = false;
            col.enabled = true;
        }
    }

    void Equip() // Equips this weapon to the hand
    {
        if (!owner)
            return;
        else if (!owner.userSettings.rightHand)
            return;
        transform.SetParent(owner.userSettings.rightHand);
        transform.localPosition = weaponSettings.equipPosition;
        Quaternion equipRot = Quaternion.Euler(weaponSettings.equipRotation);
        transform.localRotation = equipRot;
    }

    void Unequip(WeaponType wpType) // Unequips the weapon and places it to the desired location
    {
        if (!owner)
            return;
        switch (wpType)
        {
            case WeaponType.Pistol : transform.SetParent(owner.userSettings.pistolUnequipSpot);
                break;
            case WeaponType.Rifle : transform.SetParent(owner.userSettings.rifleUnequipSpot);
                break;
        }
        transform.localPosition = weaponSettings.unequipPosition;
        Quaternion unEquipRot = Quaternion.Euler(weaponSettings.unequipRotation);
        transform.localRotation = unEquipRot;
    }

    public void LoadClip() // Loads the clip and calculates the ammo
    {
        int ammoNeeded = ammo.maxClipAmmo - ammo.clipAmmo;
        if (ammoNeeded >= ammo.carryingAmmo)
        {
            ammo.clipAmmo = ammo.carryingAmmo;
            ammo.carryingAmmo = 0;
        }
        else
        {
            ammo.carryingAmmo -= ammoNeeded;
            ammo.clipAmmo = ammo.maxClipAmmo;
        }
    }
   
    public void SetEquipped(bool equip) // Sets the weapons equip state
    {
        equipped = equip;
    }

    public void SetOwner(WeaponHandler wp) // Sets the owner of this weapon
    {
        owner = wp;
    }
}
