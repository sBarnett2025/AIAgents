using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;


public enum AgentState
{
    STANDBY,
    PATHFINDING,
    MOVING,
    EXPLORING
}

public class Agent : MonoBehaviour
{
    [SerializeField] Maze maze;
    Vector2Int firstStart, finalEnd;
    Vector2Int start, end;

    AgentState state;
    [SerializeField] float timeDelay;

    // exploring options
    Stack<Vector2Int> optionsFound = new Stack<Vector2Int>();


    // Start is called before the first frame update
    void Start()
    {
        firstStart = new Vector2Int(0, 0);
        finalEnd = new Vector2Int(maze.sideSize, maze.sideSize);
        state = AgentState.EXPLORING;
    }

    // Update is called once per frame
    void Update()
    {
        if (state == AgentState.STANDBY)
        {

        }
        else if (state == AgentState.PATHFINDING)
        {
            StartCoroutine(FindPath(timeDelay));
        }
        else if (state == AgentState.MOVING)
        {
            StartCoroutine(MoveOnPath(timeDelay));
        }
        else if (state == AgentState.EXPLORING)
        {
            StartCoroutine(Explore(timeDelay));
        }
    }

    IEnumerator FindPath(float delay)
    {
        state = AgentState.STANDBY;

        ResetVisited();

        Vector2Int curr = new Vector2Int((int)transform.position.x, (int)transform.position.y);
        Vector2Int end = optionsFound.Peek();

        // pathfinding
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, float> costSoFar = new Dictionary<Vector2Int, float>();

        PriorityQueue frontier = new PriorityQueue();
        frontier.Enqueue(new KeyValuePair<float, Vector2Int>(0f, curr));

        maze.tiles[maze.sideSize * end.y + end.x].state = TileState.TRAVELED;
        List<Vector2Int> neigh;

        while (frontier.list.Count > 0)
        {
            yield return new WaitForSeconds(delay);

            KeyValuePair<float, Vector2Int> c = frontier.Dequeue();

            if (c.Value == end)
            {
                break;
            }

            // create path
            foreach (Vector2Int next in GetNeighbors(maze.visited, c.Value))
            {
                float newCost = costSoFar[c.Value] + GetDistance(c.Value, next);
                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    float prio = newCost + GetDistance(next, end);
                    frontier.Enqueue(new KeyValuePair<float, Vector2Int>(-prio, next));
                }
            }




            neigh = GetNeighbors(maze.visited, curr);









            
        }

        state = AgentState.MOVING;

    }

    IEnumerator MoveOnPath(float delay)
    {
        state = AgentState.STANDBY;

        while (transform.position.x != end.x && transform.position.y != end.y)
        {
            yield return new WaitForSeconds(delay);
        }


        //state = AgentState.EXPLORING;
    }

    IEnumerator Explore(float delay)
    {
        ResetVisited();
        state = AgentState.STANDBY;
        List<Vector2Int> neigh = new List<Vector2Int>();
        //Dictionary<Vector2Int, bool> visited = new Dictionary<Vector2Int, bool>();
        Vector2Int from = new Vector2Int(0, 0);

        Vector2Int v = new Vector2Int((int)transform.position.x, (int)transform.position.y);
        neigh = GetNeighbors(maze.visited, v);
        maze.visited[v] = true;
        while (neigh.Count > 0)
        {
            yield return new WaitForSeconds(delay);

            // add to a stack to go back to

            Vector2Int chosen = from;
            int randomNum = Random.Range(0, 100);
            int index = randomNum % neigh.Count;

            chosen = neigh[index];

            foreach (Vector2Int n in neigh)
            {
                if (n != chosen)
                {
                    optionsFound.Push(n);
                }
            }

            maze.visited[chosen] = true;
            from = new Vector2Int((int)transform.position.x, (int)transform.position.y);
            Debug.Log(from);
            
            transform.position = new Vector3(chosen.x, chosen.y, transform.position.z);
            maze.tiles[maze.sideSize * chosen.y + chosen.x].state = TileState.VISITED;

            neigh.Clear();
            neigh = GetNeighbors(maze.visited, chosen);
        }

        state = AgentState.PATHFINDING;
        Debug.Log("Done Exploring");
    }

    void ResetVisited()
    {
        for (int y = 0; y < maze.sideSize; y++)
        {
            for (int x = 0; x < maze.sideSize; x++)
            {
                Vector2Int p = new Vector2Int(x, y);
                maze.visited[p] = false;
            }
        }
    }

    List<Vector2Int> GetNeighbors(Dictionary<Vector2Int, bool> visited, Vector2Int point)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        List<Vector2Int> canidates = new List<Vector2Int>
        {
            new Vector2Int(point.x, point.y + 1),
            new Vector2Int(point.x + 1, point.y),
            new Vector2Int(point.x, point.y - 1),
            new Vector2Int(point.x - 1, point.y)
        };
        //Debug.Log("-------------------");
        foreach (Vector2Int p in canidates)
        {
            if (point.y + 1 == p.y) // up
            {
                if (!PointIsValid(p))
                {
                    continue;
                }
                if (maze.horiWallsUp[maze.sideSize * p.y + p.x].active == true)
                {
                    continue;
                }
                if (visited[p] == true)
                {
                    continue;
                }
                //Debug.Log("up");
                neighbors.Add(p);
            }
            else if (point.y - 1 == p.y) // down
            {
                if (!PointIsValid(p))
                {
                    continue;
                }
                if (maze.horiWallsUp[maze.sideSize * (p.y + 1) + p.x].active == true)
                {
                    continue;
                }
                if (visited[p] == true)
                {
                    continue;
                }
                //Debug.Log("down");
                neighbors.Add(p);
            }
            else if (point.x + 1 == p.x) // right
            {
                if (!PointIsValid(p))
                {
                    continue;
                }
                if (maze.vertWallsRight[(maze.sideSize + 1) * p.y + p.x].active == true)
                {
                    continue;
                }
                if (visited[p] == true)
                {
                    continue;
                }
                //Debug.Log("right");
                neighbors.Add(p);
            }
            else if (point.x - 1 == p.x) //left
            {
                //Debug.Log(p);
                if (!PointIsValid(p))
                {
                    continue;
                }
                if (maze.vertWallsRight[(maze.sideSize + 1) * p.y + (p.x + 1)].active == true)
                {
                    continue;
                }
                if (visited[p] == true)
                {
                    continue;
                }
                //Debug.Log("left");
                neighbors.Add(p);
            }
            else
            {
                Debug.Log("wrrpr");
            }


        }

        return neighbors;
    }


    float GetDistance(Vector2Int start, Vector2Int end)
    {
        float distance = Mathf.Abs(start.x - end.x) + Mathf.Abs(start.y - end.y);
        return distance;
    }

    bool PointIsValid(Vector2Int po)
    {
        if (po.x < 0 || po.y < 0 || po.x > maze.sideSize - 1 || po.y > maze.sideSize - 1)
        {
            return false;
        }
        return true;
    }

}
