using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    [SerializeField]
    private Camera cam;

    [SerializeField]
    private LayerMask boardLayer;

    [SerializeField]
    private DiskScript DiskBlackUp;

    [SerializeField]
    private DiskScript DiskWhiteUp;

    [SerializeField]
    private GameObject hlPrefab;

    [SerializeField]
    private UIManager uiManager;



    private Dictionary<Player, DiskScript> discPrefabs = new Dictionary<Player, DiskScript>();

    private AIPlayer dummyAiPlayer;
    private MinMaxAIPlayer hardAiPlayer;
    private GameState gameState = new GameState();
    private DiskScript[,] disks = new DiskScript[8,8];
    private bool CanMove = true;
    private List<GameObject> highlights = new List<GameObject>();
    

    // Start is called before the first frame update
    private void Start()
    {

        discPrefabs[Player.Black] = DiskBlackUp;
        discPrefabs[Player.White] = DiskWhiteUp;

        AddStartDisks();
        GameModeSelector();
        ShowLegalMoves();
        uiManager.SetPlayerTXT(gameState.CurrentPlayer);
        
    }

    // Update is called once per frame
    private void Update()
    {

        if(Input.GetKeyDown(KeyCode.Escape)){

            Application.Quit();
        
        }

        if(Input.GetMouseButtonDown(0)){

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out RaycastHit hitInfo)){

                Vector3 impact = hitInfo.point;
                Position boardPos = SceneToBoardPos(impact);
                OnBoardClicked(boardPos);
            }
        }
    }

    public void GameModeSelector () {

        
        if(DropDownSelector.gameMode == 1){
            dummyAiPlayer = new AIPlayer(Player.White);
        }
        else if(DropDownSelector.gameMode == 2){
            hardAiPlayer = new MinMaxAIPlayer(Player.White);
        }
        
    }

    private void ShowLegalMoves(){

        foreach(Position boardPos in gameState.LegalMoves.Keys){
            Vector3 scenePos = BoardToScenePos(boardPos) + Vector3.up * 0.01f;
            GameObject Highlight = Instantiate(hlPrefab, scenePos, Quaternion.identity);
            highlights.Add(Highlight);
        }
    }

    private void HideLegalMoves(){

        highlights.ForEach(Destroy);
        highlights.Clear();
    }

    private void OnBoardClicked (Position boardPos) {

        if(!CanMove){
            return;
        }

        if (gameState.MakeMove(boardPos, out MovementInfo moveInfo)){
            
            StartCoroutine(OnMoveMade(moveInfo));
        }    
    }

    private IEnumerator OnMoveMade(MovementInfo moveInfo){

        CanMove = false;
        HideLegalMoves();
        yield return ShowMove(moveInfo);
        yield return ShowTurnOutcome(moveInfo);

        if (hardAiPlayer != null && gameState.CurrentPlayer == hardAiPlayer.playerType)
        {
            // AI player's turn
            Position aiMove = hardAiPlayer.MakeMove(gameState);
            yield return new WaitForSeconds(1f); // Delay for visual effect

            if (gameState.MakeMove(aiMove, out MovementInfo aiMoveInfo))
            {
                yield return OnMoveMade(aiMoveInfo);
            }
        }
        if (dummyAiPlayer != null && gameState.CurrentPlayer == dummyAiPlayer.playerType)
        {
            // AI player's turn
            Position aiMove = dummyAiPlayer.MakeMove(gameState);
            yield return new WaitForSeconds(1f); // Delay for visual effect

            if (gameState.MakeMove(aiMove, out MovementInfo aiMoveInfo))
            {
                yield return OnMoveMade(aiMoveInfo);
            }
        }
        ShowLegalMoves();
        CanMove = true;

    }
        private Position SceneToBoardPos(Vector3 scenepos) {

        int col = (int)(scenepos.x - 0.25f);
        int row = 7 - (int)(scenepos.z - 0.25f);

        return new Position(row, col);
        
    }

    private Vector3 BoardToScenePos(Position boardPos) 
    {
        return new Vector3(boardPos.Col + 0.75f, 0, 7 - boardPos.Row + 0.75f);

    }

    private void SpawnDisk( DiskScript prefab, Position boardPos) {

        Vector3 scenePos = BoardToScenePos(boardPos) + Vector3.up * 0.2f;
        disks[boardPos.Row, boardPos.Col] = Instantiate(prefab, scenePos, Quaternion.identity);
        
    }
    

    private void AddStartDisks () {

        foreach(Position boardPos in gameState.OccupiedPositions()) 
        {

            Player player = gameState.Board[boardPos.Row, boardPos.Col];
            SpawnDisk(discPrefabs[player], boardPos);
            
        }
        
    }

    private void FlipDisks(List<Position> positions){

        foreach(Position boardPos in positions){
            disks[boardPos.Row, boardPos.Col].Flip();
        }
    }

    private IEnumerator ShowMove(MovementInfo moveInfo){

        SpawnDisk(discPrefabs[moveInfo.Player], moveInfo.Position);
        yield return new WaitForSeconds(0.33f);
        FlipDisks(moveInfo.Outflanked);
        yield return new WaitForSeconds(0.83f);
    }

    private IEnumerator ShowTurnSkipped(Player SkippedPlayer){

        uiManager.SetSkippedTXT(SkippedPlayer);
        yield return uiManager.AnimateTopText();
    }

    public IEnumerator ShowGameOver(Player winner){

        uiManager.SetTopText("Both Players Can't Move");
        yield return uiManager.AnimateTopText();

        yield return uiManager.ShowScoreTxt();
        yield return new WaitForSeconds(0.5f);

        yield return ShowCount();

        uiManager.SetWinnerText(winner);
        yield return uiManager.ShowEndScreen();

    }

    private IEnumerator ShowTurnOutcome(MovementInfo moveInfo){

        if(gameState.GameOver){
            yield return ShowGameOver(gameState.winner);
            yield break;
        }

        Player currentPlayer = gameState.CurrentPlayer;

        if(currentPlayer == moveInfo.Player){
            yield return ShowTurnSkipped(currentPlayer.Opponent());
        }

        uiManager.SetPlayerTXT(currentPlayer);
    }

    private IEnumerator ShowCount(){

        int black = 0, white = 0;

        foreach(Position pos in gameState.OccupiedPositions()){

            Player player = gameState.Board[pos.Row, pos.Col];

            if(player == Player.Black){
                black++;
                uiManager.SetBlackScoreTxt(black);
            }

            if(player == Player.White){
                white++;
                uiManager.SetWhiteScoreTxt(white);
            }

            disks[pos.Row, pos.Col].Twitch();
            yield return new WaitForSeconds(0.05f);
        }
    }


    public IEnumerator RestartGame(){

        yield return uiManager.HideEndScreen();
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name);
    }

    public void OnPlayAgianClicked(){

        StartCoroutine(RestartGame());

    }

    public void OnReturnButtonClicked () {
        SceneManager.LoadScene("Main Menu");
    }
    
}
