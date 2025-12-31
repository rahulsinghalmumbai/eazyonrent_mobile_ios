using eazyonrent.Services;


namespace eazyonrent.Pages;

public partial class LoginPage : ContentPage
{
    private readonly LoginServices loginServices;
    public LoginPage()
	{
		InitializeComponent();
        loginServices=new LoginServices();
	}
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        try
        {
            LoginButton.Text = "Logging in...";
            await Task.Delay(100);
             
          
            var loginResult = await loginServices.LoginAsync(PasswordEntry.Text.Trim());
            if (loginResult != null && loginResult.ResponseCode == "200")
            {
                await SecureStorage.SetAsync("mobile", PasswordEntry.Text.Trim()); 


                await SecureStorage.SetAsync("ListerId", loginResult.ExistUser.ListerId.ToString());
                await SecureStorage.SetAsync("ListerIdFirst", loginResult.ListerId.ToString());

                // await DisplayAlert("Success", "Login Successfully..", "OK");
                await Navigation.PushAsync(new GuesPage());
                Navigation.RemovePage(this);
            }
            else
            {
                
                await DisplayAlert("Error", loginResult?.ResponseMessage ?? "Login failed", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Login failed: {ex.Message}", "OK");
        }
        finally
        {
            LoginButton.Text = "Login"; 
        }
    }

}