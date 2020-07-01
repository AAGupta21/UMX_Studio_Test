using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject MainMenu = null;
    [SerializeField] private GameObject NumOfPlayers = null;
    [SerializeField] private GameObject Play = null;
    [SerializeField] private GameObject Victory = null;

    [SerializeField] private Text CurrPlayerText = null;
    [SerializeField] private Text SavedRollText = null;
    [SerializeField] private Text CurrRollText = null;
    [SerializeField] private Text DoubleSixInfoText = null;

    [SerializeField] private Button Skip_Button = null;
    [SerializeField] private Button Roll_Button = null;
    [SerializeField] private Button Move_Button = null;

    [SerializeField] private Text VictoryScreenInfoText = null;
    public float DelayScreenDuration = 5f;
    
    public void PreRollPlayUIUpdate(int PlayerIndex, int SavedRoll)
    {
        CurrPlayerText.gameObject.SetActive(true);
        CurrPlayerText.text = "PLAYER " + (PlayerIndex + 1).ToString() + "'s TURN";
        Roll_Button.gameObject.SetActive(true);

        if (SavedRoll == 0)
        {
            SavedRollText.gameObject.SetActive(false);
        }
        else
        {
            SavedRollText.gameObject.SetActive(true);
            SavedRollText.text = SavedRoll.ToString();
        }
    }

    public void RollEffectUIUpdate()
    {
        Roll_Button.gameObject.SetActive(false);
        CurrRollText.gameObject.SetActive(true);
    }

    public void UpdateCurrRollText(int roll)
    {
        CurrRollText.text = roll.ToString();
    }

    public void PostRollButtonPressed(int Roll, int SavedRoll)
    {
        CurrRollText.text = Roll.ToString();

        if(SavedRoll == 0)
        {
            Skip_Button.gameObject.SetActive(true);
        }
        else
        {
            Skip_Button.gameObject.SetActive(false);
        }

        Move_Button.gameObject.SetActive(true);
    }

    public void PostMovePlayUIUpdate(int roll)
    {
        CurrRollText.text = roll.ToString();

        Skip_Button.gameObject.SetActive(false);
        Move_Button.gameObject.SetActive(false);

        SavedRollText.gameObject.SetActive(false);
    }

    public void SkipTurnEvent(int roll)
    {
        Skip_Button.gameObject.SetActive(false);
        Move_Button.gameObject.SetActive(false);

        SavedRollText.gameObject.SetActive(true);
        SavedRollText.text = roll.ToString();
    }

    public void DoubleSixEvent()
    {
        DeactivateAllButtonsAndTextInPlay();
        DoubleSixInfoText.gameObject.SetActive(true);
        CurrRollText.text = "6";
        StartCoroutine(DelayInDoubleText());
    }

    private IEnumerator DelayInDoubleText()
    {
        yield return new WaitForSeconds(DelayScreenDuration * 0.5f);
        DoubleSixEvenEnd();
    }

    public void DoubleSixEvenEnd()
    {
        DoubleSixInfoText.gameObject.SetActive(false);
    }

    public  void DeactivateAllButtonsAndTextInPlay()
    {
        CurrPlayerText.gameObject.SetActive(false);
        SavedRollText.gameObject.SetActive(false);
        DoubleSixInfoText.gameObject.SetActive(false);

        Skip_Button.gameObject.SetActive(false);
        Roll_Button.gameObject.SetActive(false);
        Move_Button.gameObject.SetActive(false);
    }

    public void PlayActivate()
    {
        DeactivateAllButtonsAndTextInPlay();
        DeactivateAllScenes();
        Play.SetActive(true);
    }

    public void NumOfPlayers_Screen()
    {
        DeactivateAllScenes();
        NumOfPlayers.SetActive(true);
    }
    
    public void MainMenuActivate()
    {
        DeactivateAllScenes();
        MainMenu.SetActive(true);
    }

    public void VictoryActivate(int PlayerIndex, Color col)
    {
        VictoryScreenInfoText.text = "PLAYER " + (PlayerIndex + 1).ToString() + " WON!";

        DeactivateAllScenes();
        Victory.SetActive(true);

        StartCoroutine(Delay(col));
    }

    private IEnumerator Delay(Color bgCol)
    {
        yield return new WaitForSeconds(DelayScreenDuration);
        Camera.main.backgroundColor = bgCol;
        MainMenuActivate();
    }

    private void DeactivateAllScenes()
    {
        if(MainMenu.activeInHierarchy)
        {
            MainMenu.SetActive(false);
        }

        if(NumOfPlayers.activeInHierarchy)
        {
            NumOfPlayers.SetActive(false);
        }
        
        if (Play.activeInHierarchy)
        {
            Play.SetActive(false);
        }

        if (Victory.activeInHierarchy)
        {
            Victory.SetActive(false);
        }
    }
}