using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;

public class PlayerShoot : NetworkBehaviour
{
    [SerializeField]
    private Transform playerHead;
    [SerializeField]
    private Transform viewModel;

    public PlayerWeapon weapon;

    [SerializeField]
    private ParticleSystem bulletTracers;

    private Vector3 tracerReset;
    
    private float bulletDelayTimer;

    [SerializeField]
    private GameObject backSight;

    [SerializeField]
    private LayerMask mask;

    private Vector3 barrelStart;
    private Vector3 barrelEnd;

    private float inaccuracyRadius = 0f;

    private bool shot = false;
    private float kick;

    void Start()
    {
        if(backSight == null)
        {
            Debug.LogError("PlayerShoot: No weapon referenced");
            this.enabled = false;
        }

        tracerReset = bulletTracers.transform.localEulerAngles;
    }

    
    void Update()
    {
        bulletDelayTimer += Time.deltaTime;

        if (bulletDelayTimer >= weapon.secondShotDelay)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Shoot();
                shot = true;

                bulletDelayTimer = 0f;
            }
        }
    }

    private void FixedUpdate()
    {
        Recoil();
    }

    void Shoot()
    {
        bulletTracers.transform.localEulerAngles = tracerReset;

        barrelStart = backSight.transform.position - Vector3.up * 0.03f;

        barrelEnd = viewModel.forward + BulletInaccuracy(inaccuracyRadius);

        RaycastHit _hit;

        

        //uses the gun's sights to line up a gun barrel
        if(Physics.Raycast(barrelStart, barrelEnd, out _hit, weapon.range, mask))
        {
            if(_hit.collider.tag == "Player_Hitbox")
            {
                int _damageMultiplier = 1;
                if(_hit.transform.gameObject.layer == LayerMask.NameToLayer("HeadHitbox"))
                {
                    _damageMultiplier = 5;
                    Debug.Log("You shot " + _hit.transform.parent.parent.name + " in the head");
                }
                else if(_hit.transform.gameObject.layer == LayerMask.NameToLayer("BodyHitbox"))
                {
                    _damageMultiplier = 1;
                    Debug.Log("You shot " + _hit.transform.parent.parent.name + " in the body");
                }

                PlayerShotServerRpc(_hit.transform.parent.parent.name, weapon.damage, _damageMultiplier);
            }
        }

        bulletTracers.Play();
    }

    //takes a radius, finds a random point on a circle with that radius and converts to polar form
    private Vector3 BulletInaccuracy(float inaccuracy)
    {
        float r = Random.Range(0, inaccuracy);
        float theta = Random.Range(0, 2 * Mathf.PI);

        float xAngle = Mathf.Atan((Mathf.Cos(theta) * r) / playerHead.forward.magnitude);
        float yAngle = Mathf.Atan((Mathf.Sin(theta) * r) / playerHead.forward.magnitude);

        bulletTracers.transform.localEulerAngles += Vector3.up * xAngle * Mathf.Rad2Deg - Vector3.right * yAngle * Mathf.Rad2Deg;

        float x = 1 / (Mathf.Sqrt(1 + Mathf.Pow(Mathf.Tan(theta), 2)));
        Vector2 inaccuracyShift = new Vector2(x, x * Mathf.Tan(theta));

        if(theta > Mathf.PI / 2 && theta < 3 * Mathf.PI / 2)
        {
            inaccuracyShift *= -1f;
        }

        inaccuracyShift *= r;

        return new Vector3(0f, inaccuracyShift.y, -inaccuracyShift.x);
    }

    [ServerRpc]
    void PlayerShotServerRpc(string _playerID, int _damage, int damageMultiplier)
    {
        Debug.Log(_playerID + " has been shot");

        Player _player = GameManager.GetPlayer(_playerID);
        _player.TakeDamage(_damage * damageMultiplier);
    }

    public void UpdateInaccuracy(float inaccuracy, bool isCrouched)
    {
        inaccuracyRadius = inaccuracy;

        if(!GetComponent<PlayerViewModel>().ADS)
        {
            if(isCrouched)
            {
                inaccuracyRadius += 0.02f;
            }
            else
            {
                inaccuracyRadius += 0.04f;
            }
            
        }
    }

    //should be in specific weapon script, here for now
    private void Recoil()
    {
        if(shot)
        {
            kick += weapon.recoil;

            playerHead.transform.localEulerAngles += new Vector3(-weapon.recoil, 0f, 0f);
            viewModel.transform.localEulerAngles += new Vector3(-weapon.recoil * 2, 0f, 0f);

            shot = false;
        }
        
        if(kick != 0)
        {
            playerHead.transform.localEulerAngles = Vector3.Lerp(playerHead.transform.localEulerAngles, playerHead.transform.localEulerAngles + Vector3.right * kick, 0.2f);
            viewModel.transform.localEulerAngles = Vector3.Lerp(viewModel.transform.localEulerAngles, viewModel.transform.localEulerAngles + Vector3.right * kick * 2f, 0.2f);

            kick = Mathf.Lerp(kick, 0f, 0.2f);
        }
    }
}
