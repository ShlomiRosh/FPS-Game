using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInput : MonoBehaviour
{
    public CharacterMovment characterMove { get; protected set; }
    public WeaponHandler weaponHandler { get; protected set; }

    [System.Serializable]
    public class InputSettings
    {
        public string verticalAxis = "Vertical";
        public string horizontalAxis = "Horizontal";
        public string jumpButton = "Jump";
        public string reloadButton = "Reload";
        public string aimButton = "Fire2";
        public string fireButton = "Fire1";
        public string dropWeaponButton = "DropWeapon";
        public string switchWeaponButton = "SwitchWeapon";
    }
    [SerializeField]
    public InputSettings input;

    [System.Serializable]
    public class OtherSettings
    {
        public float lookSpeed = 5.0f;
        public float lookDistance = 30.0f;
        public bool requireInputForTurn = true;
        public LayerMask aimDetectionLayers;
    }
    [SerializeField]
    public OtherSettings other;
    public Camera TPFCamera;
    public bool debugAim;
    public Transform spine;
    bool aiming;
    Dictionary<Weapon, GameObject> crosshairPrefabMap = new Dictionary<Weapon, GameObject>();
    void Start() // Start is called before the first frame update
    {
        characterMove = GetComponent<CharacterMovment>();
        weaponHandler = GetComponent<WeaponHandler>();
        SetupCrosshairs();
    }

    void SetupCrosshairs()
    {
        if (weaponHandler.weaponsList.Count > 0)
        {
            foreach (Weapon wep in weaponHandler.weaponsList)
            {
                GameObject prefab = wep.weaponSettings.crosshairPrefab;
                if (prefab != null)
                {
                    GameObject clone = (GameObject)Instantiate(prefab);
                    crosshairPrefabMap.Add(wep, clone);
                    ToggleCrosshair(false, wep);
                }
            }
        }
    }

    void Update() // Update is called once per frame
    {
        CharacterLogic();
        CameraLookLogic();
        WeaponLogic();
    }

    void LateUpdate()
    {
        if (weaponHandler)
        {
            if (weaponHandler.currentWeapon)
            {
                if (aiming)
                    PositionSpine();
            }
        }
    }

    void CharacterLogic() // Handles character logic
    {
        if (!characterMove)
            return;
        characterMove.Animate(Input.GetAxis(input.verticalAxis), Input.GetAxis(input.horizontalAxis));
        if (Input.GetButtonDown(input.jumpButton))
            characterMove.Jump();
    }

    void CameraLookLogic() // Handles camera logic
    {
        if (!TPFCamera)
            return;
        other.requireInputForTurn = !aiming;
        if (other.requireInputForTurn)
        {
            if (Input.GetAxis(input.horizontalAxis) != 0 || Input.GetAxis(input.verticalAxis) != 0)
            {
                CharacterLook();
            }
        }
        else
        {
            CharacterLook();
        }
    }
    
    void WeaponLogic() // Handles all weapon logic
    {
        if (!weaponHandler)
            return;
        aiming = Input.GetButton(input.aimButton) || debugAim;
        weaponHandler.Aim(aiming);
        if (Input.GetButtonDown(input.switchWeaponButton))
        {
            weaponHandler.SwitchWeapons();
            UpdateCrosshairs();
        }
        if (weaponHandler.currentWeapon)
        {
            Ray aimRay = new Ray(TPFCamera.transform.position, TPFCamera.transform.forward);
            //Debug.DrawRay (aimRay.origin, aimRay.direction);
            if (Input.GetButton(input.fireButton) && aiming)
                weaponHandler.FireCurrentWeapon(aimRay);
            if (Input.GetButtonDown(input.reloadButton))
                weaponHandler.Reload();
            if (Input.GetButtonDown(input.dropWeaponButton))
            {
                DeleteCrosshair(weaponHandler.currentWeapon);
                weaponHandler.DropCurWeapon();
            }
            if (aiming)
            {
                ToggleCrosshair(true, weaponHandler.currentWeapon);
                PositionCrosshair(aimRay, weaponHandler.currentWeapon);
            }
            else
                ToggleCrosshair(false, weaponHandler.currentWeapon);
        }
        else
            TurnOffAllCrosshairs();
    }

    void TurnOffAllCrosshairs()
    {
        foreach (Weapon wep in crosshairPrefabMap.Keys)
        {
            ToggleCrosshair(false, wep);
        }
    }

    void CreateCrosshair(Weapon wep)
    {
        GameObject prefab = wep.weaponSettings.crosshairPrefab;
        if (prefab != null)
        {
            prefab = Instantiate(prefab);
            ToggleCrosshair(false, wep);
        }
    }

    void DeleteCrosshair(Weapon wep)
    {
        if (!crosshairPrefabMap.ContainsKey(wep))
            return;
        Destroy(crosshairPrefabMap[wep]);
        crosshairPrefabMap.Remove(wep);
    }
   
    void PositionCrosshair(Ray ray, Weapon wep) // Position the crosshair to the point that we are aiming
    {
        Weapon curWeapon = weaponHandler.currentWeapon;
        if (curWeapon == null)
            return;
        if (!crosshairPrefabMap.ContainsKey(wep))
            return;
        GameObject crosshairPrefab = crosshairPrefabMap[wep];
        RaycastHit hit;
        Transform bSpawn = curWeapon.weaponSettings.bulletSpawn;
        Vector3 bSpawnPoint = bSpawn.position;
        Vector3 dir = ray.GetPoint(curWeapon.weaponSettings.range) - bSpawnPoint;
        if (Physics.Raycast(bSpawnPoint, dir, out hit, curWeapon.weaponSettings.range,
            curWeapon.weaponSettings.bulletLayers))
        {
            if (crosshairPrefab != null)
            {
                ToggleCrosshair(true, curWeapon);
                crosshairPrefab.transform.position = hit.point;
                crosshairPrefab.transform.LookAt(Camera.main.transform);
            }
        }
        else
        {
            ToggleCrosshair(false, curWeapon);
        }
    }

    void ToggleCrosshair(bool enabled, Weapon wep) // Toggle on and off the crosshair prefab
    {
        if (!crosshairPrefabMap.ContainsKey(wep))
            return;
        crosshairPrefabMap[wep].SetActive(enabled);
    }

    void UpdateCrosshairs()
    {
        if (weaponHandler.weaponsList.Count == 0)
            return;
        foreach (Weapon wep in weaponHandler.weaponsList)
        {
            if (wep != weaponHandler.currentWeapon)
            {
                ToggleCrosshair(false, wep);
            }
        }
    }
    void CharacterLook() // Make the character look at a forward point from the camera
    {
        Transform mainCamT = TPFCamera.transform;
        Transform pivotT = mainCamT.parent;
        Vector3 pivotPos = pivotT.position;
        Vector3 lookTarget = pivotPos + (pivotT.forward * other.lookDistance);
        Vector3 thisPos = transform.position;
        Vector3 lookDir = lookTarget - thisPos;
        Quaternion lookRot = Quaternion.LookRotation(lookDir);
        lookRot.x = 0;
        lookRot.z = 0;
        Quaternion newRotation = Quaternion.Lerp(transform.rotation, lookRot, Time.deltaTime * other.lookSpeed);
        transform.rotation = newRotation;
    }

    void PositionSpine() // Postions the spine when aiming
    {
        if (!spine || !weaponHandler.currentWeapon || !TPFCamera)
            return;
        Transform mainCamT = TPFCamera.transform;
        Vector3 mainCamPos = mainCamT.position;
        Vector3 dir = mainCamT.forward;
        Ray ray = new Ray(mainCamPos, dir);
        spine.LookAt(ray.GetPoint(50));
        Vector3 eulerAngleOffset = weaponHandler.currentWeapon.userSettings.spineRotation;
        spine.Rotate(eulerAngleOffset);
    }
}
