using TimeForge.Infrastructure.Repositories.Common;
using TimeForge.Infrastructure.Repositories.Interfaces;

namespace TimeForge.Infrastructure.Repositories;

public class TimeForgeRepository(TimeForgeDbContext context) :  BaseRepository<TimeForgeDbContext>(context), ITimeForgeRepository
{
    //TODO special case operations go here 
}