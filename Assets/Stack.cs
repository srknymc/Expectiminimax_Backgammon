using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Stack : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private Image image;

    [SerializeField]
    private Color hightLightColor;

    [SerializeField]
    private Color defaultColor;


    public int StackID;
    public List<Stamp> Stamps;

    protected Board board;

    public bool HasStamps { get { return Stamps.Count > 0; } }
    public int GetPlayerNo { get { return Stamps[0].PlayerNo; } }

    public bool IsEnemyPlayer(Stamp stamp)
    {
        return stamp.PlayerNo != GetPlayerNo;
    }

    //kırabilir
    public bool CanBreak(Stack stack)
    {
        return IsEnemyPlayer(stack.Stamps[0]) && Stamps.Count == 1;
    }

    public void Init(Board board)
    {
        this.board = board;
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        board.OnStackClicked(this);
    }

    public void Highlight(bool On)
    {
        if(On)
        {
            image.color = hightLightColor;
        }
        else
        {
            image.color = defaultColor;
        }
    }

    public int GetDistance(Stack other)
    {
        return Math.Abs(other.StackID - this.StackID);
    }

    public void Move1Stamp(Stack other)
    {
        if(other.HasStamps)
        {
            var stamp = other.Stamps[0];
            stamp.gameObject.transform.parent = transform;
            other.Stamps.Remove(stamp);
            Stamps.Add(stamp);
        }
    }

   
}
