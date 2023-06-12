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
        private int EvaluateGameState(GameState gameState)
    {
        int mobilityScore = EvaluateMobility(gameState);
        int coinParityScore = EvaluateCoinParity(gameState);
        int cornersScore = EvaluateCornersCaptured(gameState);
        int stabilityScore = EvaluateStability(gameState);
        int patternScore = EvaluatePatternRecognition(gameState);

        // Weighted sum of the heuristic scores
        int totalScore = 3*mobilityScore + 2*coinParityScore + 10*cornersScore + 10*stabilityScore; + 5*patternScore;
        return totalScore;
    }

    private int EvaluateMobility(GameState gameState)
    {
        List<Position> legalMoves = new List<Position>(gameState.LegalMoves.Keys);
        int currentPlayerMoves = legalMoves.Count;

        // Count the opponent's legal moves
        Player opponentPlayer = GetOpponentPlayer(gameState.CurrentPlayer);
        gameState.ChangePlayer();
        List<Position> opponentLegalMoves = new List<Position>(gameState.LegalMoves.Keys);
        int opponentMoves = opponentLegalMoves.Count;
        gameState.ChangePlayer();

        // Calculate the mobility score as the difference in the number of moves
        int mobilityScore = currentPlayerMoves - opponentMoves;

        return mobilityScore;
    }

    private int EvaluateCoinParity(GameState gameState)
    {
        int currentPlayerDisks = gameState.DiskCount[gameState.CurrentPlayer];
        int opponentPlayerDisks = gameState.DiskCount[GetOpponentPlayer(gameState.CurrentPlayer)];

        // Calculate the coin parity score as the difference in the number of disks
        int coinParityScore = currentPlayerDisks - opponentPlayerDisks;

        return coinParityScore;
    }

    private int EvaluateCornersCaptured(GameState gameState)
    {
        int currentPlayerCorners = CountCornersCaptured(gameState, gameState.CurrentPlayer);
        int opponentPlayerCorners = CountCornersCaptured(gameState, GetOpponentPlayer(gameState.CurrentPlayer));

        // Calculate the corners captured score as the difference in the number of corners captured
        int cornersScore = currentPlayerCorners - opponentPlayerCorners;

        return cornersScore;
    }

    private int CountCornersCaptured(GameState gameState, Player player)
    {
        int cornersCaptured = 0;

        if (gameState.Board[0, 0] == player)
            cornersCaptured++;
        if (gameState.Board[0, GameState.Cols - 1] == player)
            cornersCaptured++;
        if (gameState.Board[GameState.Rows - 1, 0] == player)
            cornersCaptured++;
        if (gameState.Board[GameState.Rows - 1, GameState.Cols - 1] == player)
            cornersCaptured++;

        return cornersCaptured;
    }

    private Player GetOpponentPlayer(Player player)
    {
        return player == Player.Black ? Player.White : Player.Black;
    }

    private int EvaluateStability(GameState gameState)
    {
        int stabilityScore = 0;

        // Define stable disk positions (corners and edges)
        List<Position> stablePositions = new List<Position>
        {
            new Position(0, 0), new Position(0, GameState.Cols - 1),
            new Position(GameState.Rows - 1, 0), new Position(GameState.Rows - 1, GameState.Cols - 1)
        };

        // Evaluate stability for each disk
        foreach (Position position in gameState.OccupiedPositions())
        {
            Player player = gameState.Board[position.Row, position.Col];

            if (player == gameState.CurrentPlayer)
            {
                int stability = 1;

                // Check if the disk is on the edge
                if (position.Row == 0 || position.Row == GameState.Rows - 1 ||
                    position.Col == 0 || position.Col == GameState.Cols - 1)
                {
                    stability++;
                }

                // Check if the disk is in a stable position
                if (stablePositions.Contains(position))
                {
                    stability += 3;
                }

                stabilityScore += stability;
            }
        }

        return stabilityScore;
    }

    private int EvaluatePatternRecognition(GameState gameState)
    {
        int patternScore = 0;

        // Define patterns and their corresponding scores
        Dictionary<string, int> patterns = new Dictionary<string, int>
        {
            { "X.X", 5 },   // Pattern with two AI player's disks and an empty space in between
            { "XX.", 10 },  // Pattern with two AI player's disks and an empty space at the end
            { ".XX", 10 },  // Pattern with two AI player's disks and an empty space at the beginning
            { "XXX", 20 },  // Pattern with three AI player's disks in a row
            { "XXXX", 30 }, // Pattern with four AI player's disks in a row
        };

        // Adjust pattern scores for capturing corners
        if (gameState.Board[0, 0] == gameState.CurrentPlayer)
            patternScore += 100;  // Capture top-left corner
        if (gameState.Board[0, GameState.Cols - 1] == gameState.CurrentPlayer)
            patternScore += 100;  // Capture top-right corner
        if (gameState.Board[GameState.Rows - 1, 0] == gameState.CurrentPlayer)
            patternScore += 100;  // Capture bottom-left corner
        if (gameState.Board[GameState.Rows - 1, GameState.Cols - 1] == gameState.CurrentPlayer)
            patternScore += 100;  // Capture bottom-right corner

        // Check horizontal patterns
        for (int row = 0; row < GameState.Rows; row++)
        {
            string rowString = "";
            for (int col = 0; col < GameState.Cols; col++)
            {
                rowString += gameState.Board[row, col] == gameState.CurrentPlayer ? "X" : ".";
            }
            patternScore += GetPatternScore(rowString, patterns);
        }

        // Check vertical patterns
        for (int col = 0; col < GameState.Cols; col++)
        {
            string colString = "";
            for (int row = 0; row < GameState.Rows; row++)
            {
                colString += gameState.Board[row, col] == gameState.CurrentPlayer ? "X" : ".";
            }
            patternScore += GetPatternScore(colString, patterns);
        }

        // Check diagonal patterns (top-left to bottom-right)
        for (int startRow = 0; startRow < GameState.Rows; startRow++)
        {
            string diagonalString = "";
            int row = startRow;
            int col = 0;
            while (row < GameState.Rows && col < GameState.Cols)
            {
                diagonalString += gameState.Board[row, col] == gameState.CurrentPlayer ? "X" : ".";
                row++;
                col++;
            }
            patternScore += GetPatternScore(diagonalString, patterns);
        }

        // Check diagonal patterns (top-right to bottom-left)
        for (int startRow = 0; startRow < GameState.Rows; startRow++)
        {
            string diagonalString = "";
            int row = startRow;
            int col = GameState.Cols - 1;
            while (row < GameState.Rows && col >= 0)
            {
                diagonalString += gameState.Board[row, col] == gameState.CurrentPlayer ? "X" : ".";
                row++;
                col--;
            }
            patternScore += GetPatternScore(diagonalString, patterns);
        }

        return patternScore;
    }

    private int GetPatternScore(string pattern, Dictionary<string, int> patterns)
    {
        int score = 0;
        foreach (KeyValuePair<string, int> kvp in patterns)
        {
            if (pattern.Contains(kvp.Key))
            {
                score += kvp.Value;
            }
        }
        return score;
    }

}
