namespace ServicePlusDashBoard.AccountModels
{
    public class TokenResponse
    {
        public string token 
        {
            get;
            set;
        }
        public DateTime expiration 
        {
            get;
            set;
        }
        public string Status 
        { 
            get;
            set;
        }
        public string Message 
        {
            get; 
            set;
        }
    }

}
