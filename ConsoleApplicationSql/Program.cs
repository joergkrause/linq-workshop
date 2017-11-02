using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ConsoleLibrary;

namespace ConsoleApplicationSql
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WindowWidth = 120;
            Console.WindowHeight = 55;
            ConsoleHelper.SetConsoleFont(8);
            Console.WriteLine("Beispiele 1...6");
            ConsoleKeyInfo example = Console.ReadKey();
            do
            {
                Console.Clear();
                Console.WriteLine("Beispiel {0}", example.KeyChar);
                Console.WriteLine();
                if (example.Key != ConsoleKey.End && example.Key != ConsoleKey.Enter)
                {
                    try { typeof(Examples).GetMethod("Example" + example.KeyChar).Invoke(null, null); }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(ex.Message);
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
                example = Console.ReadKey();
            } while (example.Key != ConsoleKey.End);
        }
    }

    public static class Examples
    {

        # region 1 (Test DB)

        public static void Example1()
        {
            using (NorthwindDataContext dx = new NorthwindDataContext())
            {
                Console.WriteLine(dx.Customers.Count());
            }
        }
                
        # endregion

        # region 2 (Stored proc)

        public static void Example2()
        {
            using (NorthwindDataContext dx = new NorthwindDataContext())
            {
                var result = dx.Ten_Most_Expensive_Products();
                foreach (var r in result)
                {
                    Console.WriteLine("{0} at {1:C}", r.TenMostExpensiveProducts, r.UnitPrice);
                }
            }
        }
                
        # endregion

        # region 3 (Query)

        public static void Example3()
        {
            /*
             * SELECT     City, COUNT(EmployeeID) AS NumEmployees
                FROM         Employees
                WHERE     (Title = 'Sales Representative')
                GROUP BY City
                HAVING      (COUNT(EmployeeID) > 1)
                ORDER BY NumEmployees
             * */
            using (NorthwindDataContext dx = new NorthwindDataContext())
            {
                var result = from e in dx.Employees
                             where e.Title == "Sales Representative"        // WHERE
                             group e by e.City into EmployeeGroup           // GROUP BY
                             where EmployeeGroup.Count() > 1                // HAVING
                             orderby EmployeeGroup.Count()                  // ORDER BY
                             select new
                            {
                                 EmployeeGroup = EmployeeGroup,
                                 //City =  EmployeeGroup.First().City,
                                 NumEmployees = EmployeeGroup.Count()
                             };
                var r = result.First();
                Console.WriteLine("{0} employees in {1}", r.NumEmployees, r.EmployeeGroup.First().City);

                var rr = dx.Employees
                  .Where(e => e.PostalCode == "10000")
                  .ToList()
                  .Where(e => e.Title.Contains(""));



                //var result = (from e in dx.Employees
                //             where e.Title == "Sales Representative"        // WHERE
                //             select new 
                //             {
                //                 City = e.City
                //             })
                //             .GroupBy(e => e.City)
                //             .Where(g => g.Count() > 1)
                //             .OrderBy(o => o.Count()
                //             )
                //             .First();

                //Console.WriteLine("{0} employees in {1}", result.Count(), result.First().City);
                
            }
        }

        # endregion

        # region 4 (Paging / Extension Method)

        public static void Example4()
        {
            using (NorthwindDataContext dx = new NorthwindDataContext())
            {
                var orders = from o in dx.Orders select o;
                Action<Order> a = new Action<Order>(o => Console.WriteLine("Order {0} for {1}", o.OrderID, o.CustomerID));
                Console.WriteLine("Page 0:");
                orders.Page(0, 2).ConsoleWriter<Order>(a);
                Console.WriteLine("Page 2:");
                orders.Page(2, 3).ConsoleWriter<Order>(a);
            }
        }

        # endregion

        # region Before 5 ('b') --> Learn yield

        public static void Exampleb()
        {
            var result = GetMessages();
            foreach (var r in result)
                Console.WriteLine(r);
        }

        private static IEnumerable<string> GetMessages()
        {
            Console.WriteLine("Executing code before all yield returns.");
            yield return "1";
            Console.WriteLine("Executing code after yield return 1 and before yield return 2.");
            yield return "2";
            Console.WriteLine("Executing code after yield return 2 and before yield return 3.");
            yield return "3";
            Console.WriteLine("Executing code after all yield returns.");
        }

        # endregion

        # region 5 Batch 

        public static void Example5()
        {
            using (NorthwindDataContext dx = new NorthwindDataContext())
            {
                var products = from p in dx.Products select p;
                var chunks = products.Batch<Product>(5);

                Action<Product> a = new Action<Product>(p => Console.WriteLine("Product {0} with ID {1}", p.ProductName, p.ProductID));
                
                foreach (var chunk in chunks)
                {
                    Console.WriteLine("-- New chunk --");
                    Console.WriteLine("---");
                    chunk.ToList().ForEach(a);
                }

            }
        }

        # endregion

        # region 6 Order/group

        public static void Example6()
        {
            using (NorthwindDataContext dx = new NorthwindDataContext())
            {
                var orders = from o in dx.Orders
                             group o by o.CustomerID into Orders
                             orderby Orders.Key
                             select Orders;
                foreach (var order in orders)
                {
                    Console.WriteLine("{0} has {1} orders", order.First().CustomerID, order.Count());
                }
            }
        }

        # endregion

    }

}
