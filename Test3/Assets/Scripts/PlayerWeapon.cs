
using UnityEngine;

[System.Serializable]
public class PlayerWeapon
{
    public string name = "pistol";

    public int damage = 30;
    public float range = 100f;

    public int recoil = 3;
    public float recoilReset = 0.5f;

    public float secondShotDelay = 0.1f;
}
