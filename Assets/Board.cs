
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class Board : MonoBehaviour
{
    public List<Stack> Stacks;
    public Stack P1StampsStack;
    public Stack P2StampsStack;


    GameManager gameManager;
    public void Init(GameManager gameManager)
    {
        this.gameManager = gameManager;
        foreach (var stack in Stacks)
        {
            stack.Init(this);
        }
    }

    public void OnStackClicked(Stack stack)
    {
        gameManager.OnStackSelected(stack);
    }

}
