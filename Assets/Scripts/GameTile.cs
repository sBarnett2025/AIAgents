using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum TileState
{
    NONE,
    VISITED,
    FLOOR,
    START,
    END,
    TRAVELED
}

public class GameTile : MonoBehaviour
{
    public TileState state;

    [SerializeField] Material[] mats;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case TileState.NONE:
                gameObject.GetComponent<Renderer>().material = mats[0]; // white
                break;
            case TileState.VISITED:
                gameObject.GetComponent<Renderer>().material = mats[1]; // purple
                break;
            case TileState.FLOOR:
                gameObject.GetComponent<Renderer>().material = mats[2]; // blue
                break;
            case TileState.START:
                gameObject.GetComponent<Renderer>().material = mats[3]; // green
                break;
            case TileState.END:
                gameObject.GetComponent<Renderer>().material = mats[4]; // red
                break;
            case TileState.TRAVELED:
                gameObject.GetComponent<Renderer>().material = mats[5]; // yellow
                break;
            default:
                //gameObject.GetComponent<Renderer>().material = mats[0]; // white
                break;
        }
    }
}
