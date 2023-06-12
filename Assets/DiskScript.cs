using UnityEngine;

public class DiskScript : MonoBehaviour
{
    [SerializeField]
    private Player up;

    private Animator animator;

    // Start is called before the first frame update
    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void Flip() {

        if (up == Player.Black){

            animator.Play("Blacktowhite");
            up = Player.White;
        }
        else {

            animator.Play("Whitetoblack");
            up = Player.Black;

        }
        
    }

    public void Twitch () {

        animator.Play("Twitch");
        
    }
}
