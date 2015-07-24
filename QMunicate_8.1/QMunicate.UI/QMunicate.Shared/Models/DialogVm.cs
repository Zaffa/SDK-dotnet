﻿using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Media;
using QMunicate.Core.Observable;
using Quickblox.Sdk.Builder;
using Quickblox.Sdk.Modules.ChatModule.Models;
using Quickblox.Sdk.Modules.MessagesModule.Models;
using Quickblox.Sdk.Modules.Models;

namespace QMunicate.Models
{
    public class DialogVm : ObservableObject
    {
        private string lastActivity;
        private DateTime? lastMessageSent;
        private string name;
        private ImageSource image;

        #region Ctor

        public DialogVm()
        {
        }

        protected DialogVm(Dialog dialog)
        {
            Id = dialog.Id;
            Name = dialog.Name;
            LastMessageSent = dialog.LastMessageDateSent.HasValue
                ? dialog.LastMessageDateSent.Value.ToDateTime()
                : (DateTime?) null;
            LastActivity = dialog.LastMessage;
            UnreadMessageCount = dialog.UnreadMessagesCount;
            OccupantIds = dialog.OccupantsIds;
            DialogType = dialog.Type;
            int imageUploadId;
            if (int.TryParse(dialog.Photo, out imageUploadId))
                ImageUploadId = imageUploadId;
        }

        #endregion

        #region Properties

        public string Id { get; set; }
        public int? ImageUploadId { get; set; }
        public int? UnreadMessageCount { get; set; }
        public IList<int> OccupantIds { get; set; }
        public DialogType DialogType { get; set; }

        public string LastActivity
        {
            get { return lastActivity; }
            set { Set(ref lastActivity, value); }
        }

        public DateTime? LastMessageSent
        {
            get { return lastMessageSent; }
            set { Set(ref lastMessageSent, value); }
        }

        public string Name
        {
            get { return name; }
            set { Set(ref name, value); }
        }

        public ImageSource Image
        {
            get { return image; }
            set { Set(ref image, value); }
        }

        #endregion

        #region Public methods

        public static DialogVm FromDialog(Dialog dialog)
        {
            return new DialogVm(dialog);
        }

        #endregion
    }
}
