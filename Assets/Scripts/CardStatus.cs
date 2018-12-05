
using UnityEngine;

public class CardStatus {

	// Used by the CardStackController track the current visual status of an individual card and whether or not it is face up or face down.

	public GameObject Card { get; private set; }
	public bool IsFaceUp { get; set; }

	public CardStatus(GameObject card) {
		Card = card;
		IsFaceUp = false;
	}
}
