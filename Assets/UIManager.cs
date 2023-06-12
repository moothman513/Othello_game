using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{

    [SerializeField]
    private TextMeshProUGUI toptext;

    [SerializeField]
    private TextMeshProUGUI BlackScoreUp;

    [SerializeField]
    private TextMeshProUGUI WhiteScoreUp;

    [SerializeField]
    private TextMeshProUGUI WinnerTXT;

    [SerializeField]
    private RawImage overlayImg;

    [SerializeField]
    private RectTransform playAgainButton;

    [SerializeField]
    private RectTransform returnToMainButton;

    public void SetPlayerTXT(Player CurrentPlayer){

        if(CurrentPlayer == Player.Black){

            toptext.text = "Black's Turn <sprite name=DiskBlackUp>";
        }

        if(CurrentPlayer == Player.White){

            toptext.text = "White's Turn <sprite name=DiskWhiteUp>";
        }

    }

    public void SetSkippedTXT(Player SkippedPlayer){

        if(SkippedPlayer == Player.Black){

            toptext.text = "Black can't move <sprite name=DiskBlackUp>";
        }

        if(SkippedPlayer == Player.White){

            toptext.text = "White can't move <sprite name=DiskWhiteUp>";
        }

    }

    public IEnumerator AnimateTopText(){

        toptext.transform.LeanScale(Vector3.one *1.2f, 0.25f).setLoopPingPong(4);
        yield return new WaitForSeconds(2);

    }

    public void SetTopText(string message){

        toptext.text = message;

    }

    public IEnumerator ScaleDown(RectTransform rect){

        rect.LeanScale(Vector3.zero, 0.2f);
        yield return new WaitForSeconds(0.2f);
        rect.gameObject.SetActive(false);

    }

    public IEnumerator ScaleUp(RectTransform rect){

        rect.gameObject.SetActive(true);
        rect.localScale = Vector3.one;
        rect.LeanScale(Vector3.one, 0.2f);
        yield return new WaitForSeconds(0.2f);

    }

    public IEnumerator ShowScoreTxt(){
        
        yield return ScaleDown(toptext.rectTransform);
        yield return ScaleUp(BlackScoreUp.rectTransform);
        yield return ScaleUp(WhiteScoreUp.rectTransform);

    }

    public void SetBlackScoreTxt(int score){
        BlackScoreUp.text = $"<sprite name=DiskBlackUp> {score}";
    }

    public void SetWhiteScoreTxt(int score){
        WhiteScoreUp.text = $"<sprite name=DiskWhiteUp> {score}";
    }

    private IEnumerator ShowOverlay(){

        overlayImg.gameObject.SetActive(true);
        //overlayImg.color = color.clear;
        overlayImg.rectTransform.LeanAlpha(0.8f, 1);
        yield return new WaitForSeconds(1);
    }

    private IEnumerator HideOverlay(){

        overlayImg.rectTransform.LeanAlpha(0, 1);
        yield return new WaitForSeconds(1);
        overlayImg.gameObject.SetActive(false);
    }

    private IEnumerator MoveScoreDown(){

        BlackScoreUp.rectTransform.LeanMoveY(0, 0.5f);
        WhiteScoreUp.rectTransform.LeanMoveY(0, 0.5f);
        yield return new WaitForSeconds(0.5f);
    }

    public void SetWinnerText(Player winner){

        switch(winner){

            case Player.Black:
                WinnerTXT.text = "BLACK WON";
                break;
            case Player.White:
                WinnerTXT.text = "WHITE WON";
                break;
            case Player.None:
                WinnerTXT.text = "IT'S A TIE";
                break;
        }
    }

    public IEnumerator ShowEndScreen(){

        yield return ShowOverlay();
        yield return MoveScoreDown();
        yield return ScaleUp(WinnerTXT.rectTransform);
        yield return ScaleUp(playAgainButton);
        yield return ScaleUp(returnToMainButton);
    }

    public IEnumerator HideEndScreen(){

        StartCoroutine(ScaleDown(WinnerTXT.rectTransform));
        StartCoroutine(ScaleDown(BlackScoreUp.rectTransform));
        StartCoroutine(ScaleDown(WhiteScoreUp.rectTransform));
        StartCoroutine(ScaleDown(playAgainButton));
        StartCoroutine(ScaleDown(returnToMainButton));

        yield return new WaitForSeconds(0.5f);
        yield return HideOverlay();
    }

}
