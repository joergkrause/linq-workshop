using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication1
{

    # region Customer

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }

        public Customer()
        {
        }
        
        public Customer(int id, string name, string city)
        {
            Id = id;
            Name = name;
            City = city;
        }
    }

    # endregion

    # region CustomerCollection

    public class CustomerCollection : IEnumerable<Customer>
    {
        IEnumerable<Customer> _customers;
        
        public CustomerCollection(IEnumerable<Customer> customers)
        {
            _customers = customers;
        }

        public Customer GetCustomer(Predicate<Customer> isMatch)
        {
            foreach (Customer customer in _customers)
                if (isMatch(customer))
                    return customer;
            return null;
        }

        public IEnumerator<Customer> GetEnumerator()
        {
            foreach (Customer customer in _customers)
                yield return customer;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _customers.GetEnumerator();
        }
    }



    # endregion
}
