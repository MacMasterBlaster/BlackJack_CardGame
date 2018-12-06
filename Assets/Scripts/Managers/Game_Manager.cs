using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
public class Game_Manager : MonoBehaviour {
	
	// This script controls all the actual game logic. 
	// It controls the dealing of cards to the dealer and player hands as well as tracking scores and determining who wins.

    // Cardstack prefabs
	public GameObject playerPrefab;
	public GameObject dealerPrefab;
	public GameObject deckPrefab;

    // UI Panels
    public GameObject mainMenuPanel;
    public GameObject interactMenuPanel;
    public GameObject ScorePanel;
    public GameObject statsPanel;

    // UI References
	public Button newGameButton;
	public Button hitMeButton;
	public Button stickButton;
    public Button gameStatsButton;
    public TextMeshProUGUI startButtonText;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI hitMeText;
    public TextMeshProUGUI stickText;
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI dealerValueText;
    public TextMeshProUGUI playerValueText;

    private int dealersFirstCard = -1;
    private bool dealerBusts = false;
    private bool playerBusts = false;
	private CardStackController player;
	private CardStackController dealer;
	private CardStackController deck;

    // Locally stored score values
    private int dealerWins;
    private int playerWins;
    private int houseWins;

#region ButtonFunctions
	// Deals the initial hands of cards to the dealer and the player and creates the deck.
	public void StartNewGame() {
        // If cardStack instances already exist, destroy them.
		if (player != null) Destroy(player.gameObject);
		if (deck != null)   Destroy(deck.gameObject);
		if (dealer != null) Destroy(dealer.gameObject);
        
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
        interactMenuPanel.SetActive(true);
        ScorePanel.SetActive(true);
        mainMenuPanel.SetActive(false);
        statsPanel.SetActive(false);
        SetInteractState(true);
        playerValueText.text = Convert.ToString(player.HandValue());
        dealerValueText.text = "";
        gameOverText.text = "";

        // Pulls stores stats and stores values locally.
        PullStats();
	}

    // Called by the Hit button in the UI. Pulls a card from the deck and adds it to the players hand.
	public void HitMe() {
        player.AddCard(deck.RemoveCard());
        playerValueText.text = Convert.ToString(player.HandValue());
        if (player.HandValue() > 21) {
            // Disable buttons to stop player from drawing more cards.
            SetInteractState(false);
            playerBusts = true;
            StartCoroutine("DealersTurn");
        }
    }

    // Called by the stick button in the UI. Ends the players turn and starts the dealers turn.
    public void Stick() {
        SetInteractState(false);
        StartCoroutine("DealersTurn");
    }
    
    // Method called by the stats button to display the game stats.
    public void ViewStats() {
        PullStats();
        gameOverText.text = string.Format("House Wins:{0}\nDealer Wins:{1}  Player Wins:{2}", houseWins, dealerWins, playerWins);
    }

    public void Quit(){
        Application.Quit();
    }
#endregion

#region GameLogic
    // Unique hit method for dealer. Saves a reference to the first card in the dealers stack so it can be flipped when the game ends.
    private void HitDealer() {
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

    // This initiates the end phase of the program.
    // The dealer card stack continues to add cards until the dealer either hits at least 17 or busts.
    // Then GameOver() is called and the the winner is determined.
    IEnumerator DealersTurn() {
        while (dealer.HandValue() <  17) {
            HitDealer();
            yield return new WaitForSeconds(1f);
        }

        if (dealer.HandValue() > 21) {
            dealerBusts = true;
            yield return new WaitForSeconds(.2f);
        }

        // Once the dealer's card stack is is 17 or greater the dealer's first card is set to be visible so 
        // the player can see the dealer's hand.
        CardStackViewer cardStackViewer = dealer.GetComponent<CardStackViewer>();
        cardStackViewer.Toggle(dealersFirstCard, true);     
        dealerValueText.text = Convert.ToString(dealer.HandValue());

        yield return new WaitForSeconds(1f);
        GameOver();
    }

    private void PullStats() {
        dealerWins = PlayerPrefs.GetInt("DealerWins");
        playerWins = PlayerPrefs.GetInt("PlayerWins");
        houseWins = PlayerPrefs.GetInt("HouseWins");
    }

    // Updates and saves the win scores to the playerPrefs to track wins between games.
    private void UpdateStatsFile() {
        PlayerPrefs.SetInt("DealerWins", dealerWins);
        PlayerPrefs.SetInt("PlayerWins", playerWins);
        PlayerPrefs.SetInt("HouseWins", houseWins);
    }

    // Determines the winner.
    private void GameOver() {
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
        SetUIGameOver();
        UpdateStatsFile();
    }

    // Toggles the active states of the buttons in the interaction menu.
    private void SetInteractState(bool isInteractable) {
        hitMeButton.interactable = isInteractable;
        stickButton.interactable = isInteractable;
        if(isInteractable) {
            // Sets the text of the active UI buttons to white.
            hitMeText.material.SetColor("_FaceTex", Color.white);
            stickText.material.SetColor("_FaceTex", Color.white);
        }
        else {
            // Sets the text of the disabled UI buttons to grey color.
            hitMeText.material.SetColor("_FaceTex", new Color(139f,139f, 139f));
            stickText.material.SetColor("_FaceTex", new Color(139f,139f, 139f));
        }
        
    }
    
    // Updates the UI to display the values of the player's and dealer's hands. It also reactivates the main menu UI panel.
    private void SetUIGameOver() {
        mainMenuPanel.SetActive(true);
        statsPanel.SetActive(true);
        interactMenuPanel.SetActive(false);
        startButtonText.text = "Play Again";
        
        if (playerBusts) {
            playerValueText.text = Convert.ToString(player.HandValue()) + "\nBust!";
        }
        else {
            playerValueText.text = Convert.ToString(player.HandValue());
        }
        if (dealerBusts) {
            dealerValueText.text = Convert.ToString(dealer.HandValue()) + "\nBust!";
        }
        else {
            dealerValueText.text = Convert.ToString(dealer.HandValue());
        }
    }
#endregion
}