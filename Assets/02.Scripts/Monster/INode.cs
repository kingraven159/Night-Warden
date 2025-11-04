using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INode
{
    public enum ENodeState
    {
        Running,
        Success,
        Failure
    }
    public ENodeState Evaluate();
}
public sealed class ActionNode : INode
{
    Func<INode.ENodeState> _onUpdate;

    public ActionNode(Func<INode.ENodeState> onUpdate)
    {
        _onUpdate = onUpdate;
    }

    public INode.ENodeState Evaluate() => _onUpdate?.Invoke() ?? INode.ENodeState.Running;
}
public sealed class SelectorNode : INode
{
    List<INode> _childs;

    public SelectorNode(List<INode> childs)
    {
        _childs = childs;
    }

    public INode.ENodeState Evaluate()
    {
        if (_childs == null)
            return INode.ENodeState.Failure;

        foreach (var child in _childs)
        {
            var result = child.Evaluate();
            if (result != INode.ENodeState.Success)
                return result; //Running or Failure
        }
        return INode.ENodeState.Success;
    }
}
public sealed class SequenceNode : INode
{
    List<INode> _childs;
    public SequenceNode(List<INode> childs)
    {
        _childs = childs;
    }

    public INode.ENodeState Evaluate()
    {
        if (_childs == null || _childs.Count == 0)
            return INode.ENodeState.Failure;

        foreach (var child in _childs)
        {
            var result = child.Evaluate();
            if(result != INode.ENodeState.Failure)
            {
                return result; //Running or Success

            }
       }
        return INode.ENodeState.Failure;
    }
}

