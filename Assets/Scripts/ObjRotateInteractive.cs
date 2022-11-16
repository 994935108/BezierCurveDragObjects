using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjRotateInteractive : IHandRayHoverInteractive
{
    private HandRayFocusUpdate mHand = null;

    private Vector3 mHandEular=Vector3.zero;//上一次的角度
    private float mAngleDelta=0;//旋转角度的增量
    void  Update()
    {


        if (mHand != null)
        {
           
            if (Input.GetKeyUp(mHand.mKeyCode))
            {
                mHand = null;
            }
            else
            {
                mAngleDelta = mHandEular.y-mHand.transform.eulerAngles.y ;
                mHandEular =mHand.transform.eulerAngles;
                transform.rotation *= Quaternion.Euler(Vector3.up * mAngleDelta);//计算新的旋转位置
            }
        }
    }
  
    internal override void HoverKeyDown(HandRayFocusUpdate hand)
    {

        if (mHand == null)
        {
            mHand = hand;
            mHandEular = mHand.transform.eulerAngles;//记录按下时候的角度
           
        }
    }

}
