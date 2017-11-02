using System;
using System.Xml.XPath;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Xml;
using System.Globalization;
using ConsoleLibrary;

namespace ConsoleApplicationXml {
  class Program {
    static void Main(string[] args) {
      Console.ForegroundColor = ConsoleColor.White;
      Console.WindowWidth = 120;
      Console.WindowHeight = 55;
      ConsoleHelper.SetConsoleFont(8);
      Console.WriteLine("Beispiele 1...8");
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
  }

  public static class Examples {

    #region 1 XLM on-the-fly erstellen

    public static void Example1() {
      var hosts = from ip in File.ReadAllLines("IP.csv")
                  let segments = ip.Split('#')
                  let hostaddress = segments[0]
                  let hostName = segments[1]
                  select new XElement("address",  // <address><ip>...</ip><name>...</name></address>
                          new XElement("ip", hostaddress),
                          new XElement("name", hostName)
                      );
      XDocument xDoc = new XDocument(new XComment("Host Table"), new XElement("addresses", hosts));
      Console.WriteLine(xDoc.ToString());
    }

    #endregion

    #region 2 XML lesen

    public static void Example2() {
      string xml = @"<fragment>
                              <sessions conference=""BASTA"">
                                  <session sequence=""1"">
                                     <title>SharePoint als Entwicklerplattform</title>
                                     <name>Joerg Krause</name>
                                  </session>
                                  <session sequence=""2"">
                                     <title>Advanced LINQ</title>
                                     <name>Joerg Krause</name>
                                  </session>
                              </sessions>
                           </fragment>";
      XElement sessions = XElement.Parse(xml);
      var result = from s in sessions.DescendantsAndSelf()
                   let a = s.Attribute("sequence")
                   where a != null && a.Value.Equals("2")
                   select s;
      Console.WriteLine(result.FirstOrDefault().Element("title").Value);

      var anc = sessions
        .Element("sessions")
        .Element("session")
        .Element("title")
        .Value;

      Console.WriteLine(anc);
    }

    #endregion

    #region 3 XPath benutzen

    // System.Xml.Linq erforderlich

    public static void Example3() {
      string xml = @"<?xml version=""1.0""  ?> 
                           <fragment>
                              <sessions conference=""BASTA"">
                                  <session sequence=""1"">
                                     <title>SharePoint als Entwicklerplattform</title>
                                     <name>Joerg Krause</name>
                                  </session>
                                  <session sequence=""2"">
                                     <title>Advanced LINQ</title>
                                     <name>Joerg Krause</name>
                                  </session>
                              </sessions>
                           </fragment>";
      XElement sessions = XElement.Parse(xml, LoadOptions.None);
      XDocument xDoc = new XDocument(sessions);

      IEnumerable<XElement> result = xDoc.Root.XPathSelectElements("//*[@sequence='2']/*");

      Console.WriteLine(result.First().Value);
    }

    #endregion

    #region 4 XML nach Object

    public static void Example4() {
      XDocument xDoc = XDocument.Load("PurchaseOrders.xml");
      var result = from x in xDoc.Root.DescendantsAndSelf()
                   let po = x.Element("PurchaseOrder")
                   where po.Attribute("PurchaseOrderNumber").Value.Equals("99503")
                   select new {
                     Name = po.Element("Address").Element("Name").Value,
                     City = po.Element("Address").Element("City").Value
                   };
      Console.WriteLine("{0} lives in {1}", result.First().Name, result.First().City);
    }

    #endregion

    #region 5 Komplexe Abfrage

    public static void Example5() {
      XDocument xDoc = XDocument.Load("PurchaseOrders.xml", LoadOptions.None);
      var ordered = from x in xDoc.Root.Elements("PurchaseOrder")
                    let ad = x.Element("Address")
                    let od = DateTime.Parse(x.Attribute("OrderDate").Value, new CultureInfo(1033))
                    orderby od descending
                    select new {
                      Name = ad.Element("Name").Value,
                      City = ad.Element("City").Value
                    };
      foreach (var element in ordered) {
        Console.WriteLine("{0} lives in {1}", element.Name, element.City);
      }
    }

    #endregion

    #region 6 Namespaces

    public static void Example6() {
      XDocument doc = XDocument.Parse(@"
                    <?xml version=""1.0"" encoding=""utf-8"" standalone=""yes""?>
                    <?target data?>
                    <Root AttName=""An Attribute"" xmlns:jk=""http://www.joergkrause.de"">
                      <!--This is a comment-->
                      <Child>Text</Child>
                      <Child>Other Text</Child>
                      <ChildWithMixedContent>text<b>BoldText</b>otherText</ChildWithMixedContent>
                      <jk:ElementInNamespace>
                        <jk:ChildInNamespace />
                      </jk:ElementInNamespace>
                    </Root>
                    ".TrimStart());

      Action<string> write = (s => Console.WriteLine(s));

      foreach (XObject obj in doc.DescendantNodes()) {
        write(obj.GetXPath());
        if (obj is XElement)
          foreach (XAttribute at in ((XElement)obj).Attributes())
            write(at.GetXPath());
      }
    }

    #endregion

    #region 7 Traditionelle Methode

    public static void Example7() {
      XElement fileSystemTree = CreateTree(@"c:\Apps");
      Console.WriteLine(fileSystemTree);
      Console.WriteLine("------");
      long totalFileSize =
          (from f in fileSystemTree.Descendants("File")
           select (long)f.Element("Length")).Sum();
      Console.WriteLine("Total File Size:{0}", totalFileSize);
    }

    private static XElement CreateTree(string source) {
      DirectoryInfo di = new DirectoryInfo(source);
      return new XElement("Dir",
          new XAttribute("Name", di.Name),
            from d in Directory.GetDirectories(source)
            select CreateTree(d),
            from fi in di.GetFiles()
            select new XElement("File",
                new XElement("Name", fi.Name),
                new XElement("Length", fi.Length)
            ));
    }

    #endregion

    #region 8 Extension Method

    public static void Example8() {
      //Func<string, string[]> gd = (p) => Directory.GetDirectories(p);
      //var path = @"C:\StudioApps\Konferenzen\LINQ";
      //Func<string, string[]> subdir = (s) => gd(s);
      //var recurse = gd(path)[0].Recurse(subdir);
      //recurse.ToList().ForEach((d) => Console.WriteLine(d));

      foreach (var item in Coll()) {
        Console.WriteLine(item);
      }
    }

    #endregion

    public static IEnumerable<string> Coll() {
      yield return "Eins";
      yield return "Zwei";
      yield return "Drei";
      yield return "Vier";
    }

  }
}

public static class MyExtensions {

  #region Recursion

  public static IEnumerable<T> Recurse<T>(this T root, Func<T, IEnumerable<T>> findChildren) where T : class {
    yield return root;

    foreach (var child in findChildren(root)
      .SelectMany(node => Recurse(node, findChildren))
      .TakeWhile(child => child != null)) {
      yield return child;
    }
  }

  #endregion

  #region XPath

  private static string GetQName(XElement xe) {
    string prefix = xe.GetPrefixOfNamespace(xe.Name.Namespace);
    if (xe.Name.Namespace == XNamespace.None || prefix == null)
      return xe.Name.LocalName.ToString();
    else
      return prefix + ":" + xe.Name.LocalName.ToString();
  }

  private static string GetQName(XAttribute xa) {
    string prefix =
        xa.Parent.GetPrefixOfNamespace(xa.Name.Namespace);
    if (xa.Name.Namespace == XNamespace.None || prefix == null)
      return xa.Name.ToString();
    else
      return prefix + ":" + xa.Name.LocalName;
  }

  private static string NameWithPredicate(XElement el) {
    if (el.Parent != null && el.Parent.Elements(el.Name).Count() != 1)
      return GetQName(el) + "[" +
          (el.ElementsBeforeSelf(el.Name).Count() + 1) + "]";
    else
      return GetQName(el);
  }

  public static string StrCat<T>(this IEnumerable<T> source,
      string separator) {
    return source.Aggregate(new StringBuilder(),
               (sb, i) => sb
                   .Append(i.ToString())
                   .Append(separator),
               s => s.ToString());
  }

  public static string GetXPath(this XObject xobj) {
    if (xobj.Parent == null) {
      XDocument doc = xobj as XDocument;
      if (doc != null)
        return ".";
      XElement el = xobj as XElement;
      if (el != null)
        return "/" + NameWithPredicate(el);
      // the XPath data model does not include white space text nodes
      // that are children of a document, so this method returns null.
      XText xt = xobj as XText;
      if (xt != null)
        return null;
      XComment com = xobj as XComment;
      if (com != null)
        return
            "/" +
            (
                com.Document.Nodes().OfType<XComment>().Count() != 1
                ?
                "comment()[" + (com.NodesBeforeSelf().OfType<XComment>().Count() + 1) + "]"
                :
                "comment()"
            );
      XProcessingInstruction pi = xobj as XProcessingInstruction;
      if (pi != null)
        return
            "/" +
            (
                pi.Document.Nodes().OfType<XProcessingInstruction>().Count() != 1
                ?
                "processing-instruction()[" + (pi.NodesBeforeSelf().OfType<XProcessingInstruction>().Count() + 1) + "]"
                :
                "processing-instruction()"
            );
      return null;
    } else {
      XElement el = xobj as XElement;
      if (el != null) {
        return "/" + el.Ancestors().InDocumentOrder().Select(e => NameWithPredicate(e)).StrCat("/") + NameWithPredicate(el);
      }
      XAttribute at = xobj as XAttribute;
      if (at != null)
        return
            "/" + at.Parent.AncestorsAndSelf().InDocumentOrder().Select(e => NameWithPredicate(e)).StrCat("/") + "@" + GetQName(at);
      XComment com = xobj as XComment;
      if (com != null)
        return
            "/" +
            com.Parent.AncestorsAndSelf().InDocumentOrder().Select(e => NameWithPredicate(e)).StrCat("/") +
            (
                com.Parent.Nodes().OfType<XComment>().Count() != 1
                ?
                "comment()[" + (com.NodesBeforeSelf().OfType<XComment>().Count() + 1) + "]"
                :
                "comment()"
            );
      XCData cd = xobj as XCData;
      if (cd != null)
        return
            "/" +
            cd.Parent.AncestorsAndSelf().InDocumentOrder().Select(e => NameWithPredicate(e)).StrCat("/") +
            (
                cd.Parent.Nodes().OfType<XText>().Count() != 1
                ?
                "text()[" + (cd.NodesBeforeSelf().OfType<XText>().Count() + 1) + "]"
                :
                "text()"
            );
      XText tx = xobj as XText;
      if (tx != null)
        return
            "/" +
            tx.Parent.AncestorsAndSelf().InDocumentOrder().Select(e => NameWithPredicate(e)).StrCat("/") +
            (
                tx.Parent.Nodes().OfType<XText>().Count() != 1
                ?
                "text()[" + (tx.NodesBeforeSelf().OfType<XText>().Count() + 1) + "]"
                :
                "text()"
            );
      XProcessingInstruction pi = xobj as XProcessingInstruction;
      if (pi != null)
        return
            "/" +
            pi.Parent.AncestorsAndSelf().InDocumentOrder().Select(e => NameWithPredicate(e)).StrCat("/") +
            (
                pi.Parent.Nodes().OfType<XProcessingInstruction>().Count() != 1 ?
                "processing-instruction()[" +
(pi.NodesBeforeSelf().OfType<XProcessingInstruction>().Count() + 1) + "]" :
                "processing-instruction()"
            );
      return null;
    }

    #endregion
  }
}
