using System.Collections.Generic;
using UnityEngine;

namespace DogHeist.EditorTools.Greybox
{
    /// <summary>
    /// Tọa độ màn greybox Level01, đo từ ban_do_man_choi_trom_cho.png (khoảng 50 px = 1 m).
    /// Quy ước: +X là phía đông (bên phải ảnh), +Z là phía bắc (phía trên ảnh), mặt đất ở y = 0.
    /// Chỉ chứa số liệu và phép tính thuần nên test được mà không cần scene.
    /// </summary>
    internal static class GreyboxLayout
    {
        public const float YardMinX = -14f;
        public const float YardMaxX = 14f;
        public const float YardMinZ = -11f;
        public const float YardMaxZ = 11f;

        public const float FenceHeight = 1.5f;
        public const float FenceThickness = 0.2f;

        // Cổng ở hàng rào phía nam, lỗ hổng ở hàng rào phía tây.
        public const float GateCenterX = -1f;
        public const float GateWidth = 3f;
        public const float FenceHoleCenterZ = 3.4f;
        public const float FenceHoleWidth = 2f;

        public const float GroundSize = 60f;
        public const float BoundaryHeight = 3f;

        public static readonly Vector3 ThiefSpawn = new Vector3(-22f, 0f, 3.4f);

        public static readonly Vector3 HouseCenter = new Vector3(6.9f, 2f, 6.2f);
        public static readonly Vector3 HouseSize = new Vector3(11.2f, 4f, 7.2f);

        public static readonly Vector3 KennelCenter = new Vector3(-8.5f, 0.6f, -4.8f);
        public static readonly Vector3 KennelSize = new Vector3(3f, 1.2f, 2f);

        public static readonly Vector3 DogHome = new Vector3(-8.5f, 0f, -2.8f);

        // Đủ gần chuồng để tiếng sủa (16 m x độ thính lúc ngủ 0.6) đánh thức được chủ nhà.
        public static readonly Vector3 OwnerBed = new Vector3(-0.5f, 0f, 1.6f);

        public static readonly Vector3 PorchLightPosition = new Vector3(1.3f, 3.2f, 1.3f);
        public static readonly Vector3 PorchLightTarget = new Vector3(-2f, 0f, -5f);

        public static readonly Vector3 LightZoneCenter = new Vector3(-1.5f, 1f, -3.5f);
        public static readonly Vector3 LightZoneSize = new Vector3(6f, 2f, 9f);

        public static readonly Vector3 EscapeZoneCenter = new Vector3(-3f, 1f, -14.5f);
        public static readonly Vector3 EscapeZoneSize = new Vector3(34f, 2f, 3f);

        public static readonly BushSpec[] Bushes =
        {
            new BushSpec(new Vector3(-10.5f, 0f, 7.8f), 2.1f),
            new BushSpec(new Vector3(-8.4f, 0f, 8.5f), 1.8f),
            new BushSpec(new Vector3(11.3f, 0f, -2.5f), 1.9f),
            new BushSpec(new Vector3(11.6f, 0f, -4.6f), 1.6f)
        };

        public static readonly Vector3[] PatrolWaypoints =
        {
            new Vector3(8.9f, 0f, 1.3f),
            new Vector3(8.9f, 0f, -7.8f),
            new Vector3(-4.5f, 0f, -7.8f),
            new Vector3(-6f, 0f, 5f)
        };

        public static IReadOnlyList<FenceSegment> BuildFenceSegments()
        {
            var segments = new List<FenceSegment>();
            AddEdgeAlongX(segments, YardMaxZ, 0f, 0f);
            AddEdgeAlongX(segments, YardMinZ, GateCenterX, GateWidth);
            AddEdgeAlongZ(segments, YardMaxX, 0f, 0f);
            AddEdgeAlongZ(segments, YardMinX, FenceHoleCenterZ, FenceHoleWidth);
            return segments;
        }

        /// <summary>Chia đoạn [min, max] thành các phần nằm ngoài khoảng trống. gapWidth = 0 nghĩa là không có khoảng trống.</summary>
        public static List<(float From, float To)> SplitAroundGap(float min, float max, float gapCenter, float gapWidth)
        {
            var pieces = new List<(float From, float To)>();
            if (gapWidth <= 0f)
            {
                pieces.Add((min, max));
                return pieces;
            }

            var gapStart = Mathf.Clamp(gapCenter - gapWidth * 0.5f, min, max);
            var gapEnd = Mathf.Clamp(gapCenter + gapWidth * 0.5f, min, max);

            if (gapStart > min)
            {
                pieces.Add((min, gapStart));
            }

            if (gapEnd < max)
            {
                pieces.Add((gapEnd, max));
            }

            return pieces;
        }

        public static bool IsInsideYard(Vector3 point) =>
            point.x > YardMinX && point.x < YardMaxX && point.z > YardMinZ && point.z < YardMaxZ;

        public static bool IsOnGround(Vector3 point) =>
            Mathf.Abs(point.x) < GroundSize * 0.5f && Mathf.Abs(point.z) < GroundSize * 0.5f;

        public static bool IsInsideBoxXZ(Vector3 point, Vector3 center, Vector3 size) =>
            Mathf.Abs(point.x - center.x) <= size.x * 0.5f && Mathf.Abs(point.z - center.z) <= size.z * 0.5f;

        private static void AddEdgeAlongX(List<FenceSegment> segments, float z, float gapCenter, float gapWidth)
        {
            foreach (var (from, to) in SplitAroundGap(YardMinX, YardMaxX, gapCenter, gapWidth))
            {
                segments.Add(new FenceSegment(
                    new Vector3((from + to) * 0.5f, FenceHeight * 0.5f, z),
                    new Vector3(to - from, FenceHeight, FenceThickness)));
            }
        }

        private static void AddEdgeAlongZ(List<FenceSegment> segments, float x, float gapCenter, float gapWidth)
        {
            foreach (var (from, to) in SplitAroundGap(YardMinZ, YardMaxZ, gapCenter, gapWidth))
            {
                segments.Add(new FenceSegment(
                    new Vector3(x, FenceHeight * 0.5f, (from + to) * 0.5f),
                    new Vector3(FenceThickness, FenceHeight, to - from)));
            }
        }
    }
}
