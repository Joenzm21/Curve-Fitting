using LumenWorks.Framework.IO.Csv;
using MathNet.Numerics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media;

namespace Curve_Fitting
{
    public partial class Main : Form
    {
        private static readonly string superscripts = @"⁰¹²³⁴⁵⁶⁷⁸⁹";
        private MDomain domain;
        private double[] formula;
        private double dpi;

        public Main()
        {
            using (Graphics g = CreateGraphics())
                this.dpi = Math.Sqrt(g.DpiX * g.DpiX + g.DpiY * g.DpiY);
            InitializeComponent();
            string[] interval = new string[] { "", "" };
            domain = new MDomain(interval);
            valuelist.FullRowSelect = true;
            valuelist.Dock = DockStyle.Fill;
            valuelist.ListViewItemSorter = new ListViewComparer(0, SortOrder.Ascending);
        }

        private void Add()
        {
            char[] separator = new char[] { ',' };
            string[] spt = xyin.Text.Replace(" ", "").Split(separator);
            xyin.ClearUndo();
            if (spt.Length != 2)
            {
                MessageBox.Show("Input is invaild", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else if (!double.TryParse(spt[0], out double num) || !double.TryParse(spt[1], out _))
            {
                MessageBox.Show("Input is invaild", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else if (!domain.Check(num))
            {
                MessageBox.Show("Out of range", "Domain", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else if (Enumerable.Any<ListViewItem>(Enumerable.Cast<ListViewItem>((IEnumerable)valuelist.Items), c => c.SubItems[2].Text == spt[0]))
            {
                MessageBox.Show("This X value is already exist", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else
            {
                string[] items = new string[] { "", valuelist.Items.Count.ToString(), spt[0], spt[1] };
                valuelist.Items.Add(new ListViewItem(items));
                Apply();
                xyin.Focus();
            }
        }

        private void Apply()
        {
            int width = (int)((graph.Size.Width * 0.8) - 260.0);
            graph.plt.Clear();
            int count = valuelist.Items.Count;
            if ((ordernum.Value < count) && (count != 0))
            {
                double[] xarr = new double[count];
                double[] yarr = new double[count];
                for (int i = 0; i < count; i++)
                {
                    double.TryParse(valuelist.Items[i].SubItems[2].Text, out double x);
                    double.TryParse(valuelist.Items[i].SubItems[3].Text, out double y);
                    graph.plt.PlotPoint(x, y);
                    xarr[i] = x;
                    yarr[i] = y;
                }
                if (ordernum.Value > 1)
                    formula = Fit.Polynomial(xarr, yarr, (int)ordernum.Value);
                else if (ordernum.Value == 1)
                {
                    Tuple<double, double> tuple = Fit.Line(xarr, yarr);
                    formula = new double[] { tuple.Item1, tuple.Item2 };
                }
                else
                    formula = new double[] { Enumerable.Average(yarr) };
                List<string> formularstrings = new List<string> { "f(x) = " };
                for (int j = formula.Length - 1; j >= 0; j--)
                {
                    string str;
                    switch (j)
                    {
                        case 0:
                            str = Math.Round(formula[j], (int)dplacesin.Value).ToString();
                            if (GetWidthSize(formularstrings[formularstrings.Count - 1] + str) > width)
                                formularstrings.Add(str);
                            else formularstrings[formularstrings.Count - 1] += str;
                            break;

                        case 1:
                            str = Math.Round(formula[j], (int)dplacesin.Value).ToString() + "x + ";
                            if (GetWidthSize(formularstrings[formularstrings.Count - 1] + str) > width)
                                formularstrings.Add(str);
                            else formularstrings[formularstrings.Count - 1] += str;
                            break;

                        default:
                            int power = j;
                            string powerstring = "";
                            while (power > 0)
                            {
                                powerstring += superscripts[power % 10].ToString();
                                power /= 10;
                            }
                            str = Math.Round(formula[j], (int)dplacesin.Value).ToString() + "x" + string.Join("", Enumerable.Reverse(powerstring)) + " + ";
                            if (GetWidthSize(formularstrings[formularstrings.Count - 1] + str) > width)
                                formularstrings.Add(str);
                            else formularstrings[formularstrings.Count - 1] += str;
                            break;

                    }
                }
                if (!xarr.All(c => domain.Check(c)))
                {
                    MessageBox.Show("Out of range", "Domain", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    graph.plt.Title("");
                    graph.Render();
                    return;
                }
                graph.plt.Title(string.Join(Environment.NewLine, formularstrings));
                graph.plt.PlotFunction(FuncCalc);
            }
            graph.Render();
        }

        private void domainin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                char[] separator = new char[] { ',' };
                string[] interval = domainin.Text.Replace(" ", "").Split(separator);
                if (interval.Length == 0)
                {
                    string[] textArray1 = new string[] { "", "" };
                    domain = new MDomain(textArray1);
                }
                if ((((interval.Length == 2) && (interval[0].Length != 1)) && (interval[1].Length != 1)) && ((string.IsNullOrEmpty(interval[0]) || ((interval[0][0] == '(') || (interval[0][0] == '['))) && (string.IsNullOrEmpty(interval[1]) || ((interval[1][interval[1].Length - 1] == ')') || (interval[1][interval[1].Length - 1] == ']')))))
                {
                    domain = new MDomain(interval);
                    Apply();
                }
            }
        }

        private double? FuncCalc(double x)
        {
            if (!domain.Check(x))
                return null;
            double res = 0;
            for (int i = formula.Length - 1; i >= 0; i--)
                res += formula[i] * Math.Pow(x, i);
            return res;
        }

        private double GetWidthSize(string str) =>
            new FormattedText(str, CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight, new Typeface("#Arial"), 20, System.Windows.Media.Brushes.Black, dpi).Width;

        private void ordernum_ValueChanged(object sender, EventArgs e)
        {
            Apply();
        }

        private void Remove()
        {
            foreach (ListViewItem item in valuelist.SelectedItems)
            {
                for (int i = item.Index + 1; i < valuelist.Items.Count; i++)
                    valuelist.Items[i].SubItems[1] = new ListViewItem.ListViewSubItem(item, Convert.ToInt32(valuelist.Items[i].SubItems[1].Text).ToString());
                valuelist.Items.Remove(item);
            }
            Apply();
            graph.Refresh();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string path = "";
            if (openCSVDialog.ShowDialog() == DialogResult.OK)
            {
                path = openCSVDialog.FileName;
                DataTable csvTable = new DataTable();
                using (CsvReader reader = new CsvReader(new StreamReader(File.OpenRead(path)), true))
                {
                    csvTable.Load(reader);
                    for (int i = 0; i < csvTable.Rows.Count; i++)
                    {
                        if (double.TryParse(csvTable.Rows[i][0].ToString(), out double x) && double.TryParse(csvTable.Rows[i][1].ToString(), out _))
                        {
                            if (!domain.Check(x))
                            {
                                MessageBox.Show("Out of range", "Domain", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                                return;
                            }
                            else if (valuelist.Items.Count == 0 || valuelist.Items.Cast<ListViewItem>().All(c => c.SubItems[2].Text != csvTable.Rows[i][0].ToString()))
                                valuelist.Items.Add(new ListViewItem(new string[] { "",
                                    valuelist.Items.Count.ToString(),
                                    csvTable.Rows[i][0].ToString(),
                                    csvTable.Rows[i][1].ToString()}));
                        }
                    }
                }
                Apply();
                xyin.Focus();
            }
        }

        private void valuelist_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListViewComparer comparer = (ListViewComparer)valuelist.ListViewItemSorter;
            valuelist.ListViewItemSorter = new ListViewComparer(e.Column, (comparer.ColumnNumber == e.Column) ? (comparer.SortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending) : SortOrder.Ascending);
        }

        private void vl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                Remove();
            }
        }

        private void xyin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Add();
                xyin.Clear();
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
    }
}
public class MDomain
{
    internal double? a = null;
    internal double? b = null;
    private bool closea = false;
    private bool closeb = false;

    public MDomain(string[] interval)
    {
        try
        {
            if (!string.IsNullOrEmpty(interval[0]))
            {
                closea = interval[0][0] == '[';
                double.TryParse(interval[0].Substring(1, interval[0].Length - 1), out double num);
                a = new double?(num);
            }
            if (!string.IsNullOrEmpty(interval[1]))
            {
                closeb = interval[1][interval[1].Length - 1] == ']';
                double.TryParse(interval[1].Substring(0, interval[1].Length - 1), out double num2);
                b = new double?(num2);
            }
        }
        catch
        {
            a = null;
            b = null;
            closea = false;
            closeb = false;
        }
    }

    public bool Check(double x)
    {
        if (a == null && b == null) return true;
        if (a == null && x < b) return true;
        if (b == null && x > a) return true;
        if ((x == b && closeb) || (x == a) && closea) return true;
        return (x < b) && (x > a);
    }
}
public class ListViewComparer : IComparer
{
    internal int ColumnNumber;
    internal System.Windows.Forms.SortOrder SortOrder;

    public ListViewComparer(int column_number, System.Windows.Forms.SortOrder sort_order)
    {
        ColumnNumber = column_number;
        SortOrder = sort_order;
    }

    public int Compare(object object_x, object object_y)
    {
        ListViewItem item_x = object_x as ListViewItem;
        ListViewItem item_y = object_y as ListViewItem;
        string string_x;
        if (item_x.SubItems.Count <= ColumnNumber)
            string_x = "";
        else
            string_x = item_x.SubItems[ColumnNumber].Text;
        string string_y;
        if (item_y.SubItems.Count <= ColumnNumber)
            string_y = "";
        else
            string_y = item_y.SubItems[ColumnNumber].Text;
        int result;
        double double_x, double_y;
        double.TryParse(string_x, out double_x);
        double.TryParse(string_y, out double_y);
        result = double_x.CompareTo(double_y);
        if (SortOrder == SortOrder.Ascending)
        {
            return result;
        }
        else
        {
            return -result;
        }
    }
}


