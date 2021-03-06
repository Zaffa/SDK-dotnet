﻿using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Quickblox.Sdk.GeneralDataModel.Models;
using Quickblox.Sdk.Http;
using Quickblox.Sdk.Modules.ContentModule;
using Quickblox.Sdk.Modules.ContentModule.Models;
using Quickblox.Sdk.Modules.ContentModule.Requests;
using Quickblox.Sdk.Test.Helper;
using Quickblox.Sdk.Platform;

namespace Quickblox.Sdk.Test.Modules.ContentModule
{
    [TestClass]
    public class ContentClientTest
    {
        private QuickbloxClient client;

        [TestInitialize]
        public async Task TestInitialize()
        {
            this.client = new QuickbloxClient((int)GlobalConstant.ApplicationId, GlobalConstant.AuthorizationKey, GlobalConstant.AuthorizationSecret, GlobalConstant.ApiBaseEndPoint, GlobalConstant.ChatEndpoint);
            var sessionResponse = await this.client.AuthenticationClient.CreateSessionWithLoginAsync("Test654321", "Test12345",
                    deviceRequestRequest:
                        new DeviceRequest() {Platform = Quickblox.Sdk.GeneralDataModel.Models.Platform.windows_phone, Udid = Helpers.GetHardwareId()});
            client.Token = sessionResponse.Result.Session.Token;
        }

        [TestMethod]
        public async Task CreateFileInfoSuccessTest()
        {
            var settings = new CreateFileRequest()
            {
                Blob = new BlobRequest()
                {
                    Name = "museum.jpeg",
                }
            };

            var createFileInfoResponse = await this.client.ContentClient.CreateFileInfoAsync(settings);
            Assert.AreEqual(createFileInfoResponse.StatusCode, HttpStatusCode.Created);
        }

        [TestMethod]
        public async Task GetFilesInfoSuccessTest()
        {
            var getFilesResponse = await this.client.ContentClient.GetFilesAsync();
            Assert.AreEqual(getFilesResponse.StatusCode, HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task FileUploadSuccessTest()
        {
            var settings = new CreateFileRequest()
            {
                Blob = new BlobRequest()
                {
                    Name = String.Format("museum_{0}.jpeg", Guid.NewGuid()),
                }
            };

            var createFileInfoResponse = await this.client.ContentClient.CreateFileInfoAsync(settings);
            Assert.AreEqual(createFileInfoResponse.StatusCode, HttpStatusCode.Created);

            var uploadFileRequest = new UploadFileRequest();
            uploadFileRequest.BlobObjectAccess = createFileInfoResponse.Result.Blob.BlobObjectAccess;

            var uri = new Uri("ms-appx:///Modules/ContentModule/Assets/1.jpg");
            var storageFile = await StorageFile.GetFileFromApplicationUriAsync(uri);
            var stream = await storageFile.OpenReadAsync();
            var bytes = new byte[stream.Size];

            using (var dataReader = new DataReader(stream))
            {
                await dataReader.LoadAsync((uint)stream.Size);
                dataReader.ReadBytes(bytes);
            }

            uploadFileRequest.FileContent = new BytesContent()
            {
                Bytes = bytes,
                ContentType = "image/jpg",
            };

            var uploadFileResponse = await this.client.ContentClient.FileUploadAsync(uploadFileRequest);
            Assert.AreEqual(uploadFileResponse.StatusCode, HttpStatusCode.Created);

            var blobUploadCompleteRequest = new BlobUploadCompleteRequest();
            blobUploadCompleteRequest.BlobUploadSize = new BlobUploadSize() { Size = (uint) bytes.Length };
            var uploadFileCompleteResponse = await this.client.ContentClient.FileUploadCompleteAsync(createFileInfoResponse.Result.Blob.Id, blobUploadCompleteRequest);
            Assert.AreEqual(uploadFileCompleteResponse.StatusCode, HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task DeleteFilesSuccessTest()
        {
            var getFilesResponse = await this.client.ContentClient.GetFilesAsync();
            Assert.AreEqual(getFilesResponse.StatusCode, HttpStatusCode.OK);

            foreach (var item in getFilesResponse.Result.Items)
            {
                var deleteResponse = await client.ContentClient.DeleteFileAsync(item.Blob.Id);
                Assert.AreEqual(deleteResponse.StatusCode, HttpStatusCode.OK);
            }
        }

        [TestMethod]
        public async Task DownloadFileSuccessTest()
        {
            //var getFilesResponse = await this.client.ContentClient.GetFilesAsync();
            //Assert.AreEqual(getFilesResponse.StatusCode, HttpStatusCode.OK);
            //var firstFile = getFilesResponse.Result.Items.Last();

            var downloadFileResponse = await this.client.ContentClient.DownloadFileByUid("35f6c9cb777340d989ba01770bcc4e2000", false);
            Assert.AreEqual(downloadFileResponse.StatusCode, HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task DownloadFileFailTest()
        {
            var getFilesResponse = await this.client.ContentClient.GetFilesAsync();
            Assert.AreEqual(getFilesResponse.StatusCode, HttpStatusCode.OK);
            var firstFile = getFilesResponse.Result.Items.Last();

            var downloadFileResponse = await this.client.ContentClient.DownloadFileByUid(firstFile.Blob.Uid, false);
            Assert.IsNotNull(downloadFileResponse.Errors);
        }

        [TestMethod]
        public async Task UploadFileViaHelperTest()
        {
            var contentHelper = new ContentClientHelper(client);

            var uri = new Uri("ms-appx:///Modules/ContentModule/Assets/1.jpg");
            var storageFile = await StorageFile.GetFileFromApplicationUriAsync(uri);
            var stream = await storageFile.OpenReadAsync();
            var bytes = new byte[stream.Size];

            using (var dataReader = new DataReader(stream))
            {
                await dataReader.LoadAsync((uint)stream.Size);
                dataReader.ReadBytes(bytes);
            }

            var imageLink = await contentHelper.UploadPublicImage(bytes);

            Assert.IsNotNull(imageLink);
        }

        [TestMethod]
        public async Task DownloadByIdTest()
        {
            var contentHelper = new ContentClientHelper(client);

            var uri = new Uri("ms-appx:///Modules/ContentModule/Assets/1.jpg");
            var storageFile = await StorageFile.GetFileFromApplicationUriAsync(uri);
            var stream = await storageFile.OpenReadAsync();
            var bytes = new byte[stream.Size];

            using (var dataReader = new DataReader(stream))
            {
                await dataReader.LoadAsync((uint)stream.Size);
                dataReader.ReadBytes(bytes);
            }

            var uploadId = await contentHelper.UploadPrivateImage(bytes);
            Assert.IsNotNull(uploadId);

            var downloadedBytes = await client.ContentClient.DownloadFileById(uploadId.Value);
            Assert.AreEqual(bytes.Length, downloadedBytes.Result.Length);
        }
    }
}
