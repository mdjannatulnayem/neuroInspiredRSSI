using Microsoft.EntityFrameworkCore;
using RSSI_Nuro.Models;

namespace RSSI_Nuro.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options) { }


    public DbSet<ArchiveDataModel> ArchiveRecords { get; set; }
    public DbSet<ReconDataModel> ReconnectionRecords { get; set; }

}
