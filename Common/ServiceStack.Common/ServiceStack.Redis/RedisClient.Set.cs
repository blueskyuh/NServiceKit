//
// ServiceStack.Redis: ECMA CLI Binding to the Redis key-value storage system
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2010 Liquidbit Ltd.
//
// Licensed under the same terms of reddis and ServiceStack: new BSD license.
//

using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceStack.Redis
{
	/// <summary>
	/// This class contains all the set operations for the RedisClient.
	/// </summary>
	public partial class RedisClient
	{
		public RedisClientSets Sets { get; set; }

		public List<string> GetRangeFromSortedSet(string setId, int startingFrom, int endingAt)
		{
			var multiDataList = Sort(setId, startingFrom, endingAt, true, false);
			return CreateList(multiDataList);
		}

		internal byte[][] SMembers(string setId)
		{
			if (!SendDataCommand(null, "SMEMBERS {0}\r\n", setId))
				throw new Exception("Unable to connect");
			return ReadMultiData();
		}

		public List<string> GetAllFromSet(string setId)
		{
			var multiDataList = SMembers(setId);
			return CreateList(multiDataList);
		}

		internal void SAdd(string setId, byte[] value)
		{
			if (setId == null)
				throw new ArgumentNullException("setId");
			if (value == null)
				throw new ArgumentNullException("value");

			if (!SendDataCommand(value, "SADD {0} {1}\r\n", setId, value.Length))
				throw new Exception("Unable to connect");
			ExpectSuccess();
		}

		public void AddToSet(string setId, string value)
		{
			SAdd(setId, Encoding.UTF8.GetBytes(value));
		}

		internal void SRem(string setId, byte[] value)
		{
			if (setId == null)
				throw new ArgumentNullException("setId");
			if (!SendDataCommand(value, "SREM {0} {1}\r\n", setId, value.Length))
				throw new Exception("Unable to connect");
			
			ExpectSuccess();
		}

		public void RemoveFromSet(string setId, string value)
		{
			SRem(setId, Encoding.UTF8.GetBytes(value));
		}

		internal byte[] SPop(string setId)
		{
			if (!SendDataCommand(null, "SPOP {0}\r\n", setId))
				throw new Exception("Unable to connect");
			return ReadData();
		}

		public string PopFromSet(string setId)
		{
			return Encoding.UTF8.GetString(SPop(setId));
		}

		internal void SMove(string fromSetId, string toSetId, byte[] value)
		{
			if (fromSetId == null)
				throw new ArgumentNullException("fromSetId");
			if (toSetId == null)
				throw new ArgumentNullException("toSetId");

			if (!SendDataCommand(value, "SMOVE {0} {1} {2}\r\n", fromSetId, toSetId, value.Length))
				throw new Exception("Unable to connect");

			ExpectSuccess();
		}

		public void MoveBetweenSets(string fromSetId, string toSetId, string value)
		{
			SMove(fromSetId, toSetId, Encoding.UTF8.GetBytes(value));
		}

		internal int SCard(string setId)
		{
			if (setId == null)
				throw new ArgumentNullException("setId");

			return SendExpectInt("SCARD {0}\r\n", setId);
		}

		public int GetSetCount(string setId)
		{
			return SCard(setId);
		}

		internal int SIsMember(string setId, byte[] value)
		{
			if (setId == null)
				throw new ArgumentNullException("setId");
			if (!SendDataCommand(value, "SISMEMBER {0} {1}\r\n", setId, value.Length))
				throw new Exception("Unable to connect");

			return ReadInt();
		}

		public bool SetContainsValue(string setId, string value)
		{
			return SIsMember(setId, Encoding.UTF8.GetBytes(value)) == 1;
		}

		private static List<string> CreateList(byte[][] multiDataList)
		{
			var results = new List<string>();
			foreach (var multiData in multiDataList)
			{
				results.Add(Encoding.UTF8.GetString(multiData));
			}
			return results;
		}

		internal byte[][] SInter(params string[] setIds)
		{
			if (!SendDataCommand(null, "SINTER {0}\r\n", string.Join(" ", setIds)))
				throw new Exception("Unable to connect");

			return ReadMultiData();
		}

		public List<string> GetIntersectFromSets(params string[] setIds)
		{
			var multiDataList = SInter(setIds);
			return CreateList(multiDataList);
		}

		internal void SInterStore(string intoSetId, params string[] setIds)
		{
			if (!SendDataCommand(null, "SINTERSTORE {0} {1}\r\n", intoSetId, string.Join(" ", setIds)))
				throw new Exception("Unable to connect");

			ExpectSuccess();
		}

		public void StoreIntersectFromSets(string intoSetId, params string[] setIds)
		{
			SInterStore(intoSetId, setIds);
		}

		internal byte[][] SUnion(params string[] setIds)
		{
			if (!SendDataCommand(null, "SUNION {0}\r\n", string.Join(" ", setIds)))
				throw new Exception("Unable to connect");

			return ReadMultiData();
		}

		public List<string> GetUnionFromSets(params string[] setIds)
		{
			var multiDataList = SUnion(setIds);
			return CreateList(multiDataList);
		}

		internal void SUnionStore(string intoSetId, params string[] setIds)
		{
			if (!SendDataCommand(null, "SUNIONSTORE {0} {1}\r\n", intoSetId, string.Join(" ", setIds)))
				throw new Exception("Unable to connect");

			ExpectSuccess();
		}

		public void StoreUnionFromSets(string intoSetId, params string[] setIds)
		{
			SUnionStore(intoSetId, setIds);
		}

		internal byte[][] SDiff(string fromSetId, params string[] withSetIds)
		{
			if (!SendDataCommand(null, "SDIFF {0} {1}\r\n", fromSetId, string.Join(" ", withSetIds)))
				throw new Exception("Unable to connect");

			return ReadMultiData();
		}

		public List<string> GetDifferencesFromSet(string fromSetId, params string[] withSetIds)
		{
			var multiDataList = SDiff(fromSetId, withSetIds);
			return CreateList(multiDataList);
		}

		internal void SDiffStore(string intoSetId, string fromSetId, params string[] withSetIds)
		{
			if (!SendDataCommand(null, "SDIFFSTORE {0} {1} {2}\r\n", intoSetId, fromSetId, string.Join(" ", withSetIds)))
				throw new Exception("Unable to connect");

			ExpectSuccess();
		}

		public void StoreDifferencesFromSet(string intoSetId, string fromSetId, params string[] withSetIds)
		{
			SDiffStore(intoSetId, fromSetId, withSetIds);
		}

		internal byte[] SRandMember(string setId)
		{
			if (!SendDataCommand(null, "SRANDMEMBER {0}\r\n", setId))
				throw new Exception("Unable to connect");
			return ReadData();
		}

		public string GetRandomEntryFromSet(string setId)
		{
			return Encoding.UTF8.GetString(SRandMember(setId));
		}

	}
}