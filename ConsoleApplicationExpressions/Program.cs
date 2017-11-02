using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Data.SqlClient;
using ConsoleApplicationExpressions.Properties;
using System.Data;
using System.Reflection;
using ConsoleLibrary;

namespace ConsoleApplicationExpressions {
  class Program {
    static void Main(string[] args) {
      Console.ForegroundColor = ConsoleColor.White;
      Console.WindowWidth = 120;
      Console.WindowHeight = 55;
      ConsoleHelper.SetConsoleFont(8);
      Console.WriteLine("Beispiele 1...2");
      ConsoleKeyInfo example = Console.ReadKey();
      do {
        Console.Clear();
        Console.WriteLine("Beispiel {0}", example.KeyChar);
        Console.WriteLine();
        if (example.Key != ConsoleKey.End && example.Key != ConsoleKey.Enter) {
          try { typeof(Examples).GetMethod("Example" + example.KeyChar).Invoke(null, null); } catch (Exception ex) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.Message);
            Console.ForegroundColor = ConsoleColor.White;
          }
        }
        example = Console.ReadKey();
      } while (example.Key != ConsoleKey.End);
    }

    public static class Examples {
      #region 1 (Compiled Dynamic Expressions)

      public static void Example1() {
        var strings = new string[] { "Anton", "Berta", "Cäsar", "Delta", "Epsilon" };
        Expression<Func<String, String, bool>> Like_Lambda = (item, search) => item.ToLower().Contains(search.ToLower());
        Func<String, String, bool> Like = Like_Lambda.Compile();
        // Compiliert, Dynamisch
        var res = from r in strings
                  where Like(r, "Delta")
                  select r;
        System.Diagnostics.Stopwatch.StartNew();
        long start = System.Diagnostics.Stopwatch.GetTimestamp();
        for (int i = 0; i < 100000; i++) {
          var x = res.ToList();
        }
        long stop = System.Diagnostics.Stopwatch.GetTimestamp();
        Console.WriteLine(stop - start);
        // Nicht compiliert, statisch
        var res1 = from r in strings
                   where r.ToLower().Contains("Delta".ToLower())
                   select r;
        System.Diagnostics.Stopwatch.StartNew();
        start = System.Diagnostics.Stopwatch.GetTimestamp();
        for (int i = 0; i < 100000; i++) {
          var x = res1.ToList();
        }
        stop = System.Diagnostics.Stopwatch.GetTimestamp();
        Console.WriteLine(stop - start);
      }

      #endregion

      #region 2 (Make untyped sources dynamically typed)

      public static void Example2() {
        // untypisiert
        using (SqlConnection conn = new SqlConnection(Settings.Default.NORTHWNDConnectionString)) {
          DataTable dt = new DataTable();
          conn.Open();
          try {
            SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Customers", conn);
            da.Fill(dt);
          }
          finally {
            conn.Close();
          }
          var expression = MakeTypedRow<Customer>();

          var customers = from DataRow row in dt.Rows
                          select expression(row);
          customers.ToList<Customer>().ForEach(c => Console.WriteLine("{0} == {1}", c.CustomerID, c.ContactName));
        }

      }

      public class Customer {
        public string CustomerID { get; set; }
        public string ContactName { get; set; }
      }

      private static Func<DataRow, T> MakeTypedRow<T>() {
        ParameterExpression r = Expression.Parameter(typeof(DataRow), "r");
        return
           // row => row.Field(Name)
           Expression.Lambda<Func<DataRow, T>>(
              Expression.MemberInit(
                Expression.New(typeof(T)), // new Customer
                        from property in typeof(T).GetProperties() // typeof(Customer).GetProperties()
                                                                   // row[FeldName]
                                let fieldMethod = typeof(DataRowExtensions).GetMethod("Field",
                                            BindingFlags.Static | BindingFlags.Public,
                                            null,
                                            new Type[] { typeof(DataRow), typeof(string) }, null)
                                            .MakeGenericMethod(property.PropertyType)
                                // row.Field("Name")
                                let propertyValue = Expression.Call(fieldMethod, r, Expression.Constant(property.Name))
                        select (MemberBinding)Expression.Bind(property, propertyValue)),
                    r).Compile();
      }


      #endregion
    }
  }
}
