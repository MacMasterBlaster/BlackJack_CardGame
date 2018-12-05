using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardViewController : MonoBehaviour {

	public GameObject Card { get; private set; }
    public bool IsFaceUp { get; set; }

    public CardViewController(GameObject card)
    {
        Card = card;
        IsFaceUp = false;
    }
}
