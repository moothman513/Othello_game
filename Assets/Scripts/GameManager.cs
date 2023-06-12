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
    
}
