using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RiskTracker.Models {
  public class ReportData<T> {
    private ICollection<T> data_;
    private IDictionary<string, string> additional_;

    public ReportData(ICollection<T> data) {
      data_ = data;
      additional_ = new Dictionary<string, string>();
    }

    public void Put(string name, string value) { additional_.Add(name, value); }

    public int Count { get { return data_.Count; } }
    public ICollection<T> Data { get { return data_; } }
    public IDictionary<string, string> Additional { get { return additional_; } }
  }
}