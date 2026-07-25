public class ResourceNode
{
    public readonly Dictionary<ResourceNode, (int Capacity, int Flow)> Next = new();
    public readonly HashSet<ResourceNode> Prev = new();
    public void AddNext(ResourceNode node, int capacity)
    {
        Next.Add(node, (capacity, 0));
        node.Prev.Add(this);
    }
    public static void IncreaseFlow(ResourceNode from, ResourceNode to, int amount)
    {
        from.Next[to] = (from.Next[to].Capacity, from.Next[to].Flow + amount);
    }
}
