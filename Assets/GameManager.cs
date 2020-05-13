using Assets;
using AssetsAI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using AssetsAI;
using System.Threading;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private Board board; //Oyun tahtası

    [SerializeField]
    private DiceManager diceManager; // zar

    [SerializeField]
    private Text CurrentPlayer; //şuan ki oyuncu Text

    private Expectiminimax Computer; //ComputerPlayer 
    //private List<Stack> moveables;

    private Dictionary<Stack, List<Stack>> moveables; //humanplayer için hareket listesi
    private bool isFirstTouch = true; //tahtada seçilen ilk stack mi kontrolü
    Stack selectedStack; //seçilen stack
    public Stack P1Breaked; // humanplayer için kırık taş stacki
    public Stack P2Breaked; // computerplayer için kırık taş stacki
    public Stack P1Collection; // humanplayer için toplanan  taşların stacki
    public Stack P2Collection;// computerplayer için toplanan  taşların stacki
    public int currentPlayer = 1; // kim hamle yapacak
    public List<int> currentDices; //gelen zarlar
    public List<int> possibleDices;//expectiminimax için oluşabilecek zarlar
    public List<int> possibleDices2;

    public int[] dices;
    private GameState state; //Game State saklanır

    //game managerin diğer objelere initiliaze işlemleri.
    void Awake()
    {
        board.Init(this);
        diceManager.Init(this);

        P1Breaked.Init(board);
        P2Breaked.Init(board);

        P1Collection.Init(board);
        P2Collection.Init(board);
        Computer = new Expectiminimax();
        Computer.Init(this);
       
    }
    //Tahtadaki datayı state şeklinde döndüren fonksiyon
    public GameState GetBoardData()
    {
        GameState state = new GameState();
        //state.stacks

        for (int i = 0; i < 24; i++) //oyun alanı taranır
        {
            if (board.Stacks[i].HasStamps) //taş varsa oyuncuların alanına toplanır
            {
                state.stacks[i + 1].stampCount = board.Stacks[i].Stamps.Count;
                state.stacks[i + 1].playerNo = board.Stacks[i].GetPlayerNo;
            }

        }
        state.stacks[0].stampCount = P1Collection.Stamps.Count; // koleksiyon taşları eklenir
        state.stacks[25].stampCount = P2Collection.Stamps.Count;

        if (P1Breaked.HasStamps) //kırık taşlar eklenir
        {
            state.stacks[26].stampCount = P1Breaked.Stamps.Count;
            state.stacks[26].playerNo =1;

        }
        if (P2Breaked.HasStamps)
        {
            state.stacks[27].stampCount = P2Breaked.Stamps.Count;
            state.stacks[27].playerNo = 2;

        }

        state.dice[0] = currentDices[0]; //zarlar eklenir ve state return edilir
        state.dice[1] = currentDices[1];

        return state;
    }
    public void Start()
    {
      /*  currentDices = diceManager.Throw();
        GameState state2 = GetBoardData();
        state2.MoveStamp(1, 27);
        SetStateToBoard(state2);*/
    }
    public void Update()
    {
        if(state != null) //ComputerPlayer hamle yaptıysa bulunan best state tahtaya yansıtılır
        {
            SetStateToBoard(state);
            state = null;
            //states.Clear();
            if(moveables!=null)
                moveables.Clear();
            currentDices.Clear();
            NextPlayer();
        }
    }
    public void SetStateToBoard(GameState gameState) //State den gelen veri oyun tahtasına dökülür
    {
        for (int i = 0; i < 26; i++) // tahtadan tüm taşlar bir bölgeye toplanır
        {
            if (board.Stacks[i].HasStamps)
            {
                while (board.Stacks[i].Stamps.Count != 0)
                {
                    if (board.Stacks[i].GetPlayerNo == 1)
                        board.P1StampsStack.Move1Stamp(board.Stacks[i]);
                    else
                        board.P2StampsStack.Move1Stamp(board.Stacks[i]);
                }

                //stamps.AddRange(board.Stacks[i].Stamps);
                //board.Stacks[i].Stamps.Clear();
            }
        }

        for (int i = 1; i < 26; i++) //state e göre tekrardan dağıtılır
        {
            for (int j = 0; j < gameState.stacks[i].stampCount; j++)
            {

                if (gameState.stacks[i].playerNo == 1)
                {
                    board.Stacks[i - 1].Move1Stamp(board.P1StampsStack);
                }
                else
                {
                    board.Stacks[i - 1].Move1Stamp(board.P2StampsStack);
                }
            }
        }
        //kırık ve koleksiyondaki taşlar dağıtılır
        for (int i = 0; i < gameState.stacks[0].stampCount; i++)
            P1Collection.Move1Stamp(board.P1StampsStack);
        for (int i = 0; i < gameState.stacks[25].stampCount; i++)
            P2Collection.Move1Stamp(board.P2StampsStack);
        for (int i = 0; i < gameState.stacks[26].stampCount; i++)
            P1Breaked.Move1Stamp(board.P1StampsStack);
        for (int i = 0; i < gameState.stacks[27].stampCount; i++)
            P2Breaked.Move1Stamp(board.P2StampsStack);

    }
    
   
    public int GetBlockCount(GameState state, int PlayerNo)
    {
        //int count = 0;
        //for (int i = 0; i <= 23; i++)
        //{
        //    if (state.board[i].Item2 == PlayerNo && state.board[i].Item1 > 1)
        //    {
        //        count++;
        //    }
        //}
        //return count;
        return 0;
    }
    public int GetHomeStampsCount(GameState state, int PlayerNo)
    {
        //int count = 0;
        //if (PlayerNo == 1)
        //{
        //    for (int i = 0; i <= 5; i++)
        //    {
        //        if (state.board[i].Item1 > 0 && state.board[i].Item2 == PlayerNo)
        //        {
        //            count += state.board[i].Item1;
        //        }
        //    }
        //}
        //else
        //{
        //    for (int i = 23; i >= 18; i--)
        //    {
        //        if (state.board[i].Item1 > 0 && state.board[i].Item2 == PlayerNo)
        //        {
        //            count += state.board[i].Item1;
        //        }
        //    }
        //}
        //return count;
        return 0;
    }
    
    public int GetBoardStampsCount(GameState state, int PlayerNo)
    {
        //int count = 0;
        //if (PlayerNo == 1)
        //{
        //    for (int i = 6; i <= 23; i++)
        //    {
        //        if (state.board[i].Item1 > 0 && state.board[i].Item2 == PlayerNo)
        //        {
        //            count += state.board[i].Item1;
        //        }
        //    }
        //}
        //else
        //{
        //    for (int i = 17; i >= 0; i--)
        //    {
        //        if (state.board[i].Item1 > 0 && state.board[i].Item2 == PlayerNo)
        //        {
        //            count += state.board[i].Item1;
        //        }
        //    }
        //}
        //return count;
        return 0;
    }

    bool issecondplaying = false; //ComputerPlayer oynuyor mu kontrolü için
    public void OnStackSelected(Stack stack) //tahtadan stack seçildiğinde
    {
        if (stack == null && currentPlayer == 1)
            return;
        foreach (var s in board.Stacks)
            s.Highlight(false);
        P1Collection.Highlight(false);
        P2Collection.Highlight(false);
        if (currentPlayer == 2) // eğer bilgisayar hamle yapacsa
        {

            //  possibleDices2.AddRange(currentDices);
            state = null; // state null set edilir update fonksyionunda kontrülü yapılıyor. her framede.
            if(!issecondplaying)
            {
                new Thread(() => {
                    issecondplaying = true;
                    Profiler.BeginThreadProfiling("My threads", "My thread 1"); //analiz için kullanıldı

                    state = Computer.FindBestMove(); //Bilgisayarın gelen zarlar için en iyi hamleyi hesaplayacak Expectiminimax algoritmasının fonksiyonu çağırılır
                    //buradan sonuç geldiğinde update fonksiyonunda bulunan best state tahtaya set ediliyor..

                    Profiler.EndThreadProfiling();
                    issecondplaying = false; //bilgisayar oyununu oynadı

                }).Start();
            }
            
            
           
        }
        else // 1. oyununcunun oyun mantığı (HumanPlayer)
        {

            if (moveables != null && moveables.Count != 0 && !isFirstTouch && moveables[selectedStack].Contains(stack))
            {

                if (stack.HasStamps && selectedStack.HasStamps && stack.CanBreak(selectedStack))
                {
                    var breaked = stack.Stamps[0];
                    if (breaked.PlayerNo == 1)
                    {
                        P1Breaked.Move1Stamp(stack);

                    }
                    else
                    {
                        P2Breaked.Move1Stamp(stack);
                    }
                }
                stack.Move1Stamp(selectedStack);
                currentDices.Remove(stack.GetDistance(selectedStack));
                moveables = GetAllMovements(currentPlayer);
                if (moveables != null && moveables.Count < 1)
                {
                    currentDices.Clear();

                    NextPlayer();
                }
                //moveables = null;
                selectedStack = null;
                isFirstTouch = true;

            }
            else
            {
                if (stack.HasStamps && stack.GetPlayerNo == currentPlayer && moveables.Keys.Contains(stack))
                {
                    if (stack.GetPlayerNo == 1 && stack != P1Breaked && P1Breaked.HasStamps)
                        return;
                    if (stack.GetPlayerNo == 2 && stack != P2Breaked && P2Breaked.HasStamps)
                        return;

                    this.selectedStack = stack;
                    //moveables = GetMoveableStacks(stack, stack.GetPlayerNo);
                    foreach (var s in moveables[stack])
                    {
                        s.Highlight(true);
                    }
                    isFirstTouch = false;

                }
                else
                    isFirstTouch = true;

            }
        }


    }

    //1. oyuncu için GameState de anlatılan fonksiyonlar
    private bool CanMoveToCollection(int currentPlayer)
    {
        bool flag = true;
        if (currentPlayer == 1)
        {
            for (int i = 6; i < 24; i++)
            {
                var stack = board.Stacks[i];
                if (stack.HasStamps && stack.GetPlayerNo == currentPlayer)
                {
                    flag = false;
                    break;
                }
            }
        }
        else
        {
            for (int i = 0; i < 18; i++)
            {
                var stack = board.Stacks[i];
                if (stack.HasStamps && stack.GetPlayerNo == currentPlayer)
                {
                    flag = false;
                    break;
                }
            }

        }

        return flag;
    }

    //bir sonraki oyuncuyu set eden fonksiyon
    private void NextPlayer()
    {
        diceManager.dice1.text = "?";
        diceManager.dice2.text = "?";
        currentPlayer = currentPlayer == 2 ? 1 : 2;
        CurrentPlayer.text = "Player=" + currentPlayer.ToString();
        if (currentPlayer == 2)
        {
            OnDiceClick();
            OnStackSelected(null);
        }
    }
    //zar a tıklanıldığında yeni değerler ve oynayabilecek hamlesi var mı hesaplamasını yapan fonksiyon
    public void OnDiceClick()
    {
        if (currentDices.Count == 0)
        {
            currentDices = diceManager.Throw();
            moveables = GetAllMovements(currentPlayer);
            if (moveables != null && moveables.Count < 1)
            {
                currentDices.Clear();
                NextPlayer();
            }
        }
    }
    public void SetPossibleDices((int, int) DicePairs, int PlayerNo)
    {
        if (PlayerNo == 1)
        {
            possibleDices.Clear();
            possibleDices.Add(DicePairs.Item1);
            possibleDices.Add(DicePairs.Item2);
        }
        else
        {
            possibleDices2.Clear();
            possibleDices2.Add(DicePairs.Item1);
            possibleDices2.Add(DicePairs.Item2);
        }
    }
    public Dictionary<Stack, List<Stack>> GeneratePossibleMoves(int PlayerNo)
    {

        currentDices = (PlayerNo == 1) ? possibleDices : possibleDices2;
        return GetAllMovements(PlayerNo);
    }

    public bool IsMoveableWithDice(Stack startedStack, Stack targetStack)
    {
        int distance = startedStack.GetDistance(targetStack);
        return currentDices.Contains(distance);
    }

    public Dictionary<Stack, List<Stack>> GetAllMovements(int PlayerNo)
    {
        Dictionary<Stack, List<Stack>> movements = new Dictionary<Stack, List<Stack>>();
        var playerStacks = board.Stacks.Where(x => x.HasStamps && x.GetPlayerNo == PlayerNo).ToList();

        foreach (Stack x in playerStacks)
        {
            var moveableList = GetMoveableStacks(x, PlayerNo);
            if (x == P1Breaked && PlayerNo == 1)
            {
                if (moveableList.Count == 0)
                {
                    currentDices.Clear();
                    NextPlayer();
                    return null;
                }
                movements = new Dictionary<Stack, List<Stack>>();
                movements.Add(x, moveableList);
                return movements;
            }
            else
            {
                if (x == P2Breaked && PlayerNo == 2)
                {
                    if (moveableList.Count == 0)
                    {
                        currentDices.Clear();
                        NextPlayer();
                        return null;
                    }
                    movements = new Dictionary<Stack, List<Stack>>();
                    movements.Add(x, moveableList);
                    return movements;
                }
                else if (moveableList.Count > 0)
                {
                    movements.Add(x, moveableList);
                }
            }
        }
        return movements;
    }
    public List<Stack> GetMoveableStacks(Stack selectedStack, int PlayerNo)
    {
        var moveables = new List<Stack>();
        if (!selectedStack.HasStamps)
            return moveables;

        if (selectedStack.HasStamps && selectedStack.GetPlayerNo != PlayerNo)
            return moveables;


        if (PlayerNo == 1)
        {
            for (int i = selectedStack.StackID - 2; i >= 0; i--)
            {

                Stack target = board.Stacks[i];
                if (!IsMoveableWithDice(selectedStack, target))
                    continue;
                if (!target.HasStamps)
                    moveables.Add(target);
                if (target.HasStamps && target.GetPlayerNo == PlayerNo)
                    moveables.Add(target);
                if (target.HasStamps && target.CanBreak(selectedStack))
                    moveables.Add(target);

            }
            if (CanMoveToCollection(1) && IsMoveableWithDice(selectedStack, P1Collection))
                moveables.Add(P1Collection);
            //board.Stacks[selectedStack.StackID];
        }
        else
        {
            for (int i = selectedStack.StackID; i < 24; i++)
            {
                Stack target = board.Stacks[i];
                if (!IsMoveableWithDice(selectedStack, target))
                    continue;
                if (!target.HasStamps)
                    moveables.Add(target);
                if (target.HasStamps && target.GetPlayerNo == PlayerNo)
                    moveables.Add(target);
                if (target.HasStamps && target.CanBreak(selectedStack))
                    moveables.Add(target);

            }
            if (CanMoveToCollection(2) && IsMoveableWithDice(selectedStack, P2Collection))
                moveables.Add(P2Collection);

        }

        return moveables;
    }


}

