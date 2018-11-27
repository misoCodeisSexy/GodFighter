using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;
using GodFighter;
using Rewired;

[System.Serializable]
public class ButtonReference
{
    public InputType inputType;
    public ButtonPress buttonPressType;
    public string inputButtonName;

    public string inputPositiveKeyName;
    public string inputNegativeKeyName;

    public string inputPositiveAltKeyName;
    public string inputNegativeAltKeyName;

    public Texture2D inputViewerIcon1;
    public Texture2D inputViewerIcon2;
    public Texture2D activeIconImg;
}

// miso // 2017.08.30 add script
public class ButtonController : MonoBehaviour {
    
    public ReadOnlyCollection<ButtonReference> buttons { get; set; }
    public int player_number;
    
    public void OnInitialize(IEnumerable<ButtonReference> inputs, int bufferSize = 1) 
    {
        List<ButtonReference> buttonList = new List<ButtonReference>();

        if(inputs != null)
        { 
            foreach (ButtonReference button in inputs)
            {
                if(button != null)
                {
                    buttonList.Add(button);
                }
            }
        }
        this.buttons = new ReadOnlyCollection<ButtonReference>(buttonList);
    }

    public float GetAxisRaw(ButtonReference buttonReference, Player rewiredPlayer)
    {
        return rewiredPlayer.GetAxisRaw(buttonReference.inputPositiveKeyName);
    }

    public bool GetButton(ButtonReference buttonReference, Player rewiredPlayer)
    {
        return rewiredPlayer.GetButton(buttonReference.inputPositiveKeyName);
    }

    public bool GetButtonDown(ButtonReference buttonReference, Player rewiredPlayer)
    {
        return rewiredPlayer.GetButtonDown(buttonReference.inputPositiveKeyName);
    }

    public bool GetButtonUp(ButtonReference buttonReference, Player rewiredPlayer)
    {
        return rewiredPlayer.GetButtonUp(buttonReference.inputPositiveKeyName);
    }
}


