using System;
using System.Linq;
using System.Collections.Generic;
using Elements.Geometry;

namespace GeometryEx
{
    /// <summary>
    /// Extends Elements.Geometry.Mesh with utility methods.
    /// </summary>
    public static class TriangleEx
    {
        /// <summary>
        /// Returns the area of the triangle.
        /// </summary>
        /// <returns>A double.</returns>
        public static double Area(this Elements.Geometry.Triangle triangle)
        {
            var a = triangle.Vertices[0].Position;
            var b = triangle.Vertices[1].Position;
            var c = triangle.Vertices[2].Position;

            return Math.Abs((a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y)) * 0.5);
        }

        /// <summary>
        /// Returns the centroid of the triangle.
        /// </summary>
        /// <returns>A double.</returns>
        public static Vector3 Centroid(this Elements.Geometry.Triangle triangle)
        {
            var a = triangle.Vertices[0].Position;
            var b = triangle.Vertices[1].Position;
            var c = triangle.Vertices[2].Position;

            return new Line(a, new Line(b, c).Midpoint()).PointAt(0.666666666);
        }

        /// <summary>
        /// Returns the edges of a Triangle as a List of Lines.
        /// </summary>
        /// <returns>A List of Lines.</returns>
        public static List<Line> Edges(this Elements.Geometry.Triangle triangle)
        {
            var edges = new List<Line>();
            var points = new List<Vector3>();
            triangle.Vertices.ToList().ForEach(v => points.Add(v.Position));
            if (points.Distinct().Count() != 3)
            {
                return new List<Line>();
            }
            edges.Add(new Line(points[0], points[1]));
            edges.Add(new Line(points[1], points[2]));
            edges.Add(new Line(points[2], points[0]));
            return edges;
        }

        /// <summary>
        /// Return true if the supplied Elements.Geometry.Triangle has the same vertices as this Triangle.
        /// </summary>
        /// <param name="thatTriangle">Triangle to compare to this Triangle.</param>
        /// <returns>
        /// True if the Triangle vertex positions are AlmostEqual to those of the supplied Triangle.
        /// </returns>
        public static bool IsEqualTo(this Elements.Geometry.Triangle triangle,
                                          Elements.Geometry.Triangle thatTriangle)
        {
            var points = triangle.Points();
            var thosePnts = thatTriangle.Points();
            var common = 0;
            points.ForEach(p => common += p.Occurs(thosePnts));
            if (common == 3)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Tests whether the triangle's normal is parallel to the supplied normal.
        /// </summary>
        /// <param name="normal">The Vector3 normal to compare.</param>
        /// <returns>
        /// True if both normals have the same direction.
        /// </returns>
        public static bool IsLevel(this Triangle triangle, Vector3 normal)
        {
            var polygon = triangle.ToPolygon();
            if (new Line(triangle.Centroid(), normal, 1.0).Direction() !=
                new Line(polygon.Centroid(), polygon.Normal(), 1.0).Direction())
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Return true if an Equal Elements.Geometry.Triangle appears at least once in the supplied list.
        /// </summary>
        /// <returns>
        /// True if the Triangle vertex positions are AlmostEqual to those of any Triangle in the supplied List.
        /// </returns>
        public static bool IsListed(this Elements.Geometry.Triangle triangle,
                                         List<Elements.Geometry.Triangle> triangles)
        {
            foreach (var entry in triangles)
            {
                if (triangle.IsEqualTo(entry))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Return the number of times an equal Elements.Geometry.Triangle in the supplied list.
        /// </summary>
        /// <returns>
        /// An integer.
        /// </returns>
        public static int Occurs(this Elements.Geometry.Triangle triangle,
                                 List<Elements.Geometry.Triangle> triangles)
        {
            int count = 0;
            foreach (var entry in triangles)
            {
                if (triangle.IsEqualTo(entry))
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Returns the Vector3 Vertex Positions of the triangle.
        /// </summary>
        /// <param name="triangle"></param>
        /// <returns>
        /// A Vector3 List.
        /// </returns>
        public static List<Vector3> Points(this Elements.Geometry.Triangle triangle)
        {
            var points = new List<Vector3>();
            triangle.Vertices.ToList().ForEach(v => points.Add(v.Position));
            return points;
        }

        /// <summary>
        /// Inserts this Triangle into a new List.
        /// </summary>
        /// <returns>A new List containing this Triangle.</returns>
        public static List<Elements.Geometry.Triangle> ToList(this Elements.Geometry.Triangle triangle)
        {
            return new List<Elements.Geometry.Triangle> { triangle };
        }

        /// <summary>
        /// Returns the Triangle as a CCW Polygon.
        /// </summary>
        /// <returns></returns>
        public static Polygon ToPolygon(this Elements.Geometry.Triangle triangle)
        {
            if (triangle.Vertices.Select(v => v.Position).Distinct().Count() != 3)
            {
                return null;
            }
            var polygon = new Polygon(triangle.Points());
            return polygon.IsClockWise() ? polygon.Reversed() : polygon;
        }
    }
}
