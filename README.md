# Curve Fitting
An Application to fitting curve, based on input data. It has some key points:
- Support .CSV File.
- Support Domain.
- Can custom choose the order of the polynomial equation.
- Easy to sort the point list.
- Also support round the coefficient of the equation.
## How To Use
- Add point into Textbox with format "x, y"   Press Enter or right-click on the Point List View -> Add .CSV File
- (Optional) Add domain with format "[a, b]" / "(a, b)" / "[a, b)" / "(a, b]". If you just a half of domain like ",b)" / ",b]" / "(a," / "[a,", the missing part means from / to infinity. 
- Increase / Decrease Order of formula for best fitting and prevent overfitting (5 order is recommended).
- (Optional) Click on a column to ascending sort. Click again to change to descending sort.
- Select rows and press Delete to remove them.
## Library
- MathNet.Numberics
- Scottplot.Winforms
- LumenWorks.Framework.IO
## Status: Workable
