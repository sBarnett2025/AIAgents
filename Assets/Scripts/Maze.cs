using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;

public class Maze : MonoBehaviour
{
    public int sideSize;

    public List<GameTile> tiles = new List<GameTile>();
    [SerializeField] GameObject tilePrefab;

    // player
    [SerializeField] Agent agent;

    // maze walls
    [SerializeField] GameObject wallPrefab;
    public List<GameObject> horiWallsUp = new List<GameObject>();
    List<GameObject> horiWallsDown = new List<GameObject>();
    public List<GameObject> vertWallsRight = new List<GameObject>();
    List<GameObject> vertWallsLeft = new List<GameObject>();

    // maze gen
    Stack<Vector2Int> stack = new Stack<Vector2Int>();
    public Dictionary<Vector2Int, bool> visited = new Dictionary<Vector2Int, bool>();

    // Start is called before the first frame update
    void Start()
    {

        // hori
        for (int y = 0; y < sideSize + 1; y++)
        {
            for (int x = 0; x < sideSize; x++)
            {
                // up walls
                GameObject up = Instantiate(wallPrefab, new Vector3(x, y - 0.5f, -0.5f), Quaternion.identity);
                //up.transform.Rotate(new Vector3(0f, 0f, 0f));
                horiWallsUp.Add(up);

                // down walls
                GameObject down = Instantiate(wallPrefab, new Vector3(x, y - 0.5f, -0.5f), Quaternion.identity);
                down.transform.Rotate(new Vector3(0f, 0f, 180f));
                horiWallsDown.Add(down);
            }
            
        }

        // vert
        for (int y = 0; y < sideSize; y++)
        {
            for (int x = 0; x < sideSize+1; x++)
            {
                // right walls
                GameObject right = Instantiate(wallPrefab, new Vector3(x - 0.5f, y, -0.5f), Quaternion.identity);
                right.transform.Rotate(new Vector3(0f, 0f, 90f));
                vertWallsRight.Add(right);

                // left walls
                GameObject left = Instantiate(wallPrefab, new Vector3(x - 0.5f, y, -0.5f), Quaternion.identity);
                left.transform.Rotate(new Vector3(0f, 0f, -90f));
                vertWallsLeft.Add(left);
            }
        }

        GenerateMaze();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GenerateMaze()
    {
        for (int y = 0; y < sideSize; y++)
        {
            for (int x = 0; x < sideSize; x++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                GameTile tile = Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity).GetComponent<GameTile>();
                tiles.Add(tile);
                
                visited.Add(pos, false);
                //Debug.Log(visited[pos]);
                tile.state = TileState.NONE;
                tile.gameObject.transform.SetParent(gameObject.transform);
            }
        }
        ClearMaze();

        Vector2Int start = new Vector2Int(0, 0);
        StartCoroutine(ReverseBackTracker(start));
    }

    IEnumerator ReverseBackTracker(Vector2Int start)
    {
        stack.Push(start);

        while (stack.Count > 0)
        {
            Vector2Int curr = stack.Peek();
            visited[curr] = true;
            //tiles[(int)curr.y * mazeHeight + (int)curr.x].state = TileState.VISITED;
            List<Vector2Int> neighbors = GetNeighbors(curr);

            if (neighbors.Count == 0)
            {
                //stack
                stack.Pop();
                tiles[(int)curr.y * sideSize + (int)curr.x].state = TileState.FLOOR;
            }
            else
            {
                Vector2Int chosen;
                if (neighbors.Count == 1)
                {
                    chosen = neighbors[0];
                }
                else
                {
                    int randomNum = Random.Range(0, 100);
                    int index = randomNum % neighbors.Count;

                    chosen = neighbors[index];
                }

                int limit = sideSize + 1;
                int half = limit / 2;
                Debug.Log(curr);
                if (chosen.y == curr.y - 1) // north
                {
                    horiWallsUp[sideSize * ((int)curr.y) + (int)curr.x].SetActive(false);
                    horiWallsDown[sideSize * ((int)curr.y) + (int)curr.x].SetActive(false);
                }
                else if (chosen.x == curr.x + 1) // east
                {
                    vertWallsRight[(sideSize + 1) * (int)curr.y + ((int)curr.x + 1)].SetActive(false);
                    vertWallsLeft[(sideSize + 1) * (int)curr.y + ((int)curr.x + 1)].SetActive(false);
                }
                else if (chosen.y == curr.y + 1) // south
                {
                    horiWallsUp[sideSize * ((int)curr.y+1) + ((int)curr.x)].SetActive(false);
                    horiWallsDown[sideSize * ((int)curr.y+1) + ((int)curr.x)].SetActive(false);
                }
                else if (chosen.x == curr.x - 1) // west
                {
                    vertWallsRight[(sideSize + 1) * (int)curr.y + (int)curr.x].SetActive(false);
                    vertWallsLeft[(sideSize + 1) * (int)curr.y + (int)curr.x].SetActive(false);
                }
                tiles[(int)curr.y * sideSize + (int)curr.x].state = TileState.VISITED;

                stack.Push(chosen);
            }

            yield return new WaitForSeconds(0.005f);
        }

        tiles[0].state = TileState.START;
        tiles[sideSize * sideSize - 1].state = TileState.END;
        agent.gameObject.SetActive(true);
    }

    List<Vector2Int> GetNeighbors(Vector2Int point)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        List<Vector2Int> canidates = new List<Vector2Int>
        {
            new Vector2Int(point.x, point.y + 1),
            new Vector2Int(point.x + 1, point.y),
            new Vector2Int(point.x, point.y - 1),
            new Vector2Int(point.x - 1, point.y)
        };

        foreach (Vector2Int p in canidates)
        {
            if (p.x < 0 || p.x > sideSize - 1)
            {
                continue;
            }
            if (p.y < 0 || p.y > sideSize - 1)
            {
                continue;
            }
            if (visited[p] == true)
            {
                continue;
            }
            neighbors.Add(p);
        }

        return neighbors;
    }

    void ClearMaze()
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            tiles[i].state = TileState.NONE;
        }
        stack.Clear();
        for (int y = 0; y <= sideSize; y++)
        {
            for (int x = 0; x <= sideSize; x++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                visited[pos] = false;
            }
        }
    }

}
