using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]

public class HandRayFocusUpdate : MonoBehaviour
{

    
    private Vector3 mRayDire;//射线的方向
    [HideInInspector]
    public RaycastHit mRaycastHit;//碰撞信息
    private IHandRayHoverInteractive mIHandHoverInteractive = null;//记录当前选中的物体
    private Transform mFocusPoint;//射线末端的小球
    private LineRenderer mLineRenderer;
    private bool mIsDrag = false;
    private Vector3 mDragPoint;//开始拖动时候的物体碰撞点的本地坐标
    private Vector3 mRayOriginPoint;//发射射线的点
    public KeyCode mKeyCode;
    public bool mIsLeftHand;
    private float mCurrentAngleY;
    private float mCurrentAngleX;


    void Start()
    {
        mLineRenderer = GetComponent<LineRenderer>();//获取LineRender
        mFocusPoint = Instantiate(Resources.Load<GameObject>("FocusPoint")).transform;//加载Resources下面名字为FocusPoint的预制体
    }
    //绘制曲线
    void DrawLineRenderCurve(LineRenderer lineRenderer, Vector3[] points)
    {
        for (int i = 1; i <= 100; i++)
        {
            float t = i / (float)100;
            Vector3 pixel = CalculateBezierPoint(t, points[0], points[1], points[2]);
            lineRenderer.positionCount = i;
            lineRenderer.SetPosition(i - 1, pixel);
        }

    }
    //计算贝塞尔曲线的点
    Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector3 p = uu * p0;
        p += 2 * u * t * p1;
        p += tt * p2;
        return p;
    }
    /// <summary>
    /// 更新末端小球的位置。并且重新设置射线的方向
    /// </summary>
    /// <param name="point"> 末端小球的新位置</param>
    public void UpdateFocusPoint(Vector3 point, bool isDrag)
    {
        mRayOriginPoint = transform.position;
        Vector3[] middlePoint = new Vector3[3];
        middlePoint[0] = mRayOriginPoint;
        middlePoint[1] = mRayOriginPoint;
        middlePoint[2] = point;
        mRayDire = transform.forward;
        if (isDrag)
        {
            middlePoint[2] = mRaycastHit.transform.TransformPoint(mDragPoint);
            middlePoint[1] = mRayOriginPoint + (mRayDire).normalized * ((middlePoint[2] - mRayOriginPoint).magnitude);
            DrawLineRenderCurve(mLineRenderer, middlePoint);
            mFocusPoint.position = middlePoint[2];
            mFocusPoint.transform.localScale = Vector3.one * (Mathf.Clamp((Vector3.Magnitude(point - mRayOriginPoint) / 100), 0.03f, 0.08f));//计算小球的缩放
        }
        else
        {
            DrawLineRenderCurve(mLineRenderer, middlePoint);
            mFocusPoint.position = point;
            mFocusPoint.transform.localScale = Vector3.one * (Mathf.Clamp((Vector3.Magnitude(point - mRayOriginPoint) / 100), 0.03f, 0.08f));//计算小球的缩放

        }
    }
 

    // Update is called once per frame
    void Update()
    {
        ///
         //控制手的旋转
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        mCurrentAngleY += x;
        mCurrentAngleX += y;
        Mathf.Clamp(mCurrentAngleY,-90,90);
        Mathf.Clamp(mCurrentAngleX, -90, 90);
        transform.localEulerAngles = new Vector3(-mCurrentAngleX, mCurrentAngleY,0) ;
        ///

        if (mIsDrag)
        {
            UpdateFocusPoint(mRaycastHit.point, true);
            if (Input.GetKeyUp(mKeyCode))
            {
                mIsDrag = false;
            }
        }
        else
        {
            UpdateState();
        }
    }

    private void HoverEnter()
    {
        mIHandHoverInteractive.SendMessage("HoverEnter", this);
    }
    private void Hover()
    {
        mIHandHoverInteractive.SendMessage("Hover", this);
        if (Input.GetKeyDown(mKeyCode))
        {
            HoverKeyDown();
        }
        else
        {
            if (Input.GetKey(mKeyCode))
            {
                HoverKey();
                if (!mIsDrag)
                {
                    mDragPoint = mIHandHoverInteractive.transform.InverseTransformPoint(mRaycastHit.point);
                    mIsDrag = true;
                }
            }
            else
            {
                if (Input.GetKeyUp(mKeyCode))
                {
                    HoverKeyUp();
                }
            }
        }
    }
    private void HoverExit()
    {
        mIHandHoverInteractive.SendMessage("HoverExit", this);
    }
    private void HoverKeyDown()
    {
        mIHandHoverInteractive.SendMessage("HoverKeyDown", this);
    }
    private void HoverKey()
    {
        mIHandHoverInteractive.SendMessage("HoverKey", this);
    }
    private void HoverKeyUp()
    {
        mIHandHoverInteractive.SendMessage("HoverKeyUp", this);
    }
    //更新射线的状态
    private void UpdateState()
    {

        mRayOriginPoint = transform.position;
        mRayDire = transform.forward;

        Ray ray = new Ray((mRayOriginPoint), mRayDire);//从手腕的位置向前发射一条射线

        if (Physics.Raycast(ray, out mRaycastHit,Mathf.Infinity))
        {
            UpdateFocusPoint(mRaycastHit.point, false);//实时更新小球得位置

            IHandRayHoverInteractive mUI_InteractiveBaseTemp = mRaycastHit.transform.GetComponent<IHandRayHoverInteractive>(); //获取当前碰撞的物体上面的UI_InteractiveBase脚本

            if (mUI_InteractiveBaseTemp != null) //如果获取的脚本不为空
            {
                if (mIHandHoverInteractive == null) //如果mUI_InteractiveBase为空,表示刚进入,调用对应的进入方法
                {
                    mIHandHoverInteractive = mUI_InteractiveBaseTemp;
                    HoverEnter();
                }
                else
                {
                    if (mIHandHoverInteractive == mUI_InteractiveBaseTemp)
                    {

                        Hover(); 
                    }
                    else
                    {
                        mIHandHoverInteractive.HoverExit(this);
                        mIHandHoverInteractive = mUI_InteractiveBaseTemp;
                        HoverEnter();
                    }
                }
            }
            else
            {
                if (mIHandHoverInteractive != null)
                {
                    mIHandHoverInteractive.HoverExit(this);
                    mIHandHoverInteractive = null;
                }
            }
        }
        else
        {
            if (mIHandHoverInteractive != null)
            {
                HoverExit();
                mIHandHoverInteractive = null;
            }
            UpdateFocusPoint(mRayOriginPoint + mRayDire * 5, false);
        }
    }
}
