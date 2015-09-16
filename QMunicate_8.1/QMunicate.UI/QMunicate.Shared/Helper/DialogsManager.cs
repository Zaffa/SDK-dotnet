﻿using QMunicate.Core.DependencyInjection;
using QMunicate.Core.Logger;
using QMunicate.Models;
using Quickblox.Sdk;
using Quickblox.Sdk.Modules.ChatModule.Models;
using Quickblox.Sdk.Modules.ChatModule.Requests;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Quickblox.Sdk.Modules.MessagesModule.Models;
using Message = Quickblox.Sdk.Modules.MessagesModule.Models.Message;

namespace QMunicate.Helper
{
    public class DialogsManager : IDialogsManager
    {
        #region Fields

        private bool isReloadingDialogs;
        private bool areAllGroupDialogsJoined;
        private readonly IQuickbloxClient quickbloxClient;

        #endregion

        #region Ctor

        public DialogsManager(IQuickbloxClient quickbloxClient)
        {
            this.quickbloxClient = quickbloxClient;
            quickbloxClient.MessagesClient.OnMessageReceived += MessagesClientOnOnMessageReceived;
            Dialogs = new ObservableCollection<DialogVm>();
        }

        #endregion

        #region Properties

        public ObservableCollection<DialogVm> Dialogs { get; private set; }

        #endregion

        #region Public methods

        public async Task ReloadDialogs()
        {
            if (isReloadingDialogs) return;
            isReloadingDialogs = true;

            try
            {
                var retrieveDialogsRequest = new RetrieveDialogsRequest();
                var response = await quickbloxClient.ChatClient.GetDialogsAsync(retrieveDialogsRequest);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Dialogs.Clear();
                    var currentUserId = SettingsManager.Instance.ReadFromSettings<int>(SettingsKeys.CurrentUserId);
                    foreach (var dialog in response.Result.Items)
                    {
                        if (dialog.Type == DialogType.Private)
                        {
                            var dialogVm = DialogVm.FromDialog(dialog);
                            int otherUserId = dialogVm.OccupantIds.FirstOrDefault(o => o != currentUserId);
                            dialogVm.Name = GetUserNameFromContacts(otherUserId);

                            Dialogs.Add(dialogVm);
                        }
                        else if(dialog.Type == DialogType.Group)
                        {
                            var dialogVm = DialogVm.FromDialog(dialog);

                            if (!string.IsNullOrEmpty(dialogVm.Photo))
                            {
                                var imagesService = ServiceLocator.Locator.Get<IImageService>();
                                dialogVm.Image = await imagesService.GetPublicImage(dialogVm.Photo);
                            }

                            Dialogs.Add(dialogVm);
                        }
                    }

                    var cachingQbClient = ServiceLocator.Locator.Get<ICachingQuickbloxClient>();
                    foreach (DialogVm dialogVm in Dialogs)
                    {
                        if (dialogVm.DialogType == DialogType.Private)
                        {
                            int otherUserId = dialogVm.OccupantIds.FirstOrDefault(o => o != currentUserId);
                            var user = await cachingQbClient.GetUserById(otherUserId);
                            if (user != null)
                            {
                                dialogVm.Name = user.FullName;
                                dialogVm.PrivatePhotoId = user.BlobId;
                            }
                        }
                    }

                    LoadDialogImages(100);
                }
            }
            finally
            {
                isReloadingDialogs = false;
            }
        }

        public void JoinAllGroupDialogs()
        {
            if (areAllGroupDialogsJoined) return;

            int currentUserId = SettingsManager.Instance.ReadFromSettings<int>(SettingsKeys.CurrentUserId);

            foreach (DialogVm dialogVm in Dialogs)
            {
                if (dialogVm.DialogType == DialogType.Group)
                {
                    var groupChatManager = quickbloxClient.MessagesClient.GetGroupChatManager(dialogVm.XmppRoomJid, dialogVm.Id);
                    groupChatManager.JoinGroup(currentUserId.ToString());
                }
            }

            areAllGroupDialogsJoined = true;
        }

        public async Task UpdateDialog(string dialogId, string lastActivity, DateTime lastMessageSent)
        {
            if (string.IsNullOrEmpty(dialogId)) return;

            var dialog = Dialogs.FirstOrDefault(d => d.Id == dialogId);
            if (dialog != null)
            {
                dialog.LastActivity = lastActivity;
                dialog.LastMessageSent = lastMessageSent;
                int itemIndex = Dialogs.IndexOf(dialog);
                Dialogs.Move(itemIndex, 0);
            }
            else
            {
                await QmunicateLoggerHolder.Log(QmunicateLogLevel.Warn, "The dialog wasn't found in DialogsManager. Reloading dialogs.");
                await ReloadDialogs();
            }
        }

        #endregion

        #region Private methods

        private void MessagesClientOnOnMessageReceived(object sender, Message message)
        {
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await UpdateDialog(message.DialogId, message.MessageText, message.DateTimeSent);

                var notificationMessage = message as NotificationMessage;
                if (notificationMessage != null)
                {
                    await HandleNotificationMessage(notificationMessage);
                }
            });
        }

        private async Task HandleNotificationMessage(NotificationMessage notificationMessage)
        {
            if (notificationMessage.NotificationType == NotificationTypes.GroupUpdate)
            {
                var updatedDialog = Dialogs.FirstOrDefault(d => d.Id == notificationMessage.DialogId);
                if (updatedDialog != null)
                {
                    if (!string.IsNullOrEmpty(notificationMessage.RoomPhoto))
                    {
                        updatedDialog.Photo = notificationMessage.RoomPhoto;
                        var imagesService = ServiceLocator.Locator.Get<IImageService>();
                        updatedDialog.Image = await imagesService.GetPublicImage(notificationMessage.RoomPhoto);
                    }

                    if (!string.IsNullOrEmpty(notificationMessage.RoomName))
                    {
                        updatedDialog.Name = notificationMessage.RoomName;
                    }

                }
            }
        }

        private string GetUserNameFromContacts(int userId)
        {
            var otherContact = quickbloxClient.MessagesClient.Contacts.FirstOrDefault(c => c.UserId == userId);
            if (otherContact != null)
                return otherContact.Name;

            return null;
        }

        private void LoadDialogImages(int? decodePixelWidth = null, int? decodePixelHeight = null)
        {
            var imagesService = ServiceLocator.Locator.Get<IImageService>();

            Parallel.ForEach(Dialogs.Where(d => d.DialogType == DialogType.Private), async (dialogVm, state) =>
            {
                if (dialogVm.PrivatePhotoId.HasValue)
                {
                    var imageBytes = await imagesService.GetPrivateImageBytes(dialogVm.PrivatePhotoId.Value);
                    CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => 
                        dialogVm.Image = await Helpers.CreateBitmapImage(imageBytes, decodePixelWidth, decodePixelHeight));
                }
            });
        }

        #endregion

    }
}
