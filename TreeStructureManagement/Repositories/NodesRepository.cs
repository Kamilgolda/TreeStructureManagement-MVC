using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TreeStructureManagement.Data;
using TreeStructureManagement.Models;

namespace TreeStructureManagement.Repositories
{
    /// <summary>
    /// Context management class
    /// </summary>
    public class NodesRepository : INodesRepository
    {
        private readonly TreeStructureDbContext _context;

        public NodesRepository(TreeStructureDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Removes the actual data from the database and adds sample data
        /// </summary>
        public async Task AddData()
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
            $"DBCC CHECKIDENT(Nodes, RESEED, 0); SET IDENTITY_INSERT Nodes ON; INSERT INTO Nodes(Id, Name, ParentId)VALUES(1, 'Animals', null),(2, 'Mammals', 1),(3, 'Birds', 1),(4, 'Fish', 1),(5, 'Reptiles', 1),(6, 'Invertebrates', 1),(7, 'Amphibians', 1),(8, 'frogs', 7),(9, 'newts', 7),(10, 'salamanders', 7),(11, 'toads', 7),(12, 'albatrosses', 3),(13, 'chickens', 3),(14, 'falcons', 3),(15, 'owls', 3),(16, 'parrots', 3),(17, 'penguins', 3),(18, 'eels', 4),(19, 'salmon', 4),(20, 'seahorses', 4),(21, 'elephants', 2),(22, 'hamsters', 2),(23, 'rabbits', 2),(24, 'rhinoceroses', 2),(25, 'whales', 2),(26, 'crocodiles', 5),(27, 'snakes', 5),(28, 'tortoises', 5),(29, 'arthropoda', 6),(30, 'nematoda', 6),(31, 'mollusca', 6),(32, 'clams', 31),(33, 'roundworms', 30),(34, 'spiders', 29),(35, 'cockatiels', 16),(36, 'amazons', 16),(37, 'macaws', 16),(38, 'dogs', 2);");
        }

        /// <summary>
        /// Removes the actual data from the database
        /// </summary>
        public async Task RemoveAll()
        {
            _context.Nodes.RemoveRange(_context.Nodes);
            await _context.SaveChangesAsync();
        }

        /// <returns>Iqueryable nodes to for subsequent inquiries</returns>
        public IQueryable<Node> GetNodes()
        {
            return _context.Nodes.AsQueryable();
        }

        /// <summary>
        /// Adds a new item to database
        /// </summary>
        /// <param name="node">New element</param>
        public async Task AddNode(Node node)
        {
            await _context.Nodes.AddAsync(node);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Gets an element with the given id
        /// </summary>
        /// <param name="id">item ID</param>
        /// <returns>item</returns>
        public async Task<Node> GetNodeByIdAsync(long? id)
        {
            var node = await _context.Nodes.FirstOrDefaultAsync(node => node.Id == id);
            return node;
        }

        /// <summary>
        /// Updates the element with the given id
        /// </summary>
        /// <param name="node">item ID</param>
        public async Task UpdateNodeAsync(Node node)
        {
            _context.Nodes.Update(node);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Checks if the element with the given identifier exists
        /// </summary>
        /// <param name="id">item ID</param>
        /// <returns>true or false</returns>
        public bool NodeExists(long id)
        {
            return _context.Nodes.Any(node => node.Id == id);
        }

        /// <summary>
        /// Adds all descendants of the parent to the list
        /// </summary>
        /// <param name="parentId">parent ID</param>
        /// <param name="list">list with children items</param>
        public async Task GetAllChildren(long parentId, List<Node> list)
        {
            List<Node> children = await GetNodes().Where(nd => nd.ParentId == parentId).ToListAsync();
            if (children.Count != 0)
            {
                list.AddRange(children);
                foreach (var child in children)
                {
                    await GetAllChildren(child.Id, list);
                }
            }
        }

        /// <summary>
        /// Removes parent and descendants
        /// </summary>
        /// <param name="id">Parent ID</param>
        public async Task RemoveWithAllChildrenAsync(long id)
        {
            var node = await _context.Nodes.FindAsync(id);
            List<Node> toRemove = new List<Node>() { node };
            await GetAllChildren(id, toRemove);
            _context.Nodes.RemoveRange(toRemove);
            await _context.SaveChangesAsync();
        }

        /// <param name="ParentId">Parent ID</param>
        /// <returns>List with children's items.</returns>
        public async Task<List<Node>> GetChildren(long ParentId)
        {
            return await GetNodes().Where(nd => nd.ParentId == ParentId).ToListAsync();
        }

        /// <summary>
        /// Transfers all of the parent's children to the new parent
        /// </summary>
        /// <param name="oldParentId">Parent ID</param>
        /// <param name="newParentId">New parent ID</param>
        public async Task ChangeParent(long oldParentId, long newParentId)
        {
            var children = await GetChildren(oldParentId);
            foreach (var child in children)
            {
                child.ParentId = newParentId;
            }
            _context.Nodes.UpdateRange(children);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Removes the item
        /// </summary>
        /// <param name="node">Item</param>
        public async Task RemoveNodeAsync(Node node)
        {
            _context.Nodes.Remove(node);
            await _context.SaveChangesAsync();
        }

    }
}
