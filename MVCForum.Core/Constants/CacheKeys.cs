namespace MvcForum.Core.Constants
{
    public static class CacheKeys
    {
        public const string Domain = "ThisDomain";

        public static class Topic
        {
            public const string StartsWith = "Topic.";
        }

        public static class TopicTag
        {
            public const string StartsWith = "TopicTag.";
        }

        public static class Post
        {
            public const string StartsWith = "Post.";
        }

        public static class PrivateMessage
        {
            public const string StartsWith = "PrivateMessage.";
        }

        public static class PostEdit
        {
            public const string StartsWith = "PostEdit.";
        }

        public static class Category
        {
            public const string StartsWith = "Category.";
        }

        public static class Member
        {
            public const string StartsWith = "Member.";
        }

        public static class Role
        {
            public const string StartsWith = "Role.";
        }

        public static class Permission
        {
            public const string StartsWith = "Permission.";
        }

        public static class Activity
        {
            public const string StartsWith = "Activity.";
        }

        public static class Badge
        {
            public const string StartsWith = "Badge.";
        }

        public static class BannedEmail
        {
            public const string StartsWith = "BannedEmail.";
        }

        public static class Vote
        {
            public const string StartsWith = "Vote.";
        }

        public static class BannedWord
        {
            public const string StartsWith = "BannedWord.";
        }

        public static class CategoryNotification
        {
            public const string StartsWith = "CategoryNotification.";
        }

        public static class CategoryPermissionForRole
        {
            public const string StartsWith = "CategoryPermissionForRole.";
        }

        public static class Block
        {
            public const string StartsWith = "Block.";
        }

        public static class Favourite
        {
            public const string StartsWith = "Favourite.";
        }

        public static class PollAnswer
        {
            public const string StartsWith = "PollAnswer.";
        }

        public static class Poll
        {
            public const string StartsWith = "Poll.";
        }

        public static class PollVote
        {
            public const string StartsWith = "PollVote.";
        }

        public static class GlobalPermissionForRole
        {
            public const string StartsWith = "GlobalPermissionForRole.";
        }

        public static class Language
        {
            public const string StartsWith = "Language.";
        }

        public static class MembershipUserPoints
        {
            public const string StartsWith = "MembershipUserPoints.";
        }

        public static class Settings
        {
            public const string StartsWith = "Settings.";
            public static string Main = string.Concat(StartsWith, "mainsettings");
        }
    }
}
