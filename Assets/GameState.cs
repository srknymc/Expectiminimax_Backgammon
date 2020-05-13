using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AssetsAI
{
    [Serializable]
    public struct Stack
    {
        public int stampCount; //taşların sayısı
        public int playerNo; //stackde bulunan taşın sahini
    }

    [Serializable]
    public class GameState
    {
        public Stack[] stacks = new Stack[28]; // Game state de bulunan stackler 24 oyun alanı 2 adet toplanan stack ve 2 adet kırık taşların stackleri




        public int[] dice = new int[4];
        public static readonly int P1Collection = 0;
        public static readonly int P2Collection = 25;
        public static readonly int P1Breaked = 26;
        public static readonly int P2Breaked = 27;
        public GameState()
        {

        }
        public GameState DeepCopy() //hard copy işlemi
        {
            var copy = new GameState();
            for (int i = 0; i < 28; i++)
            {
                copy.stacks[i].playerNo = stacks[i].playerNo;
                copy.stacks[i].stampCount = stacks[i].stampCount;
            }
            for (int i = 0; i < 4; i++)
            {
                copy.dice[i] = dice[i];
            }
            return copy;
        }
        //verilen stacten bir diğerine taş taşıma işlemi
        public void MoveStamp(int source, int target)
        {
            if (HasStamps(source) && HasStamps(target) && CanBreak(source, target))
            {
                if (stacks[target].playerNo == 1)
                {
                    stacks[P1Breaked].stampCount++;
                    stacks[target].stampCount = 0;

                }
                else
                {
                    stacks[P2Breaked].stampCount++;
                    stacks[target].stampCount = 0;

                }
            }
            stacks[source].stampCount--;
            stacks[target].stampCount++;
            stacks[target].playerNo = stacks[source].playerNo;
            if (stacks[source].stampCount <= 0)
            {
                stacks[source].playerNo = 0;
            }

        }

        //verilen stackte taş var mı?
        public bool HasStamps(int i) => stacks[i].stampCount > 0;


        //kırık taşı var mı ?
        public bool HasBreaked(int PlayerNo)
        {
            if (PlayerNo == 1)
            {
                return stacks[P1Breaked].stampCount > 0;
            }
            else
            {
                return stacks[P2Breaked].stampCount > 0;
            }
        }
        //taş kırılabilir mi?
        public bool CanBreak(int source, int target)
        {
            return stacks[source].playerNo != stacks[target].playerNo && stacks[target].stampCount == 1;
        }
        //zar ile bir yerden bir yere hareket edebilir mi ?
        public bool IsMoveableWithDice(int startedStack, int targetStack, int dice)
        {
            int distance = Math.Abs(startedStack - targetStack);
            return dice == distance;
        }
        //Taş toplanabilir mi kontrolü
        private bool CanMoveToCollection(int currentPlayer)
        {
            bool flag = true;
            if (currentPlayer == 1)
            {
                for (int i = 6; i < 24; i++)
                {
                    var stack = i;
                    if (HasStamps(stack) && stacks[stack].playerNo == currentPlayer)
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
                    var stack = i;
                    if (HasStamps(stack) && stacks[stack].playerNo == currentPlayer)
                    {
                        flag = false;
                        break;
                    }
                }

            }

            return flag;
        }

        //verilen oyuncunun zarlar ile yapabileceği tüm ikili hamleler hesaplarınır.
        //Expectiminimax algoritmasında kullanılacak stateleri döndürür
        public List<GameState> GetAllMovements(int PlayerNo, int dice1, int dice2)
        {
            var list = new List<GameState>();

            {
                List<GameState> states = new List<GameState>(); ;
                for (int i = 0; i < stacks.Length; i++)
                {
                    if (stacks[i].playerNo == PlayerNo)
                        states.AddRange(GetMoveableStacks(i, PlayerNo, dice1)); //zar 1 ile tüm hamleler bir stack için hesaplanır state kayıt edilir
                }
                foreach (var state in states)
                {
                    bool added = false;
                    for (int i = 0; i < state.stacks.Length; i++)
                    {
                        if (state.stacks[i].playerNo == PlayerNo)
                        {
                            var ret = state.GetMoveableStacks(i, PlayerNo, dice2); //oynanan zar için kalan zar hamleleri yapılır ve tüm stateler kaydedilir
                            if (ret.Count != 0)
                            {
                                added = true;
                                list.AddRange(ret);
                            }
                        }
                    }
                    if (!added)
                    {
                        list.Add(state);
                    }
                }
            }
            //yukarıda yapılan işlemin zarların swaplanmış halleri tekrardan yapılır statelere eklenir
            {
                List<GameState> states = new List<GameState>(); ;
                for (int i = 0; i < stacks.Length; i++)
                {
                    if (stacks[i].playerNo == PlayerNo)
                        states.AddRange(GetMoveableStacks(i, PlayerNo, dice2));
                }
                foreach (var state in states)
                {
                    bool added = false;
                    for (int i = 0; i < state.stacks.Length; i++)
                    {
                        if (state.stacks[i].playerNo == PlayerNo)
                        {
                            var ret = state.GetMoveableStacks(i, PlayerNo, dice1);
                            if (ret.Count != 0)
                            {
                                added = true;
                                list.AddRange(ret);
                            }
                        }
                    }
                    if (!added)
                    {
                        list.Add(state);
                    }

                }

                return list;


            }
        }
        //Verilen stackten bir oyunucun oynayabileceği durumları döndüren fonksiyon
        public List<GameState> GetMoveableStacks(int selectedStack, int PlayerNo, int dice)
        {
            bool isStackChanged = false;
            var newstates = new List<GameState>();
            var moveables = new List<int>();
            int stackbreaked = PlayerNo == 1 ? P1Breaked : P2Breaked;
            if (HasBreaked(PlayerNo) && selectedStack != stackbreaked) //kırığı varsa seçilen stack kırılmış taşların toplandığı stack değilse boş döner
                return newstates;
            if (!HasStamps(selectedStack)) //seçilen stacte taş yok ise ..
                return newstates;

            if (stacks[selectedStack].playerNo != PlayerNo) //seçilen stack oyuncuya ait değilse ...
                return newstates;

            //Zar ile gidebileceği stackler hesaplanır kontroller yapılır ve oynanabilecekler listesine eklenir State olarak kaydedilir
            if (PlayerNo == 1)
            {
                if (selectedStack == 26)
                {
                    selectedStack = 25;
                    isStackChanged = true;
                }
                for (int i = selectedStack - 1; i >= 1; i--)
                {

                    int target = i;
                    if (!IsMoveableWithDice(selectedStack, target, dice))
                        continue;

                    if (!HasStamps(target))
                        moveables.Add(target);
                    if (HasStamps(target) && stacks[target].playerNo == PlayerNo)
                        moveables.Add(target);
                    if (HasStamps(target) && CanBreak(selectedStack, target))
                        moveables.Add(target);

                }
                if (CanMoveToCollection(1) && IsMoveableWithDice(selectedStack, P1Collection, dice))
                    moveables.Add(P1Collection);
                if (isStackChanged)
                    selectedStack = 26;
            }
            else
            {
                if (selectedStack == 27)
                {
                    selectedStack = 0;
                    isStackChanged = true;
                }
                for (int i = selectedStack + 1; i <= 24; i++)
                {
                    int target = i;
                    if (!IsMoveableWithDice(selectedStack, target, dice))
                        continue;
                    if (!HasStamps(target))
                        moveables.Add(target);
                    if (HasStamps(target) && stacks[target].playerNo == PlayerNo)
                        moveables.Add(target);
                    if (HasStamps(target) && CanBreak(selectedStack, target))
                        moveables.Add(target);

                }
                if (CanMoveToCollection(2) && IsMoveableWithDice(selectedStack, P2Collection, dice))
                    moveables.Add(P2Collection);

                if (isStackChanged)
                    selectedStack = 27;

            }
            //Verilen zarlar içerisinde 1. ile hamle yapılır ikinci zar için oynanabilecekler oynanır ve stateler kaydedilir
            foreach (var target in moveables)
            {
                GameState s = DeepCopy();

                s.MoveStamp(selectedStack, target);
                newstates.Add(s);
            }

            return newstates;
        }
        //Durum değerlendirme fonksiyonu 
        public float GetEval(int PlayerNo)
        {
            int doorcount = stacks.Where(x => x.playerNo == PlayerNo && x.stampCount > 1).Count(); // kapı sayısı
            int opencount = stacks.Where(x => x.playerNo == PlayerNo && x.stampCount == 1).Count();// açık taş sayısı
            int index = PlayerNo == 1 ? 26 : 27;
            int breaked = stacks[index].stampCount; //kırık taşlar
            int no = (PlayerNo == 1) ? 2 : 1;
            int oppHomeCount = 0; //rakibin evinde bulunan taşlar
            int myHomeCount = 0; //evinde bulunan taşlar
            //kendi bölgesinde bulunan taş sayıları hesaplanır
            if (PlayerNo == 1)
            {
                for (int i = 24; i <= 18; i--)
                {
                    if (stacks[i].playerNo == 2 && stacks[i].stampCount > 1)
                    {
                        oppHomeCount += stacks[i].stampCount;
                    }
                }

                for (int i = 1; i <= 6; i++)
                {
                    if (stacks[i].playerNo == 1 && stacks[i].stampCount > 1)
                    {
                        myHomeCount += stacks[i].stampCount;
                    }
                }

            }
            else
            {
                for (int i = 1; i <= 6; i++)
                {
                    if (stacks[i].playerNo == 1 && stacks[i].stampCount > 1)
                    {
                        oppHomeCount += stacks[i].stampCount;
                    }
                }
                for (int i = 24; i <= 18; i--)
                {
                    if (stacks[i].playerNo == 2 && stacks[i].stampCount>1)
                    {
                        myHomeCount += stacks[i].stampCount;
                    }
                }
            }
            int oppdoorcount = stacks.Where(x => x.playerNo == no && x.stampCount > 1).Count(); //rakip kapı sayıları
            int oppOpenCount = stacks.Where(x => x.playerNo == no && x.stampCount == 1).Count(); //rakip açık taş sayısı
            int index2 = no == 1 ? 26 : 27; 
            int oppBrekead = stacks[index].stampCount; // rakip kırık taşları
            int oppCollection = stacks[0].stampCount; //rakibin toplamış olduğu taş sayısı
            int homeCollection = stacks[25].stampCount; //kendi toplamış olduğu taş sayısı
            float distanceSum = 0; // dışarıdaki taşların bölgeye uzaklığı
            for (int i = 1; i < 25; i++)
            {
                if (PlayerNo == 1 && stacks[i].playerNo == 1)
                {
                    distanceSum += stacks[i].stampCount * i;
                }
                else if (PlayerNo == 2 && stacks[i].playerNo == 2)
                {
                    distanceSum += stacks[i].stampCount * (25 - i);
                }
            }
            return -(oppdoorcount*2.0f) - (.5f * breaked) + (doorcount*2.0f) - opencount + oppOpenCount + (0.5f * oppBrekead) -oppHomeCount+myHomeCount+homeCollection-oppCollection;
        }


    }
}
