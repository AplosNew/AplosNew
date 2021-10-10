using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Planning.LineDesign
{
    public class OperationNode
    {
        List<DiagramShapes> AllShapes = new List<DiagramShapes>();
        List<object> AllShapesForJson = new List<object>();



    }
    public class xGenerateLineDiagram
    {
        SqlRepository _sqlRepository = new SqlRepository();
        List<DiagramShapes> AllShapes = new List<DiagramShapes>();
        public List<object> AllShapesForJson = new List<object>();
        public void MakeBulletinList(string BulletinId)
        {

            try
            {
                DataTable dtBulletin = _sqlRepository.GetDataTable(@"SELECT d.Id,ov.UserName AS OperationVariation,d.AllotedManpower,'Tarek Talukder' AS EmployeeName,'/POPResources/EmployeeProfiles/EmpPic/1800001.jpg' AS EmpImage
                                                                  FROM trn.ProductionBulletinTemplateDetail D
                                                                INNER JOIN mst.OperationVariation AS ov ON ov.Id=d.OperationVariationId
                                                                INNER JOIN mst.Operation AS o ON o.Id=ov.OperationId
                                                                LEFT OUTER JOIN hkp.MachineVariant AS mv ON mv.Id=d.MachineVarientId

                                                                WHERE d.ProductionBulletinTemplateMasterId='PBTM192' ORDER BY mv.Sequence");

                int halfBulletinCount = dtBulletin.Rows.Count / 2;
                int Width = 200; int Height = 32; int PaddingTop = 20; int paddingLeft = 10;
                int offsetX = 0; int offsetY = 0;

                for (int i = 0; i < dtBulletin.Rows.Count; i++)
                {
                    GroupingData group = new GroupingData();
                    group.id = "group" + dtBulletin.Rows[i]["Id"].ToString();

                    offsetY = 0;
                    #region Employee Image
                    Image emp = new Image();
                    AllShapes.Add(emp);

                    emp.offsetX = offsetX;
                    emp.offsetY = offsetY;
                    emp.height = 100;
                    emp.width = 100;
                    emp.id = "Employee" + dtBulletin.Rows[i]["Id"].ToString();
                    emp.name = "Employee" + dtBulletin.Rows[i]["Id"].ToString();
                    emp.source = dtBulletin.Rows[i]["EmpImage"].ToString();
                    emp.labels.Add(new labels { text = "" });
                    AllShapesForJson.Add(emp);
                    group.children.Add(emp.name);

                    offsetY += emp.height / 2;
                    #endregion Employee Image

                    #region EmployeeName
                    Rectangle rec = new Rectangle();
                    AllShapes.Add(rec);

                    rec.offsetX = offsetX;
                    rec.offsetY = offsetY;
                    rec.height = 20;
                    rec.width = Width;
                    rec.id = "EmpName" + dtBulletin.Rows[i]["Id"].ToString();
                    rec.name = "EmpName" + dtBulletin.Rows[i]["Id"].ToString();
                    rec.labels.Add(new labels { text = dtBulletin.Rows[i]["EmployeeName"].ToString() });
                    AllShapesForJson.Add(rec);
                    group.children.Add(rec.name);


                    offsetY += rec.height;
                    #endregion Operation

                    #region Operation
                    rec = new Rectangle();
                    AllShapes.Add(rec);

                    rec.offsetX = offsetX;
                    rec.offsetY = offsetY;
                    rec.height = Height;
                    rec.width = Width;
                    rec.id = "OperationName" + dtBulletin.Rows[i]["Id"].ToString();
                    rec.name = "OperationName" + dtBulletin.Rows[i]["Id"].ToString();
                    rec.labels.Add(new labels { text = dtBulletin.Rows[i]["OperationVariation"].ToString() });
                    AllShapesForJson.Add(rec);
                    group.children.Add(rec.name);

                    #endregion Operation



                    AllShapesForJson.Add(group);
                    offsetX += rec.width + paddingLeft;

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }



    }
    public class GenerateLineDiagram
    {
        SqlRepository _sqlRepository = new SqlRepository();
        List<DiagramShapes> AllShapes = new List<DiagramShapes>();
        public List<object> AllShapesForJson = new List<object>();
        public void MakeBulletinList(string BulletinId)
        {

            try
            {
                DataTable dtBulletin = _sqlRepository.GetDataTable(@"SELECT d.Id,ov.Id AS OperationVariationId,ov.UserName AS OperationVariation,
d.AllotedManpower,NULL AS EmployeeId,'Tarek Talukder' AS EmployeeName,d.Sequence,NULL AS Designation,
mv.Id AS MachineId,mv.UserName AS MachineDesc,d.AllotedWorkstation,d.RequiredManPower,
'/POPResources/EmployeeProfiles/EmpPic/1800001.jpg' AS EmpPicPath
    FROM trn.ProductionBulletinTemplateDetail D
INNER JOIN mst.OperationVariation AS ov ON ov.Id=d.OperationVariationId
INNER JOIN mst.Operation AS o ON o.Id=ov.OperationId
LEFT OUTER JOIN hkp.MachineVariant AS mv ON mv.Id=d.MachineVarientId

WHERE d.ProductionBulletinTemplateMasterId='PBTM192' ORDER BY D.Sequence");

                int halfBulletinCount = dtBulletin.Rows.Count / 2;
                int Width = 210; int Height = 180; int PaddingTop = 20; int paddingLeft = 10;
                int offsetX = 0; int offsetY = 0;

                for (int i = 0; i < dtBulletin.Rows.Count; i++)
                {
                    GroupingData group = new GroupingData();
                    group.id = "group" + dtBulletin.Rows[i]["Id"].ToString();

                    offsetY = 0;
                    #region Employee Image
                    Html emp = new Html();
                    AllShapes.Add(emp);
                    emp.height = Height;
                    emp.width = Width;
                    emp.offsetX = offsetX + (emp.width / 2);
                    emp.offsetY = offsetY + (emp.height / 2);

                    emp.id = "E" + dtBulletin.Rows[i]["Id"].ToString() + System.DateTime.Now.Ticks.ToString();
                    emp.name = "E" + dtBulletin.Rows[i]["Id"].ToString() + System.DateTime.Now.Ticks.ToString();

                    emp.labels.Add(new labels { text = "" });

                    emp.addInfo = new addInfo
                    {
                        EmployeeId = dtBulletin.Rows[i]["EmployeeId"].ToString(),
                        EmployeeName = dtBulletin.Rows[i]["EmployeeName"].ToString(),
                        MaterialMasterId = dtBulletin.Rows[i]["MachineId"].ToString(),
                        MaterialMasterDesc = dtBulletin.Rows[i]["MachineDesc"].ToString(),
                        OperationId = dtBulletin.Rows[i]["OperationVariationId"].ToString(),
                        OperationDesc = dtBulletin.Rows[i]["OperationVariation"].ToString(),
                        Designation = dtBulletin.Rows[i]["Designation"].ToString(),
                        EmpPicPath = dtBulletin.Rows[i]["EmpPicPath"].ToString(),
                        Sequence = OTSBD.clsStaticInfo.dbl(dtBulletin.Rows[i]["Sequence"].ToString()),
                        RequiredManPower = OTSBD.clsStaticInfo.dbl(dtBulletin.Rows[i]["RequiredManPower"].ToString()),
                        AllotedWorkstation = OTSBD.clsStaticInfo.dbl(dtBulletin.Rows[i]["AllotedWorkstation"].ToString())
                    };



                    offsetX += emp.width + paddingLeft;
                    AllShapesForJson.Add(emp);
                    //group.children.Add(emp.name);

                    #endregion Employee Image

                    RightArrow arrow = new RightArrow();
                    arrow.id = "R" + dtBulletin.Rows[i]["Id"].ToString() + System.DateTime.Now.Ticks.ToString();
                    arrow.name = "R" + dtBulletin.Rows[i]["Id"].ToString() + System.DateTime.Now.Ticks.ToString();
                    arrow.height = 50;
                    arrow.width = 50;
                    arrow.offsetX = offsetX + (arrow.width / 2);
                    arrow.offsetY = offsetY + (arrow.height / 2);



                    AllShapes.Add(arrow);
                    AllShapesForJson.Add(arrow);
                    //AllShapesForJson.Add(group);
                    offsetX += arrow.width + paddingLeft;

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }



    }
}
