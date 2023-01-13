using System;
using geokassa;
using gridfiles;
using Xunit;

namespace geokassatest
{
    public class GeoTiffTests
    {
        /// <Summary>
        /// Input: X = 3172870.23825d, Y = 604208.66255d, Z = 5481574.62046d
        /// Output: Lamda = 10.78172626272929868207, Phi = 59.66033766924358161532, H = 133.59799729101359844208
        /// </Summary>
        [Fact]
       
        public void TransformCommonPointToLLH()
        {
            var p = new CommonPointXYZ
            {
                X_Source = 3172870.23825d,
                Y_Source = 604208.66255d,
                Z_Source = 5481574.62046d
            };

            Assert.Equal(10.78172626272929868207d, p.Lambda_SourceDeg, 14);
            Assert.Equal(59.66033766924358161532d, p.Phi_SourceDeg, 14);
            Assert.Equal(133.59799729101359844208d, p.H_Source, 6);
        }

        /// <Summary>
        /// Input: Lamda = 10.78172626272929868207, Phi = 59.66033766924358161532, H = 133.59799729101359844208
        /// Output: X = 3172870.23825d, Y = 604208.66255d, Z = 5481574.62046d
        /// </Summary>
        [Fact]
        public void TransformCommonPointToXYZ()
        {
            var p = new CommonPointXYZ
            {
                Lambda_SourceDeg = 10.78172626272929868207d,
                Phi_SourceDeg = 59.66033766924358161532d,
                H_Source = 133.59799729101359844208d
            };

            Assert.Equal(3172870.23825d, p.X_Source, 5);
            Assert.Equal(604208.66255d, p.Y_Source, 5);
            Assert.Equal(5481574.62046d, p.Z_Source, 5);
        }

        [Fact]
        public void Test7ParamHelmert()
        {
            // TODO: Replace with closer points
            var cps = new CommonPointSet();
            cps.PointList.Add(new CommonPointXYZ()
            {
                PointName = "P1",
                X_Source = 3138260.91460d, Y_Source = 293529.56074d, Z_Source = 5526461.89476d,
                X_Target = 3138261.4077d, Y_Target = 293529.1950d, Z_Target = 5526461.5566d
            });
            cps.PointList.Add(new CommonPointXYZ()
            {
                PointName = "P2",
                X_Source = 3143949.72043d, Y_Source = 367015.67869d, Z_Source = 5518814.23919d,
                X_Target = 3143950.2112d, Y_Target = 367015.3120d, Z_Target = 5518813.8886d
            });
            cps.PointList.Add(new CommonPointXYZ()
            {
                PointName = "P3",
                X_Source = 3131965.49862d, Y_Source = 403032.06854d, Z_Source = 5523941.73158d,
                X_Target = 3131965.9862d, Y_Target = 403031.7041d, Z_Target = 5523941.3731d
            });
            cps.PointList.Add(new CommonPointXYZ()
            {
                PointName = "P4",
                X_Source = 3143234.46880d, Y_Source = 338308.62192d, Z_Source = 5521033.59735d,
                X_Target = 3143234.9625d, Y_Target = 338308.2564d, Z_Target = 5521033.2535d
            });
            cps.PointList.Add(new CommonPointXYZ()
            {
                PointName = "P5",
                X_Source = 3181051.59622d, Y_Source = 335027.81022d, Z_Source = 5499661.60383d,
                X_Target = 3181052.0882d, Y_Target = 335027.4393d, Z_Target = 5499661.2612d
            });
            cps.PointList.Add(new CommonPointXYZ()
            {
                PointName = "P6",
                X_Source = 3116663.05249d, Y_Source = 350851.90199d, Z_Source = 5535247.35347d,
                X_Target = 3116663.5447d, Y_Target = 350851.5402d, Z_Target = 5535247.0068d
            });
            var result = cps.Helmert(0.1d, 100000d, 0.05d);

            Assert.True(result);
            Assert.Equal(-0.041216971276516953, cps.Rx.ToArcSec(), 8);
            Assert.Equal(-0.0037183533303600906, cps.Ry.ToArcSec(), 8);
            Assert.Equal(0.00550760219295924, cps.Rz.ToArcSec(), 8);
            Assert.Equal(0.57681536981246218, cps.Tx, 4);
            Assert.Equal(-1.5556197770016149, cps.Ty, 4);
            Assert.Equal(-0.37464289347149926, cps.Tz, 4);
            Assert.Equal(0.0073245487453021951, cps.S.ToPpm(), 8);
        }

        [Fact]
        public void Test4ParamHelmert()
        {
            var helmert = new Ct2File();

            helmert.PointList.Add(new CommonPointXYZ()
            {
                PointName = "P1",
                Lambda_SourceDeg = 9.05267d, Phi_SourceDeg = 62.46513d, H_Source = 0d,
                Lambda_TargetDeg = 11.34994d, Phi_TargetDeg = 63.93045d, H_Target = 0.1d
            });
            helmert.PointList.Add(new CommonPointXYZ()
            {
                PointName = "P2",
                Lambda_SourceDeg = 11.99644d, Phi_SourceDeg = 53.86300d, H_Source = 0d,
                Lambda_TargetDeg = 14.11489d, Phi_TargetDeg = 54.98329d, H_Target = 0.2d
            });
            helmert.PointList.Add(new CommonPointXYZ()
            {
                PointName = "P3",
                Lambda_SourceDeg = 11.02224d, Phi_SourceDeg = 63.07495d, H_Source = 0d,
                Lambda_TargetDeg = 13.39652d, Phi_TargetDeg = 64.49743d, H_Target = -0.1d
            });

            helmert.PointList.Add(new CommonPointXYZ()
            {
                PointName = "P4",
                Lambda_SourceDeg = 13.83649d, Phi_SourceDeg = 64.28125d, H_Source = 0d,
                Lambda_TargetDeg = 16.33116d, Phi_TargetDeg = 65.65242d, H_Target = 0d
            });
            var result = helmert.Helmert(0.00001d, 100000d, 0.001d);

            Assert.True(result);

            Assert.Equal(1.0295493546159107d, helmert.Apar, 10);
            Assert.Equal(0.016956239704441076d, helmert.Bpar, 10);
            Assert.Equal(-0.0694617213710258d, helmert.Tx, 6);
            Assert.Equal(-0.36188548811447446d, helmert.Ty, 6);

            // Debug.WriteLine($"a: {helmert.Apar } b: {helmert.Bpar} tx: {helmert.Tx} ty; {helmert.Ty}");           
        }
    }
}
