using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Takt.Services.Infrastructure.Identity;

namespace Takt.Services.Infrastructure.Database;

public class TaktDbContext : IdentityUserContext<TaktIdentityUser, long>
{
	public TaktDbContext()
	{
		
	}
}