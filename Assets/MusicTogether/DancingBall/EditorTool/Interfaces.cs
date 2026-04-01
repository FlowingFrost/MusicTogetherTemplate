using MusicTogether.DancingBall.Scene;

namespace MusicTogether.DancingBall.EditorTool
{
	public interface IEditManager
	{
		public Factory Factory { get; }
		public EditorActionDispatcher Dispatcher { get; }
		public void RecreateMapRoadList(EditorActionContext ctx);
		public void RefreshAllRoads(EditorActionContext ctx);
		public void RefreshRoadBlocks(EditorActionContext ctx);
		public void OnRoadBlockCountChanged(EditorActionContext ctx);
		public void OnBlockDisplacementRuleChanged(EditorActionContext ctx);
		public void RefreshBlockInfoDisplay(EditorActionContext ctx);
		public void RemoveBlocks(IRoad road, System.Collections.Generic.List<IBlock> blocksToRemove);
	}
}