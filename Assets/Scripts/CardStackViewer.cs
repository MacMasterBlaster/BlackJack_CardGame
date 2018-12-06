using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CardStackController))]
public class CardStackViewer : MonoBehaviour {
	
    public float posOffset;
    public GameObject cardPrefab;
    public bool isFaceUp = false;
    public bool isDeck = false;

	private CardStackController cardStack;
    private Dictionary<int, CardStatus> fetchedCards;
    private int lastCount;
    private Vector3 start;
 
    private void Awake() {
        start  = gameObject.transform.position;
        fetchedCards = new Dictionary<int, CardStatus>();
        cardStack = GetComponent<CardStackController>();
        UpdateCardStackView();
        lastCount = cardStack.CardCount;
        cardStack.CardRemoved += cardStackCardRemoved;
        cardStack.CardAdded += cardStackCardAdded;
    }

    private void Update() {
		// Update cards display if the size of the card stack has changed.
        if (lastCount != cardStack.CardCount) {
            lastCount = cardStack.CardCount;
            UpdateCardStackView();
        }
    }

    // When called, removes cards from a card stack dictionary and destoys the associated card gameobject.
    private void cardStackCardRemoved (object source, CardEventArgs e) {
        if (fetchedCards.ContainsKey(e.CardIndex)) {
            Destroy(fetchedCards[e.CardIndex].Card);
            fetchedCards.Remove(e.CardIndex);
        }
    }

    // When called, determines the position and intantiates a card. It then adds that card to the cardStack. 
    private void cardStackCardAdded(object source, CardEventArgs e) {
        float currentOffset = posOffset * cardStack.CardCount;
        Vector3 tempPos = start + new Vector3(currentOffset, 0f);
        AddCard(tempPos, e.CardIndex, cardStack.CardCount);
    }

    // Controls the positions of the card objects in the associated cardStack.
    private void UpdateCardStackView() {
		start  = gameObject.transform.position;
        int cardCount = 0;

        if (cardStack.HasCards) {
            foreach (int i in cardStack.GetCards()) {
				// Set the position of the cards relative to one another and add it to the cardStack;
                float co = posOffset * cardCount;
                Vector3 temp = start + new Vector3(co, 0f);
                AddCard(temp, i, cardCount);
                cardCount++;
            }
        }
    }

    // Creates a card and adds it to the card stack.
   	private void AddCard(Vector3 position, int cardIndex, int positionalIndex) {
        if (fetchedCards.ContainsKey(cardIndex)) {
            if(!isFaceUp){
                CardController cardCon = fetchedCards[cardIndex].Card.GetComponent<CardController>();
                cardCon.ToggleFace(fetchedCards[cardIndex].IsFaceUp);
            }
            return;
        }
		// Create a card prefab copy and assign values and positions to the cards in the cardStack.
        GameObject cardCopy = (GameObject)Instantiate(cardPrefab);
        cardCopy.transform.position = position;
        cardCopy.transform.SetParent(gameObject.transform);

        CardController cardController = cardCopy.GetComponent<CardController>();
        cardController.cardIndex = cardIndex;
       
        cardController.ToggleFace(isFaceUp);

        // Set the spriteRenderer sorting order to be reversed if the cardStack is the dealers cardStack.
        SpriteRenderer spriteRenderer = cardCopy.GetComponent<SpriteRenderer>();
        if (cardStack.isDealerDeck) {
            spriteRenderer.sortingOrder = cardStack.CardCount - positionalIndex;
        }
        else {
            spriteRenderer.sortingOrder = positionalIndex;
        }

        fetchedCards.Add(cardIndex, new CardStatus(cardCopy));
    }

    // Using the card objects cardStatus to set the visual state of a given card in a cardStack.
    public void Toggle(int card, bool isFaceUp)
    {
        fetchedCards[card].IsFaceUp = isFaceUp;
        UpdateCardStackView();
    }
}
