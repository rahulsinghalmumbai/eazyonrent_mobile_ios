

namespace eazyonrent.Connection
{
    public class AppSettings
    {

        public const string BaseApiUrl = "https://eazyonrent.com/api/";
       // public const string BaseApiUrl = "https://localhost:7274/api/";
    }
    public static class Endpoints
    {


        //1.for user login and registration
        public const string UserLogin = "User/UserRegister";
        //2. for all category
        public const string GetAllCat = "Categories/GellALLCategories";

        //3.for itme images
        public const string GetGuestItem = "Lister/GetGuestItems";
        //4.for item details by id
        public const string GetItemDetailsById = "Lister/GetItemById";
        //5.for profile item by user(lister) id
        public const string ProfileItem = "Lister/GetAllItem";
        //6.for lister profile by id
        public const string EditProfile = "Lister/EditProfileById";

       // public const string EditProfile = "Lister/EditListerById";

        //7.for create items
        public const string AddItems = "Lister/CreateItem";
        //8.for similar items by category id
        public const string SimilarItems = "Lister/GetSimilarItems";
        //9.for upload item images
        public const string AddItmeImages = "Lister/uploadItemImages";

        public const string BookItem = "Lister/bookItem";
        

        public const string HistoryItme = "Lister/bookHistory";
    }
}