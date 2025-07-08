using TimeForge.Infrastructure.Repositories.Common;

namespace TimeForge.Infrastructure.Repositories;

public class TimeForgeRepository(TimeForgeDbContext context) : BaseRepository<TimeForgeDbContext>(context)
{
    //TODO special case operations go here 
}