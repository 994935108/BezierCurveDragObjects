using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjDragInteractive : IHandRayHoverInteractive
{
    private HandRayFocusUpdate mHand = null;

 
    void Update()
    {


        if (mHand != null)
        {

            if (Input.GetKeyUp(mHand.mKeyCode))
            {
                mHand = null;
            }
            else
            {
                transform.position += mHand.handMoveDir * 5;
                //Debug.LogError(mHand.handMoveDir);
            }
        }
    }

    internal override void HoverKeyDown(HandRayFocusUpdate hand)
    {

        if (mHand == null)
        {
            mHand = hand;
           

        }
    }
    internal override void HoverKey(HandRayFocusUpdate hand)
    {
       

        Debug.LogError(hand.handMoveDir);
    }
}