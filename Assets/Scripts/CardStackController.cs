using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardStackController : MonoBehaviour
{
    public event CardEventHandler CardRemoved;
    public event CardEventHandler CardAdded;

    public bool isDealerDeck;
    
    public bool HasCards {
        get { return cardStack != null && cardStack.Count > 0; }
    }

    public int CardCount {
        get {
            if (cardStack == null) {
                return 0;
            }
            else {
                return cardStack.Count;
            }
        }
    }

    // The value of each card is determined by using a mod 13 of the index position to return value.
    // Using this we can detemerine the point value of a give stack of cards.
    // This is works because of the way the sprites are sorted in the CardController's faces array.
    public int HandValue(){
        int totalValue = 0;
        int numAces = 0;

        foreach (int cardIndex in GetCards()){
            int rank = cardIndex % 13;
            if (rank < 8) {
                rank += 2; //These are the numbers cards in the suite. The raw rank value is offset by two, to compensate just add two.
                totalValue = totalValue + rank;
                Debug.Log(gameObject.name +": cardIndex " + cardIndex + " = " + rank);
            } 
            else if (rank > 7 && rank < 12){
                rank = 10; //These are the face cards and 10.
                totalValue = totalValue + rank;
                Debug.Log(gameObject.name +": cardIndex " + cardIndex + " = " + rank);
            }
            else {
                numAces++; //Otherwise its an ace.
            }
        }
        // Aces can be either 1 or 11. The ace's value will be based on which value keeps them under 21.
        // This process is repeated with each ace in the card stack.
        for (int i = 0; i < numAces; i++) {
            if (totalValue + 11 <= 21) {
                totalValue += 11;
                Debug.Log(gameObject.name +": ace = 11");
            }
            else {
                totalValue += 1;
                Debug.Log(gameObject.name +": ace = 1");
            }
        }
        Debug.Log(gameObject.name +": Hand Total Value: " + totalValue);
        return totalValue;
    }
    
    // publicly accessable read only method for cards list so as to maintain the integrety of the cardStack's source list.
    public IEnumerable<int> GetCards() {
        foreach (int card in cardStack) {
            yield return card;
        }
    }

    //The base list for the deck of cards
    private List<int> cardStack;

    void Awake () {
        cardStack = new List<int>();
        if (isDealerDeck) {
            CreateDeck();
        }
	}

    //Remove a card form the cardStack list
    public int RemoveCard() {
        int temp = cardStack[0];
        cardStack.RemoveAt(0);
        if (CardRemoved != null){
            CardRemoved (this, new CardEventArgs(temp));
        }
        return temp;
    }

    //Add a card to the end of the card stack list.
    public void AddCard(int card) {
        cardStack.Add(card);
        if (CardAdded != null) {
            CardAdded(this, new CardEventArgs(card));
        }
    }
   
    public void CreateDeck() {
		//Check if a deck does not exists create one.
        if (cardStack == null) {
            cardStack = new List<int>();
        }
		//If the it does exist empty it.
        else {
            cardStack.Clear();
        }
		//Add all the values to the list.
        for (int i = 0; i < 52; i++) {
            cardStack.Add(i);
        }

		//Shuffle card int values using Fisher Yates shuffle algorithim. 
        //Then assign the new value to the cards list.
		int n = cardStack.Count;
		while (n > 1) {
			n--;
			int k = Random.Range(0, n + 1);
			int temp = cardStack[k];
			cardStack[k] = cardStack[n];
			cardStack[n] = temp;
		}
    }

    public void DeleteInstance(){
        Destroy(gameObject);
    }
}