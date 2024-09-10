namespace OrangeLoop.Sagas.Tests.SqlLite.Models
{
    public class Customer
    {
        public Customer() { }
        public Customer(int id, string firstName, string lastName)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
        }

        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}
