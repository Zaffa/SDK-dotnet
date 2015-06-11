﻿using QMunicate.Core.Command;
using QMunicate.Core.DependencyInjection;
using QMunicate.Helper;
using Quickblox.Sdk;
using Quickblox.Sdk.GeneralDataModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using QMunicate.Core.MessageService;
using QMunicate.Models;

namespace QMunicate.ViewModels
{
    public class SignUpViewModel : ViewModel, IFileOpenPickerContinuable
    {
        private string fullName;
        private string email;
        private string password;
        private ImageSource userImage;

        public SignUpViewModel()
        {
            ChoosePhotoCommand = new RelayCommand(ChoosePhotoCommandExecute, () => !IsLoading);
            SignUpCommand = new RelayCommand(SignUpCommandExecute, () => !IsLoading);
            LoginCommand = new RelayCommand(LoginCommandExecute, () => !IsLoading);
        }

        public string FullName
        {
            get { return fullName; }
            set { Set(ref fullName, value); }
        }

        public string Email
        {
            get { return email; }
            set { Set(ref email, value); }
        }

        public string Password
        {
            get { return password; }
            set { Set(ref password, value); }
        }

        public ImageSource UserImage
        {
            get { return userImage; }
            set { Set(ref userImage, value); }
        }

        public ICommand ChoosePhotoCommand { get; set; }

        public ICommand SignUpCommand { get; set; }

        public ICommand LoginCommand { get; set; }


        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            
        }

        private async void ChoosePhotoCommandExecute()
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".png");
#if WINDOWS_PHONE_APP
            picker.PickSingleFileAndContinue();
#endif
        }

        private async void SignUpCommandExecute()
        {
            var messageService = Factory.CommonFactory.GetInstance<IMessageService>();
            
            if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await messageService.ShowAsync("Message", "Please fill all empty input fields");
                return;
            }

            //TODO: delete this and show error messages from server instead
            if (Password.Length < 8)
            {
                await messageService.ShowAsync("Message", "Password is too short (minimum is 8 characters)");
                return;
            }
            

            IsLoading = true;

            await QuickbloxClient.CoreClient.CreateSessionBaseAsync(ApplicationKeys.ApplicationId,
                        ApplicationKeys.AuthorizationKey, ApplicationKeys.AuthorizationSecret,
                        new DeviceRequest() { Platform = Platform.windows_phone, Udid = Helpers.GetHardwareId() });

            var response = await QuickbloxClient.UsersClient.SignUpUserAsync(FullName, Password, email: Email);

            if (response.StatusCode == HttpStatusCode.Created)
            {
                var loginResponse = await QuickbloxClient.CoreClient.ByEmailAsync(Email, Password);
                if (loginResponse.StatusCode == HttpStatusCode.Accepted)
                    NavigationService.Navigate(ViewLocator.Dialogs, new DialogsNavigationParameter { CurrentUserId = loginResponse.Result.User.Id, Password = Password });
                else
                    await messageService.ShowAsync("Error", "Error happened"); //TODO: deserialize properly and show errors from server
            }
            else
                await messageService.ShowAsync("Error", "Error happened"); //TODO: deserialize properly and show errors from server

            IsLoading = false;
        }

        private void LoginCommandExecute()
        {
            NavigationService.Navigate(ViewLocator.Login);
        }

        public async void ContinueFileOpenPicker(IReadOnlyList<StorageFile> files)
        {
            await QuickbloxClient.CoreClient.CreateSessionBaseAsync(ApplicationKeys.ApplicationId,
                        ApplicationKeys.AuthorizationKey, ApplicationKeys.AuthorizationSecret,
                        new DeviceRequest() { Platform = Platform.windows_phone, Udid = Helpers.GetHardwareId() });

            if (files != null && files.Any())
            {
                FileRandomAccessStream stream = (FileRandomAccessStream)await files[0].OpenAsync(FileAccessMode.Read);
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.SetSource(stream);
                UserImage = bitmapImage;

                //CONTENT: {"errors":{"base":["Forbidden. Need user."]}}

                //var bytes = new byte[stream.Size];
                //using (var dataReader = new DataReader(stream))
                //{
                //    await dataReader.LoadAsync((uint)stream.Size);
                //    dataReader.ReadBytes(bytes);
                //}

                //await QuickbloxClient.ContentClient.UploadFile(bytes, "image/jpg");
            }
        }

    }
}
