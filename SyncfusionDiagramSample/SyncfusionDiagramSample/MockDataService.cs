namespace SyncfusionDiagramSample
{
	public class MockDataService
	{
		protected readonly List<DisplayNode> myRefreshData;
		protected readonly List<DisplayNode> myDoLayoutData;

		public MockDataService()
		{
			myRefreshData = new List<DisplayNode>() 
			{
				new DisplayNode { Id = 1, NodeName = "Top Node", NodeType = "Main Item", ParentId = null },
				new DisplayNode { Id = 2, NodeName = "Child Node", NodeType = "First Sub Item", ParentId = 1 },
				new DisplayNode { Id = 3, NodeName = "Child Node", NodeType = "Second Sub Item", ParentId = 1 },
				new DisplayNode { Id = 4, NodeName = "Second Level Node", NodeType = "Child Item", ParentId = 3 },
				new DisplayNode { Id = 5, NodeName = "Third Level Node", NodeType = "Child Item", ParentId = 4 },
			};
			myDoLayoutData = new List<DisplayNode>();
			myDoLayoutData.AddRange(myRefreshData.ToList());
		}

		public List<DisplayNode> GetRefreshData()
		{
			return myRefreshData.ToList();
		}

		public void AddRefreshNode(DisplayNode displayNode)
		{
			myRefreshData.Add(displayNode);
		}

		public List<DisplayNode> GetDoLayoutData()
		{
			return myDoLayoutData.ToList();
		}

		public void AddDoLayoutNode(DisplayNode displayNode)
		{
			myDoLayoutData.Add(displayNode);
		}
	}
}
