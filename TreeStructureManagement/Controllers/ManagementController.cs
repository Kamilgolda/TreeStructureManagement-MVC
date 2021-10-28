using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly TreeStructureDbContext _context;
        private readonly INodesRepository _nodesRepository;

        public ManagementController(TreeStructureDbContext context, INodesRepository nodesRepository)
        {
            _context = context;
            _nodesRepository = nodesRepository;
        }

        /// <summary>
        /// The main action to manage the structure
        /// </summary>
        /// <returns>Index view with list all tree elements</returns>
        // GET: Management
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

        // GET: Management/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var node = await _context.Nodes.FindAsync(id);
            if (node == null)
            {
                return NotFound();
            }
            ViewData["ParentId"] = new SelectList(_context.Nodes, "Id", "Name", node.ParentId);
            return View(node);
        }

        // POST: Management/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Name,ParentId")] Node node)
        {
            if (id != node.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(node);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NodeExists(node.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ParentId"] = new SelectList(_context.Nodes, "Id", "Name", node.ParentId);
            return View(node);
        }

        // GET: Management/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var node = await _context.Nodes
                .Include(n => n.Parent)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (node == null)
            {
                return NotFound();
            }

            return View(node);
        }

        // POST: Management/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var node = await _context.Nodes.FindAsync(id);
            _context.Nodes.Remove(node);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NodeExists(long id)
        {
            return _context.Nodes.Any(e => e.Id == id);
        }

        public async Task<IActionResult> ClearDb()
        {
            await _nodesRepository.RemoveAll();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> LoadDb()
        {
            await ClearDb();
            await _nodesRepository.AddData();
            return RedirectToAction(nameof(Index));
        }
    }
}
