using System.Linq;
using DogHeist.EditorTools.Greybox;
using NUnit.Framework;
using UnityEngine;

namespace DogHeist.Tests.EditMode
{
    public sealed class GreyboxLayoutTests
    {
        [Test]
        public void SplitAroundGap_NoGap_ReturnsWholeEdge()
        {
            var pieces = GreyboxLayout.SplitAroundGap(-5f, 5f, 0f, 0f);

            Assert.AreEqual(1, pieces.Count);
            Assert.AreEqual((-5f, 5f), pieces[0]);
        }

        [Test]
        public void SplitAroundGap_MiddleGap_ReturnsTwoPiecesAroundGap()
        {
            var pieces = GreyboxLayout.SplitAroundGap(-5f, 5f, 1f, 2f);

            Assert.AreEqual(2, pieces.Count);
            Assert.AreEqual((-5f, 0f), pieces[0]);
            Assert.AreEqual((2f, 5f), pieces[1]);
        }

        [Test]
        public void SplitAroundGap_GapWiderThanEdge_ReturnsNothing()
        {
            Assert.IsEmpty(GreyboxLayout.SplitAroundGap(-1f, 1f, 0f, 10f));
        }

        [Test]
        public void FenceSegments_LeaveGateOpen()
        {
            var gate = new Vector3(GreyboxLayout.GateCenterX, 0f, GreyboxLayout.YardMinZ);

            Assert.IsFalse(GreyboxLayout.BuildFenceSegments().Any(segment => segment.ContainsXZ(gate)));
        }

        [Test]
        public void FenceSegments_LeaveHoleOpen()
        {
            var hole = new Vector3(GreyboxLayout.YardMinX, 0f, GreyboxLayout.FenceHoleCenterZ);

            Assert.IsFalse(GreyboxLayout.BuildFenceSegments().Any(segment => segment.ContainsXZ(hole)));
        }

        [TestCase(10f, -11f)]
        [TestCase(-14f, -5f)]
        [TestCase(0f, 11f)]
        [TestCase(14f, 0f)]
        public void FenceSegments_CloseYardElsewhere(float x, float z)
        {
            var point = new Vector3(x, 0f, z);

            Assert.IsTrue(GreyboxLayout.BuildFenceSegments().Any(segment => segment.ContainsXZ(point)));
        }

        [Test]
        public void ThiefSpawn_IsOutsideYardButOnGround()
        {
            Assert.IsFalse(GreyboxLayout.IsInsideYard(GreyboxLayout.ThiefSpawn));
            Assert.IsTrue(GreyboxLayout.IsOnGround(GreyboxLayout.ThiefSpawn));
        }

        [Test]
        public void EscapeZone_IsOutsideYardInFrontOfGate()
        {
            Assert.Less(GreyboxLayout.EscapeZoneCenter.z, GreyboxLayout.YardMinZ);
            var inFrontOfGate = new Vector3(GreyboxLayout.GateCenterX, 0f, GreyboxLayout.EscapeZoneCenter.z);
            Assert.IsTrue(GreyboxLayout.IsInsideBoxXZ(inFrontOfGate, GreyboxLayout.EscapeZoneCenter, GreyboxLayout.EscapeZoneSize));
        }

        [Test]
        public void KeyPoints_AreInsideYardAndNotInsideBuildings()
        {
            var points = GreyboxLayout.PatrolWaypoints
                .Append(GreyboxLayout.DogHome)
                .Append(GreyboxLayout.OwnerBed);

            foreach (var point in points)
            {
                Assert.IsTrue(GreyboxLayout.IsInsideYard(point), $"{point} nằm ngoài sân");
                Assert.IsFalse(GreyboxLayout.IsInsideBoxXZ(point, GreyboxLayout.HouseCenter, GreyboxLayout.HouseSize), $"{point} nằm trong nhà");
                Assert.IsFalse(GreyboxLayout.IsInsideBoxXZ(point, GreyboxLayout.KennelCenter, GreyboxLayout.KennelSize), $"{point} nằm trong chuồng");
            }
        }

        [Test]
        public void Bushes_AreNotLit()
        {
            foreach (var bush in GreyboxLayout.Bushes)
            {
                Assert.IsFalse(
                    GreyboxLayout.IsInsideBoxXZ(bush.Position, GreyboxLayout.LightZoneCenter, GreyboxLayout.LightZoneSize),
                    $"Bụi cây tại {bush.Position} nằm trong vùng sáng nên không bao giờ ẩn được");
            }
        }

        [Test]
        public void ThiefSpawnAndDogHome_AreNotLit()
        {
            Assert.IsFalse(GreyboxLayout.IsInsideBoxXZ(GreyboxLayout.ThiefSpawn, GreyboxLayout.LightZoneCenter, GreyboxLayout.LightZoneSize));
            Assert.IsFalse(GreyboxLayout.IsInsideBoxXZ(GreyboxLayout.DogHome, GreyboxLayout.LightZoneCenter, GreyboxLayout.LightZoneSize));
        }
    }
}
