using System;
using Newtonsoft.Json;
using Skybrud.Social.Google;

namespace MVCForum.Website.ViewModels.OAuth
{
    public class GoogleOAuthData
    {

        #region Private fields

        private GoogleService _service;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the ID of the authenticated user.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets the name of the authenticated user.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets the email of authenticated user
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// Gets the URL to the profile picture (avatar) of the authenticated user.
        /// </summary>
        [JsonProperty("avatar")]
        public string Avatar { get; set; }

        /// <summary>
        /// Gets the client ID of the app used to authenticate the user.
        /// </summary>
        [JsonProperty("clinetId")]
        public string ClientId { get; set; }

        /// <summary>
        /// Gets the client secret of the app used to authenticate the user.
        /// </summary>
        [JsonProperty("clientSecret")]
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets the refresh token of the authenticated user. This token will not expire unless the
        /// user deauthorizes the app via his/her Google account settings.
        /// </summary>
        [JsonProperty("refreshToken")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// Gets whether the OAuth data is valid - meaning that it has a client ID, client secret
        /// and a refresh token.
        /// </summary>
        [JsonIgnore]
        public bool IsValid
        {
            get
            {
                return (
                    !String.IsNullOrWhiteSpace(ClientId)
                    &&
                    !String.IsNullOrWhiteSpace(ClientSecret)
                    &&
                    !String.IsNullOrWhiteSpace(RefreshToken)
                );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes a new instance of the GoogleService class. Invoking this method will make a
        /// call to the Google API since we need to obtain an access token from the stored OAuth
        /// data.
        /// </summary>
        public GoogleService GetService()
        {
            return _service ?? (_service = GoogleService.CreateFromRefreshToken(ClientId, ClientSecret, RefreshToken));
        }

        /// <summary>
        /// Serializes the OAuth data into a JSON string.
        /// </summary>
        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// Deserializes the specified JSON string into an OAuth data object.
        /// </summary>
        /// <param name="str">The JSON string to be deserialized.</param>
        public static GoogleOAuthData Deserialize(string str)
        {
            return JsonConvert.DeserializeObject<GoogleOAuthData>(str);
        }

        #endregion

    }
}
