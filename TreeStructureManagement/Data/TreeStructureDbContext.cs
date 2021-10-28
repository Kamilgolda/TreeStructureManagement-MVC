using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TreeStructureManagement.Models;

namespace TreeStructureManagement.Data
{
    public class TreeStructureDbContext : IdentityDbContext
    {
        public TreeStructureDbContext(DbContextOptions<TreeStructureDbContext> options)
            : base(options)
        {
        }

        public DbSet<Node> Nodes { get; set; }
    }
}
