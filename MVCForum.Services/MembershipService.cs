using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Security;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Events;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Utilities;

namespace MVCForum.Services
{
    public partial class MembershipService : IMembershipService
    {
        private const int MaxHoursToResetPassword = 48;

        private readonly IEmailService _emailService;
        private readonly IMembershipRepository _membershipRepository;
        private readonly IPostRepository _postRepository;
        private readonly IPollVoteRepository _pollVoteRepository;
        private readonly IPollAnswerRepository _pollAnswerRepository;
        private readonly IFavouriteRepository _favouriteRepository;
        private readonly ISettingsRepository _settingsRepository;
        private readonly IPollRepository _pollRepository;
        private readonly ITopicRepository _topicRepository;
        private readonly IActivityService _activityService;
        private readonly ILocalizationService _localizationService;
        private readonly IPrivateMessageService _privateMessageService;
        private readonly IMembershipUserPointsService _membershipUserPointsService;
        private readonly ITopicNotificationService _topicNotificationService;
        private readonly IUploadedFileService _uploadedFileService;
        private readonly IVoteService _voteService;
        private readonly IBadgeService _badgeService;
        private readonly ICategoryNotificationService _categoryNotificationService;
        private readonly ILoggingService _loggingService;
        private readonly ICategoryService _categoryService;

        private LoginAttemptStatus _lastLoginStatus = LoginAttemptStatus.LoginSuccessful;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="membershipRepository"> </param>
        /// <param name="settingsRepository"> </param>
        /// <param name="emailService"> </param>
        /// <param name="localizationService"> </param>
        /// <param name="activityService"> </param>
        /// <param name="privateMessageService"> </param>
        /// <param name="membershipUserPointsService"> </param>
        /// <param name="topicNotificationService"> </param>
        /// <param name="voteService"> </param>
        /// <param name="badgeService"> </param>
        /// <param name="categoryNotificationService"> </param>
        /// <param name="loggingService"></param>
        /// <param name="uploadedFileService"></param>
        /// <param name="postRepository"></param>
        /// <param name="pollVoteRepository"></param>
        /// <param name="pollAnswerRepository"></param>
        /// <param name="pollRepository"></param>
        /// <param name="topicRepository"></param>
        /// <param name="favouriteRepository"></param>
        /// <param name="categoryService"></param>
        public MembershipService(IMembershipRepository membershipRepository, ISettingsRepository settingsRepository,
            IEmailService emailService, ILocalizationService localizationService, IActivityService activityService,
            IPrivateMessageService privateMessageService, IMembershipUserPointsService membershipUserPointsService,
            ITopicNotificationService topicNotificationService, IVoteService voteService, IBadgeService badgeService,
            ICategoryNotificationService categoryNotificationService, ILoggingService loggingService, IUploadedFileService uploadedFileService,
            IPostRepository postRepository, IPollVoteRepository pollVoteRepository, IPollAnswerRepository pollAnswerRepository,
            IPollRepository pollRepository, ITopicRepository topicRepository, IFavouriteRepository favouriteRepository, 
            ICategoryService categoryService)
        {
            _membershipRepository = membershipRepository;
            _settingsRepository = settingsRepository;
            _emailService = emailService;
            _localizationService = localizationService;
            _activityService = activityService;
            _privateMessageService = privateMessageService;
            _membershipUserPointsService = membershipUserPointsService;
            _topicNotificationService = topicNotificationService;
            _voteService = voteService;
            _badgeService = badgeService;
            _categoryNotificationService = categoryNotificationService;
            _loggingService = loggingService;
            _uploadedFileService = uploadedFileService;
            _postRepository = postRepository;
            _pollVoteRepository = pollVoteRepository;
            _pollAnswerRepository = pollAnswerRepository;
            _pollRepository = pollRepository;
            _topicRepository = topicRepository;
            _favouriteRepository = favouriteRepository;
            _categoryService = categoryService;
        }


        public MembershipUser SanitizeUser(MembershipUser membershipUser)
        {
            membershipUser.Avatar = StringUtils.SafePlainText(membershipUser.Avatar);
            membershipUser.Comment = StringUtils.SafePlainText(membershipUser.Comment);
            membershipUser.Email = StringUtils.SafePlainText(membershipUser.Email);
            membershipUser.Password = StringUtils.SafePlainText(membershipUser.Password);
            membershipUser.PasswordAnswer = StringUtils.SafePlainText(membershipUser.PasswordAnswer);
            membershipUser.PasswordQuestion = StringUtils.SafePlainText(membershipUser.PasswordQuestion);
            membershipUser.Signature = StringUtils.GetSafeHtml(membershipUser.Signature, true);
            membershipUser.Twitter = StringUtils.SafePlainText(membershipUser.Twitter);
            membershipUser.UserName = StringUtils.SafePlainText(membershipUser.UserName);
            membershipUser.Website = StringUtils.SafePlainText(membershipUser.Website);
            return membershipUser;
        }

        #region Status Codes
        public string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return _localizationService.GetResourceString("Members.Errors.DuplicateUserName");

                case MembershipCreateStatus.DuplicateEmail:
                    return _localizationService.GetResourceString("Members.Errors.DuplicateEmail");

                case MembershipCreateStatus.InvalidPassword:
                    return _localizationService.GetResourceString("Members.Errors.InvalidPassword");

                case MembershipCreateStatus.InvalidEmail:
                    return _localizationService.GetResourceString("Members.Errors.InvalidEmail");

                case MembershipCreateStatus.InvalidAnswer:
                    return _localizationService.GetResourceString("Members.Errors.InvalidAnswer");

                case MembershipCreateStatus.InvalidQuestion:
                    return _localizationService.GetResourceString("Members.Errors.InvalidQuestion");

                case MembershipCreateStatus.InvalidUserName:
                    return _localizationService.GetResourceString("Members.Errors.InvalidUserName");

                case MembershipCreateStatus.ProviderError:
                    return _localizationService.GetResourceString("Members.Errors.ProviderError");

                case MembershipCreateStatus.UserRejected:
                    return _localizationService.GetResourceString("Members.Errors.UserRejected");

                default:
                    return _localizationService.GetResourceString("Members.Errors.Unknown");
            }
        }
        #endregion

        /// <summary>
        /// Return last login status
        /// </summary>
        public LoginAttemptStatus LastLoginStatus
        {
            get { return _lastLoginStatus; }
        }

        /// <summary>
        /// Validate a user by password
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="maxInvalidPasswordAttempts"> </param>
        /// <returns></returns>
        public bool ValidateUser(string userName, string password, int maxInvalidPasswordAttempts)
        {
            userName = StringUtils.SafePlainText(userName);
            password = StringUtils.SafePlainText(password);

            _lastLoginStatus = LoginAttemptStatus.LoginSuccessful;

            var user = _membershipRepository.GetUser(userName);

            if (user == null)
            {
                _lastLoginStatus = LoginAttemptStatus.UserNotFound;
                return false;
            }

            if (user.IsLockedOut)
            {
                _lastLoginStatus = LoginAttemptStatus.UserLockedOut;
                return false;
            }

            if (!user.IsApproved)
            {
                _lastLoginStatus = LoginAttemptStatus.UserNotApproved;
                return false;
            }

            var allowedPasswordAttempts = maxInvalidPasswordAttempts;
            if (user.FailedPasswordAttemptCount >= allowedPasswordAttempts)
            {
                _lastLoginStatus = LoginAttemptStatus.PasswordAttemptsExceeded;
                return false;
            }

            var salt = user.PasswordSalt;
            var hash = StringUtils.GenerateSaltedHash(password, salt);
            var passwordMatches = hash == user.Password;

            user.FailedPasswordAttemptCount = passwordMatches ? 0 : user.FailedPasswordAttemptCount + 1;

            if (user.FailedPasswordAttemptCount >= allowedPasswordAttempts)
            {
                user.IsLockedOut = true;
                user.LastLockoutDate = DateTime.UtcNow;
            }

            if (!passwordMatches)
            {
                _lastLoginStatus = LoginAttemptStatus.PasswordIncorrect;
                return false;
            }

            return _lastLoginStatus == LoginAttemptStatus.LoginSuccessful;
        }

        /// <summary>
        /// Creates a new, unsaved user, with default (empty) values
        /// </summary>
        /// <returns></returns>
        public MembershipUser CreateEmptyUser()
        {
            var now = DateTime.UtcNow;

            return new MembershipUser
                       {
                           UserName = string.Empty,
                           Password = string.Empty,
                           Email = string.Empty,
                           PasswordQuestion = string.Empty,
                           PasswordAnswer = string.Empty,
                           CreateDate = now,
                           FailedPasswordAnswerAttempt = 0,
                           FailedPasswordAttemptCount = 0,
                           LastLockoutDate = (DateTime)SqlDateTime.MinValue,
                           LastPasswordChangedDate = now,
                           IsApproved = false,
                           IsLockedOut = false,
                           LastLoginDate = (DateTime)SqlDateTime.MinValue,
                       };
        }

        /// <summary>
        /// Create new user
        /// </summary>
        /// <param name="newUser"></param>
        /// <returns></returns>
        public MembershipCreateStatus CreateUser(MembershipUser newUser)
        {
            newUser = SanitizeUser(newUser);
            var settings = _settingsRepository.GetSettings(true);

            var status = MembershipCreateStatus.Success;

            var e = new RegisterUserEventArgs { User = newUser };
            EventManager.Instance.FireBeforeRegisterUser(this, e);

            if (e.Cancel)
            {
                status = e.CreateStatus;
            }
            else
            {
                if (string.IsNullOrEmpty(newUser.UserName))
                {
                    status = MembershipCreateStatus.InvalidUserName;
                }

                // get by username
                if (_membershipRepository.GetUser(newUser.UserName) != null)
                {
                    status = MembershipCreateStatus.DuplicateUserName;
                }

                // Add get by email address
                if (_membershipRepository.GetUserByEmail(newUser.Email) != null)
                {
                    status = MembershipCreateStatus.DuplicateEmail;
                }

                if (string.IsNullOrEmpty(newUser.Password))
                {
                    status = MembershipCreateStatus.InvalidPassword;
                }

                if (status == MembershipCreateStatus.Success)
                {
                    // Hash the password
                    var salt = StringUtils.CreateSalt(AppConstants.SaltSize);
                    var hash = StringUtils.GenerateSaltedHash(newUser.Password, salt);
                    newUser.Password = hash;
                    newUser.PasswordSalt = salt;

                    newUser.Roles = new List<MembershipRole> { settings.NewMemberStartingRole };

                    // Set dates
                    newUser.CreateDate = newUser.LastPasswordChangedDate = DateTime.UtcNow;
                    newUser.LastLockoutDate = (DateTime)SqlDateTime.MinValue;
                    newUser.LastLoginDate = DateTime.UtcNow;

                    newUser.IsApproved = !settings.ManuallyAuthoriseNewMembers;
                    newUser.IsLockedOut = false;

                    // url generator
                    newUser.Slug = ServiceHelpers.GenerateSlug(newUser.UserName, _membershipRepository.GetUserBySlugLike(ServiceHelpers.CreateUrl(newUser.UserName)), null);

                    try
                    {
                        _membershipRepository.Add(newUser);

                        if (settings.EmailAdminOnNewMemberSignUp)
                        {
                            var sb = new StringBuilder();
                            sb.AppendFormat("<p>{0}</p>", string.Format(_localizationService.GetResourceString("Members.NewMemberRegistered"), settings.ForumName, settings.ForumUrl));
                            sb.AppendFormat("<p>{0} - {1}</p>", newUser.UserName, newUser.Email);
                            var email = new Email
                                            {
                                                EmailTo = settings.AdminEmailAddress,
                                                NameTo = _localizationService.GetResourceString("Members.Admin"),
                                                Subject = _localizationService.GetResourceString("Members.NewMemberSubject")
                                            };
                            email.Body = _emailService.EmailTemplate(email.NameTo, sb.ToString());
                            _emailService.SendMail(email);
                        }

                        _activityService.MemberJoined(newUser);
                        EventManager.Instance.FireAfterRegisterUser(this,
                                                                    new RegisterUserEventArgs { User = newUser });
                    }
                    catch (Exception)
                    {
                        status = MembershipCreateStatus.UserRejected;
                    }
                }
            }

            return status;
        }

        /// <summary>
        /// Get a user by username
        /// </summary>
        /// <param name="username"></param>
        /// <param name="removeTracking"></param>
        /// <returns></returns>
        public MembershipUser GetUser(string username, bool removeTracking = false)
        {
            var member = _membershipRepository.GetUser(username);

            // Do a check to log out the user if they are logged in and have been deleted
            if (member == null && HttpContext.Current.User.Identity.Name == username)
            {
                // Member is null so doesn't exist, yet they are logged in with that username - Log them out
                FormsAuthentication.SignOut();
            }

            return member;
        }

        /// <summary>
        /// Get a user by email address
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public MembershipUser GetUserByEmail(string email)
        {
            email = StringUtils.SafePlainText(email);
            return _membershipRepository.GetUserByEmail(email);
        }

        /// <summary>
        /// Get a user by slug
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public MembershipUser GetUserBySlug(string slug)
        {
            slug = StringUtils.GetSafeHtml(slug);
            return _membershipRepository.GetUserBySlug(slug);
        }

        /// <summary>
        /// Gets a user by their facebook id
        /// </summary>
        /// <param name="facebookId"></param>
        /// <returns></returns>
        public MembershipUser GetUserByFacebookId(long facebookId)
        {
            return _membershipRepository.GetUserByFacebookId(facebookId);
        }

        public MembershipUser GetUserByTwitterId(string twitterId)
        {
            return _membershipRepository.GetUserByTwitterId(twitterId);
        }

        public MembershipUser GetUserByGoogleId(string googleId)
        {
            return _membershipRepository.GetUserByGoogleId(googleId);
        }

        /// <summary>
        /// Get users by openid token
        /// </summary>
        /// <param name="openId"></param>
        /// <returns></returns>
        public MembershipUser GetUserByOpenIdToken(string openId)
        {
            openId = StringUtils.GetSafeHtml(openId);
            return _membershipRepository.GetUserByOpenIdToken(openId);
        }

        /// <summary>
        /// Get users from a list of Id's
        /// </summary>
        /// <param name="guids"></param>
        /// <returns></returns>
        public IList<MembershipUser> GetUsersById(List<Guid> guids)
        {
            return _membershipRepository.GetUsersById(guids);
        }

        /// <summary>
        /// Get by posts and date
        /// </summary>
        /// <param name="amoutOfDaysSinceRegistered"></param>
        /// <param name="amoutOfPosts"></param>
        /// <returns></returns>
        public IList<MembershipUser> GetUsersByDaysPostsPoints(int amoutOfDaysSinceRegistered, int amoutOfPosts)
        {
            return _membershipRepository.GetUsersByDaysPostsPoints(amoutOfDaysSinceRegistered, amoutOfPosts);
        }


        /// <summary>
        /// Return the roles found for this username
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public string[] GetRolesForUser(string username)
        {
            username = StringUtils.SafePlainText(username);
            var roles = new List<string>();
            var user = _membershipRepository.GetUser(username);

            if (user != null)
            {
                roles.AddRange(user.Roles.Select(role => role.RoleName));
            }

            return roles.ToArray();
        }

        /// <summary>
        /// Change the user's password
        /// </summary>
        /// <param name="user"> </param>
        /// <param name="oldPassword"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        public bool ChangePassword(MembershipUser user, string oldPassword, string newPassword)
        {
            oldPassword = StringUtils.SafePlainText(oldPassword);
            newPassword = StringUtils.SafePlainText(newPassword);

            //n3oCacheHelper.Clear(user.UserName);
            var existingUser = _membershipRepository.Get(user.Id);
            var salt = existingUser.PasswordSalt;
            var oldHash = StringUtils.GenerateSaltedHash(oldPassword, salt);

            if (oldHash != existingUser.Password)
            {
                // Old password is wrong - do not allow update
                return false;
            }

            // Cleared to go ahead with new password
            salt = StringUtils.CreateSalt(AppConstants.SaltSize);
            var newHash = StringUtils.GenerateSaltedHash(newPassword, salt);

            existingUser.Password = newHash;
            existingUser.PasswordSalt = salt;
            existingUser.LastPasswordChangedDate = DateTime.UtcNow;

            return true;
        }

        /// <summary>
        /// Reset a users password
        /// </summary>
        /// <param name="user"></param>
        /// <param name="newPassword"> </param>
        /// <returns></returns>
        public bool ResetPassword(MembershipUser user, string newPassword)
        {
            var existingUser = _membershipRepository.Get(user.Id);

            var salt = StringUtils.CreateSalt(AppConstants.SaltSize);
            var newHash = StringUtils.GenerateSaltedHash(newPassword, salt);

            existingUser.Password = newHash;
            existingUser.PasswordSalt = salt;
            existingUser.LastPasswordChangedDate = DateTime.UtcNow;

            return true;
        }

        /// <summary>
        /// Get all members
        /// </summary>
        /// <returns></returns>
        public IList<MembershipUser> GetAll()
        {
            return _membershipRepository.GetAll();
        }

        public PagedList<MembershipUser> GetAll(int pageIndex, int pageSize)
        {
            return _membershipRepository.GetAll(pageIndex, pageSize);
        }

        public PagedList<MembershipUser> SearchMembers(string search, int pageIndex, int pageSize)
        {
            return _membershipRepository.SearchMembers(StringUtils.SafePlainText(search), pageIndex, pageSize);
        }

        public IList<MembershipUser> SearchMembers(string username, int amount)
        {
            return _membershipRepository.SearchMembers(StringUtils.SafePlainText(username), amount);
        }

        public IList<MembershipUser> GetActiveMembers()
        {
            return _membershipRepository.GetActiveMembers();
        }

        /// <summary>
        /// Get user by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public MembershipUser GetUser(Guid id)
        {
            return _membershipRepository.Get(id);
        }

        /// <summary>
        /// Delete a member
        /// </summary>
        /// <param name="user"></param>
        /// <param name="unitOfWork"></param>
        public bool Delete(MembershipUser user, IUnitOfWork unitOfWork)
        {
            try
            {
                // Scrub all member data
                ScrubUsers(user, unitOfWork);

                // Just clear the roles, don't delete them
                user.Roles.Clear();

                // Now delete the member
                _membershipRepository.Delete(user);

                return true;
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex);
            }
            return false;
        }

        public IList<MembershipUser> GetLatestUsers(int amountToTake)
        {
            return _membershipRepository.GetLatestUsers(amountToTake);
        }

        public IList<MembershipUser> GetLowestPointUsers(int amountToTake)
        {
            return _membershipRepository.GetLowestPointUsers(amountToTake);
        }

        public int MemberCount()
        {
            return _membershipRepository.MemberCount();
        }

        /// <summary>
        /// Save user (does NOT update password data)
        /// </summary>
        /// <param name="user"></param>
        public void ProfileUpdated(MembershipUser user)
        {
            var e = new UpdateProfileEventArgs { User = user };
            EventManager.Instance.FireBeforeProfileUpdated(this, e);

            if (!e.Cancel)
            {
                EventManager.Instance.FireAfterProfileUpdated(this, new UpdateProfileEventArgs { User = user });
                _activityService.ProfileUpdated(user);
            }
        }

        /// <summary>
        /// Unlock a user
        /// </summary>
        /// <param name="username"></param>
        /// <param name="resetPasswordAttempts">If true, also reset password attempts to zero</param>
        public void UnlockUser(string username, bool resetPasswordAttempts)
        {
            {
                var user = GetUser(username);

                if (user == null)
                {
                    throw new ApplicationException(_localizationService.GetResourceString("Members.CantUnlock"));
                }

                var existingUser = _membershipRepository.Get(user.Id);

                user.IsLockedOut = false;
                user.Roles = existingUser.Roles;
                user.Votes = existingUser.Votes;
                user.Password = existingUser.Password;
                user.PasswordSalt = existingUser.PasswordSalt;

                if (resetPasswordAttempts)
                {
                    user.FailedPasswordAnswerAttempt = 0;
                }
            }
        }

        /// <summary>
        /// Convert all users into CSV format (e.g. for export)
        /// </summary>
        /// <returns></returns>
        public string ToCsv()
        {
            var csv = new StringBuilder();

            foreach (var user in _membershipRepository.GetAll())
            {
                csv.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7}", user.UserName, user.Email, user.CreateDate, user.Age,
                    user.Location, user.Website, user.Facebook, user.Signature);
                csv.AppendLine();
            }

            return csv.ToString();
        }

        /// <summary>
        /// Extract users from CSV format and import them
        /// </summary>
        /// <returns></returns>
        public CsvReport FromCsv(List<string> allLines)
        {
            var usersProcessed = new List<string>();
            var commaSeparator = new[] { ',' };
            var report = new CsvReport();

            if (allLines == null || allLines.Count == 0)
            {
                report.Errors.Add(new CsvErrorWarning
                {
                    ErrorWarningType = CsvErrorWarningType.BadDataFormat,
                    Message = "No users found."
                });
                return report;
            }

            var lineCounter = 0;
            foreach (var line in allLines)
            {
                try
                {
                    lineCounter++;

                    // Each line is made up of n items in a predefined order

                    var values = line.Split(commaSeparator);

                    if (values.Length < 2)
                    {
                        report.Errors.Add(new CsvErrorWarning
                        {
                            ErrorWarningType = CsvErrorWarningType.MissingKeyOrValue,
                            Message = string.Format("Line {0}: insufficient values supplied.", lineCounter)
                        });

                        continue;
                    }

                    var userName = values[0];

                    if (userName.IsNullEmpty())
                    {
                        report.Errors.Add(new CsvErrorWarning
                        {
                            ErrorWarningType = CsvErrorWarningType.MissingKeyOrValue,
                            Message = string.Format("Line {0}: no username supplied.", lineCounter)
                        });

                        continue;
                    }

                    var email = values[1];
                    if (email.IsNullEmpty())
                    {
                        report.Errors.Add(new CsvErrorWarning
                        {
                            ErrorWarningType = CsvErrorWarningType.MissingKeyOrValue,
                            Message = string.Format("Line {0}: no email supplied.", lineCounter)
                        });

                        continue;
                    }

                    // get the user
                    var userToImport = _membershipRepository.GetUser(userName);

                    if (userToImport != null)
                    {
                        report.Errors.Add(new CsvErrorWarning
                        {
                            ErrorWarningType = CsvErrorWarningType.AlreadyExists,
                            Message = string.Format("Line {0}: user already exists in forum.", lineCounter)
                        });

                        continue;
                    }

                    if (usersProcessed.Contains(userName))
                    {
                        report.Errors.Add(new CsvErrorWarning
                        {
                            ErrorWarningType = CsvErrorWarningType.AlreadyExists,
                            Message = string.Format("Line {0}: user already exists in import file.", lineCounter)
                        });

                        continue;
                    }

                    usersProcessed.Add(userName);

                    userToImport = CreateEmptyUser();
                    userToImport.UserName = userName;
                    userToImport.Slug = ServiceHelpers.GenerateSlug(userToImport.UserName, _membershipRepository.GetUserBySlugLike(ServiceHelpers.CreateUrl(userToImport.UserName)), userToImport.Slug);
                    userToImport.Email = email;
                    userToImport.IsApproved = true;
                    userToImport.PasswordSalt = StringUtils.CreateSalt(AppConstants.SaltSize);

                    string createDateStr = null;
                    if (values.Length >= 3)
                    {
                        createDateStr = values[2];
                    }
                    userToImport.CreateDate = createDateStr.IsNullEmpty() ? DateTime.UtcNow : DateTime.Parse(createDateStr);

                    if (values.Length >= 4)
                    {
                        userToImport.Age = Int32.Parse(values[3]);
                    }
                    if (values.Length >= 5)
                    {
                        userToImport.Location = values[4];
                    }
                    if (values.Length >= 6)
                    {
                        userToImport.Website = values[5];
                    }
                    if (values.Length >= 7)
                    {
                        userToImport.Facebook = values[6];
                    }
                    if (values.Length >= 8)
                    {
                        userToImport.Signature = values[7];
                    }

                    _membershipRepository.Add(userToImport);
                }
                catch (Exception ex)
                {
                    report.Errors.Add(new CsvErrorWarning { ErrorWarningType = CsvErrorWarningType.GeneralError, Message = ex.Message });
                }
            }

            return report;
        }

        public void ScrubUsers(MembershipUser user, IUnitOfWork unitOfWork)
        {
            // PROFILE
            user.Website = string.Empty;
            user.Twitter = string.Empty;
            user.Facebook = string.Empty;
            user.Avatar = string.Empty;
            user.Signature = string.Empty;

            // User Votes
            if (user.Votes != null)
            {
                var votesToDelete = new List<Vote>();
                votesToDelete.AddRange(user.Votes);
                foreach (var d in votesToDelete)
                {
                    _voteService.Delete(d);
                }
                user.Votes.Clear();
            }

            // User Badges
            if (user.Badges != null)
            {
                var toDelete = new List<Badge>();
                toDelete.AddRange(user.Badges);
                foreach (var obj in toDelete)
                {
                    _badgeService.Delete(obj);
                }
                user.Badges.Clear();
            }

            // User badge time checks
            if (user.BadgeTypesTimeLastChecked != null)
            {
                var toDelete = new List<BadgeTypeTimeLastChecked>();
                toDelete.AddRange(user.BadgeTypesTimeLastChecked);
                foreach (var obj in toDelete)
                {
                    _badgeService.DeleteTimeLastChecked(obj);
                }
                user.BadgeTypesTimeLastChecked.Clear();
            }

            // User category notifications
            if (user.CategoryNotifications != null)
            {
                var toDelete = new List<CategoryNotification>();
                toDelete.AddRange(user.CategoryNotifications);
                foreach (var obj in toDelete)
                {
                    _categoryNotificationService.Delete(obj);
                }
                user.CategoryNotifications.Clear();
            }

            // User PM Received
            var pmUpdate = false;
            if (user.PrivateMessagesReceived != null)
            {
                pmUpdate = true;
                var toDelete = new List<PrivateMessage>();
                toDelete.AddRange(user.PrivateMessagesReceived);
                foreach (var obj in toDelete)
                {
                    _privateMessageService.DeleteMessage(obj);
                }
                user.PrivateMessagesReceived.Clear();
            }

            // User PM Sent
            if (user.PrivateMessagesSent != null)
            {
                pmUpdate = true;
                var toDelete = new List<PrivateMessage>();
                toDelete.AddRange(user.PrivateMessagesSent);
                foreach (var obj in toDelete)
                {
                    _privateMessageService.DeleteMessage(obj);
                }
                user.PrivateMessagesSent.Clear();
            }

            if (pmUpdate)
            {
                unitOfWork.SaveChanges();
            }

            // User Favourites
            if (user.Favourites != null)
            {
                var toDelete = new List<Favourite>();
                toDelete.AddRange(user.Favourites);
                foreach (var obj in toDelete)
                {
                    _favouriteRepository.Delete(obj);
                }
                user.Favourites.Clear();
            }

            if (user.TopicNotifications != null)
            {
                var notificationsToDelete = new List<TopicNotification>();
                notificationsToDelete.AddRange(user.TopicNotifications);
                foreach (var topicNotification in notificationsToDelete)
                {
                    _topicNotificationService.Delete(topicNotification);
                }
                user.TopicNotifications.Clear();
            }

            // Also clear their points
            var userPoints = user.Points;
            if (userPoints.Any())
            {
                var pointsList = new List<MembershipUserPoints>();
                pointsList.AddRange(userPoints);
                foreach (var point in pointsList)
                {
                    point.User = null;
                    _membershipUserPointsService.Delete(point);
                }
                user.Points.Clear();
            }

            // Now clear all activities for this user
            var usersActivities = _activityService.GetDataFieldByGuid(user.Id);
            _activityService.Delete(usersActivities.ToList());

            // Also clear their poll votes
            var userPollVotes = user.PollVotes;
            if (userPollVotes.Any())
            {
                var pollList = new List<PollVote>();
                pollList.AddRange(userPollVotes);
                foreach (var vote in pollList)
                {
                    vote.User = null;
                    _pollVoteRepository.Delete(vote);
                }
                user.PollVotes.Clear();
            }

            unitOfWork.SaveChanges();


            // Also clear their polls
            var userPolls = user.Polls;
            if (userPolls.Any())
            {
                var polls = new List<Poll>();
                polls.AddRange(userPolls);
                foreach (var poll in polls)
                {
                    //Delete the poll answers
                    var pollAnswers = poll.PollAnswers;
                    if (pollAnswers.Any())
                    {
                        var pollAnswersList = new List<PollAnswer>();
                        pollAnswersList.AddRange(pollAnswers);
                        foreach (var answer in pollAnswersList)
                        {
                            answer.Poll = null;
                            _pollAnswerRepository.Delete(answer);
                        }
                    }

                    poll.PollAnswers.Clear();
                    poll.User = null;
                    _pollRepository.Delete(poll);
                }
                user.Polls.Clear();
            }

            unitOfWork.SaveChanges();

            // ######### POSTS TOPICS ########

            // Delete all topics first
            var topics = user.Topics;
            if (topics != null && topics.Any())
            {
                var topicList = new List<Topic>();
                topicList.AddRange(topics);
                foreach (var topic in topicList)
                {
                    topic.LastPost = null;
                    topic.Posts.Clear();
                    topic.Tags.Clear();
                    _topicRepository.Delete(topic);
                }
                user.Topics.Clear();
                unitOfWork.SaveChanges();
            }

            // Now sorts Last Posts on topics and delete all the users posts
            var posts = user.Posts;
            if (posts != null && posts.Any())
            {
                var postIds = posts.Select(x => x.Id).ToList();

                // Get all categories
                var allCategories = _categoryService.GetAll();

                // Need to see if any of these are last posts on Topics
                // If so, need to swap out last post
                var lastPostTopics = _topicRepository.GetTopicsByLastPost(postIds, allCategories.ToList());
                foreach (var topic in lastPostTopics.Where(x => x.User.Id != user.Id))
                {
                    var lastPost = topic.Posts.Where(x => !postIds.Contains(x.Id)).OrderByDescending(x => x.DateCreated).FirstOrDefault();
                    topic.LastPost = lastPost;
                }

                unitOfWork.SaveChanges();

                user.UploadedFiles.Clear();

                // Delete all posts

                var postList = new List<Post>();
                postList.AddRange(posts);
                foreach (var post in postList)
                {
                    if (post.Files != null)
                    {
                        var files = post.Files;
                        var filesList = new List<UploadedFile>();
                        filesList.AddRange(files);
                        foreach (var file in filesList)
                        {
                            // store the file path as we'll need it to delete on the file system
                            var filePath = file.FilePath;

                            // Now delete it
                            _uploadedFileService.Delete(file);

                            // And finally delete from the file system
                            System.IO.File.Delete(HostingEnvironment.MapPath(filePath));
                        }
                        post.Files.Clear();
                    }
                    _postRepository.Delete(post);
                }
                user.Posts.Clear();

                unitOfWork.SaveChanges();
            }
        }

        /// <summary>
        /// Update the user record with a newly generated password reset security token and timestamp
        /// </summary>
        public bool UpdatePasswordResetToken(MembershipUser user)
        {
            var existingUser = GetUser(user.Id);
            if (existingUser == null)
            {
                return false;   
            }
            existingUser.PasswordResetToken = CreatePasswordResetToken();
            existingUser.PasswordResetTokenCreatedAt = DateTime.UtcNow;
            return true;
        }

        /// <summary>
        /// Remove the password reset security token and timestamp from the user record
        /// </summary>
        public bool ClearPasswordResetToken(MembershipUser user)
        {
            var existingUser = GetUser(user.Id);
            if (existingUser == null)
            {
                return false;   
            }
            existingUser.PasswordResetToken = null;
            existingUser.PasswordResetTokenCreatedAt = null;
            return true;
        }

        /// <summary>
        /// To be valid:
        /// - The user record must contain a password reset token
        /// - The given token must match the token in the user record
        /// - The token timestamp must be less than 24 hours ago
        /// </summary>
        public bool IsPasswordResetTokenValid(MembershipUser user, string token)
        {
            var existingUser = GetUser(user.Id);
            if (existingUser == null || string.IsNullOrEmpty(existingUser.PasswordResetToken))
            {
                return false;
            }
            // A security token must have an expiry date
            if (existingUser.PasswordResetTokenCreatedAt == null)
            {
                return false;   
            }
            // The security token is only valid for 48 hours
            if ((DateTime.UtcNow - existingUser.PasswordResetTokenCreatedAt.Value).TotalHours >= MaxHoursToResetPassword)
            {
                return false;   
            }
            return existingUser.PasswordResetToken == token;
        }

        /// <summary>
        /// Generate a password reset token, a guid is sufficient
        /// </summary>
        private static string CreatePasswordResetToken()
        {
            return Guid.NewGuid().ToString().ToLower().Replace("-", "");
        }
    }
}
