using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class DiceManager : MonoBehaviour, IPointerClickHandler
{

    private GameManager gameManager;

    [SerializeField]
    public Text dice1;
    [SerializeField]
    public Text dice2;

    public static List<(int,int)> DicePairs = new List<(int, int)>
        {
            (1,1),
            (1,2),
            (1,3),
            (1,4),
            (1,5),
            (1,6),
            (2,1),
            (2,2),
            (2,3),
            (2,4),
            (2,5),
            (2,6),
            (3,1),
            (3,2),
            (3,3),
            (3,4),
            (3,5),
            (3,6),
            (4,1),
            (4,2),
            (4,3),
            (4,4),
            (4,5),
            (4,6),
            (5,1),
            (5,2),
            (5,3),
            (5,4),
            (5,5),
            (5,6),
            (6,1),
            (6,2),
            (6,3),
            (6,4),
            (6,5),
            (6,6)

        };


    public void Init(GameManager gameManager)
    {
        this.gameManager = gameManager;
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        gameManager.OnDiceClick();
    }

    public List<int> Throw()
    {
        var list = new List<int>();
        System.Random rnd = new System.Random();
        var a = rnd.Next(1, 7); var b = rnd.Next(1, 7);
        list.Add(a);
        list.Add(b);
        //if (a==b)
        //{
        //    list.Add(a);
        //    list.Add(b);
        //}
        dice1.text = a.ToString();
        dice2.text = b.ToString();
        return list;

    }
}
