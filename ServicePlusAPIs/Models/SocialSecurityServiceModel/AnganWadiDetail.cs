using System.ComponentModel.DataAnnotations;

namespace ServicePlusAPIs.Models.SocialSecurityServiceModel
{
    public class AnganWadiDetail
    {
        [Key]
        public int Id { get; set; }
        public string NameOfPost { get; set; }
        public int PostType { get; set; }//Anganwadi Worker (AWW) ==1 ,Anganwadi Helper (AWH)==2
        public long CenterCode { get; set; }
        public string CenterName { get; set; }  //Anganwadi center Name as a village name
        public string StatusOfReservation { get; set; }

        public int VillageLGDCode {get ; set; }//Village LGD Code

    }
}
