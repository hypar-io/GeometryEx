using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using SkeletonNet;
using SkeletonNet.Primitives;

namespace GeometryEx
{
    public static class PolygonEx
    {
        /// <summary>
        /// The ratio of the longer side to the shorter side of the Polygon's bounding box.
        /// </summary>
        public static double AlignedAspectRatio(this Polygon polygon)
        {
            var segments = polygon.AlignedBox().Segments().OrderBy(s => s.Length());
            return segments.Last().Length() / segments.First().Length();
        }

        /// <summary>
        /// Returns a Polygon bounding box rotated to the same angle as the longest segment of this Polygon.
        /// </summary>
        public static Polygon AlignedBox(this Polygon polygon)
        {
            var ang = polygon.Segments().OrderByDescending(s => s.Length()).ToList().First();
            var angle = Math.Atan2(ang.End.Y - ang.Start.Y, ang.End.X - ang.Start.X) * (180 / Math.PI);
            var perimeterJig = polygon.Rotate(Vector3.Origin, angle * -1);
            return perimeterJig.Compass().Box.Rotate(Vector3.Origin, angle);
        }

        /// <summary>
        /// Returns a list of Vector3 points representing the corners of the Polygon's aligned bounding box.
        /// </summary>
        public static List<Vector3> AlignedBoxCorners(this Polygon polygon)
        {
            return polygon.AlignedBox().Vertices.ToList();
        }

        /// <summary>
        /// The ratio of the longer side to the shorter side of the Polygon's orthogonal bounding box.
        /// </summary>
        public static double AspectRatio(this Polygon polygon)
        {
            var segments = polygon.Compass().Box.Segments().OrderBy(s => s.Length());
            return segments.Last().Length() / segments.First().Length();
        }

        /// <summary>
        /// Returns a list of Vector3 points representing the corners of the Polygon's orthogonal bounding box.
        /// </summary>
        public static List<Vector3> BoxCorners(this Polygon polygon)
        {
            var box = new CompassBox(polygon);
            return new List<Vector3>
            {
                box.SW,
                box.SE,
                box.NE,
                box.NW
            };
        }

        /// <summary>
        /// Returns the List of Polygons that can merge with this Polygon.
        /// </summary>
        /// <param name="polygons">List of Polygons to test.</param>
        /// <returns>A List of Polygons that can be merged with this Polygon.</returns>
        public static List<Polygon> CanMerge(this Polygon polygon, List<Polygon> polygons)
        {
            var mrgPolygons = new List<Polygon>();
            foreach (var poly in polygons)
            {
                var polys = polygon.ToList();
                polys.Add(poly);
                if (Shaper.Merge(polys).Count == 1)
                {
                    mrgPolygons.Add(poly);
                }
            }
            return mrgPolygons;
        }

        /// <summary>
        /// Returns a CompassBox representation of the Polygon's bounding box.
        /// </summary>
        public static CompassBox Compass(this Polygon polygon)
        {
            return new CompassBox(polygon);
        }

        /// <summary>
        /// Attempts to scale up a Polygon until coming within the tolerance percentage of the target area.
        /// </summary>
        /// <param name="polygon">Polygon to scale.</param>
        /// <param name="area">Target area of the Polygon.</param>
        /// <param name="tolerance">Area total tolerance.</param>
        /// <param name="origin">Alignment location for final Polygon.</param>
        /// <param name="within">Polygon acting as a constraining outer boundary.</param>
        /// <param name="among">List of Polygons to avoid intersecting.</param>
        /// <returns>
        /// A new Polygon.
        /// </returns>
        public static Polygon ExpandToArea(this Polygon polygon, double area, double ratio,
                                           double tolerance = 0.1, Orient origin = Orient.C,
                                           Polygon within = null, List<Polygon> among = null)
        {
            if (polygon.IsClockWise())
            {
                polygon = polygon.Reversed();
            }
            if (Math.Abs(polygon.Area() - area) <= Math.Abs(tolerance * area))
            {
                return polygon;
            }
            var position = polygon.Compass().PointBy(origin);
            Polygon tryPoly = Shaper.RectangleByArea(area, ratio);
            tryPoly = tryPoly.MoveFromTo(tryPoly.Compass().PointBy(origin), position);
            double tryArea = tryPoly.Area();
            do
            {
                var t = new Transform();
                t.Scale(tryArea / area, tryPoly.Compass().PointBy(origin));
                tryPoly = tryPoly.TransformedPolygon(t);
                if (within != null && tryPoly.Intersects(within))
                {
                    var tryPolys = within.Intersection(tryPoly);
                    if (tryPolys != null && tryPolys.Count > 0)
                    {
                        tryPoly = tryPolys.First();
                    }
                }
                if (among != null && tryPoly.Intersects(among))
                {
                    var tryPolys = Shaper.Differences(tryPoly.ToList(), among);
                    if (tryPolys != null && tryPolys.Count > 0)
                    {
                        tryPoly = tryPolys.First();
                    }
                }
                tryArea = tryPoly.Area();
            }
            while (!Shaper.NearEqual(tryPoly.Area(), area, tolerance * area) &&
                   !Shaper.NearEqual(tryPoly.Area(), tryArea, tolerance));
            return tryPoly;
        }

        /// <summary>
        /// Provides a list of Vector3 points within a Polygon by searching along Lines between Polygon vertices and the Polygon's Centroid.
        /// </summary>
        /// <param name="interval">Density of Vector points to return along each compass radial.</param>
        /// <returns>A List of Vector3 points.</returns>
        public static List<Vector3> FindInternalPoints(this Polygon polygon, double interval = 1.0)
        {
            var centroid = polygon.Centroid();
            var distPoints = polygon.Vertices.ToList().OrderByDescending(v => v.DistanceTo(centroid));
            var segments = polygon.Segments();
            var intPoints = new List<Vector3>();
            foreach (var point in distPoints)
            {
                var ray = new Line(centroid, point);
                var lines = ray.DivideByLength(interval);
                foreach (var line in lines)
                {
                    foreach (var segment in segments)
                    {
                        if (ray.Intersects2D(segment))
                        {
                            if (polygon.Contains(line.Start))
                            {
                                intPoints.Add(line.Start);
                            }
                            if (polygon.Contains(line.End))
                            {
                                intPoints.Add(line.End);
                            }
                        }
                    }
                }
            }
            return intPoints;
        }

        /// <summary>
        /// Tests whether a Polygon is covered by a Polygon perimeter and doesn't intersect with a list of Polygons.
        /// </summary>
        /// <param name="within">Polygon perimeter to test.</param>
        /// <param name="among">List of Polygons to check for intersection.</param>
        /// <returns>
        /// True if the Polygon is covered by the Polygon within and does not intersect with any Polygon in the supplied among list.
        /// </returns>
        public static bool Fits(this Polygon polygon, Polygon within = null, List<Polygon> among = null)
        {
            if (within != null && !within.Covers(polygon))
            {
                return false;
            }
            return !polygon.Intersects(among);
        }

        /// <summary>
        /// Creates the largest Polygon fitted to supplied intersecting Polygons.
        /// </summary>
        /// <param name="among">List of Polygons against which this Polygon must conform.</param>
        /// <returns>
        /// A new Polygon.
        /// </returns>
        public static Polygon FitAmong(this Polygon polygon, List<Polygon> among)
        {
            if (among == null || among.Count == 0)
            {
                return polygon;
            }
            var polygons = Shaper.Differences(polygon.ToList(), among);
            if (polygons.Count == 0)
            {
                return null;
            }
            polygon = polygons.OrderByDescending(p => Math.Abs(p.Area())).First();
            if (polygon.IsClockWise())
            {
                return polygon.Reversed();
            }
            return polygon;
        }

        /// <summary>
        /// Creates the largest Polygon fitted within a supplied intersecting perimeter.
        /// </summary>
        /// <param name="within">Polygon acting as a constraining outer boundary.</param>
        /// <returns>
        /// A new Polygon.
        /// </returns>
        public static Polygon FitMost(this Polygon polygon, Polygon within)
        {
            if (!within.Intersects(polygon))
            {
                return null;
            }
            var polygons = within.Intersection(polygon);
            if (polygons.Count == 0)
            {
                return null;
            }
            polygons = polygons.OrderByDescending(p => Math.Abs(p.Area())).ToList();
            if (polygons.First().IsClockWise())
            {
                return polygons.First().Reversed();
            }
            return polygons.First();
        }

        /// <summary>
        /// Creates the largest Polygon fitted within a supplied perimeter and conforming to supplied intersecting Polygons.
        /// </summary>
        /// <param name="within">Polygon acting as a constraining outer boundary.</param>
        /// <param name="among">List of Polygons against which this Polygon must conform.</param>
        /// <returns>
        /// A List of Polygons.
        /// </returns>
        public static Polygon FitTo(this Polygon polygon, Polygon within, List<Polygon> among)
        {
            polygon = FitMost(polygon, within);
            if (polygon == null)
            {
                return null;
            }
            return FitAmong(polygon, among);
        }

        /// <summary>
        /// Tests if any of the supplied Polygons share one or more areas with this Polygon when compared on a shared plane.
        /// </summary>
        /// <param name="polygons">List of Polygons to compare with this Polygon.</param>
        /// <returns>
        /// Returns true if any of the supplied Polygons share one or more areas with this Polygon when compared on a shared plane or if the list of supplied Polygons is null. Returns false if the none of the supplied Polygons share an area with this Polygon or if the supplied list of Polygons is null.
        /// </returns>
        public static bool Intersects(this Polygon polygon, List<Polygon> polygons)
        {
            if (polygons == null)
            {
                return false;
            }
            foreach (Polygon poly in polygons)
            {
                if (polygon.Intersects(poly))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns a List of Polygons derived from the Polygon's straight skeleton.
        /// </summary>
        /// <returns>A List of Polygons.</Line></returns>
        public static List<Polygon> Jigsaw(this Polygon polygon)
        {
            var vertices2d = new List<Vector2d>();
            foreach (var vertex in polygon.Vertices)
            {
                vertices2d.Add(new Vector2d(vertex.X, vertex.Y));
            }
            var skeleton = SkeletonBuilder.Build(vertices2d);
            var polygons = new List<Polygon>();
            foreach (var edgeResult in skeleton.Edges)
            {
                var vertices = new List<Vector3>();
                foreach (var vertex in edgeResult.Polygon)
                {
                    vertices.Add(new Vector3(vertex.X, vertex.Y, 0.0));
                }
                polygons.Add(Shaper.MakePolygon(vertices));
            }
            return polygons.OrderBy(p => p.Centroid()).ToList();
        }

        /// <summary>
        /// Returns a new Polygon displaced along a 2D vector calculated between the supplied Vector3 points.
        /// </summary>
        /// <param name="from">Vector3 base point of the move.</param>
        /// <param name="to">Vector3 target point of the move.</param>
        /// <returns>
        /// A new Polygon.
        /// </returns>
        public static Polygon MoveFromTo(this Polygon polygon, Vector3 from, Vector3 to)
        {
            var t = new Transform(new Vector3(to.X - from.X, to.Y - from.Y, to.Z - from.Z));
            return polygon.TransformedPolygon(t);
        }

        /// <summary>
        /// Returns a new Polygon placed in a spatial relationship with a supplied polygon by using supplied Orient points derived from Polygon.Compass bounding box points on each Polygon.
        /// </summary>
        /// <param name="adjTo">Reference Polygon in relation to which to place This Polygon.</param>
        /// <param name="from">Orient value indicating the point on This Polygon to use a MoveFromTo 'from' value.</param>
        /// <param name="to">Orient value indicating the point on the reference Polygon as a MoveFromTo 'to' value.</param>
        /// <returns>A new Polygon.</returns>
        public static Polygon PlaceNear(this Polygon polygon, Polygon adjTo, Orient from, Orient to)
        {
            var thisCompass = polygon.Compass();
            var adjCompass = adjTo.Compass();
            return polygon.MoveFromTo(thisCompass.PointBy(from), adjCompass.PointBy(to));
        }

        /// <summary>
        /// Creates a new CCW Polygon of the same vertices with the start point now at the specified Polygon vertex.
        /// </summary>
        /// <param name="startFrom">The Vector3 point from which to start the new Polygon.</param>
        /// <returns>A new Polygon.</returns>
        public static Polygon RewindFrom(this Polygon polygon, Vector3 startFrom)
        {
            var vertices = polygon.Vertices;
            if (!vertices.Contains(startFrom))
            {
                return null;
            }
            var points = new List<Vector3>();
            var start = vertices.IndexOf(startFrom);
            points.Add(vertices[start]);
            var i = start + 1;
            while (i < vertices.Count)
            {
                points.Add(vertices[i]);
                i++;
            }
            i = 0;
            while (i < start)
            {
                points.Add(vertices[i]);
                i++;
            }
            return new Polygon(points);
        }

        /// <summary>
        /// Creates a new CCW Polygon of the same vertices with the start point now at the indexed Polygon vertex.
        /// </summary>
        /// <param name="start">The index of the point from which to start the new Polygon.</param>
        /// <returns>A new Polygon.</returns>
        public static Polygon RewindFrom(this Polygon polygon, int start)
        {
            var vertices = polygon.Vertices;
            if (start < 0 || start > vertices.Count - 1)
            {
                return null;
            }
            var points = new List<Vector3>()
            {
                vertices[start]
            };
            var i = start + 1;
            while (i < vertices.Count)
            {
                points.Add(vertices[i % vertices.Count]);
                i++;
            }
            return Shaper.MakePolygon(points);
        }

        /// <summary>
        /// Returns a List of Lines forming the Polygon skeleton's connections between Polygon's spine and vertices.
        /// </summary>
        /// <returns>A List of Lines.</Line></returns>
        public static List<Line> Ribs(this Polygon polygon)
        {
            var skeleton = polygon.Skeleton();
            var lines = new List<Line>();
            foreach (var line in skeleton)
            {
                if (polygon.Contains(line.Start) ^ polygon.Contains(line.End))
                {
                    lines.Add(line);
                }
            }
            return lines.OrderBy(l => l.Midpoint()).ToList();
        }

        /// <summary>
        /// Returns a new Polygon rotated around a supplied Vector3 by the specified angle in degrees.
        /// </summary>
        /// <param name="pivot">The Vector3 base point of the rotation.</param>
        /// <param name="angle">The desired rotation angle in degrees.</param>
        /// <returns>
        /// A new Polygon.
        /// </returns>
        public static Polygon Rotate(this Polygon polygon, Vector3 pivot, double angle)
        {
            var theta = angle * (Math.PI / 180);
            var vertices = new List<Vector3>();
            foreach (Vector3 vertex in polygon.Vertices)
            {
                var rX = (Math.Cos(theta) * (vertex.X - pivot.X)) - (Math.Sin(theta) * (vertex.Y - pivot.Y)) + pivot.X;
                var rY = (Math.Sin(theta) * (vertex.X - pivot.X)) + (Math.Cos(theta) * (vertex.Y - pivot.Y)) + pivot.Y;
                var rZ = vertex.Z;
                vertices.Add(new Vector3(rX, rY, rZ));
            }
            return Shaper.MakePolygon(vertices);
        }

        /// <summary>
        /// Reduces Polygon vertices.
        /// </summary>
        /// <param name="tolerance">The tolerated deviation to include a vertex.</param>
        /// <returns>A new Polyline.</returns>
        public static Polygon Simplify(this Polygon polygon, double minLength, double filletFactor = 1.0)
        {
            return Shaper.MakePolygon(Shaper.Simplify(polygon.Vertices.ToList(), minLength, filletFactor));
        }

        /// <summary>
        /// Returns a List of Lines forming the Polygon's skeleton sorted by the ascending midpoint of each Line.
        /// </summary>
        /// <returns>A List of Lines.</Line></returns>
        public static List<Line> Skeleton(this Polygon polygon)
        {
            var vertices2d = new List<Vector2d>();
            foreach (var vertex in polygon.Vertices)
            {
                vertices2d.Add(new Vector2d(vertex.X, vertex.Y));
            }
            var skeleton = SkeletonBuilder.Build(vertices2d);
            var polygons = polygon.Jigsaw();
            var lines = new List<Line>();
            foreach (var poly in polygons)
            {
                var segments = poly.Segments();
                foreach (var segment in segments)
                {
                    if ((polygon.Contains(segment.Start) || polygon.Contains(segment.End)) &&
                        !segment.IsListed(lines))
                    {
                        lines.Add(segment);
                    }
                }
            }
            return lines.OrderBy(l => l.Midpoint()).ToList();
        }

        /// <summary>
        /// Returns a List of Lines forming the Polygon's centerline sorted by the ascending midpoint of each Line.
        /// </summary>
        /// <returns>A List of Lines</Line></returns>
        public static List<Line> Spine(this Polygon polygon)
        {
            var skeleton = polygon.Skeleton();
            var lines = new List<Line>();
            foreach (var line in skeleton)
            {
                if (polygon.Contains(line.Start) && polygon.Contains(line.End))
                {
                    lines.Add(line);
                }
            }
            return lines.OrderBy(l => l.Midpoint()).ToList();
        }

        /// <summary>
        /// Creates a Polygon with a reduced quantity of points tested against a deviation tolerance.
        /// </summary>
        /// <param name="tolerance">The deviation tolerance for inclusion in the final Vector3 List.</param>
        /// <returns>
        /// A new Polygon.
        /// </returns>
        public static Polygon Straighten(this Polygon polygon, double tolerance)
        {
            return Shaper.MakePolygon(SimplifyNet.Straighten(polygon.Vertices.ToList(), tolerance));
        }

        /// <summary>
        /// Inserts this Polygon into a new List.
        /// </summary>
        /// <returns>A List containing this Polygon.</returns>
        public static List<Polygon> ToList(this Polygon polygon)
        {
            return new List<Polygon> { polygon };
        }

        /// <summary>
        /// Returns a Mesh by applying the Delauney triangulation algorithm to this Polygon.
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns>
        /// A Mesh.
        /// </returns>
        public static Mesh ToMesh(this Polygon polygon, bool top = true)
        {
            var points = new List<IPoint>();
            polygon.Vertices.ToList().ForEach(v => points.Add(new Point(v.X, v.Y)));
            var elev = polygon.Vertices.First().Z;
            var delTriangles = new Delaunator(points.ToArray()).GetTriangles().ToList();
            var mesh = new Mesh();
            foreach (var triangle in delTriangles)
            {
                var vertices = new List<Vector3>();
                triangle.Points.ToList().ForEach(p => vertices.Add(new Vector3(p.X, p.Y, elev)));
                var winder = new Polygon(vertices);
                if (!polygon.Covers(winder))
                {
                    continue;
                }
                if (winder.IsClockWise() && top || !winder.IsClockWise() && !top)
                {
                    winder = winder.Reversed();
                }
                var mTriangle = new Elements.Geometry.Triangle(new Elements.Geometry.Vertex(winder.Vertices[0]),
                                                               new Elements.Geometry.Vertex(winder.Vertices[1]),
                                                               new Elements.Geometry.Vertex(winder.Vertices[2]));
                mesh.AddTriangle(mTriangle);
                mesh.AddVertex(mTriangle.Vertices[0].Position);
                mesh.AddVertex(mTriangle.Vertices[1].Position);
                mesh.AddVertex(mTriangle.Vertices[2].Position);
            }
            mesh.ComputeNormals();
            return mesh;
        }
    }
}
