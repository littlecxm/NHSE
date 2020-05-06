﻿using System;
using System.Collections.Generic;

namespace NHSE.Core
{
    /// <summary>
    /// personal.dat
    /// </summary>
    public sealed class Personal : EncryptedFilePair, IVillagerOrigin
    {
        public readonly PersonalOffsets Offsets;
        public Personal(string folder) : base(folder, "personal") => Offsets = PersonalOffsets.GetOffsets(Info);
        public override string ToString() => PlayerName;

        public uint TownID
        {
            get => BitConverter.ToUInt32(Data, Offsets.PersonalId + 0x00);
            set => BitConverter.GetBytes(value).CopyTo(Data, Offsets.PersonalId + 0x00);
        }

        public string TownName
        {
            get => GetString(Offsets.PersonalId + 0x04, 10);
            set => GetBytes(value, 10).CopyTo(Data, Offsets.PersonalId + 0x04);
        }

        public byte[] GetTownIdentity() => Data.Slice(Offsets.PersonalId + 0x00, 4 + 20);

        public uint PlayerID
        {
            get => BitConverter.ToUInt32(Data, Offsets.PersonalId + 0x1C);
            set => BitConverter.GetBytes(value).CopyTo(Data, Offsets.PersonalId + 0x1C);
        }

        public string PlayerName
        {
            get => GetString(Offsets.PersonalId + 0x20, 10);
            set => GetBytes(value, 10).CopyTo(Data, Offsets.PersonalId + 0x20);
        }

        public byte[] GetPlayerIdentity() => Data.Slice(Offsets.PersonalId + 0x1C, 4 + 20);

        public EncryptedInt32 Wallet
        {
            get => EncryptedInt32.ReadVerify(Data, Offsets.Wallet);
            set => value.Write(Data, Offsets.Wallet);
        }

        public EncryptedInt32 Bank
        {
            get => EncryptedInt32.ReadVerify(Data, Offsets.Bank);
            set => value.Write(Data, Offsets.Bank);
        }

        public EncryptedInt32 NookMiles
        {
            get => EncryptedInt32.ReadVerify(Data, Offsets.NowPoint);
            set => value.Write(Data, Offsets.NowPoint);
        }

        public EncryptedInt32 TotalNookMiles
        {
            get => EncryptedInt32.ReadVerify(Data, Offsets.TotalPoint);
            set => value.Write(Data, Offsets.TotalPoint);
        }

        public IReadOnlyList<Item> Bag // Slots 21-40
        {
            get => Item.GetArray(Data.Slice(Offsets.Pockets1, Offsets.Pockets1Count * Item.SIZE));
            set => Item.SetArray(value).CopyTo(Data, Offsets.Pockets1);
        }

        public uint BagCount // Count of the Bag Slots that are available for use
        {
            get => BitConverter.ToUInt32(Data, Offsets.Pockets1 + (Offsets.Pockets1Count * Item.SIZE));
            set => BitConverter.GetBytes(value).CopyTo(Data, Offsets.Pockets1 + (Offsets.Pockets1Count * Item.SIZE));
        }

        public IReadOnlyList<Item> Pocket // Slots 1-20
        {
            get => Item.GetArray(Data.Slice(Offsets.Pockets2, Offsets.Pockets2Count * Item.SIZE));
            set => Item.SetArray(value).CopyTo(Data, Offsets.Pockets2);
        }

        public uint PocketCount // Count of the Pocket Slots that are available for use
        {
            get => BitConverter.ToUInt32(Data, Offsets.Pockets2 + (Offsets.Pockets2Count * Item.SIZE));
            set => BitConverter.GetBytes(value).CopyTo(Data, Offsets.Pockets2 + (Offsets.Pockets2Count * Item.SIZE));
        }

        public IReadOnlyList<Item> ItemChest
        {
            get => Item.GetArray(Data.Slice(Offsets.ItemChest, Offsets.ItemChestCount * Item.SIZE));
            set => Item.SetArray(value).CopyTo(Data, Offsets.ItemChest);
        }

        public uint ItemChestCount // Count of the Item Chest Slots that are available for use
        {
            get => BitConverter.ToUInt32(Data, Offsets.ItemChest + (Offsets.ItemChestCount * Item.SIZE));
            set => BitConverter.GetBytes(value).CopyTo(Data, Offsets.ItemChest + (Offsets.ItemChestCount * Item.SIZE));
        }

        public uint[] GetCountAchievement()
        {
            var result = new uint[Offsets.MaxAchievementID];
            Buffer.BlockCopy(Data, Offsets.CountAchievement, result, 0, sizeof(uint) * result.Length);
            return result;
        }

        public void SetCountAchievement(uint[] achievements)
        {
            Buffer.BlockCopy(achievements, 0, Data, Offsets.CountAchievement, sizeof(uint) * Offsets.MaxAchievementID);
        }

        public bool[] GetRecipeList() => ArrayUtil.GitBitFlagArray(Data, Offsets.Recipes, Offsets.MaxRecipeID + 1);
        public void SetRecipeList(bool[] value) => ArrayUtil.SetBitFlagArray(Data, Offsets.Recipes, value);

        public short[] GetEventFlagsPlayer()
        {
            var result = new short[0xE00/2];
            Buffer.BlockCopy(Data, Offsets.EventFlagsPlayer, result, 0, result.Length * sizeof(short));
            return result;
        }

        public void SetEventFlagsPlayer(short[] value) => Buffer.BlockCopy(value, 0, Data, Offsets.EventFlagsPlayer, value.Length * sizeof(short));

        public byte[] GetPhotoData()
        {
            var offset = Offsets.Photo;

            // Expect jpeg marker
            if (BitConverter.ToUInt16(Data, offset) != 0xD8FF)
                return Array.Empty<byte>();
            var len = BitConverter.ToInt32(Data, offset - 4);
            return Data.Slice(offset, len);
        }
    }
}