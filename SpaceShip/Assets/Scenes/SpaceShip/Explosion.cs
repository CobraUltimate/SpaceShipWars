using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scenes.Common;

public class Explosion : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DestroyExplosion()
    {
        GameObject.Destroy(this.gameObject);
    }

}
