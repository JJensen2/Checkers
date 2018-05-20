using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CheckersBoard : MonoBehaviour
{
    public static CheckersBoard Instance { set; get; }

    public Piece[,] pieces = new Piece[8, 8];
    public GameObject whitePiecePrefab;
    public GameObject blackPiecePrefab;
    private float winTime;

    public Transform chatMessageContainer;
    public GameObject messagePrefab;

    public GameObject highlightsContainer;

    public CanvasGroup alertCanvas;
    private float lastAlert;
    private bool alertActive;

    private Vector3 boardOffset = new Vector3(-4.0f, 0, -4.0f);
    private Vector3 pieceOffset = new Vector3(0.5f, 0.125f, 0.5f);
  
    private Piece selectedPiece;
    private List<Piece> mustBeMoved; // Pieces that must move to take enemy piece   

    public bool isWhite;
    private bool isWhiteTurn;
    private bool hasKilled;
    private bool gameIsOver;

    private Vector2 mouseOver;
    private Vector2 startDrag;
    private Vector2 endDrag;

    private Client client;

    private void Start()
    {
        Instance = this;
        client = FindObjectOfType<Client>();

        foreach(Transform t in highlightsContainer.transform)
        {
            t.position = Vector3.down * 100;
        }

        if (client)
        {
            isWhite = client.isHost;
            Alert(client.players[0].name + " Vs. " + client.players[1].name);
        }
        else
        {
            Alert("White Player's Turn");
            // Dissable chat when not in multiplayer
            Transform c = GameObject.Find("Canvas").transform;
            foreach(Transform t in c)
            {
                t.gameObject.SetActive(false);
            }
            c.GetChild(0).gameObject.SetActive(true);            
        }

        isWhiteTurn = true;
        mustBeMoved = new List<Piece>();
        GenerateBoard();
    }
    private void Update()
    {
        if (gameIsOver)
        {
            if(Time.time - winTime > 3.0f)
            {
                Server server = FindObjectOfType<Server>();
                Client client = FindObjectOfType<Client>();

                if (server)
                    Destroy(server.gameObject);
                if (client)
                    Destroy(client.gameObject);
                SceneManager.LoadScene("Menu");
            }

            return;
        }
        // Rotate hilight
        foreach (Transform t in highlightsContainer.transform)
        {
            t.Rotate(Vector3.up * 90 * Time.deltaTime);
        }

        UpdateAlert();
        TrackMouse();

        if((isWhite)? isWhiteTurn :!isWhiteTurn)
        // Check whose turn 
        {
            int x = (int)mouseOver.x;
            int y = (int)mouseOver.y;

            if (selectedPiece != null)
                DragPiece(selectedPiece);
            if (Input.GetMouseButtonDown(0)) // Select piece on click curing player's turn
                SelectPiece(x, y);
            if (Input.GetMouseButtonUp(0))
                TryMove((int)startDrag.x, (int)startDrag.y, x, y);
        }

    }
    private void DragPiece(Piece p)
    {
        // Check for camera
        if (!Camera.main)
        {
            Debug.Log("Cannot locate main camera");
            return;
        }
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board"))) // Send a ray from main camera to mouse pointer
        {
            p.transform.position = hit.point + Vector3.up;
        }
    }
    private void TrackMouse()
    {
        // Check for camera
        if (!Camera.main)
        {
            Debug.Log("Cannot locate main camera");
            return;
        }
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board"))) // Send a ray from main camera to mouse pointer
        {
            mouseOver.x = (int)(hit.point.x - boardOffset.x);
            mouseOver.y = (int)(hit.point.z - boardOffset.z);
        }
        else // If the ray does not hit the mouse pointer
        {
            mouseOver.x = -1;
            mouseOver.y = -1;
        }
    }
    private List<Piece> CheckForcedMoves(Piece p, int x, int y)
    {
        List<Piece> forced = new List<Piece>();

        if (pieces[x, y].MustMove(pieces, x, y))
            forced.Add(pieces[x, y]);
        //mustBeMoved.Add(pieces[x, y]);


       //  Highlight();
        return forced;
    }
    private List<Piece> CheckForcedMoves()
    {
        List<Piece> forced = new List<Piece>();

        // Check each piece to see if it must be moved
        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 8; j++)
                if (pieces[i, j] != null && pieces[i, j].isWhite == isWhiteTurn)
                    if(pieces[i, j].MustMove(pieces, i, j))
                        forced.Add(pieces[i, j]);
       
       // Highlight();
        return forced;
    }
    private void Highlight()
    {
        Debug.Log("Highlight");
        
        foreach (Transform t in highlightsContainer.transform)
        {
            t.position = Vector3.down * 100;
        }

        if (mustBeMoved.Count > 0)
        {
            highlightsContainer.transform.GetChild(0).transform.position = mustBeMoved[0].transform.position + Vector3.down * 0.1f;
            Debug.Log(highlightsContainer.transform.GetChild(0).transform.position);
        }
        if (mustBeMoved.Count > 1)
            highlightsContainer.transform.GetChild(1).transform.position = mustBeMoved[1].transform.position + Vector3.down * 0.1f;
        if (mustBeMoved.Count > 2)
            highlightsContainer.transform.GetChild(2).transform.position = mustBeMoved[2].transform.position + Vector3.down * 0.1f;
    }
    private void SelectPiece(int x, int y) // On mouse click: pass x and y to identify selected piece
    {
        // If out of bounds
        if (x < 0 || x >= 8 || y < 0 || y >= 8)
            return;
        Piece p = pieces[x, y];
        if (p != null && p.isWhite == isWhite)
        {
            if(mustBeMoved.Count == 0)
            {
                // Try to select the piece
                selectedPiece = p;
                startDrag = mouseOver;
            }
            else // Identify pieces that must be moved
            {
                if (mustBeMoved.Find(fp => fp == p) == null)
                    return;
                selectedPiece = p;
                startDrag = mouseOver;
            }
            
        }
    }    
    public void TryMove(int x1, int y1, int x2, int y2) // Needed for multiplayer
    {
        Debug.Log("TryMove");
        mustBeMoved = CheckForcedMoves();
        startDrag = new Vector2(x1, y1);
        endDrag = new Vector2(x2, y2);
        selectedPiece = pieces[x1, y1];
        // If out of bounds 
        if (x2 < 0 || x2 >= 8 || y2 < 0 || y2 >= 8) 
        {
            if (selectedPiece != null)
                MovePiece(selectedPiece, x1, y1);

            startDrag = Vector2.zero;
            selectedPiece = null;
          //   Highlight();
            return;
        }
        // Check if selected piece not null
        if (selectedPiece != null)
        {
            // If piece did not move
            if (endDrag == startDrag)
            {
                MovePiece(selectedPiece, x1, y1);
                startDrag = Vector2.zero;
                selectedPiece = null;
            //    Highlight();
                return;
            }
            // Check if move is valid
            if (selectedPiece.ValidMove(pieces, x1, y1, x2, y2))
            {
                if (Mathf.Abs(x2 - x1) == 2) // Jump enemy piece 
                {
                    Piece targetPiece = pieces[(x1 + x2) / 2, (y1 + y2) / 2];
                    if (targetPiece != null)
                    {
                        pieces[(x1 + x2) / 2, (y1 + y2) / 2] = null;
                        DestroyImmediate(targetPiece.gameObject);
                        hasKilled = true;
                    }
                }
                // Should a piece have been captured
                if (mustBeMoved.Count != 0 && !hasKilled)
                {
                    MovePiece(selectedPiece, x1, y1);
                    startDrag = Vector2.zero;
                    selectedPiece = null;
               //     Highlight();
                    return;
                }
                pieces[x2, y2] = selectedPiece;
                pieces[x1, y1] = null;
                MovePiece(selectedPiece, x2, y2);

                EndTurn();
            }
            else
            {
                MovePiece(selectedPiece, x1, y1);
                startDrag = Vector2.zero;
                selectedPiece = null;
              //  Highlight();
                return;
            }
        }
    }
    private void EndTurn()
    {
        int x = (int)endDrag.x; 
        int y = (int)endDrag.y;

        if(selectedPiece != null)
        {
            // Promote non-king white piece to king
            if (selectedPiece.isWhite && !selectedPiece.isKing && y == 7) 
            {
                selectedPiece.isKing = true;
                // selectedPiece.transform.Rotate(Vector3.right * 180); change to king
                selectedPiece.GetComponentInChildren<Animator>().SetTrigger("FlipTrigger");
            }
            // Promote non-king black piece to king
            else if (!selectedPiece.isWhite && !selectedPiece.isKing && y == 0)
            {
                selectedPiece.isKing = true;
                // selectedPiece.transform.Rotate(Vector3.right * 180); change to king
                selectedPiece.GetComponentInChildren<Animator>().SetTrigger("FlipTrigger");
            }
        }

        if(client)
        {
            string msg = "CMOV|";
            msg += startDrag.x.ToString() + "|";
            msg += startDrag.y.ToString() + "|";
            msg += endDrag.x.ToString() + "|";
            msg += endDrag.y.ToString();

            client.Send(msg);
        }

        selectedPiece = null;
        startDrag = Vector2.zero;

        if (CheckForcedMoves(selectedPiece, x, y).Count != 0 && hasKilled)
        {
            mustBeMoved = CheckForcedMoves();
            Highlight();
            return;
        }
            //isWhite = !isWhite; // End turn, Only for local games
        hasKilled = false;
        isWhiteTurn = !isWhiteTurn;

        CheckVictory();

        if (!client)
            isWhite = !isWhite;

        //
       // if (!client) { isWhite = !isWhite; if (isWhite) Alert("White player's turn"); else Alert("Black player's turn"); } else { if (isWhite) Alert(client.players[0].name + "'s turn"); else Alert(client.players[1].name + "'s turn"); }
        //
            
        mustBeMoved = CheckForcedMoves();
        Highlight();
    }
    private void CheckVictory()
    {
        var ps = FindObjectsOfType<Piece>();
        bool hasWhite = false, hasBlack = false;
        for(int i = 0; i < ps.Length; i++)
        {
            if (ps[i].isWhite)
                hasWhite = true;
            else
                hasBlack = true;
        }
        if (!hasWhite)
            Victory(false);
        if (!hasBlack)
            Victory(true);
    }
    private void Victory(bool isWhite)
    {
        winTime = Time.time;

        if (isWhite)
            Alert("White Player Has Won!");       
        else
            Alert("Black Player Has Won!");
        gameIsOver = true;
    }
    private void GenerateBoard()
    {
        // Spawn white pieces in every other column of the bottom three rows
        for(int y = 0; y < 3; y++)
        {
            bool rowIsOdd = (y % 2 == 0);
            for (int x = 0; x < 8; x += 2)
            {
                // Spawn a white piece
                SpawnPiece((rowIsOdd)?x:x+1, y); // Offset by one space for odd rows
            }
        }

        // Spawn black pieces in every other column of the top three rows
        for (int y = 7; y > 4; y--)
        {
            bool rowIsOdd = (y % 2 == 0);
            for (int x = 0; x < 8; x += 2)
            {
                // Spawn a black piece
                SpawnPiece((rowIsOdd) ? x : x + 1, y); // Offset by one space for odd rows
            }
        }
    }
    private void SpawnPiece(int x, int y)
    {   //Check which prefab to spawn y > 4 : black, y < 4 : white
        GameObject newPiece;
        if (y > 3)
        {
            newPiece = Instantiate(blackPiecePrefab) as GameObject;
        }
        else
        {
            newPiece = Instantiate(whitePiecePrefab) as GameObject;
        }
        newPiece.transform.SetParent(transform);
        Piece p = newPiece.GetComponent<Piece>();
        pieces[x, y] = p;
        MovePiece(p, x, y);
    }
    private void MovePiece(Piece p, int x, int y)
    {
        p.transform.position = (Vector3.right * x) + (Vector3.forward * y) + boardOffset + pieceOffset;
    }
    public void Alert(string text)
    {
        alertCanvas.GetComponentInChildren<Text>().text = text;
        alertCanvas.alpha = 1;
        lastAlert = Time.time;
        alertActive = true;

    }
    public void UpdateAlert()
    {
        if(alertActive)
        {
            if(Time.time - lastAlert > 1.5f)
            {
                alertCanvas.alpha = 1 - ((Time.time - lastAlert) - 1.5f);

                if(Time.time - lastAlert > 2.5f)
                {
                    alertActive = false;
                }
            }
        }
    }
    public void ChatMessage(string msg)
    {
        GameObject go = Instantiate(messagePrefab) as GameObject;
        go.transform.SetParent(chatMessageContainer);

        go.GetComponentInChildren<Text>().text = msg;
    }
    public void SendChatMessage()
    {
        InputField i = GameObject.Find("MessageInput").GetComponent<InputField>();

        if (i.text == "")
            return;
        client.Send("CMSG|" + i.text);

        i.text = "";
    }
}
