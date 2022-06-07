// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.Lobby
{
	public sealed partial class LobbyModification : Handle
	{
		public LobbyModification()
		{
		}

		public LobbyModification(System.IntPtr innerHandle) : base(innerHandle)
		{
		}

		/// <summary>
		/// The most recent version of the <see cref="AddAttribute" /> API.
		/// </summary>
		public const int LobbymodificationAddattributeApiLatest = 1;

		/// <summary>
		/// The most recent version of the <see cref="AddMemberAttribute" /> API.
		/// </summary>
		public const int LobbymodificationAddmemberattributeApiLatest = 1;

		/// <summary>
		/// Maximum length of the name of the attribute associated with the lobby
		/// </summary>
		public const int LobbymodificationMaxAttributeLength = 64;

		/// <summary>
		/// Maximum number of attributes allowed on the lobby
		/// </summary>
		public const int LobbymodificationMaxAttributes = 64;

		/// <summary>
		/// The most recent version of the <see cref="RemoveAttribute" /> API.
		/// </summary>
		public const int LobbymodificationRemoveattributeApiLatest = 1;

		/// <summary>
		/// The most recent version of the <see cref="RemoveMemberAttribute" /> API.
		/// </summary>
		public const int LobbymodificationRemovememberattributeApiLatest = 1;

		/// <summary>
		/// The most recent version of the <see cref="SetBucketId" /> API.
		/// </summary>
		public const int LobbymodificationSetbucketidApiLatest = 1;

		/// <summary>
		/// The most recent version of the <see cref="SetInvitesAllowed" /> API.
		/// </summary>
		public const int LobbymodificationSetinvitesallowedApiLatest = 1;

		/// <summary>
		/// The most recent version of the <see cref="SetMaxMembers" /> API.
		/// </summary>
		public const int LobbymodificationSetmaxmembersApiLatest = 1;

		/// <summary>
		/// The most recent version of the <see cref="SetPermissionLevel" /> API.
		/// </summary>
		public const int LobbymodificationSetpermissionlevelApiLatest = 1;

		/// <summary>
		/// Associate an attribute with this lobby
		/// An attribute is something may be public or private with the lobby.
		/// If public, it can be queried for in a search, otherwise the data remains known only to lobby members
		/// </summary>
		/// <param name="options">Options to set the attribute and its visibility state</param>
		/// <returns>
		/// <see cref="Result.Success" /> if setting this parameter was successful
		/// <see cref="Result.InvalidParameters" /> if the attribute is missing information or otherwise invalid
		/// <see cref="Result.IncompatibleVersion" /> if the API version passed in is incorrect
		/// </returns>
		public Result AddAttribute(LobbyModificationAddAttributeOptions options)
		{
			var optionsAddress = System.IntPtr.Zero;
			Helper.TryMarshalSet<LobbyModificationAddAttributeOptionsInternal, LobbyModificationAddAttributeOptions>(ref optionsAddress, options);

			var funcResult = Bindings.EOS_LobbyModification_AddAttribute(InnerHandle, optionsAddress);

			Helper.TryMarshalDispose(ref optionsAddress);

			return funcResult;
		}

		/// <summary>
		/// Associate an attribute with a member of the lobby
		/// Lobby member data is always private to the lobby
		/// </summary>
		/// <param name="options">Options to set the attribute and its visibility state</param>
		/// <returns>
		/// <see cref="Result.Success" /> if setting this parameter was successful
		/// <see cref="Result.InvalidParameters" /> if the attribute is missing information or otherwise invalid
		/// <see cref="Result.IncompatibleVersion" /> if the API version passed in is incorrect
		/// </returns>
		public Result AddMemberAttribute(LobbyModificationAddMemberAttributeOptions options)
		{
			var optionsAddress = System.IntPtr.Zero;
			Helper.TryMarshalSet<LobbyModificationAddMemberAttributeOptionsInternal, LobbyModificationAddMemberAttributeOptions>(ref optionsAddress, options);

			var funcResult = Bindings.EOS_LobbyModification_AddMemberAttribute(InnerHandle, optionsAddress);

			Helper.TryMarshalDispose(ref optionsAddress);

			return funcResult;
		}

		public void Release()
		{
			Bindings.EOS_LobbyModification_Release(InnerHandle);
		}

		/// <summary>
		/// Remove an attribute associated with the lobby
		/// </summary>
		/// <param name="options">Specify the key of the attribute to remove</param>
		/// <returns>
		/// <see cref="Result.Success" /> if removing this parameter was successful
		/// <see cref="Result.InvalidParameters" /> if the key is null or empty
		/// <see cref="Result.IncompatibleVersion" /> if the API version passed in is incorrect
		/// </returns>
		public Result RemoveAttribute(LobbyModificationRemoveAttributeOptions options)
		{
			var optionsAddress = System.IntPtr.Zero;
			Helper.TryMarshalSet<LobbyModificationRemoveAttributeOptionsInternal, LobbyModificationRemoveAttributeOptions>(ref optionsAddress, options);

			var funcResult = Bindings.EOS_LobbyModification_RemoveAttribute(InnerHandle, optionsAddress);

			Helper.TryMarshalDispose(ref optionsAddress);

			return funcResult;
		}

		/// <summary>
		/// Remove an attribute associated with of member of the lobby
		/// </summary>
		/// <param name="options">Specify the key of the member attribute to remove</param>
		/// <returns>
		/// <see cref="Result.Success" /> if removing this parameter was successful
		/// <see cref="Result.InvalidParameters" /> if the key is null or empty
		/// <see cref="Result.IncompatibleVersion" /> if the API version passed in is incorrect
		/// </returns>
		public Result RemoveMemberAttribute(LobbyModificationRemoveMemberAttributeOptions options)
		{
			var optionsAddress = System.IntPtr.Zero;
			Helper.TryMarshalSet<LobbyModificationRemoveMemberAttributeOptionsInternal, LobbyModificationRemoveMemberAttributeOptions>(ref optionsAddress, options);

			var funcResult = Bindings.EOS_LobbyModification_RemoveMemberAttribute(InnerHandle, optionsAddress);

			Helper.TryMarshalDispose(ref optionsAddress);

			return funcResult;
		}

		/// <summary>
		/// Set the bucket ID associated with this lobby.
		/// Values such as region, game mode, etc can be combined here depending on game need.
		/// Setting this is strongly recommended to improve search performance.
		/// </summary>
		/// <param name="options">Options associated with the bucket ID of the lobby</param>
		/// <returns>
		/// <see cref="Result.Success" /> if setting this parameter was successful
		/// <see cref="Result.InvalidParameters" /> if the bucket ID is invalid or null
		/// <see cref="Result.IncompatibleVersion" /> if the API version passed in is incorrect
		/// </returns>
		public Result SetBucketId(LobbyModificationSetBucketIdOptions options)
		{
			var optionsAddress = System.IntPtr.Zero;
			Helper.TryMarshalSet<LobbyModificationSetBucketIdOptionsInternal, LobbyModificationSetBucketIdOptions>(ref optionsAddress, options);

			var funcResult = Bindings.EOS_LobbyModification_SetBucketId(InnerHandle, optionsAddress);

			Helper.TryMarshalDispose(ref optionsAddress);

			return funcResult;
		}

		/// <summary>
		/// Allows enabling or disabling invites for this lobby.
		/// The lobby will also need to have `bPresenceEnabled` true.
		/// </summary>
		/// <param name="options">Options associated with invites allowed flag for this lobby.</param>
		/// <returns>
		/// <see cref="Result.Success" /> if setting this parameter was successful
		/// <see cref="Result.IncompatibleVersion" /> if the API version passed in is incorrect
		/// </returns>
		public Result SetInvitesAllowed(LobbyModificationSetInvitesAllowedOptions options)
		{
			var optionsAddress = System.IntPtr.Zero;
			Helper.TryMarshalSet<LobbyModificationSetInvitesAllowedOptionsInternal, LobbyModificationSetInvitesAllowedOptions>(ref optionsAddress, options);

			var funcResult = Bindings.EOS_LobbyModification_SetInvitesAllowed(InnerHandle, optionsAddress);

			Helper.TryMarshalDispose(ref optionsAddress);

			return funcResult;
		}

		/// <summary>
		/// Set the maximum number of members allowed in this lobby.
		/// When updating the lobby, it is not possible to reduce this number below the current number of existing members
		/// </summary>
		/// <param name="options">Options associated with max number of members in this lobby</param>
		/// <returns>
		/// <see cref="Result.Success" /> if setting this parameter was successful
		/// <see cref="Result.IncompatibleVersion" /> if the API version passed in is incorrect
		/// </returns>
		public Result SetMaxMembers(LobbyModificationSetMaxMembersOptions options)
		{
			var optionsAddress = System.IntPtr.Zero;
			Helper.TryMarshalSet<LobbyModificationSetMaxMembersOptionsInternal, LobbyModificationSetMaxMembersOptions>(ref optionsAddress, options);

			var funcResult = Bindings.EOS_LobbyModification_SetMaxMembers(InnerHandle, optionsAddress);

			Helper.TryMarshalDispose(ref optionsAddress);

			return funcResult;
		}

		/// <summary>
		/// Set the permissions associated with this lobby.
		/// The permissions range from "public" to "invite only" and are described by <see cref="LobbyPermissionLevel" />
		/// </summary>
		/// <param name="options">Options associated with the permission level of the lobby</param>
		/// <returns>
		/// <see cref="Result.Success" /> if setting this parameter was successful
		/// <see cref="Result.IncompatibleVersion" /> if the API version passed in is incorrect
		/// </returns>
		public Result SetPermissionLevel(LobbyModificationSetPermissionLevelOptions options)
		{
			var optionsAddress = System.IntPtr.Zero;
			Helper.TryMarshalSet<LobbyModificationSetPermissionLevelOptionsInternal, LobbyModificationSetPermissionLevelOptions>(ref optionsAddress, options);

			var funcResult = Bindings.EOS_LobbyModification_SetPermissionLevel(InnerHandle, optionsAddress);

			Helper.TryMarshalDispose(ref optionsAddress);

			return funcResult;
		}
	}
}