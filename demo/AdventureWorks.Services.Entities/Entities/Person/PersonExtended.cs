
namespace AdventureWorks.Services.Entities
{
    public partial class Person
    {
        public string FullName => $"{LastName}, {FirstName}";
    }
}