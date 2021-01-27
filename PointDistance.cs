using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Windows.Forms;

namespace ArcMapAddin1CalPointDistance
{
    public class PointDistance : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public PointDistance()
        {
        }

        protected override void OnClick()
        {
            double TOLERANCE = 0.01;
            // -这个容差的设置一定要注意，要多次尝试.若属性表中出现-1，就加大这个值

            
            IMap pMap = ArcMap.Document.FocusMap;
            //第0层为点数据
            //第一层为线数据
            IFeatureLayer plineFtrLyr = pMap.get_Layer(1) as IFeatureLayer;
            IFeatureClass plineFtrCls = plineFtrLyr.FeatureClass;
            IFeatureLayer pointFtrLyr = pMap.get_Layer(0) as IFeatureLayer;
            IFeatureClass pointFtrCls = pointFtrLyr.FeatureClass;
           int pointCount= pointFtrCls.FeatureCount(null);
            IFeatureCursor pointCursor = pointFtrCls.Update(null, false);
            IFeature p = null;
            int index = pointFtrCls.FindField("LC");
            while ((p = pointCursor.NextFeature()) != null)
            {
                ITopologicalOperator pTo = p.Shape as ITopologicalOperator;
                IPoint pt = p.Shape as IPoint;
                ISpatialFilter psf = new SpatialFilterClass();
                IGeometry buffer = pTo.Buffer(TOLERANCE);
                psf.Geometry = buffer;
                psf.SpatialRel =esriSpatialRelEnum.esriSpatialRelIntersects ;
                IFeatureCursor lineCursor = plineFtrCls.Search(psf, false);
                int cout=   plineFtrCls.FeatureCount(psf);
                IFeature lineF = null;
                while ((lineF = lineCursor.NextFeature()) != null)
                {
                    IPolyline line = lineF.Shape as IPolyline;
                    var outPnt = new PointClass() as IPoint;
                    double distAlong = double.NaN;
                    double distFrom = double.NaN;
                    bool bRight = false;
                    line.QueryPointAndDistance(esriSegmentExtension.esriNoExtension, pt, false, outPnt, ref distAlong, ref distFrom, ref bRight);
                    p.set_Value(index, Math.Floor(distAlong));
                    p.Store();
                }
                

            }
            MessageBox.Show("执行完成");
            ArcMap.Application.CurrentTool = null;
        }
        protected override void OnUpdate()
        {
            Enabled = ArcMap.Application != null;
        }
    }

}
