using System;
using Silverfly.Nodes;

namespace Silverfly;

/// <summary>
/// Represents a base class for visiting and processing abstract syntax tree (AST) nodes.
/// </summary>
/// <typeparam name="TReturn">The type of the return value when visiting an AST node.</typeparam>
public abstract class NodeVisitor<TReturn> : NodeVisitorBase
{
    public virtual TReturn Visit(AstNode node)
    {
        if (!HasVisitor(node))
        {
            return VisitUnknown(node);
        }

        return AfterVisit((TReturn)InvokeVisitor(node));
    }

    public void For<TNode>(Func<TNode, TReturn> visitor)
        where TNode : AstNode
    {
        Visitors[typeof(TNode)] = visitor;
    }

    protected virtual TReturn VisitUnknown(AstNode node) => default;
    protected virtual TReturn AfterVisit(TReturn node) => node;
}

public abstract class NodeVisitor : NodeVisitorBase
{
    public void Visit(AstNode node)
    {
        if (!HasVisitor(node))
        {
            VisitUnknown(node);
        }

        InvokeVisitor(node);

        AfterVisit(node);
    }

    public virtual void Visit(BlockNode block)
    {
        foreach (var child in block.Children)
        {
            Visit(child);
        }
    }

    protected virtual void VisitUnknown(AstNode node)
    {
    }

    protected virtual void AfterVisit(AstNode node)
    {
    }

    public void For<TNode>(Action<TNode> visitor)
        where TNode : AstNode
    {
        Visitors[typeof(TNode)] = visitor;
    }
}
