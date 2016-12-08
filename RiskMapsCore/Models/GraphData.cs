using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RiskTracker.Models {
  public class GraphData {
    private IList<DataSet> dataSets_ = new List<DataSet>();

    public IList<DataSet> DataSets { get { return dataSets_; } }

    public void add(DataSet dataSet) { dataSets_.Add(dataSet);  }
  } // class GraphData

  public class DataSet {
    private string title_;
    private IList<DataRow> rows_ = new List<DataRow>();

    public DataSet(string title) {
      title_ = title;
    } // DataSet

    public string Title { get { return title_; } }
    public IList<DataRow> Rows { get { return rows_; } }

    public void add(DataRow row) { rows_.Add(row); }
  } // class DataSet

  public class DataRow {
    private string label_;
    private IList<string> values_ = new List<string>();

    public DataRow(string label) {
      label_ = label;
    } // DataRow

    public string Label { get { return label_; } }
    public IList<string> Values { get { return values_; } }

    public void add(string value) { values_.Add(value); }
  } // class DataRow
} // namespace ...