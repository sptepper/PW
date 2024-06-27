using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cam : MonoBehaviour
{

    public const float ROT_SPD = 3f;
    // Update is called once per frame
    void Update()
    {
        transform.Rotate(transform.forward, Time.deltaTime * ROT_SPD);
    }
}
