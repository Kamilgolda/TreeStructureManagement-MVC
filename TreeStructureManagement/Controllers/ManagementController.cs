using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TreeStructureManagement.Data;
using TreeStructureManagement.Models;
using TreeStructureManagement.Repositories;

namespace TreeStructureManagement.Controllers
{
    public class ManagementController : Controller
    {
        private readonly INodesRepository _nodesRepository;

        public ManagementController(TreeStructureDbContext context, INodesRepository nodesRepository)
        {
            _nodesRepository = nodesRepository;
        }

        /// <summary>
        /// The main action to manage the structure
        /// </summary>
        /// <returns>Index view with list all tree elements</returns>
        // GET: Management
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            List<Node> list = _nodesRepository.GetNodes().Include(node => node.Children).AsEnumerable().Where(node => node.ParentId == null).ToList();
            return View(list);
            //return View(await _nodesRepository.GetNodes().OrderBy(node => node.Name).ToListAsync());
        }

        /// <summary>
        /// The main action to manage the structure
        /// </summary>
        /// <returns>Index view with list all tree sorted by name elements</returns>
        // POST: Management/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Index(string sortby)
        {
            List<Node> list = _nodesRepository.GetNodes().Include(node => node.Children).AsEnumerable().Where(node => node.ParentId == null).ToList();
            ViewBag.sortby = sortby;
            return View(list);
        }

        /// <summary>
        /// Adds a SelectList to the view and determines if the structure has a root
        /// </summary>
        /// <returns>Create view</returns>
        // GET: Management/Create
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            List<Node> nodes = await _nodesRepository.GetNodes().OrderBy(node => node.Name).ToListAsync();
            if (nodes.Count() != 0)
            {
                ViewData["ParentId"] = new SelectList(nodes, "Id", "Name");
                ViewBag.HasRoot = true;
            }
            else ViewBag.HasRoot = false;
            return View();
        }

        /// <summary>
        /// If element is validated adds a new element to the database and returns to the Index view, if not return to Create view
        /// </summary>
        /// <param name="node">New element</param>
        /// <returns>If element is validated Index view, if not Create view</returns>
        // POST: Management/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Name,ParentId")] Node node)
        {
            if (ModelState.IsValid)
            {
                await _nodesRepository.AddNode(node);
                return RedirectToAction(nameof(Index));
            }
            ViewData["ParentId"] = new SelectList(await _nodesRepository.GetNodes().OrderBy(node => node.Name).ToListAsync(), "Id", "Name", node.ParentId);
            return View(node);
        }

        /// <summary>
        /// Checks if the element with the given id exists, creates a SelectList for the view, checks if the element has children
        /// </summary>
        /// <param name="id">item ID</param>
        /// <returns>Edit view with node element</returns>
        // GET: Management/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null || id < 0) return NotFound();
            var node = await _nodesRepository.GetNodeByIdAsync(id);
            if (node == null) return NotFound();
            List<Node> nodes = await _nodesRepository.GetNodes().OrderBy(nd => nd.Name).Where(nd => nd.Id != node.Id).ToListAsync();
            ViewData["ParentId"] = new SelectList(nodes, "Id", "Name", node.ParentId);
            ViewBag.hasChildren = false;
            if (await _nodesRepository.GetNodes().AnyAsync(nd => nd.ParentId == node.Id))
            {
                ViewBag.hasChildren = true;
            }
            return View(node);
        }

        /// <summary>
        /// If the item is validated, updates it and returns to the Index view, if not, return to the Edit view
        /// </summary>
        /// <param name="id">item ID</param>
        /// <param name="node">item</param>
        /// <returns>If element is validated Index view, if not Edit view</returns>
        // POST: Management/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Name,ParentId")] Node node)
        {
            if (id != node.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    await _nodesRepository.UpdateNodeAsync(node);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_nodesRepository.NodeExists(id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ParentId"] = new SelectList(await _nodesRepository.GetNodes().OrderBy(nd => nd.Name).Where(nd => nd.Id != node.Id).ToListAsync(), "Id", "Name", node.ParentId);
            return View(node);
        }

        /// <summary>
        /// Removes the element with the given id and its descendants, then moves to the Index action
        /// </summary>
        /// <param name="id">item ID</param>
        /// <returns>Go to the index action</returns>
        // POST: Management/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            await _nodesRepository.RemoveWithAllChildrenAsync(id);
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Checks if the element with the given id exists, creates a SelectList for the view.
        /// </summary>
        /// <param name="id">Parent ID</param>
        /// <returns>DeleteAndMove view with DeleteAndMoveDto model</returns>
        // GET: Management/DeleteAndMove/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAndMove(long? id)
        {
            if (id == null || id < 0) return NotFound();
            var node = await _nodesRepository.GetNodeByIdAsync(id);
            if (node == null) return NotFound();
            ViewData["Parent"] = node.Name;
            List<Node> children = await _nodesRepository.GetChildren(id.Value);
            ViewBag.Children = children;
            List<Node> nodes = await _nodesRepository.GetNodes().OrderBy(nd => nd.Name).Where(nd => nd.Id != node.Id).ToListAsync();
            nodes = nodes.Except(children).ToList();
            ViewBag.NodesSelectList = new SelectList(nodes, "Id", "Name");
            DeleteAndMoveDto dto = new DeleteAndMoveDto() { NodeId = id.Value };
            return View(dto);
        }

        /// <summary>
        /// Deletes the parent and transfers the descendants.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto">Model with parent id and new parent id</param>
        /// <returns>If element is validated Index view, if not DeleteAndMove view</returns>
        // POST: Management/DeleteAndMove/5
        [HttpPost, ActionName("DeleteAndMove")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAndMoveConfirmed(long id, [Bind("NodeId, TargetId")] DeleteAndMoveDto dto)
        {
            if (id != dto.NodeId || id < 0) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    await _nodesRepository.ChangeParent(dto.NodeId, dto.TargetId);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!_nodesRepository.NodeExists(dto.TargetId)) return NotFound();
                    else throw;
                }
                var node = await _nodesRepository.GetNodeByIdAsync(dto.NodeId);
                if (node == null) return NotFound();
                await _nodesRepository.RemoveNodeAsync(node);
                return RedirectToAction(nameof(Index));
            }
            return View(dto);
        }

        /// <summary>
        /// Removes the actual data from the database
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ClearDb()
        {
            await _nodesRepository.RemoveAll();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Removes the actual data from the database and adds sample data
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> LoadDb()
        {
            await ClearDb();
            await _nodesRepository.AddData();
            return RedirectToAction(nameof(Index));
        }
    }
}
