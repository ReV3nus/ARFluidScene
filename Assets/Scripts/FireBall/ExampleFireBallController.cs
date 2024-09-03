using System.Collections;
using System.Collections.Generic;
using Leap.Unity;
using Unity.VisualScripting;
using UnityEngine;

public class ExampleFireBallController : MonoBehaviour
{
    public GameObject fireBallPrefab;
    private FireBall fireBall;
    public CapsuleHand leftHand;
    public CapsuleHand rightHand;
    private Vector3 fireBallPos;
    
    private bool shooting = false;

    public Camera mainCamera;
    private void Start()
    {
    }

    private void Update()
    {

        
        if (!shooting && fireBall != null)
        {
            if (leftHand?.GetLeapHand()!=null)
            {
                fireBallPos = leftHand.GetLeapHand().PalmPosition.ToVector3();
            }
            else
            {
                fireBallPos = rightHand.GetLeapHand().PalmPosition.ToVector3();
            }
            
            
            Vector3 rayDirection = fireBallPos - mainCamera.transform.position;
            rayDirection.Normalize();

            Vector3 fireBallPosition = mainCamera.transform.position + rayDirection * 10.0f;

            fireBall.transform.position = fireBallPosition;

            // fireBallPos.x = -10.0f;
            // fireBall.transform.position = fireBallPos;
        }
        
        if (leftHand.holdingHandType(2, 1.0f) || rightHand.holdingHandType(2, 1.0f))
        {
            leftHand.previousTime = Time.time;
            rightHand.previousTime = Time.time;
            Debug.Log("holdingHandType in 2");

            if(fireBall == null)
            {
                if (leftHand?.GetLeapHand()!=null)
                {
                    fireBallPos = leftHand.GetLeapHand().PalmPosition.ToVector3();
                }
                else
                {
                    fireBallPos = rightHand.GetLeapHand().PalmPosition.ToVector3();
                }
                fireBallPos.z = -10.0f;
                
                
                GameObject go = Instantiate(fireBallPrefab, fireBallPos, Quaternion.identity);
                fireBall = go.GetComponent<FireBall>();
                shooting = false;
            }
        }
        if (leftHand.holdingHandType(1, 1.0f) || rightHand.holdingHandType(1, 1.0f))
        {
            Debug.Log("holdingHandType in 1");

            leftHand.previousTime = Time.time;
            rightHand.previousTime = Time.time;
            if (fireBall == null || shooting == true)
                return;
            shooting = true;
            Vector3 rayDirection = fireBallPos - mainCamera.transform.position;
            rayDirection.Normalize();
            fireBall.Shoot(rayDirection*15);

        }
        
        // if (Input.GetKeyDown(KeyCode.F))
        // {
        //     if (fireBall != null)
        //     {
        //         Destroy(fireBall.gameObject);
        //         fireBall = null;
        //     }
        //
        //     GameObject go = Instantiate(fireBallPrefab, new Vector3(-10f, 20f, 0f), Quaternion.identity);
        //     fireBall = go.GetComponent<FireBall>();
        //     shooting = false;
        // }
        if (Input.GetMouseButton(0))
        {
            if (fireBall == null || shooting == true)
                return;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                shooting = true;
                Vector3 hitPosition = hit.point;
                Vector3 dir = hitPosition - new Vector3(-10f, 20f, 0f);
                dir = dir.normalized * 30f;
                // dir = new Vector3(10f, 0f, 0f);
                fireBall.Shoot(dir);
            }
        }
    }

    public void SetShooting(bool state)
    {
        if (shooting != state)
        {
            shooting = state;
        }
    }
}
