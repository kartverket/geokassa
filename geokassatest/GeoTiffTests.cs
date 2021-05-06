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
                X1 = 3172870.23825d,
                Y1 = 604208.66255d,
                Z1 = 5481574.62046d
            };

            Assert.Equal(10.78172626272929868207d, p.Lambda1Deg, 14);
            Assert.Equal(59.66033766924358161532d, p.Phi1Deg, 14);
            Assert.Equal(133.59799729101359844208d, p.H1, 6);
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
                Lambda1Deg = 10.78172626272929868207d,
                Phi1Deg = 59.66033766924358161532d,
                H1 = 133.59799729101359844208d
            };

            Assert.Equal(3172870.23825d, p.X1, 5);
            Assert.Equal(604208.66255d, p.Y1, 5);
            Assert.Equal(5481574.62046d, p.Z1, 5);
        }

        [Fact]
        public void Test7ParamHelmert()
        {
            // TODO: Replace with closer points
            var cps = new CommonPointSet();
            cps.PointList.Add(new CommonPointXYZ()
            {
                Name = "P1",
                X1 = 3138260.91460d, Y1 = 293529.56074d, Z1 = 5526461.89476d,
                X2 = 3138261.4077d, Y2 = 293529.1950d, Z2 = 5526461.5566d
            });
            cps.PointList.Add(new CommonPointXYZ()
            {
                Name = "P2",
                X1 = 3143949.72043d, Y1 = 367015.67869d, Z1 = 5518814.23919d,
                X2 = 3143950.2112d, Y2 = 367015.3120d, Z2 = 5518813.8886d
            });
            cps.PointList.Add(new CommonPointXYZ()
            {
                Name = "P3",
                X1 = 3131965.49862d, Y1 = 403032.06854d, Z1 = 5523941.73158d,
                X2 = 3131965.9862d, Y2 = 403031.7041d, Z2 = 5523941.3731d
            });
            cps.PointList.Add(new CommonPointXYZ()
            {
                Name = "P4",
                X1 = 3143234.46880d, Y1 = 338308.62192d, Z1 = 5521033.59735d,
                X2 = 3143234.9625d, Y2 = 338308.2564d, Z2 = 5521033.2535d
            });
            cps.PointList.Add(new CommonPointXYZ()
            {
                Name = "P5",
                X1 = 3181051.59622d, Y1 = 335027.81022d, Z1 = 5499661.60383d,
                X2 = 3181052.0882d, Y2 = 335027.4393d, Z2 = 5499661.2612d
            });
            cps.PointList.Add(new CommonPointXYZ()
            {
                Name = "P6",
                X1 = 3116663.05249d, Y1 = 350851.90199d, Z1 = 5535247.35347d,
                X2 = 3116663.5447d, Y2 = 350851.5402d, Z2 = 5535247.0068d
            });
            var result = cps.Helmert(0.1d, 100000d, 0.05d);

            Assert.True(result);
            Assert.Equal(-0.041207201818965096d, cps.Rx.ToArcSec(), 8);
            Assert.Equal(-0.0036962766589199079, cps.Ry.ToArcSec(), 8);
            Assert.Equal(0.0054642511968749623, cps.Rz.ToArcSec(), 8);
            Assert.Equal(0.576253823189717d, cps.Tx, 4);
            Assert.Equal(-1.5546849330570782, cps.Ty, 4);
            Assert.Equal(-0.37411346051971311d, cps.Tz, 4);
            Assert.Equal(0.0072842152309959829d, cps.S.ToPpm(), 8);
        }

        [Fact]
        public void Test4ParamHelmert()
        {
            var helmert = new Ct2File();

            helmert.PointList.Add(new CommonPointXYZ()
            {
                Name = "P1",
                Lambda1Deg = 9.05267d, Phi1Deg = 62.46513d, H1 = 0d,
                Lambda2Deg = 11.34994d, Phi2Deg = 63.93045d, H2 = 0.1d
            });
            helmert.PointList.Add(new CommonPointXYZ()
            {
                Name = "P2",
                Lambda1Deg = 11.99644d, Phi1Deg = 53.86300d, H1 = 0d,
                Lambda2Deg = 14.11489d, Phi2Deg = 54.98329d, H2 = 0.2d
            });
            helmert.PointList.Add(new CommonPointXYZ()
            {
                Name = "P3",
                Lambda1Deg = 11.02224d, Phi1Deg = 63.07495d, H1 = 0d,
                Lambda2Deg = 13.39652d, Phi2Deg = 64.49743d, H2 = -0.1d
            });

            helmert.PointList.Add(new CommonPointXYZ()
            {
                Name = "P4",
                Lambda1Deg = 13.83649d, Phi1Deg = 64.28125d, H1 = 0d,
                Lambda2Deg = 16.33116d, Phi2Deg = 65.65242d, H2 = 0d
            });
            var result = helmert.Helmert(0.00001d, 100000d, 0.001d);

            Assert.True(result);

            Assert.Equal(1.029648449917d, helmert.Apar, 10);
            Assert.Equal(0.016941945629d, helmert.Bpar, 10);
            Assert.Equal(-0.06932768d, helmert.Tx, 6);
            Assert.Equal(-0.36724001d, helmert.Ty, 6);

            //a: 1.02964844991704 b: 0.0169419456285466 tx: -0.0693276806223762 ty; -0.367240005934891
           // Debug.WriteLine($"a: {helmert.Apar } b: {helmert.Bpar} tx: {helmert.Tx} ty; {helmert.Ty}");           
        }
    }
}
