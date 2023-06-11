using System.Collections.Generic;

public class GameState
{

   //Number of columns and rows
   public const int Rows = 8;
   public const int Cols = 8;

   //Stores what is in each position on the board
   public Player[,] Board { get; private set; }

   // the Player is the key and value is the number of disks that player's color facing up
   public Dictionary<Player, int> DiskCount {get; private set;}

   //Which player's turn is it
   public Player CurrentPlayer {get; set;}
   

   //Which Moves the Current player can make
   public Dictionary<Position, List<Position>> LegalMoves {get; private set;}

   //Specifies whether the game is over or not
   public bool GameOver {get; private set;}

   //Specifies which player is the winner
   public Player winner {get; private set;}


   public GameState()
   {
        Board = new Player[Rows, Cols];
        Board[3,3] = Player.White;
        Board[3,4] = Player.Black;
        Board[4,3] = Player.Black;
        Board[4,4] = Player.White;

        DiskCount = new Dictionary<Player, int>(){

            {Player.Black, 2},
            {Player.White, 2},
            {Player.None, 0}

        };

        CurrentPlayer = Player.Black;
        LegalMoves = FindLegalMoves(CurrentPlayer);
   }

   public bool MakeMove(Position pos, out MovementInfo moveinfo)
   {
        if(!LegalMoves.ContainsKey(pos))
        {
            moveinfo = null;
            return false;
        }

        Player movePlayer = CurrentPlayer;
        List<Position> outflanked = LegalMoves[pos];
        Board[pos.Row, pos.Col] = movePlayer;

        FlipDisks(outflanked);
        UpdateDiskCount(movePlayer, outflanked.Count);
        PassTurn();

        moveinfo = new MovementInfo { Player = movePlayer, Position = pos, Outflanked = outflanked};
        return true;
   }

   public IEnumerable<Position> OccupiedPositions(){

        for(int r = 0; r < Rows; r++) {

            for(int c = 0; c < Cols ; c++) {

                if(Board[r,c] != Player.None){

                    yield return new Position(r, c); 

                }
            }  
        }
   } 

   private void FlipDisks (List<Position> positions){

        foreach (Position pos in positions)
        {
            Board[pos.Row, pos.Col] = Board[pos.Row, pos.Col].Opponent();
        }

   }

   private void UpdateDiskCount (Player movePlayer, int outflankedCount) {

        DiskCount[movePlayer] += outflankedCount + 1;
        DiskCount[movePlayer.Opponent()] -= outflankedCount;
    
   }

   public void ChangePlayer () {

        CurrentPlayer = CurrentPlayer.Opponent();
        LegalMoves = FindLegalMoves(CurrentPlayer);

   }

   private Player FindWinner () {

        if(DiskCount[Player.Black] > DiskCount[Player.White]){
            return Player.Black;
        }

        if(DiskCount[Player.Black] < DiskCount[Player.White]){
            return Player.White;
        }

        return Player.None;
    
   }

   private void PassTurn () {

        ChangePlayer();

        if(LegalMoves.Count > 0){
            return;
        }

        ChangePlayer();

        if(LegalMoves.Count == 0){
            CurrentPlayer = Player.None;
            GameOver = true;
            winner = FindWinner();
        }
 
   }

    private bool IsInsideBoard(int r, int c)
    {
        return r>= 0 && r < Rows && c>= 0 && c < Cols;
    }

   private List<Position> OutFlankedDirected(Position pos, Player plr, int rDelta, int cDelta) 
   {
        List<Position> outflanked = new List<Position>();
        int r = pos.Row + rDelta;
        int c = pos.Col + cDelta;

        while (IsInsideBoard(r,c) && Board[r, c] != Player.None){

            if (Board[r, c] == plr.Opponent())
            {
                outflanked.Add(new Position(r, c));
                r += rDelta;
                c += cDelta;
            }
            else {return outflanked;}
        }

        return new List<Position>();
   }

    private List<Position> Outflanked(Position pos, Player plr){

        List<Position> outflanked = new List<Position>();

        for (int rDelta = -1; rDelta <= 1; rDelta++){

            for (int cDelta = -1; cDelta <= 1; cDelta++){

                if(rDelta == 0 && cDelta == 0)
                {
                    continue;
                }

                outflanked.AddRange(OutFlankedDirected(pos, plr, rDelta, cDelta));

            }
        }
        return outflanked;
   }

   private bool IsMoveLegal(Player plr, Position pos, out List<Position> outflanked)
   {

    if(Board[pos.Row, pos.Col] != Player.None){
        outflanked = null;
        return false;
    }
    outflanked = Outflanked(pos, plr);
    return outflanked.Count > 0;
   }

   private Dictionary<Position, List<Position>> FindLegalMoves(Player plr){

    Dictionary<Position, List<Position>> legalMoves = new Dictionary<Position, List<Position>>();

    for(int r = 0; r < Rows; r++) {
        for(int c = 0; c < Cols; c++) {

            Position pos = new Position(r,c);

            if(IsMoveLegal(plr, pos, out List<Position> outflanked)) {
                legalMoves[pos] = outflanked;
            }    
        } 
    }

    return legalMoves;
   }

    public GameState Clone()
    {
        GameState clonedState = new GameState();

        // Copy the board
        clonedState.Board = (Player[,])Board.Clone();

        // Copy the disk count
        clonedState.DiskCount = new Dictionary<Player, int>(DiskCount);

        // Copy the current player
        clonedState.CurrentPlayer = CurrentPlayer;

        // Copy the legal moves
        clonedState.LegalMoves = new Dictionary<Position, List<Position>>();
        foreach (KeyValuePair<Position, List<Position>> kvp in LegalMoves)
        {
            Position key = kvp.Key;
            List<Position> value = kvp.Value;
            clonedState.LegalMoves.Add(key, new List<Position>(value));
        }

        // Copy the game over and winner status
        clonedState.GameOver = GameOver;
        clonedState.winner = winner;

        return clonedState;
    }

}
