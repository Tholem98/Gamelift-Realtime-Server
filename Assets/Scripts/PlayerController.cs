using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using GameLiftClient.cs;
using RTSGame;
//using UnityEngine.Networking;

public class PlayerController : MonoBehaviour
{

    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    private Vector3 currentPos;
    private Vector3 beforePos;
    private Quaternion currentRot;
    private Quaternion beforeRot;
    //public RealTimeClient Client;
    public RTSClient Client;
    //public Camera SceneCamera;
    public Camera PlayerCamera;

    // Start is called before the first frame update

    void Start()
    {
        //SceneCamera.enabled = false;
        PlayerCamera.enabled = true;
        Client.SceneReady();
    }

    // Update is called once per frame
    void Update()
    {
        //if (!isLocalPlayer)
        //{
        //    return;
        //}

        float x = Input.GetAxis("Horizontal") * Time.deltaTime * 150.0f;
        float z = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;

        transform.Rotate(0, x, 0);
        transform.Translate(-z, 0, 0);

        currentPos = transform.position;
        currentRot = transform.rotation;

        if (currentRot != beforeRot)
        {
            beforeRot = currentRot;
            Client.handleMovement(transform.position,transform.rotation);
        }

        if (currentPos != beforePos)
        {
            beforePos = currentPos;
            Client.handleMovement(transform.position, transform.rotation);
        }



        if (Input.GetKeyDown(KeyCode.Space))
        {
            CmdFire();
        }

    }

    //public override void OnStartLocalPlayer()
    //{
    //    //base.OnStartLocalPlayer();
    //    GetComponent<MeshRenderer>().material.color = Color.blue;
    //}
    //[Command]
    public void CmdFire()
    {
        GameObject bullet = (GameObject)Instantiate(bulletPrefab,bulletSpawn.position,bulletSpawn.rotation);
        bullet.GetComponent<Rigidbody>().velocity = -bullet.transform.right * 6.0f;
        //NetworkServer.Spawn(bullet);
        Client.HopButtonPressed();
        Destroy(bullet, 2);
    }
}
