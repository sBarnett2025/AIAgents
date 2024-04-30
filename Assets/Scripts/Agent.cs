using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
    public Vector2Int cameFromExplore;

    // pathfinding
    Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
    Dictionary<Vector2Int, float> costSoFar = new Dictionary<Vector2Int, float>();

    public Stack<Vector2Int> path = new Stack<Vector2Int>();


    // Start is called before the first frame update
    void Start()
    {
        firstStart = maze.playerStart;
        transform.position = new Vector3(firstStart.x, firstStart.y, transform.position.z);
        finalEnd = maze.playerEnd;
        state = AgentState.EXPLORING;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(optionsFound.Count);
        if (state == AgentState.STANDBY)
        {

        }
        else if (state == AgentState.PATHFINDING)
        {
            StartCoroutine(FindPath());
        }
        else if (state == AgentState.MOVING)
        {
            StartCoroutine(MoveOnPath());
        }
        else if (state == AgentState.EXPLORING)
        {
            StartCoroutine(Explore());
        }
    }

    IEnumerator FindPath()
    {
        Debug.Log("start pathfinding");
        state = AgentState.STANDBY;

        ResetVisited();
        cameFrom.Clear();
        costSoFar.Clear();

        Vector2Int start = new Vector2Int((int)transform.position.x, (int)transform.position.y);
        Vector2Int curr = start;
        Vector2Int end = optionsFound.Pop();

        PriorityQueue frontier = new PriorityQueue();
        costSoFar[curr] = 0;
        frontier.Enqueue(new KeyValuePair<float, Vector2Int>(0f, curr));

        maze.tiles[maze.sideSize * end.y + end.x].state = TileState.OPTION_FOUND;

        while (frontier.list.Count > 0)
        {
            //Debug.Log(frontier.list.Count);

            KeyValuePair<float, Vector2Int> c = frontier.Dequeue();

            if (c.Value == end)
            {
                Debug.Log("end found----------------------------");
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
                    cameFrom[next] = c.Value;
                }
            }
        }

        path.Clear();
        curr = end;
        while (curr != start)
        {
            Debug.Log(curr);
            path.Push(curr);
            //maze.tiles[maze.sideSize * curr.y + curr.x].state = TileState.OPTION_FOUND;
            //Debug.Log(cameFrom[curr]);
            curr = cameFrom[curr];
            //yield return new WaitForSeconds(delay);
        }
        //path.Push(start);

        yield return new WaitForSeconds(0f);
        state = AgentState.MOVING;
        Debug.Log("done pathfinding");
    }

    IEnumerator MoveOnPath()
    {
        Debug.Log("Start Move");
        state = AgentState.STANDBY;

        while (path.Count > 0)
        {
            //Debug.Log(path.Peek());
            Vector2Int curr = path.Pop();

            transform.position = new Vector3(curr.x, curr.y, transform.position.z);
            maze.tiles[maze.sideSize * curr.y + curr.x].state = TileState.VISITED;

            if (path.Count != 0)
            {
                cameFromExplore = curr;
            }

            yield return new WaitForSeconds(timeDelay);
        }
        path.Clear();

        state = AgentState.EXPLORING;
        Debug.Log("Done Move");
    }

    IEnumerator Explore()
    {
        Debug.Log("Start Exploring");
        ResetVisited();
        state = AgentState.STANDBY;
        List<Vector2Int> neigh = new List<Vector2Int>();
        //Dictionary<Vector2Int, bool> visited = new Dictionary<Vector2Int, bool>();
        
        Vector2Int v = new Vector2Int((int)transform.position.x, (int)transform.position.y);
        //Vector2Int from = cameFrom[v];

        neigh = GetNeighbors(maze.visited, v);
        maze.visited[cameFromExplore] = true;
        maze.visited[v] = true;

        bool end = false;

        while (neigh.Count > 0)
        {
            Vector2Int chosen = new Vector2Int(0,0);
            

            foreach (Vector2Int n in neigh)
            {
                if (maze.tiles[maze.sideSize * n.y + n.x].state == TileState.END)
                {
                    chosen = n;
                    transform.position = new Vector3(chosen.x, chosen.y, transform.position.z);
                    end = true;
                    break;
                }

            }

            //Vector2Int chosen = from;
            if (!end)
            {
                int randomNum = Random.Range(0, 100);
                int index = randomNum % neigh.Count;

                chosen = neigh[index];

                Debug.Log(chosen);
            }

            foreach (Vector2Int n in neigh)
            {
                if (n != chosen && maze.tiles[maze.sideSize * n.y + n.x].state != TileState.VISITED)
                {
                    optionsFound.Push(n);
                }
            }

            maze.visited[chosen] = true;
            //from = new Vector2Int((int)transform.position.x, (int)transform.position.y);
            //Debug.Log(from);
            
            transform.position = new Vector3(chosen.x, chosen.y, transform.position.z);
            maze.tiles[maze.sideSize * chosen.y + chosen.x].state = TileState.VISITED;

            neigh.Clear();
            neigh = GetNeighbors(maze.visited, chosen);

            yield return new WaitForSeconds(timeDelay);
        }

        if (!end)
        {
            state = AgentState.PATHFINDING;
        }
        
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
            if (p.x == cameFromExplore.x && p.y == cameFromExplore.y)
            {
                continue;
            }


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
