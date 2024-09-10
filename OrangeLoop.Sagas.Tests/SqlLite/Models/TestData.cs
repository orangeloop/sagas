using System.Collections.Generic;

namespace OrangeLoop.Sagas.Tests.SqlLite.Models
{
    public static class TestData
    {
        public static IList<Customer> Customers
            =>
            [
                new Customer(1, "Albert", "Einstein"),
                new Customer(2, "Nikola", "Tesla")
            ];
    }
}
