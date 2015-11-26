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

    [SerializeField]
    private Material _placeableMaterial = null;
    public Material PlaceableMaterial
    {
        get
        {
            if (_placeableMaterial == null)
            {
                _placeableMaterial = Resources.Load<Material>("Cell/PlaceableMaterial");
            }
            return _placeableMaterial;
        }
    }

    private const float CellMargin = 1.1F;
    private const int Rows = 8;
    private const int Columns = 8;

    private GameObject[,] _cells = new GameObject[Rows, Columns];
    private GameObject[,] _stones = new GameObject[Rows, Columns];

    void Start()
    {
        for(var r = 0; r < Rows; r++)
        {
            for(var c = 0; c < Columns; c++)
            {
                _cells[r, c] = CreateCell(r, c);
            }
        }

        UpdateCell(3, 3, CellState.Black);
        UpdateCell(3, 4, CellState.White);
        UpdateCell(4, 3, CellState.White);
        UpdateCell(4, 4, CellState.Black);

        SelectedRow = 0;
        SelectedColumn = 0;

        for (var r = 0; r < Rows; r++)
        {
            for (var c = 0; c < Columns; c++)
            {
                if (IsPlaceable(r, c, _currentPlayer))
                {
                    var renderer = _cells[r, c].GetComponent<Renderer>();
                    renderer.material = PlaceableMaterial;
                }
            }
        }
    }

    private int _selectedRow;
    public int SelectedRow
    {
        get { return _selectedRow; }
        set
        {
            SelectedCell.GetComponent<Renderer>().material =
                IsPlaceable(_selectedRow, SelectedColumn, _currentPlayer)
                    ? PlaceableMaterial : NormalMaterial;

            if (value < 0) { _selectedRow = 0; }
            else if (value >= Rows) { _selectedRow = Rows - 1; }
            else { _selectedRow = value; }

            SelectedCell.GetComponent<Renderer>().material =
                IsPlaceable(_selectedRow, SelectedColumn, _currentPlayer)
                    ? PlaceableMaterial : SelectedMaterial;
        }
    }

    private int _selectedColumn;
    public int SelectedColumn
    {
        get { return _selectedColumn; }
        set
        {
            SelectedCell.GetComponent<Renderer>().material =
                IsPlaceable(SelectedRow, _selectedColumn, _currentPlayer)
                ? PlaceableMaterial : NormalMaterial;

            if (value < 0) { _selectedColumn = 0; }
            else if (value >= Columns) { _selectedColumn = Columns - 1; }
            else { _selectedColumn = value; }

            SelectedCell.GetComponent<Renderer>().material =
                IsPlaceable(SelectedRow, _selectedColumn, _currentPlayer)
                ? PlaceableMaterial : SelectedMaterial;
        }
    }

    public GameObject SelectedCell
    {
        get { return _cells[SelectedRow, SelectedColumn]; }
    }

    private Player _currentPlayer = Player.White;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) { SelectedRow--; }
        if (Input.GetKeyDown(KeyCode.DownArrow)) { SelectedRow++; }
        if (Input.GetKeyDown(KeyCode.LeftArrow)) { SelectedColumn--; }
        if (Input.GetKeyDown(KeyCode.RightArrow)) { SelectedColumn++; }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            var cellState = ToCellState(_currentPlayer);
            UpdateCell(SelectedRow, SelectedColumn, cellState);
            _currentPlayer = GetOtherPlayer(_currentPlayer);
        }

        // TODO: 選択セルが変更されたときのみ置き換える
        //for (var r = 0; r < _cells.GetLength(0); r++)
        //{
        //    for (var c = 0; c < _cells.GetLength(1); c++)
        //    {
        //        _cells[r, c].GetComponent<Renderer>()
        //            .material = NormalMaterial;
        //    }
        //}

        //var selectedCell = _cells[SelectedRow, SelectedColumn];
        //var renderer = selectedCell.GetComponent<Renderer>();
        //renderer.material = SelectedMaterial;
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

    private CellState GetCellState(int row, int column)
    {
        if (row < 0 || row >= Rows || column < 0 || column >= Columns)
        {
            return CellState.None;
        }
        var stone = _stones[row, column];
        return stone == null ? CellState.None
            : stone.transform.rotation == Quaternion.identity ? CellState.White
                : CellState.Black;
    }
    
    private bool IsPlaceable(int row, int column, Player player)
    {
        var state = GetCellState(row, column);
        if (state != CellState.None) { return false; }

        var other = ToCellState(GetOtherPlayer(player));
        var nextColumn = column + 1;
        var nextCell = GetCellState(row, nextColumn);
        if (nextCell == other)
        {
            var pc = ToCellState(player);
            for (var nc = nextColumn + 1; nc < Columns; nc++)
            {
                var s = GetCellState(row, nc);
                if (s == other) { continue; }
                return s == pc;
            }
        }

        return false;
    }

    private static CellState ToCellState(Player player)
    {
        return player == Player.Black ? CellState.Black : CellState.White;
    }

    private static Player GetOtherPlayer(Player player)
    {
        return player == Player.White ? Player.Black : Player.White;
    }
}
