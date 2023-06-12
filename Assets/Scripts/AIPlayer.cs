using System.Collections.Generic;
using UnityEngine;

public class MinMaxAIPlayer
{
    public Player playerType;
    private int maxDepth = 3;

    public MinMaxAIPlayer(Player playerType)
    {
        this.playerType = playerType;
    }

    public Position MakeMove(GameState gameState)
    {
        List<Position> legalMoves = new List<Position>(gameState.LegalMoves.Keys);
        int bestScore = int.MinValue;
        Position bestMove = null;

        foreach (Position move in legalMoves)
        {

            GameState clonedState = gameState.Clone();
            clonedState.MakeMove(move, out MovementInfo moveInfo);

            int score = Minimax(clonedState, 0, int.MinValue, int.MaxValue, false);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        return bestMove;
    }

    private int Minimax(GameState gameState, int depth, int alpha, int beta, bool isMaximizingPlayer)
    {
        // Base case: evaluate the game state if maximum depth is reached or the game is over
        if (depth == maxDepth || gameState.GameOver)
        {
            return EvaluateGameState(gameState);
        }

        // Recursive case
        List<Position> legalMoves = new List<Position>(gameState.LegalMoves.Keys);
        int bestScore;

        if (isMaximizingPlayer)
        {
            bestScore = int.MinValue;

            foreach (Position move in legalMoves)
            {
                GameState clonedState = gameState.Clone();
                clonedState.MakeMove(move, out MovementInfo moveInfo);

                int score = Minimax(clonedState, depth + 1, alpha, beta, false);
                bestScore = Mathf.Max(bestScore, score);
                alpha = Mathf.Max(alpha, bestScore);

            if (beta <= alpha){

                break; // Beta Cutoff
                
            }
            }
        }
        else
        {
            bestScore = int.MaxValue;

            foreach (Position move in legalMoves)
            {
                GameState clonedState = gameState.Clone();
                clonedState.MakeMove(move, out MovementInfo moveInfo);

                int score = Minimax(clonedState, depth + 1, alpha, beta, true);
                bestScore = Mathf.Min(bestScore, score);
                beta = Mathf.Min(beta, bestScore);

                if (beta <= alpha)
                    break; // Alpha cutoff
            }
        }

        return bestScore;
    }

}
