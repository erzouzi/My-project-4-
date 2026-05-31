using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseState
{
    protected FSMController fsm;
    protected Animator animator => fsm.animator;
    protected Blackboard bb => fsm.blackboard;
    public BaseState(FSMController fsm)
    {
        this.fsm = fsm;
    }

    public abstract void OnEnter();
    public abstract void OnExit();
    public abstract void OnUpdate();

    public virtual void OnAnimatorMove() { }
}
