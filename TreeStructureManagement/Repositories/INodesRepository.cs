using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TreeStructureManagement.Models;

namespace TreeStructureManagement.Repositories
{
    public interface INodesRepository
    {
        Task AddData();
        Task RemoveAll();
        IQueryable<Node> GetNodes();
        Task AddNode(Node node);
        Task<Node> GetNodeByIdAsync(long? id);
        Task UpdateNodeAsync(Node node);
        bool NodeExists(long id);
        Task GetAllChildren(long parentId, List<Node> list);
        Task RemoveWithAllChildrenAsync(long id);
        Task<List<Node>> GetChildren(long ParentId);
        Task ChangeParent(long oldParentId, long newParentId);
        Task RemoveNodeAsync(Node node);
    }
}
