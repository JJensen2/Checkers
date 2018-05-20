using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public bool isWhite;
    public bool isKing;

    public bool MustMove(Piece[,] board, int x, int y)
    {
        if (isWhite || isKing)
        {
            // Can capture up left
            if(x >= 2 && y <= 5)
            {
                Piece targetPiece = board[x - 1, y + 1];
                if(targetPiece != null && targetPiece.isWhite != isWhite) // If piece exists and is enemy
                {
                    if (board[x - 2, y + 2] == null) // If can land after jumping
                        return true;
                }
            }
            // Can capture up right
            if (x <= 5 && y <= 5)
            {
                Piece targetPiece = board[x + 1, y + 1];
                if (targetPiece != null && targetPiece.isWhite != isWhite) // If piece exists and is enemy
                {
                    if (board[x + 2, y + 2] == null) // If can land after jumping
                        return true;
                }
            }
        }
        if(!isWhite || isKing)
        {
            // Can capture down left
            if (x >= 2 && y >= 2)
            {
                Piece targetPiece = board[x - 1, y - 1];
                if (targetPiece != null && targetPiece.isWhite != isWhite) // If piece exists and is enemy
                {
                    if (board[x - 2, y - 2] == null) // If can land after jumping
                        return true;
                }
            }
            // Can capture down right
            if (x <= 5 && y >= 2)
            {
                Piece targetPiece = board[x + 1, y - 1];
                if (targetPiece != null && targetPiece.isWhite != isWhite) // If piece exists and is enemy
                {
                    if (board[x + 2, y - 2] == null) // If can land after jumping
                        return true;
                }
            }
        }
        return false;
    }
    public bool ValidMove(Piece[,] board, int x1, int y1, int x2, int y2)
    {
        // If moving atop another piece
        if (board[x2, y2] != null)
            return false;
        int deltaMoveX = Mathf.Abs(x1 - x2); // Number of tiles moved in x direction
        int deltaMoveY = y2 - y1; // Number of tiles moved in y direction

        // White move
        if (isWhite || isKing)
        {
            if(deltaMoveX == 1) // Normal move
            {
                if (deltaMoveY == 1)
                    return true;
            }
            else if(deltaMoveX == 2) // Jumping over enemy piece
            {
                if(deltaMoveY == 2)
                {
                    Piece targetPiece = board[(x1 + x2) / 2, (y1 + y2) / 2];
                    if (targetPiece != null && targetPiece.isWhite != isWhite) 
                        return true;
                }
            }
        }
        // Black move
        if (!isWhite || isKing)
        {
            if (deltaMoveX == 1) // Normal move
            {
                if (deltaMoveY == -1)
                    return true;
            }
            else if (deltaMoveX == 2) // Jumping over enemy piece
            {
                if (deltaMoveY == -2)
                {
                    Piece targetPiece = board[(x1 + x2) / 2, (y1 + y2) / 2];
                    if (targetPiece != null && targetPiece.isWhite != isWhite)
                        return true;
                }
            }
        }

        return false;
    }

}
