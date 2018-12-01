using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebuggingGameManager : MonoBehaviour {

	public CardStackController player;
	public CardStackController dealer;

	public void DealCard(){
		 player.AddCard(dealer.RemoveCard());
	}
}
