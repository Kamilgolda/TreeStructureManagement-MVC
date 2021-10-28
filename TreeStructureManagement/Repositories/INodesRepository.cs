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
    }
}
