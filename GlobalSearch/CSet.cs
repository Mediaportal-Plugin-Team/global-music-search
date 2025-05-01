using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.Search
{
  public class CSet : object
  {
    /// <summary> 
    /// Union
    /// </summary>
    public static object Or(object A, object B)
    {
      if ((A == null) || (B == null)) throw (new ArgumentNullException());
      if (A.GetType() != B.GetType()) throw (new Exception("Different set types"));
      return (int)A | (int)B;
    }
    /// <summary> 
    /// Union
    /// </summary>
    public static object Union(object A, object B)
    {
      if ((A == null) || (B == null)) throw (new ArgumentNullException());
      if (A.GetType() != B.GetType()) throw (new Exception("Different set types"));
      return (int)A | (int)B;
    }

    /// <summary> 
    /// Intersection
    /// </summary>
    public static object And(object A, object B)
    {
      if ((A == null) || (B == null)) throw (new ArgumentNullException());
      if (A.GetType() != B.GetType()) throw (new Exception("Different set types"));
      return (int)A & (int)B;
    }
    /// <summary> 
    /// Union
    /// </summary>
    public static object Intersection(object A, object B)
    {
      if ((A == null) || (B == null)) throw (new ArgumentNullException());
      if (A.GetType() != B.GetType()) throw (new Exception("Different set types"));
      return (int)A & (int)B;
    }

    /// <summary> 
    /// Difference
    /// </summary>
    public static object Xor(object A, object B)
    {
      if ((A == null) || (B == null)) throw (new ArgumentNullException());
      if (A.GetType() != B.GetType()) throw (new Exception("Different set types"));
      return (int)((int)A | (int)B) ^ (int)B;
    }
    /// <summary> 
    /// Difference
    /// </summary>
    public static object Diff(object A, object B)
    {
      if ((A == null) || (B == null)) throw (new ArgumentNullException());
      if (A.GetType() != B.GetType()) throw (new Exception("Different set types"));
      return (int)((int)A | (int)B) ^ (int)B;
    }

    /// <summary> 
    /// Membership - A in B, A can be a single value or a set. 
    /// </summary>
    /// 
    public static bool In(object A, object B)
    {
      if ((A == null) || (B == null)) throw (new ArgumentNullException());
      if (A.GetType() != B.GetType()) throw (new Exception("Different set types"));
      return (((int)A & (int)B) == (int)A);
    }
  }
}
