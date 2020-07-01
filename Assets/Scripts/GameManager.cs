using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] FieldGenerator fg = null;
    [SerializeField] PlayerController pc = null;
    [SerializeField] UIManager um = null;
    [SerializeField] EffectsManager em = null;

    [SerializeField] private float RollEffectDuration = 1f;

    private int Num_Of_Players = 2;

    private int Curr_Turn = 0;

    private void Start()
    {
        fg.BoardHasBeenBuildHandler += BoardIsReady;
        pc.OnInitialMoveMade += CheckForCases;
        pc.OnTurnFinalized += PlayerIsReady;

        um.MainMenuActivate();
    }

    public void NumberOfPlayers(int n)
    {
        Num_Of_Players = n;
        Curr_Turn = -1;

        um.PlayActivate();
        fg.Initialize();
    }

    public void OnPlayButtonPressed()
    {
        um.NumOfPlayers_Screen();
    }

    public void OnRollButtonPressed()
    {
        int val = pc.Roll_Dice(Curr_Turn);
        um.RollEffectUIUpdate();

        StartCoroutine(RollingEffect(val));
    }

    private IEnumerator RollingEffect(int val)
    {
        float currTime = 0f;

        while(currTime < RollEffectDuration)
        {
            um.UpdateCurrRollText(Random.Range(1, 7));

            yield return new WaitForSeconds(0.1f);
            currTime += 0.1f;
        }

        if (val < 0)
        {
            um.DoubleSixEvent();
            StartCoroutine(DelayInDoubleEventEnd());
        }
        else
        {
            um.PostRollButtonPressed(val, pc.Ret_Curr_Roll_Saved(Curr_Turn));
        }
    }

    private IEnumerator DelayInDoubleEventEnd()
    {
        yield return new WaitForSeconds(um.DelayScreenDuration * 0.5f);
        PlayerIsReady();
    }

    public void OnSkipTurnButtonPressed()
    {
        pc.SaveDiceRoll(Curr_Turn);
        um.SkipTurnEvent(pc.RetPlayerRoll(Curr_Turn));
        StartCoroutine(DelayBeforeReady());
    }

    private IEnumerator DelayBeforeReady()
    {
        yield return new WaitForSeconds(1f);
        PlayerIsReady();
    }

    public void OnMoveButtonPressed()
    {
        int move_val = pc.RetPlayerRoll(Curr_Turn) + pc.Ret_Curr_Roll_Saved(Curr_Turn);
        Vector3[] Pos = new Vector3[move_val];

        if (move_val + pc.Ret_Player_Pos(Curr_Turn) > fg.RetVictorySquareIndex())
        {
            move_val = fg.RetVictorySquareIndex() - pc.Ret_Player_Pos(Curr_Turn);
        }
        um.PostMovePlayUIUpdate(move_val);

        if(pc.Ret_Curr_Roll_Saved(Curr_Turn) != 0)
        {
            pc.EmptySavedRoll(Curr_Turn);
        }

        for(int i = 0; i < move_val; i++)
        {
            Pos[i] = fg.RetSquarePos(pc.Ret_Player_Pos(Curr_Turn) + i + 1);
        }

        pc.MovePlayer(Curr_Turn, move_val, Pos);
    }

    private void PlayerIsReady()
    {
        Curr_Turn = (Curr_Turn + 1) % Num_Of_Players;
        em.ChangeBgColor(Curr_Turn);

        um.PreRollPlayUIUpdate(Curr_Turn, pc.Ret_Curr_Roll_Saved(Curr_Turn));
    }

    private void CheckForCases()
    {
        if(pc.Ret_Player_Pos(Curr_Turn) == fg.RetVictorySquareIndex())
        {
            VictoryScreen();
        }
        else
        {
            if(fg.CheckForPlayerOnInteractableSquare(pc.Ret_Player_Pos(Curr_Turn), out int nIndex, out Vector3 OriPos, out Vector3 DestPos))
            {
                em.CameraShake(pc.ChangePlayerPos(Curr_Turn, nIndex, OriPos, DestPos));
            }
            else
            {
                PlayerIsReady();
            }
        }
    }

    private void VictoryScreen()
    {
        pc.StopPlay();
        fg.StopPlay();
        um.VictoryActivate(Curr_Turn, em.DefaultColor);
    }
    
    private void BoardIsReady()
    {
        pc.SetNumOfPlayers(Num_Of_Players);
        pc.Initialize();
    }

    public void HomeButtonPressed()
    {
        StopAllCoroutines();
        pc.StopAllCoroutines();
        fg.StopAllCoroutines();
        um.StopAllCoroutines();

        pc.StopPlay();
        fg.StopPlay();

        um.MainMenuActivate();
    }
    
    public void OnQuitButtonPressed()
    {
        Application.Quit();
    }

    private void OnDestroy()
    {
        fg.BoardHasBeenBuildHandler -= BoardIsReady;
        pc.OnInitialMoveMade -= PlayerIsReady;
        pc.OnTurnFinalized -= CheckForCases;
    }
}