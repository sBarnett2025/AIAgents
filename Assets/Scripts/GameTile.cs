using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum TileState
{
    NONE,
    VISITED,
    FLOOR,
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
                gameObject.GetComponent<Renderer>().material = mats[2]; // purple
                break;
            case TileState.FLOOR:
                gameObject.GetComponent<Renderer>().material = mats[1]; // blue
                break;
            default:
                //gameObject.GetComponent<Renderer>().material = mats[0]; // white
                break;
        }
    }
}
