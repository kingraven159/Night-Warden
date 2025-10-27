using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour
{
    public float ghostDelay;
    public GameObject ghost;
    public bool makeGhost;
    private float ghostDelayTime;

    public void GhostSpawn()
    {
        this.ghostDelayTime = this.ghostDelay;

        if (makeGhost)
        {
            if (ghostDelayTime > 0)
            {
                ghostDelayTime -= Time.deltaTime;
            }
            else
            {
                //¿‹ªÛ
                GameObject currentGhost = Instantiate(this.ghost, this.transform.position, this.transform.rotation);
                Sprite sr = GetComponent<SpriteRenderer>().sprite;
                currentGhost.GetComponent<SpriteRenderer>().sprite = sr;
                currentGhost = Instantiate(ghost, transform.position, transform.rotation);
                ghostDelayTime = ghostDelay;
                Destroy(currentGhost, 1f);
            }
        }
        makeGhost = false;
    }
}
