using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldGenerator : MonoBehaviour
{
    [SerializeField] private GameObject[] ArrayOfSquares = null;

    [SerializeField] private GameObject EmptySquare = null;
    [SerializeField] private GameObject ArrowPointerDown_Prefab = null;
    [SerializeField] private GameObject ArrowPointerUp_Prefab = null;
    
    [SerializeField] private Vector3 IniPos = Vector3.zero;

    [SerializeField] private float Square_Offset_x = 0.2f;
    [SerializeField] private float Square_Offset_y = 0.2f;
    [SerializeField] private float Square_Offset_Only = 0.2f;
    [SerializeField] private Vector2Int BoardDimensions = Vector2Int.zero;
    
    [SerializeField] private float AnimationLength = 0.05f;

    [SerializeField] private int NumOfShortcuts = 2;
    [SerializeField] private Color ShortcutSquare_Color = Color.green;

    [SerializeField] private int NumOfPits = 2;
    [SerializeField] private Color PitSquare_Color = Color.red;

    private List<GameObject> ArrowPointerlist = null;   
    private int[] ArrayOfSpecialSquares_Pits_entry = null;                    //The entry and exit squares of the shortcuts and pits.
    private int[] ArrayOfSpecialSquares_Pits_exit = null;

    private List<int> ArrayOfColoredSquares_Pits = null;                      //All sqaures that are part of the shortcut and pits.(for colour coding/ Making sure they dont clash purposes)

    private int[] ArrayOfSpecialSquares_Shortcuts_entry = null;
    private int[] ArrayOfSpecialSquares_Shortcuts_exit = null;

    private List<int> ArrayOfColoredSquares_Shortcuts = null;

    public delegate void BoardBuilt();

    public event BoardBuilt BoardHasBeenBuildHandler;

    private int MaxSquares = 0;

    private void Start()
    {
        MaxSquares = BoardDimensions.x * BoardDimensions.y;

        ArrayOfSquares = new GameObject[MaxSquares];

        ArrowPointerlist = new List<GameObject>();
        ArrayOfSpecialSquares_Pits_entry = new int[NumOfPits];
        ArrayOfSpecialSquares_Pits_exit = new int[NumOfPits];

        ArrayOfSpecialSquares_Shortcuts_entry = new int[NumOfShortcuts];
        ArrayOfSpecialSquares_Shortcuts_exit = new int[NumOfShortcuts];

        ArrayOfColoredSquares_Pits = new List<int>();
        ArrayOfColoredSquares_Shortcuts = new List<int>();
    }

    public void Initialize()
    {
        BuildBoard();
    }

    public void StopPlay()
    {
        ArrayOfColoredSquares_Pits.Clear();
        ArrayOfColoredSquares_Shortcuts.Clear();
        
        for(int i = 0; i < MaxSquares; i++)
        {
            ArrayOfSquares[i].SetActive(false);
            ArrayOfSquares[i].GetComponent<MeshRenderer>().material.color = Color.white;
        }

        foreach(GameObject g in ArrowPointerlist)
        {
            Destroy(g);
        }
    }

    private void BuildBoard()
    {
        float final_pos_x = Square_Offset_x * BoardDimensions.x - 1 - Square_Offset_Only;

        if(ArrayOfSquares[0] != null)
        {
            StartCoroutine(PlaceSquares());
        }
        else
        {
            for (int i = 0; i < BoardDimensions.y; i++)
            {
                for (int j = 0; j < BoardDimensions.x; j++)
                {
                    if (i % 2 == 0)
                    {
                        GameObject g = Instantiate(EmptySquare, new Vector3(IniPos.x + j * Square_Offset_x, IniPos.y + i * Square_Offset_y, 0f), Quaternion.identity, transform);
                        g.SetActive(false);
                        ArrayOfSquares[i * BoardDimensions.x + j] = g;
                    }
                    else
                    {
                        GameObject g = Instantiate(EmptySquare, new Vector3(final_pos_x - j * Square_Offset_x, IniPos.y + i * Square_Offset_y, 0f), Quaternion.identity, transform);
                        g.SetActive(false);
                        ArrayOfSquares[i * BoardDimensions.x + j] = g;
                    }
                }
            }

            StartCoroutine(PlaceSquares());
        }

    }

    private IEnumerator PlaceSquares()
    {
        for(int i = 0; i < MaxSquares; i++)
        {
            float currTime = 0f;
            
            Color c = ArrayOfSquares[i].GetComponent<MeshRenderer>().material.color;

            Color b = c;
            b.a = 0f;

            ArrayOfSquares[i].GetComponent<MeshRenderer>().material.color = b;

            ArrayOfSquares[i].SetActive(true);

            while (currTime < AnimationLength)
            {
                ArrayOfSquares[i].GetComponent<MeshRenderer>().material.color = Color.Lerp(b, c, currTime / AnimationLength);

                currTime += 0.01f;
                yield return new WaitForSeconds(0.01f);
            }
            
            ArrayOfSquares[i].GetComponent<MeshRenderer>().material.color = c;
        }

        GenerateSpecialSquares();
    }

    private void GenerateSpecialSquares()
    {
        for(int i = 0; i < NumOfPits; i++)
        {
            Vector2Int IniPoint = new Vector2Int(Random.Range(0, BoardDimensions.x), Random.Range(1, BoardDimensions.y));
            
            Vector2Int DropPoint = IniPoint - new Vector2Int(0, Random.Range(1, IniPoint.y + 1));

            int IniPoint_Index = Vector2IndexConvertor(IniPoint);
            int DropPoint_Index = Vector2IndexConvertor(DropPoint);
            int MidPoint_Index = Vector2IndexConvertor(new Vector2Int(IniPoint.x, IniPoint.y - Mathf.Abs(IniPoint.y - DropPoint.y)/ 2));

            if (!CheckForClash(IniPoint_Index) && !CheckForClash(DropPoint_Index) && !CheckForClash(MidPoint_Index))
            {
                ArrayOfSpecialSquares_Pits_entry[i] = IniPoint_Index;
                ArrayOfSpecialSquares_Pits_exit[i] = DropPoint_Index;

                for (int j = IniPoint.y; j >= DropPoint.y; j--)
                {
                    ArrayOfColoredSquares_Pits.Add(Vector2IndexConvertor(new Vector2Int(IniPoint.x, j)));
                }
            }
            else
            {
                i--;
            }
        }

        for (int i = 0; i < NumOfShortcuts; i++)
        {
            Vector2Int IniPoint = new Vector2Int(Random.Range(0, BoardDimensions.x), Random.Range(0, BoardDimensions.y - 1));

            Vector2Int DropPoint = IniPoint + new Vector2Int(0, Random.Range(1, (BoardDimensions.y - IniPoint.y)));
            
            int IniPoint_Index = Vector2IndexConvertor(IniPoint);
            int DropPoint_Index = Vector2IndexConvertor(DropPoint);
            int MidPoint_Index = Vector2IndexConvertor(new Vector2Int(IniPoint.x, IniPoint.y + Mathf.Abs(IniPoint.y - DropPoint.y) / 2));

            if (!CheckForClash(IniPoint_Index) && !CheckForClash(DropPoint_Index) && !CheckForClash(MidPoint_Index))
            {
                ArrayOfSpecialSquares_Shortcuts_entry[i] = IniPoint_Index;
                ArrayOfSpecialSquares_Shortcuts_exit[i] = DropPoint_Index;

                for (int j = IniPoint.y; j <= DropPoint.y; j++)
                {
                    ArrayOfColoredSquares_Shortcuts.Add(Vector2IndexConvertor(new Vector2Int(IniPoint.x, j)));
                }
            }
            else
            {
                i--;
            }
        }

        ColorSpecialSquares();
    }

    private int Vector2IndexConvertor(Vector2Int point)
    {
        int val = point.x + point.y * BoardDimensions.x;

        if (point.y % 2 == 0)
        {
            return val;
        }
        else
        {
            int nval = (val / BoardDimensions.x + 1) * BoardDimensions.x - 1 - val % BoardDimensions.x;
            return nval;
        }
    }

    private bool CheckForClash(int point)
    {
        if (point == 0 || point == (MaxSquares - 1) || ArrayOfColoredSquares_Pits.Contains(point) || ArrayOfColoredSquares_Shortcuts.Contains(point))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void ColorSpecialSquares()
    {
        StartCoroutine(ColorSpecialSquaresWithDelay());
    }

    private IEnumerator ColorSpecialSquaresWithDelay()
    {
        foreach (int i in ArrayOfColoredSquares_Pits)
        {
            float currTime = 0f;
            
            Color a = ArrayOfSquares[i].GetComponent<MeshRenderer>().material.color;

            Color b = PitSquare_Color;
            
            while (currTime < AnimationLength)
            {
                ArrayOfSquares[i].GetComponent<MeshRenderer>().material.color = Color.Lerp(a, b, currTime / AnimationLength);

                currTime += 0.01f;
                yield return new WaitForSeconds(0.01f);
            }
            
            ArrayOfSquares[i].GetComponent<MeshRenderer>().material.color = b;
        }

        foreach (int i in ArrayOfColoredSquares_Shortcuts)
        {
            float currTime = 0f;

            Color a = ArrayOfSquares[i].GetComponent<MeshRenderer>().material.color;

            Color b = ShortcutSquare_Color;

            while (currTime < AnimationLength)
            {
                ArrayOfSquares[i].GetComponent<MeshRenderer>().material.color = Color.Lerp(a, b, currTime / AnimationLength);

                currTime += 0.01f;
                yield return new WaitForSeconds(0.01f);
            }

            ArrayOfSquares[i].GetComponent<MeshRenderer>().material.color = b;
        }

        BuildArrow();

        BoardHasBeenBuildHandler?.Invoke();
    }

    private void BuildArrow()
    {
        for(int i = 0; i < NumOfPits; i++)
        {
            Vector3 pos = ArrayOfSquares[ArrayOfSpecialSquares_Pits_entry[i]].transform.position;
            pos.z = -0.5f;

            GameObject g = Instantiate(ArrowPointerDown_Prefab, pos, Quaternion.identity, transform);

            ArrowPointerlist.Add(g);
        }
        
        for (int i = 0; i < NumOfShortcuts; i++)
        {
            Vector3 pos = ArrayOfSquares[ArrayOfSpecialSquares_Shortcuts_entry[i]].transform.position;
            pos.z = -0.5f;

            GameObject g = Instantiate(ArrowPointerUp_Prefab, pos, Quaternion.identity, transform);

            ArrowPointerlist.Add(g);
        }
    }

    public Vector3 RetSquarePos(int index)
    {
        return ArrayOfSquares[index].transform.position;
    }

    public int RetVictorySquareIndex()
    {
        return MaxSquares - 1;
    }

    public bool CheckForPlayerOnInteractableSquare(int index, out int newIndex, out Vector3 ori, out Vector3 dest)
    {
        if(CheckForInteractableSquare(index, out Vector3 val, out int ni))
        {
            newIndex = ni;
            ori = ArrayOfSquares[index].transform.position;
            dest = val;
            return true;
        }
        else
        {
            newIndex = 0;
            dest = ori = val;
            return false;
        }
    }

    private bool CheckForInteractableSquare(int index, out Vector3 val, out int nIndex)
    {
        val = Vector3.zero;
        nIndex = 0;

        for(int i = 0; i < NumOfPits; i++)
        {
            if(ArrayOfSpecialSquares_Pits_entry[i] == index)
            {
                nIndex = ArrayOfSpecialSquares_Pits_exit[i];
                val = ArrayOfSquares[nIndex].transform.position;
                return true;
            }
        }

        for(int i = 0; i < NumOfShortcuts; i++)
        {
            if(ArrayOfSpecialSquares_Shortcuts_entry[i] == index)
            {
                nIndex = ArrayOfSpecialSquares_Shortcuts_exit[i];
                val = ArrayOfSquares[nIndex].transform.position;
                return true;
            }
        }

        return false;
    }
}