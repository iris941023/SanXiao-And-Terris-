using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using TMPro;

public class IceSpawner : MonoBehaviour
{
    public GameObject IceObj;
    public const int ROW_COUNT = 18;
    public const int COLUM_COUNT = 7;
    public Transform IcePivot;
    public Transform FruitPivot;
    public Transform ScoreAnimPivot;
    public float ItemSize = 0.4f;
    public List<GameObject> FruitObjs;
    private IceBoard MyIceBoard;
    private GameObject CurrentObject;
    private GameObject TempObject;
    private Item CurrentItem;
    private Item TempItem;
    private Vector3 TA;
    private Vector3 TB;
    private List<DropAnimation> DropAnimationList;
    public TextMeshProUGUI ScoreText;
    private int score;
    float dt = 0;
    EState State;
    bool IsEliminate = false;
    public GameObject ScoreAnim;
    public enum EState
    {
        GameStart,
        Swap,
        SwapAnim,
        Eliminate,
        GameOver,
        Move,
        DropAnim,
        Click,
    }    
    void Start()
    {        
        State = EState.GameStart;
        MyIceBoard = new IceBoard();
        DropAnimationList = new List<DropAnimation>();      
        for (int x = 0; x < MyIceBoard.Board.Length; x++)
        {
            for(int y = 0; y < MyIceBoard.Board[0].Length-9; y++)//MyIceBoard.Board[0].Length
            {
                // ice item.
                GameObject IceItem = Instantiate<GameObject>(IceObj);
                IceItem.transform.SetParent(this.IcePivot);
                IceItem.transform.localPosition = new Vector3(x*ItemSize, y*ItemSize,0);
                MyIceBoard.Ices[x][y] = IceItem;

                // score anim item.
                // todo z-index.
                GameObject ScoreAnimObj = Instantiate<GameObject>(ScoreAnim);
                ScoreAnimObj.transform.SetParent(this.ScoreAnimPivot);
                ScoreAnimObj.transform.localPosition = new Vector3(x * ItemSize, y * ItemSize, 0);
                MyIceBoard.ScoreAnims[x][y] = ScoreAnimObj;

                // fruit item.
                int FruitTypeIndex = UnityEngine.Random.Range(0, FruitObjs.Count);
                GameObject FruitItem = Instantiate(FruitObjs[FruitTypeIndex]);
                FruitItem.transform.SetParent(this.FruitPivot);
                MyIceBoard.Board[x][y] = new Item(FruitItem, FruitTypeIndex, new Vector2(x, y), new Vector3(x * ItemSize, y * ItemSize, 0));
                MyIceBoard.Board[x][y].SetLocalPosition(new Vector3(x * ItemSize, y * ItemSize, 0));
                State = EState.Click;
            }           
        }
    }
    void Update()
    {
        if(State == EState.Click)
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit2D hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));//œ‡Ωª
                                                                                                                   // RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), -Vector2.up);
                if (CurrentObject == null)
                {
                    CurrentObject = hit.collider.gameObject;
                    CurrentItem = GetItem(CurrentObject);
                }
                else if (CurrentObject != hit.collider.gameObject && hit.collider.gameObject != null)
                {
                    TempObject = hit.collider.gameObject;
                    TempItem = GetItem(TempObject);
                    if (clickBound(CurrentItem, TempItem))
                    {
                        State = EState.Swap;
                    }
                }
            }
        }      
        // check if current status is swap:
        if (State == EState.Swap)
        {
            if (TempObject != null && CurrentObject != null)
            {
                BoardSwap(CurrentItem, TempItem);
                eliminate(TempItem);
                eliminate(CurrentItem);
                if (!IsEliminate)
                {
                    BoardSwap(CurrentItem, TempItem);                   
                }
                TA = CurrentItem.Display.transform.position;
                TB = TempItem.Display.transform.position;
                State = EState.SwapAnim;
            }           
        }
        if(State == EState.SwapAnim)
        {
            bool bFinished = false;
            if (IsEliminate)
            {
                if (dt <=1.0f)
                {
                    dt += Time.deltaTime;
                    dt = Mathf.Min(1.0f, dt);
                    Swap(CurrentItem, TempItem, TA, TB, dt);
                }
                if(dt == 1.0f)
                {

                    bFinished = true;
                    dt = 0;
                }
            }
            else
            {
                if(dt <= 2.0f)
                {
                    dt += Time.deltaTime;
                    dt = Mathf.Min(2.0f, dt);
                    float Newdt = -Mathf.Abs(dt - 1) + 1;
                    Swap(CurrentItem, TempItem, TA, TB, Newdt);                   
                }
                if(dt == 2.0f)
                {
                    dt = 0;
                    State = EState.Click;
                    CurrentObject = null;
                }
            }         
            if(bFinished)
            {
                State = EState.Eliminate;
                CurrentObject = null;
            }
        }
        if (State == EState.Eliminate)
        {
            Render();
            for (int x = 0; x < MyIceBoard.Board.Length; x++)
            {
                for (int y = 0; y < MyIceBoard.Board[0].Length - 9; y++)
                {
                    if (MyIceBoard.Board[x][y].Type == -1)
                    {
                        UpdateScore(10);
                        MyIceBoard.ScoreAnims[x][y].GetComponent<Animation>().Play();
                    }
                }
            }
            State = EState.Move;
        }
        if (State == EState.Move)
        {
            if (DropAnimationList==null)
            {
                DropAnimationList = new List<DropAnimation>();
            }
            for (int x = 0; x < MyIceBoard.Board.Length; x++)
            {
                GameObject PA = null, PB = null;                   
                for (int y = 0; y < MyIceBoard.Board[0].Length - 9; y++)//y < MyIceBoard.Board[0].Length - 1
                {                    
                    if (MyIceBoard.Board[x][y].Type < 0)
                    {                      
                        for (int k = y + 1; k < MyIceBoard.Board[0].Length; k++)
                        {
                            if(k >= MyIceBoard.Board[0].Length - 9)//k >= MyIceBoard.Board[0].Length - 1
                            {
                                if (MyIceBoard.Board[x][k] == null)
                                {
                                    int FruitTypeIndex = UnityEngine.Random.Range(0, FruitObjs.Count);
                                    GameObject FruitGb = Instantiate(FruitObjs[FruitTypeIndex]);
                                    FruitGb.transform.SetParent(this.FruitPivot);
                                    Item FruitItem = new Item(FruitGb, FruitTypeIndex, new Vector2(x, k), new Vector3(x * ItemSize, k * ItemSize, 0));
                                    FruitItem.SetLocalPosition(new Vector3(x * ItemSize, k * ItemSize, 0));
                                    if (MyIceBoard.Board[x][y].Display != PB || PA == null)
                                    {
                                        Vector3 RealPos = DstRealPos(MyIceBoard.Board[x][y].Display.transform.position);
                                        DropAnimation da = new DropAnimation(FruitItem, MyIceBoard.Board[x][y], FruitItem.Display.transform.position, RealPos);
                                        DropAnimationList.Add(da);
                                    }
                                    else
                                    {
                                        DropAnimation da = new DropAnimation(FruitItem, MyIceBoard.Board[x][y], FruitItem.Display.transform.position, PA.transform.position);
                                        DropAnimationList.Add(da);
                                    }
                                    PA = FruitItem.Display;
                                    PB = MyIceBoard.Board[x][y].Display;
                                    BoardSwap(MyIceBoard.Board[x][y], FruitItem);
                                    break;
                                }                               
                            }
                            else
                            {
                                if (MyIceBoard.Board[x][k].Type >= 0)
                                {
                                    if (MyIceBoard.Board[x][y].Display != PB || PA == null)
                                    {
                                        Vector3 RealPos = DstRealPos(MyIceBoard.Board[x][y].Display.transform.position);
                                        DropAnimation da = new DropAnimation(MyIceBoard.Board[x][k], MyIceBoard.Board[x][y], MyIceBoard.Board[x][k].Display.transform.position, RealPos);
                                        DropAnimationList.Add(da);
                                    }
                                    else
                                    {
                                        DropAnimation da = new DropAnimation(MyIceBoard.Board[x][k], MyIceBoard.Board[x][y], MyIceBoard.Board[x][k].Display.transform.position, PA.transform.position);
                                        DropAnimationList.Add(da);
                                    }
                                    PA = MyIceBoard.Board[x][k].Display;
                                    PB = MyIceBoard.Board[x][y].Display;
                                    BoardSwap(MyIceBoard.Board[x][y], MyIceBoard.Board[x][k]);
                                    break;
                                }
                                
                            }
                            
                        }
                    }
                }              
            }
            State = EState.DropAnim;       
        }
        if (State == EState.DropAnim)
        {           
            for (int i= DropAnimationList.Count-1; i>=0; i--)
            {             
                DropAnimation da = DropAnimationList[i];               
                if (DropAnimationList[i].itemdt <= 1.0f)
                {
                    DropAnimationList[i].itemdt += Time.deltaTime;
                    DropAnimationList[i].itemdt = Mathf.Min(1.0f, DropAnimationList[i].itemdt);                  
                    Swap(da.DropItem, da.UnDropItem, da.SrcPos, da.DstPos, DropAnimationList[i].itemdt);
                }                
                if (da.DropItem.Display.transform.position == da.DstPos)
                {
                    DropAnimationList.RemoveAt(i);
                }
            }           
            if (DropAnimationList.Count == 0)
            {               
                for (int x = 0; x < MyIceBoard.Board.Length; x++)
                {
                    for (int y = 9; y < MyIceBoard.Board[0].Length; y++)
                    {
                        if (MyIceBoard.Board[x][y] != null)
                        {
                            Destroy(MyIceBoard.Board[x][y].Display);
                            MyIceBoard.Board[x][y] = null;
                        }
                    }
                }
                if (ClearBoard())
                {
                    for (int x = 0; x < MyIceBoard.Board.Length; x++)
                    {
                        for (int y = 0; y < MyIceBoard.Board[0].Length - 9; y++)
                        {
                            if (MyIceBoard.Board[x][y].Type == -1)
                            {
                                UpdateScore(10);
                                MyIceBoard.ScoreAnims[x][y].GetComponent<Animation>().Play();
                            }
                        }
                    }
                State = EState.Move;
                }
                else
                {
                    IsEliminate = false;
                    State = EState.Click;
                }          
            }
        }       
    }
    bool ClearBoard()
    {
        bool result = false;
        for (int x = 0; x < MyIceBoard.Board.Length; x++)
        {
            for (int y = 0; y < MyIceBoard.Board[0].Length - 9; y++)
            {
                if (MyIceBoard.Board[x][y].Type != -1)
                {
                    eliminate(MyIceBoard.Board[x][y]);
                }
                else
                {
                    result = true;
                }              
            }
        }
        return result;
    }
    Vector3 DstRealPos(Vector3 PA)
    {
        Vector3 Ds = new Vector3(0.0f, 0.0f, 0.0f);
        int index = 0;
        for (int i = 0; i < DropAnimationList.Count; i++)
        {

            if (DropAnimationList[i].DstPos == PA)
            {
                Ds = DropAnimationList[DropAnimationList.Count - 1].DstPos;
                Ds.y = (float)(Ds.y + 0.4);
                index++;

            }
        }
        if (index == 0)
        {
            Ds = PA;
        }
        return Ds;
    }
    void Swap(Item GA, Item GB, Vector3 TA, Vector3 TB,float dt)
    {
        GA.Display.transform.position = TA + (TB - TA) * dt;
        GB.Display.transform.position = TB + (TA - TB) * dt;
    }
    bool clickBound(Item PA,Item PB)
    {       
        if ((PA.Index.x == PB.Index.x && PA.Index.y+1 == PB.Index.y) || (PA.Index.x+1 == PB.Index.x && PA.Index.y == PB.Index.y) ||(PA.Index.x == PB.Index.x&& PA.Index.y-1==PB.Index.y)||(PA.Index.x-1 == PB.Index.x && PA.Index.y == PB.Index.y)){
            return true;
        }
        else
        {
            return false;            
        }        
    }
    void BoardSwap(Item PA,Item PB)
    {
        Vector2 PaIndex = PA.Index;
        Vector2 PbIndex = PB.Index;
        Vector3 PaPos = PA.Pos;
        Vector3 PbPos = PB.Pos;
        MyIceBoard.Board[(int)PaIndex.x][(int)PaIndex.y] = PB;
        MyIceBoard.Board[(int)PbIndex.x][(int)PbIndex.y] = PA;
        PA.Index = PbIndex;
        PB.Index = PaIndex;
        PA.Pos = PbPos;
        PB.Pos = PaPos;
    }
    void eliminate(Item PA)
    {
        int a = (int)PA.Index.x;
        int b = (int)PA.Index.y;
        IsUpSame(a, b);
        IsDownSame(a, b);
        IsLeftSame(a, b);
        IsRightSame(a, b);
    }
    
    void Render()
    {
        if (MyIceBoard != null)
        {
            for (int x = 0; x < MyIceBoard.Board.Length; x++)
            {
                for (int y = 0; y < MyIceBoard.Board[0].Length-9; y++)//MyIceBoard.Board[0].Length
                {
                    MyIceBoard.Board[x][y].Display.SetActive(MyIceBoard.Board[x][y].Type >= 0);
                }
            }
        }
    }
    void IsUpSame(int a, int b)
    {
        for (int y = b ; y < MyIceBoard.Board[0].Length - 11; y++)//MyIceBoard.Board[0].Length - 2
        {
            if (MyIceBoard.Board[a][b].Type == MyIceBoard.Board[a][y+1].Type)
            {
                if(MyIceBoard.Board[a][y+1].Type != MyIceBoard.Board[a][y + 2].Type|| y == MyIceBoard.Board[0].Length - 12)//y == MyIceBoard.Board[0].Length - 3
                {
                    bool flag = false;
                    if (y == MyIceBoard.Board[0].Length - 12 && MyIceBoard.Board[a][y + 1].Type == MyIceBoard.Board[a][y + 2].Type)//y == MyIceBoard.Board[0].Length - 3
                    {
                        MyIceBoard.Board[a][MyIceBoard.Board[0].Length - 10].Type = -1;//MyIceBoard.Board[0].Length - 1
                        flag = true;
                    }
                    if (b >= 1)
                    {
                        if (MyIceBoard.Board[a][y].Type == MyIceBoard.Board[a][b - 1].Type)
                        {
                            IsEliminate = true;
                            if (b == 1)
                            {
                                for (int h = b - 1; h <= y + 1; h++)
                                {
                                    MyIceBoard.Board[a][h].Type = -1;
                                }
                            }
                            else if (b > 1 && MyIceBoard.Board[a][y].Type != MyIceBoard.Board[a][b - 2].Type)
                            {
                                for (int h = b - 1; h <= y + 1; h++)
                                {
                                    MyIceBoard.Board[a][h].Type = -1;
                                }
                            }
                        }
                        else if ((MyIceBoard.Board[a][y].Type != MyIceBoard.Board[a][b - 1].Type && y - b >= 1) || flag)
                        {
                            IsEliminate = true;
                            for (int h = b; h <= y + 1; h++)
                            {
                                MyIceBoard.Board[a][h].Type = -1;
                            }
                        }
                    }                  
                    else
                    {
                        if (y - b >= 1)
                        {
                            IsEliminate = true;
                            for (int h = b; h <= y + 1; h++)
                            {
                                MyIceBoard.Board[a][h].Type = -1;
                            }
                        }
                    }                   
                }else if (y == MyIceBoard.Board[0].Length - 12 && MyIceBoard.Board[a][y + 1].Type == MyIceBoard.Board[a][y + 2].Type)
                {
                    IsEliminate = true;
                    for (int h = b; h <= y + 2; h++)
                    {
                        MyIceBoard.Board[a][h].Type = -1;
                    }
                }
            }
            else
            {
                break;
            }                    
        }
    }
    void IsDownSame(int a,int b)
    {
        for (int y = b ; y > 1; y--)
        {
            if (MyIceBoard.Board[a][b].Type == MyIceBoard.Board[a][y-1].Type)
            {
                if( MyIceBoard.Board[a][y-1].Type != MyIceBoard.Board[a][y - 2].Type||y == 2)
                {
                    bool flag = false;
                    if (y == 2 && MyIceBoard.Board[a][y - 1].Type == MyIceBoard.Board[a][y - 2].Type)
                    {
                        MyIceBoard.Board[a][0].Type = -1;
                        flag = true;
                    }
                    if (b <= MyIceBoard.Board[0].Length - 11)//b <= MyIceBoard.Board[0].Length - 2
                    {
                        if(MyIceBoard.Board[a][y].Type == MyIceBoard.Board[a][b + 1].Type)
                        {
                            IsEliminate = true;
                            if (b== MyIceBoard.Board[0].Length - 11)//b== MyIceBoard.Board[0].Length - 2
                            {
                                for (int h = y - 1; h <= b + 1; h++)
                                {
                                    MyIceBoard.Board[a][h].Type = -1;
                                }
                            }else if(b < MyIceBoard.Board[0].Length - 11&& MyIceBoard.Board[a][y].Type != MyIceBoard.Board[a][b + 2].Type)//b < MyIceBoard.Board[0].Length - 2
                            {
                                for (int h = y - 1; h <= b + 1; h++)
                                {
                                    MyIceBoard.Board[a][h].Type = -1;
                                }
                            }
                        }
                        else if((MyIceBoard.Board[a][y].Type != MyIceBoard.Board[a][b + 1].Type&& b - y >= 1)||flag)
                        {
                            IsEliminate = true;
                            for (int h = y - 1; h <= b; h++)
                            {
                                MyIceBoard.Board[a][h].Type = -1;
                            }
                        }                       
                    }
                    else
                    {
                        if(b - y >= 1)
                        {
                            IsEliminate = true;
                            for (int h = y - 1; h <= b; h++)
                            {
                                MyIceBoard.Board[a][h].Type = -1;
                            }
                        }
                    }                                      
                }else if (y == 2 && MyIceBoard.Board[a][y - 1].Type == MyIceBoard.Board[a][y - 2].Type)
                {
                    IsEliminate = true;
                    for (int h = y - 2; h <= b; h++)
                    {
                        MyIceBoard.Board[a][h].Type = -1;
                    }
                }
            }
            else
            {
                break;
            }
        }
    }
    void IsLeftSame(int a,int b)
    {
        for (int x = a ; x < MyIceBoard.Board.Length - 2; x++)
        {
            if (MyIceBoard.Board[a][b].Type == MyIceBoard.Board[x+1][b].Type)
            {
                if (MyIceBoard.Board[x+1][b].Type != MyIceBoard.Board[x + 2][b].Type||x - a >= 1)
                {                   
                    bool flag = false;
                    if (x == MyIceBoard.Board.Length - 3 && MyIceBoard.Board[x + 1][b].Type == MyIceBoard.Board[x + 2][b].Type)
                    {
                        MyIceBoard.Board[MyIceBoard.Board.Length - 1][b].Type = -1;
                        flag = true;
                    }
                    if (a >= 1)
                    {
                        if(MyIceBoard.Board[x][b].Type == MyIceBoard.Board[a - 1][b].Type)
                        {
                            IsEliminate = true;
                            if (a == 1)
                            {
                                for (int h = a - 1; h <= x + 1; h++)
                                {
                                    MyIceBoard.Board[h][b].Type = -1;
                                }
                            }
                            else if( MyIceBoard.Board[x][b].Type != MyIceBoard.Board[a - 2][b].Type&&a>1)
                            {
                                for (int h = a - 1; h <= x + 1; h++)
                                {
                                    MyIceBoard.Board[h][b].Type = -1;
                                }
                            }
                        }
                        else if((MyIceBoard.Board[x][b].Type != MyIceBoard.Board[a - 1][b].Type&& x - a >= 1)||flag)
                        {
                            IsEliminate = true;
                            for (int h = a; h <= x + 1; h++)
                            {
                                MyIceBoard.Board[h][b].Type = -1;
                            }
                        }                      
                    }
                    else
                    {
                        if (x - a >= 1)
                        {
                            IsEliminate = true;
                            for (int h = a; h <= x + 1; h++)
                            {
                                MyIceBoard.Board[h][b].Type = -1;
                            }
                        }
                    }                                     
                }else if (x == MyIceBoard.Board.Length - 3 && MyIceBoard.Board[x + 1][b].Type == MyIceBoard.Board[x + 2][b].Type)
                {
                    IsEliminate = true;
                    for (int h = a; h <= x + 2; h++)
                    {
                        MyIceBoard.Board[h][b].Type = -1;
                    }
                }
            }
            else
            {
                break;
            }
        }
    }
    void IsRightSame(int a,int b)
    {
        for (int x = a ; x > 1; x--)
        {
            if (MyIceBoard.Board[a][b].Type == MyIceBoard.Board[x-1][b].Type)
            {
                if (MyIceBoard.Board[x - 1][b].Type != MyIceBoard.Board[x - 2][b].Type || x == 2)
                {                   
                    bool flag = false;
                    if (x == 2 && MyIceBoard.Board[x - 1][b].Type == MyIceBoard.Board[x - 2][b].Type)
                    {
                        MyIceBoard.Board[0][b].Type = -1;
                        flag = true;
                    }
                    if (a <= MyIceBoard.Board.Length - 2)
                    {
                        if (MyIceBoard.Board[x][b].Type == MyIceBoard.Board[a + 1][b].Type)
                        {
                            IsEliminate = true;
                            if (a == MyIceBoard.Board.Length - 2)
                            {
                                for (int h = x - 1; h <= a + 1; h++)
                                {
                                    MyIceBoard.Board[h][b].Type = -1;
                                }
                            }
                            else if (a < MyIceBoard.Board.Length - 2 && MyIceBoard.Board[x][b].Type != MyIceBoard.Board[a + 2][b].Type)
                            {
                                for (int h = x - 1; h <= a + 1; h++)
                                {
                                    MyIceBoard.Board[h][b].Type = -1;
                                }
                            }
                        } else if ((MyIceBoard.Board[x][b].Type != MyIceBoard.Board[a + 1][b].Type && a - x >= 1)||flag)
                        {
                            IsEliminate = true;
                            for (int h = x - 1; h <= a; h++)
                            {
                                MyIceBoard.Board[h][b].Type = -1;
                            }
                        }
                    }
                    else
                    {
                        if (a - x >= 1)
                        {
                            IsEliminate = true;
                            for (int h = x - 1; h <= a; h++)
                            {
                                MyIceBoard.Board[h][b].Type = -1;
                            }
                        }
                    }                   
                }else if(x == 2 && MyIceBoard.Board[x - 1][b].Type == MyIceBoard.Board[x - 2][b].Type)
                {
                    IsEliminate = true;
                    for (int h = x - 2; h <= a; h++)
                    {
                        MyIceBoard.Board[h][b].Type = -1;
                    }
                }
            }
            else
            {
                break;
            }

        }
    }
    Item GetItem(GameObject GA)
    {       
        for (int x = 0; x < MyIceBoard.Board.Length; x++)
        {
            for (int y = 0; y < MyIceBoard.Board[0].Length-9; y++)//y < MyIceBoard.Board[0].Length
            {
                if (MyIceBoard.Board[x][y].Display == GA)
                {                     
                    return MyIceBoard.Board[x][y];             
                }
            }
        }
        return MyIceBoard.Board[0][0];
    }
    public bool IsClickOn2DEntity(Vector2 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), -Vector2.up);

        if (hit.collider != null)
        {
            return true;
        }
        return false;
    }
    class IceBoard
    {
        public Item[][] Board; 
        public GameObject[][] Ices;
        public GameObject[][] ScoreAnims;
        public IceBoard()
        {
            this.Board = new Item[COLUM_COUNT][];
            this.Ices = new GameObject[COLUM_COUNT][];
            this.ScoreAnims = new GameObject[COLUM_COUNT][];
            for (int i = 0; i < Board.Length; i++)
            {
                this.Board[i] = new Item[ROW_COUNT];
                this.Ices[i] = new GameObject[ROW_COUNT];
                this.ScoreAnims[i] = new GameObject[ROW_COUNT];
            }
        }
    }
    public class Item
    {
        public GameObject Display; // item for display in board.
        public int Type; // type of item;
        public Vector2 Index; // x, y in ice-board.
        public Vector3 Pos;
        public Item(GameObject InDispaly, int InType, Vector2 InIndex, Vector3 InPos)
        {
            Display = InDispaly;
            Type = InType;
            Index = InIndex;
            Pos = InPos;
        }
        public void SetLocalPosition(Vector3 InPos)
        {
            this.Display.transform.localPosition = InPos;
        }
    }
    public class DropAnimation
    {
        public Item DropItem;
        public Item UnDropItem;
        public Vector3 SrcPos;
        public Vector3 DstPos;
        public float itemdt=0;
        public DropAnimation(Item DropItem, Item UnDropItem,Vector3 SrcPos, Vector3 DstPos)
        {
            this.DropItem = DropItem;
            this.UnDropItem = UnDropItem;
            this.SrcPos = SrcPos;
            this.DstPos = DstPos;
        }
    }
    public void UpdateScore(int scoreToAdd)
    {
        score += scoreToAdd;
        ScoreText.text = score.ToString();
    }  
}

