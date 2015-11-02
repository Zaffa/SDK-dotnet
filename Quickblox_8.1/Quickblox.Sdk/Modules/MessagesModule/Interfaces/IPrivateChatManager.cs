﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quickblox.Sdk.GeneralDataModel.Models;

namespace Quickblox.Sdk.Modules.MessagesModule.Interfaces
{
    /// <summary>
    /// Manager interface for one-to-one private chats.
    /// </summary>
    public interface IPrivateChatManager
    {
        /// <summary>
        /// Event when other user is typing.
        /// </summary>
        event EventHandler OnIsTyping;

        /// <summary>
        /// Event when other user has stopped typing.
        /// </summary>
        event EventHandler OnPausedTyping;

        /// <summary>
        /// Event when a new message is received.
        /// </summary>
        event EventHandler<Message> OnMessageReceived;

        /// <summary>
        /// Sends a message to other user.
        /// </summary>
        /// <param name="message">Message text</param>
        /// <returns>Is operation successful</returns>
        bool SendMessage(string message);
        
        /// <summary>
        /// Adds other user to your roster, subsribes for his presence, and sends FriendRequest notification message.
        /// </summary>
        /// <param name="friendName">Other user name in your roster</param>
        /// <returns>Is operation successful</returns>
        bool AddToFriends(string friendName = null);

        /// <summary>
        /// Adds other user to your roster, accepts presence subscription request, and sends FriendAccepted notification message.
        /// </summary>
        /// <param name="friendName">Other user name in your roster</param>
        /// <returns>Is operation successful</returns>
        bool AcceptFriend(string friendName = null);

        /// <summary>
        /// Rejects subsription requests and sends FriendRejected notification message.
        /// </summary>
        /// <returns>Is operation successful</returns>
        bool RejectFriend();

        /// <summary>
        /// Sends FriendRemoved notification messages, removes other user from your roster and unsubscribes from presence.
        /// </summary>
        /// <returns>Is operation successful</returns>
        bool DeleteFromFriends();

        /// <summary>
        /// Notifies a user about a created group dialog.
        /// </summary>
        /// <param name="createdDialogId">Created group dialog ID</param>
        /// <returns>Is operation successful</returns>
        bool NotifyAboutGroupCreation(string createdDialogId);

        /// <summary>
        /// Notifies other user that you are typing a message.
        /// </summary>
        void NotifyIsTyping();

        /// <summary>
        /// Notifies other user that you've stopped typing a message.
        /// </summary>
        void NotifyPausedTyping();
    }
}
