using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FireBall : MonoBehaviour
{
    public GameObject explosion;
    public float StatusLerpTime = 1f;
    public float BreakForce = 5f;

    private ParticleSystem psBall, psSpark, psFire, psSmoke, psDark;
    private Rigidbody rb;

    private float shootTime;

    public ExampleFireBallController FireBallController;

    private void Awake()
    {
        psBall = GetComponent<ParticleSystem>();
        rb = GetComponent<Rigidbody>();
        psSpark = transform.Find("Sparks").GetComponent<ParticleSystem>();
        psFire = transform.Find("Fire").GetComponent<ParticleSystem>();
        psSmoke = transform.Find("Smoke").GetComponent<ParticleSystem>();
        psDark = transform.Find("FireDark").GetComponent<ParticleSystem>();
    }



    private void SetEffect(float lerpRate)
    {
        setParticleSystem(psBall,   new Vector2(4.5f, 4.5f),new Vector2(0f, 0f),     new Vector2(0f, 0f),    new Vector2(0f, 0f),    lerpRate);
        setParticleSystem(psSpark,  new Vector2(100f, 0f),  new Vector2(1f, 2f),     new Vector2(2f, 0f),    new Vector2(2f, 0f),    lerpRate);
        setParticleSystem(psFire,   new Vector2(10f, 0f),   new Vector2(0.5f, 1f),   new Vector2(1.5f, 0f),  new Vector2(2f, 0f),    lerpRate);
        setParticleSystem(psSmoke,  new Vector2(10f, 0f),   new Vector2(0.5f, 1f),   new Vector2(1.5f, 0f),  new Vector2(2f, 0f),    lerpRate);
        setParticleSystem(psDark,   new Vector2(20f, 0f),   new Vector2(0.5f, 1f),   new Vector2(1f, 0f),    new Vector2(1.5f, 0f),  lerpRate);
    }

    private void setParticleSystem(ParticleSystem ps, Vector2 rateTime, Vector2 rateDist, Vector2 velocityY, Vector2 forceY, float lerpRate)
    {
        var em = ps.emission;
        var V = ps.velocityOverLifetime;
        var F = ps.forceOverLifetime;

        em.rateOverTime = Mathf.Lerp(rateTime.x, rateTime.y, lerpRate);
        em.rateOverDistance = Mathf.Lerp(rateDist.x, rateDist.y, lerpRate);
        V.y = Mathf.Lerp(velocityY.x, velocityY.y, lerpRate);
        F.y = Mathf.Lerp(forceY.x, forceY.y, lerpRate);
    }

    private void Start()
    {
        SetEffect(0);
        //StartCoroutine(Test());
    }

    IEnumerator Test()
    {
        yield return new WaitForSeconds(2f);
        Shoot(new Vector3(100f, 0f, 0f));
        yield return new WaitForSeconds(2f);
        GameObject go = Instantiate(explosion, this.transform.position, Quaternion.identity);
        Destroy(this.gameObject);
        yield return null;
    }
    public void Shoot(Vector3 vel)
    {
        rb.velocity = vel;
        shootTime = Time.time;
        StartCoroutine(StatusTransition());
    }
    IEnumerator StatusTransition()
    {
        while (true)
        {
            float passTime = Time.time - shootTime;
            SetEffect(passTime / StatusLerpTime);
            if (passTime >= StatusLerpTime)
                break;
            yield return null;
        }
        FireBallController?.SetShooting(false);
        yield return null;
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.gameObject.tag == "BreakableWall")
        {
            collision.collider.gameObject.GetComponent<BreakableWall>().BreakWall(collision.collider.ClosestPoint(this.transform.position), BreakForce);
            GameObject go = Instantiate(explosion, this.transform.position, Quaternion.identity);
            Destroy(this.gameObject);
        }


    }
}
