using System;
using System.Collections.Generic;

/// <summary>
/// Algorithm described here: http://www.roguebasin.com/index.php?title=Precise_Permissive_Field_of_View
/// Pretty much a straight port from this Python code: http://www.roguebasin.com/index.php?title=Permissive_Field_of_View_in_Python
/// </summary>
public static class FieldOfView
{
    public static void Run(
        Vector2Int start,
        int mapWidth,
        int mapHeight,
        int radius,
        Action<Vector2Int> funcVisitTile,
        Func<Vector2Int, bool> funcTileBlocked)
    {
        // Keep track of visited tile so that they are only visited once
        var visited = new HashSet<Vector2Int>();

        // Start coordinate is always visible
        funcVisitTile(start);
        visited.Add(start);

        // Get the dimensions of the actual field of view, making
        // sure not to go off the map or beyond the radius.

        int minExtentX;
        int minExtentY;
        int maxExtentX;
        int maxExtentY;

        if (start.x < radius)
            minExtentX = start.x;
        else
            minExtentX = radius;

        if (mapWidth - start.x - 1 < radius)
            maxExtentX = mapWidth - start.x - 1;
        else
            maxExtentX = radius;

        if (start.y < radius)
            minExtentY = start.y;
        else
            minExtentY = radius;

        if (mapHeight - start.y - 1 < radius)
            maxExtentY = mapHeight - start.y - 1;
        else
            maxExtentY = radius;

        // Northeast quadrant
        CheckQuadrant(visited, start, 1, 1,
            maxExtentX, maxExtentY,
            funcVisitTile, funcTileBlocked);

        // Southeast quadrant
        CheckQuadrant(visited, start, 1, -1,
            maxExtentX, minExtentY,
            funcVisitTile, funcTileBlocked);

        // Southwest quadrant
        CheckQuadrant(visited, start, -1, -1,
            minExtentX, minExtentY,
            funcVisitTile, funcTileBlocked);

        // Northwest quadrant
        CheckQuadrant(visited, start, -1, 1,
            minExtentX, maxExtentY,
            funcVisitTile, funcTileBlocked);
    }

    private static void CheckQuadrant(
        HashSet<Vector2Int> visited,
        Vector2Int start,
        int dx,
        int dy,
        int extentX,
        int extentY,
        Action<Vector2Int> funcVisitTile,
        Func<Vector2Int, bool> funcTileBlocked)
    {
        var activeViews = new List<View>();

        var shallowLine = new Line(0, 1, extentX, 0);
        var steepLine = new Line(1, 0, 0, extentY);

        activeViews.Add(new View(shallowLine, steepLine));
        int viewIndex = 0;

        // Visit the tiles diagonally and going outwards
        //
        // .
        // .
        // .           .
        // 9        .
        // 5  8  .
        // 2  4  7
        // @  1  3  6  .  .  .
        int maxI = extentX + extentY;
        int i = 1;

        while (i != maxI + 1 && activeViews.Count > 0)
        {
            int startJ = 0 > i - extentX ? 0 : i - extentX;

            int maxJ = i < extentY ? i : extentY;

            int j = startJ;
            while (j != maxJ + 1 && viewIndex < activeViews.Count)
            {
                int x = i - j;
                int y = j;
                VisitCoord(visited, start, x, y, dx, dy, viewIndex, activeViews, funcVisitTile, funcTileBlocked);
                j += 1;
            }
            i += 1;
        }
    }

    private static void VisitCoord(HashSet<Vector2Int> visited, Vector2Int start, int x, int y, int dx, int dy,
        int viewIndex, IList<View> activeViews, Action<Vector2Int> funcVisitTile,
        Func<Vector2Int, bool> funcTileBlocked)
    {
        var topLeft = new Vector2Int(x, y + 1);
        var bottomRight = new Vector2Int(x + 1, y);

        while (viewIndex < activeViews.Count &&
               activeViews[viewIndex].SteepLine.BelowOrCollinear(bottomRight))
        {
            // The current coordinate is above the current view and is
            // ignored. The steeper fields may need it though.
            viewIndex++;
        }

        if (viewIndex == activeViews.Count ||
            activeViews[viewIndex].ShallowLine.AboveOrCollinear(topLeft))
        {
            // Either the current coordinate is above all of the fields
            // or it is below all of the fields.
            return;
        }

        // Is is now known that the current coordinate is between the
        // steep and shallow lines of the current view.

        // The real quadrant coordinates
        var real = new Vector2Int(x * dx, y * dy);

        if (!visited.Contains(start + real))
        {
            visited.Add(start + real);
            funcVisitTile(start + real);
        }

        bool isBlocked = funcTileBlocked(start + real);

        if (!isBlocked)
        {
            // The current coordinate does not block sight and
            // therefore has no effect on the view.
            return;
        }

        if (activeViews[viewIndex].ShallowLine.Above(bottomRight) &&
            activeViews[viewIndex].SteepLine.Below(topLeft))
        {
            // The current coordinate is intersected by both lines in
            // the current view. The view is completely blocked.
            activeViews.RemoveAt(viewIndex);
        }
        else if (activeViews[viewIndex].ShallowLine.Above(bottomRight))
        {
            // The current coordinate is intersected by the shallow
            // line of the current view. The shallow line needs to be
            // raised.
            AddShallowBump(topLeft, activeViews, viewIndex);
        }
        else if (activeViews[viewIndex].SteepLine.Below(topLeft))
        {
            // The current coordinate is intersected by the steep line
            // of the current view. The steep line needs to be lowered.
            AddSteepBump(bottomRight, activeViews, viewIndex);
        }
        else
        {
            // The current coordinate is completely between the two
            // lines of the current view. Split the current view int
            // two views above and below the current coordinate.

            int shallowViewIndex = viewIndex;
            viewIndex += 1;
            int steepViewIndex = viewIndex;

            activeViews.Insert(shallowViewIndex, (View)activeViews[shallowViewIndex].Clone());

            AddSteepBump(bottomRight, activeViews, shallowViewIndex);

            if (!CheckView(activeViews, shallowViewIndex))
            {
                viewIndex -= 1;
                steepViewIndex -= 1;
            }

            AddShallowBump(topLeft, activeViews, steepViewIndex);
            CheckView(activeViews, steepViewIndex);
        }
    }

    private static void AddShallowBump(Vector2Int p, IList<View> activeViews, int viewIndex)
    {
        activeViews[viewIndex].ShallowLine.xf = p.x;
        activeViews[viewIndex].ShallowLine.yf = p.y;

        activeViews[viewIndex].ShallowBump = new ViewBump(p, activeViews[viewIndex].ShallowBump);

        ViewBump currentBump = activeViews[viewIndex].SteepBump;
        while (currentBump != null)
        {
            if (activeViews[viewIndex].ShallowLine.Above(currentBump.P))
            {
                activeViews[viewIndex].ShallowLine.xi = currentBump.P.x;
                activeViews[viewIndex].ShallowLine.yi = currentBump.P.y;
            }

            currentBump = currentBump.Parent;
        }
    }

    private static void AddSteepBump(Vector2Int p, IList<View> activeViews, int viewIndex)
    {
        activeViews[viewIndex].SteepLine.xf = p.x;
        activeViews[viewIndex].SteepLine.yf = p.y;

        activeViews[viewIndex].SteepBump = new ViewBump(p, activeViews[viewIndex].SteepBump);

        ViewBump currentBump = activeViews[viewIndex].ShallowBump;
        while (currentBump != null)
        {
            if (activeViews[viewIndex].SteepLine.Below(currentBump.P))
            {
                activeViews[viewIndex].SteepLine.xi = currentBump.P.x;
                activeViews[viewIndex].SteepLine.yi = currentBump.P.y;
            }

            currentBump = currentBump.Parent;
        }
    }

    private static bool CheckView(IList<View> activeViews, int viewIndex)
    {
        // Remove the view in activeViews at index viewIndex if:
        //  * The two lines are coolinear.
        //  * The lines pass through either extremity.

        Line shallowLine = activeViews[viewIndex].ShallowLine;
        Line steepLine = activeViews[viewIndex].SteepLine;

        if (shallowLine.LineCollinear(steepLine) &&
            shallowLine.Collinear(new Vector2Int(0, 1)) ||
            shallowLine.Collinear(new Vector2Int(1, 0)))
        {
            activeViews.RemoveAt(viewIndex);
            return false;
        }
        return true;
    }

    private class Line : ICloneable
    {
        public int xf;
        public int xi;
        public int yf;
        public int yi;

        public Line(int xi, int yi, int xf, int yf)
        {
            this.xi = xi;
            this.yi = yi;
            this.xf = xf;
            this.yf = yf;
        }

        public int Dx
        {
            get { return xf - xi; }
        }

        public int Dy
        {
            get { return yf - yi; }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public bool Below(Vector2Int p)
        {
            return RelativeSlope(p) > 0;
        }

        public bool BelowOrCollinear(Vector2Int p)
        {
            return RelativeSlope(p) >= 0;
        }

        public bool Above(Vector2Int p)
        {
            return RelativeSlope(p) < 0;
        }

        public bool AboveOrCollinear(Vector2Int p)
        {
            return RelativeSlope(p) <= 0;
        }

        public bool Collinear(Vector2Int p)
        {
            return RelativeSlope(p) == 0;
        }

        public bool LineCollinear(Line line)
        {
            return Collinear(new Vector2Int(line.xi, line.yi)) &&
                   Collinear(new Vector2Int(line.xf, line.yf));
        }

        public int RelativeSlope(Vector2Int p)
        {
            return (Dy * (xf - p.x)) - (Dx * (yf - p.y));
        }
    }

    private class View : ICloneable
    {
        public View(Line shallowLine, Line steepLine)
        {
            ShallowLine = shallowLine;
            SteepLine = steepLine;
        }

        public Line ShallowLine { get; private set; }
        public Line SteepLine { get; private set; }
        public ViewBump ShallowBump { get; set; }
        public ViewBump SteepBump { get; set; }

        public object Clone()
        {
            var view = new View(
                (Line)ShallowLine.Clone(),
                (Line)SteepLine.Clone());
            if (ShallowBump != null)
                view.ShallowBump = (ViewBump)ShallowBump.Clone();
            if (SteepBump != null)
                view.SteepBump = (ViewBump)SteepBump.Clone();
            return view;
        }
    }

    private class ViewBump : ICloneable
    {
        public ViewBump(Vector2Int p, ViewBump parent)
        {
            P = p;
            Parent = parent;
        }

        public Vector2Int P { get; private set; }
        public ViewBump Parent { get; private set; }

        public object Clone()
        {
            return new ViewBump(new Vector2Int(P.x, P.y), Parent);
        }
    }
}
