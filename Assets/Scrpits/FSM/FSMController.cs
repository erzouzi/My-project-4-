using System;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;


public enum StateType
{
    Idle,       //待机
    Move,       //移动

    Attack,     //攻击
    Skill,      //技能

    Hit,        //受击
    Defence,    //防御
    Dodge,      //闪避

    Die,        //死亡
}
public class FSMController
{
    private BaseState curState;
    private StateType curType = (StateType)(-1);
    public StateType CurType => curType;
    private Dictionary<StateType, BaseState> statesDic;
    public UnitController unitController;
    public Animator animator;
    public Blackboard blackboard;
    public FSMController(Animator animator, UnitController controller)
    {
        this.statesDic = new Dictionary<StateType, BaseState>();
        this.animator = animator;
        this.unitController = controller;
        this.blackboard = new Blackboard();
    }
    public void AddState(StateType type, BaseState state)
    {
        if (statesDic.ContainsKey(type))
            return;
        statesDic.Add(type, state);
    }
    public void SwitchType(StateType type)
    {
        if (!statesDic.ContainsKey(type))
        {
            Debug.Log("字典里没有这个状态");
            return;
        }

        if (curState != null)
        {
            curState.OnExit();
        }
        curState = statesDic[type];
        curType = type;
        curState.OnEnter();
    }

    public void OnUpdate()
    {
        curState?.OnUpdate();
    }

    public void OnAnimatorMove()
    {
        curState?.OnAnimatorMove();
    }
}