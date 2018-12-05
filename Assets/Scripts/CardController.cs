using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardController : MonoBehaviour
{
    // This script controls the functionality of the individual cards.
    
    public SpriteRenderer spriteRenderer;
    public Sprite[] faces;
    public Sprite cardBack;
    public int cardIndex;
   
    void Awake() {
        if (spriteRenderer == null) {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    //Controls whether or not the cards face is shown and displays the texture based on the index assigned to the card.
    public void ToggleFace(bool showFace) {
        //TODO: Animate card flip
        if (showFace) {
            spriteRenderer.sprite = faces[cardIndex];
        }
        else {
            spriteRenderer.sprite = cardBack;
        }
    }
}
