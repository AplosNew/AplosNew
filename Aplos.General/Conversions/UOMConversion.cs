using Library.Core;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.General.Conversions
{
    public class UOMConversion
    {
        //Tarek Talukder; AGL; 7-Dec-2020
        public enum UOMCategory { BaseUOMId, PurchaseOrderUOMId, SalesOrderUOMId, StockUOMId }
        Dictionary<string, List<Factors>> MaterialUOMList = new Dictionary<string, List<Factors>>();
        SqlRepository _sqlRepository;
        public UOMConversion()
        {
            _sqlRepository = new SqlRepository();

            //order by is important
            MakeMaterialCluster(_sqlRepository.GetModelCollection<Factors>(@"select UOM.MaterialMasterId, UOM.AlternativeUOMId, UOM.BaseUOMId,
       convert(decimal(30,20),UOM.AltToBaseUOMFactor) AS AltToBaseUOMFactor,  convert(decimal(30,20),UOM.BaseToAltUOMFactor) AS BaseToAltUOMFactor, 
       
        UOM.UOMType, mm.PurchaseOrderUOMId, mm.SalesOrderUOMId, mm.StockUOMId from (
                    SELECT mm.Id AS MaterialMasterId, mm.BaseUOMId AS AlternativeUOMId,mm.BaseUOMId,
                    1 AS AltToBaseUOMFactor,1 AS BaseToAltUOMFactor,
                    'BASE' AS UOMType FROM mst.MaterialMaster AS mm
                    UNION ALL
                    SELECT mmau.MaterialMasterId, mmau.AlternativeUOMId, mmau.BaseUOMId,
                    mmau.BaseUOMFactor/mmau.AlternativeUOMFactor AS AltToBaseUOMFactor,mmau.AlternativeUOMFactor/mmau.BaseUOMFactor AS AltToBaseUOMFactor,
                    'ALT' AS UOMType FROM  mst.MaterialMasterAlternativeUOM AS mmau
                    ) AS UOM
                    LEFT JOIN mst.MaterialMaster AS mm ON mm.Id=uom.MaterialMasterId
                    ORDER BY UOM.MaterialMasterId"));

        }
        public UOMConversion(string MaterialMasterId)
        {
            _sqlRepository = new SqlRepository();

            MakeMaterialCluster(_sqlRepository.GetModelCollection<Factors>(@"select UOM.MaterialMasterId, UOM.AlternativeUOMId, UOM.BaseUOMId,
       convert(decimal(30,20),UOM.AltToBaseUOMFactor) AS AltToBaseUOMFactor,  convert(decimal(30,20),UOM.BaseToAltUOMFactor) AS BaseToAltUOMFactor, 
        UOM.UOMType, mm.PurchaseOrderUOMId, mm.SalesOrderUOMId, mm.StockUOMId from (
                    SELECT mm.Id AS MaterialMasterId, mm.BaseUOMId AS AlternativeUOMId,mm.BaseUOMId,
                    1 AS AltToBaseUOMFactor,1 AS BaseToAltUOMFactor,
                    'BASE' AS UOMType FROM mst.MaterialMaster AS mm
                    where mm.Id='" + MaterialMasterId + @"'
                    UNION ALL
                    SELECT mmau.MaterialMasterId, mmau.AlternativeUOMId, mmau.BaseUOMId,
                    mmau.BaseUOMFactor/mmau.AlternativeUOMFactor AS AltToBaseUOMFactor,mmau.AlternativeUOMFactor/mmau.BaseUOMFactor AS AltToBaseUOMFactor,
                    'ALT' AS UOMType FROM  mst.MaterialMasterAlternativeUOM AS mmau
                    where mmau.MaterialMasterId='" + MaterialMasterId + @"'
                    ) AS UOM
                    LEFT JOIN mst.MaterialMaster AS mm ON mm.Id=uom.MaterialMasterId
                ORDER BY UOM.MaterialMasterId"));
        }
        private void MakeMaterialCluster(List<Factors> UOMData)
        {
            MaterialUOMList = new Dictionary<string, List<Factors>>();
            List<Factors> _list = new List<Factors>();
            string MaterialMasterId = "";
            foreach (Factors item in UOMData)
            {
                if (MaterialMasterId != item.MaterialMasterId)
                {
                    _list = new List<Factors>();
                    MaterialUOMList.Add(item.MaterialMasterId, _list);
                }

                _list.Add(item);

                MaterialMasterId = item.MaterialMasterId;
            }
        }
        public string GetMaterialUOMByCategory(string MaterialMasterId, UOMCategory Category)
        {
            if (OTSBD.clsStaticInfo.nullrecorder(MaterialMasterId) == "")
                return "";

            if (MaterialUOMList.ContainsKey(MaterialMasterId) == false)
                return "";


            switch (Category)
            {
                case UOMCategory.BaseUOMId:
                    return MaterialUOMList[MaterialMasterId][0].BaseUOMId;
                case UOMCategory.PurchaseOrderUOMId:
                    return MaterialUOMList[MaterialMasterId][0].PurchaseOrderUOMId;
                case UOMCategory.SalesOrderUOMId:
                    return MaterialUOMList[MaterialMasterId][0].SalesOrderUOMId;
                case UOMCategory.StockUOMId:
                    return MaterialUOMList[MaterialMasterId][0].StockUOMId;
                default:
                    return "";
            }

        }

        public double Convert(string MaterialMasterId, string FromUOM, string ToUOM, double Value)
        {

            if (OTSBD.clsStaticInfo.nullrecorder(MaterialMasterId) == "")
                return 0;
            //If source and target uom are same, no need conversion
            if (FromUOM == ToUOM)
                return Value;

            List<Factors> AltUOM = MaterialUOMList[MaterialMasterId].Where(ee => ee.AlternativeUOMId == FromUOM).ToList();

            //means, need to convert the source UOM to target UOM
            if (AltUOM.Count > 0)
            {
                //converting to base
                Value = Value * AltUOM[0].AltToBaseUOMFactor;
                //and if target uom is also base;no need to further conversion
                if (AltUOM[0].BaseUOMId == ToUOM)
                    return Value;//because we have already converted the source value to base UOM. no need to further conversion

                //second step conversion from base value to alternative target value
                AltUOM = MaterialUOMList[MaterialMasterId].Where(ee => ee.AlternativeUOMId == ToUOM).ToList();
                if (AltUOM.Count > 0)
                {
                    //convert base value to alternative uom using basetoaltuomfactor
                    return Value = Value * AltUOM[0].BaseToAltUOMFactor;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                throw new Exception("No UOM found during conversion. Please check for material mater (Id: " + MaterialMasterId + @")");
            }

            return 0;
        }
    }
    class Factors : BaseModel
    {

        public string MaterialMasterId { get; set; }
        public string AlternativeUOMId { get; set; }
        public string BaseUOMId { get; set; }
        public double AltToBaseUOMFactor { get; set; }
        public double BaseToAltUOMFactor { get; set; }
        public string UOMType { get; set; }
        public string PurchaseOrderUOMId { get; set; }
        public string SalesOrderUOMId { get; set; }
        public string StockUOMId { get; set; }
    }
}
