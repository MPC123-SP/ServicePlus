using Microsoft.AspNetCore.Mvc;
using System.Runtime.Serialization;

namespace ServicePlusAPIs.Helper
{
    /// <summary>
    /// Enum representing different game names.
    /// </summary>
    public enum GameName
    {
         
        [EnumMember(Value = "Under-14")]
        Basketball = 1,
 
        [EnumMember(Value = "Under-17")]
        Soccer = 2,

       
        [EnumMember(Value = "Tennis")]
        Tennis = 3,

       
        [EnumMember(Value = "Volleyball")]
        Volleyball = 4,

        
        [EnumMember(Value = "Baseball")]
        Baseball = 5
    }

   

}
