using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Linq.Expressions;
using ConsoleLibrary;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WindowWidth = 120;
            Console.WindowHeight = 45;
            ConsoleHelper.SetConsoleFont(8);
            Console.WriteLine("Beispiele 1...9");
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
        # region 1 (List, Find)

        public static void Example1()
        {

            List<string> names = new List<string>();
            names.Add("Dave");
            names.Add("John");
            names.Add("Joerg");
            names.Add("Barney");
            names.Add("Chuck");
            string abe = names.Find(n => n == "Joerg");
            Console.WriteLine(abe);
        }

        # endregion

        # region 2 (List, Find ==> typed)

        public static void Example2()
        {
            List<Customer> customers = new List<Customer>();
            customers.Add(new Customer(1, "Dave", "Sarasota"));
            customers.Add(new Customer(2, "John", "Tampa"));
            customers.Add(new Customer
            {
                Id = 3,
                Name = "Joerg",
                City = "Berlin"
            });
            Customer result = customers.Find(c => c.Name.Equals("Joerg"));
            Console.WriteLine("{0} kommt aus {1}", result.Name, result.City);
        }

        # endregion

        # region 3 (List, Typed Collection)

        public static void Example3()
        {
            List<Customer> customers = new List<Customer>();
            customers.Add(new Customer(1, "Dave", "Sarasota"));
            customers.Add(new Customer(2, "John", "Tampa"));
            customers.Add(new Customer(3, "Joerg", "Berlin"));

            CustomerCollection cc = new CustomerCollection(customers);
            Customer result = cc.GetCustomer(c => c.Name.Equals("Joerg"));

            //Customer result = customers.Single(c => c.Name.Equals("Joerg"));

            Console.WriteLine("{0} kommt aus {1}", result.Name, result.City);
        }

        # endregion

        # region 4 (let)

        public static void Example4()
        {
            var addresses = from line in File.ReadAllLines("IP.csv")
                            where !line.StartsWith("!")
                            let segments = line.Split('#')
                            select new
                            {
                                IP = segments[0],
                                Host = segments[1]
                            };
            foreach (var address in addresses)
            {
                Console.WriteLine("{0} hat IP {1}", address.Host, address.IP);
            }
        }

        # endregion

        # region 5 (let)

        public static void Example5()
        {
            using (StreamReader sr = new StreamReader("IP.csv"))
            {
                var addresses = from line in sr.GetChunk()
                                where !line.StartsWith("!")
                                let segments = line.Split('#')
                                select new
                                {
                                    IP = segments[0],
                                    Host = segments[1]
                                };
                //var doof = addresses.Reverse(); // Liest alle Werte statt streamen
                foreach (var address in addresses)
                {
                    Console.WriteLine("{0} hat IP {1}", address.Host, address.IP);
                }
            }
        }

        # endregion

        # region 6 (Action)

        public static void Example6()
        {
            int mb = 1048576;
            var proc = from p in Process.GetProcesses()
                       where p.PrivateMemorySize64 > mb 
                       && !String.IsNullOrEmpty(p.MainWindowTitle)
                       select p;
            Action<Process> trace = (p => Trace.WriteLine(String.Format("{0} braucht {1} MB",
                                    p.ProcessName,
                                    p.PagedMemorySize64 / (mb))));
            Action<Process> write = (p => Console.WriteLine("{0} braucht {1} MB", 
                                    p.ProcessName, 
                                    p.PagedMemorySize64 / (mb)));
            var b = true;
            proc.ToList().ForEach(b ? write : trace);
        }

        # endregion

        # region 7 (Join, Group)

        public static void Example7()
        {
            string file = @"C:\Program Files\Reference Assemblies\Microsoft\Framework\v3.5\System.Core.dll";
            // Alle öffentlichen Typen, die Array oder IEnumerable<> zurückgeben
            Assembly assembly = Assembly.LoadFrom(file);
            var pubTypes = (from type in assembly.GetTypes()
                           where type.IsPublic
                           from method in type.GetMethods()
                           where (method.ReturnType.GetInterface(typeof(IEnumerable<>).FullName) != null)
                                  && !method.Name.Equals("ToString")                                        // String implementiert IEnumerable<char> !
                           group method by type).Take(5);                                                   // 5 nur für Übersicht in der Konsole :-)
            // Foreach :-(
            foreach (var groupOfMethods in pubTypes)
            {
                Console.WriteLine("Typ: {0} (hat {1} Methoden:)", groupOfMethods.Key, groupOfMethods.Count());
                foreach (var method in groupOfMethods)
                {
                    Console.WriteLine("      {0}", method.Name);
                }
            }
            // Select Many :-)
            Console.WriteLine(" **** Select Many ****");
            var result = pubTypes.SelectMany(g => g.Select(r => new { Method = r.Name, Type = g.Key })); 
            result.ToList().ForEach((r) => Console.WriteLine("Type {0} hat Methode {1}", r.Type, r.Method)); 
        }

        # endregion

        # region 8 (Predicate)

        public static void Example8()
        {
            var ii = default(int);
            var simple = Enumerable.Range(1, 10);
            Predicate<int> even = (i) => i % 2 == 0;
            Predicate<int> odd = (i) => i % 2 != 0;
            Action<string> write = (s) => Console.WriteLine(s);
            char predicate = Console.ReadKey().KeyChar;
            var result = simple
              .Where(i => predicate.Equals('e') ? even(i) : odd(i))
              .Select(i => i.ToString());
              
              //from i in simple
              //           where (predicate.Equals('e') ? even(i) : odd(i)) && 1==1
              //            select i.ToString();
            result.ToList().ForEach(r => write(r));
        }

        # endregion

        # region 9 

        public static void Example9()
        {
            var simple = new string[] { "hier", "soll", "nichts", "Hier", "doppelt", "SOLL", "vorkommen" };

            Action<string> write = (s) => Console.WriteLine(s);
            var result = from i in simple select i;
            //result.ToList().ForEach(r => write(r));

            //Console.WriteLine("...");
            var dist = result.Distinct(new DistinctStringComparer());
            dist.ToList().ForEach(Console.WriteLine);
          
        }

        public class DistinctStringComparer : IEqualityComparer<string>
        {
            public bool Equals(string x, string y)
            {
              return String.Compare(x, y, true) == 0;
            }

            public int GetHashCode(string obj)
            {
              return 0;
            }
        }

        # endregion

    }

}
