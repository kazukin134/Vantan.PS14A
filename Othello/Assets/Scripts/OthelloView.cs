using UnityEngine;

public class OthelloView : MonoBehaviour
{
	void Start ()
    {
        var cell = Resources.Load<GameObject>("Cell/Cell");
        Instantiate(cell);
	}
}
