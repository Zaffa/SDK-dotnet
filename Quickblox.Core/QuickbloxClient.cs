﻿using System;
using System.Net;
using System.Threading.Tasks;
using Quickblox.Sdk.Builder;
using Quickblox.Sdk.Modules.ChatModule;
using Quickblox.Sdk.Core;
using Quickblox.Sdk.Core.Http;
using Quickblox.Sdk.Modules.AuthModule;
using Quickblox.Sdk.Modules.AuthModule.Response;
using Quickblox.Sdk.GeneralDataModel.Response;
using Quickblox.Sdk.Modules.NotificationModule;
using Quickblox.Sdk.Modules.UsersModule;

namespace Quickblox.Sdk
{
    /// <summary>
    /// QuickbloxClient class.
    /// </summary>
    public class QuickbloxClient
    {
        private readonly String baseUri;
        private readonly String accountKey;
        private Boolean isClientInitialized;

        #region Ctor

        /// <summary>
        /// Инициализирует новый экземпляр класса QuickbloxClient.
        /// </summary>
        /// <param name="baseUri">Начальный урл.</param>
        /// <param name="accountKey">Ключ аккаунта</param>
        public QuickbloxClient(String baseUri, String accountKey)
        {
            this.baseUri = baseUri;
            this.accountKey = accountKey;

            this.CoreClient = new AuthorizationClient(this);
            this.ChatClient = new ChatClient(this);
            this.UsersClient = new UsersClient(this);
            this.NotificationClient = new NotificationClient(this);
        }

        #endregion

        #region Events

        public event EventHandler<Boolean> ClientStatusChanged;

        #endregion

        #region Properties

        public AuthorizationClient CoreClient { get; private set; }

        public ChatClient ChatClient { get; private set; }

        public UsersClient UsersClient { get; private set; }

        public NotificationClient NotificationClient { get; private set; }

        /// <summary>
        /// Возварщает время последнего запроса в UTC.
        /// </summary>
        public DateTime LastRequest
        {
            get { return HttpBase.LastRequest; }
        }

        public Boolean IsClientInitialized
        {
            get { return this.isClientInitialized; }
            private set
            {
                this.isClientInitialized = value;
                this.OnStatusChanged();
            }
        }

        public string ApiEndPoint { get; private set; }

        public string Token { get; internal set; }

        #endregion

        #region Public Members
        
        public async Task InitializeClient()
        {
            while (!this.IsClientInitialized)
            {
                await this.GetAccountSettings();
            }
        }

        #region Internal

        internal void CheckIsInitialized()
        {
            if (!this.IsClientInitialized)
                throw new NotInitializedException();
        }

        #endregion

        #endregion

        #region Private

        private async Task GetAccountSettings()
        {
                var accountResponse =
                    await HttpService.GetAsync<AccountResponse, BaseRequestSettings>(this.baseUri, QuickbloxMethods.AccountMethod,
                        new JsonSerializer() { KnownTypes = { typeof(AccountResponse) } }, null,
                        RequestHeadersBuilder.GetDefaultHeaders().GetHeaderWithQbAccountKey(this.accountKey));
            if (accountResponse.StatusCode == HttpStatusCode.OK)
            {
                this.ApiEndPoint = accountResponse.Result.ApiEndPoint;
                this.IsClientInitialized = true;
            }
        }

        private void OnStatusChanged()
        {
            var handler = this.ClientStatusChanged;
            if (handler != null)
            {
                handler.Invoke(this, this.isClientInitialized);
            }
        }

        #endregion
    }

    public class NotInitializedException : Exception
    {
    }
}
