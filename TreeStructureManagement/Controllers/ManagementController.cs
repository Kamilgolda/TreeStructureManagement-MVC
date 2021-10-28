using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TreeStructureManagement.Data;
using TreeStructureManagement.Models;

namespace TreeStructureManagement.Controllers
{
    public class ManagementController : Controller
    {
        private readonly TreeStructureDbContext _context;

        public ManagementController(TreeStructureDbContext context)
        {
            _context = context;
        }

        // GET: Management
        public async Task<IActionResult> Index()
        {
            var treeStructureDbContext = _context.Nodes.Include(n => n.Parent);
            return View(await treeStructureDbContext.ToListAsync());
        }

        // GET: Management/Details/5
        public async Task<IActionResult> Details(long? id)
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

        // GET: Management/Create
        public IActionResult Create()
        {
            ViewData["ParentId"] = new SelectList(_context.Nodes, "Id", "Name");
            return View();
        }

        // POST: Management/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ParentId")] Node node)
        {
            if (ModelState.IsValid)
            {
                _context.Add(node);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ParentId"] = new SelectList(_context.Nodes, "Id", "Name", node.ParentId);
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
    }
}
