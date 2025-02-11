using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tutorial815
{
    public class WindField : MonoBehaviour
    {
        Camera viewer;
        public float offsetY;
        public float rotation;

        [Range(0, 20)]
        public float density = 8f;

        ParticleSystem ps;

        // Start is called before the first frame update
        void Start()
        {
            viewer = Camera.main;
            ps = GetComponent<ParticleSystem>();
        }

        // Update is called once per frame
        void LateUpdate()
        {
            transform.position = viewer.transform.position + new Vector3(0, offsetY, 0);
            transform.rotation = Quaternion.Euler(0, rotation, 0);
            
            var emissionModule = ps.emission;
            emissionModule.rateOverTime = density;
        }
    }
}

