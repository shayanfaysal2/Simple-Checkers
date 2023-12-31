using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public enum PieceType
    {
        black,
        white
    }

    public PieceType pieceType;
    public bool isKing;
    public int x;
    public int y;

    [SerializeField] private GameObject crown;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Sprite whiteSprite;
    [SerializeField] private Sprite blackSprite;

    public void UpdatePosition(int x, int y)
    {
        this.x = x;
        this.y = y;
        //transform.position = new Vector2(x, y);
    }

    public void UpdateType(PieceType pieceType)
    {
        this.pieceType = pieceType;
        if (pieceType == PieceType.white)
            sr.sprite = whiteSprite;
        else
            sr.sprite = blackSprite;
    }

    public void GetSelected()
    {
        transform.localScale = new Vector3(1.1f, 1.1f, 1f);
        sr.color = Color.gray;
    }

    public void GetDeselected()
    {
        transform.localScale = Vector3.one;
        sr.color = Color.white;
    }

    public void Promote()
    {
        isKing = true;
        crown.SetActive(true);
    }

    public void Demote()
    {
        isKing = false;
        crown.SetActive(false);
    }
}
