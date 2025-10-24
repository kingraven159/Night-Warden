using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour
{
    public float ghostDelay;
    public GameObject ghost;
    public bool makeGhost;
    private GameObject currentGhost;

    private float ghostDelayTime;

    void Start()
    {
        Sprite sr = GetComponent<SpriteRenderer>().sprite;
        currentGhost.GetComponent<SpriteRenderer>().sprite = sr;
    }

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
                currentGhost = Instantiate(ghost, transform.position, transform.rotation);
                ghostDelayTime = ghostDelay;
                Destroy(currentGhost, 1f);
            }
        }
        makeGhost = false;
    }
}
