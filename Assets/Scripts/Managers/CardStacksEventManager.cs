using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void CardRemovedEventHandler(object source, CardRemovedEventArgs e);

public class CardRemovedEventArgs {
	
	public int CardIndex {get; private set;}
	
	public CardRemovedEventArgs(int cardIndex) {
		CardIndex = cardIndex;
	}
}
