using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteStaticOrderCtrl : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    // Start is called before the first frame update
    void Start()
    {
        if(spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        SetOrder();
    }

    private void Reset()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetOrder();
    }

    [ContextMenu("重设渲染层级")]
    public void SetOrder()
    {
        if (spriteRenderer != null)
        {
            Vector3 position = transform.position;
            if(spriteRenderer.spriteSortPoint == SpriteSortPoint.Pivot)
            {
                //100像素比1米，所以要乘以0.01
                position = spriteRenderer.transform.TransformPoint(-spriteRenderer.sprite.pivot*0.01f);
            }
            spriteRenderer.sortingOrder = SpriteRenderOrderUtility.GetRenderOrder(position);
        }
    }

}
