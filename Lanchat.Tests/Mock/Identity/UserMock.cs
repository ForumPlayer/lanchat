using System;
using Lanchat.Core.Identity;

namespace Lanchat.Tests.Mock.Identity
{
    public class UserMock : IUser, IInternalUser
    {
        private string nickname;
        private UserStatus userStatus;

        public UserMock()
        {
            ShortId = "9999";
            NicknameWithId = "test";
            PreviousNickname = "test";
        }

        public string NicknameWithId
        {
            get => nickname;
            set
            {
                nickname = value;
                NicknameUpdated?.Invoke(this, value);
            }
        }

        public UserStatus UserStatus
        {
            get => userStatus;
            set
            {
                userStatus = value;
                StatusUpdated?.Invoke(this, value);
            }
        }

        public event EventHandler<string> NicknameUpdated;
        public event EventHandler<UserStatus> StatusUpdated;

        public string PreviousNickname { get; }
        public string ShortId { get; }
    }
}