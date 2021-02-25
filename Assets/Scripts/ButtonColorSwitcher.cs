using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class ButtonColorSwitcher : MonoBehaviour
{
    // Start is called before the first frame update
    private void ChangeColor(Color color)
    {
        GetComponent<MeshRenderer>().material.color = color;
    }

    [SerializeField] private Color colorOff;
    [SerializeField] private Color colorOn;

    private bool recording = false;
    
    public void SwitchColor()
    {

        if (!recording)
        {
            ChangeColor(colorOn);
            recording = !recording;
        }
        else
        {
            ChangeColor(colorOff);
            recording = !recording;
        }
        
    }
}
