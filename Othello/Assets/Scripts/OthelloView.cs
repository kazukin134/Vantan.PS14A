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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) { SelectedRow--; }
        if (Input.GetKeyDown(KeyCode.DownArrow)) { SelectedRow++; }
        if (Input.GetKeyDown(KeyCode.LeftArrow)) { SelectedColumn--; }
        if (Input.GetKeyDown(KeyCode.RightArrow)) { SelectedColumn++; }

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

    private GameObject CreateCell(int r, int c)
    {
        var cell = Instantiate(Cell);
        cell.name = Cell.name + "(" + r + ", " + c + ")";
        cell.transform.parent = transform;
        cell.transform.Translate(c * CellMargin, 0, r * CellMargin);
        return cell;
    }
}
