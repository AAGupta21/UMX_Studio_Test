using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Vector3 PlayerSpawnPoint = Vector3.zero;
    [SerializeField] private GameObject PlayerPrefab = null;
    [SerializeField] private float PlayerTokenPlacementOffset = 0.3f;
    [SerializeField] private float PlayerMoveDurationRate = 0.25f;

    [SerializeField] private int Min_Dice_Roll = 1;
    [SerializeField] private int Max_Dice_Roll = 6;

    private int NumOfPlayers = 2;

    private int[] RollSaveArray = null;
    private int[] CurrRollArray = null;
    private int[] PlayerPositionsArray = null;
    private List<GameObject> ListOfPlayerCharactor = null;

    public delegate void OnTurnExpend();
    public event OnTurnExpend OnInitialMoveMade;
    public event OnTurnExpend OnTurnFinalized;

    private void Start()
    {
        RollSaveArray = new int[4];
        CurrRollArray = new int[4];
        PlayerPositionsArray = new int[4];
        ListOfPlayerCharactor = new List<GameObject>();
    }

    public void SetNumOfPlayers(int num)
    {
        NumOfPlayers = num;
    }

    public void Initialize()
    {
        if(ListOfPlayerCharactor.Count == 0)
        {
            for (int i = 0; i < 4; i++)
            {
                ListOfPlayerCharactor.Add(Instantiate(PlayerPrefab, PlayerSpawnPoint, Quaternion.identity, transform));
                PlayerPositionsArray[i] = 0;

                if (i >= NumOfPlayers)
                {
                    ListOfPlayerCharactor[i].SetActive(false);
                }
            }

            ColorCodePlayerTokens();
        }
        else
        {
            for(int i = 0; i < NumOfPlayers; i++)
            {
                PlayerTokenPositionAdjust(i);
                ListOfPlayerCharactor[i].SetActive(true);
            }
        }

        OnTurnFinalized?.Invoke();
    }

    public void StopPlay()
    {
        for(int i = 0; i < NumOfPlayers; i++)
        {
            ListOfPlayerCharactor[i].SetActive(false);
            ListOfPlayerCharactor[i].transform.position = PlayerSpawnPoint;
            PlayerPositionsArray[i] = RollSaveArray[i] = CurrRollArray[i] = 0;
        }
    }

    public int Roll_Dice(int PlayerIndex)
    {
        int val = Random.Range(Min_Dice_Roll, Max_Dice_Roll + 1);
        
        if (val == 6 && CurrRollArray[PlayerIndex] == 6)
        {
            return -1;
        }
        else
        {
            CurrRollArray[PlayerIndex] = val;
            return val;
        }
    }

    public int Ret_Curr_Roll_Saved(int PlayerIndex)
    {
        return RollSaveArray[PlayerIndex];
    }

    public int Ret_Player_Pos(int PlayerIndex)
    {
        return PlayerPositionsArray[PlayerIndex];
    }

    public void SaveDiceRoll(int PlayerIndex)
    {
        RollSaveArray[PlayerIndex] = CurrRollArray[PlayerIndex];
    }

    public int RetPlayerRoll(int PlayerIndex)
    {
        return CurrRollArray[PlayerIndex];
    }

    public void EmptySavedRoll(int PlayerIndex)
    {
        RollSaveArray[PlayerIndex] = 0;
    }

    public void MovePlayer(int PlayerIndex, int MoveAmt, Vector3[] nPos)          // NormalMovement
    {
        PlayerPositionsArray[PlayerIndex] += MoveAmt;

        for(int i = 0; i < nPos.Length; i++)
        {
            nPos[i] += AdjustmentToPos(PlayerIndex);
        }

        StartCoroutine(Movement_normal(nPos, PlayerIndex, MoveAmt));

        //some delay.
        
    }

    public float ChangePlayerPos(int PlayerIndex, int NewIndexPos, Vector3 IniPos, Vector3 fPos)            //Pit or Shortcut movement.
    {
        PlayerPositionsArray[PlayerIndex] = NewIndexPos;

        IniPos += AdjustmentToPos(PlayerIndex);
        fPos += AdjustmentToPos(PlayerIndex);

        float duration = PlayerMoveDurationRate * (Vector3.Distance(fPos, IniPos));

        StartCoroutine(Movement_Cuts(IniPos, fPos, PlayerIndex, duration));

        return duration;
    }

    private IEnumerator Movement_normal(Vector3[] Pos, int PlayerIndex, int moves)
    {
        for (int i = 0; i < moves - 1; i++)
        {
            float currTime = 0f;

            Vector3 iniPos = Pos[i];
            Vector3 finPos = Pos[i + 1];

            while (currTime < PlayerMoveDurationRate)
            {
                ListOfPlayerCharactor[PlayerIndex].transform.position = Vector3.Lerp(iniPos, finPos, currTime / PlayerMoveDurationRate);

                yield return new WaitForSeconds(0.1f);
                currTime += 0.1f;
            }

            ListOfPlayerCharactor[PlayerIndex].transform.position = finPos;
        }

        ListOfPlayerCharactor[PlayerIndex].transform.position = Pos[moves - 1];

        OnInitialMoveMade?.Invoke();
    }

    private IEnumerator Movement_Cuts(Vector3 iniPos, Vector3 fPos, int PlayerIndex, float duration)
    {
        float currTime = 0f;

        while (currTime < duration)
        {
            ListOfPlayerCharactor[PlayerIndex].transform.position = Vector3.Lerp(iniPos, fPos, currTime / duration);

            yield return new WaitForSeconds(0.1f);
            currTime += 0.1f;
        }

        ListOfPlayerCharactor[PlayerIndex].transform.position = fPos;

        OnTurnFinalized?.Invoke();
    }

    private void ColorCodePlayerTokens()
    {
        ListOfPlayerCharactor[0].GetComponent<MeshRenderer>().material.color = Color.red;
        PlayerTokenPositionAdjust(0);

        ListOfPlayerCharactor[1].GetComponent<MeshRenderer>().material.color = Color.green;
        PlayerTokenPositionAdjust(1);

        if (ListOfPlayerCharactor[2].activeInHierarchy)
        {
            ListOfPlayerCharactor[2].GetComponent<MeshRenderer>().material.color = Color.blue;
            PlayerTokenPositionAdjust(2);
        }
        if (ListOfPlayerCharactor[3].activeInHierarchy)
        {
            ListOfPlayerCharactor[3].GetComponent<MeshRenderer>().material.color = Color.yellow;
            PlayerTokenPositionAdjust(3);
        }
    }

    private void PlayerTokenPositionAdjust(int num)
    {
        switch (num)
        {
            case 0:
                {
                    ListOfPlayerCharactor[0].transform.position += new Vector3(-PlayerTokenPlacementOffset, PlayerTokenPlacementOffset);
                    break;
                }
            case 1:
                {
                    ListOfPlayerCharactor[1].transform.position += new Vector3(PlayerTokenPlacementOffset, PlayerTokenPlacementOffset);
                    break;
                }
            case 2:
                {
                    ListOfPlayerCharactor[2].transform.position += new Vector3(-PlayerTokenPlacementOffset, -PlayerTokenPlacementOffset);
                    break;
                }
            case 3:
                {
                    ListOfPlayerCharactor[3].transform.position += new Vector3(PlayerTokenPlacementOffset, -PlayerTokenPlacementOffset);
                    break;
                }
        }
    }

    private Vector3 AdjustmentToPos(int num)
    {
        switch (num)
        {
            case 0:
                {
                    return new Vector3(-PlayerTokenPlacementOffset, PlayerTokenPlacementOffset, PlayerSpawnPoint.z);
                }
            case 1:
                {
                    return new Vector3(PlayerTokenPlacementOffset, PlayerTokenPlacementOffset, PlayerSpawnPoint.z);
                }
            case 2:
                {
                    return new Vector3(-PlayerTokenPlacementOffset, -PlayerTokenPlacementOffset, PlayerSpawnPoint.z);
                }
            case 3:
                {
                    return new Vector3(PlayerTokenPlacementOffset, -PlayerTokenPlacementOffset, PlayerSpawnPoint.z);
                }
        }

        return Vector3.zero;
    }
}