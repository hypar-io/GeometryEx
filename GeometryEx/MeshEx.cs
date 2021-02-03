using System;
using System.Linq;
using System.Collections.Generic;
using Elements.Geometry;

namespace GeometryEx
{
    /// <summary>
    /// Extends Elements.Geometry.Mesh with utility methods.
    /// </summary>
    public static class MeshEx
    {
        /// <summary>
        /// Adds a List of Triangles and their Vertices to the Mesh in one step, filtering for repetition.
        /// </summary>
        /// <param name="triangles">A List of Triangles</param>
        public static int AddTriangles(this Mesh mesh, List<Triangle> triangles)
        {
            var added = 0;
            var meshTris = mesh.Triangles.ToList();
            foreach (var triangle in triangles)
            {
                if (triangle.IsListed(meshTris))
                {
                    continue;
                }
                mesh.AddTriangle(triangle);
                triangle.Vertices.ToList().ForEach(v => mesh.AddVertex(v));
                added++;
            }
            return added;
        }

        /// <summary>
        /// Returns the Mesh points adjacent to the supplied Mesh point.
        /// </summary>
        /// <param name="point">A Vector3 point located at a Mesh Vertex.</param>
        /// <returns>
        /// A List of Vector3 points or null if there is no Vertex at the supplied Vector3 point.
        /// </returns>
        public static List<Vector3> AdjacentPoints(this Mesh mesh, Vector3 point)
        {
            if (!point.IsListed(mesh.Points()))
            {
                return null;
            }
            var points = new List<Vector3>();
            EdgesAt(mesh, point).ForEach(e => points.Add(e.Start));
            return points;
        }

        /// <summary>
        /// Returns the Mesh Triangle(s) bordering the supplied Line Mesh edge.
        /// </summary>
        /// <param name="edge">A Line representing a Mesh edge.</param>
        /// <returns>
        /// A List of Triangles or null if the supplied Line does not represent a Mesh edge.
        /// </returns>
        public static List<Triangle> AdjacentTriangles(this Mesh mesh, Line edge)
        {
            if (!edge.IsListed(mesh.Edges()))
            {
                return null;
            }
            var triangles = new List<Triangle>();
            var validTriangles = mesh.Triangles.Where(t => t.Vertices.Select(v => v.Position).Distinct().Count() == 3);
            foreach (var triangle in validTriangles)
            {
                var points = triangle.Points();
                if (edge.Start.IsListed(points) && edge.End.IsListed(points))
                {
                    triangles.Add(triangle);
                }
            }
            return triangles;
        }

        /// <summary>
        /// Returns the Mesh Triangle(s) that share two vertices of the supplied Triangle.
        /// </summary>
        /// <param name="triangle">A Mesh triangle.</param>
        /// <returns>
        /// A List of Triangles or null if the supplied Triangle does not appear in the Mesh.
        /// </returns>
        public static List<Triangle> AdjacentTriangles(this Mesh mesh, Triangle triangle)
        {
            var meshTris = mesh.Triangles;
            if (!triangle.IsListed(mesh.Triangles.ToList()))
            {
                return null;
            }
            var triangles = new List<Triangle>();
            var triPoints = triangle.Points(); ;
            foreach (var triang in mesh.Triangles)
            {
                var common = 0;
                triang.Points().ForEach(p => common += p.Occurs(triPoints));
                if (common == 2)
                {
                    triangles.Add(triang);
                }
            }
            return triangles;
        }

        /// <summary>
        /// Returns the aggregate area of all Mesh triangles.
        /// </summary>
        /// <returns>
        /// A double.
        /// </returns>
        public static double Area(this Mesh mesh)
        {
            var area = 0.0;
            mesh.Triangles.ToList().ForEach(t => area += t.Area());
            return area;
        }

        /// <summary>
        /// Averages the vectors terminating at a Mesh Vertex..
        /// </summary>
        /// <param name="point">A Vector3 at the same position as a Vertex on the Mesh.</param>
        /// <returns>
        /// A Vector3 average or a MaxValue Vector3 if the supplied Vector3 point does not represent a Mesh Vertex.
        /// </returns>
        public static Vector3 AverageAt(this Mesh mesh, Vector3 point)
        {
            var average = new Vector3(double.MaxValue, double.MaxValue, double.MaxValue);
            if (!point.IsListed(mesh.Points()))
            {
                return average;
            }
            var vectors = new List<Vector3>();
            EdgesAt(mesh, point).ForEach(e =>
                vectors.Add(new Vector3(e.End.X - e.Start.X, e.End.Y - e.Start.Y, e.End.Z - e.Start.Z).Unitized()));
            average = vectors[0].Average(vectors[1]);
            for (int i = 2; i < vectors.Count; i++)
            {
                average = average.Average(vectors[i]);
            }
            return average.Unitized();
        }

        /// <summary>
        /// Returns the concave edges of the Mesh.
        /// </summary>
        /// <param name="normal">A Vector3 normal.</param>
        /// <returns>
        /// A List of Lines.
        /// </returns>
        public static List<Line> ConcaveEdges(this Mesh mesh, Vector3 normal)
        {
            var lowLines = new List<Line>();
            foreach (var edge in mesh.Edges())
            {
                if (mesh.IsConcave(edge, normal))
                {
                    lowLines.Add(edge);
                }
            }
            return lowLines;
        }

        /// <summary>
        /// Applies a concavity test to all mesh vertices in comparison to the supplied normal.
        /// </summary>
        /// <param name="normal">A Vector3 normal.</param>
        /// <returns>
        /// A List of Vector3 points.
        /// </returns>
        public static List<Vector3> ConcavePoints(this Mesh mesh, Vector3 normal)
        {
            return mesh.Points().Where(p => mesh.IsConcave(p, normal)).ToList();
        }

        /// <summary>
        /// Returns a List of Triangles concave relative to the supplied List of Triangles and a normal.
        /// </summary>
        /// <param name="triangles">A List of Triangles in the Mesh.</param>
        /// <param name="normal">A Vector normal.</param>
        /// <returns>
        /// A List of Triangles.
        /// </returns>
        public static List<Triangle> ConcaveTo(this Mesh mesh, List<Triangle> triangles, Vector3 normal)
        {
            var meshTris = mesh.Triangles.ToList();
            var concTris = new List<Triangle>(triangles);
            foreach (var triangle in triangles)
            {
                if (!triangle.IsListed(meshTris))
                {
                    continue;
                }
                var edges = triangle.Edges();
                foreach (var edge in edges)
                {
                    if (!mesh.IsConvex(edge, normal))
                    {
                        concTris.AddRange(mesh.AdjacentTriangles(edge).Where(t => !t.IsListed(concTris)));
                    }
                }
            }
            if (concTris.Count == 0 || concTris.Count == triangles.Count)
            {
                return concTris;
            }
            return mesh.ConcaveTo(concTris, normal);
        }

        /// <summary>
        /// Finds "high" lines in the mesh by comparing vertices with their adjacent vertices
        /// and returning edges between a high point and a convex point.
        /// </summary>
        /// <param name="edge">A Line representing an edge of this Mesh.</param>
        /// <param name="normal">A Vector3 normal.</param>
        /// <returns>
        /// A List of Lines.
        /// </returns>
        public static List<Line> ConvexEdges(this Mesh mesh, Vector3 normal)
        {
            var hiPoints = ConvexPoints(mesh, normal);
            var hiLines = new List<Line>();
            foreach (var point in hiPoints)
            {
                foreach (var adjPoint in mesh.AdjacentPoints(point))
                {
                    if (mesh.IsConvex(adjPoint, normal))
                    {
                        var line = new Line(adjPoint, point);
                        if (!line.IsListed(hiLines))
                        {
                            hiLines.Add(line);
                        }
                    }
                }
            }
            return hiLines;
        }

        /// <summary>
        /// Applies a convexity test to all mesh vertices in comparison to the supplied normal.
        /// </summary>
        /// <param name="normal">A Vector3 normal.</param>
        /// <returns>
        ///  A List of Vector3 points.
        /// </returns>
        public static List<Vector3> ConvexPoints(this Mesh mesh, Vector3 normal)
        {
            return mesh.Points().Where(p => mesh.IsConvex(p, normal)).ToList();
        }

        /// <summary>
        /// Returns a List of Triangles convex relative to the supplied List of Triangles and a normal.
        /// </summary>
        /// <param name="triangles">A List of Triangles in the Mesh.</param>
        /// <param name="normal">A Vector normal.</param>
        /// <returns>
        /// A List of Triangles.
        /// </returns>
        public static List<Triangle> ConvexTo(this Mesh mesh, List<Triangle> triangles, Vector3 normal)
        {
            var meshTris = mesh.Triangles.ToList();
            var convTris = new List<Triangle>(triangles);
            foreach (var triangle in triangles)
            {
                if (!triangle.IsListed(meshTris))
                {
                    continue;
                }
                var edges = triangle.Edges();
                foreach (var edge in edges)
                {
                    if (!mesh.IsConcave(edge, normal))
                    {
                        convTris.AddRange(mesh.AdjacentTriangles(edge).Where(t => !t.IsListed(convTris)));
                    }
                }
            }
            if (convTris.Count == 0 || convTris.Count == triangles.Count)
            {
                return convTris;
            }
            return mesh.ConcaveTo(convTris, normal);
        }

        /// <summary>
        /// Returns the Mesh Triangle(s) that share two vertices of the supplied Triangle
        /// and which are convex to the supplied Triangle relative to the supplied normal.
        /// </summary>
        /// <param name="triangle">A Mesh triangle.</param>
        /// <returns>
        /// A List of Triangles.
        /// </returns>
        public static List<Triangle> ConvexTo(this Mesh mesh, Triangle triangle, Vector3 normal)
        {
            var triangles = new List<Triangle>();
            var meshTris = mesh.Triangles.ToList();
            if (!triangle.IsListed(meshTris))
            {
                return triangles;
            }
            var triPoints = triangle.Points(); ;
            foreach (var triang in mesh.Triangles)
            {
                var common = triang.Points().Where(p => p.IsListed(triangle.Points())).ToList();
                if (common.Count == 2 &&
                    IsConvex(mesh, new Line(common.First(), common.Last()), normal))
                {
                    triangles.Add(triang);
                }
            }
            return triangles;
        }

        /// <summary>
        /// Returns the unique edges of a Mesh as a List of Lines.
        /// </summary>
        /// <returns>A List of Lines.</returns>
        public static List<Line> Edges(this Mesh mesh)
        {
            var edges = new List<Line>();
            mesh.Triangles.ToList().ForEach(t => edges.AddRange(t.Edges().Where(e => !e.IsListed(edges))));
            return edges;
        }

        /// <summary>
        /// Returns the unique edges intersecting a supplied point of the Mesh, or an empty list if the supplied point is not at the same position as a Mesh Vertex.
        /// </summary>
        /// <param name="point">A Vector3 at the same position as a Vertex on the Mesh.</param>
        /// <param name="orient">If true, returns lines whose end point is at the supplied point.</param>
        /// <returns>
        /// A List of Lines.
        /// </returns>
        public static List<Line> EdgesAt(this Mesh mesh, Vector3 point, bool orient = true)
        {
            var lines = new List<Line>();
            var points = mesh.Points();
            if (!points.Any(p => p.IsAlmostEqualTo(point)))
            {
                return lines;
            }
            mesh.Triangles.ToList().ForEach(t => lines.AddRange(t.Edges().Where(e => !e.IsListed(lines))));
            lines = lines.Where(e => e.Start.IsAlmostEqualTo(point) || e.End.IsAlmostEqualTo(point)).ToList();
            if (!orient)
            {
                return lines;
            }
            var edges = new List<Line>();
            foreach (var line in lines)
            {
                if (line.End.IsAlmostEqualTo(point))
                {
                    edges.Add(line);
                    continue;
                }
                edges.Add(line.Reverse());
            }
            return edges;
        }

        /// <summary>
        /// Returns a list of interior edges from a Mesh, treating all half-edges as full edges.
        /// </summary>
        /// <returns>
        /// A List of Lines.
        /// </returns>
        public static List<Line> EdgesInterior(this Mesh mesh)
        {
            var edges = new List<Line>();
            mesh.Triangles.ToList().ForEach(t => edges.AddRange(t.Edges()));
            var iEdges = edges.Where(e => e.Occurs(edges) == 2).ToList();
            edges.Clear();
            foreach (var edge in iEdges)
            {
                if (edge.Occurs(edges) == 0)
                {
                    edges.Add(edge);
                }
            }
            return edges;
        }

        /// <summary>
        /// Returns ordered Lists of perimeter edges from a Mesh.
        /// There is no guarantee of directionality, only spatially sequential lines.
        /// </summary>
        /// <returns>
        /// A List of Lines.
        /// </returns>
        public static List<List<Line>> EdgesPerimeters(this Mesh mesh)
        {
            var edges = new List<Line>();
            mesh.Triangles.ToList().ForEach(t => edges.AddRange(t.Edges()));
            var pEdges = edges.Where(e => e.Occurs(edges) == 1).ToList();
            var perimeters = new List<List<Line>>();
            while (pEdges.Count() > 0)
            {
                var edge = pEdges.First();
                pEdges = pEdges.Skip(1).ToList();
                var perimeter = new List<Line> { edge };
                var connected = pEdges.Where(e => e.Start.IsAlmostEqualTo(edge.End) || e.End.IsAlmostEqualTo(edge.End)).ToList();
                while (connected.Count() > 0)
                {
                    pEdges.Remove(connected.First());
                    edge = new Line(edge.End, edge.End.FarthestFrom(connected.First().Points()));
                    perimeter.Add(edge);
                    connected = pEdges.Where(e => e.Start.IsAlmostEqualTo(edge.End) || e.End.IsAlmostEqualTo(edge.End)).ToList();
                }
                perimeters.Add(perimeter);
            }
            if (perimeters.Count == 1)
            {
                return perimeters;
            }
            return perimeters.OrderByDescending(p => Shaper.TotalLength(p)).ToList();
        }

        /// <summary>
        /// Tests the spatial relationship of the supplied mesh point with adjacent points and a supplied normal to recursively discover the "highest" connected point with reference to the supplied normal.
        /// </summary>
        /// <param name="point">The Vector3 point to test.</param>
        /// <param name="normal">The Vector3 normal to compare.</param>
        /// <returns>
        /// A List of Vector3 "high" points relative to the supplied normal.
        /// </returns>
        public static List<Vector3> HighestFrom(this Mesh mesh, Vector3 point, Vector3 normal)
        {
            if (!point.IsListed(mesh.Points())) //point is not a Mesh vertex.
            {
                return null;
            }
            var plane = new Plane(point, normal);
            var adjPoints = AdjacentPoints(mesh, point)
                                           .Where(p => plane.SignedDistanceTo(p) > 0.0)
                                           .OrderByDescending(p => plane.SignedDistanceTo(p)).ToList();
            if (adjPoints.Count == 0)
            {
                return point.ToList();
            }
            var hiPoints = new List<Vector3>();
            adjPoints.ForEach(pnt => hiPoints.AddRange(HighestFrom(mesh, pnt, normal)));
            return hiPoints.Distinct().ToList();
        }

        /// <summary>
        /// Tests the spatial relationship of the supplied mesh point with adjacent points and a supplied normal to determine whether the point represents a concavity with respect to the normal.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="point">The Vector3 point to test.</param>
        /// <param name="normal">The Vector3 normal to compare.</param>
        /// <returns>
        /// True if all adjacent points are "higher" along the supplied normal in comparison to the supplied point.
        /// </returns>
        public static bool IsConcave(this Mesh mesh, Vector3 point, Vector3 normal)
        {
            if (!point.IsListed(mesh.Points())) //point is not a Mesh vertex.
            {
                return false;
            }
            var adjPoints = AdjacentPoints(mesh, point);
            foreach (var pnt in adjPoints)
            {
                var plane = new Plane(pnt, normal);
                if (plane.SignedDistanceTo(point) > 0.0)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Calculates whether a supplied edge of the Mesh is concave relative to its adjacent triangles or a supplied normal.
        /// </summary>
        /// <param name="edge">A Line representing an edge of this Mesh.</param>
        /// <param name="normal">A vector to compare adjacent triangle normals.</param>
        /// <returns>
        /// True if the edge is concave relative to the adjoining triangles or the supplied normal.
        /// </returns>
        public static bool IsConcave(this Mesh mesh, Line edge, Vector3 normal)
        {
            if (!edge.IsListed(mesh.Edges()))
            {
                return false;
            }
            var nDir = new Line(edge.End, normal, normal.Length());
            normal = nDir.End;
            var plane = new Plane(edge.Start, edge.End, normal);
            var triangles = mesh.AdjacentTriangles(edge);
            var intersects = 0;
            var parallels = 0;
            foreach (var triangle in triangles)
            {
                var polygon = triangle.ToPolygon();
                var ray = new Ray(polygon.Centroid(), polygon.Normal());
                var pDir = new Line(polygon.Centroid(), polygon.Normal(), normal.Length());
                if (pDir.Direction() == nDir.Direction())
                {
                    parallels++;
                }
                if (ray.Intersects(plane, out var v, out var d))
                {
                    intersects++;
                }
            }
            if (intersects == 1 && parallels == 1)
            {
                return true;
            }
            if (intersects < triangles.Count)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Tests the spatial relationship of the supplied mesh point with adjacent points and a supplied normal to determine whether the point represents a convexity with respect to the normal.
        /// </summary>
        /// <param name="point">The Vector3 point to test.</param>
        /// <param name="normal">The Vector3 normal to compare.</param>
        /// <returns>
        /// True if all adjacent points are "higher" along the supplied normal in comparison to the supplied point.
        /// </returns>
        public static bool IsConvex(this Mesh mesh, Vector3 point, Vector3 normal)
        {
            if (!point.IsListed(mesh.Points()))
            {
                return false;
            }
            var adjPoints = AdjacentPoints(mesh, point);
            foreach (var pnt in adjPoints)
            {
                var plane = new Plane(pnt, normal);
                if (plane.SignedDistanceTo(point) < 0.0)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Calculates whether a supplied edge of the Mesh is convex relative to its adjacent triangles or a supplied normal.
        /// </summary>
        /// <param name="edge">A Line representing an edge of this Mesh.</param>
        /// <param name="normal">A vector to compare adjacent triangle normals.</param>
        /// <returns>
        /// True if the edge is convex relative to the adjoining triangles or the supplied normal.
        /// </returns>
        public static bool IsConvex(this Mesh mesh, Line edge, Vector3 normal)
        {
            if (!edge.IsListed(mesh.Edges()))
            {
                return false;
            }
            var nDir = new Line(edge.End, normal, normal.Length());
            normal = nDir.End;
            var plane = new Plane(edge.Start, edge.End, normal);
            var triangles = mesh.AdjacentTriangles(edge);
            var intersects = 0;
            var parallels = 0;
            foreach (var triangle in triangles)
            {
                var polygon = triangle.ToPolygon();
                var ray = new Ray(polygon.Centroid(), polygon.Normal());
                var pDir = new Line(polygon.Centroid(), polygon.Normal(), normal.Length());
                if (pDir.Direction() == nDir.Direction())
                {
                    parallels++;
                }
                if (ray.Intersects(plane, out var v, out var d))
                {
                    intersects++;
                }
            }
            if (intersects == 1 && parallels == 1)
            {
                return true;
            }
            if (intersects > 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Tests the spatial relationship of the supplied Mesh point with adjacent points and a supplied normal to determine whether the points represent a plane with respect to the normal.
        /// </summary>
        /// <param name="point">The Vector3 point representing a Mesh Vertex to test.</param>
        /// <param name="normal">The Vector3 normal to compare.</param>
        /// <returns>
        /// True if all adjacent points are on the same plane along the supplied normal in comparison to the supplied point.
        /// </returns>
        public static bool IsFlat(this Mesh mesh, Vector3 point, Vector3 normal)
        {
            if (!point.IsListed(mesh.Points()))
            {
                return false;
            }
            var adjPoints = AdjacentPoints(mesh, point);
            foreach (var pnt in adjPoints)
            {
                var plane = new Plane(pnt, normal);
                if (plane.SignedDistanceTo(point) != 0.0)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Tests the spatial relationship of the supplied mesh edge with
        /// respect to the adjacent Triangles to determine if the edge is flat.
        /// </summary>
        /// <param name="edge">The Line representing a Mesh edge to test.</param>
        /// <returns>
        /// True if the adjacent triangles have normals in the same direction.
        /// </returns>
        public static bool IsFlat(this Mesh mesh, Line edge)
        {
            if (!edge.IsListed(mesh.Edges()))
            {
                return false;
            }
            var triangles = mesh.AdjacentTriangles(edge);
            if (triangles.Count == 1)
            {
                return true;
            }
            var pDir0 = new Line(triangles[0].Centroid(), triangles[0].ToPolygon().Normal(), 1.0).Direction();
            var pDir1 = new Line(triangles[1].Centroid(), triangles[1].ToPolygon().Normal(), 1.0).Direction();
            if (pDir0 != pDir1)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Returns whether all normals of this Mesh's triangles are equivalent.
        /// </summary>
        /// <returns>
        /// True if the absolute value of all normals is equivalent.
        /// </returns>
        public static bool IsFlat(this Mesh mesh)
        {
            var triangles = mesh.Triangles;
            var pDir0 = new Line(triangles[0].Centroid(), triangles[0].ToPolygon().Normal(), 1.0).Direction();
            foreach (var triangle in triangles.Skip(1))
            {
                if (pDir0 != new Line(triangle.Centroid(), triangle.ToPolygon().Normal(), 1.0).Direction())
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Tests the spatial relationship of the supplied mesh point with adjacent points and a supplied normal to recursively discover the "lowest" connected point with reference to the supplied normal.
        /// </summary>
        /// <param name="point">The Vector3 point to test.</param>
        /// <param name="normal">The Vector3 normal to compare.</param>
        /// <returns>
        /// A List of Vector3 "low" points relative to the supplied normal.
        /// </returns>
        public static List<Vector3> LowestFrom(this Mesh mesh, Vector3 point, Vector3 normal)
        {
            if (!point.IsListed(mesh.Points())) //point is not a Mesh vertex.
            {
                return null;
            }
            var plane = new Plane(point, normal);
            var adjPoints = AdjacentPoints(mesh, point)
                                           .Where(p => plane.SignedDistanceTo(p) < 0.0)
                                           .OrderBy(p => plane.SignedDistanceTo(p)).ToList();
            if (adjPoints.Count == 0)
            {
                return point.ToList();
            }
            var lowPoints = new List<Vector3>();
            adjPoints.ForEach(pnt => lowPoints.AddRange(LowestFrom(mesh, pnt, normal)));
            return lowPoints.Distinct().ToList();
        }

        public enum Normal { X, Y, Z }
        /// <summary>
        /// Returns a Plane for every distinct specified coordinate in this Mesh.
        /// </summary>
        /// <returns>
        /// A List of Planes in ascending axis order.
        /// </returns>
        public static List<Plane> Planes(this Mesh mesh, Normal axis)
        {
            var slices = new List<double>();
            var planes = new List<Plane>();
            var points = new List<Vector3>();
            mesh.Triangles.ToList().ForEach(t => t.Vertices.ToList().ForEach(v => points.Add(v.Position)));
            switch (axis)
            {
                case Normal.X:
                    points.ForEach(v => slices.Add(v.X));
                    slices.Distinct().ToList().ForEach(s => planes.Add(new Plane(new Vector3(s, 0.0, 0.0), Vector3.XAxis)));
                    planes = planes.OrderBy(p => p.Origin.X).ToList();
                    break;
                case Normal.Y:
                    points.ForEach(v => slices.Add(v.Y));
                    slices.Distinct().ToList().ForEach(s => planes.Add(new Plane(new Vector3(0.0, s, 0.0), Vector3.YAxis)));
                    planes = planes.OrderBy(p => p.Origin.Y).ToList();
                    break;
                case Normal.Z:
                    points.ForEach(v => slices.Add(v.Z));
                    slices.Distinct().ToList().ForEach(s => planes.Add(new Plane(new Vector3(0.0, 0.0, s), Vector3.ZAxis)));
                    planes = planes.OrderBy(p => p.Origin.Z).ToList();
                    break;
            }
            return planes; ;
        }

        /// <summary>
        /// Returns a list of distinct points in the Mesh.
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns>
        /// A Vector3 List.
        /// </returns>
        public static List<Vector3> Points(this Mesh mesh)
        {
            var triangles = mesh.Triangles;
            var points = new List<Vector3>();
            triangles.ToList().ForEach(t => points.AddRange(t.Points()));
            return points.Distinct().ToList();
        }

        /// <summary>
        /// Returns the perimeter Vector3 points of the Mesh in CCW order.
        /// </summary>
        /// <returns>
        /// A List of Vector3 points.
        /// </returns>
        public static List<Vector3> PointsBoundary(this Mesh mesh)
        {
            var pPoints = new List<Vector3>();
            var edges = EdgesPerimeters(mesh);
            if (edges.Count == 0)
            {
                return pPoints;
            }
            edges.First().ForEach(e => pPoints.Add(e.Start));
            var polygon = new Polygon(pPoints);
            return polygon.IsClockWise() ? polygon.Reversed().Vertices.ToList() : polygon.Vertices.ToList();
        }

        /// <summary>
        /// Returns the list of Mesh interior Vector3 points.
        /// </summary>
        /// <returns>
        /// A List of Vector3 points.
        /// </returns>
        public static List<Vector3> PointsInterior(this Mesh mesh)
        {
            var pPoints = mesh.PointsBoundary();
            return mesh.Points().Where(p => !p.IsListed(pPoints)).ToList();
        }

        /// <summary>
        /// Returns a 2D CCW polygon of the Mesh perimeter.
        /// </summary>
        /// <returns>
        /// A CCW Polygon.
        /// </returns>
        public static Polygon PolygonBoundary(this Mesh mesh)
        {
            var edges = EdgesPerimeters(mesh);
            if (edges.Count == 0)
            {
                return null;
            }
            var pPoints = new List<Vector3>();
            edges.First().ForEach(e => pPoints.Add(new Vector3(e.Start.X, e.Start.Y, 0.0)));
            var polygon = new Polygon(pPoints.Distinct().ToList());
            return !polygon.IsClockWise() ? polygon : polygon.Reversed();
        }

        /// <summary>
        /// Returns the third points ofTtriangles from one delivered edge.
        /// </summary>
        /// <param name="edge">A Line representing a Mesh edge.</param>
        /// <returns>
        /// A List of Vector3 points or null if the supplied Line does not represent a Mesh edge.
        /// </returns>
        public static List<Vector3> ThirdPoints(this Mesh mesh, Line edge)
        {
            if (!edge.IsListed(mesh.Edges()))
            {
                return null;
            }
            var thirdPoints = new List<Vector3>();
            foreach (var triangle in mesh.Triangles)
            {
                var points = triangle.Points(); ;
                if (edge.Start.IsListed(points) && edge.End.IsListed(points))
                {
                    thirdPoints.AddRange(points.Where(p => !edge.Start.IsAlmostEqualTo(p) && !edge.End.IsAlmostEqualTo(p)));
                }
            }
            return thirdPoints;
        }

        public struct Vertex
        {
            public int index;
            public bool isBoundary;
            public Vector3 position;
        }

        public struct IndexedVertices
        {
            public List<List<int>> triangles;
            public List<Vertex> vertices;
        }

        /// <summary>
        /// Converts the Mesh to Indexed Vertex form.
        /// NOTE: ELIMINATES HOLES IN THE MESH.
        /// TODO: PRESERVE HOLES.
        /// </summary>
        /// <returns>
        /// A List of IndexedVertices.
        /// </returns>
        public static IndexedVertices ToIndexedVertices(this Mesh mesh)
        {
            IndexedVertices indexedVertices;
            indexedVertices.triangles = new List<List<int>>();
            indexedVertices.vertices = new List<Vertex>();
            var i = 0;
            foreach (var point in mesh.PointsBoundary())
            {
                indexedVertices.vertices.Add(
                    new Vertex
                    {
                        index = i,
                        isBoundary = true,
                        position = point
                    });
                i++;
            }
            foreach (var point in mesh.PointsInterior())
            {
                indexedVertices.vertices.Add(
                    new Vertex
                    {
                        index = i,
                        isBoundary = false,
                        position = point
                    });
                i++;
            }
            foreach (var triangle in mesh.Triangles)
            {
                var indices = new List<int>();
                triangle.Vertices.ToList().ForEach(
                    p => indices.Add(indexedVertices.vertices.First(v => v.position.IsAlmostEqualTo(p.Position)).index));
                indexedVertices.triangles.Add(indices);
            }
            return indexedVertices;
        }
    }
}
