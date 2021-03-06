﻿using System;
using System.Linq;
using System.Collections.Generic;
using ClipperLib;
using Elements.Geometry;

namespace GeometryEx
{
    public static class LineEx
    {
        /// <summary>
        /// Constructs the geometric differences between this Line and the supplied Polygons.
        /// </summary>
        /// <param name="diffs">The list of intersecting Polygons.</param>
        /// <returns>
        /// Returns a list of Lines representing the subtraction of the Lines intersecting the supplied list of Polygons.
        /// </returns>
        public static List<Line> Differences(this Line line, IList<Polygon> diffs)
        {
            var thisPath = LineToClipper(line);
            var polyPaths = new List<List<IntPoint>>();
            foreach (Polygon poly in diffs)
            {
                polyPaths.Add(Shaper.PolygonToClipper(poly));
            }
            Clipper clipper = new Clipper();
            clipper.AddPath(thisPath, PolyType.ptSubject, false);
            clipper.AddPaths(polyPaths, PolyType.ptClip, true);
            var solution = new PolyTree();
            clipper.Execute(ClipType.ctDifference, solution, PolyFillType.pftEvenOdd);
            var soLines = Clipper.OpenPathsFromPolyTree(solution);
            var lines = new List<Line>();
            foreach (List<IntPoint> path in soLines)
            {
                lines.Add(LineFromClipper(path.ToList()));
            }
            return lines;
        }

        /// <summary>
        /// Measures the distance from this Line's midpoint to the supplied plane.
        /// </summary>
        /// <param name="plane">The Plane to measure to.</param>
        /// <returns>
        /// A double.
        /// </returns>
        public static double DistanceTo(this Line line, Plane plane)
        {
           return line.Midpoint().DistanceTo(plane);
        }

        /// <summary>
        /// Returns a List of Vector3 points representing the division of 
        /// the linear geometry into the supplied number of segments.
        /// </summary>
        /// <param name="segments">The quantity of desired segments.</param>
        /// <returns>
        /// A List of Vector3 including the start and end points of the series.
        /// </returns>
        public static List<Vector3> Divide(this Line line, int segments)
        {
            var lines = line.DivideIntoEqualSegments(segments);
            var points = new List<Vector3>();
            foreach (var segment in lines)
            {
                points.Add(segment.Start);
            }
            points.Add(lines.Last().End);
            return points;
        }

        /// <summary>
        /// Extends this Line from its start point by the supplied distance.
        /// </summary>
        /// <param name="length">length by which to extend this line.</param>
        /// <returns>
        /// A new Line.
        /// </returns>
        public static Line ExtendStart(this Line line, double length)
        {
            length += line.Length();
            return new Line(line.Start, line.Direction().Negate(), length);
        }

        /// <summary>
        /// Extends this Line from its end point by the supplied distance.
        /// </summary>
        /// <param name="length">length by which to extend this line.</param>
        /// <returns>
        /// A new Line.
        /// </returns>
        public static Line ExtendEnd(this Line line, double length)
        {
            length += line.Length();
            return new Line(line.Start, line.Direction(), length);
        }

        /// <summary>
        /// Checks for intersection with the supplied Lines.
        /// </summary>
        /// <param name="lines">List of lines to check.</param>
        public static bool IntersectsMiddle(this Line line, List<Line> lines)
        {
            if (line == null || lines == null || lines.Count == 0)
            {
                return false;
            }
            for (var i = 0; i < lines.Count; i++)
            {
                if (line.Intersects2D(lines[i]) &&
                   !line.Start.IsAlmostEqualTo(lines[i].Start) && !line.Start.IsAlmostEqualTo(lines[i].End) &&
                   !line.End.IsAlmostEqualTo(lines[i].Start) && !line.End.IsAlmostEqualTo(lines[i].End))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Finds the implied intersection of this Line with a supplied Line.
        /// </summary>
        /// <param name="intr">Line to find intersection with this Line.</param>
        /// <returns>
        /// A Vector3 point or null if the lines are parallel.
        /// </returns>
        public static Vector3 Intersection(this Line line, Line intr)
        {
            var lineSlope = line.Slope();
            var intrSlope = intr.Slope();
            if (lineSlope.NearEqual(intrSlope))
            {
                return new Vector3(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
            }
            if (Math.Abs(lineSlope) == double.PositiveInfinity && intrSlope.NearEqual(0.0))
            {
                return new Vector3(line.Start.X, intr.Start.Y);
            }
            if (Math.Abs(intrSlope) == double.PositiveInfinity && lineSlope.NearEqual(0.0))
            {
                return new Vector3(intr.Start.X, line.Start.Y);
            }
            double lineB;
            double intrB;
            if (Math.Abs(lineSlope) == double.PositiveInfinity)
            {
                intrB = intr.End.Y - (intrSlope * intr.End.X);
                return new Vector3(line.End.X, intrSlope * line.End.X + intrB);
            }
            if (Math.Abs(intrSlope) == double.PositiveInfinity)
            {
                lineB = line.End.Y - (lineSlope * line.End.X);
                return new Vector3(intr.End.X, lineSlope * intr.End.X + lineB);
            }
            lineB = line.End.Y - (lineSlope * line.End.X);
            intrB = intr.End.Y - (intrSlope * intr.End.X);
            var x = (intrB - lineB) / (lineSlope - intrSlope);
            var y = lineSlope * x + lineB;
            return new Vector3(x, y);
        }

        /// <summary>
        /// Returns whether the endpoints of this Line both fall on the supplied Line.
        /// </summary>
        /// <param name="thatLine">Line to compare to this Line.</param>
        /// <returns>
        /// True is this Line's Start and End points fall on the supplied Line.
        /// </returns>
        public static bool IsColinearWith(this Line line, Line thatLine, bool contiguous = false)
        {
            if (contiguous && (thatLine.PointOnLine(line.Start, true) || thatLine.PointOnLine(line.End, true)))
            {
                return true;
            }
            if (!thatLine.PointOnLine(line.Start, true) || !thatLine.PointOnLine(line.End, true))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Return true if the supplied Line has the same 2D endpoints as this Line.
        /// </summary>
        /// <param name="thatLine">Line to compare to this Line.</param>
        /// <returns>
        /// True if the 2D endpoints are near equal to those of the supplied Line.
        /// </returns>
        public static bool IsEqual2D(this Line line, Line thatLine)
        {
            var line2D = new Line(new Vector3(line.Start.X, line.Start.Y, 0.0),
                                  new Vector3(line.End.X, line.End.Y, 0.0));
            var thatLine2D = new Line(new Vector3(thatLine.Start.X, thatLine.Start.Y, 0.0),
                                      new Vector3(thatLine.End.X, thatLine.End.Y, 0.0));
            if ((line2D.Start.IsAlmostEqualTo(thatLine2D.Start) || line2D.Start.IsAlmostEqualTo(thatLine2D.End)) &&
               ((line2D.End.IsAlmostEqualTo(thatLine2D.Start) || line2D.End.IsAlmostEqualTo(thatLine2D.End))))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Return true if the supplied Line has the same endpoints as this Line.
        /// </summary>
        /// <param name="thatLine">Line to compare to this Line.</param>
        /// <returns>
        /// True if the endpoints are near equal to those of the supplied Line.
        /// </returns>
        public static bool IsEqualTo(this Line line, Line thatLine)
        {
            if ((line.Start.IsAlmostEqualTo(thatLine.Start) || line.Start.IsAlmostEqualTo(thatLine.End)) &&
               ((line.End.IsAlmostEqualTo(thatLine.Start) || line.End.IsAlmostEqualTo(thatLine.End))))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns whether a Line is parallel to the x-axis.
        /// </summary>
        /// <returns>
        /// True if the Line's Direction equals Vector3.XAxis or its negation.
        /// </returns>
        public static bool IsHorizontal(this Line line)
        {
            if (line.Direction() != Vector3.XAxis && line.Direction() != Vector3.XAxis.Negate())
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Return true if an Equal Line appears at least once in the supplied list.
        /// </summary>
        /// <returns>
        /// True if both Line endpoints are NearEqual to those of a Line in the supplied List.
        /// </returns>
        public static bool IsListed(this Line line, List<Line> lines)
        {
            foreach (var entry in lines)
            {
                if (line.IsEqualTo(entry))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if this Line is parallel to the supplied line.
        /// </summary>
        /// <param name="thatLine">Line to compare to this Line.</param>
        /// <returns>
        /// True if the Lines have equal or negated Direction.
        /// </returns>
        public static bool IsParallelTo(this Line line, Line thatLine)
        {
            if (line.Direction() != thatLine.Direction() &&
                line.Direction() != thatLine.Direction().Negate())
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Returns true if this Line is perpendicular to the supplied line.
        /// </summary>
        /// <param name="thatLine">Line to compare to this line.</param>
        /// <returns>
        /// True if the product of the slopes is -1.
        /// </returns>
        public static bool IsPerpendicularTo(this Line line, Line thatLine)
        {
            if (line.Direction().Dot(thatLine.Direction()) != 0.0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Returns whether a Line is parallel to the y-axis.
        /// </summary>
        /// <returns>
        /// True if the Line's Direction equals Vector3.YAxis or its negation.
        /// </returns>
        public static bool IsVertical(this Line line)
        {
            if (line.Direction() != Vector3.YAxis && line.Direction() != Vector3.YAxis.Negate())
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Calculates a new Line from this Line and a supplied Line with a single coincident endpoint and identical slope.
        /// </summary>
        /// <returns>
        /// A new Line.
        /// </returns>
        public static Line JoinTo (this Line line, Line join)
        {
            if(!line.IsColinearWith(join, true))
            {
                return null;
            }
            var length = line.Length() + join.Length();
            if (line.Start.DistanceTo(join.Start).NearEqual(length))
            {
                return new Line(line.Start, join.Start);
            }
            if (line.Start.DistanceTo(join.End).NearEqual(length))
            {
                return new Line(line.Start, join.End);
            }
            if (line.End.DistanceTo(join.Start).NearEqual(length))
            {
                return new Line(line.End, join.Start);
            }
            if (line.End.DistanceTo(join.End).NearEqual(length))
            {
                return new Line(line.End, join.End);
            }
            return null;
        }

        /// <summary>
        /// Returns the midpoint between the Line's start and end.
        /// </summary>
        /// <returns>
        /// A Vector3 point.
        /// </returns>
        public static Vector3 Midpoint(this Line line)
        {
            return new Vector3((line.Start.X + line.End.X) * 0.5, 
                               (line.Start.Y + line.End.Y) * 0.5, 
                               (line.Start.Z + line.End.Z) * 0.5);
        }

        /// <summary>
        /// Returns a new line displaced from the supplied line along a 2D vector calculated between the supplied Vector3 points.
        /// </summary>
        /// <param name="from">The Vector3 base point of the move.</param>
        /// <param name="to">The Vector3 target point of the move.</param>
        /// <returns>
        /// A new Line.
        /// </returns>
        public static Line MoveFromTo(this Line line, Vector3 from, Vector3 to)
        {
            var v = new Vector3(to.X - from.X, to.Y - from.Y);
            return new Line(line.Start + v, line.End + v);
        }

        /// <summary>
        /// Return the number of times an Equal Line appears in the supplied list.
        /// </summary>
        /// <returns>
        /// An integer.
        /// </returns>
        public static int Occurs(this Line line, List<Line> lines)
        {
            int count = 0;
            foreach (var entry in lines)
            {
                if (line.IsEqualTo(entry))
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Returns the perpendicular distance from this Line to the supplied Vector3 point.
        /// </summary>
        /// <param name="point">Vector3 representing a point.</param>
        /// <returns>A double.</returns>
        public static double PerpendicularDistanceTo(this Line line, Vector3 point)
        {
            if (line.PointOnLine(point, true))
            {
                return 0.0;
            }
            var area = Math.Abs(Shaper.MakePolygon(new[] { line.Start, line.End, point }.ToList()).Area());
            return area / (line.Length() * 0.5);
        }

        /// <summary>
        /// Tests whether any of the suppiled points falls along this line.
        /// </summary>
        /// <param name="point">List of Vector3 points to compare to this Line.</param>
        /// <returns>
        /// True if any of the supplied Vector3 points is coincident with this Line.
        /// </returns>
        public static bool PointsOnLine(this Line line, List<Vector3> points)
        {
            foreach(var point in points)
            {
                if (line.PointOnLine(point, true))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns a Vector3 List of this Line's Start and End points.
        /// </summary>
        /// <param name="line"></param>
        /// <returns>A List of Vector3s.</returns>
        public static List<Vector3> Points(this Line line)
        {
            return new List<Vector3> { line.Start, line.End };
        }

        /// <summary>
        /// Returns a point the supplied distance along this Line.
        /// </summary>
        /// <param name="distance">Distance along this Line to the desired point.</param>
        /// <returns>
        /// A Vector3 point on the line.
        /// If the distance exceeds the length of the line, returns the endpoint of this Line.
        /// </returns>
        public static Vector3 PositionAt(this Line line, double distance)
        {
            if (distance >= line.Length())
            {
                return line.End;
            }
            return line.PointAt(distance / line.Length());
        }

        /// <summary>
        /// Returns a Line with reversed endpoints in comparison to this Line.
        /// </summary>
        /// <returns>
        /// A new Line.
        /// </returns>
        public static Line Reverse(this Line line)
        {
            return new Line(line.End, line.Start);
        }

        /// <summary>
        /// Creates a new Line from the supplied Line rotated around the supplied pivot point by the specified angle in degrees.
        /// </summary>
        /// <param name="pivot">Vector3 base point of the rotation.</param>
        /// <param name="angle">Desired rotation angle in degrees.</param>
        /// <returns>
        /// A new Line.
        /// </returns>
        public static Line Rotate(this Line line, Vector3 pivot, double angle)
        {
            var theta = angle * (Math.PI / 180);
            var sX = (Math.Cos(theta) * (line.Start.X - pivot.X)) - (Math.Sin(theta) * (line.Start.Y - pivot.Y)) + pivot.X;
            var sY = (Math.Sin(theta) * (line.Start.X - pivot.X)) + (Math.Cos(theta) * (line.Start.Y - pivot.Y)) + pivot.Y;
            var eX = (Math.Cos(theta) * (line.End.X - pivot.X)) - (Math.Sin(theta) * (line.End.Y - pivot.Y)) + pivot.X;
            var eY = (Math.Sin(theta) * (line.End.X - pivot.X)) + (Math.Cos(theta) * (line.End.Y - pivot.Y)) + pivot.Y;
            return new Line(new Vector3(sX, sY), new Vector3(eX, eY));
        }

        /// <summary>
        /// Returns a list of Lines by dividing the supplied Line by the supplied length from the Line's start point.
        /// </summary>
        /// <param name="length">Longest allowable segment.</param>
        /// <returns>
        /// A list of Lines of the specified length and a shorter line representing any remainder, or a list containing a copy of the supplied Line if the supplied length is greater than the Line.
        /// </returns>
        public static List<Line> Segment(this Line line, double length, double minimum = 1e-08)
        {
            var lines = new List<Line>();
            var segments = line.Length() / length;
            if (segments <= 1.0 || minimum > length)
            {
                lines.Add(line);
                return lines;
            }
            var start = line.Start;
            for (int i = 0; i < Math.Floor(segments); i++)
            {
                var end = line.PointAt(length / line.Length() * (i + 1));
                if (start.IsAlmostEqualTo(end))
                {
                    break;
                }
                var newLine = new Line(start, end);
                lines.Add(newLine);
                start = newLine.End;
            }
            start = lines.Last().End;
            if (!start.IsAlmostEqualTo(line.End))
            {
                var remainder = new Line(start, line.End);
                if (remainder.Length() < minimum)
                {
                    var joiner = lines.Last();
                    lines.Reverse();
                    lines = lines.Skip(1).ToList();
                    lines.Reverse();
                    var tstJoin = joiner.JoinTo(remainder);
                    if (tstJoin != null)
                    {
                        remainder = new Line(tstJoin.Start, tstJoin.End);
                    }
                }
                lines.Add(remainder);
            }
            return lines;
        }

        /// <summary>
        /// Returns a list of Lines by dividing the supplied Line by the supplied length from the specified start point.
        /// </summary>
        /// <param name="length">Longest allowable segment.</param>
        /// <returns>
        /// A list of Lines of the specified length and shorter line or lines representing any remainder, or a List containing a copy of the supplied Line if the supplied length is greater than the Line.
        /// </returns>
        public static List<Line> SegmentFrom(this Line line, double length, DivideFrom from = DivideFrom.Start, double minimum = 1e-08)
        {
            var lines = new List<Line>();
            if (length >= line.Length())
            {
                lines.Add(line);
                return lines;
            }
            switch (from)
            {
                case DivideFrom.Center:
                    lines.AddRange(new Line(line.Midpoint(), line.Start).Segment(length, minimum));
                    lines.AddRange(new Line(line.Midpoint(), line.End).Segment(length, minimum));
                    break;
                case DivideFrom.Centered:
                    var start = line.PositionAt((line.Length() * 0.5) - (length * 0.5));
                    var end = line.PositionAt((line.Length() * 0.5) + (length * 0.5));
                    lines.Add(new Line(start, end));
                    lines.AddRange(new Line(start, line.Start).Segment(length, minimum));
                    lines.AddRange(new Line(end, line.End).Segment(length, minimum));
                    break;
                case DivideFrom.End:
                    lines.AddRange(new Line(line.End, line.Start).Segment(length, minimum));
                    break;
                default:
                    lines.AddRange(line.Segment(length, minimum));
                    break;
            }
            return lines;
        }

        /// <summary>
        /// Returns whether this Line shares an endpoint with the supplied Line.
        /// </summary>
        /// <param name="thatLine">Line to compare to this Line.</param>
        /// <returns>
        /// True if the Lines share an endpoint.
        /// </returns>
        public static bool SharesEndpointWith(this Line line, Line thatLine)
        {
            if (!line.IsEqualTo(thatLine) &&
                (line.End.IsAlmostEqualTo(thatLine.End) || line.Start.IsAlmostEqualTo(thatLine.Start) ||
                line.Start.IsAlmostEqualTo(thatLine.End) || line.End.IsAlmostEqualTo(thatLine.Start)))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns a list of Lines sharing an endpoint with the supplied Line.
        /// </summary>
        /// <param name="thatLine">Line to compare to this Line.</param>
        /// <returns>
        /// A List of Lines.
        /// </returns>
        public static List<Line> SharesEndpointWith(this Line line, List<Line> lines)
        {
            return lines.Where(l => l.SharesEndpointWith(line)).ToList();
        }

        /// <summary>
        /// Returns the slope of the Line, normalizing a vertical Line to a slope of positive infinity.
        /// </summary>
        /// <returns>
        /// A double representing the slope of the line.
        /// </returns>
        public static double Slope (this Line line)
        {
            var slope = (line.End.Y - line.Start.Y) / (line.End.X - line.Start.X);
            if (slope.NearEqual(double.NegativeInfinity) || slope.NearEqual(double.PositiveInfinity))
            {
                slope = double.PositiveInfinity;
            }
            return slope;
        }

        /// <summary>
        /// Inserts this Line into a new List.
        /// </summary>
        /// <returns>A List containing this Line.</returns>
        public static List<Line> ToList(this Line line)
        {
            return new List<Line> { line };
        }

        /// <summary>
        /// Returns this Line as a Polyline.
        /// </summary>
        /// <param name="line"></param>
        /// <returns>A new Polyline</returns>
        public static Polyline ToPolyline(this Line line)
        {
            return new Polyline(line.Points());
        }

        /// <summary>
        /// Construct a clipper path from this Line.
        /// </summary>
        /// <param name="scale">
        /// Scaling factor for translation into Clipper coordinates.
        /// </param>
        /// <returns>
        /// A Clipper path.
        /// </returns>
        internal static List<IntPoint> LineToClipper(this Line line, double scale = Shaper.SCALE)
        {
            var path = new List<IntPoint>
            {
                new IntPoint(line.Start.X * scale, line.Start.Y * scale),
                new IntPoint(line.End.X * scale, line.End.Y * scale)
            };
            return path.ToList();
        }

        /// <summary>
        /// Construct a Line from a Clipper path 
        /// </summary>
        /// <param name="scale">
        /// Scaling factor for translation into Clipper coordinates.
        /// </param>
        /// <returns>
        /// A new Line.
        /// </returns>
        internal static Line LineFromClipper(this List<IntPoint> line, double scale = Shaper.SCALE)
        {
            return new Line(new Vector3(line.First().X / scale, line.First().Y / scale),
                            new Vector3(line.Last().X / scale, line.Last().Y / scale));
        }
    }
}
