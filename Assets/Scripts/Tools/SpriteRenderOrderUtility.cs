using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteRenderOrderUtility 
{
    
    public static int GetRenderOrder(Vector3 positon)
    {
        return Mathf.RoundToInt(-positon.z * 100);
    }

}
