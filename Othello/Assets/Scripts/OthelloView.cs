using System;
using UnityEngine;

public class OthelloView : MonoBehaviour
{
    [SerializeField]
    private GameObject _cell = null;
    public GameObject Cell
    {
        get
        {
            if (_cell == null)
            {
                _cell = Resources.Load<GameObject>("Cell/Cell");
            }
            return _cell;
        }
    }

    [SerializeField]
    private GameObject _stone = null;
    public GameObject Stone
    {
        get
        {
            if (_stone == null)
            {
                _stone = Resources.Load<GameObject>("Stone/Stone");
            }
            return _stone;
        }
    }

    [SerializeField]
    private Material _normalMaterial = null;
    public Material NormalMaterial
    {
        get
        {
            if (_normalMaterial == null)
            {
                _normalMaterial = Resources.Load<Material>("Cell/NormalMaterial");
            }
            return _normalMaterial;
        }
    }

    [SerializeField]
    private Material _selectedMaterial = null;
    public Material SelectedMaterial
    {
        get
        {
            if (_selectedMaterial == null)
            {
                _selectedMaterial = Resources.Load<Material>("Cell/SelectedMaterial");
            }
            return _selectedMaterial;
        }
    }

    private const float CellMargin = 1.1F;

    private GameObject[,] _cells = new GameObject[8, 8];
    private GameObject[,] _stones = new GameObject[8, 8];

    void Start ()
    {
        for(var r = 0; r < _cells.GetLength(0); r++)
        {
            for(var c = 0; c < _cells.GetLength(1); c++)
            {
                _cells[r, c] = CreateCell(r, c);
            }
        }
	}

    public int SelectedRow { get; set; }
    public int SelectedColumn { get; set; }

    private CellState _currentCellState = CellState.White;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) { SelectedRow--; }
        if (Input.GetKeyDown(KeyCode.DownArrow)) { SelectedRow++; }
        if (Input.GetKeyDown(KeyCode.LeftArrow)) { SelectedColumn--; }
        if (Input.GetKeyDown(KeyCode.RightArrow)) { SelectedColumn++; }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            UpdateCell(SelectedRow, SelectedColumn, _currentCellState);
            _currentCellState =
                _currentCellState == CellState.White
                    ? CellState.Black
                    : CellState.White;
        }

        for (var r = 0; r < _cells.GetLength(0); r++)
        {
            for (var c = 0; c < _cells.GetLength(1); c++)
            {
                _cells[r, c].GetComponent<Renderer>()
                    .material = NormalMaterial;
            }
        }

        var selectedCell = _cells[SelectedRow, SelectedColumn];
        var renderer = selectedCell.GetComponent<Renderer>();
        renderer.material = SelectedMaterial;

    }

    private void UpdateCell(int row, int column, CellState cellState)
    {
        var stone = _stones[row, column];
        if (cellState == CellState.None && stone != null)
        {
            Destroy(stone);
        }
        else
        {
            if (stone == null)
            {
                stone = CreateStone(row, column);
                _stones[row, column] = stone;
            }
            stone.transform.rotation =
                cellState == CellState.White
                    ? Quaternion.identity
                    : Quaternion.Euler(180, 0, 0);
        }
    }

    private GameObject CreateGameObject(GameObject origin, int r, int c)
    {
        var gameObj = Instantiate(origin);
        gameObj.name = origin.name + "(" + r + ", " + c + ")";
        gameObj.transform.parent = transform;
        gameObj.transform.Translate(c * CellMargin, 0, r * CellMargin);
        return gameObj;
    }
    private GameObject CreateCell(int r, int c)
    {
        return CreateGameObject(Cell, r, c);
    }
    private GameObject CreateStone(int r, int c)
    {
        return CreateGameObject(Stone, r, c);
    }
}
