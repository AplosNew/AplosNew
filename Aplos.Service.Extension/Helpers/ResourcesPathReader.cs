using Library.Data;
using Library.Service.Properties;
using System;
using System.IO;

namespace Library.Service.Extension.Helpers
{
    public class ResourcesPathReader
    {
        public static string GetVirtualDirectory()
        {
            var root_folder = IISManager.GetVirtualDirectoryName("ROOT_FOLDER");
#if DEBUG
            return root_folder;
#else
            return IISManager.GetVirtualPath("APP_NAME", root_folder);
#endif
        }

        public static string GetROOT_FOLDER()
        {
#if DEBUG
            return "POPResources";
#else
                var appName = IISManager.GetApplicationName("APP_NAME");
                if (string.IsNullOrEmpty(appName))
                    return IISManager.GetVirtualDirectoryName("ROOT_FOLDER");
                else
                    return appName + "/" + IISManager.GetVirtualDirectoryName("ROOT_FOLDER");
#endif
        }
        public static string GetROOT_FOLDER_Without_APP_Name()
        {
#if DEBUG
            return "POPResources";
#else
                
                    return IISManager.GetVirtualDirectoryName("ROOT_FOLDER");
           
#endif
        }
        public static string GetVirtualFolderName()
        {
            return IISManager.GetVirtualDirectoryName("ROOT_FOLDER");
        }

        private static string ResolveFilePath(string Path)
        {
            try
            {


                if (System.IO.Directory.Exists(Path) == false)
                {
                    try
                    {
                        System.IO.Directory.CreateDirectory(Path);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            catch (Exception wx)
            {


            }
            return Path;
        }
        public static string GetLogoOrImagePath()
        {
            try
            {
                return GetVirtualDirectory() + "/Organizations/";
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }

        public static string GetMaterialsImagePath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/Materials/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }
        public static string GetProductImagePath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/Products/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }
        public static string GetVASPath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/VAS/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }
        public static string GetToDoPath()
        {
            try
            {
                return ResolveFilePath( GetVirtualDirectory() + "/ToDo/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }
        public static string GetOrderCostingPath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/OrderCosting/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }
        public static string GetExpensesImagePath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/Expenses/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }
        public static string GetPurchaseOrderPath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/PurchaseOrder/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }
        public static string GetGRNPath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/GRN/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }
        public static string GetEmployeeJobDescriptionPath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/JD/JobDescriptions/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }

        public static string GetSOPDocumentPath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/SOP/SOPDocument/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }

        public static string GetSOPCategoryPath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/SOP/SOPCategory/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }

        public static string GetSOPSubCategoryPath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/SOP/SOPSubCategory/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }

        public static string GetSOPActivityDocumentPath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/SOP/Document/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }

        public static string GetEmployeeResignationLetterPath()
        {
            try
            {
                //return GetVirtualDirectory() + "/EmployeeProfiles/ResignationLetter/";
                return ResolveFilePath(GetVirtualDirectory() + "/EmployeeProfiles/ResignationLetter/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }

        public static string GetLCDocPath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/LCDocuments/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }

        public static string GetPrePurchaseInvoiceDocPath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/PrePurchaseInvoiceDocuments/Invoice/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }

        public static string GetPrePurchaseBLAWBDocPath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/PrePurchaseInvoiceDocuments/BLAWB/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }

        public static string GetPrePurchaseCNFDocPath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/PrePurchaseInvoiceDocuments/CNF/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }

        public static string GetPrePurchasePackingDocPath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/PrePurchaseInvoiceDocuments/Packing/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }

        public static string GetPrePurchaseTransportDocPath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/PrePurchaseInvoiceDocuments/Transport/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }

        public static string GetPrePurchaseVesselDocPath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/PrePurchaseInvoiceDocuments/Vessel/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }

        // Used in Service
        public static string GetEmployeePicPath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/PreRecruitments/EmpPic/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }
        public static string GetAuthorizedSignaturePath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/EmployeeProfiles/AuthorizedSignature/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }
        public static string GetCardHolderSignaturePath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/EmployeeProfiles/CardHolderSignature/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }

        // Used in Service
        public static string GetEmployeeDestinationPicPath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/EmployeeProfiles/EmpPic/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }
        public static string GetCostingPicPath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/Costing/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }
        public static string GetAttendanceRawData()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/Attendance/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }

        // Used in Service
        public static string GetExperienceSourcePath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/PreRecruitments/ExperienceDoc/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }

        // Used in Service
        public static string GetExperienceDestinationPath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/EmployeeProfiles/ExperienceDoc/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }

        // Used in Service
        public static string GetQualificationSourcePath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/PreRecruitments/QualificationDoc/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }

        // Used in Service
        public static string GetQualificationDestinationPath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/EmployeeProfiles/QualificationDoc/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }

        // Used in Service
        public static string GetTrainingSourcePath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/PreRecruitments/TrainingDoc/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }

        public static string GetTrainingDestinationPath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/EmployeeProfiles/TrainingDoc/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }

        public static string GetDocumentSourcePath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/PreRecruitments/Documents/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }

        public static string GetDocumentDestinationPath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/EmployeeProfiles/Documents/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }

        public static string GetUserPicUrl()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/UserPictures/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }

        public static void IsValidFileExtention(string extention)
        {
            try
            {
                var formate = false;
                var validFileFormate = new[] { "png", "jpg", "jpeg", "gif", "doc", "docx", "xls", "xlsx", "xlx", "ppt", "pptx", "pdf" };
                for (var i = 0; i < validFileFormate.Length; i++)
                {
                    var vF = "." + validFileFormate[i];
                    if (vF == extention.ToLower())
                        formate = true;
                }
                if (!formate)
                {
                    throw new CustomException(ServiceResources.IsValidFileFormate);
                }
            }
            catch (Exception)
            {
                throw new CustomException(ServiceResources.IsValidFileFormate);
            }
        }

        public static string GetEmployeeFingerPrintPath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/Biometrics/FingerPrint/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }

        public static string GetEmployeeFingerPrintForSBPath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/EmployeeProfiles/FingerPrint/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }
        public static string GetJoiningLetterPath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/EmployeeProfiles/JoiningLetter/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }

        public static string GetTempReportPath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/TempReports/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }

        public static string GetConfirmationLetterPath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "\\Templates\\");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }


        public static string GetCheckPath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "\\Templates\\Check\\");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }


        public static string GetMasterOrderFilePath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "\\Templates\\");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }

        public static string GetIssueRefPath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/IssueRefDoc/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }
        public static string GetPdfDocUrl()
        {
            try
            {
                return GetROOT_FOLDER() + "/Output/";
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }
        public static string SavePdfDocUrl()
        {
            try
            {
                var path = ResolveFilePath(GetVirtualDirectory() + "/Output/");
                //if (!Directory.Exists(path)) throw new CustomException(Resources.FilePathNotFound);
                if (!Directory.Exists(path)) throw new Exception(path);

                return path;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.ToString());
            }
        }

        public static string GetIssueTransactionDocumentsPath()
        {
            try
            {
                //return GetVirtualDirectory() + "/IssueTransactionDocuments/";
                return GetToDoPath();
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }
        public static string GetICSMasterDocumentPath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/Farming/ICSMaster/");

            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }
        public static string GetFarmerMasterPlotDocumentPath()
        {
            try
            {
                return ResolveFilePath(GetVirtualDirectory() + "/Farming/FarmerMaster/");
            }
            catch
            {
                throw new CustomException(ServiceResources.FilePathNotFound);
            }
        }

    }
}