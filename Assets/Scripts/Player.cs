//This enum is used to store which player turn it is and contents of each position on the board

public enum Player
{

    None, Black, White

}

public static class PlayerExtend {

    public static Player Opponent(this Player player){

        if (player == Player.Black){
            return Player.White;
        }
        else if (player == Player.White){
            return Player.Black;
        }
        return Player.None;
    }
}
