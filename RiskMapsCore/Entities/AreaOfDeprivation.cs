using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace RiskTracker.Entities {
  public class AreaOfDeprivation {
    private static List<string> postCodes_;

    public static bool InAOD(string postCode) {
      if (postCode == null)
        return false;

      postCode = postCode.ToLower().Replace(" ", "");
      return postCodes().Where(p => p == postCode).Count() != 0;
    } // InAOD

    private static List<string> postCodes() {
      if (postCodes_ != null)
        return postCodes_;

      postCodes_ = new List<string>();

      loadFromResource("PostCode_LOSA_Birmingham.csv", postCodes_);
      loadFromResource("PostCode_LOSA_Greater_Manchester.csv", postCodes_);
      
      return postCodes_;
    } // postCodes

    private static void loadFromResource(string resourceName, List<string> postCode) {
      var assembly = Assembly.GetExecutingAssembly();
      var textReader = new StreamReader(assembly.GetManifestResourceStream("RiskMapsCore.Resources." + resourceName));
      while (textReader.Peek() != -1) {
        string line = textReader.ReadLine();
        line = line.Substring(0, line.IndexOf(','));
        line = line.ToLower().Replace(" ", "");
        postCodes_.Add(line);
      } // while
    } // loadFromFile
  } // AreaOfDeprivation
} // RiskTracker.Entities