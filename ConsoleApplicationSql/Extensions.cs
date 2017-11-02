using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ConsoleApplicationSql {
  public static class Extensions {

    #region SQL

    public static IQueryable<T> Page<T>(this IQueryable<T> query, int page, int pagesize) {
      int skip = Math.Max(pagesize * (page - 1), 0);
      return query.Skip(skip).Take(pagesize);
    }

    public static IEnumerable<IQueryable<T>> Batch<T>(this IQueryable<T> source, int batchSize) {
      for (IQueryable<T> s = source; s.Any(); s = s.Skip(batchSize)) {
        yield return s.Take(batchSize);
      }
    }

    public static void ConsoleWriter<T>(this IQueryable<T> resultset, Action<T> action) {
      resultset.ToList<T>().ForEach(action);
    }

    #endregion
  }
}
