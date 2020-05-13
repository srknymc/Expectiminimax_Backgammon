using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetsAI;

namespace Assets
{
    public class Expectiminimax
    {
        private GameManager gm; // oyun bilgilerinin bulunduğu gamemaneger objesi
        public enum Node // ağaç içerisinde bulunan düğümler
        {
            Max,
            Min,
            Chance
        }
        public void Init(GameManager gm) // gamemaneger init
        {
            this.gm = gm;
        }

        public Node[] Nodes = new Node[] { Node.Max, Node.Chance, Node.Min, Node.Chance }; // ağacın temsili mod 4 işlemi ile dönülüyor max node undan başlanır ve sırayla ilerlenir.
        private readonly int DefaultDepth = 3;
        public int InitDepth = -1;
        public Dictionary<GameState, double> MoveQuality; //hareketler ve expectiminimaxtan gelen değerlerin saklandığı yapı
        GameState tempState;

        //Kullanılmıyor GameState içindeki Eval() fonksiyonu kullanılıyor.
        //public double HeuristicValue(int NodeIndex, int depth)
        //{
        //    int StampsCountOnBoard = gm.GetBoardStampsCount(tempState, 2);
        //    int StampsCountOnHome = gm.GetHomeStampsCount(tempState, 2);

        //    int OppStampsountOnBoard = gm.GetBoardStampsCount(tempState, 1);
        //    int OppStampsCountOnHome = gm.GetHomeStampsCount(tempState, 1);

        //    int ComputerBlockCount = gm.GetBlockCount(tempState, 2);
        //    int OppBlockCount = gm.GetBlockCount(tempState, 1);

        //    if (Nodes[NodeIndex] == Node.Max)
        //    {
        //        //return ComputerBlockCount;
        //        return HeuristicCoeff * (OppStampsountOnBoard + (0.7 * StampsCountOnHome) - StampsCountOnBoard - OppStampsCountOnHome + (ComputerBlockCount * 1.20));
        //    }
        //    else
        //    {
        //        //return -OppBlockCount;
        //        return -HeuristicCoeff * (StampsCountOnBoard + (0.7 * OppStampsCountOnHome) - OppStampsountOnBoard - StampsCountOnHome + (1.20 * OppBlockCount));
        //    }
        //}
        public double GetDiceProb((int, int) DicePair) //Zarların olasılıkları herhangi bir zar ikilisinin gelmesi 1/36 ; aynı ikilinin gelmesi 2/36
        {
            return (DicePair.Item1 == DicePair.Item2) ? (1.0 / 36.0) : (2.0 / 36.0);
        } 
        public double expectiminimax(GameState gameState, int NodeIndex, int depth) //algoritmanın ana mantığını barındıran fonksiyon
        {
            double result = 0.0; // return değer için

            if (depth == 0) // derinlik sona ulaştıysa değerlendirme fonksiyonu çağır ve değeri döndür
            {
                if (Nodes[NodeIndex - 1] == Node.Max) //Bilgisayar oyuncusu içinse
                {
                   return gameState.GetEval(2);
                }
                else //humanplayer
                {
                    return -1.0 * gameState.GetEval(1);
                }
            }
            Dictionary<Stack, List<Stack>> moves;
            List<GameState> possibleMoves = new List<GameState>(); ; // zarlar için olası tüm durumların saklandığı  yapı
            InitDepth = (InitDepth == -1) ? depth : InitDepth; // ilk çağrıda maxdan değerler dönmesi için
            switch (Nodes[NodeIndex]) //gelen node üzerinden değerlendirme yapılır
            {
                case Node.Max: // max ise bilgisayar oyuncusunun hamlelerine bakılır
                    possibleMoves = gameState.GetAllMovements(2, gameState.dice[0], gameState.dice[1]); // olası tüm hamleler GameState objesinden istenir.

                    if (possibleMoves != null && possibleMoves.Count > 0) // hamleler listesi boş değil ise
                    {
                        Dictionary<GameState, double> MoveQuality = new Dictionary<GameState, double>(); // State ve puanı yapısını oluştur.
                        foreach (GameState state in possibleMoves) //olası tüm hamleleri gez
                        {
                            //tempState = state;
                            MoveQuality.Add(state, expectiminimax(state, (NodeIndex + 1) % 4, depth - 1)); // her hamlenin puanını kendini çağırarak aşağıya doğru hesapla
                        }
                        if (depth == InitDepth) // ilk çağrı için max değerleri döndür.
                        {
                            this.MoveQuality = new Dictionary<GameState, double>();
                            this.MoveQuality = MoveQuality;
                        }
                        result = MoveQuality.Values.Max();
                    }
                    else
                    {
                        if (depth == InitDepth) // hamle yok ise ve ilk çağrı ise boş liste döndür result olarak kötü ..
                        {
                            MoveQuality = new Dictionary<GameState, double>();
                        }
                        result = -0.9;
                    }

                    break;
                case Node.Min: // Min düğümünde ise
                    possibleMoves = gameState.GetAllMovements(1, gameState.dice[0], gameState.dice[1]); // verilen zarlardan hamlelerini oluştur.
                    if (possibleMoves != null && possibleMoves.Count > 0)
                    {
                        Dictionary<GameState, double> MoveQuality = new Dictionary<GameState, double>();
                        foreach (GameState state in possibleMoves) //hamleleri dön
                        {
                            tempState = state;
                            MoveQuality.Add(state, expectiminimax(state, (NodeIndex + 1) % 4, depth - 1)); //listeye ekle değerlerini hesaplayıp
                        }
                        result = MoveQuality.Values.Min(); //min değeri return et

                    }
                    else
                    {
                        result = 0.9;
                    }

                    break;
                case Node.Chance:

                        List<double> funcValues = new List<double>(); //fonksiyon değerlerini tutan liste
                        var DicePair = DiceManager.DicePairs; // olası tüm zarların listesi
                        foreach ((int, int) pair in DicePair) // her zar ikilisi için değerlere bak stateleri oluştur ve puanları kaydet
                        {

                            var copy = gameState.DeepCopy();
                            copy.dice[0] = pair.Item1;
                            copy.dice[1] = pair.Item2;
                            funcValues.Add(expectiminimax(copy, (NodeIndex + 1) % 4, depth - 1)); // her zar ikilisini değerlendir.
                            /*  if (Nodes[(NodeIndex + 1) % 4] == Node.Max)
                              {
                                  gm.SetPossibleDices(pair, 2);
                              }
                              else
                                  gm.SetPossibleDices(pair, 1);*/
                        }
                    //if (Nodes[NodeIndex - 1] == Node.Max)
                    //{
                    //    result = gameState.GetEval(2);
                    //}
                    //else
                    //{
                    //    result = -1.0 * gameState.GetEval(1);
                    //}
                         double weightedSum = 0.0; // her zarı ihtimali ile çarp ve genel ağırlığı result olarak set et
                        int i = 0;
                        foreach (double x in funcValues)
                        {
                            double prob = GetDiceProb(DicePair[i++]);
                            weightedSum += x * prob;
                        }
                        result = weightedSum;
                    
                    break;
            }
            return result;
        }
        public GameState FindBestMove()
        {
            InitDepth = -1; //ilk çağrı için
            expectiminimax(gm.GetBoardData(),0, 3); //şuanki tahta durumunu başlangıç düğümünü ve derinliği gönder
            if (MoveQuality != null ) //Eğer hareketler ve puanları boş değilse 
                return MoveQuality.Aggregate((l, r) => l.Value > r.Value ? l : r).Key; // Maksimum puana sahip State i return et. Return sonucu State tahtaya set edilir.
            return null;
        }
    }
}
