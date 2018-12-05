using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void CardEventHandler(object source, CardEventArgs e);

public class CardEventArgs {
	
	public int CardIndex {get; private set;}
	
	public CardEventArgs(int cardIndex) {
		CardIndex = cardIndex;
	}
}
