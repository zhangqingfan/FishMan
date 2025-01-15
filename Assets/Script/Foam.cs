using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Foam : MonoBehaviour
{
    int speed = 10;
    Vector3 oldPos = Vector3.zero;
    float oldSpeed = 0;
    ParticleSystem ps;


    // Start is called before the first frame update
    void Start()
    {
        oldPos = transform.position;
        ps = GetComponent<ParticleSystem>();
        if(ps != null)
        {
            var m = ps.main;
            m.loop = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        var offset = transform.position - oldPos;
        offset.y = 0;
        var speed = offset.magnitude / Time.deltaTime;
        var curSpeed = Mathf.Lerp(oldSpeed, speed, Time.deltaTime * 2);

        if (ps != null)
        {
            var e = ps.emission;
            e.rateOverTime = curSpeed;
        }

        oldSpeed = curSpeed;
        oldPos = transform.position;
    }
}
