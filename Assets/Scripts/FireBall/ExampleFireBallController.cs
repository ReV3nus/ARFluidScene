using System.Collections;
using System.Collections.Generic;
using Leap.Unity;
using Unity.VisualScripting;
using UnityEngine;

public class ExampleFireBallController : MonoBehaviour
{
    public GameObject fireBallPrefab;
    public GameObject fireBallTargetPrefab;
    private GameObject fireBallTargetGameObject;
    private FireBall fireBall;
    private FireBall fireBallTarget;

    public CapsuleHand leftHand;
    public CapsuleHand rightHand;
    private Vector3 PalmPos;
    private Vector3 PalmNormal;
    public Vector3 fireBallOffest;
    
    private bool shooting = false;

    public Camera mainCamera;
    public float distance = 4.0f;
    


// 在应用程序启动时激活并设置分辨率
    void Start()
    {
        var fp = new Vector3(0f,13.0f,-1);
        
        if(fireBall == null)
        {
            GameObject fireGameObject = Instantiate(fireBallPrefab, fp, Quaternion.identity);
            fireBall = fireGameObject.GetComponent<FireBall>();
            // fireBallTargetGameObject = Instantiate(fireBallTargetPrefab, fp, Quaternion.identity);
            // fireBallTarget = fireBallTargetGameObject.GetComponent<FireBall>();
        }
        shooting = true;
        
        fireBall.transform.position = fp;
        // fireBallTargetGameObject.transform.position = new Vector3(0, 0, 0);
        fireBall.Shoot(new Vector3(0, 0, 50f));
#if UNITY_EDITOR
        // 编辑器模式下的代码
        Debug.Log("Running in Unity Editor");
#else
        // 非编辑器模式下的代码（即在构建的游戏中）
        int display1Width = 1920;
        int display1Height = 1080;
        
        Debug.Log("Number of displays: " + Display.displays.Length);
        
        if (Display.displays.Length > 0)
        {
            Display.displays[0].Activate();
            Display.displays[0].SetRenderingResolution(display1Width, display1Width);
        }
        
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
            Display.displays[1].SetRenderingResolution(display1Width, display1Height);
        }
#endif
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            var fp = new Vector3(0f,13.0f,-1);
        
            if(fireBall == null)
            {
                GameObject fireGameObject = Instantiate(fireBallPrefab, fp, Quaternion.identity);
                fireBall = fireGameObject.GetComponent<FireBall>();
                // fireBallTargetGameObject = Instantiate(fireBallTargetPrefab, fp, Quaternion.identity);
                // fireBallTarget = fireBallTargetGameObject.GetComponent<FireBall>();
            }
            shooting = true;
        
            fireBall.transform.position = fp;
            // fireBallTargetGameObject.transform.position = new Vector3(0, 0, 0);
            fireBall.Shoot(new Vector3(0, 0, 50f));
        }

        if (shooting) return;
        
        if (fireBallTargetGameObject)
        {
            if(fireBall == null)
                Destroy(fireBallTargetGameObject);
        }
        
        if ( rightHand.holdingHandType(1, 1.0f) )
        {
            Debug.Log("holdingHandType 1 in right");

            rightHand.previousTime = Time.time;
            if (fireBall == null || shooting == true)
                return;
            shooting = true;
            PalmNormal = rightHand.GetLeapHand().PalmNormal.ToVector3();
            PalmNormal += fireBallOffest;

            if(PalmNormal.z > 0)
                fireBall.Shoot(PalmNormal*50);

        }
        
        if ( leftHand.holdingHandType(1, 1.0f) )
        {
            Debug.Log("holdingHandType 1 in Left");

            leftHand.previousTime = Time.time;
            if (fireBall == null || shooting == true)
                return;
            shooting = true;
            PalmNormal = leftHand.GetLeapHand().PalmNormal.ToVector3();
            PalmNormal += fireBallOffest;

            if(PalmNormal.z > 0)
                fireBall.Shoot(PalmNormal*50);

        }

        if (!shooting && fireBall != null)
        {
            if (leftHand?.GetLeapHand()?.PalmNormal.ToVector3().z > 0)
            {
                PalmPos = leftHand.GetLeapHand().PalmPosition.ToVector3();
                PalmNormal = leftHand.GetLeapHand().PalmNormal.ToVector3();
            }
            else if (rightHand?.GetLeapHand()?.PalmNormal.ToVector3().z > 0)
            {
                PalmPos = rightHand.GetLeapHand().PalmPosition.ToVector3();
                PalmNormal = rightHand.GetLeapHand().PalmNormal.ToVector3();
            }
            
            else
            {
                return;
            }
            
            // rayDirection.Normalize();
            // rayDirection *= (1/rayDirection.z );

            PalmNormal += fireBallOffest;
            Vector3 fireBallPosition = mainCamera.transform.position + PalmNormal * distance;
            
            fireBall.transform.position = fireBallPosition;
            if(PalmNormal.z > 0)
                fireBallTarget.transform.position = PalmPos + PalmNormal * (29/PalmNormal.z) ;

            // fireBallPos.x = -10.0f;
            // fireBall.transform.position = fireBallPos;
        }
        
        if (rightHand != null && rightHand.holdingHandType(2, 1.0f) )
        {
            rightHand.previousTime = Time.time;
            Debug.Log("holdingHandType in 2 righthand");

            if(fireBall == null)
            {
                PalmPos = rightHand.GetLeapHand().PalmPosition.ToVector3();
                
                GameObject fireGameObject = Instantiate(fireBallPrefab, PalmPos, Quaternion.identity);
                fireBall = fireGameObject.GetComponent<FireBall>();
                fireBallTargetGameObject = Instantiate(fireBallTargetPrefab, PalmPos, Quaternion.identity);
                fireBallTarget = fireBallTargetGameObject.GetComponent<FireBall>();
                shooting = false;
            }

        }
        
        if (leftHand != null && leftHand.holdingHandType(2, 1.0f) )
        {
            leftHand.previousTime = Time.time;
            Debug.Log("holdingHandType in 2 lefthand");

            if(fireBall == null)
            {
                PalmPos = leftHand.GetLeapHand().PalmPosition.ToVector3();
                
                GameObject fireGameObject = Instantiate(fireBallPrefab, PalmPos, Quaternion.identity);
                fireBall = fireGameObject.GetComponent<FireBall>();
                fireBallTargetGameObject = Instantiate(fireBallTargetPrefab, PalmPos, Quaternion.identity);
                fireBallTarget = fireBallTargetGameObject.GetComponent<FireBall>();
                shooting = false;
            }
            
        }



        

        // if (Input.GetMouseButton(0))
        // {
        //     if (fireBall == null || shooting == true)
        //         return;
        //     Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //     RaycastHit hit;
        //
        //     if (Physics.Raycast(ray, out hit))
        //     {
        //         shooting = true;
        //         Vector3 hitPosition = hit.point;
        //         Vector3 dir = hitPosition - new Vector3(-10f, 20f, 0f);
        //         dir = dir.normalized * 30f;
        //         // dir = new Vector3(10f, 0f, 0f);
        //         fireBall.Shoot(dir);
        //     }
        // }
    }

    public void SetShooting(bool state)
    {
        if (shooting != state)
        {
            shooting = state;
        }
    }
}
