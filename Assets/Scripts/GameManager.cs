using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;//字体
using UnityEngine.SceneManagement;//场景管理器
using UnityEngine.UI;//ui
public enum EState
{
    GameStart,
    Place,
    Drop,
    GameOver,
}
public enum EModelType
{ 
    L,
    J,
    T,
    O,
    I
}
public enum Direction
{
    Left,
    Right,
    Down,
}
public class GameManager : MonoBehaviour
{
    public GameObject itemPrefab;
    public Transform Pivot;
    public float itemSize = 0.5f;
    public float UnitDeltaTime = 1;
    float CurrentDeltaTime = 0;
    EState State;
    private List<EModelType> modelTypePools;
    Model currentModel;
    ChessBoard chessBoard;
    public float FastDropDeltaTime = 0.5f;
    private bool bFastDrop = false;
    public TextMeshProUGUI scoreText;//TextMeshProUGUI把字体当模型渲染，3d字体
    public TextMeshProUGUI gameOverText;
    private int score;
    public bool isGameActive;
    public Button restartButton;
    public GameObject titleScreen;
    // Start is called before the first frame update
    void Start()
    {
        State = EState.GameStart;
        titleScreen.gameObject.SetActive(false);
        score = 0;
        // init chess board
        chessBoard = new ChessBoard();
        // init item
        for (int x = 0; x < chessBoard.board.Length; x++)
        {
            for (int y = 0; y < chessBoard.board[x].Length; y++)
            {
                GameObject item = Instantiate<GameObject>(itemPrefab);
                item.transform.SetParent(this.Pivot);
                item.transform.localPosition = new Vector3(x * itemSize, y * itemSize, 0);
                chessBoard.items[x][y] = item;
            }
        }
        // init model
        modelTypePools = new List<EModelType>();
        modelTypePools.Add(EModelType.L);
        modelTypePools.Add(EModelType.J);
        modelTypePools.Add(EModelType.T);
        modelTypePools.Add(EModelType.O);
        modelTypePools.Add(EModelType.I);
        State = EState.Place;
    }
    void Update()
    {
        CurrentDeltaTime += bFastDrop ? FastDropDeltaTime : Time.deltaTime;
        if (CurrentDeltaTime > UnitDeltaTime)
        {
            CurrentDeltaTime = 0;
            // handle place
            if (State == EState.Place)
            {
                int modelTypeIndex = Random.Range(0, modelTypePools.Count);
                EModelType modelType = modelTypePools[modelTypeIndex];
                Model model = ModelFactory.NewModel(modelType);
                model.origin = new Vector2(chessBoard.board.Length / 2, chessBoard.board.Length);
                model.Place(chessBoard, 1);
                currentModel = model;
                State = EState.Drop;
            }
            // handle drop
            else if (State == EState.Drop)
            {
                Vector2 OldOrigin = currentModel.origin;
                currentModel.origin.y -= 1;
                if (currentModel.Isbound(chessBoard) && !currentModel.Isobstacle(chessBoard, Direction.Down))
                {
                    currentModel.origin = OldOrigin;
                    currentModel.Place(chessBoard, 0);
                    currentModel.origin.y -= 1;
                    currentModel.Place(chessBoard, 1);
                }
                else
                {
                    GameOver();
                    State = EState.Place;
                }
            }
        }
        if (currentModel != null)
        {
            Vector2 TempOrigin = currentModel.origin;
            currentModel.origin.x -= 1;
            if (Input.GetKeyDown(KeyCode.A) && currentModel.Isbound(chessBoard) && !currentModel.Isobstacle(chessBoard, Direction.Left))
            {
                currentModel.origin = TempOrigin;
                currentModel.Place(chessBoard, 0);
                currentModel.origin.x -= 1;
                currentModel.Place(chessBoard, 1);
            }
            else
            {
                currentModel.origin.x += 1;
            }
            currentModel.origin.x += 1;
            if (Input.GetKeyDown(KeyCode.D) && currentModel.Isbound(chessBoard) && !currentModel.Isobstacle(chessBoard, Direction.Right))
            {
                currentModel.origin = TempOrigin;
                currentModel.Place(chessBoard, 0);
                currentModel.origin.x += 1;
                currentModel.Place(chessBoard, 1);
            }
            else
            {
                currentModel.origin.x -= 1;
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                currentModel.Place(chessBoard, 0);
                int temp = currentModel.Newint;
                currentModel.Rotate(temp % 4);
                currentModel.Place(chessBoard, 1);
                currentModel.Newint++;
            }
            bFastDrop = false;
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKey(KeyCode.S))
            {
                bFastDrop = true;
            }
        }
        if (currentModel != null)
        {
            DeleteLines();
        }
        Render();
    }

    void DeleteLines()
    {
        for(int y=0;y<chessBoard.board.Length;y++)
        {
            for(int x=0; x < chessBoard.board.Length;x++)
            {
                if (chessBoard.board[x][y] >0)
                {
                    if (x == chessBoard.board.Length - 1)
                    {
                        for(int k=0;k< chessBoard.board.Length;k++)
                        {
                            chessBoard.board[k][y] = 0;                           
                        }
                        for(int i=y; i < chessBoard.board.Length-1; i++)
                        {
                            for(int j=0;j< chessBoard.board.Length; j++)
                            {
                                chessBoard.board[j][i] = chessBoard.board[j][i+1];
                            }
                        }
                        UpdateScore(10);
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }
    void Render()//画面显示
    {
        if (chessBoard != null)
        {
            for (int x = 0; x < chessBoard.board.Length; x++)
            {
                for (int y = 0; y < chessBoard.board[x].Length; y++)
                {
                    chessBoard.items[x][y].SetActive(chessBoard.board[x][y] > 0);
                }
            }
        }             
    }
    public void UpdateScore(int scoreToAdd)
    {
        score += scoreToAdd;
        scoreText.text = "Score:" + score;
    }
    public void GameOver()
    {       
        for (int x = 0; x < chessBoard.board.Length; x++)
        {
            for (int y = 0; y < chessBoard.board[x].Length; y++)
            {
                if (chessBoard.board[x][20] == 1)
                {
                    restartButton.gameObject.SetActive(true);
                    gameOverText.gameObject.SetActive(true);
                    for (int i = 0; i < chessBoard.board.Length; i++)
                    {
                        for (int j = 0; j < chessBoard.board[i].Length; j++)
                        {
                            chessBoard.board[x][y] = 0;
                        }
                    }
                }
            }
        }
    }
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    class ChessBoard
    {
        public int[][] board;
        public GameObject[][] items;
        public ChessBoard()
        {
            this.board = new int[20][];
            this.items = new GameObject[20][];
            for (int i = 0; i < board.Length; i++)
            {
                this.board[i] = new int[24];
                this.items[i] = new GameObject[24];
            }           
        }
    }
    class ModelFactory
    {
        public static Model NewModel(EModelType ModelType)
        {
            switch (ModelType)
            {
                case EModelType.L: return new L();
                case EModelType.J: return new J();
                case EModelType.T: return new T();
                case EModelType.O: return new O();
                case EModelType.I: return new I();
            }
            return new Model();
        }
    }
    class Model
    {
        //public float angle = 90;
        public Vector2[] point;
        public Vector2 origin;
        public EModelType ModelType;
        public int Newint;
        public virtual void Place(ChessBoard chessBoard, int value)
        {

        }
        public bool Isbound(ChessBoard chessBoard)
        {
            bool result = true;
            for (int i = 0; i < point.Length; i++)
            {
                Vector2 temp = this.Move(point[i].x, point[i].y);
                if (temp.x >= 0 && temp.x < chessBoard.board.Length && temp.y >= 0 && temp.y < chessBoard.board[0].Length)
                {

                }
                else
                {
                    result = false;
                    break;
                }
            }
            return result;
        }
        public bool Isobstacle(ChessBoard chessBoard,Direction direction)
        {
            bool result = false;
            int i = 0;
            while (i < point.Length)
            {
                Vector2 current = this.Move(point[i].x, point[i].y);
                Vector2 currentDown = new Vector2(current.x, current.y - 1);
                Vector2 currentLeft = new Vector2(current.x - 1, current.y);
                Vector2 currentRight = new Vector2(current.x + 1, current.y);
                // 判断不是自己
                int j = 0;
                while (j < point.Length&&direction==Direction.Down)
                {
                    Vector2 temp2 = this.Move(point[j].x, point[j].y);
                    if (currentDown == temp2)
                    {
                        break;
                    }
                    else
                    {
                        j++;
                    }
                }
                while (j < point.Length && direction == Direction.Left)
                {
                    Vector2 temp2 = this.Move(point[j].x, point[j].y);
                    if (currentLeft == temp2)
                    {
                        break;
                    }
                    else
                    {
                        j++;
                    }
                }
                while (j < point.Length && direction == Direction.Right)
                {
                    Vector2 temp2 = this.Move(point[j].x, point[j].y);
                    if (currentRight == temp2)
                    {
                        break;
                    }
                    else
                    {
                        j++;
                    }
                }
                bool bIsNotSelf = j == point.Length;
                if (bIsNotSelf)
                {
                    // 如果不是自己，判断棋盘是不是有值
                    if (chessBoard.board[(int)(current.x)][(int)(current.y)] == 1 )
                    {
                        result = true;
                        break;
                    }
                    else
                    {
                    }
                }
                i++;
            }
            return result;
        }
        public virtual void Rotate(int Newint)
        {

        }
        public virtual Vector2 Move(float x, float y)
        {
            x = x + origin.x;
            y = y + origin.y;
            return new Vector2(x, y);
        }
    }
    class L : Model
    {

        public L()
        {
            point = new Vector2[4];
            point[0] = new Vector2(0, 2);
            point[1] = new Vector2(0, 1);
            point[2] = new Vector2(0, 0);
            point[3] = new Vector2(1, 0);
            ModelType = EModelType.L;
        }
        public override void Place(ChessBoard chessBoard, int value)
        {
            base.Place(chessBoard, value);
            Vector2 temp0 = base.Move(point[0].x, point[0].y);
            Vector2 temp1 = base.Move(point[1].x, point[1].y);
            Vector2 temp2 = base.origin;
            Vector2 temp3 = base.Move(point[3].x, point[3].y);
            chessBoard.board[(int)temp0.x][(int)temp0.y] = value;
            chessBoard.board[(int)temp1.x][(int)temp1.y] = value;
            chessBoard.board[(int)temp2.x][(int)temp2.y] = value;
            chessBoard.board[(int)temp3.x][(int)temp3.y] = value;

        }
        public override void Rotate(int Newint)
        {
            point[2] = new Vector2(0, 0);//记得是偏移，而不是原点
            if (Newint == 1)
            {
                point[0] = new Vector2(2, 0);
                point[1] = new Vector2(1, 0);
                point[3] = new Vector2(0, -1);
            }
            if (Newint == 2)
            {
                point[0] = new Vector2(0, -2);
                point[1] = new Vector2(0, -1);
                point[3] = new Vector2(-1, 0);
            }
            if (Newint == 3)
            {
                point[0] = new Vector2(-2, 0);
                point[1] = new Vector2(-1, 0);
                point[3] = new Vector2(0, 1);
            }
            if(Newint == 0)
            {
                point[0] = new Vector2(0, 2);
                point[1] = new Vector2(0, 1);
                point[3] = new Vector2(1, 0);
            }
        }
    }
    class J : Model
    {
        public J()
        {
            point = new Vector2[4];
            point[0] = new Vector2(0, 2);
            point[1] = new Vector2(0, 1);
            point[2] = new Vector2(0, 0);
            point[3] = new Vector2(-1, 0);
            ModelType = EModelType.J;
        }
        public override void Place(ChessBoard chessBoard, int value)
        {
            base.Place(chessBoard, value);
            Vector2 temp0 = base.Move(point[0].x, point[0].y);
            Vector2 temp1 = base.Move(point[1].x, point[1].y);
            Vector2 temp2 = base.origin;
            Vector2 temp3 = base.Move(point[3].x, point[3].y);
            chessBoard.board[(int)temp0.x][(int)temp0.y] = value;
            chessBoard.board[(int)temp1.x][(int)temp1.y] = value;
            chessBoard.board[(int)temp2.x][(int)temp2.y] = value;
            chessBoard.board[(int)temp3.x][(int)temp3.y] = value;
        }
        public override void Rotate(int Newint)
        {
            point[2] = new Vector2(0, 0);
            if (Newint == 1)
            {
                point[0] = new Vector2(2, 0);
                point[1] = new Vector2(1, 0);
                point[3] = new Vector2(0, 1);
            }
            if (Newint == 2)
            {
                point[0] = new Vector2(0, -2);
                point[1] = new Vector2(0, -1);
                point[3] = new Vector2(1, 0);
            }
            if (Newint == 3)
            {
                point[0] = new Vector2(-2, 0);
                point[1] = new Vector2(-1, 0);
                point[3] = new Vector2(0, -1);
            }
            if(Newint == 0)
            {
                point[0] = new Vector2(0, 2);
                point[1] = new Vector2(0, 1);
                point[3] = new Vector2(-1, 0);
            }
        }
    }
    class T : Model
    {
        public T()
        {
            point = new Vector2[4];
            point[0] = new Vector2(-1, 0);
            point[1] = new Vector2(0, 0);
            point[2] = new Vector2(1, 0);
            point[3] = new Vector2(0, -1);
            ModelType = EModelType.T;
        }
        public override void Place(ChessBoard chessBoard, int value)
        {
            base.Place(chessBoard, value);
            Vector2 temp0 = base.Move(point[0].x, point[0].y);
            Vector2 temp1 = base.origin;
            Vector2 temp2 = base.Move(point[2].x, point[2].y);
            Vector2 temp3 = base.Move(point[3].x, point[3].y);
            chessBoard.board[(int)temp0.x][(int)temp0.y] = value;
            chessBoard.board[(int)temp1.x][(int)temp1.y] = value;
            chessBoard.board[(int)temp2.x][(int)temp2.y] = value;
            chessBoard.board[(int)temp3.x][(int)temp3.y] = value;
        }
        public override void Rotate(int Newint)
        {
            point[1] = new Vector2(0, 0);
            if (Newint == 1)
            {
                point[0] = new Vector2(0, 1);
                point[2] = new Vector2(0, -1);
                point[3] = new Vector2(-1, 0);
            }
            if (Newint == 2)
            {
                point[0] = new Vector2(1, 0);
                point[2] = new Vector2(-1, 0);
                point[3] = new Vector2(0, 1);
            }
            if (Newint == 3)
            {
                point[0] = new Vector2(0, -1);
                point[2] = new Vector2(0, 1);
                point[3] = new Vector2(1, 0);
            }
            if(Newint == 0)
            {
                point[0] = new Vector2(-1, 0);
                point[2] = new Vector2(1, 0);
                point[3] = new Vector2(0, -1);
            }
        }
    }
    class I : Model
    {
        public I()
        {
            point = new Vector2[4];
            point[0] = new Vector2(0, 2);
            point[1] = new Vector2(0, 1);
            point[2] = new Vector2(0, 0);
            point[3] = new Vector2(0, -1);
            ModelType = EModelType.I;
        }
        public override void Place(ChessBoard chessBoard, int value)
        {
            base.Place(chessBoard, value);
            Vector2 temp0 = base.Move(point[0].x, point[0].y);
            Vector2 temp1 = base.Move(point[1].x, point[1].y);
            Vector2 temp2 = base.origin;
            Vector2 temp3 = base.Move(point[3].x, point[3].y);
            chessBoard.board[(int)temp0.x][(int)temp0.y] = value;
            chessBoard.board[(int)temp1.x][(int)temp1.y] = value;
            chessBoard.board[(int)temp2.x][(int)temp2.y] = value;
            chessBoard.board[(int)temp3.x][(int)temp3.y] = value;

        }
        public override void Rotate(int Newint)
        {
            point[0] = new Vector2(0, 0);
            if (Newint == 1)
            {
                point[0] = new Vector2(2, 0);
                point[1] = new Vector2(1, 0);
                point[3] = new Vector2(-1, 0);
            }
            if (Newint == 2)
            {
                point[0] = new Vector2(0, -2);
                point[1] = new Vector2(0, -1);
                point[3] = new Vector2(0, 1);
            }
            if (Newint == 3)
            {
                point[0] = new Vector2(-2, 0);
                point[1] = new Vector2(-1, 0);
                point[3] = new Vector2(1, 0);
            }
            if(Newint == 0)
            {
                point[0] = new Vector2(0, 2);
                point[1] = new Vector2(0, 1);
                point[3] = new Vector2(0, -1);
            }
        }
    }
    class O : Model
    {
        public O()
        {
            point = new Vector2[4];
            point[0] = new Vector2(0, 0);
            point[1] = new Vector2(1, 0);
            point[2] = new Vector2(1, -1);
            point[3] = new Vector2(0, -1);
            ModelType = EModelType.O;
        }
        public override void Place(ChessBoard chessBoard, int value)
        {
            base.Place(chessBoard, value);
            Vector2 temp0 = base.origin;
            Vector2 temp1 = base.Move(point[1].x, point[1].y);
            Vector2 temp2 = base.Move(point[2].x, point[2].y);
            Vector2 temp3 = base.Move(point[3].x, point[3].y);
            chessBoard.board[(int)temp0.x][(int)temp0.y] = value;
            chessBoard.board[(int)temp1.x][(int)temp1.y] = value;
            chessBoard.board[(int)temp2.x][(int)temp2.y] = value;
            chessBoard.board[(int)temp3.x][(int)temp3.y] = value;
        }
    }
}






