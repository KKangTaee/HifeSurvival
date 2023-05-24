using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Simple_Char_Move : MonoBehaviour
{
    [SerializeField]
    private float speed = 1;
    
    private SpriteRenderer characterSprite;

    // Start is called before the first frame update
    void Start()
    {
        characterSprite = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        MoveCharacter();

        FlipSpriteToMovement();
    }

    void MoveCharacter()
    {
        //I am putting these placeholder variables here, to make the logic behind the code easier to understand
        //we differentiate the movement speed between horizontal(x) and vertical(y) movement, since isometric uses "fake perspective"
        float horizontalMovement = Input.GetAxisRaw("Horizontal") * speed * Time.deltaTime;
        //since we are using this with isometric visuals, the vertical movement needs to be slower
        //for some reason, 50% feels too slow, so we will be going with 75%
        float verticalMovement = Input.GetAxisRaw("Vertical") * speed * 0.5f * Time.deltaTime;


        this.transform.Translate(horizontalMovement, verticalMovement, 0);
    }

    //if the player moves left, flip the sprite, if he moves right, flip it back, stay if no input is made
    void FlipSpriteToMovement()
    {
        if(characterSprite != null )
        {
            if (Input.GetAxisRaw("Horizontal") < 0)
                characterSprite.flipX = true;
            else if (Input.GetAxisRaw("Horizontal") > 0)
                characterSprite.flipX = false;
        }
    }
}
