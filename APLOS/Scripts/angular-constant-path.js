
var virtualPath = {};
virtualPath.ROOT_FOLDRR = '/' + getCookie('ROOT_FOLDRR');
virtualPath.LogoOrImage = virtualPath.ROOT_FOLDRR + '/Organizations/';
virtualPath.VASPath = virtualPath.ROOT_FOLDRR + '/VAS/';
virtualPath.MaterialsImage = virtualPath.ROOT_FOLDRR + '/Materials/';
virtualPath.ProductsImage = virtualPath.ROOT_FOLDRR + '/Products/';
virtualPath.HRMSImage = '/EmpPic/';
virtualPath.EmployeeImage = virtualPath.ROOT_FOLDRR + '/UserPictures/';
virtualPath.EmployeeJobDescription = virtualPath.ROOT_FOLDRR + '/JobDescriptions';
virtualPath.SOPActivityDocument = virtualPath.ROOT_FOLDRR + '/SOP/Document';
virtualPath.SOPDocument = virtualPath.ROOT_FOLDRR + '/SOP/SOPDocument';
virtualPath.SOPCategory = virtualPath.ROOT_FOLDRR + '/SOP/SOPCategory';
virtualPath.SOPSubCategory = virtualPath.ROOT_FOLDRR + '/SOP/SOPSubCategory';
virtualPath.EmpPic = virtualPath.ROOT_FOLDRR + '/PreRecruitments/EmpPic/';
virtualPath.QualificationDocument = virtualPath.ROOT_FOLDRR + '/PreRecruitments/QualificationDoc/';
virtualPath.ExperienceDocument = virtualPath.ROOT_FOLDRR + '/PreRecruitments/ExperienceDoc/';
virtualPath.TrainingDocument = virtualPath.ROOT_FOLDRR + '/PreRecruitments/TrainingDoc/';
virtualPath.PreRecruitmentDocument = virtualPath.ROOT_FOLDRR + '/PreRecruitments/Documents';
virtualPath.ResignationLetter = virtualPath.ROOT_FOLDRR + '/EmployeeProfiles/ResignationLetter';
virtualPath.EmployeePic = virtualPath.ROOT_FOLDRR + '/EmployeeProfiles/EmpPic/';
virtualPath.EmployeeQualificationDocument = virtualPath.ROOT_FOLDRR + '/EmployeeProfiles/QualificationDoc/';
virtualPath.EmployeeExperienceDocument = virtualPath.ROOT_FOLDRR + '/EmployeeProfiles/ExperienceDoc/';
virtualPath.EmployeeTrainingDocument = virtualPath.ROOT_FOLDRR + '/EmployeeProfiles/TrainingDoc/';
virtualPath.EmployeeDocument = virtualPath.ROOT_FOLDRR + '/EmployeeProfiles/Documents';
virtualPath.ExpensesDocument = virtualPath.ROOT_FOLDRR + '/Expenses';
virtualPath.QuickCostingImagePath = virtualPath.ROOT_FOLDRR + '/Costing';
virtualPath.IssueTransactionDocument = virtualPath.ROOT_FOLDRR + '/ToDo';
virtualPath.CardHolderSignature = virtualPath.ROOT_FOLDRR + '/EmployeeProfiles/CardHolderSignature/';
virtualPath.AuthorizedSignature = virtualPath.ROOT_FOLDRR + '/EmployeeProfiles/AuthorizedSignature/';
virtualPath.PurchaseOrder = virtualPath.ROOT_FOLDRR + '/PurchaseOrder';
virtualPath.GRN = virtualPath.ROOT_FOLDRR + '/GRN';
virtualPath.ServicePO = virtualPath.ROOT_FOLDRR + '/ServicePO';
virtualPath.BOMPath = virtualPath.ROOT_FOLDRR + '/BoMDocument';
virtualPath.MOIPath = virtualPath.ROOT_FOLDRR + '/MOIDocument';
virtualPath.JobWorkValueAddedContract = virtualPath.ROOT_FOLDRR + '/JobWork/JobWorkValueAddedContract';
virtualPath.JobWorkTransformationContract = virtualPath.ROOT_FOLDRR + '/JobWork/JobWorkTransformationContract';
virtualPath.ProductionBulletinImage = virtualPath.ROOT_FOLDRR + '/ProductionBulletin/';
virtualPath.BulletinTemplateImage = virtualPath.ROOT_FOLDRR + '/BulletinTemplate/';
virtualPath.OSTransformationPO = virtualPath.ROOT_FOLDRR + '/JobWorkPurchaseOrder';
virtualPath.InvoiceDocument = virtualPath.ROOT_FOLDRR + '/InvoiceDocument';
virtualPath.ActivityDocuments = virtualPath.ROOT_FOLDRR + '/ActivityDocuments';
virtualPath.MSAPath = virtualPath.ROOT_FOLDRR + '/MSADocument';
virtualPath.ICUPath = virtualPath.ROOT_FOLDRR + '/ICUDocument';
virtualPath.GeneralContractPath = virtualPath.ROOT_FOLDRR + '/GeneralContractDocument';
virtualPath.SMEPath = virtualPath.ROOT_FOLDRR + '/SMEDocument';
virtualPath.FabricRollFile = virtualPath.ROOT_FOLDRR + '/FabricRollFile';
virtualPath.PAIPath = virtualPath.ROOT_FOLDRR + '/PAIDocument';
virtualPath.BlackListDocument = virtualPath.ROOT_FOLDRR + '/BlackList';
virtualPath.QRPdfDocument = virtualPath.ROOT_FOLDRR +'/QRPdfDocument/';
virtualPath.PostSalesInvoiceDoc = virtualPath.ROOT_FOLDRR +'/PostSalesInvoice/';
virtualPath.GarmentPic = virtualPath.ROOT_FOLDRR + '/DefectPic/';

function getCookie(cname) {
    var name = cname + "=";
    var decodedCookie = decodeURIComponent(document.cookie);
    var ca = decodedCookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) === ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) === 0)
            return c.substring(name.length, c.length);
    }
    return "";
}
