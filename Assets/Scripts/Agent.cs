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
    Maze maze;
    Vector2Int firstStart, finalEnd;
    Vector2Int start, end;

    AgentState state;
    [SerializeField] float timeDelay;

    // pathfinding
    Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
    Dictionary<Vector2Int, float> costSoFar = new Dictionary<Vector2Int, float>();



    // Start is called before the first frame update
    void Start()
    {
        firstStart = new Vector2Int(0, 0);
        finalEnd = new Vector2Int(maze.sideSize, maze.sideSize);
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

        }
    }

    IEnumerator FindPath(float delay)
    {
        state = AgentState.STANDBY;

        while (true)
        {
            yield return new WaitForSeconds(delay);
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


        state = AgentState.EXPLORING;
    }

    IEnumerator Explore(float delay)
    {
        state = AgentState.STANDBY;
        List<Vector2Int> neigh = new List<Vector2Int>();
        List<Vector2Int> visited = new List<Vector2Int>();

        neigh = GetNeighbors(new Vector2Int((int)transform.position.x, (int)transform.position.y));

        while (neigh.Count > 0)
        {
            yield return new WaitForSeconds(delay);

            // add to a stack to go back to

            Vector2Int chosen;
            int randomNum = Random.Range(0, 100);
            int index = randomNum % neigh.Count;

            chosen = neigh[index];
            transform.position = new Vector3(chosen.x, chosen.y, transform.position.z);
            neigh.Clear();

            neigh = GetNeighbors(chosen);
        }

        state = AgentState.PATHFINDING;
        Debug.Log("Done Exploring");
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
            if (point.y + 1 == p.y)
            {
                if (maze.horiWallsUp[maze.sideSize * (p.y + 1) + p.x].active == true)
                {
                    continue;
                }
                if (!PointIsValid(p))
                {
                    continue;
                }
                neighbors.Add(p);
            }
            else if (point.y - 1 == p.y)
            {
                if (maze.horiWallsUp[maze.sideSize * p.y + p.x].active == true)
                {
                    continue;
                }
                if (!PointIsValid(p))
                {
                    continue;
                }
                neighbors.Add(p);
            }
            else if (point.x + 1 == p.x)
            {
                if (maze.vertWallsRight[(maze.sideSize + 1) * p.y + (p.x + 1)].active == true)
                {
                    continue;
                }
                if (!PointIsValid(p))
                {
                    continue;
                }
                neighbors.Add(p);
            }
            else if (point.x - 1 == p.x)
            {
                if (maze.vertWallsRight[(maze.sideSize + 1) * p.y + p.x].active == true)
                {
                    continue;
                }
                if (!PointIsValid(p))
                {
                    continue;
                }
                neighbors.Add(p);
            }
            else
            {
                Debug.Log("wrrpr");
            }


        }








        return neighbors;
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
