using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CardStackController))]
public class CardStackViewer : MonoBehaviour {
	
    public float posOffset;
    public GameObject cardPrefab;
    public bool isFaceUp = false; //This should only be true if it is the player hand or when the dealer's hand wins.

	private CardStackController deck;
    private Dictionary<int, GameObject> fetchedCards;
    int lastCount;
    private Vector3 start;
 
    void Start() {
        fetchedCards = new Dictionary<int, GameObject>();
        deck = GetComponent<CardStackController>();
        ShowCards();
        lastCount = deck.CardCount;
        deck.CardRemoved += deckCardRemoved;
    }

    void Update() {
		//Update cards display if the size of the card stack has changed.
        if (lastCount != deck.CardCount) {
            lastCount = deck.CardCount;
            ShowCards();
        }
    }

    //When called, removes cards for a card stack dictionary and destoys the associated card gameobject.
    void deckCardRemoved (object source, CardRemovedEventArgs e){
        if (fetchedCards.ContainsKey(e.CardIndex)) {
            Destroy(fetchedCards[e.CardIndex]);
            fetchedCards.Remove(e.CardIndex);
        }
    }

    private void ShowCards() {
		start  = gameObject.transform.position;
        int cardCount = 0;

        if (deck.HasCards) {
            foreach (int i in deck.GetCards()) {
				//Set the position of the cards relative to one another and add it to the cardStack;
                float co = posOffset * cardCount;
                Vector3 temp = start + new Vector3(co, 0f);
                AddCard(temp, i, cardCount);
                cardCount++;
            }
        }
    }

   	private void AddCard(Vector3 position, int cardIndex, int positionalIndex) {
        if (fetchedCards.ContainsKey(cardIndex)) {
            return;
        }
    
		//Create and assign values and positions to the cards in the deck.
        GameObject cardCopy = (GameObject)Instantiate(cardPrefab);
        cardCopy.transform.position = position;
        cardCopy.transform.SetParent(gameObject.transform);

        CardController cardController = cardCopy.GetComponent<CardController>();
        cardController.cardIndex = cardIndex;
        
        cardController.ToggleFace(isFaceUp);

        //Set the spriteRenderer sorting order to be reversed if the cardStack is the dealers deck.
        SpriteRenderer spriteRenderer = cardCopy.GetComponent<SpriteRenderer>();
        if (deck.isDealerDeck) {
            spriteRenderer.sortingOrder = deck.CardCount - positionalIndex;
        }
        else {
            spriteRenderer.sortingOrder = positionalIndex;
        }

        fetchedCards.Add(cardIndex, cardCopy);
    }
}
