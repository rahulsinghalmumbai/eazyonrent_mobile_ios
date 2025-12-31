namespace eazyonrent
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

           // MainPage = new AppShell();
        }
        protected override Window CreateWindow(IActivationState? activationState)
        {
            //return new Window(new AppShell());
            //return new Window(new NavigationPage(new eazyonrent.Pages.UserProfilePage()));
            return new Window(new NavigationPage(new eazyonrent.Pages.LoginPage()));

        }
    }
}
