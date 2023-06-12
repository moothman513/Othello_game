public class Position
{
    public int  Row {get; }
    public int  Col {get; }


    public Position (int row, int colmn){

        Row = row;
        Col = colmn;

    }

    public override bool Equals(object a){

        if (a is Position other){

            return Row == other.Row && Col == other.Col;
        }

        return false;
    }

    public override int GetHashCode(){
        return 8 * Row + Col;
    }
}
