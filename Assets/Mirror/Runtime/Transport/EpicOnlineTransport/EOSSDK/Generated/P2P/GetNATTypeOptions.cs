// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.P2P
{
	/// <summary>
	/// Structure containing information needed to get perviously queried NAT-types
	/// </summary>
	public class GetNATTypeOptions
	{
	}

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
	internal struct GetNATTypeOptionsInternal : ISettable, System.IDisposable
	{
		private int m_ApiVersion;

		public void Set(GetNATTypeOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = P2PInterface.GetnattypeApiLatest;
			}
		}

		public void Set(object other)
		{
			Set(other as GetNATTypeOptions);
		}

		public void Dispose()
		{
		}
	}
}