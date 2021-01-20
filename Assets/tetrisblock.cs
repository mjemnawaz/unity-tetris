using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class tetrisblock : MonoBehaviour
{
    public Vector3 rotationPnt;
    public Vector3[] kickstyle = new Vector3[40];
    private int position = 0;
    private float downIntervalStartingPoint; 
    public float downIntervalLength = 0.8f; // interval between tetris block moving down 
    public static int height = 21;
    public static int width = 11;
    public static Transform[,] board = new Transform[width, height];

    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform children in transform)
        {
            int x = Mathf.RoundToInt(children.transform.position.x);
            int y = Mathf.RoundToInt(children.transform.position.y);
            if (((x>=0 && x < width)||(y>=0 && y < height))&&(board[x, y] != null)) this.enabled = false;
            // TODO: signal game over
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Move(-1, 0, 0);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Move(1, 0, 0);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.X))
        {
            Rotate(-90, rotationPnt); //rotate right
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            Rotate(90, rotationPnt); //rotate left
        }
        if (Time.time - downIntervalStartingPoint > (Input.GetKeyDown(KeyCode.DownArrow) ? downIntervalLength/10 : downIntervalLength))
        {
            Move(0, -1, 0);
            downIntervalStartingPoint = Time.time;
        }
    }

    // these will ensure that the tetris block cannot make any invalid moves or rotations [beyond bounds]
    void Move(int x, int y, int z)
    {
        bool valid = true;
        foreach (Transform children in transform)
        {
            int nx = x + Mathf.RoundToInt(children.transform.position.x);
            int ny = y + Mathf.RoundToInt(children.transform.position.y);
            if (((nx < 0 || nx >= width) || (ny < 0 || ny >= height)) || ((board[nx,ny] != null) && (board[nx,ny] != children))) valid = false;
        }
        if (valid) transform.position += new Vector3(x, y, z);
        else if (x == 0 && y == -1) //if the tetris block reaches the ground or resting place
        {
            AddToGrid();
            this.enabled = false;
            FindObjectOfType<spawntetrisblock>().Spawn(); //spawn a new block
        }
    }
    void Rotate(int degree, Vector3 rotationPnt)
    {
        int newPosition = ((degree==-90) ? position+1 : position-1);
        int currKick = 10 * Math.Min(position, newPosition) + (position > newPosition ? 5 : 0);
        newPosition = newPosition % 4;
        Debug.Log("Shifting from position " + position + " to " + newPosition);
        Debug.Log("Starting at kick #" + currKick);
        transform.RotateAround(transform.TransformPoint(rotationPnt), new Vector3(0, 0, 1), degree);
        bool valid = true; // the block should be in bounds on all four sides
        for (int i = currKick; i < currKick+5; i++)
        {
            Debug.Log("Trying " + i + " kick, ...");
            valid = true;
            foreach (Transform children in transform)
            {
                int x = Mathf.RoundToInt(kickstyle[i][0] + children.transform.position.x);
                int y = Mathf.RoundToInt(kickstyle[i][1] + children.transform.position.y);
                Debug.Log("Coordinates " + x + ", " + y);
                if (((x < 0 || x >= width) || (y < 0 || y >= height)) || ((board[x, y] != null) && (board[x, y] != children))) valid = false;
            }
            if (valid)
            {
                Debug.Log("Success!");
                transform.position += kickstyle[i];
                position = newPosition;
                break;
            }
        }
        if (!valid)
        {
            Debug.Log("Failed, no rotation");
            transform.RotateAround(transform.TransformPoint(rotationPnt), new Vector3(0, 0, 1), degree * -1);
        }
    }
    void AddToGrid()
    {
        foreach (Transform children in transform)
        {
            int x = Mathf.RoundToInt(children.transform.position.x);
            int y = Mathf.RoundToInt(children.transform.position.y);
            board[x, y] = children;
        }
        CheckForLines();
    }

    void CheckForLines()
    {
        for (int y = 0; y < height; y++)
        {
            bool line = true;
            for (int x = 0; x < width; x++) if (board[x,y] == null) line = false;
            if (line) //if there is a complete line to delete
            {
                FindObjectOfType<score>().UpdateScore();
                Debug.Log("Found a line to destroy at height = " + y);
                for (int x = 0; x < width; x++)
                {
                    Destroy(board[x,y].gameObject);
                    board[x, y] = null;
                }
                RowDown(y);
            }
        }
    }

    void RowDown(int line)
    {
        int[,] mapBoard = new int[width, height]; //maps out the location of the different chunks
        List<Tuple<int,int>> mapList = new List<Tuple<int,int>>(); //the range of the diff chunks
        List<int> moveDown = new List<int>(); //stores how far down to move thes different chunks
        Stack<Tuple<int,int>> stack = new Stack<Tuple<int,int>>();
        for (int y = line+1; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (board[x,y] != null &&  mapBoard[x,y]==0)
                {
                    Debug.Log("Found chunk " + (mapList.Count+1) +" at " + x + ", " + y);
                    stack.Push(Tuple.Create(x,y));
                    Debug.Log("Adding " + x + ", " + y);
                    int xmin = x, xmax = x;
                    while (stack.Count != 0)
                    {
                        Tuple<int,int> curr = stack.Pop();
                        int currX = curr.Item1, currY = curr.Item2;
                        if ((currX-1>=0&&board[currX-1,currY]!=null)&&(!stack.Contains(Tuple.Create(currX-1,currY))&&mapBoard[currX-1,currY]==0))
                        {
                            stack.Push(Tuple.Create(currX - 1, currY));
                            Debug.Log("Adding " + (currX-1) + ", " + currY);
                        }
                        if ((currY-1>=(line+1)&&board[currX,currY-1]!=null)&&(!stack.Contains(Tuple.Create(currX,currY-1))&& mapBoard[currX,currY-1]==0))
                        {
                            stack.Push(Tuple.Create(currX, currY - 1));
                            Debug.Log("Adding " + currX + ", " + (currY-1));
                        }
                        if ((currX+1<width&&board[currX+1,currY]!=null)&&(!stack.Contains(Tuple.Create(currX+1,currY))&&mapBoard[currX+1,currY]==0))
                        {
                            stack.Push(Tuple.Create(currX + 1, currY));
                            Debug.Log("Adding " + (currX + 1) + ", " + currY);
                        }
                        if((currY+1<height&&board[currX,currY+1]!=null)&&(!stack.Contains(Tuple.Create(currX,currY+1))&&mapBoard[currX,currY+1]==0)) 
                        {     
                            stack.Push(Tuple.Create(currX, currY + 1));
                            Debug.Log("Adding " + currX + ", " + (currY + 1));
                        }
                        mapBoard[currX, currY] = mapList.Count + 1;
                        if (currX < xmin) xmin = currX;
                        if (currX > xmax) xmax = currX;
                    }
                    mapList.Add(Tuple.Create(xmin,xmax));
                    moveDown.Add(0);
                }
                /*if (board[x, y] != null)
                {
                    board[x, y - 1] = board[x, y];
                    board[x, y] = null;
                    board[x, y - 1].transform.position += new Vector3(0, -1, 0);
                }*/
            }
        }
        Debug.Log("Finished finding all the chunks! " + moveDown.Count);
        for (int i = 0; i < moveDown.Count; i++)
        {
            int xmin = mapList[i].Item1, xmax = mapList[i].Item2;
            Debug.Log("how far down for chunk " + i + ", which is between " + xmin +" and " +xmax+ "?");
            for (int y = line; y>=0; y--)
            {
                bool clear = true;
                for (int x = xmin; x <= xmax; x++)
                {
                    if (board[x, y] != null) clear = false;
                }
                if (clear==false) break;
                moveDown[i]++;
                Debug.Log(moveDown[i] - 1 + " to " + moveDown[i]);
            }
            Debug.Log("move chunk " + i + " down " + moveDown[i] + " spaces");
        }
        Debug.Log("now to move down");
        for (int y = line; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (mapBoard[x, y] != 0)
                {
                    int down = moveDown[mapBoard[x, y]-1];
                    board[x, y - down] = board[x, y];
                    board[x, y] = null;
                    board[x, y - down].transform.position += new Vector3(0, -1*down, 0);
                }
            }
        }
    }
}
