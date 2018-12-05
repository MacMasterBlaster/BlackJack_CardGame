using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Game_Manager : MonoBehaviour {

	/* 
	This script controls all the actual game logic. 
	It controls the dealing of cards to the dealer and player hands as well as tracking scores and determining who wins.
	*/

	public GameObject playerPrefab;
	public GameObject dealerPrefab;
	public GameObject deckPrefab;

    // UI References
	public Button newGameButton;
	public Button hitMeButton;
	public Button stickButton;
    public Button gameStatsButton;
    public TextMeshProUGUI startButtonText;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI dealerValueText;
    public TextMeshProUGUI playerValueText;

    private int dealersFirstCard = -1;
    private bool dealerBusts = false;
    private bool playerBusts = false;
	private CardStackController player;
	private CardStackController dealer;
	private CardStackController deck;

    private int dealerWins;
    private int playerWins;
    private int houseWins;

	// Deals the initial hands of cards to the dealer and the player and creates the deck.
	public void StartNewGame() {
        // If cardStack instances already exist, destroy them.
		if (player != null) player.DeleteInstance();
		if (deck != null)   deck.DeleteInstance();
		if (dealer != null) dealer.DeleteInstance();
        
        // Reset local private variables.
        dealersFirstCard = -1;
        dealerBusts = false;
        playerBusts = false;
        
        // Instantiate new cardStack instances.
        player = Instantiate(playerPrefab).GetComponent<CardStackController>();
        deck = Instantiate(deckPrefab).GetComponent<CardStackController>();
        dealer = Instantiate(dealerPrefab).GetComponent<CardStackController>();

        // Deal out initial cards to player and dealer.
        for (int i = 0; i < 2; i++) {
			player.AddCard(deck.RemoveCard());
			HitDealer();
		}
        // Update UI
        newGameButton.interactable = false;
        hitMeButton.interactable = true;
        stickButton.interactable = true;
        playerValueText.text = "";
        dealerValueText.text = "";
        gameOverText.text = "";

        PullStats();
	}

	public void HitMe() {
        player.AddCard(deck.RemoveCard());
        if (player.HandValue() > 21) {
            hitMeButton.interactable = false;
            stickButton.interactable = false;
            playerBusts = true;
            StartCoroutine("DealersTurn");
        }
    }

    public void Stick() {
        hitMeButton.interactable = false;
        stickButton.interactable = false;
        StartCoroutine("DealersTurn");
    }

    void HitDealer() {
        int card = deck.RemoveCard();

        if (dealersFirstCard < 0) {
            dealersFirstCard = card;
        }

        dealer.AddCard(card);
        
        // Every card after the first card should be visible.
        if (dealer.CardCount > 1) {
            CardStackViewer cardStackViewer = dealer.GetComponent<CardStackViewer>();
            cardStackViewer.Toggle(card, true);
        }
    }

    void GameOver() {
        if (dealerBusts && playerBusts) {
            gameOverText.text = "House Wins!"; // Should only appear if both the player and the dealer bust.
            houseWins++;
        }
        else if ((!dealerBusts && dealer.HandValue() >= player.HandValue()) || (player.HandValue() == dealer.HandValue()) || (playerBusts && !dealerBusts))  {
            gameOverText.text = "Dealer Wins!";
            dealerWins++;
        }
        else if ((dealerBusts && !playerBusts) || (player.HandValue() > dealer.HandValue()) || (player.HandValue() <= 21 && player.HandValue() > dealer.HandValue())) {
            gameOverText.text = "Player Wins!";
            playerWins++;
        }
        UpdateUI();
        UpdateStatsFile();
    }

    void UpdateUI() {
        hitMeButton.interactable = false;
        stickButton.interactable = false;
        newGameButton.interactable = true;
        startButtonText.text = "Play Again";
        if (playerBusts) {
            playerValueText.text = Convert.ToString(player.HandValue()) + "\nPlayer Busts!";
        }
        else {
            playerValueText.text = Convert.ToString(player.HandValue());
        }
        if (dealerBusts) {
            dealerValueText.text = Convert.ToString(dealer.HandValue()) + "\nDealer Busts!";
        }
        else {
            dealerValueText.text = Convert.ToString(dealer.HandValue());
        }
    }

    // This initiates the end phase of the program.
    // The dealer card stack continues to add cards until the dealer either hits atleast 17 or busts.
    // Then GameOver() is called and the the winner is determined.
    IEnumerator DealersTurn() {
        while (dealer.HandValue() <  17) {
            HitDealer();
            yield return new WaitForSeconds(1f);
        }

        if (dealer.HandValue() > 21) {
            dealerBusts = true;
            yield return new WaitForSeconds(1f);
        }

        // Once the dealer's card stack is is 17 or greater the dealer's first card is set to be visible so 
        //the player can see the dealer's hand.
        CardStackViewer cardStackViewer = dealer.GetComponent<CardStackViewer>();
        cardStackViewer.Toggle(dealersFirstCard, true);
        GameOver();
    }

    public void PullStats() {
        dealerWins = PlayerPrefs.GetInt("DealerWins");
        playerWins = PlayerPrefs.GetInt("PlayerWins");
        houseWins = PlayerPrefs.GetInt("HouseWins");
        //Debug.Log(string.Format("pulled stats: Dealer {0}, Player {1}, House {2}", dealerWins, playerWins, houseWins));
    }

    public void UpdateStatsFile() {
        PlayerPrefs.SetInt("DealerWins", dealerWins);
        PlayerPrefs.SetInt("PlayerWins", playerWins);
        PlayerPrefs.SetInt("HouseWins", houseWins);
        //Debug.Log(string.Format("Updated stats: Dealer {0}, Player {1}, House {2}", dealerWins, playerWins, houseWins));
    }
    
    public void ViewStats() {
        PullStats();
        gameOverText.text = string.Format("Dealer Wins: {0}  Player Wins: {1}  House Wins: {2}", dealerWins, playerWins, houseWins);
    }

}
