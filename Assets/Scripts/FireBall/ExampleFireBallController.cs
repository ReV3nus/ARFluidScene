using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ExampleFireBallController : MonoBehaviour
{
    public GameObject fireBallPrefab;
    private FireBall fireBall;
    private bool shooting = false;

    private void Start()
    {
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (fireBall != null)
            {
                Destroy(fireBall.gameObject);
                fireBall = null;
            }

            GameObject go = Instantiate(fireBallPrefab, new Vector3(-10f, 20f, 0f), Quaternion.identity);
            fireBall = go.GetComponent<FireBall>();
            shooting = false;
        }
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
                dir = dir.normalized * 10f;
                fireBall.Shoot(dir);
            }
        }
    }
}
