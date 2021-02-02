using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using Elements.Geometry;
using Elements.Geometry.Solids;
using GeometryEx;



namespace GeometryExTests
{
    public class LineExTests
    {
        [Fact]
        public void Differences()
        {
            var line = new Line(new Vector3(-5.0, 5.0), new Vector3(15.0, 5.0));
            var polygons = new List<Polygon>
            {
                new Polygon
                (
                    new[]
                    {
                        new Vector3(0.0, 0.0),
                        new Vector3(10.0, 0.0),
                        new Vector3(10.0, 10.0),
                        new Vector3(0.0, 10.0)
                    }
                )
            };
            var lines = line.Differences(polygons);
            var points = new List<Vector3>();
            foreach (var segment in lines)
            {
                points.Add(segment.Start);
                points.Add(segment.End);
            }
            Assert.Equal(2, lines.Count);
            Assert.Contains(line.Start, points);
            Assert.Contains(new Vector3(0.0, 5.0), points);
            Assert.Contains(new Vector3(10.0, 5.0), points);
            Assert.Contains(line.End, points);
        }

        [Fact]
        public void DistanceTo()
        {
            var triangles = new List<Elements.Geometry.Triangle>
            {
                new Elements.Geometry.Triangle(new Vertex(new Vector3(2.0, 2.0, 12.0)),
                                               new Vertex(new Vector3(9.0, 2.0, 12.0)),
                                               new Vertex(new Vector3(5.0, 4.0, 10.0))),
                new Elements.Geometry.Triangle(new Vertex(new Vector3(5.0, 4.0, 10.0)),
                                               new Vertex(new Vector3(9.0, 2.0, 12.0)),
                                               new Vertex(new Vector3(5.0, 11.0, 10.0))),
                new Elements.Geometry.Triangle(new Vertex(new Vector3(5.0, 11.0, 10.0)),
                                               new Vertex(new Vector3(9.0, 2.0, 12.0)),
                                               new Vertex(new Vector3(9.0, 13.0, 12.0))),
                new Elements.Geometry.Triangle(new Vertex(new Vector3(9.0, 13.0, 12.0)),
                                               new Vertex(new Vector3(2.0, 13.0, 12.0)),
                                               new Vertex(new Vector3(5.0, 11.0, 10.0))),
                new Elements.Geometry.Triangle(new Vertex(new Vector3(2.0, 13.0, 12.0)),
                                               new Vertex(new Vector3(2.0, 2.0, 12.0)),
                                               new Vertex(new Vector3(5.0, 4.0, 10.0))),
                new Elements.Geometry.Triangle(new Vertex(new Vector3(5.0, 4.0, 10.0)),
                                               new Vertex(new Vector3(5.0, 11.0, 10.0)),
                                               new Vertex(new Vector3(2.0, 13.0, 12.0)))
            };
            var mesh = new Mesh();
            triangles.ForEach(t => mesh.AddTriangle(t));
            var edges = mesh.Edges();
            var plane = new Plane(Vector3.Origin, Vector3.ZAxis);
            var sort = edges.OrderBy(e => e.DistanceTo(new Plane(Vector3.Origin, Vector3.ZAxis))).ToList();
            Assert.Contains(new Vector3(5.0, 4.0, 10.0), sort.First().Points());
            Assert.Contains(new Vector3(5.0, 11.0, 10.0), sort.First().Points());
        }

        [Fact]
        public void Divide()
        {
            var line = new Line(Vector3.Origin, new Vector3(0.0, 150.0));
            Assert.Equal(25, line.Divide(24).Count);
        }

        [Fact]
        public void Extend()
        {
            var line = new Line(Vector3.Origin, new Vector3(5.0, 5.0));
            line = line.ExtendEnd(3.0);
            Assert.Equal(Math.Sqrt(50.0) + 3.0, line.Length());
            line = line.ExtendEnd(5.0);
            Assert.Equal(Math.Sqrt(50.0) + 8.0, line.Length(), 8);
        }

        [Fact]
        public void Intersection()
        {
            var line = new Line(new Vector3(1.0, 1.0), new Vector3(6.0, 6.0));
            var intr = new Line(new Vector3(1.0, 5.0), new Vector3(5.0, 1.0));
            var point = line.Intersection(intr);
            Assert.Equal(3.0, point.X);
            Assert.Equal(3.0, point.Y);
            line = new Line(new Vector3(1.0, 3.0), new Vector3(5.0, 7.0));
            intr = new Line(new Vector3(3.0, 1.0), new Vector3(3.0, 7.0));
            point = line.Intersection(intr);
            Assert.Equal(3.0, point.X);
            Assert.Equal(5.0, point.Y);
            line = new Line(new Vector3(2.0, 4.0), new Vector3(9.0, 4.0));
            intr = new Line(new Vector3(3.0, 1.0), new Vector3(8.0, 6.0));
            point = line.Intersection(intr);
            Assert.Equal(6.0, point.X);
            Assert.Equal(4.0, point.Y);
        }

        [Fact]
        public void IsColinearWith()
        {
            var thisLine = new Line(new Vector3(1.0, 1.0), new Vector3(6.0, 6.0));
            var thatLine = new Line(new Vector3(3.0, 3.0), new Vector3(5.0, 5.0));
            Assert.True(thatLine.IsColinearWith(thisLine));
            Assert.True(thisLine.IsColinearWith(thisLine));

            thisLine = new Line(new Vector3(1.0, 1.0, 1.0), new Vector3(5.0, 5.0, 5.0));
            thatLine = new Line(new Vector3(3.0, 3.0, 3.0), new Vector3(8.0, 8.0, 8.0));
            Assert.True(thatLine.IsColinearWith(thisLine, true));

            thisLine = new Line(new Vector3(1.0, 1.0), new Vector3(7.0, 6.0));
            thatLine = new Line(new Vector3(3.0, 3.0), new Vector3(5.0, 5.0));
            Assert.False(thisLine.IsColinearWith(thatLine));
        }

        [Fact]
        public void IsEqual2D()
        {
            var line = new Line(new Vector3(1.0, 1.0, 5.0), new Vector3(6.0, 6.0, 4.0));
            var thatLine = new Line(new Vector3(6.0, 6.0, 7.0), new Vector3(1.0, 1.0, 9.0));
            Assert.True(line.IsEqual2D(thatLine));
            thatLine = new Line(new Vector3(7.0, 6.0, 0.0), new Vector3(1.0, 1.0, 6.0));
            Assert.False(line.IsEqual2D(thatLine));
        }

        [Fact]
        public void IsEqualTo()
        {
            var line = new Line(new Vector3(1.0, 1.0), new Vector3(6.0, 6.0));
            var thatLine = new Line(new Vector3(6.0, 6.0), new Vector3(1.0, 1.0));
            Assert.True(line.IsEqualTo(thatLine));
            thatLine = new Line(new Vector3(7.0, 6.0), new Vector3(1.0, 1.0));
            Assert.False(line.IsEqualTo(thatLine));
        }

        [Fact]
        public void IsHorizontal()
        {
            var line = new Line(new Vector3(1.0, 1.0), new Vector3(6.0, 6.0));
            Assert.False(line.IsHorizontal());

            line = new Line(new Vector3(1.0, 1.0), new Vector3(6.0, 1.0));
            Assert.True(line.IsHorizontal());
        }

        [Fact]
        public void IsListed()
        {
            var line = new Line(new Vector3(1.0, 1.0), new Vector3(6.0, 6.0));
            var lines = new List<Line>
            {
                new Line (new Vector3(5.0, 6.0), new Vector3(2.0, 1.0)),
                new Line (new Vector3(7.0, 6.0), new Vector3(8.0, 1.0)),
                new Line (new Vector3(6.0, 6.0), new Vector3(1.0, 1.0))
            };
            Assert.True(line.IsListed(lines));
        }

        [Fact]
        public void IsParallelTo()
        {
            var line = new Line(new Vector3(6.0, 6.0), new Vector3(1.0, 1.0));
            var intr = new Line(new Vector3(5.0, 5.0), new Vector3(8.0, 8.0));
            Assert.True(line.IsParallelTo(intr));

            line = new Line(new Vector3(1.0, 1.0), new Vector3(6.0, 6.0));
            intr = new Line(new Vector3(5.0, 5.0), new Vector3(8.0, 9.0));
            Assert.False(line.IsParallelTo(intr));
        }

        [Fact]
        public void IsPerpendicularTo()
        {
            var line = new Line(new Vector3(1.0, 1.0), new Vector3(6.0, 1.0));
            var intr = new Line(new Vector3(5.0, 5.0), new Vector3(5.0, 1.0));
            Assert.True(line.IsPerpendicularTo(intr));

            line = new Line(new Vector3(1.0, 1.0), new Vector3(6.0, 6.0));
            intr = new Line(new Vector3(5.0, 5.0), new Vector3(8.0, 9.0));
            Assert.False(line.IsPerpendicularTo(intr));
        }

        [Fact]
        public void IsVertical()
        {
            var line = new Line(new Vector3(1.0, 1.0), new Vector3(6.0, 6.0));
            Assert.False(line.IsVertical());

            line = new Line(new Vector3(1.0, 1.0), new Vector3(1.0, 5.0));
            Assert.True(line.IsVertical());
        }

        [Fact]
        public void JoinTo()
        {
            var line = new Line(new Vector3(1.0, 1.0), new Vector3(6.0, 6.0));
            var join = new Line(new Vector3(1.0, 1.0), new Vector3(-5.0, -5.0));
            line = line.JoinTo(join);
            Assert.Equal(6.0, line.Start.X);
            Assert.Equal(6.0, line.Start.Y);
            Assert.Equal(-5.0, line.End.X);
            Assert.Equal(-5.0, line.End.Y);

            line = new Line(new Vector3(6.0, 6.0), new Vector3(1.0, 1.0));
            join = new Line(new Vector3(1.0, 1.0), new Vector3(-5.0, -5.0));
            line = line.JoinTo(join);
            Assert.Equal(6.0, line.Start.X);
            Assert.Equal(6.0, line.Start.Y);
            Assert.Equal(-5.0, line.End.X);
            Assert.Equal(-5.0, line.End.Y);

            line = new Line(new Vector3(6.0, 6.0), new Vector3(1.0, 1.0));
            join = new Line(new Vector3(-5.0, -5.0), new Vector3(1.0, 1.0));
            line = line.JoinTo(join);
            Assert.Equal(6.0, line.Start.X);
            Assert.Equal(6.0, line.Start.Y);
            Assert.Equal(-5.0, line.End.X);
            Assert.Equal(-5.0, line.End.Y);

            line = new Line(new Vector3(1.0, 1.0), new Vector3(6.0, 6.0));
            join = new Line(new Vector3(0.0, 0.0), new Vector3(-5.0, -5.0));
            line = line.JoinTo(join);
            Assert.Null(line);

            line = new Line(new Vector3(1.0, 1.0), new Vector3(6.0, 7.0));
            join = new Line(new Vector3(1.0, 1.0), new Vector3(-5.0, -5.0));
            line = line.JoinTo(join);
            Assert.Null(line);
        }

        [Fact]
        public void Midpoint()
        {
            var line = new Line(Vector3.Origin, new Vector3(6.0, 0.0, 4.0));
            Assert.True(new Vector3(3.0, 0.0, 2.0).IsAlmostEqualTo(line.Midpoint()));
        }

        [Fact]
        public void MoveFromTo()
        {
            var line = new Line(Vector3.Origin, new Vector3(0.0, 150));
            var moved = line.MoveFromTo(Vector3.Origin, new Vector3(0.0, 150.0));
            Assert.Equal(0.0, moved.End.X);
            Assert.Equal(300.0, moved.End.Y);
        }

        [Fact]
        public void Occurs()
        {
            var line = new Line(new Vector3(1.0, 1.0), new Vector3(6.0, 6.0));
            var lines = new List<Line>
            {
                new Line (new Vector3(5.0, 6.0), new Vector3(2.0, 1.0)),
                new Line (new Vector3(7.0, 6.0), new Vector3(8.0, 1.0)),
                new Line (new Vector3(6.0, 6.0), new Vector3(1.0, 1.0)),
                 new Line (new Vector3(1.0, 1.0), new Vector3(6.0, 6.0))
            };
            Assert.Equal(2, line.Occurs(lines));
        }

        [Fact]
        public void PerpendicularDistanceTo()
        {
            var line = new Line(Vector3.Origin, new Vector3(6.0, 0.0));
            var point = new Vector3(3.0, 4.5);
            Assert.Equal(4.5, line.PerpendicularDistanceTo(point));
            point = new Vector3(3.0, -5.5);
            Assert.Equal(5.5, line.PerpendicularDistanceTo(point));
        }

        [Fact]
        public void PointsOnLine()
        {
            var line = new Line(Vector3.Origin, new Vector3(6.0, 0.0));
            var points =
                new List<Vector3>
                {
                    new Vector3(3.0, 0.0),
                    new Vector3(3.0, 1.0)
                };
            Assert.True(line.PointsOnLine(points));
        }

        [Fact]
        public void PositionAt()
        {
            var line = new Line(Vector3.Origin, new Vector3(6.0, 0.0));
            var position = line.PositionAt(3.0);
            Assert.Equal(3.0, position.X);
            Assert.Equal(0.0, position.Y);
        }

        [Fact]
        public void Reverse()
        {
            var line = new Line(Vector3.Origin, new Vector3(5.0, 1.0));
            var revLine = line.Reverse();
            Assert.Equal(5.0, revLine.Start.X);
            Assert.Equal(1.0, revLine.Start.Y);
        }

        [Fact]
        public void Rotate()
        {
            var line = new Line(Vector3.Origin, new Vector3(5.0, 0.0));
            var rotated = line.Rotate(Vector3.Origin, 90.0);
            Assert.Equal(0.0, rotated.End.X, 10);
            Assert.Equal(5.0, rotated.End.Y, 10);
        }

        [Fact]
        public void Segment()
        {
            var line = new Line(Vector3.Origin, new Vector3(20.0, 0.0));
            var segments = line.Segment(9.0, 4.0);
            Assert.Equal(2, segments.Count);
        }

        [Fact]
        public void SegmentFrom()
        {
            var line = new Line(Vector3.Origin, new Vector3(20.0, 0.0));
            var segments = line.SegmentFrom(9.0, DivideFrom.Start);
            Assert.Equal(3, segments.Count);
        }

        [Fact]
        public void SharesEndPointWith()
        {
            var line1 = new Line(new Vector3(1.0, 1.0), new Vector3(6.0, 1.0));
            var line2 = new Line(new Vector3(6.0, 1.0), new Vector3(10.0, 15.0));
            var line3 = new Line(new Vector3(6.0, 2.0), new Vector3(5.0, 1.0));
            Assert.True(line1.SharesEndpointWith(line2));
            Assert.False(line1.SharesEndpointWith(line3));
            Assert.False(line1.SharesEndpointWith(line1));

            var lines = line1.SharesEndpointWith(new List<Line> { line1, line2, line3 });
            Assert.Single(lines);

            lines = line1.SharesEndpointWith(new List<Line> { line3 });
            Assert.Empty(lines);
        }

        [Fact]
        public void Slope()
        {
            var line = new Line(Vector3.Origin, new Vector3(20.0, 20.0));
            Assert.Equal(1.0, line.Slope());
        }
    }
}
