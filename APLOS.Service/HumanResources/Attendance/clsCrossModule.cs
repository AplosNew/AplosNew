using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


    public class clsCrossModule
    {
    public string GetAttSum()
    {
        try
        {
            return @" TotalPresent = CASE WHEN Category = 'Present' and LTSystemID is null THEN 1 
                                                       WHEN Category = 'Present' and LTSystemID is not null and LeaveDuration<1 THEN (1-LeaveDuration)
                                                       WHEN Category = 'Half Day' and LTSystemID is not null  THEN (1-LeaveDuration)
                                                       WHEN Category = 'Half Day' and LTSystemID is null  THEN 0.5
                                                       ELSE 0 END,

                                                        --LWP and LWOP both r considered          
			                            TotalLate = CASE WHEN Category = 'Late' and LTSystemID is null THEN 1
                                                        WHEN Category = 'Late' and LTSystemID is not null and LeaveDuration<1 THEN (1-LeaveDuration)
                                                        WHEN Category = 'Late' and LTSystemID is not null and LeaveDuration=1 THEN 1
                                                        ELSE 0 END,

			                            TotalAbsent = CASE WHEN Category = 'Absent' and LTSystemID is null THEN 1
                                                        WHEN Category = 'Absent' and LTSystemID is not null and LeaveDuration<1 THEN (1-LeaveDuration)
                                                        WHEN Category = 'Absent' and LTSystemID is not null and LeaveDuration=1 THEN 1
                                                        WHEN Category = 'Half Day' and LTSystemID is null  THEN 0.5
                                                        ELSE 0 END,

			                            TotalLv = CASE WHEN LTSystemID is not null  and Category<>'Leave' and LeaveDuration<1 and IsLWP=0 THEN LeaveDuration
                                                          WHEN LTSystemID is not null  and Category='Leave' and IsLWP=0 THEN LeaveDuration
                                                           ELSE 0 END,

                                        TotalLWP = CASE WHEN LTSystemID is not null  and Category<>'Leave' and LeaveDuration<1 and IsLWP=1 THEN LeaveDuration
                                                        WHEN LTSystemID is not null  and Category='Leave' and IsLWP=1 THEN LeaveDuration                                                        
                                                        ELSE 0 END,

			                            TotalMLv = 0,
                                        TotalCompAssignLv = 0,
			                            TotalWeekOff = CASE WHEN Category = 'Weekend' THEN 1
                                                        ELSE 0 END,

			                            TotalHoliDay = CASE WHEN Category = 'Holiday' THEN 1
                                                       ELSE 0 END,

                                        TotalWeekOffHoliDay = 0,";
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    public string xxGetAttSum()
    {
        try
        {
            return @" TotalPresent = CASE WHEN DayStatus = 'P' and LTSystemID is null THEN 1 
                                                       WHEN DayStatus = 'WP' and LTSystemID is null THEN 1

                                                       WHEN DayStatus = 'HP' and LTSystemID is null THEN 1
                                                       WHEN DayStatus = 'WHP' and LTSystemID is null THEN 1

                                                       WHEN DayStatus = 'HWP' and LTSystemID is null THEN 1

                                                       WHEN DayStatus = 'P' and LTSystemID is not null and IsHalfDayLeave = 1 THEN 0.5
                                                       WHEN DayStatus = 'P' and LTSystemID is not null and IsHalfDayLeave = 0 THEN 1
                                                       WHEN DayStatus = 'LVP' and LTSystemID is not null and IsHalfDayLeave = 1 THEN 0.5
                                                       --WHEN DayStatus = 'LVP' and LTSystemID is not null and IsHalfDayLeave = 0   THEN 1
                                                        -- lwp n normal in both case it will b 0.5
                                                       WHEN DayStatus = 'WP' and LTSystemID is not null and IsHalfDayLeave = 1 THEN 0.5
                                                       WHEN DayStatus = 'HP' and LTSystemID is not null and IsHalfDayLeave = 1 THEN 0.5
                                                       WHEN DayStatus = 'WHP' and LTSystemID is not null and IsHalfDayLeave = 1 THEN 0.5
                                                       WHEN DayStatus = 'HWP' and LTSystemID is not null and IsHalfDayLeave = 1 THEN 0.5


                                                      WHEN DayStatus = 'WP' and LTSystemID is not null and IsHalfDayLeave = 0   THEN 1
                                                       WHEN DayStatus = 'HP' and LTSystemID is not null and IsHalfDayLeave = 0  THEN 1
                                                       WHEN DayStatus = 'WHP' and LTSystemID is not null and IsHalfDayLeave = 0  THEN 1
                                                       WHEN DayStatus = 'HWP' and LTSystemID is not null and IsHalfDayLeave = 0  THEN 1

                                                        --if late and half leave, rest of the day will b Present not late. not considereing 1st or 2nd half
                                                       WHEN DayStatus = 'L' and LTSystemID is not null and IsHalfDayLeave = 1  THEN 0.5
                                                       WHEN DayStatus = 'WL' and LTSystemID is not null and IsHalfDayLeave = 1   THEN 0.5
                                                       WHEN DayStatus = 'HL' and LTSystemID is not null and IsHalfDayLeave = 1  THEN 0.5
                                                       WHEN DayStatus = 'WHL' and LTSystemID is not null and IsHalfDayLeave = 1   THEN 0.5
                                                       WHEN DayStatus = 'HWL' and LTSystemID is not null and IsHalfDayLeave = 1    THEN 0.5
                                                       WHEN DayStatus = 'LVL' and LTSystemID is not null and IsHalfDayLeave = 1    THEN 0.5

                                                       WHEN DayStatus = 'RST' THEN 1

                                                       WHEN DayStatus = 'OD' THEN 1
                                                       WHEN DayStatus = 'HDP'  THEN 0.5
                                                       WHEN DayStatus = 'HDA' and LTSystemID is null THEN 0.5
                                                        ---for AW/PW
                                                        WHEN DayStatus = 'AW' and LTSystemID is null THEN 1
                                                        WHEN DayStatus = 'AW' and LTSystemID is not null and IsHalfDayLeave = 1 THEN 0.5
                                                        WHEN DayStatus = 'PW' and LTSystemID is null THEN 1
                                                        WHEN DayStatus = 'PW' and LTSystemID is not null and IsHalfDayLeave = 1  THEN 0.5

                                                        WHEN DayStatus = 'AHP' and LTSystemID is null THEN 1
                                                        WHEN DayStatus = 'AHP' and LTSystemID is not null and IsHalfDayLeave = 1 THEN 0.5

                                                        WHEN DayStatus = 'CWP' and LTSystemID is null THEN 1
                                                        WHEN DayStatus = 'CWP' and LTSystemID is not null and IsHalfDayLeave = 1 THEN 0.5


                                                       ELSE 0 END,
			                            TotalLate = CASE WHEN DayStatus = 'L' and LTSystemID is null THEN 1
                                                        WHEN DayStatus = 'L' and LTSystemID is not null and IsHalfDayLeave = 0  THEN 1
                                                       WHEN DayStatus = 'WL' and LTSystemID is null THEN 1

                                                       WHEN DayStatus = 'HL' and LTSystemID is null THEN 1
                                                       WHEN DayStatus = 'WHL' and LTSystemID is null THEN 1
                                                       WHEN DayStatus = 'HWL' and LTSystemID is null THEN 1
                                                       --WHEN DayStatus = 'LVL' and LTSystemID is not null and IsHalfDayLeave = 0    THEN 1   

                                                       WHEN DayStatus = 'WL' and LTSystemID is not null and IsHalfDayLeave = 0   THEN 1
                                                       WHEN DayStatus = 'HL' and LTSystemID is not null and IsHalfDayLeave = 0  THEN 1
                                                       WHEN DayStatus = 'WHL' and LTSystemID is not null and IsHalfDayLeave = 0   THEN 1
                                                       WHEN DayStatus = 'HWL' and LTSystemID is not null and IsHalfDayLeave = 0    THEN 1
                                                   

                                                       ELSE 0 END,
			                            TotalAbsent = CASE WHEN DayStatus = 'A' and LTSystemID is null THEN 1
                                                        WHEN DayStatus = 'WA' and LTSystemID is null THEN 1
                                                        WHEN DayStatus = 'HA' and LTSystemID is null THEN 1

                                                        WHEN DayStatus = 'HWA' and LTSystemID is null THEN 1

                                                         WHEN DayStatus = 'WA' and LTSystemID is not null and IsHalfDayLeave = 0  THEN 1
                                                         WHEN DayStatus = 'HA' and LTSystemID is not null and IsHalfDayLeave = 0  THEN 1
                                                       
                                                        WHEN DayStatus = 'LV'and LTSystemID is not null and IsHalfDayLeave = 1 THEN 0.5

                                                        WHEN DayStatus = 'WA' and LTSystemID is not null and IsHalfDayLeave = 1  THEN 0.5
                                                        WHEN DayStatus = 'WA' and LTSystemID is not null and IsHalfDayLeave = 0  THEN 1
                                                        WHEN DayStatus = 'A' and LTSystemID is not null and IsHalfDayLeave = 1  THEN 0.5
                                                        WHEN DayStatus = 'A' and LTSystemID is not null and IsHalfDayLeave = 0  THEN 1
                                                       
                                                        WHEN DayStatus = 'LVA' and LTSystemID is not null and IsHalfDayLeave = 0 THEN 1
                                                        WHEN DayStatus = 'LVA' and LTSystemID is not null and IsHalfDayLeave = 1 THEN 0.5

                                                        WHEN DayStatus = 'HDP' and LTSystemID is null THEN 0.5
                                                        WHEN DayStatus = 'HDA' THEN 0.5

														 WHEN DayStatus = 'WLV' and LTSystemID is not null and IsHalfDayLeave = 0 and IsLWP=1 THEN 1
                                                         WHEN DayStatus = 'HLV' and LTSystemID is not null and IsHalfDayLeave = 0 and IsLWP=1 THEN 1
														-- WHEN DayStatus = 'WLV' and LTSystemID is not null and IsHalfDayLeave = 1 and IsLWP=1 THEN 0.5
                                                        -- WHEN DayStatus = 'HLV' and LTSystemID is not null and IsHalfDayLeave = 1 and IsLWP=1 THEN 0.5

                                                        ELSE 0 END,
			                            TotalLv = CASE WHEN LTSystemID is not null  and IsHalfDayLeave = 1 and IsLWP=0 THEN 0.5

                                                          WHEN LTSystemID is not null  and IsHalfDayLeave = 0 and DayStatus='LV' and IsLWP=0 THEN 1
                                                          WHEN LTSystemID is not null  and IsHalfDayLeave = 0 and DayStatus='A' and IsLWP=0  THEN 1

                                                          WHEN LTSystemID is not null  and IsHalfDayLeave = 0 and DayStatus='WLV' and IsLWP=0  THEN 1
                                                          WHEN LTSystemID is not null  and IsHalfDayLeave = 0 and DayStatus='HLV' and IsLWP=0  THEN 1

                                                          WHEN DayStatus = 'LVL' and LTSystemID is not null and IsHalfDayLeave = 0   and IsLWP=0 THEN 1 
                                                          WHEN DayStatus = 'LVP' and LTSystemID is not null and IsHalfDayLeave = 0   and IsLWP=0 THEN 1

                                                        WHEN DayStatus = 'AHP' and LTSystemID is not null and IsHalfDayLeave = 1 THEN 0.5
                                                        WHEN DayStatus = 'CWP' and LTSystemID is not null and IsHalfDayLeave = 1 THEN 0.5

                                                           ELSE 0 END,

                                        TotalLWP = CASE WHEN LTSystemID is not null  and IsHalfDayLeave = 1 and DayStatus<>'LV' and IsLWP=1 THEN 0.5
                                                        WHEN LTSystemID is not null  and IsHalfDayLeave = 1 and DayStatus='LV' and IsLWP=1 THEN 0.5
                                                        WHEN LTSystemID is not null  and IsHalfDayLeave = 0 and DayStatus<>'LV' and IsLWP=1 THEN 0
                                                        WHEN LTSystemID is not null  and IsHalfDayLeave = 0 and IsLWP=1 THEN 1
                                                        ELSE 0 END,

			                            TotalMLv = CASE WHEN DayStatus = 'MLV' THEN 1

                                                        WHEN DayStatus = 'MLVP' THEN 1

                                                        WHEN DayStatus = 'MLVL' THEN 1

                                                        WHEN DayStatus = 'WMLV' THEN 1

                                                        WHEN DayStatus = 'HMLV' THEN 1

                                                        WHEN DayStatus = 'WMLVP' THEN 1

                                                        WHEN DayStatus = 'HMLVP' THEN 1

                                                        WHEN DayStatus = 'WMLVL' THEN 1

                                                        WHEN DayStatus = 'HMLVL' THEN 1
                                                        WHEN DayStatus = 'WHMLV' THEN 1
                                                        WHEN DayStatus = 'WHMLVP' THEN 1
                                                        WHEN DayStatus = 'WHMLVL' THEN 1

                                                        WHEN DayStatus = 'HWMLV' THEN 1

                                                        WHEN DayStatus = 'HWMLVP' THEN 1

                                                        WHEN DayStatus = 'HWMLVL' THEN 1

                                                        ELSE 0 END,
                                        TotalCompAssignLv = CASE WHEN DayStatus = 'CAL' THEN 1
                                                        WHEN DayStatus = 'CALP' THEN 1

                                                        WHEN DayStatus = 'CALL' THEN 1

                                                        WHEN DayStatus = 'WCAL' THEN 1

                                                        WHEN DayStatus = 'HCAL' THEN 1

                                                        WHEN DayStatus = 'WCALP' THEN 1

                                                        WHEN DayStatus = 'HCALP' THEN 1

                                                        WHEN DayStatus = 'WCALL' THEN 1

                                                        WHEN DayStatus = 'HCALL' THEN 1
                                                        WHEN DayStatus = 'WHCAL' THEN 1
                                                        WHEN DayStatus = 'WHCALP' THEN 1
                                                        WHEN DayStatus = 'WHCALL' THEN 1

                                                        WHEN DayStatus = 'HWCAL' THEN 1

                                                        WHEN DayStatus = 'HWCALP' THEN 1

                                                        WHEN DayStatus = 'HWCALL' THEN 1

                                                        ELSE 0 END,
			                            TotalWeekOff = CASE WHEN DayStatus = 'W' THEN 1
                                                            WHEN DayStatus = 'HW' THEN 1
                                                            WHEN DayStatus = 'WH' THEN 1
                                                            WHEN DayStatus = 'CW' THEN 1

                                                        ELSE 0 END,
			                            TotalHoliDay = CASE WHEN DayStatus = 'H' THEN 1
                                                            WHEN DayStatus = 'AH' THEN 1

                                                       ELSE 0 END,
                                        TotalWeekOffHoliDay = 0,";
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    public string xGetAttSum()
    {
        try
        {
            return @" TotalPresent = CASE WHEN DayStatus = 'P' and LTSystemID is null THEN 1 
                                                       WHEN DayStatus = 'WP' and LTSystemID is null THEN 1

                                                       WHEN DayStatus = 'HP' and LTSystemID is null THEN 1
                                                       WHEN DayStatus = 'WHP' and LTSystemID is null THEN 1

                                                       WHEN DayStatus = 'HWP' and LTSystemID is null THEN 1

                                                       WHEN DayStatus = 'P' and LTSystemID is not null and IsHalfDayLeave = 1 THEN 0.5
                                                       WHEN DayStatus = 'P' and LTSystemID is not null and IsHalfDayLeave = 0 THEN 1
                                                       WHEN DayStatus = 'LVP' and LTSystemID is not null and IsHalfDayLeave = 1 THEN 0.5
                                                       --WHEN DayStatus = 'LVP' and LTSystemID is not null and IsHalfDayLeave = 0   THEN 1
                                                        -- lwp n normal in both case it will b 0.5
                                                       WHEN DayStatus = 'WP' and LTSystemID is not null and IsHalfDayLeave = 1 THEN 0.5
                                                       WHEN DayStatus = 'HP' and LTSystemID is not null and IsHalfDayLeave = 1 THEN 0.5
                                                       WHEN DayStatus = 'WHP' and LTSystemID is not null and IsHalfDayLeave = 1 THEN 0.5
                                                       WHEN DayStatus = 'HWP' and LTSystemID is not null and IsHalfDayLeave = 1 THEN 0.5


                                                      WHEN DayStatus = 'WP' and LTSystemID is not null and IsHalfDayLeave = 0   THEN 1
                                                       WHEN DayStatus = 'HP' and LTSystemID is not null and IsHalfDayLeave = 0  THEN 1
                                                       WHEN DayStatus = 'WHP' and LTSystemID is not null and IsHalfDayLeave = 0  THEN 1
                                                       WHEN DayStatus = 'HWP' and LTSystemID is not null and IsHalfDayLeave = 0  THEN 1

                                                        --if late and half leave, rest of the day will b Present not late. not considereing 1st or 2nd half
                                                       WHEN DayStatus = 'L' and LTSystemID is not null and IsHalfDayLeave = 1  THEN 0.5
                                                       WHEN DayStatus = 'WL' and LTSystemID is not null and IsHalfDayLeave = 1   THEN 0.5
                                                       WHEN DayStatus = 'HL' and LTSystemID is not null and IsHalfDayLeave = 1  THEN 0.5
                                                       WHEN DayStatus = 'WHL' and LTSystemID is not null and IsHalfDayLeave = 1   THEN 0.5
                                                       WHEN DayStatus = 'HWL' and LTSystemID is not null and IsHalfDayLeave = 1    THEN 0.5
                                                       WHEN DayStatus = 'LVL' and LTSystemID is not null and IsHalfDayLeave = 1    THEN 0.5

                                                       WHEN DayStatus = 'RST' THEN 1

                                                       WHEN DayStatus = 'OD' THEN 1
                                                       WHEN DayStatus = 'HDP'  THEN 0.5
                                                       WHEN DayStatus = 'HDA' and LTSystemID is null THEN 0.5

                                                       ELSE 0 END,
			                            TotalLate = CASE WHEN DayStatus = 'L' and LTSystemID is null THEN 1
                                                        WHEN DayStatus = 'L' and LTSystemID is not null and IsHalfDayLeave = 0  THEN 1
                                                       WHEN DayStatus = 'WL' and LTSystemID is null THEN 1

                                                       WHEN DayStatus = 'HL' and LTSystemID is null THEN 1
                                                       WHEN DayStatus = 'WHL' and LTSystemID is null THEN 1
                                                       WHEN DayStatus = 'HWL' and LTSystemID is null THEN 1
                                                       --WHEN DayStatus = 'LVL' and LTSystemID is not null and IsHalfDayLeave = 0    THEN 1   

                                                       WHEN DayStatus = 'WL' and LTSystemID is not null and IsHalfDayLeave = 0   THEN 1
                                                       WHEN DayStatus = 'HL' and LTSystemID is not null and IsHalfDayLeave = 0  THEN 1
                                                       WHEN DayStatus = 'WHL' and LTSystemID is not null and IsHalfDayLeave = 0   THEN 1
                                                       WHEN DayStatus = 'HWL' and LTSystemID is not null and IsHalfDayLeave = 0    THEN 1
                                                   

                                                       ELSE 0 END,
			                            TotalAbsent = CASE WHEN DayStatus = 'A' and LTSystemID is null THEN 1
                                                        WHEN DayStatus = 'WA' and LTSystemID is null THEN 1
                                                        WHEN DayStatus = 'HA' and LTSystemID is null THEN 1

                                                        WHEN DayStatus = 'HWA' and LTSystemID is null THEN 1

                                                         WHEN DayStatus = 'WA' and LTSystemID is not null and IsHalfDayLeave = 0  THEN 1
                                                         WHEN DayStatus = 'HA' and LTSystemID is not null and IsHalfDayLeave = 0  THEN 1
                                                       
                                                        WHEN DayStatus = 'LV'and LTSystemID is not null and IsHalfDayLeave = 1 THEN 0.5

                                                        WHEN DayStatus = 'WA' and LTSystemID is not null and IsHalfDayLeave = 1  THEN 0.5
                                                        WHEN DayStatus = 'WA' and LTSystemID is not null and IsHalfDayLeave = 0  THEN 1
                                                        WHEN DayStatus = 'A' and LTSystemID is not null and IsHalfDayLeave = 1  THEN 0.5
                                                        WHEN DayStatus = 'A' and LTSystemID is not null and IsHalfDayLeave = 0  THEN 1
                                                       
                                                        WHEN DayStatus = 'LVA' and LTSystemID is not null and IsHalfDayLeave = 0 THEN 1
                                                        WHEN DayStatus = 'LVA' and LTSystemID is not null and IsHalfDayLeave = 1 THEN 0.5

                                                        WHEN DayStatus = 'HDP' and LTSystemID is null THEN 0.5
                                                        WHEN DayStatus = 'HDA' THEN 0.5

														 WHEN DayStatus = 'WLV' and LTSystemID is not null and IsHalfDayLeave = 0 and IsLWP=1 THEN 1
                                                         WHEN DayStatus = 'HLV' and LTSystemID is not null and IsHalfDayLeave = 0 and IsLWP=1 THEN 1
														-- WHEN DayStatus = 'WLV' and LTSystemID is not null and IsHalfDayLeave = 1 and IsLWP=1 THEN 0.5
                                                        -- WHEN DayStatus = 'HLV' and LTSystemID is not null and IsHalfDayLeave = 1 and IsLWP=1 THEN 0.5

                                                        ELSE 0 END,
			                            TotalLv = CASE WHEN LTSystemID is not null  and IsHalfDayLeave = 1 and IsLWP=0 THEN 0.5

                                                          WHEN LTSystemID is not null  and IsHalfDayLeave = 0 and DayStatus='LV' and IsLWP=0 THEN 1
                                                          WHEN LTSystemID is not null  and IsHalfDayLeave = 0 and DayStatus='A' and IsLWP=0  THEN 1

                                                          WHEN LTSystemID is not null  and IsHalfDayLeave = 0 and DayStatus='WLV' and IsLWP=0  THEN 1
                                                          WHEN LTSystemID is not null  and IsHalfDayLeave = 0 and DayStatus='HLV' and IsLWP=0  THEN 1

                                                          WHEN DayStatus = 'LVL' and LTSystemID is not null and IsHalfDayLeave = 0   and IsLWP=0 THEN 1 
                                                          WHEN DayStatus = 'LVP' and LTSystemID is not null and IsHalfDayLeave = 0   and IsLWP=0 THEN 1

                                                           ELSE 0 END,

                                        TotalLWP = CASE WHEN LTSystemID is not null  and IsHalfDayLeave = 1 and DayStatus<>'LV' and IsLWP=1 THEN 0.5
                                                        WHEN LTSystemID is not null  and IsHalfDayLeave = 1 and DayStatus='LV' and IsLWP=1 THEN 0.5
                                                        WHEN LTSystemID is not null  and IsHalfDayLeave = 0 and DayStatus<>'LV' and IsLWP=1 THEN 0
                                                        WHEN LTSystemID is not null  and IsHalfDayLeave = 0 and IsLWP=1 THEN 1
                                                        ELSE 0 END,

			                            TotalMLv = CASE WHEN DayStatus = 'MLV' THEN 1

                                                        WHEN DayStatus = 'MLVP' THEN 1

                                                        WHEN DayStatus = 'MLVL' THEN 1

                                                        WHEN DayStatus = 'WMLV' THEN 1

                                                        WHEN DayStatus = 'HMLV' THEN 1

                                                        WHEN DayStatus = 'WMLVP' THEN 1

                                                        WHEN DayStatus = 'HMLVP' THEN 1

                                                        WHEN DayStatus = 'WMLVL' THEN 1

                                                        WHEN DayStatus = 'HMLVL' THEN 1
                                                        WHEN DayStatus = 'WHMLV' THEN 1
                                                        WHEN DayStatus = 'WHMLVP' THEN 1
                                                        WHEN DayStatus = 'WHMLVL' THEN 1

                                                        WHEN DayStatus = 'HWMLV' THEN 1

                                                        WHEN DayStatus = 'HWMLVP' THEN 1

                                                        WHEN DayStatus = 'HWMLVL' THEN 1

                                                        ELSE 0 END,
                                        TotalCompAssignLv = CASE WHEN DayStatus = 'CAL' THEN 1
                                                        WHEN DayStatus = 'CALP' THEN 1

                                                        WHEN DayStatus = 'CALL' THEN 1

                                                        WHEN DayStatus = 'WCAL' THEN 1

                                                        WHEN DayStatus = 'HCAL' THEN 1

                                                        WHEN DayStatus = 'WCALP' THEN 1

                                                        WHEN DayStatus = 'HCALP' THEN 1

                                                        WHEN DayStatus = 'WCALL' THEN 1

                                                        WHEN DayStatus = 'HCALL' THEN 1
                                                        WHEN DayStatus = 'WHCAL' THEN 1
                                                        WHEN DayStatus = 'WHCALP' THEN 1
                                                        WHEN DayStatus = 'WHCALL' THEN 1

                                                        WHEN DayStatus = 'HWCAL' THEN 1

                                                        WHEN DayStatus = 'HWCALP' THEN 1

                                                        WHEN DayStatus = 'HWCALL' THEN 1

                                                        ELSE 0 END,
			                            TotalWeekOff = CASE WHEN DayStatus = 'W' THEN 1
                                                            WHEN DayStatus = 'HW' THEN 1
                                                            WHEN DayStatus = 'WH' THEN 1

                                                        ELSE 0 END,
			                            TotalHoliDay = CASE WHEN DayStatus = 'H' THEN 1

                                                       ELSE 0 END,
                                        TotalWeekOffHoliDay = CASE WHEN DayStatus = 'WH' THEN 1

                                                        WHEN DayStatus = 'HW' THEN 1

                                                       ELSE 0 END,";
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}
