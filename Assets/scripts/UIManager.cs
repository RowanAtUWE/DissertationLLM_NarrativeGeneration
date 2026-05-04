using UnityEngine;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{
    /// <summary>
    /// Manages player input mode selection (Do, Say, Ask) through the UI.
    ///
    /// This allows the player to define the type of interaction they are making,
    /// which is then included in the LLM prompt to influence how the storyteller responds.
    /// </summary>

    public Button actionButton;

    public Button doButton;
    public Button sayButton;
    public Button askButton;

    public GameObject actionButtonGroup;

    public TextMeshProUGUI actionButtonText;

    private TextMeshProUGUI chosenButtonText;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowActionButtons()
    {
        if (actionButtonGroup.activeSelf)
        {
            actionButtonGroup.SetActive(false);
        }
        else
        {
            actionButtonGroup.SetActive(true);
        }
    }

    public void SetActionButtonText(Button clickedButton)
    {
        chosenButtonText = clickedButton.GetComponentInChildren<TextMeshProUGUI>();

        actionButtonText.text = chosenButtonText.text;

        actionButtonGroup.SetActive(false);
    }
}
