using Microsoft.EntityFrameworkCore;

public class Employee
{
		public int EmployeeID { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
}	

public class AppDbContext: DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
	{
	}
	
	public DbSet<Employee> Employees { get; set; }
}