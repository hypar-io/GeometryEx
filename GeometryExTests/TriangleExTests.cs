using System;
using System.Collections.Generic;
using Xunit;
using Elements;
using Elements.Geometry;
using Elements.Geometry.Solids;
using GeometryEx;
using Vertex = Elements.Geometry.Vertex;

namespace GeometryExTests
{
    public class TriangleExTests
    {
        [Fact]
        public void Area()
        {
            var t =
                new Triangle(new Vertex(Vector3.Origin),
                             new Vertex(Vector3.XAxis),
                             new Vertex(Vector3.YAxis));
            Assert.True(t.Area().NearEqual(0.5));
        }

        [Fact]
        public void Centroid()
        {
            var t =
                new Triangle(new Vertex(Vector3.Origin),
                             new Vertex(new Vector3(1.0, 0.0, 1.0)),
                             new Vertex(new Vector3(0.0, 1.0, 1.0)));
            Assert.True(t.Centroid().IsAlmostEqualTo(new Vector3(0.333333, 0.333333, 0.66666667)));
        }

        [Fact]
        public void Edges()
        {
            var t1 =
                new Triangle(new Vertex(new Vector3(2.0, 2.0, 12.0)),
                             new Vertex(new Vector3(9.0, 2.0, 12.0)),
                             new Vertex(new Vector3(5.0, 4.0, 10.0)));
            var t2 =
                new Triangle(new Vertex(new Vector3(5.0, 4.0, 10.0)),
                             new Vertex(new Vector3(9.0, 2.0, 12.0)),
                             new Vertex(new Vector3(5.0, 11.0, 10.0)));
            var edges1 = t1.Edges();
            var edges2 = t2.Edges();
            Assert.Equal(3, edges1.Count);
            Assert.Equal(3, edges2.Count);
            Assert.True(new Line(new Vector3(2.0, 2.0, 12.0), new Vector3(9.0, 2.0, 12.0)).IsListed(edges1));
            Assert.True(new Line(new Vector3(2.0, 2.0, 12.0), new Vector3(5.0, 4.0, 10.0)).IsListed(edges1));
            Assert.True(new Line(new Vector3(9.0, 2.0, 12.0), new Vector3(5.0, 4.0, 10.0)).IsListed(edges2));
            Assert.True(new Line(new Vector3(9.0, 2.0, 12.0), new Vector3(5.0, 11.0, 10.0)).IsListed(edges2));
        }

        [Fact]
        public void IsLevel()
        {
            var t = new Triangle(new Vertex(Vector3.Origin),
                                 new Vertex(new Vector3(1.0, 0.0, 1.0)),
                                 new Vertex(new Vector3(0.0, 1.0, 1.0)));
            Assert.False(t.IsLevel(Vector3.ZAxis));
            t = new Triangle(new Vertex(Vector3.Origin),
                             new Vertex(new Vector3(1.0, 0.0, 0.0)),
                             new Vertex(new Vector3(0.0, 1.0, 0.0)));
            Assert.True(t.IsLevel(Vector3.ZAxis));
        }

        [Fact]
        public void Points()
        {
            var t1 =
                new Triangle(new Vertex(new Vector3(2.0, 2.0, 12.0)),
                             new Vertex(new Vector3(9.0, 2.0, 12.0)),
                             new Vertex(new Vector3(5.0, 4.0, 10.0)));
            var points = t1.Points();
            Assert.Equal(3, points.Count);
        }
    }
}
