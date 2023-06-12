using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropDownSelector : MonoBehaviour
{

    public static int gameMode {get; set;}
    public TMPro.TMP_Dropdown myDrop;

    public void DropDownManager(){

        if(myDrop.value == 0) gameMode = 0;
        else if(myDrop.value == 1) gameMode = 1;
        else if(myDrop.value == 2) gameMode = 2;
        Debug.Log(gameMode);

    }

}
