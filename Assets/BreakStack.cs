using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.EventSystems;
[Serializable]
public class BreakStack : Stack
{
    public override void OnPointerClick(PointerEventData eventData)
    {
        board.OnStackClicked(this);
    }
}
