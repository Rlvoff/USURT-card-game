using UnityEngine;

public class StepTrigger : MonoBehaviour
{
    public int newSortingOrder = 1;//переход на слой 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SpriteRenderer playerSprite = other.GetComponent<SpriteRenderer>();
            if (playerSprite != null) 
            {
                playerSprite.sortingOrder = newSortingOrder;
            }
        }   
    }
}
