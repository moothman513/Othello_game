using System.Collections.Generic;
using UnityEngine;

public class AIPlayer
{
    public Player playerType;
    private System.Random random;

    public AIPlayer(Player playerType)
    {
        this.playerType = playerType;
        random = new System.Random();
    }

    public Position MakeMove(GameState gameState)
    {
        List<Position> legalMoves = new List<Position>(gameState.LegalMoves.Keys);
        int randomIndex = random.Next(legalMoves.Count);
        Position selectedMove = legalMoves[randomIndex];
        return selectedMove;
    }
}






