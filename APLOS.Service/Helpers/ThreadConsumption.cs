using Syncfusion.Calculate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Service.Helpers
{
    public class ThreadConsumption
    {
        public enum FxKeys
        {
            SPI,
            FabWht
        }
        public class FKeys
        {
            public FxKeys Key { get; set; }
            public double Value { get; set; }
        }

        CalcQuickBase calcQuick = new CalcQuickBase();
        List<FKeys> _formulaFixedKeywordValues;
        string _formula = "";
        public ThreadConsumption(params FKeys[] FixedValues)
        {
            _formulaFixedKeywordValues = new List<FKeys>();
            foreach (var item in FixedValues)
                _formulaFixedKeywordValues.Add(item);
        }

        public double ExecuteFunction(string Formula, double ResultPercentage = 100)
        {
            double Result = 0;

            _formula = Formula.ToLower();
            try
            {
                for (int i = 0; i < _formulaFixedKeywordValues.Count; i++)
                {
                    _formula = _formula.Replace(_formulaFixedKeywordValues[i].Key.ToString().ToLower(), _formulaFixedKeywordValues[i].Value.ToString());
                }


                Result = OTSBD.clsStaticInfo.dbl((calcQuick.ParseAndCompute(_formula)));

                if (ResultPercentage != 100)
                    Result = (ResultPercentage / 100) * Result;
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return Result;
        }

    }
}
