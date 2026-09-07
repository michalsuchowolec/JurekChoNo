public class ResourceFlow
{
    private readonly ResourceBundle _budget;
    private readonly ResourceBundle _cost;
    private ResourceNode _source = new();
    private ResourceNode _sink = new();

    private ResourceFlow(ResourceBundle budget, ResourceBundle cost)
    {
        _budget = budget;
        _cost = cost;
    }

    public static bool CanAfford(ResourceBundle budget, ResourceBundle cost)
    {
        var flow = new ResourceFlow(budget, cost);
        return flow.CanAfford();
    }

    public bool CanAfford()
    {
        ConstructGraph();
        MaxFlow();
        foreach(var cost in _sink.Prev)
        {
            if(cost.Next[_sink].Capacity != cost.Next[_sink].Flow) return false;
        }
        return true;
    }

    public void ConstructGraph()
    {
        _source = new ResourceNode();
        _sink = new ResourceNode();
        var budgetNodes = new Dictionary<ResourceSymbol, ResourceNode>();
        var costNodes = new Dictionary<ResourceSymbol, ResourceNode>();

        foreach(var symbol in _cost.Amounts.Keys)
        {
            costNodes[symbol] = new ResourceNode();
            costNodes[symbol].AddNext(_sink, _cost[symbol]);
        }
        foreach(var budgetSymbol in _budget.Amounts.Keys)
        {
            budgetNodes[budgetSymbol] = new ResourceNode();
            _source.AddNext(budgetNodes[budgetSymbol], _budget[budgetSymbol]);
            foreach(var costSymbol in costNodes.Keys)
            {
                if (budgetSymbol.Overlaps(costSymbol))
                {
                    budgetNodes[budgetSymbol].AddNext(costNodes[costSymbol], _budget[budgetSymbol]);
                }
            }
        }
    }

    public void MaxFlow()
    {
        while (true)
        {
            var stack = new Stack<ResourceNode>();
            stack.Push(_source);
            if(!DSF(stack, int.MaxValue)) break;
        }

    }

    public bool DSF(Stack<ResourceNode> path, int minFlow)
    {
        var result = false;
        if(path.Peek() == _sink)
        {
            var list = path.ToList();
            list.Reverse();
            for(int i = 0; i < path.Count - 1; i++)
            {
                if(list[i].Next.ContainsKey(list[i + 1]))
                {
                    ResourceNode.IncreaseFlow(list[i], list[i + 1], minFlow);
                }
                else
                {
                    ResourceNode.IncreaseFlow(list[i + 1], list[i], -minFlow);
                }
            }
            return true;
        }
        foreach(var next in path.Peek().Next.Keys)
        {
            if(path.Contains(next)) continue;
            var capacity = path.Peek().Next[next].Capacity;
            var flow = path.Peek().Next[next].Flow;
            var newMinFlow = capacity - flow;
            if(newMinFlow <= 0) continue;
            path.Push(next);
            result |= DSF(path, Math.Min(minFlow, capacity - flow));
            path.Pop();
        }
        foreach(var prev in path.Peek().Prev)
        {
            if(path.Contains(prev)) continue;
            var flow = prev.Next[path.Peek()].Flow;
            if(flow <= 0) continue;
            path.Push(prev);
            result |= DSF(path, Math.Min(minFlow, flow));
            path.Pop();
        }
        return result;
    }
}
