using System.Collections;
using System.Linq;
using Assets.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private Color _red;
    private Color _yellow;
    
    private GameObject[,] _board;

    private Color _activeColor;
    private GameObject _activeCoin;

    // Coin prefab
    public Transform Coin;

    private bool _gameOver = false;

    // Use this for initialization
	void Start ()
	{
	    InititialiseBoard();

	    InitialiseColours();

	    StartCoroutine(GameLoop());
	}

    private void InititialiseBoard()
    {
        _board = new GameObject[6, 7];
        var grid = GameObject.Find("grid");

        foreach (Transform child in grid.transform)
        {
            var childGameObject = child.gameObject;

            if (!childGameObject.name.StartsWith("tile"))
            {
                continue;
            }

            var indexString = childGameObject.name.Substring(4);

            var row = int.Parse(indexString.Substring(0, 1));
            var col = int.Parse(indexString.Substring(1));

            _board[row, col] = childGameObject;
        }
    }

    private void InitialiseColours()
    {
        ColorUtility.TryParseHtmlString("#E88989FF", out _red);
        ColorUtility.TryParseHtmlString("#E7DF77FF", out _yellow);

        // Red always starts
        _activeColor = _red;
    }

    private IEnumerator GameLoop()
    {
        // Create a coin
        yield return StartCoroutine(InitialiseCoin());

        // Wait for coin to say that it's taken it's turn (user slides it)
        yield return StartCoroutine(WaitForTurnTaken());

        yield return StartCoroutine(FinishTurn());

        yield return StartCoroutine(CheckForWin());

        yield return StartCoroutine(PrepareNextTurn());

        if (_gameOver)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            StartCoroutine(GameLoop());
        }
    }

    private IEnumerator InitialiseCoin()
    {
        _activeCoin = Instantiate(Coin).gameObject;
        _activeCoin.GetComponent<SpriteRenderer>().color = _activeColor;

        yield return null;
    }

    private IEnumerator WaitForTurnTaken()
    {
        while (!_activeCoin.GetComponent<CoinMovementManager>().Completed)
        {
            yield return null;
        }
    }

    private IEnumerator FinishTurn()
    {
        var circleCollider = _activeCoin.GetComponent<CircleCollider2D>();

        var results = new Collider2D[4];

        circleCollider.OverlapCollider(new ContactFilter2D(), results);

        if (!results.Any())
        {
            yield return null;
        }

        Collider2D closestCollider = null;
        var closestDistance = float.MaxValue;

        foreach (var result in results.Where(it => it != null))
        {
            var thisPosition = circleCollider.bounds.center;
            var otherPosition = result.bounds.center;

            var distance = Vector3.Distance(thisPosition, otherPosition);

            if (distance < closestDistance)
            {
                closestCollider = result;
                closestDistance = distance;
            }
        }


        if (closestCollider != null)
        {
            closestCollider.gameObject.GetComponent<SpriteRenderer>().color = _activeColor;
        }

        Destroy(_activeCoin);

        yield return new WaitForSeconds(1.5f);
    }

    private IEnumerator CheckForWin()
    {
        if (false)
        {
            _gameOver = true;
        }

        yield return null;
    }

    private IEnumerator PrepareNextTurn()
    {
        _activeColor = _activeColor == _red ? _yellow : _red;

        yield return null;
    }
}
