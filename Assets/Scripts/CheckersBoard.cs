using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckersBoard : MonoBehaviour
{
    public Piece[,] pieces = new Piece[8, 8];
    public GameObject whitePiecePrefab;
    public GameObject blackPiecePrefab;

    private Vector3 boardOffset = new Vector3(-4.0f, 0, -4.0f);
    private Vector3 pieceOffset = new Vector3(0.5f, 0, 0.5f);

    private void Start()
    {
        GenerateBoard();
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
            newPiece = Instantiate(whitePiecePrefab) as GameObject;
        }
        else
        {
            newPiece = Instantiate(blackPiecePrefab) as GameObject;
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
}
