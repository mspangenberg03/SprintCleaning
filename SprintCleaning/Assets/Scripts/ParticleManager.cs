using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    [SerializeField,Tooltip("How long the particles last")]
    private float _particleLifespan;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<ParticleSystem>().startLifetime = _particleLifespan;
        StartCoroutine(Lifetime());
    }
    IEnumerator Lifetime()
    {
        yield return new WaitForSeconds(_particleLifespan);
        Destroy(gameObject);
    }
}
