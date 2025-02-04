﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Othello.Model;

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
        Initialize();
        Reset();

        StartCoroutine(UpdateGame());
    }

    private void Initialize()
    {
        for (var r = 0; r < Rows; r++)
        {
            for (var c = 0; c < Columns; c++)
            {
                _cells[r, c] = CreateCell(r, c);
            }
        }
    }

    private void Reset()
    {
        Clear();

        UpdateCell(3, 3, CellState.White);
        UpdateCell(3, 4, CellState.Black);
        UpdateCell(4, 3, CellState.Black);
        UpdateCell(4, 4, CellState.White);

        _isGameOver = false;
        _record = "";
        _currentPlayer = Player.Black;

        UpdateCellMaterials();
        UpdateStoneCount();
    }

    private void Reset(string record)
    {
        Reset();

        var positions = CellPosition.ParseList(record);
        foreach (var pt in positions) { GoNext(pt); }
    }

    private void Clear()
    {
        for (var r = 0; r < Rows; r++)
        {
            for (var c = 0; c < Columns; c++)
            {
                UpdateCell(r, c, CellState.None);
            }
        }
    }

    private void UpdateCellMaterials()
    {
        for (var r = 0; r < Rows; r++)
        {
            for (var c = 0; c < Columns; c++)
            {
                UpdateCellMaterial(r, c);
            }
        }
    }

    private void UpdateCellMaterial(int row, int column)
    {
        var cell = _cells[row, column];
        cell.GetComponent<Renderer>().material =
            row == SelectedRow && column == SelectedColumn
                ? SelectedMaterial
                : IsPlaceable(row, column, _currentPlayer)
                    ? PlaceableMaterial
                    : NormalMaterial;
    }

    private int _selectedRow;
    public int SelectedRow
    {
        get { return _selectedRow; }
        set
        {
            var oldSelectedRow = _selectedRow;

            if (value < 0) { _selectedRow = 0; }
            else if (value >= Rows) { _selectedRow = Rows - 1; }
            else { _selectedRow = value; }

            UpdateCellMaterial(oldSelectedRow, SelectedColumn);
            UpdateCellMaterial(_selectedRow, SelectedColumn);
        }
    }

    private int _selectedColumn;
    public int SelectedColumn
    {
        get { return _selectedColumn; }
        set
        {
            var oldSelectedColumn = _selectedColumn;

            if (value < 0) { _selectedColumn = 0; }
            else if (value >= Columns) { _selectedColumn = Columns - 1; }
            else { _selectedColumn = value; }

            UpdateCellMaterial(SelectedRow, oldSelectedColumn);
            UpdateCellMaterial(SelectedRow, _selectedColumn);
        }
    }

    public GameObject SelectedCell
    {
        get { return _cells[SelectedRow, SelectedColumn]; }
    }

    private PlayerType _blackPlayerType = PlayerType.Human;
    private PlayerType _whitePlayerType = PlayerType.Computer;
    private Player _currentPlayer = Player.Black;
    private int _blackCount = 0;
    private int _whiteCount = 0;

    private PlayerType GetPlayerType(Player player)
    {
        return player == Player.Black ? _blackPlayerType : _whitePlayerType;
    }

    private void UpdateStoneCount()
    {
        _blackCount = 0;
        _whiteCount = 0;
        for(var r = 0; r < Rows; r++)
        {
            for(var c = 0; c < Columns; c++)
            {
                var state = GetCellState(r, c);
                if (state == CellState.Black) { _blackCount++; }
                if (state == CellState.White) { _whiteCount++; }
            }
        }
    }

    private bool _isGameOver = false;
    private IEnumerator UpdateGame()
    {
        while (true)
        {
            while (!_isGameOver)
            {
                yield return StartCoroutine(GetUpdateCoroutine());
            }
            while (true)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Reset();
                    break;
                }
                yield return null;
            }
        }
    }
    private IEnumerator GetUpdateCoroutine()
    {
        return (GetPlayerType(_currentPlayer) == PlayerType.Human) ?
                UpdatePlayer() : UpdateComputer();
    }

    private IEnumerator UpdateComputer()
    {
        var positions = GetPlaceableCellPositions(_currentPlayer).ToArray();
        var pt = positions[UnityEngine.Random.Range(0, positions.Length - 1)];
        Debug.Log(pt);
        GoNext(pt); //TODO: 石を置けるセルから最善手を選択する

        UpdateCellMaterials();
        UpdateStoneCount();
        yield return null;
    }


    private IEnumerator UpdatePlayer()
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow)) { SelectedRow--; }
            if (Input.GetKeyDown(KeyCode.DownArrow)) { SelectedRow++; }
            if (Input.GetKeyDown(KeyCode.LeftArrow)) { SelectedColumn--; }
            if (Input.GetKeyDown(KeyCode.RightArrow)) { SelectedColumn++; }
            if (Input.GetKeyDown(KeyCode.Return)
                && IsPlaceable(SelectedRow, SelectedColumn, _currentPlayer))
            {
                GoNext(SelectedRow, SelectedColumn);
                UpdateCellMaterials();
                UpdateStoneCount();
                break;
            }
            yield return null;
        }
    }

    private string _record = "";
    private void GoNext(int row, int column)
    {
        GoNext(new CellPosition(row, column));
    }
    private void GoNext(CellPosition pt)
    {
        _record += pt.ToString();
        Place(pt.Row, pt.Column, _currentPlayer);
        var other = _currentPlayer.GetOtherPlayer();
        if (HasPlaceableCell(other)) { _currentPlayer = other; }
        else { _isGameOver = !HasPlaceableCell(_currentPlayer); }
    }

    private void Place(int row, int column, Player player)
    {
        var cellState = player.ToCellState();
        UpdateCell(row, column, cellState);

        var cellPositions = GetReversableCellPositions(row, column, player);
        foreach (var pt in cellPositions)
        {
            UpdateCell(pt.Row, pt.Column, cellState);
        }
    }

    private IEnumerable<CellPosition> GetPlaceableCellPositions(Player player)
    {
        for (var r = 0; r < Rows; r++)
        {
            for (var c = 0; c < Columns; c++)
            {
                if (IsPlaceable(r, c, player))
                {
                    yield return new CellPosition(r, c);
                }
            }
        }
    }

    private IEnumerable<CellPosition> GetReversableCellPositions(int row, int column, Player player)
    {
        var state = GetCellState(row, column);
        var other = player.GetOtherPlayer().ToCellState();
        var pc = player.ToCellState();

        var list = new List<CellPosition>();
        list.AddRange(GetReversableCellPositionsByDirection(row, column, -1, 0, pc, other));
        list.AddRange(GetReversableCellPositionsByDirection(row, column, 1, 0, pc, other));
        list.AddRange(GetReversableCellPositionsByDirection(row, column, 0, -1, pc, other));
        list.AddRange(GetReversableCellPositionsByDirection(row, column, 0, 1, pc, other));
        list.AddRange(GetReversableCellPositionsByDirection(row, column, -1, -1, pc, other));
        list.AddRange(GetReversableCellPositionsByDirection(row, column, -1, 1, pc, other));
        list.AddRange(GetReversableCellPositionsByDirection(row, column, 1, -1, pc, other));
        list.AddRange(GetReversableCellPositionsByDirection(row, column, 1, 1, pc, other));
        return list;
    }
    private IEnumerable<CellPosition> GetReversableCellPositionsByDirection(int row, int column, int nr, int nc, CellState player, CellState other)
    {
        var npt = new CellPosition(row + nr, column + nc);
        if (GetCellState(npt.Row, npt.Column) != other)
        {
            return new CellPosition[0];
        }

        var list = new List<CellPosition>();
        list.Add(npt);
        for (
            var ipt = new CellPosition(npt.Row + nr, npt.Column + nc);
            ipt.Row >= 0 && ipt.Column >= 0 &&
            ipt.Row < Rows && ipt.Column < Columns;
            ipt.Row += nr, ipt.Column += nc)
        {
            var s = GetCellState(ipt.Row, ipt.Column);
            if (s == player) { return list; }
            else if (s == other) { list.Add(ipt); }
            else { break; }
        }
        return new CellPosition[0];
    }

    private void UpdateCell(int row, int column, CellState cellState)
    {
        var stone = _stones[row, column];
        if (cellState == CellState.None)
        {
            if (stone != null) { Destroy(stone); }
            _stones[row, column] = null;
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
        gameObj.transform.Translate(c * CellMargin, 0, -r * CellMargin);
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

    private bool HasPlaceableCell(Player player)
    {
        for(var r = 0; r < Rows; r++)
        {
            for(var c = 0; c < Columns; c++)
            {
                if (IsPlaceable(r, c, player)) { return true; }
            }
        }
        return false;
    }
     
    private bool IsPlaceable(int row, int column, Player player)
    {
        var state = GetCellState(row, column);
        if (state != CellState.None) { return false; }

        var other = player.GetOtherPlayer().ToCellState();
        var pc = player.ToCellState();
        return IsPlaceableByDirection(row, column, -1 , 0, pc, other)
            || IsPlaceableByDirection(row, column, 1, 0, pc, other)
            || IsPlaceableByDirection(row, column, 0, -1, pc, other)
            || IsPlaceableByDirection(row, column, 0, 1, pc, other)
            || IsPlaceableByDirection(row, column, -1, -1, pc, other)
            || IsPlaceableByDirection(row, column, -1, 1, pc, other)
            || IsPlaceableByDirection(row, column, 1, -1, pc, other)
            || IsPlaceableByDirection(row, column, 1, 1, pc, other);
    }

    private bool IsPlaceableByDirection(int row, int column, int nr, int nc, CellState player, CellState other)
    {
        var nextRow = row + nr;
        var nextColumn = column + nc;
        var nextCell = GetCellState(nextRow, nextColumn);
        if (nextCell == other)
        {
            for (int ir = nextRow + nr, ic = nextColumn + nc
                ; ir < Rows && ic < Columns ; ir += nr, ic += nc)
            {
                var s = GetCellState(ir, ic);
                if (s == other) { continue; }
                return s == player;
            }
        }
        return false;
    }

    private void OnGUI()
    {
        if (_isGameOver) { GUILayout.Label("Game over"); }
        else { GUILayout.Label("Player: " + _currentPlayer);  }
        GUILayout.Label("Black: " + _blackCount);
        GUILayout.Label("White: " + _whiteCount);
    }
}
